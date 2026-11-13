namespace FoodOrdersApi.Models.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "Home";
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }
        public string? Premises { get; set; }
        public bool IsDefault { get; set; }
    }
}
