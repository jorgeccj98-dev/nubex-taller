using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanimecApp.Controllers;

public class ClientesController : Controller
{
    private readonly DanimecDbContext _ctx;

    public ClientesController(DanimecDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index(string? buscar)
    {
        var query = _ctx.Clientes
            .Include(c => c.Vehiculos)
            .Include(c => c.NotasServicio)
            .AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
            query = query.Where(c => c.Nombre.Contains(buscar)
                || c.Apellido.Contains(buscar)
                || (c.Empresa != null && c.Empresa.Contains(buscar))
                || (c.Email != null && c.Email.Contains(buscar)));

        ViewBag.Buscar = buscar;
        return View(await query.OrderBy(c => c.Nombre).ToListAsync());
    }

    public IActionResult Create() => View(new Cliente { FechaRegistro = DateTime.Now });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            _ctx.Clientes.Add(cliente);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Cliente {cliente.NombreCompleto} registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(cliente);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _ctx.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _ctx.Update(cliente);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(cliente);
    }

    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _ctx.Clientes
            .Include(c => c.Vehiculos)
            .Include(c => c.NotasServicio).ThenInclude(n => n.Vehiculo)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _ctx.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _ctx.Clientes.Remove(cliente);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Cliente eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }
}
