using app.Services;
using Microsoft.AspNetCore.Authorization;
using Entidades;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using api;

namespace app.Controllers
{
    [ApiController]
    [Route("api")]
    public class UpsController : ControllerBase
    {
        private readonly IUpsService upsService;
        private readonly AuthService authService;

        public UpsController(IUpsService upsService, AuthService authService)
        {
            this.upsService = upsService;
            this.authService = authService;
        }

        [HttpPost("calcular/ups/sinistros")]
        [Authorize]
        public async Task<IActionResult> CalcularUpsSinistrosAsync()
        {
            authService.Require(User, Permissao.UpsCalcularSinistro);
            await upsService.CalcularUpsEmMassaAsync();
            return Ok();
        }

        [HttpGet("calcular/ups/escola")]
        [Authorize]
        public async Task<IActionResult> CalcularUpsEscolaAsync([FromQuery] Escola escola, [FromQuery] double? raioKm)
        {
            authService.Require(User, Permissao.UpsCalcularEscola);
            double raio = 2.0;
            if (raioKm != null)
                raio = (double) raioKm;
            var upsDetalhado = await upsService.CalcularUpsEscolaAsync(escola, raio);
            return new OkObjectResult(upsDetalhado);
        }
    }
}
