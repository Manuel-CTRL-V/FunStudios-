using FunStudiosWebApp.Models;
using FunStudiosWebApp.Security;

namespace FunStudiosWebApp.Data;

/// <summary>
/// Siembra datos iniciales al arrancar la aplicación (catálogos + demo).
/// Con el proveedor InMemory los datos viven solo mientras corre la app,
/// por lo que el seeder se ejecuta en cada inicio si la base está vacía.
/// Al migrar a SQL Server este seeder se reemplaza por migraciones + datos reales.
/// </summary>
public static class FunStudiosSeeder
{
    public static async Task InicializarAsync(FunStudiosContext context, bool incluirDemo)
    {
        // Catálogos del sistema (Roles, tipos de acceso, métodos de pago, parámetros)
        await SembrarCatalogosAsync(context);

        // Usuario administrador inicial
        await SembrarUsuariosAsync(context);

        // Datos de demostración: solo en entornos de desarrollo
        if (incluirDemo)
        {
            await SembrarUsuariosDemoAsync(context);
            await SembrarFamiliasYClientesAsync(context);
            await SembrarOperacionDemoAsync(context);
        }
    }

    private static async Task SembrarCatalogosAsync(FunStudiosContext context)
    {
        // Si ya hay roles, los catálogos ya fueron sembrados.
        if (context.Roles.Any()) return;

        context.Roles.AddRange(
            new Rol { RolId = Rol.Administrador, NombreRol = "Administrador" },
            new Rol { RolId = Rol.Recepcion, NombreRol = "Recepción" });

        context.TiposAcceso.AddRange(
            new TipoAcceso
            {
                TipoAccesoId = TipoAcceso.DayPass, NombreAcceso = "Day Pass",
                Precio = 400.00m, Activo = true,
                Descripcion = "Entrada de un día (pago de $400 RD$)."
            },
            new TipoAcceso
            {
                TipoAccesoId = TipoAcceso.FidelidadGratis, NombreAcceso = "Fidelidad Gratis",
                Precio = 0.00m, Activo = true,
                Descripcion = "Entrada gratis acumulada por fidelidad (sin pago)."
            },
            new TipoAcceso
            {
                TipoAccesoId = TipoAcceso.Membresia, NombreAcceso = "Membresía",
                Precio = 0.00m, Activo = true,
                Descripcion = "Entrada incluida en una membresía vigente (sin pago)."
            });

        context.MetodosPago.AddRange(
            new MetodoPago { MetodoPagoId = 1, NombreMetodo = "Efectivo" },
            new MetodoPago { MetodoPagoId = 2, NombreMetodo = "Tarjeta" },
            new MetodoPago { MetodoPagoId = 3, NombreMetodo = "Transferencia" });

        context.Parametros.AddRange(
            new Parametro
            {
                Clave = "Fidelidad.ChecksParaPremio", Valor = "8",
                Descripcion = "Cantidad de visitas (Day Pass) necesarias para ganar una entrada gratis.",
                Tipo = "numero", NombreVisible = "Checks para premio de fidelidad"
            },
            new Parametro
            {
                Clave = "Fidelidad.ChecksPorDia", Valor = "1",
                Descripcion = "Cantidad máxima de checks que un niño puede sumar por entradas Day Pass dentro de un mismo día.",
                Tipo = "numero", NombreVisible = "Checks máximos por día"
            },
            new Parametro
            {
                Clave = "Moneda.Simbolo", Valor = "RD$",
                Descripcion = "Símbolo de la moneda que se muestra en toda la aplicación.",
                Tipo = "texto", NombreVisible = "Símbolo de moneda"
            },
            new Parametro
            {
                Clave = "Parque.Nombre", Valor = "FunStudios",
                Descripcion = "Nombre del parque que se muestra en reportes y encabezados.",
                Tipo = "texto", NombreVisible = "Nombre del parque"
            });

        await context.SaveChangesAsync();
    }

    private static async Task SembrarUsuariosAsync(FunStudiosContext context)
    {
        // Crea únicamente el administrador inicial si no existe.
        // Cambie la contraseña al entrar por primera vez en producción.
        if (context.Usuarios.Any(u => u.NombreUsuario == "admin")) return;

        context.Usuarios.Add(new Usuario
        {
            Nombres = "Ana",
            Apellidos = "Martínez",
            NombreUsuario = "admin",
            PasswordHash = PasswordHasher.Hash("admin123"),
            RolId = Rol.Administrador,
            Activo = true
        });

        await context.SaveChangesAsync();
    }

    /// <summary>Usuarios de demostración (solo entorno de desarrollo).</summary>
    private static async Task SembrarUsuariosDemoAsync(FunStudiosContext context)
    {
        if (context.Usuarios.Any(u => u.NombreUsuario == "recepcion")) return;

        context.Usuarios.AddRange(
            new Usuario
            {
                Nombres = "Carla",
                Apellidos = "Reyes",
                NombreUsuario = "recepcion",
                PasswordHash = PasswordHasher.Hash("recepcion123"),
                RolId = Rol.Recepcion,
                Activo = true
            },
            new Usuario
            {
                Nombres = "Luis",
                Apellidos = "Ortega",
                NombreUsuario = "lortega",
                PasswordHash = PasswordHasher.Hash("recepcion123"),
                RolId = Rol.Recepcion,
                Activo = false
            });

        await context.SaveChangesAsync();
    }

    private static async Task SembrarFamiliasYClientesAsync(FunStudiosContext context)
    {
        // Los datos demo solo se siembran si aún no hay familias.
        if (context.Familias.Any()) return;

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var familias = new List<Familia>
        {
            new()
            {
                FamiliaId = 1, ApellidosFamilia = "Pérez-García", CodigoFamilia = "FAM-001",
                Tutores = new List<Tutor>
                {
                    new() { Nombre = "Laura García",   Telefono = "555-0101", EsPrincipal = true },
                    new() { Nombre = "Carlos Pérez",   Telefono = "555-0102" }
                }
            },
            new()
            {
                FamiliaId = 2, ApellidosFamilia = "Rodríguez", CodigoFamilia = "FAM-002",
                Tutores = new List<Tutor>
                {
                    new() { Nombre = "María Rodríguez", Telefono = "555-0203", EsPrincipal = true }
                }
            },
            new()
            {
                FamiliaId = 3, ApellidosFamilia = "Thompson-López", CodigoFamilia = "FAM-003",
                Tutores = new List<Tutor>
                {
                    new() { Nombre = "Ana López",       Telefono = "555-0304", EsPrincipal = true },
                    new() { Nombre = "James Thompson",  Telefono = "555-0305" }
                }
            },
            new()
            {
                FamiliaId = 4, ApellidosFamilia = "Kim Herrera", CodigoFamilia = "FAM-004",
                Tutores = new List<Tutor>
                {
                    new() { Nombre = "Soo-Yeon Kim",    Telefono = "555-0406", EsPrincipal = true }
                }
            },
            new()
            {
                FamiliaId = 5, ApellidosFamilia = "Gómez Ruiz", CodigoFamilia = "FAM-005",
                Tutores = new List<Tutor>
                {
                    new() { Nombre = "Valentina Gómez", Telefono = "555-0507", EsPrincipal = true },
                    new() { Nombre = "Andrés Ruiz",     Telefono = "555-0508" }
                }
            }
        };

        context.Familias.AddRange(familias);
        await context.SaveChangesAsync();

        // Tutor principal de cada familia para asignar en las entradas históricas
        var tutorPorFamilia = new Dictionary<int, (string nombre, string? telefono)>
        {
            [1] = ("Laura García",      "555-0101"),
            [2] = ("María Rodríguez",  "555-0203"),
            [3] = ("Ana López",        "555-0304"),
            [4] = ("Soo-Yeon Kim",     "555-0406"),
            [5] = ("Valentina Gómez",  "555-0507")
        };

        var clientes = new List<Cliente>
        {
            // Familia Pérez-García: Leo con 7/8 checks (a punto de ganar) y Ana bebé sin enrolar aún
            new() { ClienteId = 101, FamiliaId = 1, Nombre = "Leo", FechaNacimiento = hoy.AddYears(-6).AddMonths(-3), NotasMedicas = "Alergia al maní.", Nacionalidad = "Dominicana" },
            new() { ClienteId = 102, FamiliaId = 1, Nombre = "Ana", FechaNacimiento = hoy.AddYears(-1).AddMonths(2), NotasMedicas = null, Nacionalidad = "Dominicana" },

            // Familia Rodríguez: Sofia con entrada gratis disponible y Mateo recién empezando
            new() { ClienteId = 201, FamiliaId = 2, Nombre = "Sofía", FechaNacimiento = hoy.AddYears(-8), NotasMedicas = null, Nacionalidad = "Dominicana" },
            new() { ClienteId = 202, FamiliaId = 2, Nombre = "Mateo", FechaNacimiento = hoy.AddYears(-4).AddDays(120), NotasMedicas = "Usa lentes.", Nacionalidad = "Venezolana" },

            // Familia Thompson-López: Emma con membresía activa
            new() { ClienteId = 301, FamiliaId = 3, Nombre = "Emma", FechaNacimiento = hoy.AddYears(-7).AddMonths(6), NotasMedicas = null, Nacionalidad = "Estadounidense" },

            // Familia Kim Herrera: Daniel inactivo (no viene desde hace meses)
            new() { ClienteId = 401, FamiliaId = 4, Nombre = "Daniel", FechaNacimiento = hoy.AddYears(-9).AddMonths(1), NotasMedicas = "Asma. Inhalador en mochila roja.", Nacionalidad = "Panameña" },

            // Familia Gómez Ruiz: Valentina y Sebastián, clientes frecuentes
            new() { ClienteId = 501, FamiliaId = 5, Nombre = "Valentina", FechaNacimiento = hoy.AddYears(-5).AddMonths(8), NotasMedicas = null, Nacionalidad = "Dominicana" },
            new() { ClienteId = 502, FamiliaId = 5, Nombre = "Sebastián", FechaNacimiento = hoy.AddYears(-3).AddMonths(4), NotasMedicas = null, Nacionalidad = "Dominicana" }
        };

        context.Clientes.AddRange(clientes);
        await context.SaveChangesAsync();

        context.Membresias.AddRange(
            new Membresia
            {
                ClienteId = 301, NombreMembresia = "Mensual", Descripcion = "Acceso ilimitado de lunes a viernes.",
                FechaInicio = hoy.AddDays(-10), FechaFin = hoy.AddDays(20),
                Precio = 1500.00m, Estado = "Activa", FechaCreacion = DateTime.Now.AddDays(-12)
            },
            new Membresia
            {
                ClienteId = 301, NombreMembresia = "Mensual", Descripcion = "Ciclo anterior.",
                FechaInicio = hoy.AddMonths(-1).AddDays(-2), FechaFin = hoy.AddMonths(-1),
                Precio = 1500.00m, Estado = "Vencida", FechaCreacion = DateTime.Now.AddMonths(-1).AddDays(-4)
            },
            new Membresia
            {
                ClienteId = 501, NombreMembresia = "Semestral", Descripcion = "Promoción aniversario.",
                FechaInicio = hoy.AddDays(-45), FechaFin = hoy.AddMonths(6).AddDays(-15),
                Precio = 6500.00m, Estado = "Activa", FechaCreacion = DateTime.Now.AddDays(-46)
            });

        // Cuentas de fidelidad: una por cliente (relación 1 a 1)
        var fidelidades = clientes.Select(c => new FidelidadCuenta
        {
            ClienteId = c.ClienteId,
            ConteoChecks = c.ClienteId switch
            {
                101 => 7,   // A punto de ganar su entrada gratis
                102 => 0,   // Bebé sin visitas
                201 => 0,   // Ya canjeó: tiene saldo disponible
                202 => 3,
                301 => 5,
                401 => 1,
                501 => 0, // Ciclo completo reiniciado tras canjear
                _ => 2
            },
            EntradasGratisGanadas = c.ClienteId switch
            {
                201 => 1,   // Puede entrar gratis hoy (RF10)
                501 => 2,   // Dos veces premiada históricamente
                _ => 0
            },
            FechaUltimaActualizacion = DateTime.Now.AddDays(-1)
        }).ToList();

        context.FidelidadCuentas.AddRange(fidelidades);
        await context.SaveChangesAsync();
    }

    /// <summary>Entradas, salidas, ventas y logs para que dashboards y reportes muestren actividad realista.</summary>
    private static async Task SembrarOperacionDemoAsync(FunStudiosContext context)
    {
        if (context.EntradasHistorial.Any()) return;

        var hoy = DateTime.Now.Date;

        var tutorPorFamilia = new Dictionary<int, (string nombre, string? telefono)>
        {
            [1] = ("Laura García",      "555-0101"),
            [2] = ("María Rodríguez",  "555-0203"),
            [3] = ("Ana López",        "555-0304"),
            [4] = ("Soo-Yeon Kim",     "555-0406"),
            [5] = ("Valentina Gómez",  "555-0507")
        };

        // ---------- Historial de días anteriores ----------
        var ventasHistoricas = new List<Venta>();
        for (var diasAtras = 14; diasAtras >= 1; diasAtras--)
        {
            var fechaDia = hoy.AddDays(-diasAtras);
            // Entre 2 y 5 day passes vendidos por día histórico
            var pasesDelDia = 2 + (fechaDia.DayOfYear + diasAtras) % 4;

            // Horario: L-V 15:00-20:00 | S-D 10:00-21:00
            var esFinDeSemanaHistorico = fechaDia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var horaApertura = esFinDeSemanaHistorico ? 10 : 15;
            var horasDisponibles = esFinDeSemanaHistorico ? 11 : 6;

            for (var i = 0; i < pasesDelDia; i++)
            {
                var clienteId = new[] { 101, 201, 202, 301, 401, 501, 502 }[(diasAtras * 3 + i) % 7];
                var horaBase = horaApertura + (i * horasDisponibles) / pasesDelDia;
                var horaVenta = fechaDia.AddHours(horaBase).AddMinutes((i * 15) % 60);

                var venta = new Venta
                {
                    ClienteId = clienteId,
                    MetodoPagoId = ((diasAtras + i) % 3) + 1,
                    Monto = 400.00m,
                    Descripcion = "Day Pass",
                    FechaVenta = horaVenta
                };
                ventasHistoricas.Add(venta);
                context.Ventas.Add(venta);
            }

            // Una entrada gratis canjeadas cada ciertos días (RF10 - histórico)
            if (diasAtras % 5 == 0)
            {
                var familiaGratis = diasAtras % 2 == 0 ? 5 : 2;
                var horaGratis = esFinDeSemanaHistorico ? fechaDia.AddHours(12) : fechaDia.AddHours(16);
                context.EntradasHistorial.Add(new EntradaHistorial
                {
                    FamiliaId = familiaGratis,
                    ClienteId = diasAtras % 2 == 0 ? 501 : 201,
                    FechaHoraEntrada = horaGratis,
                    FechaHoraSalida = horaGratis.AddHours(4),
                    TipoAccesoId = TipoAcceso.FidelidadGratis,
                    TutorNombre = tutorPorFamilia[familiaGratis].nombre,
                    TutorTelefono = tutorPorFamilia[familiaGratis].telefono,
                    Acompanantes = 1
                });
            }

            await context.SaveChangesAsync();
            foreach (var venta in ventasHistoricas)
            {
                var famId = context.Clientes.Local.First(c => c.ClienteId == venta.ClienteId).FamiliaId;
                context.EntradasHistorial.Add(new EntradaHistorial
                {
                    FamiliaId = famId,
                    ClienteId = venta.ClienteId,
                    FechaHoraEntrada = venta.FechaVenta.AddMinutes(5),
                    FechaHoraSalida = venta.FechaVenta.AddHours(4),
                    TipoAccesoId = TipoAcceso.DayPass,
                    VentaId = venta.VentaId,
                    TutorNombre = tutorPorFamilia[famId].nombre,
                    TutorTelefono = tutorPorFamilia[famId].telefono,
                    Acompanantes = 1
                });
            }
            ventasHistoricas.Clear();
        }

        await context.SaveChangesAsync();

        // ---------- Hoy: niños dentro del parque ahora ----------
        var entradasHoy = new List<(int clienteId, int tipoAcceso, DateTime entrada)>
        {
            (301, TipoAcceso.Membresia, DateTime.Now.AddHours(-3).AddMinutes(-20)),
            (501, TipoAcceso.Membresia, DateTime.Now.AddHours(-2).AddMinutes(-45)),
            (202, TipoAcceso.DayPass,   DateTime.Now.AddHours(-1).AddMinutes(-10)),
            (101, TipoAcceso.DayPass,   DateTime.Now.AddMinutes(-35))
        };

        // Tutor de hoy por clienteId para asignar responsable de entrada
        var tutorHoyPorCliente = new Dictionary<int, (string nombre, string? telefono)>
        {
            [301] = ("Ana López", "555-0304"),       // Emma → Thompson: tutor 2 (James) alguna vez, hoy Ana
            [501] = ("Valentina Gómez", "555-0507"), // Sebastián → Gómez: tutor 1
            [202] = ("María Rodríguez", null),       // Mateo → Rodríguez: solo tutor 1
            [101] = ("Carlos Pérez", "555-0102")     // Leo → Pérez: tutor 2 Carlos acompaña hoy
        };

        foreach (var (clienteId, tipoAccesoId, horaEntrada) in entradasHoy)
        {
            var familiaId = context.Clientes.Local
                .First(c => c.ClienteId == clienteId)
                .FamiliaId;

            var tutor = tutorHoyPorCliente.GetValueOrDefault(clienteId, tutorPorFamilia[familiaId]);

            var entradaHoy = new EntradaHistorial
            {
                FamiliaId = familiaId,
                ClienteId = clienteId,
                FechaHoraEntrada = horaEntrada,
                TipoAccesoId = tipoAccesoId,
                TutorNombre = tutor.nombre,
                TutorTelefono = tutor.telefono,
                // Ejemplo multi-tutor: Leo (Pérez-García) entra con Carlos + Laura, Sebastián (Gómez) con dos tutores
                Acompanantes = clienteId is 101 or 501 ? 2 : 1
            };

            // El Day Pass de hoy genera su venta correspondiente (histórico básico de facturas)
            if (tipoAccesoId == TipoAcceso.DayPass)
            {
                var ventaHoy = new Venta
                {
                    ClienteId = clienteId,
                    MetodoPagoId = (clienteId % 3) + 1,
                    Monto = 400.00m,
                    Descripcion = "Day Pass",
                    FechaVenta = horaEntrada
                };
                context.Ventas.Add(ventaHoy);
                await context.SaveChangesAsync();
                entradaHoy.VentaId = ventaHoy.VentaId;
            }

            context.EntradasHistorial.Add(entradaHoy);
        }

        // Salida registrada esta mañana (control de presencia RF7)
        var esFinDeSemanaHoy = hoy.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var horaSebastian = esFinDeSemanaHoy ? hoy.AddHours(10).AddMinutes(30) : hoy.AddHours(15).AddMinutes(30);
        context.EntradasHistorial.Add(new EntradaHistorial
        {
            FamiliaId = 5,
            ClienteId = 502,
            FechaHoraEntrada = horaSebastian,
            FechaHoraSalida = horaSebastian.AddHours(4),
            TipoAccesoId = TipoAcceso.DayPass,
            TutorNombre = "Abuela Rosa",
            TutorTelefono = "809-555-1234",
            Acompanantes = 1
        });

        await context.SaveChangesAsync();

        // ---------- Logs biométricos demo ----------
        context.LogsBiometrico.AddRange(
            new LogBiometrico
            {
                FechaHoraEvento = DateTime.Now.AddHours(-4), Gravedad = "Info", Operacion = "Enrolamiento",
                MensajeDetalle = "Enrolamiento completado: 3/3 muestras válidas para 'Valentina'.",
                IPEstacionTrabajo = "192.168.1.50", UsuarioId = 2, ClienteId = 501
            },
            new LogBiometrico
            {
                FechaHoraEvento = DateTime.Now.AddHours(-3).AddMinutes(-19), Gravedad = "Info", Operacion = "Identificacion",
                MensajeDetalle = "Identificación exitosa: Emma Thompson-López (entrada por Membresía).",
                IPEstacionTrabajo = "192.168.1.50", UsuarioId = 2, ClienteId = 301
            },
            new LogBiometrico
            {
                FechaHoraEvento = DateTime.Now.AddHours(-2), Gravedad = "Advertencia", Operacion = "Identificacion",
                MensajeDetalle = "Huella no reconocida tras 3 intentos. Se solicitó registro manual.",
                IPEstacionTrabajo = "192.168.1.50", UsuarioId = 2
            },
            new LogBiometrico
            {
                FechaHoraEvento = DateTime.Now.AddMinutes(-36), Gravedad = "Error", Operacion = "ConexionLector",
                MensajeDetalle = "[SIMULADO] Lector DigitalPersona no responde. Reintentando conexión...",
                IPEstacionTrabajo = "192.168.1.50", UsuarioId = 2
            });
    }
}
