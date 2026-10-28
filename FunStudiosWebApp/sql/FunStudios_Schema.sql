-- =====================================================
-- FunStudios - Esquema completo (SOLO estructura, sin datos)
-- Generado desde la base real (localhost\FunStudios) con SMO.
-- Las tablas se ordenan para que las claves foraneas
-- se creen despues de las tablas que referencian.
--
-- Antes de ejecutar, seleccione la base destino, por ejemplo:
--   USE [FunStudios];
-- =====================================================

-- ---------- Tabla: Familias ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Familias](
	[FamiliaId] [int] IDENTITY(1,1) NOT NULL,
	[ApellidosFamilia] [nvarchar](150) NOT NULL,
	[CodigoFamilia] [nvarchar](20) NULL,
 CONSTRAINT [PK_Familias] PRIMARY KEY CLUSTERED 
(
	[FamiliaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Familias_CodigoFamilia] ON [dbo].[Familias]
(
	[CodigoFamilia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO

-- ---------- Tabla: MetodosPago ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MetodosPago](
	[MetodoPagoId] [int] IDENTITY(1,1) NOT NULL,
	[NombreMetodo] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_MetodosPago] PRIMARY KEY CLUSTERED 
(
	[MetodoPagoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO

-- ---------- Tabla: Parametros ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Parametros](
	[ParametroId] [int] IDENTITY(1,1) NOT NULL,
	[Clave] [nvarchar](100) NOT NULL,
	[Valor] [nvarchar](500) NULL,
	[Descripcion] [nvarchar](300) NULL,
	[Tipo] [nvarchar](20) NOT NULL,
	[NombreVisible] [nvarchar](150) NULL,
 CONSTRAINT [PK_Parametros] PRIMARY KEY CLUSTERED 
(
	[ParametroId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Parametros_Clave] ON [dbo].[Parametros]
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Parametros] ADD  CONSTRAINT [DF_Parametros_Tipo]  DEFAULT (N'texto') FOR [Tipo]
GO

-- ---------- Tabla: Roles ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RolId] [int] IDENTITY(1,1) NOT NULL,
	[NombreRol] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[RolId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO

-- ---------- Tabla: TiposAcceso ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TiposAcceso](
	[TipoAccesoId] [int] IDENTITY(1,1) NOT NULL,
	[NombreAcceso] [nvarchar](50) NOT NULL,
	[Precio] [decimal](10, 2) NOT NULL,
	[Descripcion] [nvarchar](200) NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_TiposAcceso] PRIMARY KEY CLUSTERED 
(
	[TipoAccesoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
ALTER TABLE [dbo].[TiposAcceso] ADD  CONSTRAINT [DF_TiposAcceso_Precio]  DEFAULT ((0)) FOR [Precio]
GO
ALTER TABLE [dbo].[TiposAcceso] ADD  CONSTRAINT [DF_TiposAcceso_Activo]  DEFAULT ((1)) FOR [Activo]
GO

-- ---------- Tabla: Tutores ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tutores](
	[TutorId] [int] IDENTITY(1,1) NOT NULL,
	[FamiliaId] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Telefono] [nvarchar](20) NOT NULL,
	[EsPrincipal] [bit] NOT NULL,
	[HuellaTemplate] [varbinary](max) NULL,
 CONSTRAINT [PK_Tutores] PRIMARY KEY CLUSTERED 
(
	[TutorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_Tutores_FamiliaId] ON [dbo].[Tutores]
(
	[FamiliaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Tutores] ADD  CONSTRAINT [DF_Tutores_EsPrincipal]  DEFAULT ((0)) FOR [EsPrincipal]
GO
ALTER TABLE [dbo].[Tutores]  WITH CHECK ADD  CONSTRAINT [FK_Tutores_Familias] FOREIGN KEY([FamiliaId])
REFERENCES [dbo].[Familias] ([FamiliaId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Tutores] CHECK CONSTRAINT [FK_Tutores_Familias]
GO

-- ---------- Tabla: Usuarios ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[UsuarioId] [int] IDENTITY(1,1) NOT NULL,
	[RolId] [int] NOT NULL,
	[Nombres] [nvarchar](100) NOT NULL,
	[Apellidos] [nvarchar](100) NOT NULL,
	[NombreUsuario] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](500) NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED 
(
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
 CONSTRAINT [UQ_Usuarios_NombreUsuario] UNIQUE NONCLUSTERED 
(
	[NombreUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_Usuarios_RolId] ON [dbo].[Usuarios]
(
	[RolId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Usuarios] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Usuarios]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY([RolId])
REFERENCES [dbo].[Roles] ([RolId])
GO
ALTER TABLE [dbo].[Usuarios] CHECK CONSTRAINT [FK_Usuarios_Roles]
GO

-- ---------- Tabla: Clientes ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clientes](
	[ClienteId] [int] IDENTITY(1,1) NOT NULL,
	[FamiliaId] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[FechaNacimiento] [date] NOT NULL,
	[NotasMedicas] [nvarchar](500) NULL,
	[HuellaNinoTemplate] [varbinary](max) NULL,
	[Nacionalidad] [nvarchar](50) NULL,
 CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED 
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_Clientes_FamiliaId] ON [dbo].[Clientes]
(
	[FamiliaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Clientes]  WITH CHECK ADD  CONSTRAINT [FK_Clientes_Familias] FOREIGN KEY([FamiliaId])
REFERENCES [dbo].[Familias] ([FamiliaId])
GO
ALTER TABLE [dbo].[Clientes] CHECK CONSTRAINT [FK_Clientes_Familias]
GO

-- ---------- Tabla: FidelidadCuentas ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FidelidadCuentas](
	[FidelidadId] [int] IDENTITY(1,1) NOT NULL,
	[ClienteId] [int] NOT NULL,
	[ConteoChecks] [tinyint] NOT NULL,
	[EntradasGratisGanadas] [int] NOT NULL,
	[FechaUltimaActualizacion] [datetime2](7) NULL,
 CONSTRAINT [PK_FidelidadCuentas] PRIMARY KEY CLUSTERED 
(
	[FidelidadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
 CONSTRAINT [UQ_FidelidadCuentas_ClienteId] UNIQUE NONCLUSTERED 
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
ALTER TABLE [dbo].[FidelidadCuentas] ADD  DEFAULT ((0)) FOR [ConteoChecks]
GO
ALTER TABLE [dbo].[FidelidadCuentas] ADD  DEFAULT ((0)) FOR [EntradasGratisGanadas]
GO
ALTER TABLE [dbo].[FidelidadCuentas]  WITH CHECK ADD  CONSTRAINT [FK_FidelidadCuentas_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Clientes] ([ClienteId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FidelidadCuentas] CHECK CONSTRAINT [FK_FidelidadCuentas_Clientes]
GO

-- ---------- Tabla: LogsBiometrico ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogsBiometrico](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[FechaHoraEvento] [datetime2](7) NOT NULL,
	[Gravedad] [nvarchar](20) NULL,
	[Operacion] [nvarchar](50) NULL,
	[MensajeDetalle] [nvarchar](500) NOT NULL,
	[IPEstacionTrabajo] [nvarchar](45) NOT NULL,
	[UsuarioId] [int] NULL,
	[ClienteId] [int] NULL,
 CONSTRAINT [PK_LogsBiometrico] PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_LogsBiometrico_ClienteId] ON [dbo].[LogsBiometrico]
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_LogsBiometrico_UsuarioId] ON [dbo].[LogsBiometrico]
(
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[LogsBiometrico]  WITH CHECK ADD  CONSTRAINT [FK_LogsBiometrico_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Clientes] ([ClienteId])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[LogsBiometrico] CHECK CONSTRAINT [FK_LogsBiometrico_Clientes]
GO
ALTER TABLE [dbo].[LogsBiometrico]  WITH CHECK ADD  CONSTRAINT [FK_LogsBiometrico_Usuarios] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuarios] ([UsuarioId])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[LogsBiometrico] CHECK CONSTRAINT [FK_LogsBiometrico_Usuarios]
GO

-- ---------- Tabla: Membresias ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Membresias](
	[MembresiaId] [int] IDENTITY(1,1) NOT NULL,
	[ClienteId] [int] NOT NULL,
	[NombreMembresia] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](300) NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Precio] [decimal](18, 2) NOT NULL,
	[Estado] [nvarchar](20) NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Membresias] PRIMARY KEY CLUSTERED 
(
	[MembresiaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_Membresias_ClienteId] ON [dbo].[Membresias]
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Membresias] ADD  DEFAULT ('Activa') FOR [Estado]
GO
ALTER TABLE [dbo].[Membresias]  WITH CHECK ADD  CONSTRAINT [FK_Membresias_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Clientes] ([ClienteId])
GO
ALTER TABLE [dbo].[Membresias] CHECK CONSTRAINT [FK_Membresias_Clientes]
GO

-- ---------- Tabla: Ventas ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ventas](
	[VentaId] [int] IDENTITY(1,1) NOT NULL,
	[ClienteId] [int] NOT NULL,
	[MetodoPagoId] [int] NOT NULL,
	[Monto] [decimal](18, 2) NOT NULL,
	[Descripcion] [nvarchar](300) NOT NULL,
	[FechaVenta] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED 
(
	[VentaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_Ventas_ClienteId] ON [dbo].[Ventas]
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Ventas_MetodoPagoId] ON [dbo].[Ventas]
(
	[MetodoPagoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Clientes] ([ClienteId])
GO
ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Clientes]
GO
ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_MetodosPago] FOREIGN KEY([MetodoPagoId])
REFERENCES [dbo].[MetodosPago] ([MetodoPagoId])
GO
ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_MetodosPago]
GO

-- ---------- Tabla: EntradasHistorial ----------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntradasHistorial](
	[EntradaId] [int] IDENTITY(1,1) NOT NULL,
	[FamiliaId] [int] NOT NULL,
	[ClienteId] [int] NOT NULL,
	[FechaHoraEntrada] [datetime2](7) NOT NULL,
	[FechaHoraSalida] [datetime2](7) NULL,
	[TipoAccesoId] [int] NOT NULL,
	[MembresiaId] [int] NULL,
	[VentaId] [int] NULL,
	[TutorNombre] [nvarchar](150) NULL,
	[TutorTelefono] [nvarchar](20) NULL,
	[Acompanantes] [int] NOT NULL,
 CONSTRAINT [PK_EntradasHistorial] PRIMARY KEY CLUSTERED 
(
	[EntradaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
CREATE NONCLUSTERED INDEX [IX_EntradasHistorial_ClienteId] ON [dbo].[EntradasHistorial]
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_EntradasHistorial_FamiliaId] ON [dbo].[EntradasHistorial]
(
	[FamiliaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_EntradasHistorial_MembresiaId] ON [dbo].[EntradasHistorial]
(
	[MembresiaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_EntradasHistorial_TipoAccesoId] ON [dbo].[EntradasHistorial]
(
	[TipoAccesoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_EntradasHistorial_VentaId] ON [dbo].[EntradasHistorial]
(
	[VentaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[EntradasHistorial] ADD  CONSTRAINT [DF_EntradasHistorial_Acompanantes]  DEFAULT ((1)) FOR [Acompanantes]
GO
ALTER TABLE [dbo].[EntradasHistorial]  WITH CHECK ADD  CONSTRAINT [FK_EntradasHistorial_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Clientes] ([ClienteId])
GO
ALTER TABLE [dbo].[EntradasHistorial] CHECK CONSTRAINT [FK_EntradasHistorial_Clientes]
GO
ALTER TABLE [dbo].[EntradasHistorial]  WITH CHECK ADD  CONSTRAINT [FK_EntradasHistorial_Familias] FOREIGN KEY([FamiliaId])
REFERENCES [dbo].[Familias] ([FamiliaId])
GO
ALTER TABLE [dbo].[EntradasHistorial] CHECK CONSTRAINT [FK_EntradasHistorial_Familias]
GO
ALTER TABLE [dbo].[EntradasHistorial]  WITH CHECK ADD  CONSTRAINT [FK_EntradasHistorial_Membresias] FOREIGN KEY([MembresiaId])
REFERENCES [dbo].[Membresias] ([MembresiaId])
GO
ALTER TABLE [dbo].[EntradasHistorial] CHECK CONSTRAINT [FK_EntradasHistorial_Membresias]
GO
ALTER TABLE [dbo].[EntradasHistorial]  WITH CHECK ADD  CONSTRAINT [FK_EntradasHistorial_TiposAcceso] FOREIGN KEY([TipoAccesoId])
REFERENCES [dbo].[TiposAcceso] ([TipoAccesoId])
GO
ALTER TABLE [dbo].[EntradasHistorial] CHECK CONSTRAINT [FK_EntradasHistorial_TiposAcceso]
GO
ALTER TABLE [dbo].[EntradasHistorial]  WITH CHECK ADD  CONSTRAINT [FK_EntradasHistorial_Ventas] FOREIGN KEY([VentaId])
REFERENCES [dbo].[Ventas] ([VentaId])
GO
ALTER TABLE [dbo].[EntradasHistorial] CHECK CONSTRAINT [FK_EntradasHistorial_Ventas]
GO

