namespace ViralContentApi.DTOs
{
    public class BillingProviderNotReadyException : Exception
    {
        public BillingProviderNotReadyException(string message) : base(message)
        {
        }
    }
}