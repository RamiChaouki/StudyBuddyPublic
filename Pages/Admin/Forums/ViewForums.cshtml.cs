using System.ComponentModel.DataAnnotations;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StudyBuddy.Pages.Admin.Forums
{
  [Authorize(Roles = "Admin")]
  public class ViewForumsModel : PageModel
  {
    public List<Forum> forumsList { get; set; }
    private readonly StudyBuddyDbContext db;
    private readonly ILogger<ViewForumsModel> _logger;

    public ViewForumsModel(StudyBuddyDbContext db, ILogger<ViewForumsModel> logger)
    {
      this.db = db;
      _logger = logger;
    }
    [BindProperty]
    public string Message { get; set; }

    [BindProperty]
    public string forumId { get; set; }
    public async Task OnGetAsync()
    {
      forumsList = await db.Forums.ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var forumFromDB = await db.Forums.FindAsync(forumId);
      List<Post> forumPosts = new List<Post>();
      if (forumFromDB.Posts != null)
      {
        forumPosts = forumFromDB.Posts.ToList();
      }
      foreach (var post in forumPosts)
      {
        db.Posts.Remove(post);
        await db.SaveChangesAsync();
      }
      if (forumFromDB.Posts.Count < 1 || forumFromDB.Posts == null)
      {
        db.Forums.Remove(forumFromDB);
        await db.SaveChangesAsync();
        return RedirectToPage("./ViewForums");
      }
      Message = "OOps! somethng went wrong. We are unable to delete the cohort!";
      return Page();

    }
  }
}
