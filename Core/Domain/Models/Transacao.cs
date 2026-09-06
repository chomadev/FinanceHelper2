using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace FinanceHelper.Core.Domain.Models
{
    [Table("transacoes")]
    public class Transacao
    {
        [Key]
        [Ignore]
        public Guid Id { get; set; }

        [Name("Data")]
        [Format("dd/MM/yyyy")]
        [Column("data")]
        public DateOnly Data { get; set; }

        [Name("Valor")]
        [Column("valor")]
        public decimal Valor { get; set; }

        [Name("Identificador")]
        [Column("identificador")]
        public string Identificador { get; set; } = string.Empty;

        [Name("Descrição")]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Ignore]
        [Column("categoria_sugerida")]
        public string CategoriaSugerida { get; set; } = string.Empty;

        [Ignore]
        [Column("is_recorrente")]
        public bool IsRecorrente { get; set; }
    }
} 