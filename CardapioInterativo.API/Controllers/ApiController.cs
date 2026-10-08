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
    }


    public class LoginRequestDto()
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
