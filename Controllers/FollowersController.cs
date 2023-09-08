using Instagram_clone.Context;
using Instagram_clone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace Instagram_clone.Controllers
{
    [Authorize] // Requires authentication for these actions
    public class FollowersController : Controller
    {
        private readonly ApplicationDBContext _context;

        public FollowersController(ApplicationDBContext context)
        {
            _context = context;
        }
    }
}
