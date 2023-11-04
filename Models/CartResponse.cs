namespace CommerceAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public string? Subtotal { get; set; }
    }

    public class CartResponse
    {
        public string Total { get; set; } = "0.00";

        public List<CartItem>? CartItems { get; set; }
    }
}