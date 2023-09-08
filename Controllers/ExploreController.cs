using Instagram_clone.Context;
using Instagram_clone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Instagram_clone.Controllers
{
    public class ExploreController : Controller
    {
        private readonly ILogger<ExploreController> _logger;
        private readonly ApplicationDBContext _context; // Add this field

        public ExploreController(ILogger<ExploreController> logger, ApplicationDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            // Define the range of user IDs
            int minUserId = 0;
            int maxUserId = 45;

            var posts = _context.post
                .Where(post => post.UserId >= minUserId && post.UserId <= maxUserId)
                .Select(post => new Post
                {
                    id = post.id,
                    title = post.title,
                    description = post.description,
                    UserId = post.UserId,
                    content = post.content,
                    imageurl = post.imageurl,
                    UserName = post.User.username,  // Assuming there is a navigation property 'User' on the Post entity
                    user_profile_pic = post.User.profile_pic  // Assuming there is a navigation property 'User' on the Post entity
                })
                .ToList();

            return View(posts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}