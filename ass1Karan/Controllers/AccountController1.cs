using ass1Karan.Models;
using Microsoft.AspNetCore.Mvc;
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

        private string HashPassword(string password)
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

        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(string email, string password)
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


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }


    }
}
