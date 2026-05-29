using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class ClientesController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        public ActionResult Index()
        {
            return View(db.Clientes.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                db.Clientes.Add(cliente);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        public ActionResult Edit(int id)
        {
            var cliente = db.Clientes.Find(id);
            if (cliente == null) return HttpNotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cliente).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        public ActionResult Delete(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            var cliente = db.Clientes.Find(id);
            if (cliente == null) return HttpNotFound();

            // Verificar si tiene ventas asociadas
            bool tieneVentas = db.Ventas.Any(v => v.ClienteId == id);
            ViewBag.TieneVentas = tieneVentas;
            ViewBag.TotalVentas = db.Ventas.Count(v => v.ClienteId == id);

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");

            bool tieneVentas = db.Ventas.Any(v => v.ClienteId == id);
            if (tieneVentas)
            {
                TempData["Error"] = "No se puede eliminar este cliente porque tiene ventas registradas.";
                return RedirectToAction("Index");
            }

            var cliente = db.Clientes.Find(id);
            db.Clientes.Remove(cliente);
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