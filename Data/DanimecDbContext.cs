using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DanimecApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DanimecApp.Data;

public class DanimecDbContext : IdentityDbContext<IdentityUser>
{
    public DanimecDbContext(DbContextOptions<DanimecDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<NotaServicio> NotasServicio { get; set; }
    public DbSet<TrabajoRealizado> TrabajosRealizados { get; set; }
    public DbSet<RepuestoServicio> RepuestosServicios { get; set; }
    public DbSet<TrabajoExterno> TrabajosExternos { get; set; }
    public DbSet<InventarioItem> InventarioItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NotaServicio -> Cliente (restrict delete)
        modelBuilder.Entity<NotaServicio>()
            .HasOne(n => n.Cliente)
            .WithMany(c => c.NotasServicio)
            .HasForeignKey(n => n.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // NotaServicio -> Vehiculo (restrict delete)
        modelBuilder.Entity<NotaServicio>()
            .HasOne(n => n.Vehiculo)
            .WithMany(v => v.NotasServicio)
            .HasForeignKey(n => n.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        modelBuilder.Entity<TrabajoRealizado>()
            .Ignore(t => t.Total);

        modelBuilder.Entity<RepuestoServicio>()
            .Ignore(r => r.Total);

        modelBuilder.Entity<InventarioItem>()
            .Ignore(i => i.TieneAlerta);

        modelBuilder.Entity<Cliente>()
            .Ignore(c => c.NombreCompleto);

        modelBuilder.Entity<Vehiculo>()
            .Ignore(v => v.Descripcion);

        // Indexes
        modelBuilder.Entity<NotaServicio>()
            .HasIndex(n => n.Numero)
            .IsUnique();

        modelBuilder.Entity<InventarioItem>()
            .HasIndex(i => i.Codigo);
    }
}
