using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace DanimecApp.Controllers;

[Authorize]
public class VehiculosController : Controller
{
    private readonly DanimecDbContext _ctx;

    public VehiculosController(DanimecDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index(string? buscar, int? clienteId)
    {
        var query = _ctx.Vehiculos
            .Include(v => v.Cliente)
            .AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
            query = query.Where(v => v.Marca.Contains(buscar)
                || v.Modelo.Contains(buscar)
                || (v.Placa != null && v.Placa.Contains(buscar)));

        if (clienteId.HasValue)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        ViewBag.Buscar = buscar;
        ViewBag.ClienteId = clienteId;
        ViewBag.Clientes = new SelectList(await _ctx.Clientes.OrderBy(c => c.Nombre).ToListAsync(), "Id", "NombreCompleto");
        return View(await query.OrderBy(v => v.Marca).ThenBy(v => v.Modelo).ToListAsync());
    }

    public async Task<IActionResult> Create(int? clienteId)
    {
        await LoadClientes(clienteId);
        return View(new Vehiculo { ClienteId = clienteId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vehiculo vehiculo)
    {
        if (ModelState.IsValid)
        {
            _ctx.Vehiculos.Add(vehiculo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Vehículo {vehiculo.Marca} {vehiculo.Modelo} ({vehiculo.Placa}) registrado.";
            return RedirectToAction(nameof(Index));
        }
        await LoadClientes(vehiculo.ClienteId);
        return View(vehiculo);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vehiculo = await _ctx.Vehiculos.FindAsync(id);
        if (vehiculo == null) return NotFound();
        await LoadClientes(vehiculo.ClienteId);
        return View(vehiculo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
    {
        if (id != vehiculo.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _ctx.Update(vehiculo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Vehículo actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        await LoadClientes(vehiculo.ClienteId);
        return View(vehiculo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vehiculo = await _ctx.Vehiculos.FindAsync(id);
        if (vehiculo != null)
        {
            _ctx.Vehiculos.Remove(vehiculo);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Vehículo eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadClientes(int? selected = null)
    {
        var clientes = await _ctx.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", selected);
    }
}
