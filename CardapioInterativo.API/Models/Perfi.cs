using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Perfi
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
