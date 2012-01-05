using System.ComponentModel.DataAnnotations;

namespace EFCodeFirstCache.Test
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [StringLength(30)]
        [Required]
        public string ProductNumber { get; set; }

        [StringLength(50)]
        [Required]
        public string ProductName { get; set; }

        [StringLength(int.MaxValue)]
        public string Notes { get; set; }

        public bool IsActive { get; set; }
    }
}
