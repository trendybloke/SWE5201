using System.Collections.Generic;

namespace WebAPITester.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public virtual ICollection<Event>? Events { get; set; } = new List<Event>();

        //public Tag(int Id, string Content)
        //{
        //    this.Id = Id;
        //    this.Content = Content;
        //}
    }
}
