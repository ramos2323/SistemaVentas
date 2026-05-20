using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class VentasController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        public ActionResult Index()
        {
            var ventas = db.Ventas
                .Include("Cliente")
                .Include("Usuario")
                .OrderByDescending(v => v.Fecha)
                .ToList();
            return View(ventas);
        }

        public ActionResult Create()
        {
            ViewBag.ClienteId = new SelectList(db.Clientes, "Id", "Nombre");
            ViewBag.Productos = db.Productos.Where(p => p.Activo && p.Stock > 0).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Venta venta, int[] ProductoIds, int[] Cantidades)
        {
            if (ModelState.IsValid && ProductoIds != null && ProductoIds.Length > 0)
            {
                // Usuario logueado (sesion)
                venta.UsuarioId = (int)Session["UsuarioId"];
                venta.Fecha = DateTime.Now;
                venta.Estado = "Completada";

                db.Ventas.Add(venta);
                db.SaveChanges();

                // Agregar detalles
                for (int i = 0; i < ProductoIds.Length; i++)
                {
                    var producto = db.Productos.Find(ProductoIds[i]);
                    if (producto != null && Cantidades[i] > 0)
                    {
                        var detalle = new DetalleVenta
                        {
                            VentaId = venta.Id,
                            ProductoId = ProductoIds[i],
                            Cantidad = Cantidades[i],
                            PrecioUnitario = producto.Precio
                        };
                        db.DetalleVentas.Add(detalle);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClienteId = new SelectList(db.Clientes, "Id", "Nombre", venta.ClienteId);
            ViewBag.Productos = db.Productos.Where(p => p.Activo && p.Stock > 0).ToList();
            return View(venta);
        }

        public ActionResult Details(int id)
        {
            var venta = db.Ventas
                .Include("Cliente")
                .Include("Usuario")
                .Include("DetalleVentas")
                .FirstOrDefault(v => v.Id == id);
            if (venta == null) return HttpNotFound();
            return View(venta);
        }

        // Reporte de ventas
        public ActionResult Reporte()
        {
            var ventas = db.Ventas
                .Include("Cliente")
                .Include("DetalleVentas")
                .OrderByDescending(v => v.Fecha)
                .ToList();
            return View(ventas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}