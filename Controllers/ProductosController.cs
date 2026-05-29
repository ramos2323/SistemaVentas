using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class ProductosController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();
        [HttpGet]
        public ActionResult Buscar(string q)
        {
            var productos = db.Productos
                .Include("Categoria")
                .Where(p => p.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToLower();
                productos = productos.Where(p =>
                    p.Nombre.ToLower().Contains(q) ||
                    p.Categoria.Nombre.ToLower().Contains(q) ||
                    p.Descripcion.ToLower().Contains(q)
                );
            }

            var resultado = productos.Select(p => new {
                p.Id,
                p.Nombre,
                Categoria = p.Categoria.Nombre,
                p.Precio,
                p.Stock,
                p.Activo
            }).ToList();

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Index()
        {
            var productos = db.Productos.Include("Categoria").ToList();
            return View(productos);
        }

        public ActionResult Create()
        {
            ViewBag.CategoriaId = new SelectList(db.Categorias.Where(c => c.Nombre != null), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                db.Productos.Add(producto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoriaId = new SelectList(db.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        public ActionResult Edit(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null) return HttpNotFound();
            ViewBag.CategoriaId = new SelectList(db.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Producto producto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(producto).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoriaId = new SelectList(db.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        public ActionResult Delete(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            var producto = db.Productos.Include("Categoria").FirstOrDefault(p => p.Id == id);
            if (producto == null) return HttpNotFound();

            bool tieneVentas = db.DetalleVentas.Any(d => d.ProductoId == id);
            ViewBag.TieneVentas = tieneVentas;
            ViewBag.TotalVentas = db.DetalleVentas.Count(d => d.ProductoId == id);

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");

            bool tieneVentas = db.DetalleVentas.Any(d => d.ProductoId == id);
            if (tieneVentas)
            {
                TempData["Error"] = "No se puede eliminar este producto porque aparece en ventas registradas.";
                return RedirectToAction("Index");
            }

            var producto = db.Productos.Find(id);
            db.Productos.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool EstaLogueado() => Session["UsuarioId"] != null;

        // Vista de inventario
        public ActionResult Inventario()
        {
            var inventario = db.Productos.Include("Categoria").Where(p => p.Activo).ToList();
            return View(inventario);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}