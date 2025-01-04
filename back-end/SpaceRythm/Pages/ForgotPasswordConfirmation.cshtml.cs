using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpaceRythm.Pages
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        private readonly ILogger<ForgotPasswordConfirmationModel> _logger;

        public ForgotPasswordConfirmationModel(ILogger<ForgotPasswordConfirmationModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
