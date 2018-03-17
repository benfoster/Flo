namespace FloSample
{
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseSummary { get; set; }
        public string AcquirerResponseCode { get; set; }
        public string AuthCode { get; set; }
        public bool Approved { get; set; }
    }
}