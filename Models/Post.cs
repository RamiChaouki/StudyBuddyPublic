using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyBuddy.Models
{
  public class Post
  {
    [Key]
    public int Id { get; set; }

    // public string AuthorId { get; set; }
    // [ForeignKey("AuthorId")]
    public virtual ApplicationUser Author { get; set; }

    [Required, MinLength(2), MaxLength(255)]
    public string Title { get; set; }
    [Required, MinLength(2), MaxLength(20000)]
    public string Content { get; set; }
    public DateTime Create_time { get; set; }

    [Required]
    public int ForumId { get; set; }
    [ForeignKey("ForumId")]
    public virtual Forum? Forum { get; set; }

    public int? ParentId { get; set; }
    [ForeignKey("ParentId")]
    public virtual Post? ParentPost { get; set; }

    [InverseProperty("ParentPost")]
    public virtual List<Post>? Replies { get; set; }

    public ICollection<PostImage>? PostImages { get; set; }
  }
}