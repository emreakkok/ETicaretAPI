using ETicaretAPI.API.Entities;
using ETicaretAPI.API.Services.Interfaces;

namespace ETicaretAPI.API.Services.Implementations
{
    public class CartService : ICartService
    {
        public void AddItem(Cart cart, Product product, int quantity)
        {
            var item = cart.CartItems.FirstOrDefault(c => c.ProductId == product.Id);
            if (item == null)
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void DeleteItem(Cart cart, int productId , int quantity)
        {
            var item = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);
            
            if(item == null)
                return;

            item.Quantity -= quantity;

            if (item.Quantity == 0)
            {
                cart.CartItems.Remove(item);
            }
        }
    }
}
