using ProductInventory.Web.Models;

namespace ProductInventory.Web.Services;

/// <summary>
/// Simulates a real, latent backing store: every operation goes through Task.Delay(500)
/// so the UI's loading states are meaningfully exercised, not just theoretical.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(500);

    private readonly List<Product> _products = new();
    private readonly Lock _lock = new();

    public InMemoryProductRepository()
    {
        var seed = new (string Name, decimal Price, int Quantity, bool IsActive)[]
        {
            ("Wireless Mouse", 19.99m, 120, true),
            ("Mechanical Keyboard", 79.99m, 45, true),
            ("27\" 4K Monitor", 349.00m, 18, true),
            ("USB-C Hub", 29.50m, 0, true),
            ("Laptop Stand", 24.00m, 60, true),
            ("Noise Cancelling Headphones", 199.99m, 12, true),
            ("Webcam 1080p", 45.00m, 33, false),
            ("External SSD 1TB", 89.99m, 27, true),
            ("Desk Lamp", 15.75m, 80, true),
            ("Ergonomic Chair", 249.00m, 8, true),
        };

        foreach (var (name, price, quantity, isActive) in seed)
        {
            _products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                Quantity = quantity,
                IsActive = isActive
            });
        }
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm = null, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        lock (_lock)
        {
            IEnumerable<Product> query = _products;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderBy(p => p.Name).Select(p => p.Clone()).ToList();
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        lock (_lock)
        {
            return _products.FirstOrDefault(p => p.Id == id)?.Clone();
        }
    }

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        var toAdd = product.Clone();
        toAdd.Id = Guid.NewGuid();

        lock (_lock)
        {
            _products.Add(toAdd);
        }

        return toAdd.Clone();
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        lock (_lock)
        {
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index == -1)
                throw new InvalidOperationException($"Product '{product.Id}' was not found.");

            _products[index] = product.Clone();
            return _products[index].Clone();
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        lock (_lock)
        {
            _products.RemoveAll(p => p.Id == id);
        }
    }
}
