namespace CSV2.Models
{
    public class CsvRecord
    {
      
            public string transaction_uti { get; set; }
            public string isin { get; set; }
            public double notional { get; set; }
            public CurrencyType notional_currency { get; set; }
            public TransactionType transaction_type { get; set; }
            public string transaction_datetime { get; set; }
            public double rate { get; set; }
            public string lei { get; set; }
            public string? legalName { get; set; } = null;
            public string? bic { get; set; } = null;
            public double? transaction_costs { get; set; } = null;
           
    }

    public enum CurrencyType
    {
        None ,
        EUR,
        GBP
    }

    public enum TransactionType
    {
        Sell,
        Buy
    }
}
