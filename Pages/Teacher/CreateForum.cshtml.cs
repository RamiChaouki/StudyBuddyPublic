using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace StuddyBuddy.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class CreateForumModel : PageModel
    {

        private readonly StudyBuddyDbContext db;
        private ILogger<CreateForumModel> logger;
        public CreateForumModel(StudyBuddyDbContext db, ILogger<CreateForumModel> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [BindProperty]
        public Forum NewForum { get; set; } = default!;

        public SelectList CohortsList { get; set; }

        public IActionResult OnGet()
        {
            CohortsList = new SelectList(db.Cohorts, nameof(Cohort.Id), nameof(Cohort.CohortName));
            if (CohortsList.Count() < 1)
            {
                ModelState.AddModelError(String.Empty, "There are no cohorts in the database. Ask the admin to create a new Cohort so that you can add a forum");
                return Page();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || db.Forums == null || NewForum == null)
            {
                return Page();
            }
            NewForum.Create_time = DateTime.Now;
            db.Forums.Add(NewForum);
            await db.SaveChangesAsync();

            return RedirectToPage("./NewForumSuccess", new { ForumName = NewForum.Title, ForumDescription = NewForum.Description, CohortId = NewForum.CohortId });
        }
    }
}
