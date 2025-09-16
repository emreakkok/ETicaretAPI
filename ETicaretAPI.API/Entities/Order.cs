namespace ETicaretAPI.API.Entities
{
    public class Order : BaseEntity
    {
        public string ?FirstName { get; set; }
        public string ?LastName { get; set; }
        public string ?Phone{ get; set; }
        public string ?City { get; set; }

        public string? AddressLine { get; set; }

        public string? UserId { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public decimal TotalPrice
        { 
            get 
            {
                return OrderItems.Sum(item => item.Price * item.Quantity);
            }
        }
    }

    public class OrderItem : BaseEntity
    {

        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public string ProductImage { get; set; }
        public string ?ProductName { get; set; }
        public Product ?Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

    }

    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
