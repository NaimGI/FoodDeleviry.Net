using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrdersApi.Entities.Objects
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Total price is required")]
        public double TotalPrice { get; set; }
        [Required(ErrorMessage = "Positions value is required")]
        public int Positions { get; set; }

        public string? Notes { get; set; }



        [Required(ErrorMessage = "Individual order has to be connected to cart")]
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; }



        [Required(ErrorMessage = "Account is required")]
        [ForeignKey("AccountId")]
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }


        public virtual ICollection<MealOrder> MealOrder { get; set; }
    }
}
