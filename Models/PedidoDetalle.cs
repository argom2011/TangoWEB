using System.Text.Json.Serialization;
using TangoWEB.Models;

public class PedidoDetalle
{
    public int PedidoDetalleID { get; set; }
    public int PedidoID { get; set; }
    public int ProductoID { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    [JsonIgnore]                 // 🔥 evita el error: Items[0].Pedido required
    public Pedido Pedido { get; set; }

    [JsonIgnore]                 // 🔥 evita el error: Items[0].Producto required
    public Producto Producto { get; set; }
}
