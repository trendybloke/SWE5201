using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Data;

namespace App.Models
{
    internal class EventBooking
    {
        public int Id { get; set; }
        public int HostedEventId { get; set; }
        public HostedEvent HostedEvent { get; set; }
        public string UserId { get; set; }
        // public UserSummary User { get; set; }
        public bool Attended { get; set; }
    }
}
