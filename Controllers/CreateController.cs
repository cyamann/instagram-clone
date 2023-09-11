using Instagram_clone.Context;
using Instagram_clone.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Instagram_clone.Controllers
{
    public class CreateController : Controller
    {
        private readonly ApplicationDBContext _context;

        public CreateController(ApplicationDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Post post, IFormFile Image)
        {
            try
            {

                // Get the UserId of the currently logged-in user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfilePicPath = _context.users
                    .Where(u => u.userId == int.Parse(userId)) // Assuming userId is the correct property name
                    .Select(u => u.profile_pic)
                    .FirstOrDefault(); // Use FirstOrDefault to get the first matching user's profile picture path

                if (userProfilePicPath != null)
                {
                    // userProfilePicPath now contains the user's profile picture path
                    post.user_profile_pic = userProfilePicPath; // Set the user's profile picture path in your post object
                }

                // Set the UserId for the post
                post.UserId = int.Parse(userId);

                // Set other properties based on the logged-in user's information
                // Example:
                var username = _context.users
                    .Where(u => u.userId == int.Parse(userId)) // Assuming userId is the correct property name
                    .Select(u => u.username)
                    .FirstOrDefault();
               
                post.UserName = username;

                post.imageurl = Image.FileName;


                // Add the post to the database and save changes
                _context.post.Add(post);
                    await _context.SaveChangesAsync();
                TempData["Message"] = "Image uploaded successfully!";

                return View(post); // Redirect to the list of posts or home page
                
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, "An error occurred while saving the post.");
            }

            return View(post);
        }

    }
}
