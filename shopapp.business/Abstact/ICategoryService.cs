using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.business.Abstact
{
    public interface ICategoryService: IValidator<Category>
    {
        Task<Category> GetById(int id);

        Category GetByIdWithProducts(int categoryId);

        Task<List<Category>> GetAll();
        void Create(Category entity);
        Task<Category> CreateAsync(Category entity);
        void Update(Category entity);
        void Delete(Category entity);
        void DeleteFromCategory(int productId,int categoryId);
    }
}