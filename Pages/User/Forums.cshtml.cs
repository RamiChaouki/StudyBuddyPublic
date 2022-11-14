using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;


using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace StuddyBuddy.Pages.User
{
    [Authorize(Roles = "Teacher,Student")]
    public class ForumsModel : PageModel
    {

        private readonly BlobContainerClient _containerClient;
        private readonly StudyBuddyDbContext db;
        private readonly ILogger<ForumsModel> _logger;

        private readonly SignInManager<ApplicationUser> signInManager;

        public ForumsModel(ILogger<ForumsModel> logger, StudyBuddyDbContext db, SignInManager<ApplicationUser> signInManager, IOptions<AzureStorageConfig> config)
        {
            _logger = logger;
            this.db = db;
            this.signInManager = signInManager;
            _containerClient = new BlobContainerClient(config.Value.CONTAINER_CONNECTION_STRING, config.Value.CONTAINER_NAME);
        }

        public ApplicationUser ActiveUser { get; set; }

        public List<Forum> ForumsList { get; set; }
        public List<Post> PostsList { get; set; }

        public int ActiveForumId { get; set; }
        public Forum ActiveForum { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ActivePostId { get; set; }
        public Post ActivePost { get; set; }

        public List<Post> ActivePostReplies { get; set; }

        public Post NewReply { get; set; }

        [BindProperty]
        [Required, MinLength(2), MaxLength(255)]
        public string ReplyTitle { get; set; }

        [BindProperty]
        [Required, MinLength(2), MaxLength(20000)]
        public string ReplyContent { get; set; }

        [BindProperty]
        public List<IFormFile> FilesToUpload { get; set; }

        public string Message { get; set; }
        public string PostsMessage { get; set; }
        public string ForumsMessage { get; set; }
        public string ActivePostMessage { get; set; }



        public async Task<IActionResult> OnGetAsync()
        {


            ActiveUser = await db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefaultAsync();

            ForumsList = await DefaultForumsList();

            if (ForumsList == null || ForumsList.Count < 1)
            {
                return Page();
            }

            if (ActivePostId > 0)
            {
                return OnPostPost(ActivePostId).Result;
            }

            if (ActiveForum == null)
            {
                ActiveForum = ForumsList[0];
                ActiveForumId = ActiveForum.Id;
            }

            if (ActiveForum.Posts == null || ActiveForum.Posts.Count < 1)
            {
                return Page();
            }

            PostsList = await db.Posts.Include(p => p.PostImages).Include(p => p.Author).Include(p => p.Replies).ThenInclude(reply => reply.PostImages).Where(p => p.ForumId == ActiveForumId && p.ParentId == null).ToListAsync();
            Console.WriteLine("Count of posts = " + PostsList.Count);
            PostsMessage = "Count of posts = " + PostsList.Count;
            // PostsList = ActiveForum.Posts.Where(p => p.ParentId == null).ToList();

            if (ActivePost == null)
            {
                ActivePost = PostsList[0];
                ActivePostId = ActivePost.Id;
            }

            // ActivePostReplies = await db.Posts.Include(p => p.Replies).ThenInclude(r => r.PostImages).ToListAsync();
            ActivePostReplies = ActivePost.Replies;

            return Page();
        }


        public async Task<IActionResult> OnPostForum(int id)
        {
            ForumsList = await DefaultForumsList();
            ActiveForumId = id;

            ActiveForum = ForumsList.FirstOrDefault(f => f.Id == ActiveForumId);
            if (ActiveForum.Posts == null)
            {
                return Page();
            }

            PostsList = await db.Posts.Include(p => p.PostImages).Include(p => p.Author).Include(p => p.Replies).ThenInclude(reply => reply.PostImages).Where(p => p.ForumId == ActiveForumId && p.ParentId == null).ToListAsync();
            // PostsList = ActiveForum.Posts.Where(p => p.ParentId == null).ToList();
            ActivePost = null;

            ModelState.Clear();

            return Page();
        }

        public async Task<IActionResult> OnPostPost(int id)
        {
            ForumsList = await DefaultForumsList();
            ActivePostId = id;

            ActivePost = await db.Posts.Include(p => p.PostImages).Include(p => p.Author).Include(p => p.Replies).ThenInclude(reply => reply.PostImages).Where(p => p.Id == ActivePostId).FirstOrDefaultAsync();

            if (ActivePost == null)
            {
                // PostsMessage = $"Nothing to show.";
                return Page();
            }

            ActivePostReplies = ActivePost.Replies;

            ActiveForumId = ActivePost.ForumId;
            ActiveForum = ActivePost.Forum;

            PostsList = await db.Posts.Include(p => p.PostImages).Include(p => p.Author).Include(p => p.Replies).ThenInclude(reply => reply.PostImages).Where(p => p.ForumId == ActiveForumId && p.ParentId == null).ToListAsync();

            ReplyTitle = "";
            ReplyContent = "";

            ModelState.Clear();

            return Page();
        }

        public async Task<IActionResult> OnPostReply(int parentId, int forumId)
        {

            ForumsList = await DefaultForumsList();

            if (!ModelState.IsValid || db.Posts == null)
            {
                ActivePostId = parentId;
                return Page();
            }


            NewReply = new Post()
            {
                Title = ReplyTitle,
                Content = ReplyContent,
                Author = ActiveUser,
                Create_time = DateTime.Now,
                ForumId = forumId,
                ParentId = parentId
            };

            db.Posts.Add(NewReply);
            var result = await db.SaveChangesAsync();

            if (result > 0)
            {
                if (FilesToUpload == null || FilesToUpload.Count < 1)
                {
                    return RedirectToPage("./Forums", new { ActivePostId = parentId });
                }

                foreach (IFormFile UploadFile in FilesToUpload)
                {
                    Console.WriteLine("Uploading file = " + UploadFile.FileName);
                    try
                    {
                        string filename = "post-" + NewReply.Id + "-image-" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        using (var stream = UploadFile.OpenReadStream())
                        {
                            await _containerClient.CreateIfNotExistsAsync();

                            var blobClient = _containerClient.GetBlobClient(filename);

                            // Upload the file to the container
                            // var result = await _containerClient.UploadBlobAsync(filename, UploadFile.OpenReadStream());
                            var azureResponse = await blobClient.UploadAsync(stream, true);

                            Console.WriteLine(azureResponse.GetRawResponse().ReasonPhrase + ": " + filename);
                            // lblUploadResults.Add("Uploaded: " + filename);

                            // await _containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
                            await db.PostImages.AddAsync(new PostImage { PostId = NewReply.Id, ImageUrl = blobClient.Uri.AbsoluteUri });
                            var imageSaveRes = await db.SaveChangesAsync();
                            if (imageSaveRes > 0)
                            {
                                continue;
                            }
                            else
                            {
                                db.Posts.Remove(NewReply);
                                await db.SaveChangesAsync();
                                ModelState.AddModelError(String.Empty, $"Unable to upload image: {UploadFile.FileName}");
                                return Page();
                            }
                        }
                    }
                    catch (SystemException e)
                    {
                        db.Posts.Remove(NewReply);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"Unable to upload File: {e.Message}");
                        ModelState.AddModelError(String.Empty, $"Unable to upload images");
                        return Page();
                    }

                }

            }

            // return RedirectToPage()
            return RedirectToPage("./Forums", new { ActivePostId = parentId });

        }


        public async Task<IActionResult> OnPostDeleteReply(int id)
        {

            Post replyToDelete = await db.Posts.Include(p => p.Replies).Where(p => p.Id == id).FirstOrDefaultAsync();

            if (replyToDelete == null)
            {
                return Page();
            }

            int parentId = 0;
            if (replyToDelete.ParentId != null)
            {
                parentId = (int)replyToDelete.ParentId;
            }


            if (replyToDelete.Replies != null || replyToDelete.Replies.Count > 0)
            {
                db.Posts.RemoveRange(replyToDelete.Replies);
                await db.SaveChangesAsync();
                // foreach (var reply in replyToDelete.Replies)
                // {
                //     db.Posts.RemoveRange(reply);
                //     await db.SaveChangesAsync();
                // }

            }

            if (replyToDelete.Replies == null || replyToDelete.Replies.Count < 1)
            {
                db.Posts.Remove(replyToDelete);
                await db.SaveChangesAsync();
            }

            ForumsList = await DefaultForumsList();

            if (parentId < 1)
            {
                return RedirectToPage("./Forums");
            }

            return RedirectToPage("./Forums", new { ActivePostId = parentId });
        }

        private async Task<List<Forum>> DefaultForumsList()
        {
            List<Forum> ForumsList = new List<Forum>();

            ActiveUser = await db.Users.Include(u => u.Cohort).Where(u => u.UserName == User.Identity.Name).FirstOrDefaultAsync();

            if (User.IsInRole("Student"))
            {
                var cohort = ActiveUser.Cohort;
                ForumsList = db.Forums.Include(f => f.Posts).Where(f => f.CohortId == cohort.Id).ToList();
                // Console.WriteLine(ForumsList.Count());
            }

            if (User.IsInRole("Teacher"))
            {
                ForumsList = db.Forums.Include(f => f.Posts).ToList();
            }

            return ForumsList;
        }

    }
}
