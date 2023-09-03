using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.business.Abstact
{
    public interface IOrderService
    {
        void Create(Order entity);
        List<Order> GetOrders(string userId);
    }
}