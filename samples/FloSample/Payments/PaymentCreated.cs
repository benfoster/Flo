using System;

namespace FloSample
{
    public class PaymentCreated
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseSummary { get; set; }
        public bool Approved { get; set; }
    }
}