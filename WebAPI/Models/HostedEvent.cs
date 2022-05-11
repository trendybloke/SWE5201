using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class HostedEvent
    {
        // public static int NextId = 1;
        public int Id { get; set; }
        public int? _EventId { get; set; }
        public Event? Event { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
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

        //public HostedEvent(int Id, Event Event, string Room, DateTime StartTime,
        //                    int Minutes, int Hours, int Days, float Fee)
        //{
        //    this.Id = Id;
        //    // NextId++;

        //    this.Event = Event;
        //    this.EventId = Event.Id;
        //    this.Room = Room;
        //    this.StartTime = StartTime;
        //    DurationMinutes = Minutes;
        //    DurationHours = Hours;
        //    DurationDays = Days;
        //    EntranceFee = Fee;
        //}

        //public HostedEvent(int Id, int EventId, string Room, DateTime StartTime,
        //            int Minutes, int Hours, int Days, float Fee)
        //{
        //    this.Id = Id;
        //    // NextId++;

        //    this.EventId = EventId;
        //    this.Event = null;
        //    this.Room = Room;
        //    this.StartTime = StartTime;
        //    DurationMinutes = Minutes;
        //    DurationHours = Hours;
        //    DurationDays = Days;
        //    EntranceFee = Fee;
        //}
    }
}
