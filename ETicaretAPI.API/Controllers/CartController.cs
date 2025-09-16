using ETicaretAPI.API.Data;
using ETicaretAPI.API.DTOs;
using ETicaretAPI.API.Entities;
using ETicaretAPI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETicaretAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;

        public CartController(AppDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            try
            {
                var cart = await GetOrCreate();
                return Ok(CartToDTO(cart));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { title = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CartDTO>> AddItemToCart(int productId, int quantity)
        {
            try
            {
                var cart = await GetOrCreate();
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                    return NotFound(new { title = "Ürün bulunamadı." });

                _cartService.AddItem(cart, product, quantity);
                await _context.SaveChangesAsync();

                return Ok(CartToDTO(cart));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { title = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteItemFromCart(int productId, int quantity)
        {
            try
            {
                var cart = await GetOrCreate();
                _cartService.DeleteItem(cart, productId, quantity);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Ürün sepetten kaldırıldı." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { title = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }

        private async Task<Cart> GetOrCreate()
        {
            // JWT token'dan kullanıcı bilgisini al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("JWT token'dan kullanıcı bilgisi alınamadı.");


            //// Debug için - hangi claim'lerin geldiğini kontrol et
            //if (string.IsNullOrEmpty(userId))
            //{
            //    var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            //    Console.WriteLine($"Available claims: {string.Join(", ", allClaims)}");
            //    throw new UnauthorizedAccessException("JWT token'dan kullanıcı bilgisi alınamadı.");
            //}

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private CartDTO CartToDTO(Cart cart)
        {
            return new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(i => new CartItemDTO
                {
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    ImageUrl = i.Product.ImageUrl,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}