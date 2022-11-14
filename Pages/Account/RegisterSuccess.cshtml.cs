using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Pages
{
    public class RegisterSuccessModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        public void OnGet()
        {
        }
    }
}
