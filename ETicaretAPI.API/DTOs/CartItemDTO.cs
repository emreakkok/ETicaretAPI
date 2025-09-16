using ETicaretAPI.API.Entities;

namespace ETicaretAPI.API.DTOs
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }

    }
}
