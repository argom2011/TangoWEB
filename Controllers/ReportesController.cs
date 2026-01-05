//# Panorama Técnico – ReportesController
//* **Tecnología:** ASP.NET Core MVC + Razor Views + Entity Framework Core (ORM)
//* **Tipo de controlador:** `Controller` para renderizar vistas y manejar datos complejos de consulta
//* **Responsabilidades:**
//* Acción principal: `VentasDashboard` → obtiene pedidos filtrados por fechas, cliente o producto
//* Prepara **listas agregadas para gráficos**:
//* Ventas por producto (`ventasPorProducto`)
//* Ventas por cliente (`ventasPorCliente`)
//* Carga datos de **clientes y productos** para combos de filtros en la vista
//* **Acceso a datos:**
//* Usa **Entity Framework Core (ORM)** con `Include` y `ThenInclude` para cargar relaciones:
//* `Pedidos` → `Items` → `Producto`
//* `Pedidos` → `Cliente`
//* Filtrado dinámico con LINQ (`Where`) y proyecciones (`Select`)
//* **Frontend:**
//* Integración con Razor Views (`VentasDashboard.cshtml`)
//* Visualización de tablas y gráficos mediante listas agregadas en `ViewBag`
//* **Otros detalles:**
//* Manejo de excepciones simple: captura errores y devuelve lista vacía
//* Todo el procesamiento se hace **del lado del servidor**, generando datos listos para la vista
//**Resumen conceptual:**
//Este controlador **no modifica la base de datos**, solo **consulta y proyecta información** usando **ORM (Entity Framework Core)**. La estructura permite construir dashboards y filtros dinámicos sin usar stored procedures ni datasets, aprovechando relaciones entre entidades y LINQ para la agregación de datos.




using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangoWEB.Data;
using TangoWEB.Models;

namespace TangoWEB.Controllers
{
    public class ReportesController : Controller // ⚠️ Hereda de Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // Acción para el Dashboard
        public async Task<IActionResult> VentasDashboard(DateTime? fechaDesde, DateTime? fechaHasta, int? clienteID, int? productoID)
        {
            try
            {
                var query = _context.Pedidos
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Producto)
                    .Include(p => p.Cliente)
                    .AsQueryable();

                
                if (fechaDesde.HasValue)
                {
                    var desde = fechaDesde.Value.Date;
                    query = query.Where(p => p.Fecha >= desde);
                }

                if (fechaHasta.HasValue)
                {
                    var hasta = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.Fecha <= hasta);
                }



                if (clienteID.HasValue)
                    query = query.Where(p => p.ClienteID == clienteID.Value);

                if (productoID.HasValue)
                    query = query.Where(p => p.Items.Any(i => i.ProductoID == productoID.Value));

                var pedidos = await _context.Pedidos
                    .Where(p =>
                        (!fechaDesde.HasValue || p.Fecha >= fechaDesde.Value) &&
                        (!fechaHasta.HasValue || p.Fecha <= fechaHasta.Value) &&
                        (!clienteID.HasValue || p.ClienteID == clienteID.Value) &&
                        (!productoID.HasValue || p.Items.Any(i => i.ProductoID == productoID.Value))
                    )
                    .OrderByDescending(p => p.Fecha)
                    .Select(p => new Pedido
                    {
                        PedidoID = p.PedidoID,
                        Fecha = p.Fecha,
                        ClienteID = p.ClienteID,
                        Total = p.Total,
                        Cliente = new Cliente
                        {
                            ClienteID = p.Cliente.ClienteID,
                            Nombre = p.Cliente.Nombre
                        },

                        Items = p.Items
                        .Where(i => !productoID.HasValue || i.ProductoID == productoID.Value)
                        .Select(i => new PedidoDetalle
                        {
                         PedidoDetalleID = i.PedidoDetalleID,
                         ProductoID = i.ProductoID,
                         Cantidad = i.Cantidad,
                         PrecioUnitario = i.PrecioUnitario,
                         Subtotal = i.Subtotal,
                         Producto = new Producto
                         {
                            ProductoID = i.Producto.ProductoID,
                            Nombre = i.Producto.Nombre
                         }
                       })
                        .ToList()
                       })
                          .ToListAsync();


                        // Datos para gráficos
                var ventasPorProducto = pedidos
                    .SelectMany(p => p.Items)
                    .GroupBy(i => i.Producto?.Nombre ?? "Sin Nombre")
                    .Select(g => new { Producto = g.Key, Total = g.Sum(x => x.Subtotal) })
                    .ToList();

                var ventasPorCliente = pedidos
                    .GroupBy(p => p.Cliente?.Nombre ?? "Sin Nombre")
                    .Select(g => new { Cliente = g.Key, Total = g.Sum(p => p.Total) })
                    .ToList();

                ViewBag.VentasPorProducto = ventasPorProducto;
                ViewBag.VentasPorCliente = ventasPorCliente;

                ViewBag.Clientes = await _context.Clientes
                .OrderBy(c => c.Nombre)
                .Select(c => new Cliente
                {
                    ClienteID = c.ClienteID,
                    Nombre = c.Nombre
                })
                .ToListAsync();

                ViewBag.Productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .Select(p => new Producto
                {
                    ProductoID = p.ProductoID,
                    Nombre = p.Nombre
                })
                .ToListAsync();



                // ⚠️ Vista en Views/Home
                return View("~/Views/Home/VentasDashboard.cshtml", pedidos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en VentasDashboard: " + ex.Message);
                return View("~/Views/Home/VentasDashboard.cshtml", new List<Pedido>());
            }
        }
    }
}
