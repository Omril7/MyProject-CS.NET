﻿using Microsoft.EntityFrameworkCore;
using static TelHai.CS.ServerAPI.Models.Question;

namespace TelHai.CS.ServerAPI.Models
{
    public class ExamContext : DbContext
    {
        public ExamContext(DbContextOptions<ExamContext> options) : base(options)
        {
        }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Questions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

        }
    }

}