using System.ComponentModel.DataAnnotations;

namespace ProductInventory.Web.Models;

public class Product
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }

    public bool IsActive { get; set; } = true;

    public Product Clone() => new()
    {
        Id = Id,
        Name = Name,
        Price = Price,
        Quantity = Quantity,
        IsActive = IsActive
    };
}
