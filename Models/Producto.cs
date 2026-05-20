using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, 99999.99, ErrorMessage = "Precio inválido")]
        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock no puede ser negativo")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Seleccione una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Navegación
        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        public virtual ICollection<DetalleVenta> DetalleVentas { get; set; }
    }
}