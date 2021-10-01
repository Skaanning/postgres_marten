using Marten;
using martendbtest.Controllers;
using Weasel.Postgresql.Tables;

namespace martendbtest
{
    public class DbIndexSetup : MartenRegistry
    {
        public DbIndexSetup()
        {
            For<User>().Duplicate(x => x.UserName, configure: docIndex =>
            {
                docIndex.Name = "idx_userName";
                docIndex.Method = IndexMethod.hash;
            });

            For<User>().Index(x => x.Email);

            For<Order>().GinIndexJsonData();

            For<Order>().Index(x => x.UserId, configure: docIndex =>
            {
                docIndex.Name = "idx_userId";
                docIndex.Method = IndexMethod.hash;
            });
        }
    }
}