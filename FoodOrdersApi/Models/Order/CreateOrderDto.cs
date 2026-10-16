namespace FoodOrdersApi.Models.Order
{
    public class CreateOrderDto
    {
        public string? Notes { get; set; }


        public int CartId { get; set; }
        public int AccountId { get; set; }
        public double TotalPrice { get; set; }
    }
}
