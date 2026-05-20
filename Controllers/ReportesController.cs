using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using SistemaVentas.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace SistemaVentas.Controllers
{
    public class ReportesController : Controller
    {
        private SistemaVentasContext db = new SistemaVentasContext();

        private bool EstaLogueado() => Session["UsuarioId"] != null;
        private bool EsAdmin() => Session["UsuarioRol"] != null && Session["UsuarioRol"].ToString() == "Admin";

        public ActionResult Index()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            ViewBag.TotalVentas = db.Ventas.Count();
            ViewBag.TotalIngresos = db.Ventas.Sum(v => (decimal?)v.Total) ?? 0;
            ViewBag.TotalProductos = db.Productos.Count(p => p.Activo);
            ViewBag.TotalClientes = db.Clientes.Count();
            return View();
        }

        public ActionResult VentasPDF()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            var ventas = db.Ventas.Include("Cliente").Include("Usuario")
                .OrderByDescending(v => v.Fecha).ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                Font fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.WHITE);
                Font fSubtit = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);
                Font fHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                Font fCell = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                Font fTotal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(21, 128, 61));

                PdfPTable banner = new PdfPTable(1);
                banner.WidthPercentage = 100;
                PdfPCell bannerCell = new PdfPCell(new Phrase("REPORTE DE VENTAS", fTitulo));
                bannerCell.BackgroundColor = new BaseColor(13, 110, 253);
                bannerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                bannerCell.Padding = 14; bannerCell.Border = Rectangle.NO_BORDER;
                banner.AddCell(bannerCell);
                doc.Add(banner);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") +
                    "   |   Total registros: " + ventas.Count, fSubtit));
                doc.Add(new Paragraph(" "));

                PdfPTable tabla = new PdfPTable(6);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 8, 20, 22, 20, 15, 15 });

                foreach (var h in new[] { "#", "Fecha", "Cliente", "Vendedor", "Total", "Estado" })
                {
                    PdfPCell hCell = new PdfPCell(new Phrase(h, fHeader));
                    hCell.BackgroundColor = new BaseColor(13, 110, 253);
                    hCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    hCell.Padding = 7; hCell.Border = Rectangle.NO_BORDER;
                    tabla.AddCell(hCell);
                }

                bool alt = false;
                foreach (var v in ventas)
                {
                    BaseColor bg = alt ? new BaseColor(240, 245, 255) : BaseColor.WHITE;
                    foreach (var c in new[] { v.Id.ToString(), v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        v.Cliente.Nombre, v.Usuario.Nombre, "$" + v.Total.ToString("N2"), v.Estado })
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(c, fCell));
                        cell.BackgroundColor = bg;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 6; cell.Border = Rectangle.BOTTOM_BORDER;
                        cell.BorderColor = new BaseColor(220, 220, 220);
                        tabla.AddCell(cell);
                    }
                    alt = !alt;
                }
                doc.Add(tabla);
                doc.Add(new Paragraph(" "));
                Paragraph pTotal = new Paragraph("TOTAL GENERAL:  $" + ventas.Sum(v => v.Total).ToString("N2"), fTotal);
                pTotal.Alignment = Element.ALIGN_RIGHT;
                doc.Add(pTotal);
                doc.Close();

                return File(ms.ToArray(), "application/pdf",
                    "Reporte_Ventas_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
            }
        }

        public ActionResult VentasExcel()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            ExcelPackage.License.SetNonCommercialPersonal("SistemaVentas");
            var ventas = db.Ventas.Include("Cliente").Include("Usuario")
                .OrderByDescending(v => v.Fecha).ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Ventas");
                ws.Cells["A1:F1"].Merge = true;
                ws.Cells["A1"].Value = "REPORTE DE VENTAS - SISTEMA VENTAS";
                ws.Cells["A1"].Style.Font.Size = 16; ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(13, 110, 253));
                ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A2:F2"].Merge = true;
                ws.Cells["A2"].Value = "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                ws.Cells["A2"].Style.Font.Italic = true;
                ws.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                string[] headers = { "#", "Fecha", "Cliente", "Vendedor", "Total ($)", "Estado" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[4, i + 1].Value = headers[i];
                    ws.Cells[4, i + 1].Style.Font.Bold = true;
                    ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(13, 110, 253));
                    ws.Cells[4, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    ws.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 5; bool alt = false;
                foreach (var v in ventas)
                {
                    ws.Cells[row, 1].Value = v.Id;
                    ws.Cells[row, 2].Value = v.Fecha.ToString("dd/MM/yyyy HH:mm");
                    ws.Cells[row, 3].Value = v.Cliente.Nombre;
                    ws.Cells[row, 4].Value = v.Usuario.Nombre;
                    ws.Cells[row, 5].Value = v.Total;
                    ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 6].Value = v.Estado;
                    if (alt)
                    {
                        ws.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 245, 255));
                    }
                    for (int c = 1; c <= 6; c++) ws.Cells[row, c].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                    alt = !alt; row++;
                }
                ws.Cells[row, 4].Value = "TOTAL GENERAL:"; ws.Cells[row, 4].Style.Font.Bold = true;
                ws.Cells[row, 5].Formula = $"SUM(E5:E{row - 1})";
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00"; ws.Cells[row, 5].Style.Font.Bold = true;
                ws.Cells[row, 5].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(21, 128, 61));
                ws.Cells.AutoFitColumns();

                return File(package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Ventas_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
            }
        }

        public ActionResult InventarioPDF()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            var productos = db.Productos.Include("Categoria").Where(p => p.Activo).ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                Font fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.WHITE);
                Font fHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                Font fCell = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                Font fSubtit = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);
                Font fTotal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(21, 128, 61));

                PdfPTable banner = new PdfPTable(1);
                banner.WidthPercentage = 100;
                PdfPCell bannerCell = new PdfPCell(new Phrase("REPORTE DE INVENTARIO", fTitulo));
                bannerCell.BackgroundColor = new BaseColor(21, 128, 61);
                bannerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                bannerCell.Padding = 14; bannerCell.Border = Rectangle.NO_BORDER;
                banner.AddCell(bannerCell);
                doc.Add(banner);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") +
                    "   |   Productos activos: " + productos.Count, fSubtit));
                doc.Add(new Paragraph(" "));

                PdfPTable tabla = new PdfPTable(6);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 25, 18, 15, 12, 18, 12 });

                foreach (var h in new[] { "Producto", "Categoria", "Precio", "Stock", "Valor Total", "Estado" })
                {
                    PdfPCell hCell = new PdfPCell(new Phrase(h, fHeader));
                    hCell.BackgroundColor = new BaseColor(21, 128, 61);
                    hCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    hCell.Padding = 7; hCell.Border = Rectangle.NO_BORDER;
                    tabla.AddCell(hCell);
                }

                bool alt = false;
                foreach (var p in productos)
                {
                    string estado = p.Stock == 0 ? "Agotado" : p.Stock < 10 ? "Stock bajo" : "Disponible";
                    BaseColor bg = alt ? new BaseColor(240, 255, 245) : BaseColor.WHITE;
                    foreach (var c in new[] { p.Nombre, p.Categoria.Nombre, "$" + p.Precio.ToString("N2"),
                        p.Stock.ToString(), "$" + (p.Precio * p.Stock).ToString("N2"), estado })
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(c, fCell));
                        cell.BackgroundColor = bg; cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 6; cell.Border = Rectangle.BOTTOM_BORDER;
                        cell.BorderColor = new BaseColor(220, 220, 220);
                        tabla.AddCell(cell);
                    }
                    alt = !alt;
                }
                doc.Add(tabla);
                doc.Add(new Paragraph(" "));
                Paragraph pTotal = new Paragraph("VALOR TOTAL INVENTARIO:  $" +
                    productos.Sum(p => p.Precio * p.Stock).ToString("N2"), fTotal);
                pTotal.Alignment = Element.ALIGN_RIGHT;
                doc.Add(pTotal);
                doc.Close();

                return File(ms.ToArray(), "application/pdf",
                    "Inventario_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
            }
        }

        public ActionResult InventarioExcel()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Auth");
            if (!EsAdmin()) return RedirectToAction("Acceso", "Usuarios");

            ExcelPackage.License.SetNonCommercialPersonal("SistemaVentas");
            var productos = db.Productos.Include("Categoria").Where(p => p.Activo).ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Inventario");
                ws.Cells["A1:F1"].Merge = true;
                ws.Cells["A1"].Value = "INVENTARIO - SISTEMA VENTAS";
                ws.Cells["A1"].Style.Font.Size = 16; ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(21, 128, 61));
                ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A2:F2"].Merge = true;
                ws.Cells["A2"].Value = "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                ws.Cells["A2"].Style.Font.Italic = true;
                ws.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                string[] headers = { "Producto", "Categoria", "Precio ($)", "Stock", "Valor ($)", "Estado" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[4, i + 1].Value = headers[i];
                    ws.Cells[4, i + 1].Style.Font.Bold = true;
                    ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(21, 128, 61));
                    ws.Cells[4, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    ws.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 5; bool alt = false;
                foreach (var p in productos)
                {
                    string estado = p.Stock == 0 ? "Agotado" : p.Stock < 10 ? "Stock bajo" : "Disponible";
                    ws.Cells[row, 1].Value = p.Nombre;
                    ws.Cells[row, 2].Value = p.Categoria.Nombre;
                    ws.Cells[row, 3].Value = p.Precio; ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 4].Value = p.Stock;
                    ws.Cells[row, 5].Value = p.Precio * p.Stock; ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 6].Value = estado;

                    if (p.Stock == 0) ws.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    else if (p.Stock < 10) ws.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Orange);
                    else ws.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(21, 128, 61));

                    if (alt)
                    {
                        ws.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 255, 245));
                    }
                    for (int c = 1; c <= 6; c++) ws.Cells[row, c].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                    alt = !alt; row++;
                }
                ws.Cells[row, 4].Value = "VALOR TOTAL:"; ws.Cells[row, 4].Style.Font.Bold = true;
                ws.Cells[row, 5].Formula = $"SUM(E5:E{row - 1})";
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00"; ws.Cells[row, 5].Style.Font.Bold = true;
                ws.Cells[row, 5].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(21, 128, 61));
                ws.Cells.AutoFitColumns();

                return File(package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Inventario_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}