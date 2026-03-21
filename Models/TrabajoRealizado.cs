using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanimecApp.Models;

public class TrabajoRealizado
{
    public int Id { get; set; }

    [Required]
    public int NotaServicioId { get; set; }
    public NotaServicio? NotaServicio { get; set; }

    [Required, StringLength(250), Display(Name = "Descripción")]
    public string Descripcion { get; set; } = "";

    [StringLength(100), Display(Name = "Técnico")]
    public string? Tecnico { get; set; }

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Precio Unitario")]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Total")]
    public decimal Total => PrecioUnitario * Cantidad;
}
