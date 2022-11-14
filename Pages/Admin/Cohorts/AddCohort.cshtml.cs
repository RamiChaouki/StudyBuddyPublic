using System.ComponentModel.DataAnnotations;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudyBuddy.Pages.Admin.Cohorts
{
  [Authorize(Roles = "Admin")]
  public class AddCohortModel : PageModel
  {
    private readonly StudyBuddyDbContext db;
    private readonly ILogger<AddCohortModel> _logger;

    public AddCohortModel(StudyBuddyDbContext db, ILogger<AddCohortModel> logger)
    {
      this.db = db;
      _logger = logger;
    }


    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
      [Required, MinLength(2), MaxLength(50)]
      public string CohortName { get; set; }

      [Required, MinLength(2), MaxLength(255)]
      public string Description { get; set; }
    }
    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPostAsync()
    {
      if (ModelState.IsValid)
      {

        var newCohort = new Cohort { CohortName = Input.CohortName, Description = Input.Description, Create_time = DateTime.Now };
        await db.AddAsync(newCohort);
        await db.SaveChangesAsync();

        return RedirectToPage("./ViewCohorts"); ;
      }
      return Page();
    }

  }
}
