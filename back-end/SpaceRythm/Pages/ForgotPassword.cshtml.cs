using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceRythm.Interfaces;
using SpaceRythm.Services;

namespace SpaceRythm.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
    //public class ForgotPasswordModel : PageModel
    //{
    //    private readonly IUserService _userService;

    //    public ForgotPasswordModel(IUserService userService)
    //    {
    //        _userService = userService;
    //    }

    //    public void OnGet()
    //    {
    //        Console.WriteLine("ForgotPassword page loaded");
    //    }

    //    public async Task<IActionResult> OnPostAsync(string email)
    //    {
    //        // Redirect the request to the Users controller
    //        return RedirectToAction("ForgotPassword", "Users", new { email });
    //    }
    //}
}
