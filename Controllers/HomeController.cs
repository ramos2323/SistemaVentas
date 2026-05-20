using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers
{
    public class HomeController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        public ActionResult Index()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.TotalVentas = db.Ventas.Count();
            ViewBag.IngresosTotales = db.Ventas.Sum(v => (decimal?)v.Total) ?? 0;
            ViewBag.TotalClientes = db.Clientes.Count();
            ViewBag.TotalProductos = db.Productos.Count(p => p.Activo);
            ViewBag.ProductosAgotados = db.Productos.Count(p => p.Stock == 0);
            ViewBag.StockBajo = db.Productos.Count(p => p.Stock > 0 && p.Stock < 10);

            // Ultimas 5 ventas
            ViewBag.UltimasVentas = db.Ventas
                .Include("Cliente")
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .ToList();

            return View();
        }
    }
}