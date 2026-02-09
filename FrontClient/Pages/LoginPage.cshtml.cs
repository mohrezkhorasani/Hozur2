using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace FrontClient.Pages
{
    public class LoginPageModel : PageModel
    {
        public void OnGet()
        {
        }
        public IActionResult OnPostLoginHandler( string username,string password)
        {
            // Handle login logic here
            // For example:
            if (username == "info@hekmateislami.com" && password == "He123456@")
            {
                var options = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(14) // Set expiration for the cookie
                };
                
                Response.Cookies.Append("bbbToken", Guid.NewGuid().ToString(),options);
                return RedirectToPage("/Index"); // Redirect or render success page
            }

            return Page(); // Return the same page with an error message or redirect to another page
        }
    }
}
