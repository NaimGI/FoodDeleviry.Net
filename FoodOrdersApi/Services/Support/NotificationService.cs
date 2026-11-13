namespace FoodOrdersApi.Services.Support
{
    using FoodOrdersApi.Entities;
    using FoodOrdersApi.Entities.Enum.Notice;
    using FoodOrdersApi.Entities.Enum.Report;
    using FoodOrdersApi.Entities.Objects;
    using Microsoft.EntityFrameworkCore;

    public interface INotificationService
    {
        void NotifyOrderCreated(int orderId);
        void NotifyDeliveryPricingChanged(int restaurantId, double value, int category);
        void NotifyRestaurantCreated(int restaurantId);
        void NotifyMealCreated(int mealId, int restaurantId);
        void NotifyReportCreated(int reportId, int orderId, int mealId, int restaurantId);
        void NotifyReportResolved(int reportId, int accountId, ReportStatus status);
    }

    public sealed class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public void NotifyOrderCreated(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Cart).ThenInclude(c => c.Restaurant)
                .FirstOrDefault(o => o.Id == orderId);
            if (order == null) return;

            var now = DateTime.UtcNow;
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.OrderCreated,
                Title = $"Order #{order.Id} created",
                Message = $"Order #{order.Id} ({order.TotalPrice:0.###} DT) was created at {order.Cart.Restaurant.Name}.",
                SenderId = order.AccountId,
                DstAccountId = order.AccountId,
                Time = now,
                Read = false,
                OrderId = order.Id,
                RestaurantId = order.Cart.RestaurantId,
            });
            _context.SaveChanges();
        }

        public void NotifyDeliveryPricingChanged(int restaurantId, double value, int category)
        {
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant == null) return;

            var now = DateTime.UtcNow;
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.DeliveryFeeChanged,
                Title = "Delivery pricing changed",
                Message = $"Delivery pricing changed for {restaurant.Name} (category {category}, value {value:0.###}).",
                SenderId = 1, // 'system' account; see AdminExport note in service
                DstAccountId = 1,
                Time = now,
                Read = false,
                RestaurantId = restaurantId,
            });
            _context.SaveChanges();
        }

        public void NotifyRestaurantCreated(int restaurantId)
        {
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant == null) return;

            var now = DateTime.UtcNow;
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.RestaurantCreated,
                Title = "New restaurant added",
                Message = $"{restaurant.Name} ({restaurant.Category}) was added.",
                SenderId = 1,
                DstAccountId = 1,
                Time = now,
                Read = false,
                RestaurantId = restaurantId,
            });
            _context.SaveChanges();
        }

        public void NotifyMealCreated(int mealId, int restaurantId)
        {
            var meal = _context.Meals.FirstOrDefault(m => m.Id == mealId);
            if (meal == null) return;

            var now = DateTime.UtcNow;
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.MealCreated,
                Title = "New meal added",
                Message = $"{meal.Name} ({meal.Price:0.###} DT) was added to restaurant #{restaurantId}.",
                SenderId = 1,
                DstAccountId = 1,
                Time = now,
                Read = false,
                MealId = mealId,
                RestaurantId = restaurantId,
            });
            _context.SaveChanges();
        }

        public void NotifyReportCreated(int reportId, int orderId, int mealId, int restaurantId)
        {
            var now = DateTime.UtcNow;
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.ReportCreated,
                Title = "New report submitted",
                Message = $"Report #{reportId} submitted (order #{orderId}, meal #{mealId}, restaurant #{restaurantId}).",
                SenderId = 1,
                DstAccountId = 1,
                Time = now,
                Read = false,
                OrderId = orderId,
                MealId = mealId,
                RestaurantId = restaurantId,
            });
            _context.SaveChanges();
        }

        public void NotifyReportResolved(int reportId, int accountId, ReportStatus status)
        {
            var now = DateTime.UtcNow;
            var message = $"Report #{reportId} status changed to {status}.";
            _context.AppNotifications.Add(new AppNotification
            {
                Type = NotificationType.ReportResolved,
                Title = "Report status updated",
                Message = message,
                SenderId = 1,
                DstAccountId = accountId,
                Time = now,
                Read = false,
            });
            _context.SaveChanges();
        }
    }
}
