using FoodOrdersApi.Entities;
using FoodOrdersApi.Entities.Objects;
using Microsoft.AspNetCore.Identity;

namespace FoodOrdersApi.Services
{
    public static class DevelopmentDataSeeder
    {
        private const string DemoLogin = "demo.user";
        private const string DemoPassword = "Demo1234!";
        private const string AdminLogin = "administrator";
        private const string AdminPassword = "Admin1234!";
        private const string DemoOrganizationName = "Demo Organization";

        public static void Seed(AppDbContext context, IPasswordHasher<Account> passwordHasher)
        {
            if (!IsDatabaseEmpty(context))
            {
                return;
            }

            var organization = context.Organizations
                .FirstOrDefault(item => item.Name == DemoOrganizationName);
            if (organization is null)
            {
                organization = new Organization { Name = DemoOrganizationName };
                context.Organizations.Add(organization);
                context.SaveChanges();
            }

            if (!context.Users.Any(user =>
                    user.OrganizationId == organization.Id &&
                    user.FirstName == "Demo" &&
                    user.LastName == "User"))
            {
                context.Users.Add(new User
                {
                    FirstName = "Demo",
                    LastName = "User",
                    OrganizationId = organization.Id
                });
            }

            var restaurantData = new[]
            {
                (Name: "Bella Pasta", Category: "Italian"),
                (Name: "Green Bowl", Category: "Healthy"),
                (Name: "Burger Corner", Category: "Burgers")
            };

            foreach (var item in restaurantData)
            {
                if (!context.Restaurants.Any(restaurant => restaurant.Name == item.Name))
                {
                    context.Restaurants.Add(new Restaurant
                    {
                        Name = item.Name,
                        Category = item.Category
                    });
                }
            }

            context.SaveChanges();

            var mealData = new[]
            {
                (Restaurant: "Bella Pasta", Name: "Classic Spaghetti", Description: "Spaghetti with tomato and basil sauce.", Price: 12.50),
                (Restaurant: "Bella Pasta", Name: "Creamy Mushroom Penne", Description: "Penne pasta with mushrooms in a creamy sauce.", Price: 14.00),
                (Restaurant: "Bella Pasta", Name: "Margherita Pizza", Description: "Pizza with tomato, mozzarella, and fresh basil.", Price: 13.50),
                (Restaurant: "Green Bowl", Name: "Garden Salad Bowl", Description: "Mixed greens, seasonal vegetables, and house dressing.", Price: 10.00),
                (Restaurant: "Green Bowl", Name: "Roasted Veggie Bowl", Description: "Roasted seasonal vegetables with grains.", Price: 12.00),
                (Restaurant: "Green Bowl", Name: "Avocado Grain Bowl", Description: "Grains, avocado, greens, and citrus dressing.", Price: 13.00),
                (Restaurant: "Burger Corner", Name: "Classic Burger", Description: "Beef patty with lettuce, tomato, and house sauce.", Price: 11.50),
                (Restaurant: "Burger Corner", Name: "Crispy Chicken Burger", Description: "Crispy chicken with lettuce and mayo.", Price: 12.00),
                (Restaurant: "Burger Corner", Name: "Plant-Based Burger", Description: "Plant-based patty with fresh vegetables.", Price: 12.50)
            };

            foreach (var item in mealData)
            {
                var restaurant = context.Restaurants.First(entry => entry.Name == item.Restaurant);
                if (!context.Meals.Any(meal =>
                        meal.RestaurantId == restaurant.Id &&
                        meal.Name == item.Name))
                {
                    context.Meals.Add(new Meal
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        RestaurantId = restaurant.Id
                    });
                }
            }

            var account = EnsureAccount(context, passwordHasher, DemoLogin, DemoPassword);
            EnsureAccount(context, passwordHasher, AdminLogin, AdminPassword);

            if (!context.Addresses.Any(address =>
                    address.AccountId == account.Id &&
                    address.Label == "Demo home"))
            {
                context.Addresses.Add(new Address
                {
                    AccountId = account.Id,
                    Country = "Sampleland",
                    City = "Sample City",
                    Street = "Example Street",
                    Building = "1",
                    Label = "Demo home",
                    IsDefault = true
                });
            }

            context.SaveChanges();
        }

        private static bool IsDatabaseEmpty(AppDbContext context) =>
            !context.Accounts.Any() &&
            !context.Restaurants.Any() &&
            !context.Meals.Any() &&
            !context.MealOrder.Any() &&
            !context.Carts.Any() &&
            !context.Orders.Any() &&
            !context.Users.Any() &&
            !context.Organizations.Any() &&
            !context.Addresses.Any() &&
            !context.MealCalories.Any() &&
            !context.DeliveryPricings.Any() &&
            !context.OrderRatings.Any() &&
            !context.Reports.Any() &&
            !context.AppNotifications.Any();

        private static Account EnsureAccount(
            AppDbContext context,
            IPasswordHasher<Account> passwordHasher,
            string login,
            string password)
        {
            var account = context.Accounts.FirstOrDefault(item => item.Login == login);
            if (account is not null)
            {
                return account;
            }

            account = new Account { Login = login };
            account.HashedPassword = passwordHasher.HashPassword(account, password);
            context.Accounts.Add(account);
            context.SaveChanges();
            return account;
        }
    }
}
