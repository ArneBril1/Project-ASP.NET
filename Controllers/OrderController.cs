using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Breadpit.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace Breadpit.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Page where a registered user can place an order
        public IActionResult PlaceOrder()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // Action for processing a placed order
        [HttpPost]
        public IActionResult PlaceOrder(List<OrderItemViewModel> orderItems)
        {
            // Logic to process the order and save it in the database
            return RedirectToAction("Index", "Home");
        }

        // Page for viewing all orders and their summary
        public IActionResult OrderSummary()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToList();

            return View(orders);
        }


        // Action for submitting an order
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(List<OrderItemViewModel> orderItems)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = orderItems.Sum(i => i.Quantity * _context.Products.Find(i.ProductId).Price)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in orderItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(OrderSummary));
        }
        public void PurgeOrders()
        {
            var orders = _context.Orders.Include(o => o.OrderItems).ToList();
            foreach (var order in orders)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
            }
            _context.Orders.RemoveRange(orders);
            _context.SaveChanges();
        }
    }
}
