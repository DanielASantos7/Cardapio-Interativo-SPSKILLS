using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Cardapio
{
    public int Id { get; set; }

    public int? RestauranteId { get; set; }

    public int? PratoId { get; set; }

    public double? Valor { get; set; }

    public virtual Prato? Prato { get; set; }

    public virtual Restaurante? Restaurante { get; set; }
}
