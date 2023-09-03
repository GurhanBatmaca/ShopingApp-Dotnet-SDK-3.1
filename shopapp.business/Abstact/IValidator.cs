using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace shopapp.business.Abstact
{
    public interface IValidator<T>
    {
        string ErrorMessage { get; set; }
        bool Validation (T entity);
    }
}