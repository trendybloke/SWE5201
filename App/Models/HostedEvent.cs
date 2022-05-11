using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Models
{
    public class HostedEvent
    {
        public static int NextId = 1;
        public int Id { get; set; }
        public Event Event { get; set; }
        public Room Room { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int DurationHours { get; set; }
        public int DurationDays { get; set; }
        public float EntranceFee { get; set; }
        public string DurationString
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                // x hour(s) and x minute(s), over x day(s)
                if (DurationHours > 0)
                {
                    stringBuilder.Append($"{DurationHours} hour(s)");
                    if (DurationMinutes > 0)
                        stringBuilder.Append($" and {DurationMinutes} minute(s)");
                }
                else
                {
                    if (DurationMinutes > 0)
                        stringBuilder.Append($"{DurationMinutes} minute(s)");
                }

                if (DurationDays > 0)
                    stringBuilder.Append($", over {DurationDays} day(s).");
                else
                    stringBuilder.Append(".");

                return stringBuilder.ToString();
            }
        }
        public DateTime EndTime
        {
            get
            {
                return StartTime.AddMinutes(DurationMinutes)
                        .AddHours(DurationHours)
                        .AddDays(DurationDays);
            }
        }

        public string StartEndString
        {
            get
            {
                return $"Start: {StartTime}, End: {EndTime}";
            }
        }

        /*
        public HostedEvent(Event Event, string Room, DateTime StartTime,
                            int Minutes, int Hours, int Days, float Fee)
        {
            this.Id = NextId;
            NextId++;

            this.Event = Event;
            this.Room = Room;
            this.StartTime = StartTime;
            this.DurationMinutes = Minutes;
            this.DurationHours = Hours;
            this.DurationDays = Days;
            this.EntranceFee = Fee;
        }
        */
    }
}
