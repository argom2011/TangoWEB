using System;

namespace TangoWEB.Models
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; }
        public string CUIT { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public int Activo { get; set; } = 1; // 1 = Sí, 0 = No

    }
}
