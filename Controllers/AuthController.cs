using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class AuthController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        public ActionResult Login()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            string hash = GetHash(password);
            var usuario = db.Usuarios.FirstOrDefault(u =>
                u.Email == email &&
                u.PasswordHash == hash &&
                u.Activo);

            if (usuario != null)
            {
                Session["UsuarioId"] = usuario.Id;
                Session["UsuarioNombre"] = usuario.Nombre;
                Session["UsuarioRol"] = usuario.Rol;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email o contraseña incorrectos.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Auth");
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
    }
}