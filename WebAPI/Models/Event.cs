using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Interfaces;

namespace WebAPI.Models
{
    public class Event : ITagged
    {
        // public static int NextId = 1;
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Tag>? Tags { get; set; } = new List<Tag>();

        // CONSTRUCTOR BREAKS CONTROLLER DEF.
        //public Event(int Id, string Title, string Description, ICollection<Tag> Tags)
        //{
        //    this.Id = Id;
        ////    NextId++;
        //    this.Title = Title;
        //    this.Description = Description;
        //    this.Tags = Tags;
        //}
    }
}
