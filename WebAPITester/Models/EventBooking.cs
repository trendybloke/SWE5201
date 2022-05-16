using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPITester.Models
{
    public class EventBooking
    {
        public int Id { get; set; }
        public int? HostedEventId { get; set; }
        public HostedEvent? HostedEvent { get; set; }
        public string UserId { get; set; }
        // public ApplicationUser? User { get; set; }
        public bool Attended { get; set; }
    }
}
