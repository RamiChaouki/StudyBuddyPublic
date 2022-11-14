using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Pages.TeamBuilder
{
    public class TeamBuilderStudentsModel : PageModel
    {
        private readonly StudyBuddyDbContext db;
        private UserManager<ApplicationUser> userManager;
        public TeamBuilderStudentsModel(StudyBuddyDbContext db,UserManager<ApplicationUser> userManager)
        {
            this.db=db;
            this.userManager=userManager;
        }


        [BindProperty]
        public List<char> AreChecked{get;set;}

        //array of bool that is used to determine based off of the user's stored preferences if a particular time of day was prefered
        public bool [] checkedStatus{get;set;}

        public List<ApplicationUser> UserList;
        
        [BindProperty]
        public string AvoidPseudoList{get;set;} //student1@dsffd.cs student2@dssd.cs

        [BindProperty]
        public string PreferredPseudoList{get;set;}

        public List<ApplicationUser> AvoidList{get;set;}
        public List<ApplicationUser> PreferList{get;set;}
        private ApplicationUser currentUser;
        public async Task OnGetAsync()
        {   //TODO FIND WAY TO FILTER USERLIST (LINQ?) WITH THE GET COLLEAGUE RATING
            //TODO CREATE AVOID AND PREFER LIST THAT WILL BE INITIALIZED ON GET AND ITERATED OVER IN THE VIEW
            currentUser= await db.Users.Include("Cohort").Where(u=>u.UserName==User.Identity.Name).FirstOrDefaultAsync();
            Dictionary<string,ColleagueRating> GetColleagueRating = await db.ColleagueRatings.Include("RatedStudent").Where(cr=>cr.RatingStudentId==currentUser.Id).ToDictionaryAsync(key=>key.RatedStudent.UserName);
            checkedStatus=availabilitiesParse(currentUser.TimePreferences);//Looks at the user's availabilties so that the boxes can be pre checked
            UserList = await db.Users.Include("RatedStudents")
                            .Where(
                                    u=>u.Cohort.CohortName==currentUser.Cohort.CohortName                       //Of the same Cohort as the current User
                                    &&
                                    u.Id!=currentUser.Id                                                        //But not including the current user themselves
                                    &&
                                    (
                                    u.RatedStudents
                                        .Where(r=>r.RatingStudentId==currentUser.Id).FirstOrDefault()==null     //Catches any new colleague that hadn't been initialized
                                    ||
                                    u.RatedStudents
                                        .Where(r=>r.RatingStudentId==currentUser.Id && r.Pref==Preferences.Neutral).FirstOrDefault()!=null) //Find (!=null) Users that are a "RatedStudent" in the Colleague Rating Model, and whose Rating student is the current student
                                    )    
                            .ToListAsync();                                                                     //if this search returns null, then this user hasn't been rated by the current user

            AvoidList = await db.Users.Include("RatedStudents")
                            .Where(
                                    u=>u.Cohort.CohortName==currentUser.Cohort.CohortName                       //Of the same Cohort as the current User
                                    &&
                                    u.Id!=currentUser.Id                                                        //But not including the current user themselves 
                                    && 
                                    u.RatedStudents                                                             //Find user that is a "RatedStudent" in the ColleagueRating Model ...
                                        .Where(r=>r.RatingStudent==currentUser &&r.Pref==Preferences.Avoid).FirstOrDefault()!=null) //... and who's rating student is the current user and where the preference is Avoid -- if this is not null, then we want to add that user to the avoid list
                                    .ToListAsync();
            
            PreferList = await db.Users.Include("RatedStudents")
                            .Where(
                                    u=>u.Cohort.CohortName==currentUser.Cohort.CohortName                       //Of the same Cohort as the current User
                                    &&
                                    u.Id!=currentUser.Id                                                        //But not including the current user themselves 
                                    && 
                                    u.RatedStudents
                                        .Where(r=>r.RatingStudent==currentUser &&r.Pref==Preferences.Prefer).FirstOrDefault()!=null)
                            .ToListAsync();
           //UserList = await db.Users.Where(u=>u.Cohort.CohortName==currentUser.Cohort.CohortName).ToListAsync();
        }

        //OVERLOAD for OnGetAsync -- PASSES RESULTS OF OnPostAync TO METHOD -- THIS SAVES TWO CALLS TO THE DB
        // public async Task OnGetAsync(ApplicationUser currUser,List<ApplicationUser> userList)
        // {
        //     currentUser= currUser;
        //     checkedStatus=availabilitiesParse(currentUser.TimePreferences);//Looks at the user's availabilties so that the boxes can be pre checked
        //     UserList = userList;
        // }

        public async Task<IActionResult> OnPostAsync()
        {   //!!!!!!!!!!!!!!!!INITIALIZATION!!!!!!!!!!!!!!!!!!!!!!!!!!!
            
            //SANITIZE INPUT: AVAILABILITES -- AVOID -- PREFER
            (string avail, HashSet<string> avoid, HashSet<string> prefer) sanitizedInput = SanitizeInputs(AreChecked,AvoidPseudoList,PreferredPseudoList);
            
            //LOAD CURRENT USER FROM DB
            currentUser= await db.Users.Include("Cohort").Where(u=>u.UserName==User.Identity.Name).FirstOrDefaultAsync();

            //COLLEAGUE RATING LIST WHERE RATING STUDENT IS THE CURRENT USER (these will be updated)
            //List<ColleagueRating> ratingCurrUserCR = await db.ColleagueRatings.Include("RatedStudent").Where(cr=>cr.RatingStudentId==currentUser.Id).ToListAsync();
            Dictionary<string,ColleagueRating> ratingCurrUserCR= await db.ColleagueRatings.Include("RatedStudent").Where(cr=>cr.RatingStudentId==currentUser.Id).ToDictionaryAsync(key=>key.RatedStudent.UserName);
            
            //LIST OF ALL CLASSMATES IN COHORT
            List<ApplicationUser> currUserColleagues = await db.Users.Include(r=>r.RatingStudents).Where(u=>u.Cohort.CohortName==currentUser.Cohort.CohortName && u.Id!=currentUser.Id).ToListAsync();

            //IF AVAIL IS DIFFERENT--> UPDATE COLLEAGUE RATING WHERE CURRENT USER IS THE RATED USER (since changing your avail will affect how people score you)
            if(currentUser.TimePreferences!=sanitizedInput.avail)
            {
                List<ColleagueRating> ratedCurrUserCR = await db.ColleagueRatings.Include("RatingStudent").Where(cr=>cr.RatedStudentId==currentUser.Id).ToListAsync(); //list of ColleagueRating where CurrUser is rated
                if(ratedCurrUserCR!=null)
                {
                    foreach (ColleagueRating cr in ratedCurrUserCR)
                    {
                        short newRating=await CalculateRating(cr.Pref,cr.RatingStudent.TimePreferences,currentUser);
                        cr.Rating=newRating;
                    }
                }
            }

            //UPDATE ALREADY EXISTING COLLEAGUERATING WHERE CURRENT USER IS RATING
            foreach (var(key,cr) in ratingCurrUserCR)
            {
                Preferences newPref;
                short newRating;
                string ratedStudentUsername=cr.RatedStudent.UserName;
                if(sanitizedInput.avoid.Contains(ratedStudentUsername))
                {
                    newPref=Preferences.Avoid;
                    newRating=await CalculateRating(newPref,sanitizedInput.avail,cr.RatedStudent);
                }
                else if(sanitizedInput.prefer.Contains(ratedStudentUsername))
                {
                    newPref=Preferences.Prefer;
                    newRating= await CalculateRating(newPref,sanitizedInput.avail,cr.RatedStudent);
                }
                else
                {
                    newPref=Preferences.Neutral;
                    newRating= await CalculateRating(newPref,sanitizedInput.avail,cr.RatedStudent);
                }
                //Updated the CR with the New Values
                cr.Rating=newRating;
                cr.Pref=newPref;
            }
            
            //CREATE COLLEAGUE RATINGS FOR COLLEAGUES THAT DON'T HAVE ONE YET
            //TODO: CREATE OPPOSITE CR WHERE RATED RATES CURR USER
            foreach(ApplicationUser c in currUserColleagues)
            {
                if(!ratingCurrUserCR.ContainsKey(c.UserName)) //if this search returns false, it means there is no ColleagueRating for this classmate
                {
                    if(sanitizedInput.prefer.Contains(c.UserName)) //Uses hashsets to determine in O(1) if the user to be created is prefered, avoid or neutral
                    {
                        await CreateRating(currentUser.Id,c,sanitizedInput.avail,Preferences.Prefer);
                    }
                    else if (sanitizedInput.avoid.Contains(c.UserName))
                    {
                        await CreateRating(currentUser.Id,c,sanitizedInput.avail,Preferences.Avoid);
                    }
                    else
                    {
                        await CreateRating(currentUser.Id,c,sanitizedInput.avail,Preferences.Neutral);
                    }
                    
                    //CREATE OPPOSITE CR WHERE RATED RATES CURR USER
                    // await CreateRating(c.Id,currentUser,c.TimePreferences,Preferences.Neutral);// TOFIX: Risk of Overwriting existing CR because 
                   
                    
                    
                }
            }

            currentUser.TimePreferences=sanitizedInput.avail;
            // await userManager.UpdateAsync(currentUser);
            await db.SaveChangesAsync();
            // await OnGetAsync(currentUser,currUserColleagues);
            await OnGetAsync();
            return Page();
        }

        public (string, HashSet<string>,HashSet<string>) SanitizeInputs (List<char> availabilities, string avoidString, string preferredString)
        {
            string availability=SanitizeAvailabilies(availabilities);
            HashSet<string> avoidArray = SanitizePreferences(avoidString).ToHashSet();
            HashSet<string> preferredArray = SanitizePreferences(preferredString).ToHashSet();
            return (availability, avoidArray, preferredArray);
        }

        public string SanitizeAvailabilies(List<char> availabilities)
        {
            return new string(availabilities.ToArray());
        }

        public string[] SanitizePreferences (string preferences)
        {
            if(preferences==null)return new string[]{};
            preferences=preferences.Trim();
            return preferences.Split(' ');
        }

        public bool [] availabilitiesParse(string avail)
        {
            bool [] morAftEve={false,false,false};
            if(avail==null)
            {
                return morAftEve;
            }
            
            foreach(char c in avail.ToCharArray())
            {
                if(c=='M') morAftEve[0]=true;
                if(c=='A') morAftEve[1]=true;
                if(c=='E') morAftEve[2]=true;
            }
            return morAftEve;
        }
        
        //FIXME: should be taking ratedID too
        public async Task CreateRating (string raterId, ApplicationUser rated, string raterTimePreferences, Preferences pref)
        {
            //ColleagueRating CR =await db.ColleagueRatings.Where(cr=>cr.RatingStudent.Id==raterId && cr.RatedStudent.UserName==ratedUserName).FirstOrDefaultAsync();
            //ApplicationUser rated = await userManager.FindByNameAsync(ratedUserName);

            short finalRating=await CalculateRating(pref,raterTimePreferences,rated);
            
            
            ColleagueRating CR= new ColleagueRating{RatingStudentId=raterId,RatedStudentId=rated.Id,Rating=finalRating, Pref=pref};
            // if(CR==null)
            // {
            //     CR= new ColleagueRating{RatingStudentId=raterId,RatedStudentId=rated.Id,Rating=finalRating};
            // }
            // else
            // {
            //     CR.Rating=finalRating;
            // }
            await db.ColleagueRatings.AddAsync(CR);
            await db.SaveChangesAsync();

            //TODO: INSERT A RATING VALUE FROM THE RATED TO THE RATING (this needs to be done to update in case t)
        }

        public async Task<short> CalculateRating(Preferences raterPref, string raterTimePref, ApplicationUser rated)
        {
            
                int timePrefBonus = ColleagueWeight.CountCommonTimePreference(raterTimePref,rated.TimePreferences); //up to +3 to rating for matching schedules
                int preferenceBonus=0;
                if(raterPref==Preferences.Prefer) preferenceBonus=3;
                if(raterPref==Preferences.Avoid) preferenceBonus=-10;
                int finalRating=preferenceBonus+timePrefBonus;
                return (short)finalRating;
        } 
    }
}
