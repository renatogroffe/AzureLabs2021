using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Npgsql;
using Dapper.Contrib.Extensions;
using WorkerAcoes.Models;

namespace WorkerAcoes.Data
{
    public class AcoesRepository
    {
        private readonly IConfiguration _configuration;
        private readonly TelemetryConfiguration _telemetryConfig;

        public AcoesRepository(IConfiguration configuration,
            TelemetryConfiguration telemetryConfig)
        {
            _configuration = configuration;
            _telemetryConfig = telemetryConfig;
        }

        public void Save(Acao acao)
        {
            var inicio = DateTime.Now;
            var watch = new Stopwatch();
            watch.Start();

            var conexao = new NpgsqlConnection(
                _configuration.GetConnectionString("BaseAcoes"));

            var historico = new HistoricoAcao()
            {
                Codigo = acao.Codigo,
                CodReferencia = $"{acao.Codigo}{DateTime.Now:yyyyMMddHHmmss}",
                DataReferencia = DateTime.Now,
                Valor = acao.Valor
            };
            conexao.Insert<HistoricoAcao>(historico);

            watch.Stop();
            TelemetryClient client = new (_telemetryConfig);
            client.TrackDependency(
                "PostgreSQL", "INSERT HistoricoAcoes",
                JsonSerializer.Serialize(historico),
                inicio, watch.Elapsed, true);
        }
    }
}