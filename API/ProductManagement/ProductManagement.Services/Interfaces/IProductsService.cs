using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductManagement.EFCore.Models;

namespace ProductManagement.Services.Interfaces
{
    public interface IProductsService : IBaseService<Product>
    {
    }
}
