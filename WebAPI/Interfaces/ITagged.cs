using System.Text;
using WebAPI.Models;

namespace WebAPI.Interfaces
{
    public interface ITagged
    {
        public ICollection<Tag>? Tags { get; set; }

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

                //sb.Remove(sb.Length - 1, -2);

                sb.Length = sb.Length - 2;

                sb.Append(".");

                return sb.ToString();
            }
        }
    }
}
