

namespace TangoWEB.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; } = 0;

        public int StockActual { get; set; } = 0;

        public int StockMinimo { get; set; } = 0;

         public int Activo { get; set; } = 1; // 1 = Sí, 0 = No
    }
}
