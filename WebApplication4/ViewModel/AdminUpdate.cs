using WebApplication4.Models;

namespace WebApplication4.ViewModel
{
    public class AdminUpdate
    {



        public ServiceRequestAddress Address { get; set; }

        public int ServiceRequestId { get; set; }

        public string Date { get; set; }


        //public DateTime ServiceStartDate { get; set; }


        public string StartTime { get; set; }


        public string WhyReschedule { get; set; }

        public string CallCenterNote { get; set; }
    }
}


