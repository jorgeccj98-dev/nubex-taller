using System.ComponentModel.DataAnnotations;

namespace DanimecApp.Models;

public class Vehiculo
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required, StringLength(60), Display(Name = "Marca")]
    public string Marca { get; set; } = "";

    [Required, StringLength(60), Display(Name = "Modelo")]
    public string Modelo { get; set; } = "";

    [Display(Name = "Año")]
    public int? Anio { get; set; }

    [StringLength(20), Display(Name = "Placa")]
    public string? Placa { get; set; }

    [StringLength(30), Display(Name = "Color")]
    public string? Color { get; set; }

    [Display(Name = "KM Actual")]
    public int? KmActual { get; set; }

    [Display(Name = "Descripción")]
    public string Descripcion => $"{Marca} {Modelo} {Anio} ({Placa})";

    public ICollection<NotaServicio> NotasServicio { get; set; } = new List<NotaServicio>();
}
