using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using APIAcoes.Models;

namespace APIAcoes.Data
{
    public class AcoesRepository
    {
        private readonly IConfiguration _configuration;
        private readonly AcoesContext _context;
        private readonly TelemetryConfiguration _telemetryConfig;

        public AcoesRepository(
            IConfiguration configuration,
            AcoesContext context,
            TelemetryConfiguration telemetryConfig)
        {
            _configuration = configuration;
            _context = context;
            _telemetryConfig = telemetryConfig;
        }

        public HistoricoAcao[] GetAll()
        {
            var inicio = DateTime.Now;
            var watch = new Stopwatch();
            watch.Start();

            var dados = _context.HistoricoAcoes
                .OrderByDescending(h => h.Id).ToArray();

            watch.Stop();
            TelemetryClient client = new (_telemetryConfig);
            client.TrackDependency(
                "PostgreSQL", "SELECT HistoricoAcoes",
                $"{dados.Count()} registro(s) encontrado(s)",
                inicio, watch.Elapsed, true);

            return dados;
        }
    }
}