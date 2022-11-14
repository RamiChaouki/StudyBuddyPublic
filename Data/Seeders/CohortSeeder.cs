using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace StudyBuddy.Data.Seeders
{
    public class CohortSeeder
    {
        
        public static async Task seedCohort(StudyBuddyDbContext db)
        {
            string [] cohortNames = {"FSD-1", "FSD-2", "FSD-3"};

            foreach (string c in cohortNames)
            {
                if(await db.Cohorts.Where(d=>d.CohortName==c).FirstOrDefaultAsync()==null)
                {
                    Cohort newCohort = new Cohort {CohortName=c, Description=c+" class of "+DateTime.Today.ToString("yyyy"), Create_time=DateTime.Now};
                    await db.Cohorts.AddAsync(newCohort);
                    
                }
            }
        await db.SaveChangesAsync();
        }
    }
}