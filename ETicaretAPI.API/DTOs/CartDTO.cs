

namespace ETicaretAPI.API.DTOs
{
    public class CartDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; } = null!;

        public List<CartItemDTO> CartItems { get; set; } = new();
    }
}
