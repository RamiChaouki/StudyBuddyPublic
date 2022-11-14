using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Models
{
    public class ColleagueRating
    {
        public int Id { get; set; }

        public string RatingStudentId { get; set; }
        public string RatedStudentId { get; set; }

        [ForeignKey("RatingStudentId")]
        public ApplicationUser RatingStudent { get; set; }

        [ForeignKey("RatedStudentId")]
        public ApplicationUser RatedStudent { get; set; }

        [Required]
        public short Rating { get; set; } = 0;

        [Required]
        public Preferences Pref{get;set;}=Preferences.Neutral; 
    }

    public enum Preferences
    {
        Prefer,
        Neutral,
        Avoid
    }
}