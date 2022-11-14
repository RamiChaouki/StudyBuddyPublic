using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyBuddy.Models;
using StudyBuddy.Data;

namespace StudyBuddy.Pages.TeamBuilder
{
    public class TeamBuildResultsModel : PageModel
    {
        private readonly StudyBuddyDbContext db;
        public TeamBuildResultsModel(StudyBuddyDbContext db)
        {
            this.db=db;
        }
        public int Number {get;set;}
        public string TeacherCohort {get;set;}
        public List<Team> Teams {get;set;}

        public async Task OnGet(int TeamSize, string Cohort)
        {
            Number=TeamSize;
            TeacherCohort=Cohort;
            Teams = await TeamBuilderAlgo.TeamBuilderAlgorithm(db,TeamSize,Cohort);
            
        }
    }
}
