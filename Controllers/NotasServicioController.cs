using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace DanimecApp.Controllers;

[Authorize]
public class NotasServicioController : Controller
{
    private readonly DanimecDbContext _ctx;

    public NotasServicioController(DanimecDbContext ctx) => _ctx = ctx;

    // GET: Historial de Servicios
    public async Task<IActionResult> Index(string? buscar, EstadoServicio? estado)
    {
        var query = _ctx.NotasServicio
            .Include(n => n.Cliente)
            .Include(n => n.Vehiculo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
            query = query.Where(n => n.Numero.Contains(buscar)
                || n.Cliente!.Nombre.Contains(buscar)
                || n.Cliente!.Apellido.Contains(buscar)
                || n.Vehiculo!.Placa!.Contains(buscar));

        if (estado.HasValue)
            query = query.Where(n => n.Estado == estado.Value);

        var notas = await query.OrderByDescending(n => n.FechaEntrada).ToListAsync();
        ViewBag.Buscar = buscar;
        ViewBag.EstadoFiltro = estado;

        return View(notas);
    }

    // GET: Nueva Nota de Servicio
    public async Task<IActionResult> Create()
    {
        await LoadSelectLists();
        var nota = new NotaServicio { FechaEntrada = DateTime.Now };
        return View(nota);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotaServicio nota)
    {
        // Generate number
        var count = await _ctx.NotasServicio.CountAsync() + 1;
        nota.Numero = $"NS-{DateTime.Now.Year}-{count:D3}";

        ModelState.Remove("Numero");

        if (ModelState.IsValid)
        {
            _ctx.NotasServicio.Add(nota);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Nota de Servicio {nota.Numero} creada exitosamente.";
            return RedirectToAction(nameof(Details), new { id = nota.Id });
        }

        await LoadSelectLists(nota.ClienteId, nota.VehiculoId);
        return View(nota);
    }

    // GET: Detalle y Gestión
    public async Task<IActionResult> Details(int id)
    {
        var nota = await _ctx.NotasServicio
            .Include(n => n.Cliente)
            .Include(n => n.Vehiculo)
            .Include(n => n.TrabajosRealizados)
            .Include(n => n.RepuestosServicios).ThenInclude(r => r.InventarioItem)
            .Include(n => n.TrabajosExternos)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null) return NotFound();

        // Recalculate totals server side
        decimal subTrab = nota.TrabajosRealizados.Sum(t => t.PrecioUnitario * t.Cantidad);
        decimal subRep = nota.RepuestosServicios.Sum(r => r.PrecioUnitario * r.Cantidad);
        decimal subExt = nota.TrabajosExternos.Sum(e => e.Monto);
        nota.SubTotal = subTrab + subRep + subExt;
        nota.Iva = nota.SubTotal * 0.16m;
        nota.Total = nota.SubTotal + nota.Iva;
        _ctx.Update(nota);
        await _ctx.SaveChangesAsync();

        return View(nota);
    }

    // POST: Add Trabajo Realizado
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTrabajo(int notaId, string descripcion, string? tecnico, decimal precioUnitario, int cantidad)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();

        var trabajo = new TrabajoRealizado
        {
            NotaServicioId = notaId,
            Descripcion = descripcion,
            Tecnico = tecnico,
            PrecioUnitario = precioUnitario,
            Cantidad = cantidad
        };
        _ctx.TrabajosRealizados.Add(trabajo);
        await _ctx.SaveChangesAsync();

        TempData["Success"] = "Trabajo agregado corretamente.";
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Add Repuesto
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRepuesto(int notaId, int? inventarioItemId, string descripcion, decimal precioUnitario, int cantidad)
    {
        var repuesto = new RepuestoServicio
        {
            NotaServicioId = notaId,
            InventarioItemId = inventarioItemId,
            Descripcion = descripcion,
            PrecioUnitario = precioUnitario,
            Cantidad = cantidad
        };
        _ctx.RepuestosServicios.Add(repuesto);
        await _ctx.SaveChangesAsync();

        TempData["Success"] = "Repuesto agregado correctamente.";
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Add Trabajo Externo
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExterno(int notaId, string proveedor, string descripcion, decimal monto, bool facturaAdjunta)
    {
        var externo = new TrabajoExterno
        {
            NotaServicioId = notaId,
            Proveedor = proveedor,
            Descripcion = descripcion,
            Monto = monto,
            FacturaAdjunta = facturaAdjunta
        };
        _ctx.TrabajosExternos.Add(externo);
        await _ctx.SaveChangesAsync();

        TempData["Success"] = "Trabajo externo registrado.";
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Delete Trabajo Realizado
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrabajo(int id, int notaId)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var trabajo = await _ctx.TrabajosRealizados.FindAsync(id);
        if (trabajo != null)
        {
            _ctx.TrabajosRealizados.Remove(trabajo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Trabajo eliminado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Delete Repuesto
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRepuesto(int id, int notaId)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var repuesto = await _ctx.RepuestosServicios.FindAsync(id);
        if (repuesto != null)
        {
            _ctx.RepuestosServicios.Remove(repuesto);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Repuesto eliminado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Delete Trabajo Externo
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExterno(int id, int notaId)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var externo = await _ctx.TrabajosExternos.FindAsync(id);
        if (externo != null)
        {
            _ctx.TrabajosExternos.Remove(externo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Trabajo externo eliminado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Edit Trabajo Realizado
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrabajo(int id, int notaId, string descripcion, string? tecnico, decimal precioUnitario, int cantidad)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var trabajo = await _ctx.TrabajosRealizados.FindAsync(id);
        if (trabajo != null)
        {
            trabajo.Descripcion = descripcion;
            trabajo.Tecnico = tecnico;
            trabajo.PrecioUnitario = precioUnitario;
            trabajo.Cantidad = cantidad;
            _ctx.TrabajosRealizados.Update(trabajo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Trabajo actualizado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Edit Repuesto
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRepuesto(int id, int notaId, string descripcion, decimal precioUnitario, int cantidad)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var repuesto = await _ctx.RepuestosServicios.FindAsync(id);
        if (repuesto != null)
        {
            repuesto.Descripcion = descripcion;
            repuesto.PrecioUnitario = precioUnitario;
            repuesto.Cantidad = cantidad;
            _ctx.RepuestosServicios.Update(repuesto);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Repuesto actualizado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Edit Trabajo Externo
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExterno(int id, int notaId, string proveedor, string descripcion, decimal monto, bool facturaAdjunta)
    {
        var nota = await _ctx.NotasServicio.FindAsync(notaId);
        if (nota == null) return NotFound();
        if (nota.Estado == EstadoServicio.Finalizado || nota.Estado == EstadoServicio.Vencido)
        {
            TempData["Error"] = "No se puede modificar una nota finalizada.";
            return RedirectToAction(nameof(Details), new { id = notaId });
        }

        var externo = await _ctx.TrabajosExternos.FindAsync(id);
        if (externo != null)
        {
            externo.Proveedor = proveedor;
            externo.Descripcion = descripcion;
            externo.Monto = monto;
            externo.FacturaAdjunta = facturaAdjunta;
            _ctx.TrabajosExternos.Update(externo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Trabajo externo actualizado correctamente.";
        }
        return RedirectToAction(nameof(Details), new { id = notaId });
    }

    // POST: Change Estado
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoServicio estado)
    {
        var nota = await _ctx.NotasServicio.FindAsync(id);
        if (nota == null) return NotFound();

        nota.Estado = estado;
        if (estado == EstadoServicio.Finalizado)
            nota.FechaSalidaReal = DateTime.Now;

        _ctx.Update(nota);
        await _ctx.SaveChangesAsync();
        TempData["Success"] = "Estado actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // AJAX: Get vehicles by client
    [HttpGet]
    public async Task<IActionResult> VehiculosPorCliente(int clienteId)
    {
        var vehiculos = await _ctx.Vehiculos
            .Where(v => v.ClienteId == clienteId)
            .Select(v => new { v.Id, nombre = $"{v.Marca} {v.Modelo} {v.Anio} ({v.Placa})" })
            .ToListAsync();
        return Json(vehiculos);
    }

    private async Task LoadSelectLists(int? clienteId = null, int? vehiculoId = null)
    {
        var clientes = await _ctx.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);

        if (clienteId.HasValue)
        {
            var vehiculos = await _ctx.Vehiculos.Where(v => v.ClienteId == clienteId).ToListAsync();
            ViewBag.Vehiculos = new SelectList(vehiculos, "Id", "Descripcion", vehiculoId);
        }
        else
        {
            ViewBag.Vehiculos = new SelectList(Enumerable.Empty<Vehiculo>(), "Id", "Descripcion");
        }
    }

    // POST: Delete Nota de Servicio
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nota = await _ctx.NotasServicio
            .Include(n => n.TrabajosRealizados)
            .Include(n => n.RepuestosServicios)
            .Include(n => n.TrabajosExternos)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null) return NotFound();

        if (nota.Estado == EstadoServicio.Finalizado)
        {
            TempData["Error"] = "No se puede eliminar una nota de servicio finalizada.";
            return RedirectToAction(nameof(Index));
        }

        _ctx.NotasServicio.Remove(nota);
        await _ctx.SaveChangesAsync();
        TempData["Success"] = $"Nota {nota.Numero} eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

