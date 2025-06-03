namespace SistemaAgendaCitas.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAgendaCitas.Models.Entities;
using System;
using System.Collections.Generic;

public class IndexCitasViewModel
{
    public DateTime? Fecha { get; set; }
    public int? ClienteId { get; set; }
    public EstadoCita? Estado { get; set; }

    public List<SelectListItem> Clientes { get; set; } = new();
    public List<SelectListItem> Estados { get; set; } = new();

    public List<Cita> Citas { get; set; } = new();
}
