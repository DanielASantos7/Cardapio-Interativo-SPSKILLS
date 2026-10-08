using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Cidade
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public virtual ICollection<Restaurante> Restaurantes { get; set; } = new List<Restaurante>();
}
