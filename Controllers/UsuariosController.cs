using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class UsuariosController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        private bool EsAdmin()
        {
            return Session["UsuarioRol"] != null && Session["UsuarioRol"].ToString() == "Admin";
        }

        private bool EstaLogueado()
        {
            return Session["UsuarioId"] != null;
        }

        public ActionResult Index()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");
            return View(db.Usuarios.ToList());
        }

        public ActionResult Create()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");
            ViewBag.Roles = new SelectList(new[] { "Admin", "Vendedor" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Nombre, string Email, string Password, string Rol)
        {
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Rol))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                ViewBag.Roles = new SelectList(new[] { "Admin", "Vendedor" }, Rol);
                return View();
            }

            if (db.Usuarios.Any(u => u.Email == Email))
            {
                ViewBag.Error = "Este email ya está registrado.";
                ViewBag.Roles = new SelectList(new[] { "Admin", "Vendedor" }, Rol);
                return View();
            }

            var usuario = new Usuario
            {
                Nombre = Nombre,
                Email = Email,
                PasswordHash = GetHash(Password),
                Rol = Rol,
                Activo = true
            };

            db.Usuarios.Add(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");
            var usuario = db.Usuarios.Find(id);
            if (usuario == null) return HttpNotFound();
            ViewBag.Roles = new SelectList(new[] { "Admin", "Vendedor" }, usuario.Rol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id, string Nombre, string Email, string NuevoPassword, string Rol, bool Activo = false)
        {
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            var existente = db.Usuarios.Find(Id);
            if (existente == null) return HttpNotFound();

            existente.Nombre = Nombre;
            existente.Email = Email;
            existente.Rol = Rol;
            existente.Activo = Activo;

            if (!string.IsNullOrEmpty(NuevoPassword))
                existente.PasswordHash = GetHash(NuevoPassword);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");
            var usuario = db.Usuarios.Find(id);
            if (usuario == null) return HttpNotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");
            if ((int)Session["UsuarioId"] == id)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario.";
                return RedirectToAction("Index");
            }
            var usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Acceso()
        {
            return View();
        }

        private string GetHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}