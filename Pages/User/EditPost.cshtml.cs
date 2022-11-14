using System.ComponentModel.DataAnnotations;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace StuddyBuddy.Pages.User
{
    [Authorize(Roles = "Teacher,Student")]
    public class EditPostModel : PageModel
    {
        private readonly BlobContainerClient _containerClient;

        private readonly StudyBuddyDbContext db;
        private ILogger<NewPostModel> logger;
        public EditPostModel(StudyBuddyDbContext db, ILogger<NewPostModel> logger, IOptions<AzureStorageConfig> config)
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

        public List<PostImage> PostImages { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {

            var postToEdit = await db.Posts.Include(p => p.Author).Include(p => p.PostImages).Where(p => p.Id == Id).FirstOrDefaultAsync();

            if (postToEdit == null)
            {
                return NotFound();
            }

            Title = postToEdit.Title;
            Content = postToEdit.Content;
            PostImages = postToEdit.PostImages.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || db.Posts == null)
            {
                return Page();
            }

            var postToEdit = await db.Posts.Include(p => p.Author).Where(p => p.Id == Id).FirstOrDefaultAsync();

            if (postToEdit == null)
            {
                return NotFound();
            }

            if (postToEdit.Author.UserName != User.Identity.Name)
            {
                return Unauthorized();
            }

            postToEdit.Title = Title;
            postToEdit.Content = Content;

            int activePostId = (postToEdit.ParentId == null) ? postToEdit.Id : (int)postToEdit.ParentId;

            if (FilesToUpload == null || FilesToUpload.Count < 1)
            {
                db.Posts.Update(postToEdit);
                await db.SaveChangesAsync();

                return RedirectToPage("./Forums", new { ActivePostId = activePostId });
            }

            // int count = 1;
            foreach (IFormFile UploadFile in FilesToUpload)
            {
                Console.WriteLine(UploadFile.FileName);
                try
                {
                    string filename = "post-" + postToEdit.Id + "-image-" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
                        await db.PostImages.AddAsync(new PostImage { PostId = postToEdit.Id, ImageUrl = blobClient.Uri.AbsoluteUri });
                        var imageSaveRes = await db.SaveChangesAsync();
                        if (imageSaveRes > 0)
                        {
                            // count++;
                            continue;
                        }
                        else
                        {
                            ModelState.AddModelError(String.Empty, $"Unable to upload image: {UploadFile.FileName}");
                            return Page();
                        }
                    }

                }
                catch (SystemException e)
                {
                    Console.WriteLine($"Unable to upload File: {e.Message}");
                    ModelState.AddModelError(String.Empty, $"Unable to upload images");
                    return Page();
                }
            }

            return RedirectToPage("./Forums", new { ActivePostId = activePostId });
        }

        public async Task<IActionResult> OnPostDeleteImage(int imageId)
        {
            PostImage imageToDelete = await db.PostImages.FindAsync(imageId);
            int postId = imageToDelete.PostId;
            string imageUrl = imageToDelete.ImageUrl;

            if (imageToDelete == null)
            {
                ModelState.AddModelError(String.Empty, "Image not found!");
                return Page();
            }
            string filename = Path.GetFileName(imageUrl);

            try
            {
                db.PostImages.Remove(imageToDelete);
                int affectedRows = await db.SaveChangesAsync();

                if (affectedRows < 1)
                {
                    ModelState.AddModelError(String.Empty, "Unable to delete image from DB!");
                    return Page();
                }

                await _containerClient.CreateIfNotExistsAsync();
                var blobClient = _containerClient.GetBlobClient(filename);

                // Delete the file from the container
                var azureResponse = await blobClient.DeleteIfExistsAsync();

                Console.WriteLine(azureResponse.GetRawResponse().ReasonPhrase + ": " + filename);

                if (azureResponse)
                {
                    return RedirectToPage("./EditPost", new { Id = postId });
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Unable to delete image from container!");
                    return Page();
                }
            }
            catch (Exception ex) when (ex is SystemException or Azure.RequestFailedException)
            {
                Console.WriteLine($"Unable to delete image: {ex.Message}");
                ModelState.AddModelError(String.Empty, "Unable to delete image!");
                return Page();
            }
        }

    }

}

