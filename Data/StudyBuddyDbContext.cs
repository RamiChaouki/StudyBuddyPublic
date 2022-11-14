using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using StudyBuddy.Models;

namespace StudyBuddy.Data
{
    public class StudyBuddyDbContext : IdentityDbContext<ApplicationUser>
    {
        public StudyBuddyDbContext(DbContextOptions<StudyBuddyDbContext> options) : base(options) { }

        public StudyBuddyDbContext() { }

        public DbSet<ColleagueRating> ColleagueRatings { get; set; }
        public DbSet<ColleagueWeight> ColleagueWeights { get; set; }

        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<Forum> Forums { get; set; }

        public DbSet<Cohort> Cohorts { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostImage> PostImages { get; set; }






        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ColleagueWeight>()
                        .HasOne(cw => cw.WeightingStudent)
                        .WithMany(u => u.WeightingStudents)
                        .HasForeignKey(cw => cw.WeightingStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ColleagueWeight>()
                        .HasOne(cw => cw.WeightedStudent)
                        .WithMany(u => u.WeightedStudents)
                        .HasForeignKey(cw => cw.WeightedStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ColleagueRating>()
                        .HasOne(cr => cr.RatingStudent)
                        .WithMany(u => u.RatingStudents)
                        .HasForeignKey(cr => cr.RatingStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ColleagueRating>()
                        .HasOne(cr => cr.RatedStudent)
                        .WithMany(u => u.RatedStudents)
                        .HasForeignKey(cr => cr.RatedStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                        .HasOne(u => u.Cohort)
                        .WithMany(co => co.Students)
                        .HasForeignKey(co => co.CohortId)
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Forum>()
                        .HasOne(f => f.Cohort)
                        .WithMany(co => co.Forums)
                        .HasForeignKey(co => co.CohortId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Post>()
                        .HasOne(p => p.Forum)
                        .WithMany(f => f.Posts)
                        .HasForeignKey(co => co.ForumId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Post>()
                        .HasOne(p => p.ParentPost)
                        .WithMany(p => p.Replies)
                        .HasForeignKey(r => r.ParentId)
                        .OnDelete(DeleteBehavior.NoAction);

            // modelBuilder.Entity<Post>()
            //                     .HasOne(p => p.Author)
            //                     .WithMany(a => a.Posts)
            //                     // .HasForeignKey(p => p.AuthorId)
            //                     .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PostImage>()
                                     .HasOne(p => p.Post)
                                     .WithMany(p => p.PostImages)
                                     .HasForeignKey(co => co.PostId)
                                     .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ColleagueRating>()
                        .HasOne(cr => cr.RatingStudent)
                        .WithMany(u => u.RatingStudents)
                        .HasForeignKey(cr => cr.RatingStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ColleagueRating>()
                        .HasOne(cr => cr.RatedStudent)
                        .WithMany(u => u.RatedStudents)
                        .HasForeignKey(cr => cr.RatedStudentId)
                        .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }


}
