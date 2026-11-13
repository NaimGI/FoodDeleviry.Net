using System.ComponentModel.DataAnnotations;

namespace FoodOrdersApi.Models.Account
{
    public class UpdateProfileDto
    {
        [Required]
        [MinLength(6)]
        public string Login { get; set; }
    }
}
