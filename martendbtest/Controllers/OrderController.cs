using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Marten.Linq.MatchesSql;
using Microsoft.AspNetCore.Mvc;

namespace martendbtest.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IDocumentStore _documentStore;

        public OrderController(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Orders(
            Guid userId)
        {
            await using var lightweightSession = _documentStore.LightweightSession();

            var result =  await lightweightSession.Query<Order>().Where(o => o.UserId.Equals(userId)).ToListAsync();

            return result;
        }

        [HttpGet("byItemId")]
        public async Task<IEnumerable<Order>> Orders(
            int itemId)
        {
            await using var lightweightSession = _documentStore.LightweightSession();
            return await lightweightSession.Query<Order>().Where(x => x.Items.Any(i => i.Id == itemId)).ToListAsync();
        }

        [HttpGet("byItemIdv2")]
        public async Task<IEnumerable<Order>> Ordersv2(
            int itemId)
        {
            await using var lightweightSession = _documentStore.LightweightSession();
            return await lightweightSession.Query<Order>().Where(x => x.MatchesSql($"data @@ '$.Items[*].Id == {itemId}'"))
                .ToListAsync();
        }
    }
}