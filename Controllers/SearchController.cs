using System;
using System.Collections.Generic;
using System.Linq;
using Instagram_clone.Context;
using Instagram_clone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Instagram_clone.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDBContext _context;

        public SearchController(ApplicationDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    ViewBag.ErrorMessage = "Please enter a valid username.";
                    return View();
                }

                var userPosts = _context.users
                    .Join(
                        _context.post,
                        u => u.userId,
                        p => p.UserId,
                        (u, p) => new
                        {
                            Username = u.username,
                            PostTitle = p.title,
                            PostDescription = p.description,
                            PostImage = p.imageurl,
                            // Add other post properties you want to fetch here
                        }
                    )
                    .Where(up => up.Username == username)
                    .Select(up => new Post
                    {
                        title = up.PostTitle,
                        description = up.PostDescription,
                        imageurl = up.PostImage
                    })
                    .ToList();

                if (userPosts.Any())
                {
                    ViewBag.Username = username;
                    return View("Index", userPosts);
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                ViewBag.ErrorMessage = "An error occurred while processing your request.";
            }

            ViewBag.ErrorMessage = "User not found or has no posts.";
            return View();
        }

    }
}
