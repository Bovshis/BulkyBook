using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Category> Categories { get; }
        IRepository<SubCategory> SubCategories { get; }
        IRepository<Format> Formats { get; }
        IRepository<Book> Books { get; }
        IRepository<Product> Products { get; }
        IRepository<Company> Companies { get; }
        IRepository<ApplicationUser> ApplicationUsers { get; }
        IRepository<ShoppingCart> ShoppingCarts{ get; }
        IRepository<OrderDetail> OrderDetails { get; }
        IRepository<OrderHeader> OrderHeaders { get; }
        void Save();
        Task SaveAsync();
    }
}
