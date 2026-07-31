using ProductInventory.Web.Models;

namespace ProductInventory.Web.Services;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm = null, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> AddAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
