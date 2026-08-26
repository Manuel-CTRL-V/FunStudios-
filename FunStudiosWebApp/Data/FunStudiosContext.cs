using FunStudiosWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Data;

/// <summary>
/// DbContext de FunStudios. La configuración completa de entidades y relaciones
/// se hace inline en OnModelCreating, siguiendo el patrón del proyecto de referencia.
/// Por ahora usa el proveedor InMemory; al migrar a SQL Server solo cambia la
/// configuración del proveedor en Program.cs y se agregan los entity.ToTable("...")
/// con los nombres indicados en cada sección.
/// </summary>
public class FunStudiosContext : DbContext
{
    public FunStudiosContext(DbContextOptions<FunStudiosContext> options)
        : base(options)
    {
    }

    public DbSet<Familia> Familias { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<TipoAcceso> TiposAcceso { get; set; }
    public DbSet<Membresia> Membresias { get; set; }
    public DbSet<EntradaHistorial> EntradasHistorial { get; set; }
    public DbSet<FidelidadCuenta> FidelidadCuentas { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<LogBiometrico> LogsBiometrico { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Roles (tabla SQL: Roles) ----------
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(r => r.RolId);
            entity.Property(r => r.NombreRol).HasMaxLength(50).IsRequired();
        });

        // ---------- Usuarios (tabla SQL: Usuarios) ----------
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.UsuarioId);
            entity.Property(u => u.Nombres).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Apellidos).HasMaxLength(100).IsRequired();
            entity.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();

            entity.HasOne(u => u.Rol)
                  .WithMany()
                  .HasForeignKey(u => u.RolId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Familias (tabla SQL: Familias) ----------
        modelBuilder.Entity<Familia>(entity =>
        {
            entity.HasKey(f => f.FamiliaId);
            entity.Property(f => f.ApellidosFamilia).HasMaxLength(150).IsRequired();
            entity.Property(f => f.NombreTutor1).HasMaxLength(100);
            entity.Property(f => f.TelefonoTutor1).HasMaxLength(20).IsRequired();
            entity.Property(f => f.NombreTutor2).HasMaxLength(100);
            entity.Property(f => f.TelefonoTutor2).HasMaxLength(20);

            entity.HasMany(f => f.Clientes)
                  .WithOne(c => c.Familia!)
                  .HasForeignKey(c => c.FamiliaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Clientes (tabla SQL: Clientes) ----------
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.ClienteId);
            entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(c => c.FechaNacimiento).IsRequired();
            entity.Property(c => c.NotasMedicas).HasMaxLength(500);

            // Relación 1 a 1 con la cuenta de fidelidad
            entity.HasOne(c => c.FidelidadCuenta)
                  .WithOne(fc => fc.Cliente!)
                  .HasForeignKey<FidelidadCuenta>(fc => fc.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- TipoAcceso (tabla SQL: TipoAcceso) ----------
        modelBuilder.Entity<TipoAcceso>(entity =>
        {
            entity.HasKey(ta => ta.TipoAccesoId);
            entity.Property(ta => ta.NombreAcceso).HasMaxLength(50).IsRequired();
        });

        // ---------- Membresias (tabla SQL: Membresias) ----------
        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.HasKey(m => m.MembresiaId);
            entity.Property(m => m.NombreMembresia).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Descripcion).HasMaxLength(300);
            entity.Property(m => m.Estado).HasMaxLength(20).IsRequired();

            entity.HasOne(m => m.Cliente!)
                  .WithMany(c => c.Membresias)
                  .HasForeignKey(m => m.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Entradas_Historial (tabla SQL: Entradas_Historial) ----------
        modelBuilder.Entity<EntradaHistorial>(entity =>
        {
            entity.HasKey(eh => eh.EntradaId);
            entity.Property(eh => eh.FechaHoraEntrada).IsRequired();
            entity.Property(eh => eh.TutorNombre).HasMaxLength(150);
            entity.Property(eh => eh.TutorTelefono).HasMaxLength(20);

            entity.HasOne(eh => eh.Familia)
                  .WithMany()
                  .HasForeignKey(eh => eh.FamiliaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eh => eh.Cliente!)
                  .WithMany(c => c.Entradas)
                  .HasForeignKey(eh => eh.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eh => eh.TipoAcceso!)
                  .WithMany()
                  .HasForeignKey(eh => eh.TipoAccesoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eh => eh.Membresia)
                  .WithMany(m => m.Entradas)
                  .HasForeignKey(eh => eh.MembresiaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eh => eh.Venta)
                  .WithMany()
                  .HasForeignKey(eh => eh.VentaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Fidelidad_Cuentas (tabla SQL: Fidelidad_Cuentas) ----------
        modelBuilder.Entity<FidelidadCuenta>(entity =>
        {
            entity.HasKey(fc => fc.FidelidadId);
        });

        // ---------- Venta (tabla SQL: Venta) ----------
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(v => v.VentaId);
            entity.Property(v => v.Descripcion).HasMaxLength(300).IsRequired();

            entity.HasOne(v => v.Cliente!)
                  .WithMany(c => c.Ventas)
                  .HasForeignKey(v => v.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.MetodoPago!)
                  .WithMany()
                  .HasForeignKey(v => v.MetodoPagoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- MetodoPago (tabla SQL: MetodoPago) ----------
        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(mp => mp.MetodoPagoId);
            entity.Property(mp => mp.NombreMetodo).HasMaxLength(50).IsRequired();
        });

        // ---------- Logs_Biometrico (tabla SQL: Logs_Biometrico) ----------
        modelBuilder.Entity<LogBiometrico>(entity =>
        {
            entity.HasKey(lb => lb.LogId);
            entity.Property(lb => lb.Gravedad).HasMaxLength(20);
            entity.Property(lb => lb.Operacion).HasMaxLength(50);
            entity.Property(lb => lb.MensajeDetalle).HasMaxLength(500).IsRequired();
            entity.Property(lb => lb.IPEstacionTrabajo).HasMaxLength(45).IsRequired();

            entity.HasOne(lb => lb.Usuario)
                  .WithMany()
                  .HasForeignKey(lb => lb.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(lb => lb.Cliente!)
                  .WithMany(c => c.LogsBiometrico)
                  .HasForeignKey(lb => lb.ClienteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
