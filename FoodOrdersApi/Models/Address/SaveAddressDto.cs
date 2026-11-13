using System.ComponentModel.DataAnnotations;

namespace FoodOrdersApi.Models.Address
{
    public class SaveAddressDto
    {
        [Required]
        [StringLength(40)]
        public string Label { get; set; } = "Home";

        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string Building { get; set; }

        public string? Premises { get; set; }

        public bool IsDefault { get; set; }
    }
}
