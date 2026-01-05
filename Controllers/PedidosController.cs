//# Panorama Técnico – PedidosController
//* **Tecnología:** ASP.NET Core Web API + EF Core para ORM + ADO.NET para operaciones críticas.
//* **Modelo de acceso a datos:**
//  * **ORM (EF Core):** Se usa para `GET`, `PUT`, `DELETE` y combos de clientes/productos (`DbContext`, `Include`, `ToListAsync`).
//  * **ADO.NET / Dataset-like:** Se usa para `ConfirmarVenta`, enviando un **TVP (DataTable)** al SP `sp_Ventas_ConfirmarPedido` que maneja la operación completa en SQL Server.
//* **Operaciones expuestas:**
//  * `GET /api/Pedidos` → Listado de pedidos con detalle
//  * `GET /api/Pedidos/{id}` → Pedido por ID
//  * `POST /api/Pedidos/confirmar` → Confirmar venta **transaccional**, todo o nada
//  * `PUT /api/Pedidos/{id}` → Actualizar pedido
//  * `DELETE /api/Pedidos/{id}` → Eliminar pedido
//  * `GET /api/Pedidos/ClientesCombo` → Lista de clientes activos
//  * `GET /api/Pedidos/ProductosCombo` → Lista de productos activos
//* **Diseño transaccional:**
//  * La confirmación de venta impacta múltiples tablas (`Pedidos`, `PedidoDetalle`, `MovimientoStock`)
//  * Se maneja dentro de un **Stored Procedure con transacción**, asegurando:
//    * Atomicidad
//    * Consistencia
//    * Integridad de datos
//    * Cero estados intermedios corruptos
//* **Ventaja del enfoque mixto:**
//  * EF Core simplifica las operaciones CRUD y consultas
//  * ADO.NET con SP asegura operaciones críticas seguras, evitando inconsistencias en procesos complejos como ventas o pagos.
//* **Patrón general:** API REST que combina **ORM para lectura/CRUD ligero** y **SP SQL Server para transacciones críticas**.





using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangoWEB.Data; // Habilita AppDbContext
using TangoWEB.Models;

namespace TangoWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base: http://localhost:5260/api/Pedidos
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public PedidosController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("TangoDB");
        }

        // -------------------------------
        // GET: api/Pedidos
        // URL en Postman: GET http://localhost:5260/api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Items)
                    .ToListAsync();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // -------------------------------
        // GET: api/Pedidos/{id}
        // URL en Postman: GET http://localhost:5260/api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.PedidoID == id);

                if (pedido == null)
                    return NotFound($"Pedido con ID {id} no encontrado");

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // -------------------------------
        // POST: api/Pedidos/ConfirmarVenta
        // URL en Postman: POST http://localhost:5260/api/Pedidos/ConfirmarVenta
        // -----------------------------------------------------------------------------------------
        // CONFIRMAR VENTA – DISEÑO TRANSACCIONAL
        //
        // Este endpoint NO utiliza múltiples llamadas a APIs REST para insertar cada entidad
        // (Pedido, Detalle, Stock, etc.) porque la operación impacta en varias tablas críticas:
        //
        //  - Pedidos
        //  - PedidoDetalle
        //  - MovimientoStock
        //  - (posiblemente) Asientos contables
        //
        // Si se resolviera con varios POST encadenados, ante una falla intermedia el sistema
        // quedaría en un estado inconsistente (por ejemplo: pedido creado pero stock no
        // actualizado), generando incongruencias graves, típicas de sistemas de pagos.
        //
        // Por este motivo se delega la operación completa a un Stored Procedure:
        //      sp_Ventas_ConfirmarPedido
        //
        // La API sólo envía el pedido y su detalle como un DataTable (TVP – Table Valued Parameter),
        // y el motor de SQL Server se encarga de:
        //
        //  - Abrir la transacción
        //  - Insertar pedido y detalle
        //  - Actualizar stock
        //  - Validar reglas de negocio
        //  - Hacer COMMIT si todo es correcto
        //  - Ejecutar ROLLBACK automático ante cualquier error
        //
        // De esta forma se garantiza:
        //  ✔ Atomicidad
        //  ✔ Consistencia
        //  ✔ Integridad de datos
        //  ✔ Cero estados intermedios corruptos
        //
        // Este patrón replica el funcionamiento de sistemas financieros reales, donde una
        // operación debe confirmarse completa o no impactar en absoluto.
        // -----------------------------------------------------------------------------------------

        [HttpPost("confirmar")]
        public IActionResult ConfirmarVenta([FromBody] JsonElement body)
        {
            try
            {
                int clienteID = body.GetProperty("clienteID").GetInt32();
                decimal total = body.GetProperty("total").GetDecimal();
                var items = body.GetProperty("items").EnumerateArray();

                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("sp_Ventas_ConfirmarPedido", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ClienteID", clienteID);
                cmd.Parameters.AddWithValue("@Total", total);

                DataTable dt = new DataTable();
                dt.Columns.Add("ProductoID", typeof(int));
                dt.Columns.Add("Cantidad", typeof(int));
                dt.Columns.Add("PrecioUnitario", typeof(decimal));

                foreach (var i in items)
                    dt.Rows.Add(
                        i.GetProperty("productoID").GetInt32(),
                        i.GetProperty("cantidad").GetInt32(),
                        i.GetProperty("precioUnitario").GetDecimal()
                    );

                var p = cmd.Parameters.AddWithValue("@Detalle", dt);
                p.SqlDbType = SqlDbType.Structured;

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok("Venta confirmada");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        // -------------------------------
        // PUT: api/Pedidos/{id}
        // URL en Postman: PUT http://localhost:5260/api/Pedidos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, [FromBody] Pedido pedido)
        {
            if (id != pedido.PedidoID)
                return BadRequest("El ID no coincide");

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pedidos.Any(e => e.PedidoID == id))
                    return NotFound($"Pedido con ID {id} no encontrado");
                else
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // -------------------------------
        // DELETE: api/Pedidos/{id}
        // URL en Postman: DELETE http://localhost:5260/api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.PedidoID == id);

                if (pedido == null)
                    return NotFound($"Pedido con ID {id} no encontrado");

                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // -------------------------------
        // GET: api/Pedidos/ClientesCombo
        // URL en Postman: GET http://localhost:5260/api/Pedidos/ClientesCombo
        [HttpGet("ClientesCombo")]
        public async Task<ActionResult<IEnumerable<object>>> GetClientesCombo()
        {
            var clientes = await _context.Clientes
                .Where(c => c.Activo == 1)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.ClienteID,
                    Display = c.Nombre + " - " + (c.CUIT ?? "")
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // -------------------------------
        // GET: api/Pedidos/ProductosCombo
        // URL en Postman: GET http://localhost:5260/api/Pedidos/ProductosCombo
        [HttpGet("ProductosCombo")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductosCombo()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo == 1)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.ProductoID,
                    p.Precio, 
                    Display = p.Nombre + " - $" + p.Precio.ToString("0.00")
                })
                .ToListAsync();

            return Ok(productos);
        }
    }
}
