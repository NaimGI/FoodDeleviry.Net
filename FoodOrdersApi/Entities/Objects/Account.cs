using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FoodOrdersApi.Entities.Objects
{
    public class Account
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string HashedPassword { get; set; }

        // Navigation property: one account can have many carts
        public virtual ICollection<Cart> Carts { get; set; }

        // Navigation property: one account can have many orders
        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
