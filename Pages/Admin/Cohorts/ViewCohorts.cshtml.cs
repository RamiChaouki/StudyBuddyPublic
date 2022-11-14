using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace StudyBuddy.Pages.Admin.Cohorts
{
  [Authorize(Roles = "Admin")]
  public class ViewCohortsModel : PageModel
  {
    public List<Cohort> cohortsList { get; set; }
    private readonly StudyBuddyDbContext db;
    private readonly ILogger<ViewCohortsModel> _logger;
    [BindProperty]
    public int cohortId { get; set; }

    public ViewCohortsModel(StudyBuddyDbContext db, ILogger<ViewCohortsModel> logger)
    {
      this.db = db;
      _logger = logger;
    }
    public async Task OnGetAsync()
    {
      cohortsList = await db.Cohorts.ToListAsync();
    }

    public string Message { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
      var cohortFromDB = await db.Cohorts.FindAsync(cohortId);
      //Fixme need to delete cohortID in users table first
      List<Forum> cohortForums = new List<Forum>();
      if (cohortFromDB.Forums != null)
      {
        cohortForums = cohortFromDB.Forums.ToList();
      }

      foreach (var forum in cohortForums)
      {
        List<Post> forumPosts = new List<Post>();
        if (forum.Posts != null)
        {
          forumPosts = forum.Posts.ToList();
        }
        foreach (var post in forumPosts)
        {
          db.Posts.Remove(post);
          await db.SaveChangesAsync();
        }
        Forum updatedForum = await db.Forums.FindAsync(forum.Id);
        if (updatedForum.Posts.Count < 1 || updatedForum.Posts == null)
        {
          db.Forums.Remove(updatedForum);
          await db.SaveChangesAsync();
        }
      }

      var updatedCohort = await db.Cohorts.FindAsync(cohortId);
      if (updatedCohort.Forums == null)
      {
        db.Cohorts.Remove(updatedCohort);
        await db.SaveChangesAsync();
        return RedirectToPage("./ViewCohorts");
      }
      Message = "OOps! somethng went wrong. We are unable to delete the cohort!";
      return Page();
    }

  }
}
