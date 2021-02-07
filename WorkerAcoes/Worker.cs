using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Confluent.Kafka;
using WorkerAcoes.Data;
using WorkerAcoes.Models;
using WorkerAcoes.Validators;

namespace WorkerAcoes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly AcoesRepository _repository;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly TelemetryConfiguration _telemetryConfig;


        public Worker(ILogger<Worker> logger, IConfiguration configuration,
            AcoesRepository repository,
            TelemetryConfiguration telemetryConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
            _telemetryConfig = telemetryConfig;

            _consumer = new ConsumerBuilder<Ignore, string>(
                new ConsumerConfig()
                {
                    BootstrapServers = _configuration["ApacheKafka:Broker"],
                    GroupId = _configuration["ApacheKafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string topico = _configuration["ApacheKafka:Topic"];

            _logger.LogInformation($"Topic = {topico}");
            _logger.LogInformation($"Group Id = {_configuration["ApacheKafka:GroupId"]}");
            _logger.LogInformation("Aguardando mensagens...");
            _consumer.Subscribe(topico);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Run(() =>
                {
                    var result = _consumer.Consume(stoppingToken);

                    var inicio = DateTime.Now;
                    var watch = new Stopwatch();
                    watch.Start();

                    var dadosAcao = result.Message.Value;

                    watch.Stop();
                    TelemetryClient client = new (_telemetryConfig);
                    client.TrackDependency(
                        "Kafka", $"Consume {topico}", dadosAcao, inicio, watch.Elapsed, true);

                    _logger.LogInformation(
                        $"[{_configuration["ApacheKafka:GroupId"]} | Nova mensagem] " +
                        dadosAcao);
                    ProcessarAcao(dadosAcao);
                });
           }
        }

        private void ProcessarAcao(string dados)
        {
            Acao acao;            
            try
            {
                acao = JsonSerializer.Deserialize<Acao>(dados,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                acao = null;
            }

            if (acao is not null &&
                new AcaoValidator().Validate(acao).IsValid)
            {
                _repository.Save(acao);
                _logger.LogInformation("Ação registrada com sucesso!");
            }
            else
            {
                _logger.LogError("Dados inválidos para a Ação");
            } 
        }
    }
}