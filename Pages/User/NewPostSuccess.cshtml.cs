using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StuddyBuddy.Pages.User
{
    public class NewPostSuccessModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string PostTitle { get; set; }

        public void OnGet()
        {
        }
    }
}
