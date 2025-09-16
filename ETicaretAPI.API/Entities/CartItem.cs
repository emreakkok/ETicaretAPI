namespace ETicaretAPI.API.Entities
{
    public class CartItem : BaseEntity
    {

        public int ProductId { get; set; }

        public int CartId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; } = null!;

        // public Cart Cart { get; set; } = null!;
    }
}
