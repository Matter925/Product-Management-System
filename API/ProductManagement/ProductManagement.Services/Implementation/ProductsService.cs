using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Models;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services.Implementation
{
    public class ProductsService : BaseService<Product>, IProductsService
    {
        public ProductsService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, httpContextAccessor)
        {
        }
    }
}

