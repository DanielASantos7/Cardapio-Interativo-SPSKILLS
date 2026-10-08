using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Prato
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public string? Descricao { get; set; }

    public string? Foto { get; set; }

    public string? Ingredientes { get; set; }

    public double? TempoPreparo { get; set; }

    public int? CategoriaId { get; set; }

    public virtual ICollection<Cardapio> Cardapios { get; set; } = new List<Cardapio>();

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<ClienteCurtida> ClienteCurtida { get; set; } = new List<ClienteCurtida>();
}
