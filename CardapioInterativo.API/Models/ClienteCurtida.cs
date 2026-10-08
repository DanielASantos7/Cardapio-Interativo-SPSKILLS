using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class ClienteCurtida
{
    public int Id { get; set; }

    public int? IdCliente { get; set; }

    public int? IdPrato { get; set; }

    public virtual Usuario? IdClienteNavigation { get; set; }

    public virtual Prato? IdPratoNavigation { get; set; }
}
