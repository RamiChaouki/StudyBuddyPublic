using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace StudyBuddy.Pages.Admin
{
  [Authorize(Roles = "Admin")]
  public class AdminModel : PageModel
  {
    public int NumberOfUsers { get; set; }
    public int NumberOfCohorts { get; set; }
    public int NumberOfForums { get; set; }

    private readonly StudyBuddyDbContext db;
    private readonly ILogger<AdminModel> _logger;


    public AdminModel(StudyBuddyDbContext db, ILogger<AdminModel> logger)
    {
      this.db = db;
      _logger = logger;
    }
    public async Task OnGetAsync()
    {
      List<Forum> forums = await db.Forums.ToListAsync();
      NumberOfForums = forums.Count();
      List<Cohort> cohorts = await db.Cohorts.ToListAsync();
      NumberOfCohorts = cohorts.Count();
      List<ApplicationUser> users = await db.Users.ToListAsync();
      NumberOfUsers = users.Count();
    }
  }
}
