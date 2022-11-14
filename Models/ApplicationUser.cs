using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Models
{

    public class ApplicationUser : IdentityUser
    {

        public string? TimePreferences { get; set; }

        public int? CohortId { get; set; }

        [ForeignKey("CohortId")]
        public virtual Cohort? Cohort { get; set; }

        // [InverseProperty("Author")]
        public virtual ICollection<Post>? Posts { get; set; }

        [InverseProperty("WeightingStudent")]
        public virtual ICollection<ColleagueWeight>? WeightingStudents { get; set; }


        [InverseProperty("WeightedStudent")]
        public virtual ICollection<ColleagueWeight>? WeightedStudents { get; set; }


        [InverseProperty("RatingStudent")]
        public virtual ICollection<ColleagueRating>? RatingStudents { get; set; }


        [InverseProperty("RatedStudent")]
        public virtual ICollection<ColleagueRating>? RatedStudents { get; set; }
    }
}