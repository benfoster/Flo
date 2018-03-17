using FloSample;

public class RiskContext
{
    public int MerchantId { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public string Cardholder { get; set; }
    public string CardNumber { get; set; }
    public string MerchantCountry { get; set; }
    public string CustomerCountry { get; set; }
    public RiskAssessment Result { get; set; } = new RiskAssessment();
}