namespace FloSample
{
    public class RequestPayment
    {
        public int MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CardNumber { get; set; }
        public string Cardholder { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public bool Is3ds { get; set; }
        public bool AttemptN3d { get; set; }
        public bool Capture { get; set; }
        public string RecipientName { get; set; }
    }
}