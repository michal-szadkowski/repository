using System.ComponentModel.DataAnnotations;
using Repository.WebApi.Data;

namespace Repository.WebApi;

public class Item : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    public int Stock { get; set; }
}
