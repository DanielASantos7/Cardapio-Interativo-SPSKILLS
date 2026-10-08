using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Restaurante
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public string? Descricao { get; set; }

    public string? Foto { get; set; }

    public string? Endereco { get; set; }

    public int? CidadeId { get; set; }

    public int? TipoId { get; set; }

    public int? DonoId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Cardapio> Cardapios { get; set; } = new List<Cardapio>();

    public virtual Cidade? Cidade { get; set; }

    public virtual Usuario? Dono { get; set; }

    public virtual TiposRestaurate? Tipo { get; set; }
}
