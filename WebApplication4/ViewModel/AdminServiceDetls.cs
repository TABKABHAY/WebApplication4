using WebApplication4.Models;


namespace WebApplication4.ViewModel
{
    public class AdminServiceDetls
    {

        public int? ServiceRequestId { get; set; }

        public string? ZipCode { get; set; }

        public string? Email { get; set; }

        public string? CustomerName { get; set; }

        public string? ServiceProviderName { get; set; }

        public int? Status { get; set; }


        public string? FromDate { get; set; }

        public string? ToDate { get; set; }
    }
}
