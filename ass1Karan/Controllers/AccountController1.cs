using ass1Karan.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ass1Karan.Controllers
{
    public class AccountController : Controller
    {
        private readonly StudentDBContext _context;

        public AccountController(StudentDBContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword)
        {
            try
            {
                
                if (password != confirmPassword)
                {
                    ViewBag.Error = "Passwords do not match.";
                    return View();
                }

                if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
                {
                    ViewBag.Error = "Password must have at least 8 characters, 1 uppercase, 1 lowercase, and 1 digit.";
                    return View();
                }

                
                if (_context.Users.Any(u => u.Email == email))
                {
                    ViewBag.Error = "Email already registered.";
                    return View();
                }

                
                string hashedPassword = HashPassword(password);

                
                var user = new User { Email = email, PasswordHash = hashedPassword };
                _context.Users.Add(user);
                _context.SaveChanges();

                ViewBag.Success = "Registration successful! You can now log in.";
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Register(): {ex.Message}");
                ViewBag.Error = "Something went wrong during registration. Please try again later.";
                return View();
            }
        }

        private string HashPassword(string password)
        {
            try
            {
                using (var sha = SHA256.Create())
                {
                    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var builder = new StringBuilder();
                    foreach (var b in bytes)
                        builder.Append(b.ToString("x2"));
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HashPassword(): {ex.Message}");
                throw new Exception("Password hashing failed.");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Email and password are required.";
                    return View();
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    ViewBag.Error = "Email not registered.";
                    return View();
                }

                string hashedPassword = HashPassword(password);

                if (user.PasswordHash != hashedPassword)
                {
                    ViewBag.Error = "Invalid password.";
                    return View();
                }

               
                HttpContext.Session.SetString("UserEmail", user.Email);

                return RedirectToAction("Index", "News");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Login(): {ex.Message}");
                ViewBag.Error = "Login failed due to a system error. Please try again.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Logout(): {ex.Message}");
                ViewBag.Error = "Unable to logout. Please try again later.";
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
