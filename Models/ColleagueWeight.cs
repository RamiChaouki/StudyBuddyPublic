using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StudyBuddy.Data;

namespace StudyBuddy.Models
{
    public class ColleagueWeight
    {
        public int ColleagueWeightId { get; set; }

        public string WeightingStudentId { get; set; }
        public string WeightedStudentId { get; set; }

        [ForeignKey("WeightingStudentId")]
        public ApplicationUser WeightingStudent { get; set; }

        [ForeignKey("WeightedStudentId")]
        public ApplicationUser WeightedStudent { get; set; }

        [Required]
        public short Weight { get; set; } = 0;

        public async static void UpdateStudentWeights(ApplicationUser weightingStudent, Dictionary<string, int> studentWeights, StudyBuddyDbContext db)
        {
            foreach (var newStudentWeight in studentWeights)
            {
                var oldStudentWeight = db.ColleagueWeights.Where(cw => cw.WeightingStudentId == weightingStudent.Id && cw.WeightedStudentId == newStudentWeight.Key).FirstOrDefault();
                oldStudentWeight.Weight = (short)newStudentWeight.Value;
                db.ColleagueWeights.Update(oldStudentWeight);
                await db.SaveChangesAsync();
            }
        }


        public static Dictionary<string, int> CalculateStudentWeights(ApplicationUser weightingStudent, StudyBuddyDbContext db)
        {
            var cohortId = weightingStudent.Cohort.Id;
            // FIXME: should have role student
            List<ApplicationUser> cohortStudents = db.Users.Where(u => u.CohortId == cohortId && u.Id != weightingStudent.Id).ToList();

            Dictionary<string, int> studentWeights = new Dictionary<string, int>();

            foreach (ApplicationUser weightedStudent in cohortStudents)
            {
                int weight = CalculateWeight(weightingStudent, weightedStudent, db);
                studentWeights[weightedStudent.Id] = weight;
            }
            return studentWeights;
        }

        public static int CalculateWeight(ApplicationUser weightingStudent, ApplicationUser weightedStudent, StudyBuddyDbContext db)
        {

            int timePrefWeight = CountCommonTimePreference(weightingStudent.TimePreferences, weightedStudent.TimePreferences);

            var weightingStudentRating = db.ColleagueRatings.Where(cr => cr.RatingStudentId == weightingStudent.Id && cr.RatedStudentId == weightedStudent.Id).FirstOrDefault();
            var weightedStudentRating = db.ColleagueRatings.Where(cr => cr.RatingStudentId == weightedStudent.Id && cr.RatedStudentId == weightingStudent.Id).FirstOrDefault();

            int RatingWeight = weightingStudentRating.Rating + weightedStudentRating.Rating;

            return timePrefWeight + RatingWeight;
        }


        public static int CountCommonTimePreference(string timepref1, string timepref2)
        {
            string longerString, shorterString;
            if(timepref1==null) timepref1="";
            if(timepref2==null) timepref2="";

            if (timepref1.Length > timepref2.Length)
            {
                longerString = timepref1;
                shorterString = timepref2;
            }
            else
            {
                longerString = timepref2;
                shorterString = timepref1;
            }

            int countOfEquals = 0;

            foreach (char i in shorterString)
            {
                if (longerString.Contains(i)) countOfEquals += 1;
            }

            return countOfEquals;
        }
    }
}