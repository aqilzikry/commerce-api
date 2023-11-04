using System.ComponentModel.DataAnnotations;

namespace CommerceAPI.Models
{
    public class CartRequest
    {
        [Required]
        public int? ProductId {  get; set; }
        public int? CartId { get; set; }
        public int? Quantity { get; set; }
    }
}
