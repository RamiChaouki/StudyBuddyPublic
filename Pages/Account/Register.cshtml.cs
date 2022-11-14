using System.ComponentModel.DataAnnotations;
using Blog.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyBuddy.Models;

namespace Blog.Pages //?
{
  public class RegisterModel : PageModel
  {

    private UserManager<ApplicationUser> userManager;

    private ILogger<RegisterModel> logger;
    public RegisterModel(UserManager<ApplicationUser> userManager, ILogger<RegisterModel> logger)
    {
      this.userManager = userManager;
      this.logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {


      [Required, EmailAddress]
      public string Email { get; set; }

      [Required]
      [StringLength(100, MinimumLength = 6, ErrorMessage = " The {0} must be at least {2} and at max {1} characters long")]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm Password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (ModelState.IsValid)
      {
        var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
          logger.LogInformation($"User {Input.Email} created a new account with password");
          // var result2 = await userManager.AddToRoleAsync(user, "User");
          // if (result2.Succeeded)
          // {
          //     logger.LogInformation($"User {Input.Email} Has been added to Role User");
          return RedirectToPage("RegisterSuccess", new { email = Input.Email });
          // }
          // else
          //     {
          //         await userManager.DeleteAsync(user);
          //         foreach (var error in result2.Errors)
          //         {
          //             ModelState.AddModelError(string.Empty, error.Description);
          //         }
          //     }
        }
        else
        {
          foreach (var error in result.Errors)
          {
            ModelState.AddModelError(string.Empty, error.Description);
          }
        }
      }
      return Page();
    }
  }
}
