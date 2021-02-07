using System;

namespace APIAcoes.Models
{
    public class HistoricoAcao
    {
        public int Id { get; set; }
        public string CodReferencia { get; set; }
        public string Codigo { get; set; }
        public DateTime? DataReferencia { get; set; }
        public decimal? Valor { get; set; }
    }
}