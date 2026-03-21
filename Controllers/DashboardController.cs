using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanimecApp.Controllers;

public class DashboardController : Controller
{
    private readonly DanimecDbContext _ctx;
    private readonly IConfiguration _config;

    public DashboardController(DanimecDbContext ctx, IConfiguration config)
    {
        _ctx = ctx;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TallerNombre = _config["Taller:Nombre"] ?? "DANIMEC";
        ViewBag.TallerSubtitulo = _config["Taller:Subtitulo"] ?? "Precision Workshop";
        ViewBag.Tecnico = _config["Taller:Tecnico"] ?? "Técnico";
        ViewBag.Cargo = _config["Taller:Cargo"] ?? "Jefe Técnico";

        ViewBag.EnProgreso = await _ctx.NotasServicio.CountAsync(n => n.Estado == EstadoServicio.EnProgreso);
        ViewBag.Cotizaciones = await _ctx.NotasServicio.CountAsync(n => n.Estado == EstadoServicio.Cotizacion);
        ViewBag.FinalizadosHoy = await _ctx.NotasServicio.CountAsync(n =>
            n.Estado == EstadoServicio.Finalizado && n.FechaSalidaReal != null &&
            n.FechaSalidaReal.Value.Date == DateTime.Today);
        ViewBag.Vencidos = await _ctx.NotasServicio.CountAsync(n => n.Estado == EstadoServicio.Vencido);

        var recientes = await _ctx.NotasServicio
            .Include(n => n.Cliente)
            .Include(n => n.Vehiculo)
            .OrderByDescending(n => n.FechaEntrada)
            .Take(8)
            .ToListAsync();

        var alertasInventario = await _ctx.InventarioItems
            .Where(i => i.StockActual <= i.StockMinimo)
            .OrderBy(i => i.StockActual)
            .Take(3)
            .ToListAsync();

        ViewBag.AlertasInventario = alertasInventario;

        return View(recientes);
    }
}
