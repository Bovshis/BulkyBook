using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Categories = new Repository<Category>(_db);
            CoverTypes = new Repository<CoverType>(_db);
            Products = new Repository<Product>(_db);
            Companies = new Repository<Company>(_db);
            ShoppingCarts = new Repository<ShoppingCart>(_db);
            ApplicationUsers = new Repository<ApplicationUser>(_db);
            OrderDetails = new Repository<OrderDetail>(_db);
            OrderHeaders = new Repository<OrderHeader>(_db);
        }

        public IRepository<Category> Categories { get; }

        public IRepository<CoverType> CoverTypes { get; }
        public IRepository<Product> Products { get; }
        public IRepository<Company> Companies { get; }
        public IRepository<ApplicationUser> ApplicationUsers { get; }
        public IRepository<ShoppingCart> ShoppingCarts { get; }
        public IRepository<OrderDetail> OrderDetails { get; }
        public IRepository<OrderHeader> OrderHeaders { get; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
