using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace StudyBuddy.Pages.Admin.Users
{
  [Authorize(Roles = "Admin")]
  public class ViewUsersModel : PageModel
  {

    public List<ApplicationUser> usersList { get; set; }
    private readonly StudyBuddyDbContext db;
    private readonly ILogger<ViewUsersModel> _logger;
    private RoleManager<IdentityRole> roleManager;
    private UserManager<ApplicationUser> userManager;


    public ViewUsersModel(StudyBuddyDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleMgr, ILogger<ViewUsersModel> logger)
    {
      this.db = db;
      _logger = logger;
      this.userManager = userManager;
      this.roleManager = roleMgr;
    }
    [BindProperty]
    public IList<UserRolesViewModel> model { get; set; } = new List<UserRolesViewModel>();
    public class UserRolesViewModel
    {
      public string Id { get; set; }
      public string UserName { get; set; }
      public string Email { get; set; }
      public IEnumerable<string> Role { get; set; }
      public string CohortName { get; set; }
    }

    [BindProperty]
    public string UserIdDelete { get; set; }
    public async Task OnGetAsync()
    {
      usersList = await db.Users.Include("Cohort").ToListAsync();
      // var users = await userManager.Users.ToListAsync();


      foreach (ApplicationUser user in usersList)
      {
        // if (user.Cohort == null) continue;
        UserRolesViewModel urv = new UserRolesViewModel()
        {
          Id = user.Id,
          UserName = user.UserName,
          Email = user.Email,
          Role = await userManager.GetRolesAsync(user)
        };
        if (user.Cohort != null)
        {
          urv.CohortName = user.Cohort.CohortName;
          // int countRole = urv.Role.Count();
        }
        model.Add(urv);
      }
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var userFromDB = await db.Users.FindAsync(UserIdDelete);

      db.Users.Remove(userFromDB);
      await db.SaveChangesAsync();
      return RedirectToPage("./ViewUsers");
    }



  }
}
