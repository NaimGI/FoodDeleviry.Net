namespace FoodOrdersApi.Models.Account
{
    public class AccountProfileDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public double TotalSpent { get; set; }
    }
}
