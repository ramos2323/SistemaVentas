using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SistemaVentas.Models
{
    public class SistemaVentasContext : DbContext
    {
        public SistemaVentasContext() : base("SistemaVentasContext") { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Evita que EF intente crear o modificar la BD
            Database.SetInitializer<SistemaVentasContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}