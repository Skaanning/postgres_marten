using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace martendbtest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDocumentStore _documentStore;

        public UserController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers(int page = 1, int pageSize = 10)
        {
            await using var session = _documentStore.LightweightSession();
            var users = await session.QueryAsync<User>(
                sql: "select data from mt_doc_user order by id offset :offset limit :limit",
                parameters: new { offset = (page - 1) * pageSize, limit = pageSize });

            return users;
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            await using var session = _documentStore.LightweightSession();

            var users = await session.Query<User>()
                .Where(x=>x.Email.EmailAddress.Contains(email))
                .FirstOrDefaultAsync();

            return Ok(users);
        }

        [HttpGet("byusername/{userName}")]
        public async Task<IActionResult> GetByUserName(string userName)
        {
            await using var session = _documentStore.LightweightSession();

            var users = await session.Query<User>()
                .Where(x=> x.UserName.Equals(userName))
                .FirstOrDefaultAsync();

            return Ok(users);
        }
    }
}