//Panorama Técnico – ProductosController (resumido)
//Tecnología: ASP.NET Core MVC con Razor Views + Bootstrap.
//Acceso a datos: ADO.NET directo usando SqlConnection, SqlCommand y SqlDataReader.
//Persistencia: Stored Procedures en SQL Server (sp_Productos_Listar, sp_Productos_Insertar, sp_Productos_Actualizar, sp_Productos_Eliminar).
//CRUD:
//Listar productos(Index)
//Crear productos(Create GET/POST)
//Editar productos(Edit GET/POST)
//Eliminar productos(Delete)
//Manejo de errores: try/catch con mensajes a TempData o ModelState.
//Métodos auxiliares: ObtenerProducto para reutilizar código al editar o mostrar detalle.
//Tipo de patrón: Dataset / connected, no usa ORM(como EF Core).
//Ventaja: Control completo sobre SQL, integridad y consistencia centralizada en la base de datos.
//Limitación: Conversión manual de tipos y manejo más verboso comparado con ORM.
//Resumen: Frontend MVC → Controller → SP SQL Server → Base de datos.Patrón clásico estilo Tango Gestión, seguro y consistente.

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using TangoWEB.Models;
using System.Collections.Generic;

namespace TangoWEB.Controllers
{
    public class ProductosController : Controller
    {
        private readonly string _connectionString;

        public ProductosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TangoDB");
        }

        //======================================================
        // LISTAR
        //======================================================
        public IActionResult Index(string buscar = null)
        {
            var productos = new List<Producto>();

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Productos_Listar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Buscar",
                    string.IsNullOrEmpty(buscar) ? DBNull.Value : buscar);

                conn.Open();
                using SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    productos.Add(new Producto
                    {
                        ProductoID = (int)r["ProductoID"],                        
                        Nombre = r["Nombre"].ToString(),
                        Descripcion = r["Descripcion"]?.ToString(),
                        Precio = (decimal)r["Precio"],
                        StockActual = (int)r["StockActual"],
                        StockMinimo = (int)r["StockMinimo"],                       
                        Activo = Convert.ToInt32(r["Activo"])
                    });
                }
            }
            catch
            {
                TempData["Error"] = "Error al cargar productos";
            }

            return View("~/Views/Home/Productos.cshtml", productos);
        }

        //======================================================
        // CREAR (GET)
        //======================================================
        public IActionResult Create()
        {
            return View("~/Views/Home/CrearProductos.cshtml", new Producto());
        }

        //======================================================
        // CREAR (POST)
        //======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Producto producto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/CrearProductos.cshtml", producto);

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Productos_Insertar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                cmd.Parameters.AddWithValue("@StockActual", producto.StockActual);
                cmd.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                cmd.Parameters.AddWithValue("@Activo", producto.Activo);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Producto creado correctamente";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error al crear el producto");
                return View("~/Views/Home/CrearProductos.cshtml", producto);
            }
        }

        //======================================================
        // EDITAR (GET)
        //======================================================
        public IActionResult Edit(int id)
        {
            try
            {
                var producto = ObtenerProducto(id);
                if (producto == null)
                    return NotFound();

                return View("~/Views/Home/CrearProductos.cshtml", producto);
            }
            catch
            {
                TempData["Error"] = "Error al cargar producto";
                return RedirectToAction("Index");
            }
        }

        //======================================================
        // EDITAR (POST)
        //======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Producto producto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/CrearProductos.cshtml", producto);

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Productos_Actualizar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductoID", producto.ProductoID);                
                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                cmd.Parameters.AddWithValue("@StockActual", producto.StockActual);
                cmd.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                cmd.Parameters.AddWithValue("@Activo", producto.Activo);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Producto actualizado correctamente";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error al actualizar el producto");
                return View("~/Views/Home/CrearProductos.cshtml", producto);
            }
        }

        //======================================================
        // ELIMINAR
        //======================================================
        public IActionResult Delete(int id)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Productos_Eliminar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Producto eliminado";
            }
            catch
            {
                TempData["Error"] = "Error al eliminar producto";
            }

            return RedirectToAction("Index");
        }

        //======================================================
        // MÉTODO REUTILIZABLE
        //======================================================
        private Producto? ObtenerProducto(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_Productos_Listar", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Buscar", DBNull.Value);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                if ((int)r["ProductoID"] == id)
                {
                    return new Producto
                    {
                        ProductoID = (int)r["ProductoID"],                        
                        Nombre = r["Nombre"].ToString(),
                        Descripcion = r["Descripcion"]?.ToString(),
                        Precio = (decimal)r["Precio"],
                        StockActual = (int)r["StockActual"],
                        StockMinimo = (int)r["StockMinimo"],
                        Activo = Convert.ToInt32(r["Activo"])
                    };
                }
            }
            return null;
        }
    }
}
