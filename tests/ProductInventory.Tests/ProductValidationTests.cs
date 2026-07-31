using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ProductInventory.Web.Models;
using Xunit;

namespace ProductInventory.Tests;

public class ProductValidationTests
{
    private static List<ValidationResult> Validate(Product product)
    {
        var context = new ValidationContext(product);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(product, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Valid_Product_HasNoErrors()
    {
        var product = new Product { Name = "Widget", Price = 10m, Quantity = 5 };

        Validate(product).Should().BeEmpty();
    }

    [Fact]
    public void Empty_Name_IsInvalid()
    {
        var product = new Product { Name = "", Price = 10m, Quantity = 5 };

        Validate(product).Should().Contain(r => r.MemberNames.Contains(nameof(Product.Name)));
    }

    [Fact]
    public void Name_LongerThan100Chars_IsInvalid()
    {
        var product = new Product { Name = new string('a', 101), Price = 10m, Quantity = 5 };

        Validate(product).Should().Contain(r => r.MemberNames.Contains(nameof(Product.Name)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositive_Price_IsInvalid(decimal price)
    {
        var product = new Product { Name = "Widget", Price = price, Quantity = 5 };

        Validate(product).Should().Contain(r => r.MemberNames.Contains(nameof(Product.Price)));
    }

    [Fact]
    public void Negative_Quantity_IsInvalid()
    {
        var product = new Product { Name = "Widget", Price = 10m, Quantity = -1 };

        Validate(product).Should().Contain(r => r.MemberNames.Contains(nameof(Product.Quantity)));
    }

    [Fact]
    public void Zero_Quantity_IsValid()
    {
        var product = new Product { Name = "Widget", Price = 10m, Quantity = 0 };

        Validate(product).Should().BeEmpty();
    }
}
