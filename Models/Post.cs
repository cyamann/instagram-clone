using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Instagram_clone.Models
{
    public class Post
    {
        [Key]
        public int id { get; set; } 

        public string title { get; set; }

        public string description { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        public User User { get; set; }

        public string content { get; set; }

        public string imageurl { get; set; }

        public string UserName { get; set; }

        public string user_profile_pic { get; set; }
    }
}
