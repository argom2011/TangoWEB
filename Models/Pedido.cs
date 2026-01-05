//using TangoWEB.Models;
using System.Text.Json.Serialization;

namespace TangoWEB.Models;
public class Pedido
{
    public int PedidoID { get; set; }
    public int ClienteID { get; set; }
    [JsonIgnore]
    public Cliente Cliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";

    public List<PedidoDetalle> Items { get; set; } = new();


}

