namespace CyberSourceIntegration.Models
{
    public class PaymentRequestPayload
    {
        public ClientReferenceInformation ClientReferenceInformation { get; set; } = new ClientReferenceInformation();
        public ProcessingInformation ProcessingInformation { get; set; } = new ProcessingInformation();
        public PaymentInformation PaymentInformation { get; set; } = new PaymentInformation();
    }

}
