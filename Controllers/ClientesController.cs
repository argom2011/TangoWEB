//# Panorama Técnico – `ClientesController` (resumido)
//***Tecnología:**ASP.NET Core MVC con Razor Views + Bootstrap.
//***Acceso a datos:**ADO.NET directo con `SqlConnection`, `SqlCommand` y `SqlDataReader`.
//***Persistencia:**Stored Procedures en SQL Server (`sp_Clientes_Listar`, `sp_Clientes_Insertar`, etc.).
//***CRUD:**Operaciones de Listar, Crear, Editar y Eliminar clientes usando SP.
//***Manejo de errores:** `try/catch` con mensajes a `TempData` o `ModelState`.
//***Tipo de patrón:****Dataset / connected * *, no se usa ORM(EF Core).
//***Ventaja:**Control total de SQL, integridad y consistencia centralizada en la base de datos.
//***Limitación:**Código más verboso y mantenimiento manual de conversiones de datos.
//**Resumen:**Frontend MVC → Controller → SP SQL Server → Base de datos.Patrón clásico tipo Tango Gestión, seguro y consistente, sin ORM.

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using TangoWEB.Models;
using System.Collections.Generic;

namespace TangoWEB.Controllers
{
    public class ClientesController : Controller
    {
        private readonly string _connectionString;

        public ClientesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TangoDB");
        }

        //======================================================
        // LISTAR
        //======================================================
        public IActionResult Index(string buscar = null)
        {
            var clientes = new List<Cliente>();

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Clientes_Listar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Buscar",
                    string.IsNullOrEmpty(buscar) ? DBNull.Value : buscar);

                conn.Open();
                using SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    clientes.Add(new Cliente
                    {
                        ClienteID = (int)r["ClienteID"],
                        Nombre = r["Nombre"].ToString(),
                        CUIT = r["CUIT"]?.ToString(),
                        Direccion = r["Direccion"]?.ToString(),
                        Telefono = r["Telefono"]?.ToString(),
                        Email = r["Email"]?.ToString(),
                        FechaAlta = (DateTime)r["FechaAlta"],
                        Activo = Convert.ToInt32(r["Activo"])
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar clientes";
                // acá podrías loguear ex.Message / ex.StackTrace
            }

            return View("~/Views/Home/Clientes.cshtml", clientes);
        }

        //======================================================
        // CREAR (GET)
        //======================================================
        public IActionResult Create()
        {
            return View("~/Views/Home/CrearClientes.cshtml", new Cliente());
        }

        //======================================================
        // CREAR (POST)
        //======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/CrearClientes.cshtml", cliente);

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Clientes_Insertar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", cliente.CUIT);
                cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Activo", cliente.Activo);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Cliente creado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error al crear el cliente");
                return View("~/Views/Home/CrearClientes.cshtml", cliente);
            }
        }

        //======================================================
        // EDITAR (GET)
        //======================================================
        public IActionResult Edit(int id)
        {
            try
            {
                var cliente = ObtenerCliente(id);
                if (cliente == null)
                    return NotFound();

                return View("~/Views/Home/CrearClientes.cshtml", cliente);
            }
            catch
            {
                TempData["Error"] = "Error al cargar cliente";
                return RedirectToAction("Index");
            }
        }

        //======================================================
        // EDITAR (POST)
        //======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/CrearClientes.cshtml", cliente);

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Clientes_Actualizar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", cliente.ClienteID);
                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", cliente.CUIT);
                cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Activo", cliente.Activo);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Cliente actualizado correctamente";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error al actualizar el cliente");
                return View("~/Views/Home/CrearClientes.cshtml", cliente);
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
                using SqlCommand cmd = new SqlCommand("sp_Clientes_Eliminar", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["Ok"] = "Cliente eliminado";
            }
            catch
            {
                TempData["Error"] = "Error al eliminar cliente";
            }

            return RedirectToAction("Index");
        }

        //======================================================
        // MÉTODO REUTILIZABLE
        //======================================================
        private Cliente? ObtenerCliente(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_Clientes_Listar", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Buscar", DBNull.Value);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                if ((int)r["ClienteID"] == id)
                {
                    return new Cliente
                    {
                        ClienteID = (int)r["ClienteID"],
                        Nombre = r["Nombre"].ToString(),
                        CUIT = r["CUIT"]?.ToString(),
                        Direccion = r["Direccion"]?.ToString(),
                        Telefono = r["Telefono"]?.ToString(),
                        Email = r["Email"]?.ToString(),
                        FechaAlta = (DateTime)r["FechaAlta"],
                        Activo = Convert.ToInt32(r["Activo"])

                    };
                }
            }
            return null;
        }
    }
}
