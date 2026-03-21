using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanimecApp.Models;

public class NotaServicio
{
    public int Id { get; set; }

    [Display(Name = "N° Orden")]
    public string Numero { get; set; } = "";

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    [Required, Display(Name = "Fecha Entrada")]
    public DateTime FechaEntrada { get; set; } = DateTime.Now;

    [Display(Name = "Fecha Salida Estimada")]
    public DateTime? FechaSalidaEstimada { get; set; }

    [Display(Name = "Fecha Salida Real")]
    public DateTime? FechaSalidaReal { get; set; }

    [Display(Name = "Estado")]
    public EstadoServicio Estado { get; set; } = EstadoServicio.EnProgreso;

    [StringLength(500), Display(Name = "Motivo de Ingreso")]
    public string? MotivoIngreso { get; set; }

    [StringLength(1000), Display(Name = "Observaciones del Cliente")]
    public string? ObservacionesCliente { get; set; }

    [Column(TypeName = "decimal(18,2)"), Display(Name = "SubTotal")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)"), Display(Name = "IVA")]
    public decimal Iva { get; set; }

    [Column(TypeName = "decimal(18,2)"), Display(Name = "Total")]
    public decimal Total { get; set; }

    public ICollection<TrabajoRealizado> TrabajosRealizados { get; set; } = new List<TrabajoRealizado>();
    public ICollection<RepuestoServicio> RepuestosServicios { get; set; } = new List<RepuestoServicio>();
    public ICollection<TrabajoExterno> TrabajosExternos { get; set; } = new List<TrabajoExterno>();

    public void RecalcularTotales()
    {
        decimal subTrabajo = TrabajosRealizados.Sum(t => t.Total);
        decimal subRepuesto = RepuestosServicios.Sum(r => r.Total);
        decimal subExterno = TrabajosExternos.Sum(e => e.Monto);
        SubTotal = subTrabajo + subRepuesto + subExterno;
        Iva = SubTotal * 0.16m;
        Total = SubTotal + Iva;
    }
}
