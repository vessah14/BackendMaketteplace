using Microsoft.AspNetCore.Mvc;
using Backend_Mboagreen.Models;
using Backend_Mboagreen.Services;
using System.Text.Json;

namespace Backend_Mboagreen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaiementController : ControllerBase
    {
        private readonly CamerPayService _camerpay;

        public PaiementController(CamerPayService camerpay)
        {
            _camerpay = camerpay;
        }

        [HttpPost("initier")]
        public async Task<IActionResult> Initier([FromBody] PaiementRequest request)
        {
            var simulatorBaseUrl = $"{Request.Scheme}://{Request.Host}/api/paiement/simulateur";
            var result = await _camerpay.InitierPaiement(request, simulatorBaseUrl);
            return Ok(result);
        }

        [HttpGet("statut/{uuid}")]
        public async Task<IActionResult> Statut(string uuid)
        {
            var result = await _camerpay.VerifierPaiement(uuid);
            return Ok(result);
        }

        [HttpPost("callback")]
        public IActionResult Callback([FromBody] JsonElement payload)
        {
            Console.WriteLine("Callback reçu : " + payload.ToString());
            return Ok();
        }

        [HttpGet("simulateur/{uuid}")]
        public IActionResult Simulateur(string uuid)
        {
            if (!_camerpay.PaiementSimuleExiste(uuid))
            {
                return NotFound();
            }

            var page = $"""
                <!doctype html>
                <html lang="fr">
                <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Simulation de paiement</title></head>
                <body style="font-family: sans-serif; max-width: 32rem; margin: 4rem auto; padding: 0 1rem;">
                  <h1>Simulateur de paiement</h1>
                  <p>Choisissez le résultat de ce paiement de test.</p>
                  <form method="post" action="/api/paiement/simulateur/{uuid}/resultat">
                    <button name="status" value="SUCCESS" type="submit">Simuler un paiement réussi</button>
                    <button name="status" value="FAILED" type="submit">Simuler un échec</button>
                  </form>
                </body>
                </html>
                """;

            return Content(page, "text/html; charset=utf-8");
        }

        [HttpPost("simulateur/{uuid}/resultat")]
        public IActionResult ResultatSimulateur(string uuid, [FromForm] string status)
        {
            if (!_camerpay.FinaliserPaiementSimule(uuid, status))
            {
                return NotFound();
            }

            var result = status == "SUCCESS" ? "Paiement simulé avec succès." : "Échec de paiement simulé.";
            return Content($"<!doctype html><html lang=\"fr\"><meta charset=\"utf-8\"><title>Résultat</title><body style=\"font-family:sans-serif;max-width:32rem;margin:4rem auto;padding:0 1rem\"><h1>{result}</h1><p>Vous pouvez fermer cette page et revenir à la boutique.</p></body></html>", "text/html; charset=utf-8");
        }
    }
}