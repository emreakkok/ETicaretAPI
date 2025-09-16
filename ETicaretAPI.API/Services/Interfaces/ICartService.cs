using ETicaretAPI.API.Entities;

namespace ETicaretAPI.API.Services.Interfaces
{
    public interface ICartService
    {
        void AddItem(Cart cart, Product product, int quantity);
        void DeleteItem(Cart cart, int productId, int quantity);
    }
}
