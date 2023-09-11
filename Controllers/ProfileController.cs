using Instagram_clone.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Instagram_clone.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ProfileController(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);



            // Retrieve the user's posts
            var userPosts = await _context.post
                .Where(p => p.UserId == int.Parse(userId)) // Replace UserId with your actual foreign key property
                .ToListAsync();

            // Check if there are any user posts
            if (userPosts.Any())
            {
                // Pass the userPosts to the view
                return View(userPosts);
            }
            else
            {
                // Handle the case where there are no posts
                return View("NoPosts"); // Create a view named "NoPosts" for this case
            }
        }
    }
}
