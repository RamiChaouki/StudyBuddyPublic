using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using StudyBuddy.Data;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Models;

namespace StudyBuddy.Pages.TeamBuilder
{
    public class TeamBuilderTeacherModel : PageModel
    {
        private readonly StudyBuddyDbContext db;
        public TeamBuilderTeacherModel(StudyBuddyDbContext db)
        {
            this.db=db;
        }
        [BindProperty,Range(2,6,ErrorMessage="Please select the size of your team.")]
        public int Number{get;set;}

        public List<SelectListItem> Options {get; set;}

        [BindProperty]
        public string Cohort{get;set;}
        public async Task OnGetAsync()
        {
            Options =await db.Cohorts.Select(c=>
                                            new SelectListItem
                                            {
                                                Value=c.CohortName,
                                                Text = c.CohortName
                                            }).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ApplicationUser teacher = await db.Users.Include("Cohort").Where(u=>u.UserName==User.Identity.Name).FirstOrDefaultAsync();
            // string TeacherCohort=teacher.Cohort.CohortName;
            string TeacherCohort=Cohort;
            if(!ModelState.IsValid)
            {
                return Page();
            }
            return RedirectToPage("TeamBuildResults",new{TeamSize=Number,Cohort=TeacherCohort});
        }
    }
}
