using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyBuddy.Data;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyBuddy.Pages.TeamBuilder
{
    public class TeamBuilderAlgo
    {
        public static async Task<List<Team>>  TeamBuilderAlgorithm(StudyBuddyDbContext db, int teamSize, string cohortName)
        {
            //All the students in the affected cohort
            List<ApplicationUser> Students = await db.Users.Include(u=>u.RatingStudents).Where(u=>u.Cohort.CohortName==cohortName).ToListAsync();

            //Quick reference dictionary to find ratings
            Dictionary<string,Dictionary<string,int>> ReferenceDictionary = new Dictionary<string,Dictionary<string,int>>();

            //List of Usernames -- Inputed to Classroom Obj
            HashSet<string> StudentUsernames = new HashSet<string>();

            foreach(ApplicationUser s in Students)
            {
                Dictionary<string,int> SubDictionary= new Dictionary<string,int>();
                StudentUsernames.Add(s.UserName);
                foreach(ColleagueRating r in s.RatingStudents)
                {
                    SubDictionary.Add(r.RatedStudent.UserName,r.Rating);
                }
                ReferenceDictionary.Add(s.UserName,SubDictionary);
            }

            //INITIALIZATION PHASE
            //Create 4 random classes
            List<Classroom> classrooms=new List<Classroom>();
            for(int i=0;i<4;i++)
            {
                classrooms.Add(new Classroom(teamSize,StudentUsernames));
            }

            Classroom bestClassroom=classrooms[0].DeepClone();

            //LOOP BEGINS
            for(int generation=0;generation<1000;generation++)
            {
                //SELECTION PHASE
                for(int i=0;i<classrooms.Count();i++)
                {
                    classrooms[i].CalculateClassRating(ReferenceDictionary);                
                }

                classrooms=classrooms.OrderBy(c=>c.ClassRating*-1).ToList(); //orders classrooms from highest rated to lowest

                //keeps tabs on the highest classroom score
                if(classrooms[0].ClassRating>bestClassroom.ClassRating)
                {
                    bestClassroom=classrooms[0].DeepClone();
                    bestClassroom.CalculateClassRating(ReferenceDictionary);
                }

                //CROSSOVER PHASE
                //Classroom0 overwrites Classroom3 and Classroom 0
                classrooms[3]=classrooms[0].Crossover(0); //Crossover(0) means we seed the best team into the new gen
                classrooms[0]=classrooms[0].Crossover(1);

                //Classroom1 overwrites Classroom2 and Classroom 1
                classrooms[2]=classrooms[1].Crossover(0);
                classrooms[1]=classrooms[1].Crossover(1);

                //MUTATION PHASE
                classrooms[0]=classrooms[0].Mutate(1); //Make sure to mutate the team that was crossed over, refer back to crossover phase
                classrooms[1]=classrooms[1].Mutate(1);
                classrooms[2]=classrooms[2].Mutate(0);
                classrooms[3]=classrooms[3].Mutate(0);
                
            }

            return bestClassroom.Teams;

        }
    }

    public class Classroom
    {
        private HashSet<string>Usernames; //searches faster and all usernames are unique anyways
        public List<Team> Teams{get;set;}
        public int ClassRating{get;set;}

        public Classroom()
        {
            Usernames=new HashSet<string>();
            Teams=new List<Team>();
        }
        //Initial initialization
        public Classroom(int TeamSize, HashSet<string>Usernames)
        {
            this.Usernames=Usernames;
            this.Teams=new List<Team>();
            CreateTeams(TeamSize);
            PopulateTeams();
        }

        //For Crossover Initialization
        public Classroom(HashSet<string> usernames, List<Team> teams)
        {
            this.Usernames=usernames;
            this.Teams=teams;
        }

        public Classroom DeepClone()
        {
            return JsonSerializer.Deserialize<Classroom>(JsonSerializer.Serialize(this,this.GetType()));
        }
        private void CreateTeams(int TeamSize)
        {
            //TODO EXCEPTIONS FOR WHEN TEAMSIZE IS LARGER THAN NUMOFSTUDENTS
            //TODO EXCEPTION WHEN YOU CAN'T SATISFY THE SMALLER TEAM CONDITION

            //Number of students in Cohort
            double numOfStudents=(double) this.Usernames.Count();
            
            //Check if teams divide with no leftover
            if(numOfStudents%TeamSize==0)
            {
                //Create evenly sized teams
                for(int numOfTeams=0; numOfTeams<numOfStudents/TeamSize;numOfTeams++)
                {
                    Teams.Add(new Team(TeamSize));
                }
            }
            else
            {
                
                double numOfBigTeams=Math.Floor(numOfStudents/TeamSize); 
                //calculation to find how many teams we would need of size Teamsize-1
                double numOfSmallTeams=(numOfStudents-numOfBigTeams*TeamSize)/(TeamSize-1);
                
                while(numOfSmallTeams%1!=0)
                {
                    numOfBigTeams--;
                    numOfSmallTeams=(numOfStudents-numOfBigTeams*TeamSize)/(TeamSize-1);
                }

                for(int i=0; i<numOfBigTeams;i++)
                {
                    Teams.Add(new Team(TeamSize));
                }

                for(int j=0; j<numOfSmallTeams;j++)
                {
                    Teams.Add(new Team(TeamSize-1));
                }

            }

            this.Teams=Teams;
        }

        //Will randomly Populate the teams -- Probably only used in initialization
        public async void PopulateTeams()
        {
            Random rnd = new Random();
            // Stack<string> temp = new Stack<string>(this.Usernames.OrderBy(c=>rnd.Next())); //SUPER WEIRD behaviour in the do-while loop
            List<string> temp = new List<string>(this.Usernames.OrderBy(c=>rnd.Next()));
            int teamIndex=0;
            int listIndex=0;
            do
            {
                for(int j=0;j<this.Teams[teamIndex].TeamSize;j++)
                {
                    this.Teams[teamIndex].Teammates.Add(temp[listIndex]);
                    if(temp.Count()==listIndex+1)break;
                    listIndex++;
                }
                teamIndex++;
            }while(temp.Count()-(listIndex+1)!=0);
        }

        //SELECTION METHOD - Score Classroom
        public void CalculateClassRating(Dictionary<string,Dictionary<string,int>> ReferenceDictionary)
        {
            ClassRating=0;
            foreach(Team t in Teams)
            {
                t.CalculateTeamRating(ReferenceDictionary);
                ClassRating+=t.TeamRating;
            }
        }

        //Mutates Crossed-over classroom - teamToMutate is either a 0 or 1, assumes that teams are ordered descendingly by score
        public Classroom Mutate(int teamToMutate)
        {
            //Creates Copy of Teams
            List<Team> mutatedTeams=new List<Team>(this.Teams);
            
            Random rnd = new Random();
            
            //Finds one student from the target team (should be highest or second highest scoring team) and the Index
            int exchangeStudent1Index=rnd.Next(mutatedTeams[teamToMutate].TeamSize);
            string exchangeStudent1=mutatedTeams[teamToMutate].Teammates[exchangeStudent1Index];
            
            //Finds another team to trade the exchange1 student
            int teamToMutateWith=teamToMutate;
            
            while(teamToMutateWith==teamToMutate)
            {
                teamToMutateWith=rnd.Next(Teams.Count());
            }
            
            //Name and Index of second student
            int exchangeStudent2Index = rnd.Next(mutatedTeams[teamToMutateWith].TeamSize);
            string exchangeStudent2= mutatedTeams[teamToMutateWith].Teammates[exchangeStudent2Index];

            //Student Exchange! hahaha
            mutatedTeams[teamToMutate].Teammates[exchangeStudent1Index]=exchangeStudent2;
            mutatedTeams[teamToMutateWith].Teammates[exchangeStudent2Index]=exchangeStudent1;

            //Return mutated Classroom
            Classroom mutatedClassroom = new Classroom(this.Usernames,mutatedTeams);
            return mutatedClassroom;
        }

        //ELEGANT CODE -- teamToKeep is either 0 or 1, referring to either the first or the second highest scoring team
        public Classroom Crossover(int teamToKeep)
        {
                //Identify index of highest rated team
                //int largestTeamRatingIndex=Int32.MinValue;
                //int largestTeamRating=Int32.MinValue;
                //Ranks teams by descending order - i.e. the largest is at index [0]
                List<Team> orderedTeam=new List<Team>(this.Teams.OrderBy(t=>-1*t.TeamRating).ToList());
                
                // for(int i=0;i<Teams.Count();i++)
                // {
                //     if(Teams[i].TeamRating>largestTeamRating)
                //     {
                //         largestTeamRatingIndex=i;
                //         largestTeamRating=Teams[i].TeamRating;
                //     }
                // }

                //Create a HashSet that EXCLUDES the members of the highest rated team (for loop)
                HashSet<string> studentsToExclude=new HashSet<string>(Usernames);

                //Loop through the highest rated team and removes it's students from the studentsToExclude set
                foreach(string t in orderedTeam[teamToKeep].Teammates)
                {
                    studentsToExclude.Remove(t);
                }

                //Create Randomizer
                Random rnd = new Random();

                //Shuffle remaining students
                Stack<string> studentsToExcludeStack = new Stack<string>(studentsToExclude.OrderBy(s=>rnd.Next()).ToList());
                try{
                    //Repopulate Teams -- Starts from second largest team i=1
                    for(int i=0;i<orderedTeam.Count();i++)
                    {
                        //skips modifying the team to keep
                        if(i==teamToKeep)continue;
                        for(int teamsize=0;teamsize<orderedTeam[i].TeamSize;teamsize++)
                        {
                            orderedTeam[i].Teammates[teamsize]=studentsToExcludeStack.Pop();
                        }
                    }
                }catch(InvalidOperationException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                //Create a Classroom object
                Classroom crossoverOtherClassroom = new Classroom(this.Usernames,orderedTeam);
                return crossoverOtherClassroom;
        }

    }

    public class Team:ICloneable
    {
        public Team(int TeamSize)
        {
            this.TeamSize=TeamSize;
            Teammates=new List<string>();
        }
        public Team Clone() { return (Team)this.MemberwiseClone(); }
        object ICloneable.Clone() { return Clone(); }
        public List<string> Teammates{get;set;}
        public int TeamSize{get;set;}
        public int TeamRating{get;set;}

        public void CalculateTeamRating(Dictionary<string,Dictionary<string,int>> ReferenceDictionary)
        {
            TeamRating=0;//initializes TeamRating
            foreach(string t1 in Teammates)
            {
                foreach(string t2 in Teammates)
                {
                    if(t1.Equals(t2))continue;//skips the student comparing himself

                    TeamRating+=ReferenceDictionary.GetValueOrDefault(t1).GetValueOrDefault(t2); //Finds rating of t1 towards t2 in O(1) time, default of an int, which is 0
                }
            }
        }
    }
}