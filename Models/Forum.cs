using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyBuddy.Models
{
    public class Forum
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(2), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(2), MaxLength(255)]
        public string Description { get; set; }
        public DateTime Create_time { get; set; }
        [Required]
        public int CohortId { get; set; }
        [ForeignKey("CohortId")]
        public virtual Cohort? Cohort { get; set; }

        public ICollection<Post>? Posts { get; set; }
    }
}