using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanimecApp.Models;

public class InventarioItem
{
    public int Id { get; set; }

    [StringLength(30), Display(Name = "Código")]
    public string? Codigo { get; set; }

    [Required, StringLength(150), Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [StringLength(80), Display(Name = "Categoría")]
    public string? Categoria { get; set; }

    [Display(Name = "Stock Actual")]
    public int StockActual { get; set; }

    [Display(Name = "Stock Mínimo")]
    public int StockMinimo { get; set; } = 5;

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Precio Compra")]
    public decimal PrecioCompra { get; set; }

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Precio Venta")]
    public decimal PrecioVenta { get; set; }

    [StringLength(100), Display(Name = "Proveedor")]
    public string? Proveedor { get; set; }

    [Display(Name = "Alerta Stock")]
    public bool TieneAlerta => StockActual <= StockMinimo;

    public ICollection<RepuestoServicio> RepuestosServicios { get; set; } = new List<RepuestoServicio>();
}
