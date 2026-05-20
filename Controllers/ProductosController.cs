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
            var producto = db.Productos.Include("Categoria").FirstOrDefault(p => p.Id == id);
            if (producto == null) return HttpNotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var producto = db.Productos.Find(id);
            db.Productos.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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