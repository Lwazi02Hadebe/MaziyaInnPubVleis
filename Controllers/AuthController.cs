using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Models;
using MaziyaInnPubVleis.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Diagnostics;

namespace MaziyaInnPubVleis.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Role Selection Page (Homepage)
        public IActionResult Index()
        {
            // Clear any existing sessions to prevent conflicts
            HttpContext.Session.Clear();

            _logger.LogInformation("Accessed role selection page");

            // Check if there's a success message from logout
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View();
        }

        // GET: Admin Login Page
        [HttpGet]
        public IActionResult AdminLogin()
        {
            // If already logged in as admin, redirect to admin dashboard
            if (HttpContext.Session.GetInt32("AdminUserId") != null)
            {
                _logger.LogInformation("Admin already logged in, redirecting to dashboard");
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation("Accessed admin login page");
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string username, string password, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("Admin/Manager login attempt for username: {Username}", username);

                // Basic validation
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    TempData["ErrorMessage"] = "Please enter both username and password.";
                    return View();
                }

                // Find user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if (user != null)
                {
                    // Verify password
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                    if (isPasswordValid)
                    {
                        // Check if user has admin or manager privileges
                        if (user.Role == UserRole.Admin || user.Role == UserRole.Manager || user.Role == UserRole.Cashier)
                        {
                            // Set admin session with all necessary data
                            HttpContext.Session.SetInt32("AdminUserId", user.UserId);
                            HttpContext.Session.SetString("AdminUsername", user.Username);
                            HttpContext.Session.SetString("AdminRole", user.Role.ToString());
                            HttpContext.Session.SetString("AdminEmail", user.Email);
                            HttpContext.Session.SetString("AdminLoginTime", DateTime.Now.ToString());

                            // Update last login time
                            user.LastLogin = DateTime.Now;
                            await _context.SaveChangesAsync();

                            _logger.LogInformation("Admin/Manager login successful for user: {Username}, Role: {Role}", user.Username, user.Role);

                            TempData["SuccessMessage"] = $"Welcome back, {user.Username}!";

                            // Redirect based on role
                            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }

                            if (user.Role == UserRole.Admin || user.Role == UserRole.Manager)
                            {
                                return RedirectToAction("Dashboard", "Manager");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Admin login denied - insufficient privileges for user: {Username}", user.Username);
                            TempData["ErrorMessage"] = "Access denied. Administrator or Manager privileges required.";
                            return View();
                        }
                    }
                }

                _logger.LogWarning("Admin login failed for username: {Username}", username);
                TempData["ErrorMessage"] = "Invalid username or password.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login for username: {Username}", username);
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return View();
            }
        }

        // GET: Check if admin is logged in
        public IActionResult CheckAdminAuth()
        {
            var isAuthenticated = HttpContext.Session.GetInt32("AdminUserId") != null;
            var userInfo = isAuthenticated ? new
            {
                Username = HttpContext.Session.GetString("AdminUsername"),
                Role = HttpContext.Session.GetString("AdminRole"),
                LoginTime = HttpContext.Session.GetString("AdminLoginTime")
            } : null;

            return Json(new { isAuthenticated, userInfo });
        }

        // Admin Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminLogout()
        {
            try
            {
                var username = HttpContext.Session.GetString("AdminUsername");

                // Clear all session data
                HttpContext.Session.Clear();

                _logger.LogInformation("Admin logout successful for user: {Username}", username);

                TempData["SuccessMessage"] = "You have been logged out successfully.";
                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin logout");
                TempData["ErrorMessage"] = "Error during logout.";
                return RedirectToAction("Index", "Auth");
            }
        }

        // GET: Quick logout (without anti-forgery token)
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}