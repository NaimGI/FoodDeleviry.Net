using FoodOrdersApi.Entities.Enum.Notice;

namespace FoodOrdersApi.Entities.Objects
{
    public class AppNotification
    {
        public int Id { get; set; }

        /// Delivery, order, restaurant, report, meal. See NotificationType.
        public NotificationType Type { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";

        public int DstAccountId { get; set; }
        public virtual Account DstAccount { get; set; }

        public int SenderId { get; set; }
        public virtual Account Sender { get; set; }

        public DateTime Time { get; set; }
        public bool Read { get; set; }

        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? MealId { get; set; }
        public virtual Meal Meal { get; set; }

        public int? RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }
    }
}
