using ProductInventory.Web.Models;

namespace ProductInventory.Web.Services;

public interface IProductRepository
{
    /// <summary>
    /// True once a client has attempted to hydrate this (shared, Singleton) repository from
    /// its own browser storage. Guards against every newly-opened tab re-importing its local
    /// copy over the already-shared state.
    /// </summary>
    bool IsHydrated { get; set; }

    Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm = null, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> AddAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReplaceAllAsync(IReadOnlyList<Product> products, CancellationToken ct = default);
}
