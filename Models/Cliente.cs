using System.ComponentModel.DataAnnotations;

namespace DanimecApp.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [Required, StringLength(100)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = "";

    [Display(Name = "Nombre Completo")]
    public string NombreCompleto => $"{Nombre} {Apellido}";

    [Phone, Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress, Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(150), Display(Name = "Empresa")]
    public string? Empresa { get; set; }

    [Display(Name = "Fecha Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    public ICollection<NotaServicio> NotasServicio { get; set; } = new List<NotaServicio>();
}
