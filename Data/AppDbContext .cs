using Microsoft.EntityFrameworkCore;
using TangoWEB.Models;

namespace TangoWEB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; } // ojo: nombre plural opcional, pero conviene PedidoDetalles

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación Pedido -> Cliente
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany() // si Cliente tuviera List<Pedido> se pondría WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteID);

            // Relación PedidoDetalle -> Producto
            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Producto)
                .WithMany() // si Producto tuviera List<PedidoDetalle> se pondría WithMany(p => p.Detalles)
                .HasForeignKey(d => d.ProductoID);

            // Relación PedidoDetalle -> Pedido
            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.PedidoID);
        }
    }
}
