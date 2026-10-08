using System;
using System.Collections.Generic;

namespace CardapioInterativo.API.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public string? Cpf { get; set; }

    public string? Email { get; set; }

    public string? Senha { get; set; }

    public int? PerfilId { get; set; }

    public virtual ICollection<ClienteCurtida> ClienteCurtida { get; set; } = new List<ClienteCurtida>();

    public virtual Perfi? Perfil { get; set; }

    public virtual ICollection<Restaurante> Restaurantes { get; set; } = new List<Restaurante>();
}
