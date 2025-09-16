namespace ETicaretAPI.API.Entities
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        
    }
}
