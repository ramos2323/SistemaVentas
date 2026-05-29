using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class CategoriasController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        public ActionResult Index()
        {
            return View(db.Categorias.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                db.Categorias.Add(categoria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(categoria);
        }

        public ActionResult Edit(int id)
        {
            var categoria = db.Categorias.Find(id);
            if (categoria == null) return HttpNotFound();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoria).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(categoria);
        }



        public ActionResult Delete(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            var categoria = db.Categorias.Find(id);
            if (categoria == null) return HttpNotFound();

            bool tieneProductos = db.Productos.Any(p => p.CategoriaId == id);
            ViewBag.TieneProductos = tieneProductos;
            ViewBag.TotalProductos = db.Productos.Count(p => p.CategoriaId == id);

            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");

            bool tieneProductos = db.Productos.Any(p => p.CategoriaId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar esta categoría porque tiene productos asociados.";
                return RedirectToAction("Index");
            }

            var categoria = db.Categorias.Find(id);
            db.Categorias.Remove(categoria);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool EstaLogueado() => Session["UsuarioId"] != null;


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}