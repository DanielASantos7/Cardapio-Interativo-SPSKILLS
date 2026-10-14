using CardapioInterativo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardapioInterativo.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext db;

        public ApiController(AppDbContext context)
        {
            db = context;
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var usuario = await db.Usuarios
                                  .Where(u => u.Email == dto.Email && u.Senha == dto.Password)
                                  .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Unauthorized("Credenciais inválidas");
            }

            return Ok(new
            {
                usuario.Email,
                usuario.PerfilId,
                usuario.Id
            });
        }

        [HttpGet("restaurantes")]
        public async Task<IActionResult> GetTodosRestaurantes(
            [FromQuery] int usuarioId,
            [FromQuery] int perfilId,
            [FromQuery] string pesquisa = "")
        {
            var query = db.Restaurantes
                .Include(r => r.Tipo)
                .Include(r => r.Cidade)
                .AsQueryable();

            if (perfilId == 1)
            {
                query = query.Where(r => r.DonoId == usuarioId);
            }

            if (perfilId != 1)
            {
                query = query.Where(r => r.DeletedAt == null);
            }

            if (!string.IsNullOrWhiteSpace(pesquisa) && pesquisa.Length >= 2)
            {
                query = query.Where(r =>
                    r.Nome.Contains(pesquisa) ||
                    r.Tipo.Nome.Contains(pesquisa));
            }

            var restaurantes = await query
                .OrderBy(r => r.DeletedAt != null)
                .ThenBy(r => r.Nome)
                .ToListAsync();

            return Ok(restaurantes.Select(r => new RestaurantesResponseDto
            {
                Id = r.Id,
                Nome = r.Nome,
                Descricao = r.Descricao,
                Cidade = r.Cidade.Nome,
                Tipo = r.Tipo.Nome,
                Excluido = r.DeletedAt != null
            }));
        }

        [HttpDelete("restaurantes/{id:int}")]
        public async Task<IActionResult> ExcluirRestaurante(
            int id,
            [FromQuery] int usuarioId,
            [FromQuery] int perfilId
            )
        {
            if (perfilId != 1)
                return Forbid();

            var restaurante = await db.Restaurantes
                .FirstOrDefaultAsync(r => r.Id == id && r.DonoId == usuarioId);

            if (restaurante == null)
                return NotFound();

            if (restaurante.DeletedAt != null)
                return BadRequest("Restaurante já excluído.");

            restaurante.DeletedAt = DateTime.Now;

            await db.SaveChangesAsync();

            return Ok(true);
        }

        [HttpPost("restaurantes/{id:int}/restaurar")]
        public async Task<IActionResult> RestaurarRestaurante(
            int id,
            [FromQuery] int usuarioId,
            [FromQuery] int perfilId)
        {
            if (perfilId != 1)
                return Forbid();

            var restaurante = await db.Restaurantes
                .FirstOrDefaultAsync(r => r.Id == id && r.DonoId == usuarioId);

            if (restaurante == null)
                return NotFound();

            if (restaurante.DeletedAt == null)
                return BadRequest("Restaurante já está ativo");

            restaurante.DeletedAt = null;

            await db.SaveChangesAsync();

            return Ok(true);
        }

        [HttpGet("cardapio/{restauranteId:int}")]
        public async Task<IActionResult> GetCardapio(int restauranteId)
        {
            var idsMenu = await db.Cardapios
                .Where(c => c.RestauranteId == restauranteId &&
                            c.PratoId != null)
                .Select(c => c.PratoId!.Value)
                .ToListAsync();

            var disponiveis = await db.Pratos
                .Where(p => !idsMenu.Contains(p.Id))
                .Select(p => new PratoDto { Id = p.Id })
                .ToListAsync();

            var menu = await db.Pratos
                .Where(p => idsMenu.Contains(p.Id))
                .Select(p => new PratoDto { Id = p.Id })
                .ToListAsync();

            return Ok(new CardapioDto
            {
                Disponiveis = disponiveis,
                Menu = menu
            });
        }

        [HttpPost("cardapio")]
        public async Task<IActionResult> AdicionarPrato(
            [FromBody] CardapioRequestDto dto)
        {
            bool existe = await db.Cardapios.AnyAsync(c =>
                c.RestauranteId == dto.RestauranteId &&
                c.PratoId == dto.PratoId);

            if (existe)
                return BadRequest("Esse prato já está no cardápio.");

            var cardapio = new Cardapio
            {
                RestauranteId = dto.RestauranteId,
                PratoId = dto.PratoId,
                Valor = dto.Valor
            };

            db.Cardapios.Add(cardapio);
            await db.SaveChangesAsync();

            return Ok(true);
        }

        [HttpDelete("cardapio/{restauranteId:int}/{pratoId:int}")]
        public async Task<IActionResult> RemoverPrato(
            int restauranteId,
            int pratoId)
        {
            var cardapio = await db.Cardapios.FirstOrDefaultAsync(c =>
                c.RestauranteId == restauranteId &&
                c.PratoId == pratoId);

            if (cardapio == null)
                return NotFound("Prato não encontrado no cardápio.");

            db.Cardapios.Remove(cardapio);
            await db.SaveChangesAsync();

            return Ok(true);
        }

        // Busca o restaurante e seu tipo para montar o cabeçalho da tela.
        [HttpGet("cardapio/cliente/{restauranteId}")]
        public async Task<IActionResult> GetCardapioCliente(
            int restauranteId,
            [FromQuery] int usuarioId)
        {
            var restaurante = await db.Restaurantes
                .AsNoTracking() // Evita rastrear dados que serão apenas consultados.
                .Include(r => r.Tipo) // Carrega o tipo do restaurante.
                .FirstOrDefaultAsync(r => r.Id == restauranteId);

            if (restaurante == null)
                return NotFound("Restaurante não encontrado.");

            // Busca os pratos associados ao restaurante.
            var pratos = await db.Cardapios
                .AsNoTracking()
                .Where(c =>
                    c.RestauranteId == restauranteId &&
                    c.PratoId != null)
                .Select(c => new PratoClienteDto
                {
                    Id = c.PratoId!.Value,
                    Nome = c.Prato != null ? c.Prato.Nome : "Sem nome",

                    // Usa a categoria do prato ou uma categoria padrão.
                    Categoria = c.Prato != null &&
                                c.Prato.Categoria != null
                        ? c.Prato.Categoria.Nome
                        : "Geral",

                    // O preço vem da associação entre restaurante e prato.
                    Preco = c.Valor,

                    // Verifica se o cliente já curtiu este prato.
                    IsCurtido = db.ClienteCurtidas.Any(cc =>
                        cc.IdPrato == c.PratoId &&
                        cc.IdCliente == usuarioId)
                })
                .OrderBy(p => p.Categoria) // Ordenação inicial por categoria.
                .ThenBy(p => p.Nome)
                .ToListAsync();

            // Retorna os dados que a tela precisa exibir.
            return Ok(new CardapioClienteResponseDto
            {
                NomeRestaurante = restaurante.Nome,
                TipoRestaurante = restaurante.Tipo != null
                    ? restaurante.Tipo.Nome
                    : "Geral",
                Pratos = pratos
            });
        }

        [HttpPost("cardapio/curtir")]
        public async Task<IActionResult> CurtirPrato(
            [FromBody] CurtirPratoDto dto)
        {
            var curtida = await db.ClienteCurtidas
                .FirstOrDefaultAsync(c =>
                    c.IdPrato == dto.PratoId &&
                    c.IdCliente == dto.ClienteId);

            if (dto.IsCurtido && curtida == null)
            {
                db.ClienteCurtidas.Add(new ClienteCurtida
                {
                    IdPrato = dto.PratoId,
                    IdCliente = dto.ClienteId
                });
            }
            else if (!dto.IsCurtido && curtida != null)
            {
                db.ClienteCurtidas.Remove(curtida);
            }

            await db.SaveChangesAsync();

            return Ok(true);
        }

        // Retorna os pratos ordenados por categoria e nome.
        [HttpGet("pratos")]
        public async Task<IActionResult> GetPratos()
        {
            var pratos = await db.Pratos
                .AsNoTracking()
                .Select(p => new PratoConsultaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Ingredientes = p.Ingredientes,

                    // Obtém o nome da categoria do prato.
                    CategoriaNome = p.Categoria != null
                        ? p.Categoria.Nome
                        : "Geral",

                    // Conta quantas curtidas o prato recebeu.
                    TotalCurtidas = db.ClienteCurtidas
                        .Count(c => c.IdPrato == p.Id)
                })
                .OrderBy(p => p.CategoriaNome)
                .ThenBy(p => p.Nome)
                .ToListAsync();

            return Ok(pratos);
        }

        // GET api/categorias
        // Busca as categorias cadastradas para preencher o Picker do Mobile.
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await db.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // POST api/pratos
        // Cadastra um novo prato no banco de dados.
        [HttpPost("pratos")]
        public async Task<IActionResult> CriarPrato(
            [FromBody] PratoCriacaoDto dto)
        {
            // Valida os campos obrigatórios.
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Nome) ||
                string.IsNullOrWhiteSpace(dto.Descricao) ||
                string.IsNullOrWhiteSpace(dto.Ingredientes) ||
                dto.TempoPreparo <= 0 ||
                dto.CategoriaId <= 0)
            {
                return BadRequest(
                    "Preencha todos os campos obrigatórios corretamente.");
            }

            // Verifica se a categoria selecionada existe.
            var categoriaExiste = await db.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId);

            if (!categoriaExiste)
            {
                return BadRequest("A categoria selecionada não existe.");
            }

            // Cria o prato sem armazenar uma imagem no banco.
            var prato = new Prato
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao.Trim(),
                Ingredientes = dto.Ingredientes.Trim(),
                TempoPreparo = dto.TempoPreparo,
                CategoriaId = dto.CategoriaId
            };

            // Adiciona o prato e persiste as alterações.
            db.Pratos.Add(prato);

            await db.SaveChangesAsync();

            return Ok(true);
        }

        // GET api/restaurantes/ranking
        // Retorna os três restaurantes com mais curtidas nos pratos.
        [HttpGet("restaurantes/ranking")]
        public async Task<IActionResult> GetRankingRestaurantes()
        {
            // 1. Busca os dados base sem agregações complexas para evitar erro 500 do EF Core
            var restaurantes = await db.Restaurantes
                .Include(r => r.Cidade)
                .Where(r => r.DeletedAt == null)
                .ToListAsync();

            var cardapios = await db.Cardapios
                .Include(c => c.Prato)
                .ToListAsync();

            var curtidas = await db.ClienteCurtidas.ToListAsync();

            // 2. Processa o ranking em memória (LINQ to Objects)
            var ranking = restaurantes.Select(r =>
            {
                var pratosRestaurante = cardapios
                    .Where(c => c.RestauranteId == r.Id && c.PratoId != null)
                    .ToList();

                var pratosRanking = pratosRestaurante.Select(c => new PratoRankingDto
                {
                    Nome = c.Prato != null ? c.Prato.Nome : "Prato sem nome",
                    Curtidas = curtidas.Count(cc => cc.IdPrato == c.PratoId)
                }).ToList();

                return new RestauranteRankingDto
                {
                    Id = r.Id,
                    Nome = r.Nome,
                    Descricao = r.Descricao,
                    Endereco = r.Cidade != null ? r.Cidade.Nome : "Endereço não informado",
                    TotalCurtidas = pratosRanking.Sum(p => p.Curtidas),
                    Pratos = pratosRanking
                        .OrderByDescending(p => p.Curtidas)
                        .ThenBy(p => p.Nome)
                        .ToList()
                };
            })
            .OrderByDescending(r => r.TotalCurtidas)
            .ThenBy(r => r.Nome)
            .Take(3)
            .ToList();

            return Ok(ranking);
        }

        // DTOs da resposta do ranking.
        public class RestauranteRankingDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Descricao { get; set; } = string.Empty;
            public string Endereco { get; set; } = string.Empty;
            public int TotalCurtidas { get; set; }
            public List<PratoRankingDto> Pratos { get; set; } = new();
        }

        public class PratoRankingDto
        {
            public string Nome { get; set; } = string.Empty;
            public int Curtidas { get; set; }
        }
    }




    public class LoginRequestDto()
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class RestaurantesResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Cidade { get; set; }
        public string Tipo { get; set; }
        public int Avaliacao { get; set; }
        public bool Excluido { get; set; }
    }


    public class PratoDto
    {
        public int? Id { get; set; }
    }

    public class CardapioDto
    {
        public List<PratoDto> Disponiveis { get; set; } = new();
        public List<PratoDto> Menu { get; set; } = new();
    }

    public class CardapioRequestDto
    {
        public int RestauranteId { get; set; }
        public int PratoId { get; set; }
        public double? Valor { get; set; }
    }

    public class PratoClienteDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = "Geral";
        public double? Preco { get; set; }
        public bool IsCurtido { get; set; }
    }

    public class CardapioClienteResponseDto
    {
        public string NomeRestaurante { get; set; } = string.Empty;
        public string TipoRestaurante { get; set; } = string.Empty;
        public List<PratoClienteDto> Pratos { get; set; } = new();
    }

    public class CurtirPratoDto
    {
        public int PratoId { get; set; }
        public int ClienteId { get; set; }
        public bool IsCurtido { get; set; }
    }


    public class PratoConsultaDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Ingredientes { get; set; } = string.Empty;

        public string CategoriaNome { get; set; } = "Geral";

        public int TotalCurtidas { get; set; }
    }

    public class CategoriaDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
    }

    public class PratoCriacaoDto
    {
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Ingredientes { get; set; } = string.Empty;

        public int TempoPreparo { get; set; }

        public int CategoriaId { get; set; }
    }

    public class PratoRankingDto
    {
        public string Nome { get; set; } = string.Empty;

        public int Curtidas { get; set; }
    }
}
