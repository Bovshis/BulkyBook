using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Category> Categories { get; }
        IRepository<CoverType> CoverTypes { get; }
        IRepository<Product> Products { get; }
        IRepository<Company> Companies { get; }
        IRepository<ApplicationUser> ApplicationUsers { get; }
        IRepository<ShoppingCart> ShoppingCarts{ get; }
        void Save();
        Task SaveAsync();
    }
}
