using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using shopapp.business.Abstact;
using shopapp.entity;
using shopapp.webui.Extentions;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private ICartService _cartService;
        private IOrderService _orderService;
        private UserManager<User> _userManager;

        
        public CartController(ICartService cartService,IOrderService orderService,UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
            _orderService = orderService;

        }
        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            return View(
                new CartModel()
                {
                    CardId = cart.Id,
                    CartItems = cart.CartItems.Select(i => new CartItemModel()
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        Name = i.Product.Name,
                        Price = (double)i.Product.Price,
                        ImageUrl = i.Product.ImageUrl,
                        Quantity = i.Quantity

                    }).ToList()
                }
            );
      
        }

        [HttpPost]
        public IActionResult AddToCart(int productId,int quantity)
        {
            var userId = _userManager.GetUserId(User);

            _cartService.AddToCart(userId,productId,quantity);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteFromCart(int productId)
        {
            var userId = _userManager.GetUserId(User);

            _cartService.DeleteFromCart(userId,productId);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            var orderModel = new OrderModel();

            orderModel.CartModel = new CartModel()
            {
                CardId = cart.Id,
                CartItems = cart.CartItems.Select(i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = (double)i.Product.Price,
                    ImageUrl = i.Product.ImageUrl,
                    Quantity = i.Quantity

                }).ToList()
            };

            return View(orderModel);
        }

        [HttpPost]
        public IActionResult Checkout(OrderModel model)
        {

            if(ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var cart = _cartService.GetCartByUserId(userId);

                model.CartModel = new CartModel()
                {
                    CardId = cart.Id,
                    CartItems = cart.CartItems.Select(i => new CartItemModel()
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        Name = i.Product.Name,
                        Price = (double)i.Product.Price,
                        ImageUrl = i.Product.ImageUrl,
                        Quantity = i.Quantity

                    }).ToList()
                };

                var payment = PaymentProcess(model);

                if(payment.Status == "success")
                {
                    SaveOrder(model,payment,userId);

                    ClearCart(model.CartModel.CardId);

                    return View("Success");
                }
                else
                {
                    TempData.Put("message",new AlertMessage
                    {
                        Title = "Hata",
                        Message = $"{payment.ErrorMessage}.",
                        AlertType = "danger"
                    });

                }
            }

            return View(model);
        }

        public IActionResult GetOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderService.GetOrders(userId);
            
            var orderListModel = new List<OrderListModel>();
            OrderListModel orderModel;

            foreach (var order in orders)
            {
                orderModel = new OrderListModel();

                orderModel.OrderId = order.Id;
                orderModel.OrderNumber = order.OrderNumber;
                orderModel.OrderDate = order.OrderDate;
                orderModel.Phone = order.Phone;
                orderModel.FirstName = order.FirstName;
                orderModel.LastName = order.LastName;
                orderModel.Email = order.Email;
                orderModel.Address = order.Address;
                orderModel.City = order.City;
                orderModel.OrderState = order.OrderState;
                orderModel.PaymentType = order.PaymentType;

                orderModel.OrderItems = order.OrderItems.Select(i => new OrderItemModel(){
                    OrderItemId = i.Id,
                    Name = i.Product.Name,
                    Price = (double)i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.ImageUrl
                }).ToList();

                orderListModel.Add(orderModel);

            }

            return View(orderListModel);
        }

        private void ClearCart(int cartId)
        {
            _cartService.ClearCart(cartId);
        }

        private void SaveOrder(OrderModel model, Payment payment, string userId)
        {
            var order = new Order();

            order.OrderNumber = new Random().Next(111111,999999).ToString();
            order.OrderState = EnumOrderState.completed;
            order.PaymentType = EnumPaymentType.CrediCard;
            order.PaymentId = payment.PaymentId;
            order.ConversationId = payment.ConversationId;
            order.OrderDate = new DateTime();
            order.FirstName = model.FirstName;
            order.LastName = model.LastName;
            order.UserId = userId;
            order.Address = model.Address;
            order.Phone = model.Phone;
            order.Email = model.Email;
            order.City = model.City;
            order.Note = model.Note;

            order.OrderItems = new List<entity.OrderItem>();

            foreach (var item in model.CartModel.CartItems)
            {
                var orderItem = new shopapp.entity.OrderItem()
                {
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                };

                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }

        private Payment PaymentProcess(OrderModel model)
        {
            Options options = new Options();
            options.ApiKey = "<>";
            options.SecretKey = "<>";
            options.BaseUrl = "<>";
     
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = new Random().Next(111111111,999999999).ToString();
            request.Price = model.CartModel.TotalPrice().ToString();
            request.PaidPrice = model.CartModel.TotalPrice().ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "<>";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = model.CardName;
            paymentCard.CardNumber = model.CardNumber;
            paymentCard.ExpireMonth = model.ExpirationMonth;
            paymentCard.ExpireYear = model.ExpirationYear;
            paymentCard.Cvc = model.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = "<>";
            buyer.Name = model.FirstName;
            buyer.Surname = model.LastName;
            buyer.GsmNumber = "<>";
            buyer.Email = model.Email;
            buyer.IdentityNumber = "";
            buyer.LastLoginDate = "<>";
            buyer.RegistrationDate = "<>";
            buyer.RegistrationAddress = "<>";
            buyer.Ip = "<>";
            buyer.City = "<>";
            buyer.Country = "<>";
            buyer.ZipCode = "<>";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "<>";
            shippingAddress.City = "<>";
            shippingAddress.Country = "<>";
            shippingAddress.Description = "<>";
            shippingAddress.ZipCode = "<>";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "<>";
            billingAddress.City = "<>";
            billingAddress.Country = "<>";
            billingAddress.Description = "<>";
            billingAddress.ZipCode = "<>";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem basketItem ;

            foreach (var item in model.CartModel.CartItems)
            {
                basketItem = new BasketItem();  
                basketItem.Id = item.ProductId.ToString();
                basketItem.Name = item.Name;
                basketItem.Category1 = "<>";
                basketItem.Price = (item.Price*item.Quantity).ToString();   
                basketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                basketItems.Add(basketItem);
            }

            request.BasketItems = basketItems;

            return Payment.Create(request, options);

        }
    }
}