using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Data.Seeders
{
    public class StudentSeeder
    {
        public static async Task seedStudent(StudyBuddyDbContext db,UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            int studentSeeds=30;
            int studentNumber=1;
            string studentPassword= "Student123";
            while(studentSeeds>0)
            {
                string studentUsername = "Student"+studentNumber+"@johnabbottcollege.net";
                studentNumber++;

                if(userManager.FindByNameAsync(studentUsername).Result==null)
                {
                    ApplicationUser student = new ApplicationUser(){UserName=studentUsername,Email=studentUsername, EmailConfirmed=true, CohortId=1};
                    IdentityResult result = userManager.CreateAsync(student,studentPassword).Result;

                    if(result.Succeeded)
                    {
                        userManager.AddToRoleAsync(student,"Student").Wait();
                    }
                    
                }
                studentSeeds--;
            }
        }
    }
}