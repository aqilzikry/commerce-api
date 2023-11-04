using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommerceAPI.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Quantity { get; set; }

        public Product? Product { get; set; }
    }
}