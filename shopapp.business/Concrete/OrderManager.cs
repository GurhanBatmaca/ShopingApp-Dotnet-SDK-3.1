using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.business.Abstact;
using shopapp.data.Abstract;
using shopapp.entity;

namespace shopapp.business.Concrete
{
    public class OrderManager : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void Create(Order entity)
        {
            _unitOfWork.Orders.Create(entity);
            _unitOfWork.Save();
        }

        public List<Order> GetOrders(string userId)
        {
            return _unitOfWork.Orders.GetOrders(userId);
        }
    }
}