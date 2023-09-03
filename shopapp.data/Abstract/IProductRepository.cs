using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.data.Concrete.EfCore;
using shopapp.entity;

namespace shopapp.data.Abstract
{
    public interface IProductRepository : IRepository<Product>
    {
        Product GetProductDetails(string url);
        Product GetByIdWithCategories(int id);
        List<Product> GetProductsByCategory (string name,int page,int pageSize);
        List<Product> GetPopularProducts();
        List<Product> GetSearchResult(string searchString);
        List<Product> GetHomePageProducts();
        int GetCountByCategory(string category);
        void Update(Product entity, int[] categoryIds);
    }
}