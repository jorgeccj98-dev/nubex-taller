using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanimecApp.Models;

public class TrabajoExterno
{
    public int Id { get; set; }

    [Required]
    public int NotaServicioId { get; set; }
    public NotaServicio? NotaServicio { get; set; }

    [Required, StringLength(150), Display(Name = "Proveedor")]
    public string Proveedor { get; set; } = "";

    [Required, StringLength(300), Display(Name = "Descripción")]
    public string Descripcion { get; set; } = "";

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Monto")]
    public decimal Monto { get; set; }

    [Display(Name = "Factura Adjunta")]
    public bool FacturaAdjunta { get; set; }
}
