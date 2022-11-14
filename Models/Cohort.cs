using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Models
{
  public class Cohort
  {
    [Key]
    public int Id { get; set; }
    [Required, MinLength(2), MaxLength(50)]
    public string CohortName { get; set; }
    [Required, MinLength(2), MaxLength(255)]
    public string Description { get; set; }
    public DateTime Create_time { get; set; }

    public ICollection<ApplicationUser> Students { get; set; }

    public ICollection<Forum> Forums { get; set; }
  }
}