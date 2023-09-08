using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Instagram_clone.Models
{
    public class User
    {
        [Key]
        public int userId { get; set; } 
        public string username { get; set; } 
        public string password { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public long phone { get; set; }
        public string gender { get; set; }
        public ICollection<Post> Posts { get; set; }
        public string profile_pic { get; set; }
        public int[] follows { get; set; }
        public List<Post> FollowedPosts { get; set; } // List of posts that the user follows
    }
}
