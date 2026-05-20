using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models
{
    [Table("Ventas")]
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Seleccione un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Completada";

        [StringLength(500)]
        [Display(Name = "Observación")]
        public string Observacion { get; set; }

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<DetalleVenta> DetalleVentas { get; set; }
    }
}