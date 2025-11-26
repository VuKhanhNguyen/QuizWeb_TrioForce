using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Implementations;
using QuizWeb_TrioForce.Repositories.Interfaces;

using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.Services.Implementations;

namespace QuizWeb_TrioForce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
            });
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<AppDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.MaxAge = null;
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //DI for repositories
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ILevelRepository, LevelRepository>();
            builder.Services.AddScoped<IMarkedQuestionRepository, MarkedQuestionRepository>();
            builder.Services.AddScoped<IProgressQuestionSetRepository, ProgressQuestionSetRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IQuestionSetRepository, QuestionSetRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRankingRepository, RankingRepository>();
            builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
            builder.Services.AddScoped<IAnsweredQuestionRepository, AnsweredQuestionRepository>();
            //DI for services
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ILevelService, LevelService>();
            builder.Services.AddScoped<IMarkedQuestionService, MarkedQuestionService>();
            builder.Services.AddScoped<IProgressQuestionSetService, ProgressQuestionSetService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuestionSetService, QuestionSetService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRankingService, RankingService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IAnsweredQuestionService, AnsweredQuestionService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();
            app.Run();
        }
    }
}
