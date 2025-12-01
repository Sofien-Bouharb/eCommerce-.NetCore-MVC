namespace eCommerce.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }

        public string ShippingAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }

}
