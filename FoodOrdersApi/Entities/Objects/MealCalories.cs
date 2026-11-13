namespace FoodOrdersApi.Entities.Objects
{
    public class MealCalories
    {
        public int Id { get; set; }

        public int MealId { get; set; }
        public virtual Meal Meal { get; set; }

        public double Calories { get; set; }

        /// "Estimated" — calculated by AiCalibrationService.
        public string Source { get; set; } = "Estimated";
        public string Explanation { get; set; } = "";
    }
}
