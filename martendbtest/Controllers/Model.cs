using System;
using System.Collections.Generic;

namespace martendbtest.Controllers
{
    public record User(Guid Id, string FirstName, string LastName, string UserName, Email Email);
    public record Email(string EmailAddress);
    public record Order(Guid Id, decimal Total, decimal Subtotal, decimal Shipping, List<Item> Items, Guid UserId);
    public record Item(int Id, string Name, int Quantity, decimal Price);
}