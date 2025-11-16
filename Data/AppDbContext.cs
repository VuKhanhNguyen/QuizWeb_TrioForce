using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using quizweb.Models;
namespace quizweb.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {
        }

        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<AnsweredQuestion> AnsweredQuestions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<MarkedQuestion> MarkedQuestions { get; set; } = null!;
        public DbSet<ProgressQuestionSet> ProgressQuestionSets { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<QuestionSet> QuestionSets { get; set; } = null!;
        public DbSet<Ranking> Rankings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Answer>(e =>
            {
                e.HasKey(e => e.AnswerId);
                e.Property(e => e.AnswerText)
                    .IsRequired()
                    .HasMaxLength(500);

                e.HasOne(e => e.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AnsweredQuestion>(e =>
            {
                e.HasKey(a => new { a.UserName, a.QSetId, a.QuestionId });
                e.Property(a => a.SelectedAnswerId)
                    .IsRequired();

                e.HasOne(a => a.User)
                    .WithMany(u => u.AnsweredQuestions)
                    .HasForeignKey(a => a.UserName)
                    .HasPrincipalKey(u => u.UserName)
                    .OnDelete(DeleteBehavior.Cascade); // khi xóa User, xóa hết AnsweredQuestion

                e.HasOne(a => a.QuestionSet)
                    .WithMany(qs => qs.AnsweredQuestions)
                    .HasForeignKey(a => a.QSetId)
                    .OnDelete(DeleteBehavior.Restrict); // khi xóa QuestionSet, không xóa AnsweredQuestion

                e.HasOne(a => a.Question)
                    .WithMany()
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict); // khi xóa Question, không xóa AnsweredQuestion

                e.HasOne(a => a.Answer)
                    .WithMany()
                    .HasForeignKey(a => a.SelectedAnswerId)
                    .OnDelete(DeleteBehavior.Restrict); // khi xóa Answer, không xóa AnsweredQuestion

            });

            builder.Entity<ApplicationUser>(e =>
            {
                //e.HasIndex(u => u.UserName)
                //    .IsUnique();
                e.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(u => u.BirthDay)
                    .HasColumnType("date");
            });

            builder.Entity<Category>(e =>
            {
                e.HasKey(c => c.CategoryId);
                e.Property(c => c.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);
            });


            builder.Entity<Level>(e =>
            {
                e.HasKey(e => e.LevelId);
                e.Property(l => l.LevelName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            builder.Entity<MarkedQuestion>(e =>
            {
                e.HasKey(e => new { e.UserName, e.QuestionId });

                e.HasOne(m => m.User) //
                    .WithMany(u => u.MarkedQuestions)
                    .HasForeignKey(m => m.UserName)
                    .HasPrincipalKey(u => u.UserName)
                    .OnDelete(DeleteBehavior.Cascade); // khi xóa User, xóa hết MarkedQuestion

                e.HasOne(m => m.Question)
                    .WithMany() //  ONE question can be marked by many users
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);  // chuyển từ Cascade sang Restrict (hạn chế) bời vì gặp lỗi cascade paths
            });

            builder.Entity<ProgressQuestionSet>(e =>
            {
                e.HasKey(e => new { e.UserName, e.QSetId });

                e.HasOne(e => e.User)
                    .WithMany(u => u.ProgressQuestionSets)
                    .HasForeignKey(e => e.UserName)
                    .HasPrincipalKey(u => u.UserName)
                    .OnDelete(DeleteBehavior.Cascade); // khi xóa User, xóa hết ProgressQuestionSet

                e.HasOne(e => e.QuestionSet)
                    .WithMany(qs => qs.ProgressQuestionSets)
                    .HasForeignKey(e => e.QSetId)
                    .OnDelete(DeleteBehavior.Restrict); // khi xóa QuestionSet, không xóa ProgressQuestionSet

                e.HasOne(e => e.Question)
                    .WithMany() //  ONE question can be in many ProgressQuestionSets
                    .HasForeignKey(e => e.QuestionLastId)
                    .OnDelete(DeleteBehavior.Restrict); // khi xóa Question, không xóa ProgressQuestionSet
            });

            builder.Entity<Question>(e =>
            {
                e.HasKey(q => q.QuestionId);
                e.Property(q => q.QuestionText)
                    .IsRequired()
                    .HasMaxLength(1000);
                e.HasOne(q => q.QuestionSet)
                    .WithMany(qs => qs.Questions)
                    .HasForeignKey(q => q.QSetId);
               
            });

            builder.Entity<QuestionSet>(e =>
            {
                e.HasKey(qs => qs.QSetId);
                e.Property(qs => qs.QSetName)
                    .IsRequired()
                    .HasMaxLength(100);
                e.Property(qs => qs.Description)
                    .IsRequired()
                    .HasMaxLength(500);
                e.Property(qs => qs.AuthorName)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasOne(qs => qs.Author)
                    .WithMany(u => u.QuestionSets)
                    .HasForeignKey(qs => qs.AuthorName)
                    .HasPrincipalKey(u => u.UserName);
                e.HasOne(qs => qs.Level)
                    .WithMany(l => l.QuestionSets)
                    .HasForeignKey(qs => qs.LevelId);
                e.HasOne(qs => qs.Category)
                    .WithMany(c => c.QuestionSets)
                    .HasForeignKey(qs => qs.CategoryId);
            });

            builder.Entity<Ranking>(e =>
            {
                e.HasKey(r => r.UserName);

                e.HasOne(r => r.User)
                    .WithOne(u => u.Ranking)
                    .HasForeignKey<Ranking>(r => r.UserName)
                    .HasPrincipalKey<ApplicationUser>(u => u.UserName);
            });

        }
    }
}
