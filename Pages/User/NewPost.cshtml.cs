using System.ComponentModel.DataAnnotations;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace StuddyBuddy.Pages.User
{
    [Authorize(Roles = "Teacher,Student")]
    public class NewPostModel : PageModel
    {
        private readonly BlobContainerClient _containerClient;
        private readonly StudyBuddyDbContext db;
        private ILogger<NewPostModel> logger;
        public NewPostModel(StudyBuddyDbContext db, ILogger<NewPostModel> logger, IOptions<AzureStorageConfig> config)
        {
            this.db = db;
            this.logger = logger;
            _containerClient = new BlobContainerClient(config.Value.CONTAINER_CONNECTION_STRING, config.Value.CONTAINER_NAME);
        }

        [BindProperty]
        [Required, MinLength(2), MaxLength(255)]
        public string Title { get; set; }

        [BindProperty]
        [Required, MinLength(2), MaxLength(20000)]
        public string Content { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public List<IFormFile> FilesToUpload { get; set; }
        public List<PostImage> PostImageList { get; set; } = new List<PostImage>();

        public string Message { get; set; }



        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || db.Posts == null)
            {
                return Page();
            }

            Post NewPost = new Post() { Title = Title, Content = Content, Create_time = DateTime.Now, ForumId = Id };
            NewPost.Author = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            await db.Posts.AddAsync(NewPost);
            var result = await db.SaveChangesAsync();

            if (result > 0)
            {
                if (FilesToUpload == null || FilesToUpload.Count < 1)
                {
                    return RedirectToPage("./NewPostSuccess", new { PostTitle = NewPost.Title });
                }

                foreach (IFormFile UploadFile in FilesToUpload)
                {
                    try
                    {
                        string filename = "post-" + NewPost.Id + "-image-" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        Console.WriteLine("Uploading: " + filename);
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
                            await db.PostImages.AddAsync(new PostImage { PostId = NewPost.Id, ImageUrl = blobClient.Uri.AbsoluteUri });
                            var imageSaveRes = await db.SaveChangesAsync();
                            if (imageSaveRes > 0)
                            {
                                continue;
                            }
                            else
                            {
                                db.Posts.Remove(NewPost);
                                await db.SaveChangesAsync();
                                ModelState.AddModelError(String.Empty, $"Unable to upload image: {UploadFile.FileName}");
                                return Page();
                            }
                        }

                    }
                    catch (SystemException e)
                    {
                        db.Posts.Remove(NewPost);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"Unable to upload File: {e.Message}");
                        ModelState.AddModelError(String.Empty, $"Unable to upload images");
                        return Page();
                    }
                }
            }

            return RedirectToPage("./NewPostSuccess", new { PostTitle = NewPost.Title });
        }
    }
}
