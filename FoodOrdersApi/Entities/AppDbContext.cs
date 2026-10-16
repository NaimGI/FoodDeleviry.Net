using FoodOrdersApi.Entities.Objects;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdersApi.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MealOrder> MealOrder { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Address> Addresses { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Meal>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.Meals)
                .HasForeignKey(m => m.RestaurantId);


            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Restaurant)
                    .WithMany(r => r.Carts)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Organization)
                    .WithMany(r => r.Carts)
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account -> Carts (one-to-many)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Account)
                .WithMany(a => a.Carts)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Account -> Orders (one-to-many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Account)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MealOrder>()
                .HasKey(mo => new { mo.MealId, mo.OrderId });
        }
    }
}
