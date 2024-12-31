using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.API.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public DateOnly? CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }

    public class CreateProductDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;
        [Required]
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }

        public DateOnly? CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }

    public class UpdateProductDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;
        [Required]
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
    }
}
