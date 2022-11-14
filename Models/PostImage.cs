using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyBuddy.Models
{
  public class PostImage
  {
    [Key]
    public int Id { get; set; }
    [Required]
    public int PostId { get; set; }
    [ForeignKey("PostId")]
    public virtual Post Post { get; set; }

    [Required, MinLength(2), MaxLength(500)]
    public string ImageUrl { get; set; }

  }
}