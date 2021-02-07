using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Confluent.Kafka;
using APIAcoes.Data;
using APIAcoes.Models;

namespace APIAcoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcoesController : ControllerBase
    {
        private readonly ILogger<AcoesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly TelemetryConfiguration _telemetryConfig;
        private readonly AcoesRepository _repository;

        public AcoesController(ILogger<AcoesController> logger,
            IConfiguration configuration,
            TelemetryConfiguration telemetryConfig,
            AcoesRepository repository)
        {
            _logger = logger;
            _configuration = configuration;
            _telemetryConfig = telemetryConfig;
            _repository = repository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Resultado), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<Resultado> Post(Acao acao)
        {
            var inicio = DateTime.Now;
            var watch = new Stopwatch();
            watch.Start();

            var conteudoAcao = JsonSerializer.Serialize(acao,
                new () { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            _logger.LogInformation($"Dados: {conteudoAcao}");

            string topic = _configuration["ApacheKafka:Topic"];
            var configKafka = new ProducerConfig
            {
                BootstrapServers = _configuration["ApacheKafka:Broker"]
            };

            using (var producer = new ProducerBuilder<Null, string>(configKafka).Build())
            {
                var result = await producer.ProduceAsync(
                    topic,
                    new Message<Null, string>
                    { Value = conteudoAcao });

                _logger.LogInformation(
                    $"Apache Kafka - Envio para o tópico {topic} concluído | " +
                    $"{conteudoAcao} | Status: { result.Status.ToString()}");
            }

            watch.Stop();
            TelemetryClient client = new (_telemetryConfig);
            client.TrackDependency(
                "Kafka", $"Produce {topic}", conteudoAcao, inicio, watch.Elapsed, true);

            return new Resultado()
            {
                Mensagem = "Informações de ação enviadas com sucesso!"
            };
        }        

        [HttpGet]
        public ActionResult<HistoricoAcao[]> GetAll()
        {
            var dados = _repository.GetAll();
            _logger.LogInformation($"GetAll - encontrado(s) {dados.Count()} registro(s)");
            return dados.ToArray();
        }
    }
}