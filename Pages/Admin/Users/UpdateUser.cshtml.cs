using System.ComponentModel.DataAnnotations;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StuddyBuddy.Pages.Admin.Users
{
  [Authorize(Roles = "Admin")]
  public class UpdateUserModel : PageModel
  {
    private UserManager<ApplicationUser> userManager;
    private readonly StudyBuddyDbContext db;
    private readonly ILogger<UpdateUserModel> _logger;
    private RoleManager<IdentityRole> roleManager;

    public UpdateUserModel(StudyBuddyDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleMgr, ILogger<UpdateUserModel> logger)
    {
      this.db = db;
      _logger = logger;
      this.userManager = userManager;
      this.roleManager = roleMgr;
    }
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }
    [BindProperty]
    public ApplicationUser User { get; set; }
    [BindProperty]
    public IEnumerable<Cohort> cohortsList { get; set; }
    [BindProperty]
    public List<IdentityRole> RolesList { get; set; }
    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
      [Required]
      public string UserName { get; set; }
      [Required, EmailAddress]
      public string Email { get; set; }

      // [Required]
      // [StringLength(100, MinimumLength = 6, ErrorMessage = " The {0} must be at least {2} and at max {1} characters long")]
      // [DataType(DataType.Password)]
      // public string Password { get; set; }

      // [DataType(DataType.Password)]
      // [Display(Name = "Confirm Password")]
      // [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      // public string ConfirmPassword { get; set; }

      [Required]
      public int CohortId { get; set; }
      public bool confirmEmail { get; set; }
      [Required]
      public string selectedRole { get; set; }
    }

    public async Task OnGetAsync()
    {
      User = await db.Users.FindAsync(Id);
      cohortsList = await db.Cohorts.ToListAsync();
      // CreateRole("Admin");
      // CreateRole("Student");
      // CreateRole("Teacher");
      RolesList = await roleManager.Roles.ToListAsync();


    }

    public async Task<IActionResult> OnPostAsync()
    {
      // var user = userManager.FindByIdAsync(Id);
      var user = await db.Users.FindAsync(Id);
      User = user;
      if (ModelState.IsValid)
      {

        if (User != null)
        {

          User.UserName = Input.UserName;
          User.Email = Input.Email;
          User.CohortId = Input.CohortId;
          User.EmailConfirmed = Input.confirmEmail;

        }
        var result1 = await userManager.UpdateAsync(User);
        // We are removing the user from the old role. 
        var roles = await userManager.GetRolesAsync(User);
        await userManager.RemoveFromRolesAsync(User, roles.ToArray());
        // Now we are adding the user to the new role
        var result2 = await userManager.AddToRoleAsync(User, Input.selectedRole);
        db.SaveChanges();
        return RedirectToPage("./ViewUsers");
      }
      return Page();
    }

    private async Task CreateRole(string roleName)
    {
      bool result = await roleManager.RoleExistsAsync(roleName);
      if (!result)
      {
        // first we create  role    
        var role = new IdentityRole();
        role.Name = roleName;
        await roleManager.CreateAsync(role);
      }

    }

  }
}
