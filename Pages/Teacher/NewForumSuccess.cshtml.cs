using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace StuddyBuddy.Pages.Teacher
{
    // [Authorize(Roles = "Teacher")]
    public class NewForumSuccessModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string ForumName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ForumDescription { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CohortId { get; set; }

        public void OnGet()
        {
        }
    }
}
