using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Interfaces;

namespace App.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public string TagsString
        {
            get
            {
                if (Tags == null || Tags.Count == 0)
                    return "None";

                StringBuilder sb = new StringBuilder();

                foreach (Tag tag in Tags)
                {
                    sb.Append($"{tag.Content}, ");
                }

                // sb.Remove(sb.Length - 1, -2);

                sb.Length = sb.Length - 2;

                sb.Append(".");

                return sb.ToString();
            }
        }
    }
}
