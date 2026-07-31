using System.Diagnostics;
using FluentAssertions;
using ProductInventory.Web.Models;
using ProductInventory.Web.Services;
using Xunit;

namespace ProductInventory.Tests;

public class InMemoryProductRepositoryTests
{
    private readonly InMemoryProductRepository _sut = new();

    [Fact]
    public async Task GetAllAsync_ReturnsSeededProducts()
    {
        var products = await _sut.GetAllAsync();

        products.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public async Task GetAllAsync_SimulatesLatency()
    {
        var stopwatch = Stopwatch.StartNew();

        await _sut.GetAllAsync();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(450);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersCaseInsensitively()
    {
        var products = await _sut.GetAllAsync("keyboard");

        products.Should().ContainSingle(p => p.Name == "Mechanical Keyboard");
    }

    [Fact]
    public async Task AddAsync_AssignsNewIdAndPersists()
    {
        var product = new Product { Name = "New Gadget", Price = 9.99m, Quantity = 5, IsActive = true };

        var added = await _sut.AddAsync(product);

        added.Id.Should().NotBe(Guid.Empty);
        var all = await _sut.GetAllAsync();
        all.Should().ContainSingle(p => p.Id == added.Id && p.Name == "New Gadget");
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingProduct()
    {
        var added = await _sut.AddAsync(new Product { Name = "Old Name", Price = 1m, Quantity = 1 });
        added.Name = "Updated Name";
        added.Price = 2.5m;

        var updated = await _sut.UpdateAsync(added);

        updated.Name.Should().Be("Updated Name");
        updated.Price.Should().Be(2.5m);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownId_Throws()
    {
        var act = () => _sut.UpdateAsync(new Product { Id = Guid.NewGuid(), Name = "Ghost" });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var added = await _sut.AddAsync(new Product { Name = "To Delete", Price = 1m, Quantity = 1 });

        await _sut.DeleteAsync(added.Id);

        var all = await _sut.GetAllAsync();
        all.Should().NotContain(p => p.Id == added.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsClone_NotSharedReference()
    {
        var added = await _sut.AddAsync(new Product { Name = "Clone Check", Price = 1m, Quantity = 1 });

        var fetched1 = await _sut.GetByIdAsync(added.Id);
        fetched1!.Name = "Mutated Locally";

        var fetched2 = await _sut.GetByIdAsync(added.Id);
        fetched2!.Name.Should().Be("Clone Check", "callers must not be able to mutate repository state through returned references");
    }
}
