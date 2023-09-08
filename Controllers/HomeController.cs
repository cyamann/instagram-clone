using Instagram_clone.Context;
using Instagram_clone.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace Instagram_clone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string first_name, string last_name)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(u =>
                u.username == user.username || u.email == user.email);

            if (existingUser == null)
            {
                first_name = user.first_name;
                // Create a new user entity with the provided data
                User newUser = new User
                {
                    username = user.username,
                    password = HashPassword(user.password),
                    first_name = first_name, // Kullanıcıdan alınan adı atayın
                    last_name = last_name,   // Kullanıcıdan alınan soyadı atayın
                    email = user.email,
                    phone = user.phone,
                    gender = user.gender,
                    profile_pic = user.profile_pic
                };

                _context.users.Add(newUser);
                await _context.SaveChangesAsync();
                await AuthenticateUserAsync(user.username);
                NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;port=5432;Database=Instagram;User Id=postgres;Password=123");
                conn.Open();

                // Assuming you have already executed the INSERT query for the user
                // and you want to get the userId of the inserted user
                NpgsqlCommand getUserIdCmd = new NpgsqlCommand("SELECT \"userId\" FROM users WHERE username = @Username", conn);
                getUserIdCmd.Parameters.AddWithValue("@Username", user.username);
                int? insertedUserId = (int?)getUserIdCmd.ExecuteScalar();

                conn.Close();

                if (insertedUserId.HasValue)
                {
                    // Redirect to the "ForYou" action with the user ID
                    return RedirectToAction("RegisterSuccess", "Home");
                }
                else
                {
                    // Handle the case where the user ID was not found
                    return RedirectToAction("Index", "Home"); // Redirect to another page, e.g., home
                }


            }
            return RedirectToAction("Index", "Home"); // Redirect to another page, e.g., home

        }

        // Helper method to authenticate the user
        private async Task AuthenticateUserAsync(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                // Add other claims if needed (e.g., roles)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
        // Helper method to hash passwords (replace with a secure password hashing library)
        private string HashPassword(string password)
        {
            // Implement a secure password hashing method here
            // Use a secure password hashing library like BCrypt or Argon2
            // For simplicity, we are not hashing the password in this example, which is not secure
            return password;
        }
        // Helper method to verify passwords (replace with a secure password hashing library)
        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            // Implement a secure password verification method here
            // Compare the hashedPassword with the hash of enteredPassword
            // You should use a secure password hashing library like BCrypt or Argon2
            // For simplicity, we are using a basic equality check here, which is not secure
            return enteredPassword == hashedPassword;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {

            var user = await _context.users
            .Where(u => u.email == email && u.follows != null) // Check for non-null 'follows'
            .FirstOrDefaultAsync(); 

                if (user != null && VerifyPassword(password, user.password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.email),
                new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()), // Convert user.userId to string
                // Add other claims as needed, e.g., roles, additional user data, etc.
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Check if 'follows' is null and handle it
                    if (user.follows == null)
                    {
                        // Treat it as if there are no followings for the user
                        // You can add custom logic here if needed
                        return RedirectToAction("ForYou", "Home");
                    }

                    // Handle the case where there are followings for the user
                    // Add your custom logic for handling followings here
                    // Redirect to the appropriate action based on followings

                    return RedirectToAction("ForYou", "Home");
                }


            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Sign out the user
            return RedirectToAction("Index", "Home"); // Redirect to the home page
        }
        [Authorize] // Ensure that the user is authenticated
        public IActionResult RegisterSuccess()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
            {
                string userId = userIdClaim.Value;

                // Your custom logic for handling the redirection after successful registration
                // For example, you can retrieve user-specific data here
                // var userData = YourCustomDataRetrieval(userId);

                // Redirect to the "ForYou" action with the user ID
                return RedirectToAction("ForYou", "Home", new { id = userId });
            }

            // If the user is not authenticated, redirect to the "Index" action (or another appropriate action).
            return RedirectToAction("Index", "Home"); // Redirect to the "Index" action
        }




        [Authorize]
        public IActionResult ForYou()
        {
            // 1. Retrieve the User ID of the currently logged-in user from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                // Handle the case where the User ID is not found
                return NotFound();
            }

            // Extract the user ID from the claim
            string userId = userIdClaim.Value;

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;port=5432;Database=Instagram;User Id=postgres;Password=123");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("select following_id from followers", conn);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            List<int[]> myArrayList = new List<int[]>();

            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    int[] values = reader.GetFieldValue<int[]>(0);
                    myArrayList.Add(values);
                }
                // do your bidding
            }
            conn.Close();
            NpgsqlConnection conn2 = new NpgsqlConnection("Server=localhost;port=5432;Database=Instagram;User Id=postgres;Password=123");
            conn2.Open();
            NpgsqlCommand cmd2 = new NpgsqlCommand("select follower_id from followers", conn2);
            NpgsqlDataReader reader2 = cmd2.ExecuteReader();

            int[] myArray = new int[60];
            int counter = 0;
            while (reader2.Read())
            {
                if (!reader2.IsDBNull(0))
                {
                    myArray[counter] = reader2.GetFieldValue<int>(0); // Türü belirtin (int)
                    counter++;
                }
                // do your bidding
            }
            reader.Close();
            reader2.Close();
            conn2.Close();

            int[] valuess = new int[60];
            for (int i = 0; i < myArrayList.Count; i++)
            {
                if(i.Equals(int.Parse( userId)))
                {
                     valuess = myArrayList[i];
                }
            }
            HashSet<int> valuesHashSet = new HashSet<int>(valuess);

            var followedPosts = _context.post
                .Where(p => valuesHashSet.Contains(p.UserId))
                .ToList();


            // Pass the followed posts as the model to the view
            return View("ForYou",followedPosts);
        }



        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
