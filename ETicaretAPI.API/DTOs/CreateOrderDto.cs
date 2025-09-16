namespace ETicaretAPI.API.DTOs
{
    public class CreateOrderDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }

        public string? AddressLine { get; set; }



        // Ödeme bilgileri
        public string CardNumber { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public string Cvc { get; set; } = null!;
        public string CardHolderName { get; set; } = null!;
    }
}
