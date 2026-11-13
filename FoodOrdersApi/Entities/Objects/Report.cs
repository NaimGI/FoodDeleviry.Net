using FoodOrdersApi.Entities.Enum.Report;

namespace FoodOrdersApi.Entities.Objects
{
    public class Report
    {
        public int Id { get; set; }

        public ReportType Type { get; set; }
        public string Description { get; set; } = "";
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? MealId { get; set; }
        public virtual Meal Meal { get; set; }

        public int? RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }

        /// The account that submitted the report (JWT user).
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
