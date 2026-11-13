using AutoMapper;
using FoodOrdersApi.Entities.Objects;
using FoodOrdersApi.Entities;
using FoodOrdersApi.Models.Order;
using Microsoft.EntityFrameworkCore;
using FoodOrdersApi.Entities.Enum;
using System.Linq.Expressions;
using FoodOrdersApi.Models.MealOrder;

namespace FoodOrdersApi.Services
{
    public interface IOrderService
    {
        int Create(CreateOrderDto dto);
        PagedResult<OrderListDto> GetAll(string filters, int page, string sortBy, SortDirection sortDireciton);
        DetailsOrderDto GetByID(int id);
        OrderEditDto GetEdit(int id);
        IEnumerable<IndividualOrderDto> GetFromCart(int id);
        int Update(int id, UpdateOrderDto dto);
        int Delete(int id);

        // New: get orders by account id
        IEnumerable<OrderListDto> GetByAccount(int accountId);

        // New: get all detailed orders by account id
        IEnumerable<OrderDto> GetAllByAccount(int accountId);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderPricingService _pricing;

        public OrderService(AppDbContext context, IMapper mapper, IOrderPricingService pricing)
        {
            _context = context;
            _mapper = mapper;
            _pricing = pricing;
        }



        // Create new order
        public int Create(CreateOrderDto dto)
        {
            var order = _mapper.Map<Order>(dto);

            var account = _context.Accounts.FirstOrDefault(u => u.Id == dto.AccountId);
            if (account == null) return -2;

            var cart = _context.Carts
                .FirstOrDefault(u => u.Id == dto.CartId);
            if (cart == null) return -3;

            order.Subtotal = 0;
            order.TotalPrice = 0;
            order.IsPriority = dto.IsPriority;
            order.IsBadWeather = dto.IsBadWeather;
            order.Cart = cart;
            _pricing.Recalculate(order);

            _context.Orders.Add(order);

            // set cart inactive after creating order
            cart.IsActive = false;

            _context.SaveChanges();
            _context.AppNotifications.Add(new AppNotification
            {
                Type = FoodOrdersApi.Entities.Enum.Notice.NotificationType.RateOrder,
                Title = "How was your order?",
                Message = $"Please rate order #{order.Id} after delivery.",
                DstAccountId = order.AccountId,
                SenderId = order.AccountId,
                Time = DateTime.UtcNow,
                Read = false,
                OrderId = order.Id,
                RestaurantId = cart.RestaurantId
            });
            _context.SaveChanges();
            return order.Id;
        }


        // Get all orders
        public PagedResult<OrderListDto> GetAll(string filters, int page, string sortBy, SortDirection sortDireciton)
        {
            var baseQuery = _context.Orders
                .Include(o => o.Cart).ThenInclude(c => c.Restaurant)
                .Include(o => o.Cart).ThenInclude(c => c.Organization)
                .Include(o => o.Account)
                .Where(o => filters == null || (
                    o.Cart.Restaurant.Name.ToLower().Contains(filters) ||
                    o.Cart.Organization.Name.ToLower().Contains(filters)) ||
                    o.Id.ToString().Contains(filters));

            if (!string.IsNullOrEmpty(sortBy))
            {
                var columnsSelector = new Dictionary<string, Expression<Func<Order, object>>>
                {
                    { "id", o => o.Id },
                    { "name", o => o.Account.Login },
                    { "organization", o => o.Cart.Organization.Name},
                    { "restaurant", o => o.Cart.Restaurant.Name},
                    { "totalPrice", o => o.TotalPrice},
                    { "positions", o => o.Positions}
                };

                var selectedColumn = columnsSelector[sortBy];

                if (sortBy != "name")
                {
                    baseQuery = sortDireciton == SortDirection.ASC
                        ? baseQuery.OrderBy(selectedColumn)
                        : baseQuery.OrderByDescending(selectedColumn);
                }
                else
                {
                    baseQuery = sortDireciton == SortDirection.ASC
                        ? baseQuery.OrderBy(o => o.Account.Login)
                        : baseQuery.OrderByDescending(o => o.Account.Login);
                }
            }

            var orders = baseQuery
                .Skip(10 * (page - 1))
                .Take(10)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    Name = o.Account.Login,
                    Organization = o.Cart.Organization.Name,
                    Restaurant = o.Cart.Restaurant.Name,
                    Positions = o.Positions,
                    TotalPrice = o.TotalPrice,

                })
                .ToList();

            var totalCount = baseQuery.Count();

            var result = new PagedResult<OrderListDto>(orders, totalCount, page);

            return result;
        }


        // Get order by ID
        public DetailsOrderDto GetByID(int id)
        {
            var order = _context.Orders
                .Include(o => o.MealOrder)
                .Include(o => o.Account)
                .Include(o => o.Cart)
                .Select(o => new DetailsOrderDto
                {
                    Id = o.Id,
                    TotalPrice = o.TotalPrice,
                    NumberOfMeals = o.MealOrder.Sum(mo => mo.Quantity),
                    Notes = o.Notes,
                    Restaurant = o.Cart.Restaurant.Name,
                    Organization = o.Cart.Organization.Name,
                    Cart = o.CartId,
                    User = o.Account.Login
                })
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return null;

            var orderDto = _mapper.Map<DetailsOrderDto>(order);

            return orderDto;
        }


        // Get edit order by ID
        public OrderEditDto GetEdit(int id)
        {
            var order = _context.Orders
                .Include(o => o.Cart)
                .Select(c => new OrderEditDto
                {
                    Id = c.Id,
                    Notes = c.Notes,
                    Cart = c.CartId,
                    Restaurant = c.Cart.RestaurantId
                })
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return null;

            var orderDto = _mapper.Map<OrderEditDto>(order);

            return orderDto;
        }


        // Get all orders from a cart
        public IEnumerable<IndividualOrderDto> GetFromCart(int id)
        {
            var cart = _context.Carts.
                FirstOrDefault(c => c.Id == id);

            var orders = _context.Orders
                .Include(o => o.Account)
                .Where(o => o.CartId == id)
                .Select(o => new IndividualOrderDto
                {
                    Id =  o.Id,
                    Positions =  o.Positions,
                    TotalPrice = o.TotalPrice,
                    User = o.Account.Login
                })
                .ToList();
            var orderDtos = _mapper.Map<List<IndividualOrderDto>>(orders);

            return orderDtos;
        }

        // Get orders by account id
        public IEnumerable<OrderListDto> GetByAccount(int accountId)
        {
            var orders = _context.Orders
                .Include(o => o.Cart).ThenInclude(c => c.Restaurant)
                .Include(o => o.Cart).ThenInclude(c => c.Organization)
                .Include(o => o.Account)
                .Where(o => o.AccountId == accountId)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    Name = o.Account.Login,
                    Organization = o.Cart.Organization.Name,
                    Restaurant = o.Cart.Restaurant.Name,
                    Positions = o.Positions,
                    TotalPrice = o.TotalPrice,
                })
                .ToList();

            return _mapper.Map<List<OrderListDto>>(orders);
        }

        // Get all detailed orders by account id
        public IEnumerable<OrderDto> GetAllByAccount(int accountId)
        {
            var orders = _context.Orders
                .Include(o => o.MealOrder).ThenInclude(mo => mo.Meal)
                .Include(o => o.Account)
                .Include(o => o.Cart).ThenInclude(c => c.Restaurant)
                .Include(o => o.Cart).ThenInclude(c => c.Organization)
                .Where(o => o.AccountId == accountId)
                .ToList();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        // Update order with id
        public int Update(int id, UpdateOrderDto dto)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return -1;

            order.Notes = dto.Notes;

            _context.SaveChanges();
            return order.Id;
        }

        // Delete order with id
        public int Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.Cart)
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return -1;

            order.Cart.TotalCartPrice -= order.TotalPrice;
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return 1;
        }
    }
}
