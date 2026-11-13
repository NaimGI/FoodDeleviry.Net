namespace FoodOrdersApi.Entities.Objects
{
    public class DeliveryPricing
    {
        public int Id { get; set; }

        /// 0 = default (free), 1 = weather multiplier, 2 = priority express fee.
        public int Category { get; set; }
        public double Value { get; set; }

        public int? RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }
    }
}
