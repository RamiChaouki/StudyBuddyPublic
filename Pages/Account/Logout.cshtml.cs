using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyBuddy.Models;

namespace Blog.Pages
{
  public class LogoutModel : PageModel
  {
    private UserManager<ApplicationUser> userManager;
    private SignInManager<ApplicationUser> signInManager;

    private ILogger<RegisterModel> logger;
    public LogoutModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RegisterModel> logger)
    {
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.logger = logger;
    }
    public async Task<IActionResult> OnGetAsync()
    {
      if (signInManager.IsSignedIn(User))
      {
        await signInManager.SignOutAsync();
        // logger.LogInformation($"User {User.Identity.Name} has logged out");
        return RedirectToPage("/Index");
      }
      return Page();
    }
  }
}
