using ETicaretAPI.API.Data;
using ETicaretAPI.API.DTOs;
using ETicaretAPI.API.Entities;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        public OrderController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Kullanıcının kendi siparişleri
        [HttpGet("GetOrders")]
        [Authorize]
        public async Task<ActionResult<List<OrderDto>>> GetOrders()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Phone = o.Phone,
                City = o.City,
                AddressLine = o.AddressLine,
                UserId = o.UserId,
                OrderStatus = o.OrderStatus,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        [HttpGet("GetOrder/{id}")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            var orderDto = new OrderDto
            {
                Id = order.Id,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Phone = order.Phone,
                City = order.City,
                AddressLine = order.AddressLine,
                UserId = order.UserId,
                OrderStatus = order.OrderStatus,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPost("CreateOrder")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto orderDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                    return BadRequest(new { message = "Kullanıcı bulunamadı." });

                // Model validation
                if (string.IsNullOrEmpty(orderDto.FirstName) || string.IsNullOrEmpty(orderDto.LastName) ||
                    string.IsNullOrEmpty(orderDto.Phone) || string.IsNullOrEmpty(orderDto.City) ||
                    string.IsNullOrEmpty(orderDto.AddressLine) || string.IsNullOrEmpty(orderDto.CardNumber) ||
                    string.IsNullOrEmpty(orderDto.ExpiryDate) || string.IsNullOrEmpty(orderDto.Cvc) ||
                    string.IsNullOrEmpty(orderDto.CardHolderName))
                {
                    return BadRequest(new { message = "Tüm alanları doldurunuz." });
                }

                // Kullanıcının sepetini getir
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                    return BadRequest(new { message = "Sepetiniz boş." });

                // Stok kontrolü
                foreach (var ci in cart.CartItems)
                {
                    if (ci.Product.Stock < ci.Quantity)
                        return BadRequest(new { message = $"'{ci.Product.Name}' ürünü stokta yeterli değil." });
                }

                // Ödeme işlemini gerçekleştir
                var paymentResult = await ProcessPayment(orderDto, cart);

                if (paymentResult == null || paymentResult.Status != "success")
                {
                    return BadRequest(new { message = "Ödeme işlemi başarısız oldu. Lütfen kart bilgilerinizi kontrol edin." });
                }

                // Sipariş oluştur
                var order = new Order
                {
                    FirstName = orderDto.FirstName,
                    LastName = orderDto.LastName,
                    Phone = orderDto.Phone,
                    City = orderDto.City,
                    AddressLine = orderDto.AddressLine,
                    UserId = userId,
                    OrderStatus = OrderStatus.Completed, // Ödeme başarılı olduğu için Completed
                    OrderItems = cart.CartItems.Select(ci => new Entities.OrderItem
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        ProductImage = ci.Product.ImageUrl ?? "",
                        Quantity = ci.Quantity,
                        Price = ci.Product.Price
                    }).ToList()
                };

                _context.Orders.Add(order);

                // Stokları güncelle
                foreach (var ci in cart.CartItems)
                {
                    ci.Product.Stock -= ci.Quantity;
                    _context.Products.Update(ci.Product);
                }

                // Sepeti temizle
                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();

                // DTO olarak dön
                var orderDtoResult = new OrderDto
                {
                    Id = order.Id,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    Phone = order.Phone,
                    City = order.City,
                    AddressLine = order.AddressLine,
                    UserId = order.UserId,
                    OrderStatus = order.OrderStatus,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                };

                return Ok(orderDtoResult);
            }
            catch { return StatusCode(500, new { message = "Sunucu hatası. Lütfen daha sonra tekrar deneyin." }); }
        }

        private async Task<Payment> ProcessPayment(CreateOrderDto model, Cart cart)
        {
            try
            {
                Options options = new Options();
                options.ApiKey = _config["PaymentApi:APIKey"];
                options.SecretKey = _config["PaymentApi:APISecret"];
                options.BaseUrl = "https://sandbox-api.iyzipay.com";

                // Toplam fiyatı hesapla
                decimal totalPrice = cart.CartItems.Sum(x => x.Product.Price * x.Quantity);

                CreatePaymentRequest request = new CreatePaymentRequest();
                request.Locale = Locale.TR.ToString();
                request.ConversationId = Guid.NewGuid().ToString();
                request.Price = totalPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                request.PaidPrice = totalPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                request.Currency = Currency.TRY.ToString();
                request.Installment = 1;
                request.BasketId = cart.Id.ToString();
                request.PaymentChannel = PaymentChannel.WEB.ToString();
                request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

                // Kart bilgileri (formdan gelen)
                PaymentCard paymentCard = new PaymentCard();
                paymentCard.CardHolderName = model.CardHolderName;
                paymentCard.CardNumber = model.CardNumber.Replace(" ", "").Replace("-", "");

                // Expiry date işleme (MM/YY formatı)
                var expiryParts = model.ExpiryDate.Split('/');
                if (expiryParts.Length == 2)
                {
                    paymentCard.ExpireMonth = expiryParts[0];
                    paymentCard.ExpireYear = "20" + expiryParts[1]; // YY -> 20YY
                }

                paymentCard.Cvc = model.Cvc;
                paymentCard.RegisterCard = 0;
                request.PaymentCard = paymentCard;

                // Alıcı bilgileri (formdan gelen)
                Buyer buyer = new Buyer();
                buyer.Id = cart.UserId;
                buyer.Name = model.FirstName;
                buyer.Surname = model.LastName;
                buyer.GsmNumber = model.Phone;
                buyer.Email = "test@test.com"; // Gerçek uygulamada user'dan al
                buyer.IdentityNumber = "11111111111"; // Test için
                buyer.LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                buyer.RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                buyer.RegistrationAddress = model.AddressLine;
                buyer.Ip = "127.0.0.1"; // Gerçek uygulamada client IP'si al
                buyer.City = model.City;
                buyer.Country = "Turkey";
                buyer.ZipCode = "34000";
                request.Buyer = buyer;

                // Teslimat adresi
                Address shippingAddress = new Address();
                shippingAddress.ContactName = $"{model.FirstName} {model.LastName}";
                shippingAddress.City = model.City;
                shippingAddress.Country = "Turkey";
                shippingAddress.Description = model.AddressLine;
                shippingAddress.ZipCode = "34000";
                request.ShippingAddress = shippingAddress;

                // Fatura adresi
                Address billingAddress = new Address();
                billingAddress.ContactName = $"{model.FirstName} {model.LastName}";
                billingAddress.City = model.City;
                billingAddress.Country = "Turkey";
                billingAddress.Description = model.AddressLine;
                billingAddress.ZipCode = "34000";
                request.BillingAddress = billingAddress;

                // Sepet öğeleri
                List<BasketItem> basketItems = new List<BasketItem>();
                foreach (var cartItem in cart.CartItems)
                {
                    BasketItem basketItem = new BasketItem();
                    basketItem.Id = cartItem.ProductId.ToString();
                    basketItem.Name = cartItem.Product.Name;
                    basketItem.Category1 = "Product"; // Gerçek uygulamada kategori bilgisi
                    basketItem.Category2 = "General";
                    basketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                    basketItem.Price = (cartItem.Product.Price * cartItem.Quantity).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    basketItems.Add(basketItem);
                }
                request.BasketItems = basketItems;

                return await Payment.Create(request, options);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Payment Error: {ex.Message}");
                return null;
            }
        }
    }
}