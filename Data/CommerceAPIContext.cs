using CommerceAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CommerceAPI.Data
{
    public class CommerceAPIContext : IdentityDbContext<User>
    {
        public CommerceAPIContext(DbContextOptions<CommerceAPIContext> options) : base(options) { }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
    }
}
