using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.Extensions;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace martendbtest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeedDbController : ControllerBase
    {
        private readonly IDocumentStore _documentStore;

        public SeedDbController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await using var session = _documentStore.LightweightSession();
            session.DeleteWhere<User>(x => true);
            session.DeleteWhere<Order>(x => true);
            await session.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate()
        {
            var userIds = await AddUsers();
            await AddOrders(userIds);

            return Ok();
        }

        private async Task AddOrders(Guid[] userIds)
        {
            var itemFaker = new Faker<Item>()
                .CustomInstantiator(f => new Item(f.Random.Int(0), f.Commerce.Product(), f.Random.Int(0, 12), f.Finance.Amount(0m, 1000m, 2)));

            foreach (var userId in userIds)
            {
                var orderFaker = new Faker<Order>()
                    .CustomInstantiator(f =>
                    {
                        var items = itemFaker.Generate(f.Random.Number(1, 5));
                        var shipping = f.Finance.Amount();
                        var subtotal = items.Sum(x => x.Price);
                        return new Order(f.Random.Guid(), shipping + subtotal, subtotal, shipping, items, userId);
                    });

                var orders = orderFaker.GenerateBetween(1, 10);
                await _documentStore.BulkInsertAsync(orders);
            }
        }

        private async Task<Guid[]> AddUsers()
        {
            var emailFaker = new Faker<Email>()
                .CustomInstantiator(x => new Email(x.Person.Email));

            var userFaker = new Faker<User>()
                .CustomInstantiator(f => new User(new Guid(), f.Person.FirstName, f.Person.LastName, f.Internet.UserName(),
                    emailFaker.Generate()));

            var generatedUsers = userFaker.Generate(100_000);
            await _documentStore.BulkInsertAsync(generatedUsers, batchSize: 500);
            return generatedUsers.Select(x => x.Id).ToArray();
        }
    }
}