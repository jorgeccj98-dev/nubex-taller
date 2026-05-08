using DanimecApp.Data;
using DanimecApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace DanimecApp.Controllers;

[Authorize]
public class InventarioController : Controller
{
    private readonly DanimecDbContext _ctx;

    public InventarioController(DanimecDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index(string? buscar, string? categoria)
    {
        var query = _ctx.InventarioItems.AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
            query = query.Where(i => i.Nombre.Contains(buscar) || (i.Codigo != null && i.Codigo.Contains(buscar)));

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(i => i.Categoria == categoria);

        var items = await query.OrderBy(i => i.Categoria).ThenBy(i => i.Nombre).ToListAsync();
        var categorias = await _ctx.InventarioItems.Select(i => i.Categoria).Distinct().OrderBy(c => c).ToListAsync();

        ViewBag.Buscar = buscar;
        ViewBag.CategoriaFiltro = categoria;
        ViewBag.Categorias = categorias;
        ViewBag.TotalItems = await _ctx.InventarioItems.CountAsync();
        ViewBag.Alertas = await _ctx.InventarioItems.CountAsync(i => i.StockActual <= i.StockMinimo);

        return View(items);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventarioItem item)
    {
        if (ModelState.IsValid)
        {
            _ctx.InventarioItems.Add(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item agregado al inventario.";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _ctx.InventarioItems.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InventarioItem item)
    {
        if (id != item.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _ctx.Update(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _ctx.InventarioItems.FindAsync(id);
        if (item != null)
        {
            _ctx.InventarioItems.Remove(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item eliminado del inventario.";
        }
        return RedirectToAction(nameof(Index));
    }

    // AJAX endpoint: search for inventory items for picker
    [HttpGet]
    public async Task<IActionResult> Buscar(string term)
    {
        var items = await _ctx.InventarioItems
            .Where(i => i.Nombre.Contains(term) || (i.Codigo != null && i.Codigo.Contains(term)))
            .Select(i => new { i.Id, i.Nombre, i.Codigo, i.Categoria, i.PrecioVenta, i.StockActual })
            .Take(10)
            .ToListAsync();
        return Json(items);
    }
}
