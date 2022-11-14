using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace Blog.Data
{
    public class DefaultRoleAndUsers
    {

        public static async Task SeedUsersAndRoles(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNamesList = new string[] { "Student", "Teacher", "Admin" };
            foreach (string roleName in roleNamesList)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    IdentityRole role = new IdentityRole() { Name = roleName };
                    IdentityResult result = roleManager.CreateAsync(role).Result;
                    foreach (IdentityError error in result.Errors)
                    {
                        // TODO: Log errors
                    }
                }
            }

            string adminEmail = "admin@admin.com";
            string adminPass = "Admin123";

            if (userManager.FindByNameAsync(adminEmail).Result == null)
            {
                ApplicationUser user = new ApplicationUser() { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                IdentityResult result = userManager.CreateAsync(user, adminPass).Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                    // FIXME: errors
                }
                foreach (IdentityError error in result.Errors)
                {
                    // TODO: Log errors
                }
            }

            string teacherEmail = "teacher@studybuddy.com";
            string teacherPass = "Teacher123";

            if (userManager.FindByNameAsync(teacherEmail).Result == null)
            {
                ApplicationUser user = new ApplicationUser() { UserName = teacherEmail, Email = teacherEmail, EmailConfirmed = true };
                IdentityResult result = userManager.CreateAsync(user, teacherPass).Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Teacher").Wait();
                    // FIXME: errors
                }
                foreach (IdentityError error in result.Errors)
                {
                    // TODO: Log errors
                }
            }
        }

        // public static async void UpdateUserRoles(StudyBuddyDbContext db, UserManager<ApplicationUser> userManager)
        // {
        //     List<ApplicationUser> usersList = db.Users.ToList();
        //     foreach (ApplicationUser user in usersList)
        //     {
        //         var roles = await userManager.GetRolesAsync(user);
        //         var userHasAnyRole = roles.Count > 0;
        //         if (!userHasAnyRole)
        //         {
        //             userManager.AddToRoleAsync(user, "User").Wait();
        //         }
        //         // FIXME: errors
        //     }
        // }
    }
}