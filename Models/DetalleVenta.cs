using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models
{
    [Table("DetalleVentas")]
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Venta")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "Seleccione un producto")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Display(Name = "Precio Unitario")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PrecioUnitario { get; set; }

        // Columna calculada en BD (solo lectura)
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal { get; set; }

        // Navegación
        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
}