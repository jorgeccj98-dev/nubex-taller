using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Identity;

namespace DanimecApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(DanimecDbContext ctx, UserManager<IdentityUser> userManager)
    {
        ctx.Database.EnsureCreated();

        // Seed Admin User
        var adminEmail = "admin@danimec.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(user, "Admin123!");
        }

        if (ctx.Clientes.Any()) return;

        // Clientes
        var clientes = new List<Cliente>
        {
            new() { Nombre = "Javier", Apellido = "Morales", Telefono = "+1-555-0101", Email = "javier.morales@email.com", FechaRegistro = DateTime.Now.AddMonths(-6) },
            new() { Nombre = "Elena", Apellido = "Petrova", Telefono = "+1-555-0102", Email = "elena.petrova@email.com", FechaRegistro = DateTime.Now.AddMonths(-4) },
            new() { Nombre = "Ricardo", Apellido = "Sánchez", Telefono = "+1-555-0103", Email = "ricardo.sanchez@email.com", FechaRegistro = DateTime.Now.AddMonths(-3) },
            new() { Nombre = "Constructora", Apellido = "Corp S.A.", Empresa = "Constructora Corp S.A.", Telefono = "+1-555-0199", Email = "admin@constructoracorp.com", FechaRegistro = DateTime.Now.AddMonths(-8) }
        };
        ctx.Clientes.AddRange(clientes);
        ctx.SaveChanges();

        // Vehículos
        var vehiculos = new List<Vehiculo>
        {
            new() { ClienteId = clientes[0].Id, Marca = "Audi", Modelo = "A4", Anio = 2022, Placa = "ABC-1234", Color = "Negro", KmActual = 45000 },
            new() { ClienteId = clientes[1].Id, Marca = "BMW", Modelo = "X5", Anio = 2020, Placa = "DEF-5678", Color = "Blanco", KmActual = 78000 },
            new() { ClienteId = clientes[2].Id, Marca = "Porsche", Modelo = "911", Anio = 2021, Placa = "GHI-9012", Color = "Rojo", KmActual = 32000 },
            new() { ClienteId = clientes[3].Id, Marca = "Toyota", Modelo = "Hilux", Anio = 2019, Placa = "PBW-4521", Color = "Plata", KmActual = 230000 },
            new() { ClienteId = clientes[0].Id, Marca = "Mercedes", Modelo = "C200", Anio = 2023, Placa = "JKL-3456", Color = "Gris", KmActual = 18000 },
            new() { ClienteId = clientes[2].Id, Marca = "Honda", Modelo = "Civic", Anio = 2020, Placa = "MNO-7890", Color = "Azul", KmActual = 95000 },
        };
        ctx.Vehiculos.AddRange(vehiculos);
        ctx.SaveChanges();

        // Inventario
        var inventario = new List<InventarioItem>
        {
            new() { Codigo = "FLT-001", Nombre = "Filtro de Aceite Sintético", Categoria = "Filtros", StockActual = 24, StockMinimo = 10, PrecioCompra = 8.50m, PrecioVenta = 18.00m, Proveedor = "Bosch" },
            new() { Codigo = "FRN-001", Nombre = "Pastillas de Freno Delanteras", Categoria = "Frenos", StockActual = 4, StockMinimo = 6, PrecioCompra = 22.00m, PrecioVenta = 45.00m, Proveedor = "Brembo" },
            new() { Codigo = "ELC-001", Nombre = "Bujía Iridium de Alta Eficiencia", Categoria = "Eléctrico", StockActual = 16, StockMinimo = 8, PrecioCompra = 12.00m, PrecioVenta = 25.00m, Proveedor = "NGK" },
            new() { Codigo = "MOT-001", Nombre = "Kit Correa de Distribución", Categoria = "Motor", StockActual = 3, StockMinimo = 4, PrecioCompra = 45.00m, PrecioVenta = 95.00m, Proveedor = "Gates" },
            new() { Codigo = "FLT-002", Nombre = "Filtro de Aire", Categoria = "Filtros", StockActual = 12, StockMinimo = 8, PrecioCompra = 6.00m, PrecioVenta = 14.00m, Proveedor = "Mann" },
            new() { Codigo = "LUB-001", Nombre = "Aceite Motor 5W-30 Sintético", Categoria = "Lubricantes", StockActual = 30, StockMinimo = 15, PrecioCompra = 9.00m, PrecioVenta = 20.00m, Proveedor = "Mobil 1" },
            new() { Codigo = "FRN-002", Nombre = "Disco de Freno Ventilado", Categoria = "Frenos", StockActual = 2, StockMinimo = 4, PrecioCompra = 38.00m, PrecioVenta = 78.00m, Proveedor = "Brembo" },
            new() { Codigo = "ELC-002", Nombre = "Batería 60Ah", Categoria = "Eléctrico", StockActual = 5, StockMinimo = 3, PrecioCompra = 65.00m, PrecioVenta = 120.00m, Proveedor = "Bosch" },
        };
        ctx.InventarioItems.AddRange(inventario);
        ctx.SaveChanges();

        // Notas de Servicio
        var notas = new List<NotaServicio>
        {
            new() {
                Numero = "NS-2024-001", ClienteId = clientes[0].Id, VehiculoId = vehiculos[0].Id,
                FechaEntrada = DateTime.Now.AddDays(-5), FechaSalidaEstimada = DateTime.Now.AddDays(1),
                Estado = EstadoServicio.EnProgreso, MotivoIngreso = "Sistema de frenos con falla",
                SubTotal = 320m, Iva = 51.2m, Total = 371.2m
            },
            new() {
                Numero = "NS-2024-002", ClienteId = clientes[1].Id, VehiculoId = vehiculos[1].Id,
                FechaEntrada = DateTime.Now.AddDays(-3), FechaSalidaEstimada = DateTime.Now.AddDays(2),
                Estado = EstadoServicio.EnProgreso, MotivoIngreso = "Cambio anual de aceite y filtros",
                SubTotal = 180m, Iva = 28.8m, Total = 208.8m
            },
            new() {
                Numero = "NS-2024-003", ClienteId = clientes[2].Id, VehiculoId = vehiculos[2].Id,
                FechaEntrada = DateTime.Now.AddDays(-2), FechaSalidaEstimada = DateTime.Now.AddDays(3),
                Estado = EstadoServicio.Cotizacion, MotivoIngreso = "Afinación de suspensión",
                SubTotal = 550m, Iva = 88m, Total = 638m
            },
            new() {
                Numero = "NS-2024-004", ClienteId = clientes[3].Id, VehiculoId = vehiculos[3].Id,
                FechaEntrada = DateTime.Now.AddDays(-10), FechaSalidaEstimada = DateTime.Now.AddDays(-2),
                Estado = EstadoServicio.Vencido, MotivoIngreso = "Problema de recalentamiento",
                SubTotal = 750m, Iva = 120m, Total = 870m
            },
            new() {
                Numero = "NS-2024-005", ClienteId = clientes[0].Id, VehiculoId = vehiculos[4].Id,
                FechaEntrada = DateTime.Now.AddDays(-1), FechaSalidaEstimada = DateTime.Now,
                FechaSalidaReal = DateTime.Now, Estado = EstadoServicio.Finalizado,
                MotivoIngreso = "Mantenimiento preventivo 20,000 km",
                SubTotal = 220m, Iva = 35.2m, Total = 255.2m
            },
        };
        ctx.NotasServicio.AddRange(notas);
        ctx.SaveChanges();

        // Trabajos y Repuestos para la primera nota
        var trabajos = new List<TrabajoRealizado>
        {
            new() { NotaServicioId = notas[0].Id, Descripcion = "Mano de obra cambio pastillas de freno", Tecnico = "Marcos Rodriguez", PrecioUnitario = 80m, Cantidad = 1 },
            new() { NotaServicioId = notas[0].Id, Descripcion = "Diagnóstico sistema ABS", Tecnico = "Marcos Rodriguez", PrecioUnitario = 60m, Cantidad = 1 },
        };
        ctx.TrabajosRealizados.AddRange(trabajos);

        var repuestos = new List<RepuestoServicio>
        {
            new() { NotaServicioId = notas[0].Id, InventarioItemId = inventario[1].Id, Descripcion = "Pastillas de Freno Delanteras Brembo", PrecioUnitario = 45m, Cantidad = 2 },
            new() { NotaServicioId = notas[0].Id, InventarioItemId = inventario[6].Id, Descripcion = "Disco de Freno Ventilado", PrecioUnitario = 78m, Cantidad = 1 },
        };
        ctx.RepuestosServicios.AddRange(repuestos);
        ctx.SaveChanges();
    }
}
