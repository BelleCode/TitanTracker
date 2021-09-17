using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models;
using TitanTracker.Models.Enums;


namespace TitanTracker.Data
{
    public static class DataUtility // Only want one so, make static
    {
        //Company Ids
        private static int Alphacompany1Id;

        private static int Betacompany2Id;
        private static int Charlecompany3Id;
        private static int Deltacompany4Id;
        private static int Echocompany5Id;

        public static string GetConnectionString(IConfiguration configuration)
        {
            //The default connection string will come from appSettings like usual
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            //It will be automatically overwritten if we are running on Heroku
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            using var svcScope = host.Services.CreateScope();
            var svcProvider = svcScope.ServiceProvider;
            //Service: An instance of RoleManager
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            //Service: An instance of RoleManager
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //Service: An instance of the UserManager
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BTUser>>();
            //Migration: This is the programmatic equivalent to Update-Database
            await dbContextSvc.Database.MigrateAsync();

            //Custom  Bug Tracker Seed Methods
            await SeedRolesAsync(roleManagerSvc);
            await SeedDefaultCompaniesAsync(dbContextSvc);
            await SeedDefaultUsersAsync(userManagerSvc);
            await SeedDemoUsersAsync(userManagerSvc);
            await SeedDefaultTicketTypeAsync(dbContextSvc);
            await SeedDefaultTicketStatusAsync(dbContextSvc);
            await SeedDefaultTicketPriorityAsync(dbContextSvc);
            await SeedDefaultProjectPriorityAsync(dbContextSvc);
            await SeedDefautProjectsAsync(dbContextSvc);
            await SeedDefautTicketsAsync(dbContextSvc);
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.ProgramManager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.ProjectManager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Developer.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Submitter.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.DemoUser.ToString()));
        }

        public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Company> defaultcompanies = new List<Company>() {
                    new Company() { Name = "Company1", Description="This is default Company 1" },
                    new Company() { Name = "Company2", Description="This is default Company 2" },
                    new Company() { Name = "Company3", Description="This is default Company 3" },
                    new Company() { Name = "Company4", Description="This is default Company 4" },
                    new Company() { Name = "Company5", Description="This is default Company 5" }
                };

                var dbCompanies = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultcompanies.Where(c => !dbCompanies.Contains(c.Name)));
                await context.SaveChangesAsync();

                //Get company Ids
                Alphacompany1Id = context.Companies.FirstOrDefault(p => p.Name == "Company1").Id;
                Betacompany2Id = context.Companies.FirstOrDefault(p => p.Name == "Company2").Id;
                Charlecompany3Id = context.Companies.FirstOrDefault(p => p.Name == "Company3").Id;
                Deltacompany4Id = context.Companies.FirstOrDefault(p => p.Name == "Company4").Id;
                Echocompany5Id = context.Companies.FirstOrDefault(p => p.Name == "Company5").Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Companies.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultProjectPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Models.ProjectPriority> projectPriorities = new List<ProjectPriority>() {
                    new ProjectPriority() { Name = BTProjectPriority.Low.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Medium.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Important.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Urgent.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Required.ToString() },
                };

                var dbProjectPriorities = context.ProjectPriorities.Select(c => c.Name).ToList();
                await context.ProjectPriorities.AddRangeAsync(projectPriorities.Where(c => !dbProjectPriorities.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Project Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefautProjectsAsync(ApplicationDbContext context)
        {
            //Get project priority Ids
            int priorityLow = context.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Low.ToString()).Id;
            int priorityMedium = context.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Medium.ToString()).Id;
            int priorityImportant = context.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Important.ToString()).Id;
            int priorityUrgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Urgent.ToString()).Id;
            int priorityRequired = context.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Required.ToString()).Id;

            try
            {
                IList<Project> projects = new List<Project>() {
                    #region AlphaCompany1Id
                    //Portfolio
                    new Project()
                     {
                         CompanyId = Alphacompany1Id,
                         Name = "Personal Porfolio - Web Application",
                         Description="Single page html, css & javascript page.  Serves as a landing page for candidates and contains a bio and links to all applications and challenges." ,
                         StartDate = new DateTime(2021,7,06),
                         EndDate = new DateTime(2021,7,06).AddMonths(4),
                         ProjectPriorityId = priorityRequired
                     },
                    // Personal Blog
                    new Project()
                     {
                         CompanyId = Alphacompany1Id,
                         Name = "Personal Blog -  Web Application",
                         Description="Candidate's custom built web application using .Net Core with MVC, a postgres database and hosted in a heroku container.  The app is designed for the candidate to create, update and maintain a live blog site.",
                         StartDate = new DateTime(2021,8,06),
                         EndDate = new DateTime(2021,8,06).AddMonths(2),
                         ProjectPriorityId = priorityMedium
                     },
                    // Issue Tracker
                    new Project()
                     {
                         CompanyId = Alphacompany1Id,
                         Name = "Issue Tracker - Web Application",
                         Description="A custom designed .Net Core application with postgres database.  The application is a multi tennent application designed to track issue tickets' progress.  Implemented with identity and user roles, Tickets are maintained in projects which are maintained by users in the role of projectmanager.  Each project has a team and team members.",
                         StartDate = new DateTime(2021,8,23),
                         EndDate = new DateTime(2021,8,23).AddMonths(1),
                         ProjectPriorityId = priorityImportant
                     },
                    // Address Book
                    new Project()
                     {
                         CompanyId = Alphacompany1Id,
                         Name = "Address Book - Web Application",
                         Description="A custom designed .Net Core application with postgres database.  This is an application to serve as a rolodex of contacts for a given user..",
                         StartDate = new DateTime(2021,7,18),
                         EndDate = new DateTime(2021,7,18).AddMonths(2),
                         ProjectPriorityId = priorityUrgent
                     },
                    // Bootstrap Invoice Lab
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "BootStrap Invoice Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // Movie App
                    new Project()
                     {
                         CompanyId = Alphacompany1Id,
                         Name = "Movie Information Web Application",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,24),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // Fizz Buzz
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Fizz Buzz",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(5),
                         ProjectPriorityId = priorityRequired
                     },
                    // SuperDog
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Super Dog",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(3),
                         ProjectPriorityId = priorityRequired
                     },
                    // Palidrome
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Palidrome",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // Hundo
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Hundo",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // Loan Calculator
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Loan Calculator",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // Sunset Hills
                    new Project()                                         {
                         CompanyId = Alphacompany1Id,
                         Name = "Sunset Hills",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // Hero Search
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Hero Search",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityMedium
                     },
                    // Bootstrap Grid Lab
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Bootstrap Grid Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // Bootstrap Invoice Lab
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "BootStrap Invoice Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // Bootstrap Carousel & Cards Lab
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Bootstrap Carousel & Cards Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // Rewind
                    new Project()                                         
                    {
                         CompanyId = Alphacompany1Id,
                         Name = "Rewind",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
#endregion
                    #region Betacompany2Id
                    // 2 Portfolio
                    new Project()
                     {
                         CompanyId = Betacompany2Id,
                         Name = "2 Personal Porfolio - Web Application",
                         Description="Single page html, css & javascript page.  Serves as a landing page for candidates and contains a bio and links to all applications and challenges." ,
                         StartDate = new DateTime(2021,7,06),
                         EndDate = new DateTime(2021,7,06).AddMonths(4),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Personal Blog
                    new Project()
                     {
                         CompanyId = Betacompany2Id,
                         Name = "2 Personal Blog -  Web Application",
                         Description="Candidate's custom built web application using .Net Core with MVC, a postgres database and hosted in a heroku container.  The app is designed for the candidate to create, update and maintain a live blog site.",
                         StartDate = new DateTime(2021,8,06),
                         EndDate = new DateTime(2021,8,06).AddMonths(2),
                         ProjectPriorityId = priorityMedium
                     },
                    // 2 Issue Tracker
                    new Project()
                     {
                         CompanyId = Betacompany2Id,
                         Name = "2 Issue Tracker - Web Application",
                         Description="A custom designed .Net Core application with postgres database.  The application is a multi tennent application designed to track issue tickets' progress.  Implemented with identity and user roles, Tickets are maintained in projects which are maintained by users in the role of projectmanager.  Each project has a team and team members.",
                         StartDate = new DateTime(2021,8,23),
                         EndDate = new DateTime(2021,8,23).AddMonths(1),
                         ProjectPriorityId = priorityImportant
                     },
                    // 2 Address Book
                    new Project()
                     {
                         CompanyId = Betacompany2Id,
                         Name = "2 Address Book - Web Application",
                         Description="A custom designed .Net Core application with postgres database.  This is an application to serve as a rolodex of contacts for a given user..",
                         StartDate = new DateTime(2021,7,18),
                         EndDate = new DateTime(2021,7,18).AddMonths(2),
                         ProjectPriorityId = priorityUrgent
                     },
                    // 2 Bootstrap Invoice Lab
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 BootStrap Invoice Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // 2 Movie App
                    new Project()
                     {
                         CompanyId = Betacompany2Id,
                         Name = "2 Movie Information Web Application",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,24),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Fizz Buzz
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Fizz Buzz",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(5),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 SuperDog
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Super Dog",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(3),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Palidrome
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Palidrome",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Hundo
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Hundo",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Loan Calculator
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Loan Calculator",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Sunset Hills
                    new Project()                                         {
                         CompanyId = Betacompany2Id,
                         Name = "2 Sunset Hills",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityRequired
                     },
                    // 2 Hero Search
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Hero Search",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityMedium
                     },
                    // 2 Bootstrap Grid Lab
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Bootstrap Grid Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // 2 Bootstrap Invoice Lab
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 BootStrap Invoice Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // 2 Bootstrap Carousel & Cards Lab
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Bootstrap Carousel & Cards Lab",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    // 2 Rewind
                    new Project()
                    {
                         CompanyId = Betacompany2Id,
                         Name = "2 Rewind",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,7,15),
                         EndDate = new DateTime(2021,7,24).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    #endregion
                };

                var dbProjects = context.Projects.Select(c => c.Name).ToList();
                await context.Projects.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Projects.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultUsersAsync(UserManager<BTUser> userManager)
        {
            //Seed Default Admin User
            #region AlphaCompany1Id
            var defaultUser = new BTUser
            {
                UserName = "btadmin1@bugtracker.com",
                Email = "btadmin1@bugtracker.com",
                FirstName = "Felcity",
                LastName = "Smoak",
                PreferredName = "Overwatch",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
            #endregion
            //Seed Default Admin User
            defaultUser = new BTUser
            {
                UserName = "btadmin2@bugtracker.com",
                Email = "btadmin2@bugtracker.com",
                FirstName = "Cisco",
                LastName = "Ramon",
                PreferredName = "Vibe",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProgramManager1 User
            defaultUser = new BTUser
            {
                UserName = "ProgramManager1@bugtracker.com",
                Email = "ProgramManager1@bugtracker.com",
                FirstName = "Kara",
                LastName = "Danvers",
                PreferredName = "SuperGirl",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProgramManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProgramManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProgramManager1-2 User
            defaultUser = new BTUser
            {
                UserName = "ProgramManager1-2@bugtracker.com",
                Email = "ProgramManager1-2@bugtracker.com",
                FirstName = "James",
                LastName = "Olsen",
                PreferredName = "Guardian",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProgramManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProgramManager1-2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProgramManager2 User
            defaultUser = new BTUser
            {
                UserName = "ProgramManager2@bugtracker.com",
                Email = "ProgramManager2@bugtracker.com",
                FirstName = "Barry",
                LastName = "Allen",
                PreferredName = "Flash",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProgramManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProgramManager2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProjectManager1 User
            defaultUser = new BTUser
            {
                UserName = "ProjectManager1@bugtracker.com",
                Email = "ProjectManager1@bugtracker.com",
                FirstName = "John",
                LastName = "Diggle",
                PreferredName = "Spartan",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProjectManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProjectManager1-2 User
            defaultUser = new BTUser
            {
                UserName = "ProjectManager1-2@bugtracker.com",
                Email = "ProjectManager1-2@bugtracker.com",
                FirstName = "Querl",
                LastName = "Dox",
                PreferredName = "Brainiac",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProgramManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProgramManager1-2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
            //Seed Default ProjectManager2 User
            defaultUser = new BTUser
            {
                UserName = "ProjectManager2@bugtracker.com",
                Email = "ProjectManager2@bugtracker.com",
                FirstName = "Caitlin",
                LastName = "Snow",
                PreferredName = "Killer Frost",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProjectManager2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default ProjectManager3 User
            defaultUser = new BTUser
            {
                UserName = "ProjectManager3@bugtracker.com",
                Email = "ProjectManager3@bugtracker.com",
                FirstName = "John",
                LastName = "Snow",
                PreferredName = "JS",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProjectManager3 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer1 User
            defaultUser = new BTUser
            {
                UserName = "Developer1@bugtracker.com",
                Email = "Developer1@bugtracker.com",
                FirstName = "Clark",
                LastName = "Kent",
                PreferredName = "SuperMan",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer2 User
            defaultUser = new BTUser
            {
                UserName = "Developer2@bugtracker.com",
                Email = "Developer2@bugtracker.com",
                FirstName = "Curtis",
                LastName = "Holt",
                PreferredName = "Mister Terrific",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer3 User
            defaultUser = new BTUser
            {
                UserName = "Developer3@bugtracker.com",
                Email = "Developer3@bugtracker.com",
                FirstName = "Natasha",
                LastName = "Romanoff",
                PreferredName = "Black Widow",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer3 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer4 User
            defaultUser = new BTUser
            {
                UserName = "Developer4@bugtracker.com",
                Email = "Developer4@bugtracker.com",
                FirstName = "Carol",
                LastName = "Danvers",
                PreferredName = "Captain Marvel",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer4 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer5 User
            defaultUser = new BTUser
            {
                UserName = "Developer5@bugtracker.com",
                Email = "Developer5@bugtracker.com",
                FirstName = "Clint",
                LastName = "Barton",
                PreferredName = "Hawkeye",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer5 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer6 User
            defaultUser = new BTUser
            {
                UserName = "Developer6@bugtracker.com",
                Email = "Developer6@bugtracker.com",
                FirstName = "Bruce",
                LastName = "Banner",
                PreferredName = "Hulk",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer5 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Submitter1 User
            defaultUser = new BTUser
            {
                UserName = "Submitter1@bugtracker.com",
                Email = "Submitter1@bugtracker.com",
                FirstName = "Scott",
                LastName = "Lang",
                PreferredName = "Ant-Man",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Submitter1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Submitter2 User
            defaultUser = new BTUser
            {
                UserName = "Submitter2@bugtracker.com",
                Email = "Submitter2@bugtracker.com",
                FirstName = "Peter",
                LastName = "Parker",
                PreferredName = "Spider-Man",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Submitter2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDemoUsersAsync(UserManager<BTUser> userManager)
        {
            //Seed Demo Admin User
            var defaultUser = new BTUser
            {
                UserName = "demoadmin@bugtracker.com",
                Email = "demoadmin@bugtracker.com",
                FirstName = "Demo",
                LastName = "Admin",
                PreferredName = "DemoAdmin",
                EmailConfirmed = true,
                CompanyId = Alphacompany1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Demo ProjectManager User
            defaultUser = new BTUser
            {
                UserName = "demopm@bugtracker.com",
                Email = "demopm@bugtracker.com",
                FirstName = "Demo",
                LastName = "ProjectManager",
                PreferredName = "DemoPM",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo ProjectManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Demo Developer User
            defaultUser = new BTUser
            {
                UserName = "demodev@bugtracker.com",
                Email = "demodev@bugtracker.com",
                FirstName = "Demo",
                LastName = "Developer",
                PreferredName = "DemoDev",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Demo Submitter User
            defaultUser = new BTUser
            {
                UserName = "demosub@bugtracker.com",
                Email = "demosub@bugtracker.com",
                FirstName = "Demo",
                LastName = "Submitter",
                PreferredName = "DemoSub",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Submitter User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Demo New User
            defaultUser = new BTUser
            {
                UserName = "demonew@bugtracker.com",
                Email = "demonew@bugtracker.com",
                FirstName = "Demo",
                LastName = "NewUser",
                PreferredName = "NewUserD",
                EmailConfirmed = true,
                CompanyId = Betacompany2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo New User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>() {
                     new TicketType() { Name = BTTicketType.FeatureDefect.ToString() },         // Something doesn't work as designed
                     new TicketType() { Name = BTTicketType.Performance.ToString() },           // Feature works as designed, but is too slow or too demanding on other resources
                     new TicketType() { Name = BTTicketType.Polish.ToString()},                 // Feature works well, but is "rough around the edges", and has imperfections or cosmetic issues which impact perception
                     new TicketType() { Name = BTTicketType.Security.ToString() },              // Feature works but is security is lack or data is vulnerable
                     new TicketType() { Name = BTTicketType.Usability.ToString() },             // Feature works as designed, but is difficult for the user to use or undiscoverable
                     new TicketType() { Name = BTTicketType.Accessibility.ToString() },         // Feature is not ADA compliant
                };

                var dbTicketTypes = context.TicketTypes.Select(c => c.Name).ToList();
                await context.TicketTypes.AddRangeAsync(ticketTypes.Where(c => !dbTicketTypes.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Types.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketStatusAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketStatus> ticketStatuses = new List<TicketStatus>() {
                    new TicketStatus() { Name = BTTicketStatus.New.ToString() },                    // 01 - unassigned just in and adding defect info
                    new TicketStatus() { Name = BTTicketStatus.DefectReview.ToString() },           // 02 - Assigned to someone for reviewDefect needs analysed to verify it is a bug
                    new TicketStatus() { Name = BTTicketStatus.ClosedDuplicate.ToString()  },       // 03 - Was this issue already raised
                    new TicketStatus() { Name = BTTicketStatus.ClosedRejected.ToString()  },        // 04 - Is it a valid defect
                    new TicketStatus() { Name = BTTicketStatus.Committed.ToString()  },             // 07 - Ready for work in this sprint/iteration
                    new TicketStatus() { Name = BTTicketStatus.Deffered.ToString()  },              // 05 - TODO: DEFFERED BUTTON!!!! -> Send this to the next iteration and set as Defect Review Is it within scope move backlog
                    new TicketStatus() { Name = BTTicketStatus.DevReady.ToString()  },              // 06 - Ticket approved and waiting for iteration/sprint
                    new TicketStatus() { Name = BTTicketStatus.InDev.ToString()  },                 // 08 - Assigned and Dev Work in Progress status: doing
                    new TicketStatus() { Name = BTTicketStatus.InTest.ToString()  },                // 10 - Assigned and testing test cases
                    new TicketStatus() { Name = BTTicketStatus.DevCompleteTestReady.ToString()  },  // 09 - Code is Fixed but not tested
                    new TicketStatus() { Name = BTTicketStatus.ResolvedFixed.ToString()  },         // 11 - Code is fixed
                };

                var dbTicketStatuses = context.TicketStatuses.Select(c => c.Name).ToList();
                await context.TicketStatuses.AddRangeAsync(ticketStatuses.Where(c => !dbTicketStatuses.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Statuses.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketPriority> ticketPriorities = new List<TicketPriority>() {
                                                    new TicketPriority() { Name = BTTicketPriority.Low.ToString()  },
                                                    new TicketPriority() { Name = BTTicketPriority.Medium.ToString() },
                                                    new TicketPriority() { Name = BTTicketPriority.High.ToString()},
                                                    new TicketPriority() { Name = BTTicketPriority.Urgent.ToString()},
                                                    new TicketPriority() { Name = BTTicketPriority.Breaking.ToString()},
                };

                var dbTicketPriorities = context.TicketPriorities.Select(c => c.Name).ToList();
                await context.TicketPriorities.AddRangeAsync(ticketPriorities.Where(c => !dbTicketPriorities.Contains(c.Name)));
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefautTicketsAsync(ApplicationDbContext context)
        {
            //Get project Ids
            int portfolioId = context.Projects.FirstOrDefault(p => p.Name == "Build a Personal Porfolio").Id;
            int blogId = context.Projects.FirstOrDefault(p => p.Name == "Build a supplemental Blog Web Application").Id;
            int bugtrackerId = context.Projects.FirstOrDefault(p => p.Name == "Build an Issue Tracking Web Application").Id;
            int movieId = context.Projects.FirstOrDefault(p => p.Name == "Build a Movie Information Web Application").Id;
            int FizzBuzzId = context.Projects.FirstOrDefault(p => p.Name == "Build Fizz Buzz").Id;

            //Get ticket type Ids
            BTTicketType typeFeatureDefect = BTTicketType.FeatureDefect;
            BTTicketType typePerformance = BTTicketType.Performance;
            BTTicketType typePolish = BTTicketType.Polish;
            BTTicketType typeSecurity = BTTicketType.Security;
            BTTicketType typeUsability = BTTicketType.Usability;

            //Get ticket priority Ids
            BTTicketPriority priorityLow = BTTicketPriority.Low;            // Pri : 4 Review before next iteration. Remove if not Pri 3 +
            BTTicketPriority priorityMedium = BTTicketPriority.Medium;      // Pri : 3 Product can ship without this item, but should be addressed before next release
            BTTicketPriority priorityHigh = BTTicketPriority.High;          // Pri : 2 Product should not ship without successful resolution to the work item, but it doesn not have to be addressed in this iteration
            BTTicketPriority priorityUrgent = BTTicketPriority.Urgent;      // Pri : 1 Product should not ship without successful resolution
            BTTicketPriority priorityBreaking = BTTicketPriority.Breaking;  // Pri : 0 Product is live and this is a breaking Bug

            //Get ticket status Ids
            BTTicketStatus statusNew = BTTicketStatus.New;                                      // 01 - unassigned just in and adding defect info
            BTTicketStatus statusDev = BTTicketStatus.DefectReview;                             // 02 - Assigned to someone for reviewDefect needs analysed to verify it is a bug
            BTTicketStatus statusClosedDuplicate = BTTicketStatus.ClosedDuplicate;              // 03 - Was this issue already raised
            BTTicketStatus statusClosedRejected = BTTicketStatus.ClosedRejected;                // 04 - Is it a valid defect (obsolete, non-reproducible, incomplete data)
            BTTicketStatus statusCommitted = BTTicketStatus.Committed;                          // 07 - Ready for work in this sprint/iteration
            BTTicketStatus statusDeffered = BTTicketStatus.Deffered;                            // 05 - TODO: DEFFERED BUTTON!!!! -> Send this to the next iteration and set as Defect Review Is it within scope move backlog
            BTTicketStatus statusDevReady = BTTicketStatus.DevReady;                            // 06 - Ticket approved and waiting for iteration/sprint
            BTTicketStatus statusInDev = BTTicketStatus.InDev;                                  // 08 - Assigned and Dev Work in Progress status: doing
            BTTicketStatus statusInTest = BTTicketStatus.InTest;                                // 10 - Assigned and testing test cases
            BTTicketStatus statusDevCompleteTestReady = BTTicketStatus.DevCompleteTestReady;    // 09 - Code is Fixed but not tested
            BTTicketStatus statusResolvedFixed = BTTicketStatus.ResolvedFixed;                  // 11 - Code is fixed

            try
            {
                IList<Ticket> tickets = new List<Ticket>() {
                        //PORTFOLIO
                        
                        new Ticket () {Title = "	Porfolio Ticket 1	",  Description = "	Ticket details for portfolio ticket 1	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusCommitted , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 2	",  Description = "	Ticket details for portfolio ticket 2	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDeffered  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 3	",  Description = "	Ticket details for portfolio ticket 3	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusClosedDuplicate   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 4	",  Description = "	Ticket details for portfolio ticket 4	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusClosedRejected    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 5	",  Description = "	Ticket details for portfolio ticket 5	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 6	",  Description = "	Ticket details for portfolio ticket 6	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 7	",  Description = "	Ticket details for portfolio ticket 7	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 8	",  Description = "	Ticket details for portfolio ticket 8	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 9	",  Description = "	Ticket details for portfolio ticket 9	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 10	",  Description = "	Ticket details for portfolio ticket 10	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 11	",  Description = "	Ticket details for portfolio ticket 11	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 12	",  Description = "	Ticket details for portfolio ticket 12	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 13	",  Description = "	Ticket details for portfolio ticket 13	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 14	",  Description = "	Ticket details for portfolio ticket 14	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 15	",  Description = "	Ticket details for portfolio ticket 15	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 16	",  Description = "	Ticket details for portfolio ticket 16	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 17	",  Description = "	Ticket details for portfolio ticket 17	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 18	",  Description = "	Ticket details for portfolio ticket 18	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 19	",  Description = "	Ticket details for portfolio ticket 19	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 20	",  Description = "	Ticket details for portfolio ticket 20	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 21	",  Description = "	Ticket details for portfolio ticket 21	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 22	",  Description = "	Ticket details for portfolio ticket 22	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 23	",  Description = "	Ticket details for portfolio ticket 23	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 24	",  Description = "	Ticket details for portfolio ticket 24	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 25	",  Description = "	Ticket details for portfolio ticket 25	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 26	",  Description = "	Ticket details for portfolio ticket 26	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 27	",  Description = "	Ticket details for portfolio ticket 27	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 28	",  Description = "	Ticket details for portfolio ticket 28	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 29	",  Description = "	Ticket details for portfolio ticket 29	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 30	",  Description = "	Ticket details for portfolio ticket 30	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 31	",  Description = "	Ticket details for portfolio ticket 31	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 32	",  Description = "	Ticket details for portfolio ticket 32	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 33	",  Description = "	Ticket details for portfolio ticket 33	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 34	",  Description = "	Ticket details for portfolio ticket 34	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 35	",  Description = "	Ticket details for portfolio ticket 35	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 36	",  Description = "	Ticket details for portfolio ticket 36	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 37	",  Description = "	Ticket details for portfolio ticket 37	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 38	",  Description = "	Ticket details for portfolio ticket 38	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 39	",  Description = "	Ticket details for portfolio ticket 39	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 40	",  Description = "	Ticket details for portfolio ticket 40	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 41	",  Description = "	Ticket details for portfolio ticket 41	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 42	",  Description = "	Ticket details for portfolio ticket 42	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 43	",  Description = "	Ticket details for portfolio ticket 43	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 44	",  Description = "	Ticket details for portfolio ticket 44	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 45	",  Description = "	Ticket details for portfolio ticket 45	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 46	",  Description = "	Ticket details for portfolio ticket 46	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 47	",  Description = "	Ticket details for portfolio ticket 47	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 48	",  Description = "	Ticket details for portfolio ticket 48	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 49	",  Description = "	Ticket details for portfolio ticket 49	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 50	",  Description = "	Ticket details for portfolio ticket 50	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 51	",  Description = "	Ticket details for portfolio ticket 51	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 52	",  Description = "	Ticket details for portfolio ticket 52	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 53	",  Description = "	Ticket details for portfolio ticket 53	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 54	",  Description = "	Ticket details for portfolio ticket 54	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 55	",  Description = "	Ticket details for portfolio ticket 55	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 56	",  Description = "	Ticket details for portfolio ticket 56	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 57	",  Description = "	Ticket details for portfolio ticket 57	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 58	",  Description = "	Ticket details for portfolio ticket 58	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 59	",  Description = "	Ticket details for portfolio ticket 59	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 60	",  Description = "	Ticket details for portfolio ticket 60	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 61	",  Description = "	Ticket details for portfolio ticket 61	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 62	",  Description = "	Ticket details for portfolio ticket 62	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 63	",  Description = "	Ticket details for portfolio ticket 63	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 64	",  Description = "	Ticket details for portfolio ticket 64	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 65	",  Description = "	Ticket details for portfolio ticket 65	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 66	",  Description = "	Ticket details for portfolio ticket 66	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 67	",  Description = "	Ticket details for portfolio ticket 67	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 68	",  Description = "	Ticket details for portfolio ticket 68	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 69	",  Description = "	Ticket details for portfolio ticket 69	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 70	",  Description = "	Ticket details for portfolio ticket 70	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 71	",  Description = "	Ticket details for portfolio ticket 71	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 72	",  Description = "	Ticket details for portfolio ticket 72	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 73	",  Description = "	Ticket details for portfolio ticket 73	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 74	",  Description = "	Ticket details for portfolio ticket 74	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 75	",  Description = "	Ticket details for portfolio ticket 75	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 76	",  Description = "	Ticket details for portfolio ticket 76	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 77	",  Description = "	Ticket details for portfolio ticket 77	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 78	",  Description = "	Ticket details for portfolio ticket 78	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 79	",  Description = "	Ticket details for portfolio ticket 79	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 80	",  Description = "	Ticket details for portfolio ticket 80	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 81	",  Description = "	Ticket details for portfolio ticket 81	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 82	",  Description = "	Ticket details for portfolio ticket 82	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 83	",  Description = "	Ticket details for portfolio ticket 83	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 84	",  Description = "	Ticket details for portfolio ticket 84	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 85	",  Description = "	Ticket details for portfolio ticket 85	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 86	",  Description = "	Ticket details for portfolio ticket 86	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 87	",  Description = "	Ticket details for portfolio ticket 87	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 88	",  Description = "	Ticket details for portfolio ticket 88	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 89	",  Description = "	Ticket details for portfolio ticket 89	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 90	",  Description = "	Ticket details for portfolio ticket 90	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 91	",  Description = "	Ticket details for portfolio ticket 91	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 92	",  Description = "	Ticket details for portfolio ticket 92	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 93	",  Description = "	Ticket details for portfolio ticket 93	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 94	",  Description = "	Ticket details for portfolio ticket 94	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 95	",  Description = "	Ticket details for portfolio ticket 95	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Porfolio Ticket 96	",  Description = "	Ticket details for portfolio ticket 96	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Porfolio Ticket 97	",  Description = "	Ticket details for portfolio ticket 97	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Porfolio Ticket 98	",  Description = "	Ticket details for portfolio ticket 98	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Porfolio Ticket 99	",  Description = "	Ticket details for portfolio ticket 99	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Porfolio Ticket 100	",  Description = "	Ticket details for portfolio ticket 100	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeUsability   },

                   //BLOG
                        new Ticket () {Title = "	Blog Ticket 1	",  Description = "	Ticket details for portfolio ticket 1	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusCommitted , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 2	",  Description = "	Ticket details for portfolio ticket 2	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDeffered  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 3	",  Description = "	Ticket details for portfolio ticket 3	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusClosedDuplicate   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 4	",  Description = "	Ticket details for portfolio ticket 4	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusClosedRejected    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 5	",  Description = "	Ticket details for portfolio ticket 5	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 6	",  Description = "	Ticket details for portfolio ticket 6	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 7	",  Description = "	Ticket details for portfolio ticket 7	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 8	",  Description = "	Ticket details for portfolio ticket 8	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 9	",  Description = "	Ticket details for portfolio ticket 9	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 10	",  Description = "	Ticket details for portfolio ticket 10	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 11	",  Description = "	Ticket details for portfolio ticket 11	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 12	",  Description = "	Ticket details for portfolio ticket 12	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 13	",  Description = "	Ticket details for portfolio ticket 13	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 14	",  Description = "	Ticket details for portfolio ticket 14	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 15	",  Description = "	Ticket details for portfolio ticket 15	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 16	",  Description = "	Ticket details for portfolio ticket 16	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 17	",  Description = "	Ticket details for portfolio ticket 17	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 18	",  Description = "	Ticket details for portfolio ticket 18	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 19	",  Description = "	Ticket details for portfolio ticket 19	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 20	",  Description = "	Ticket details for portfolio ticket 20	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 21	",  Description = "	Ticket details for portfolio ticket 21	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 22	",  Description = "	Ticket details for portfolio ticket 22	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 23	",  Description = "	Ticket details for portfolio ticket 23	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 24	",  Description = "	Ticket details for portfolio ticket 24	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 25	",  Description = "	Ticket details for portfolio ticket 25	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 26	",  Description = "	Ticket details for portfolio ticket 26	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 27	",  Description = "	Ticket details for portfolio ticket 27	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 28	",  Description = "	Ticket details for portfolio ticket 28	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 29	",  Description = "	Ticket details for portfolio ticket 29	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 30	",  Description = "	Ticket details for portfolio ticket 30	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 31	",  Description = "	Ticket details for portfolio ticket 31	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 32	",  Description = "	Ticket details for portfolio ticket 32	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 33	",  Description = "	Ticket details for portfolio ticket 33	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 34	",  Description = "	Ticket details for portfolio ticket 34	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 35	",  Description = "	Ticket details for portfolio ticket 35	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 36	",  Description = "	Ticket details for portfolio ticket 36	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 37	",  Description = "	Ticket details for portfolio ticket 37	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 38	",  Description = "	Ticket details for portfolio ticket 38	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 39	",  Description = "	Ticket details for portfolio ticket 39	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 40	",  Description = "	Ticket details for portfolio ticket 40	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 41	",  Description = "	Ticket details for portfolio ticket 41	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 42	",  Description = "	Ticket details for portfolio ticket 42	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 43	",  Description = "	Ticket details for portfolio ticket 43	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 44	",  Description = "	Ticket details for portfolio ticket 44	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 45	",  Description = "	Ticket details for portfolio ticket 45	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 46	",  Description = "	Ticket details for portfolio ticket 46	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 47	",  Description = "	Ticket details for portfolio ticket 47	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 48	",  Description = "	Ticket details for portfolio ticket 48	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 49	",  Description = "	Ticket details for portfolio ticket 49	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 50	",  Description = "	Ticket details for portfolio ticket 50	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 51	",  Description = "	Ticket details for portfolio ticket 51	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 52	",  Description = "	Ticket details for portfolio ticket 52	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 53	",  Description = "	Ticket details for portfolio ticket 53	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 54	",  Description = "	Ticket details for portfolio ticket 54	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 55	",  Description = "	Ticket details for portfolio ticket 55	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 56	",  Description = "	Ticket details for portfolio ticket 56	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 57	",  Description = "	Ticket details for portfolio ticket 57	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 58	",  Description = "	Ticket details for portfolio ticket 58	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 59	",  Description = "	Ticket details for portfolio ticket 59	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 60	",  Description = "	Ticket details for portfolio ticket 60	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 61	",  Description = "	Ticket details for portfolio ticket 61	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 62	",  Description = "	Ticket details for portfolio ticket 62	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 63	",  Description = "	Ticket details for portfolio ticket 63	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 64	",  Description = "	Ticket details for portfolio ticket 64	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 65	",  Description = "	Ticket details for portfolio ticket 65	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 66	",  Description = "	Ticket details for portfolio ticket 66	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 67	",  Description = "	Ticket details for portfolio ticket 67	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 68	",  Description = "	Ticket details for portfolio ticket 68	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 69	",  Description = "	Ticket details for portfolio ticket 69	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 70	",  Description = "	Ticket details for portfolio ticket 70	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 71	",  Description = "	Ticket details for portfolio ticket 71	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 72	",  Description = "	Ticket details for portfolio ticket 72	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 73	",  Description = "	Ticket details for portfolio ticket 73	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 74	",  Description = "	Ticket details for portfolio ticket 74	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 75	",  Description = "	Ticket details for portfolio ticket 75	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 76	",  Description = "	Ticket details for portfolio ticket 76	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 77	",  Description = "	Ticket details for portfolio ticket 77	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 78	",  Description = "	Ticket details for portfolio ticket 78	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 79	",  Description = "	Ticket details for portfolio ticket 79	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 80	",  Description = "	Ticket details for portfolio ticket 80	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 81	",  Description = "	Ticket details for portfolio ticket 81	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 82	",  Description = "	Ticket details for portfolio ticket 82	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 83	",  Description = "	Ticket details for portfolio ticket 83	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 84	",  Description = "	Ticket details for portfolio ticket 84	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 85	",  Description = "	Ticket details for portfolio ticket 85	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 86	",  Description = "	Ticket details for portfolio ticket 86	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 87	",  Description = "	Ticket details for portfolio ticket 87	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 88	",  Description = "	Ticket details for portfolio ticket 88	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 89	",  Description = "	Ticket details for portfolio ticket 89	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 90	",  Description = "	Ticket details for portfolio ticket 90	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 91	",  Description = "	Ticket details for portfolio ticket 91	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 92	",  Description = "	Ticket details for portfolio ticket 92	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 93	",  Description = "	Ticket details for portfolio ticket 93	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 94	",  Description = "	Ticket details for portfolio ticket 94	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 95	",  Description = "	Ticket details for portfolio ticket 95	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Blog Ticket 96	",  Description = "	Ticket details for portfolio ticket 96	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Blog Ticket 97	",  Description = "	Ticket details for portfolio ticket 97	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Blog Ticket 98	",  Description = "	Ticket details for portfolio ticket 98	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Blog Ticket 99	",  Description = "	Ticket details for portfolio ticket 99	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Blog Ticket 100	",  Description = "	Ticket details for portfolio ticket 100	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeUsability   },

                        //BUGTRACKER

                        new Ticket () {Title = "	Bug Tracker Ticket 1	",  Description = "	Ticket details for portfolio ticket 1	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusCommitted , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 2	",  Description = "	Ticket details for portfolio ticket 2	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDeffered  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 3	",  Description = "	Ticket details for portfolio ticket 3	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusClosedDuplicate   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 4	",  Description = "	Ticket details for portfolio ticket 4	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusClosedRejected    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 5	",  Description = "	Ticket details for portfolio ticket 5	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 6	",  Description = "	Ticket details for portfolio ticket 6	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 7	",  Description = "	Ticket details for portfolio ticket 7	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 8	",  Description = "	Ticket details for portfolio ticket 8	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 9	",  Description = "	Ticket details for portfolio ticket 9	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 10	",  Description = "	Ticket details for portfolio ticket 10	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 11	",  Description = "	Ticket details for portfolio ticket 11	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 12	",  Description = "	Ticket details for portfolio ticket 12	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 13	",  Description = "	Ticket details for portfolio ticket 13	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 14	",  Description = "	Ticket details for portfolio ticket 14	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 15	",  Description = "	Ticket details for portfolio ticket 15	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 16	",  Description = "	Ticket details for portfolio ticket 16	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 17	",  Description = "	Ticket details for portfolio ticket 17	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 18	",  Description = "	Ticket details for portfolio ticket 18	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 19	",  Description = "	Ticket details for portfolio ticket 19	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 20	",  Description = "	Ticket details for portfolio ticket 20	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 21	",  Description = "	Ticket details for portfolio ticket 21	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 22	",  Description = "	Ticket details for portfolio ticket 22	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 23	",  Description = "	Ticket details for portfolio ticket 23	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 24	",  Description = "	Ticket details for portfolio ticket 24	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 25	",  Description = "	Ticket details for portfolio ticket 25	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 26	",  Description = "	Ticket details for portfolio ticket 26	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 27	",  Description = "	Ticket details for portfolio ticket 27	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 28	",  Description = "	Ticket details for portfolio ticket 28	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 29	",  Description = "	Ticket details for portfolio ticket 29	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 30	",  Description = "	Ticket details for portfolio ticket 30	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 31	",  Description = "	Ticket details for portfolio ticket 31	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 32	",  Description = "	Ticket details for portfolio ticket 32	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityLow , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 33	",  Description = "	Ticket details for portfolio ticket 33	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 34	",  Description = "	Ticket details for portfolio ticket 34	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 35	",  Description = "	Ticket details for portfolio ticket 35	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 36	",  Description = "	Ticket details for portfolio ticket 36	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 37	",  Description = "	Ticket details for portfolio ticket 37	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 38	",  Description = "	Ticket details for portfolio ticket 38	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 39	",  Description = "	Ticket details for portfolio ticket 39	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 40	",  Description = "	Ticket details for portfolio ticket 40	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 41	",  Description = "	Ticket details for portfolio ticket 41	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 42	",  Description = "	Ticket details for portfolio ticket 42	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 43	",  Description = "	Ticket details for portfolio ticket 43	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 44	",  Description = "	Ticket details for portfolio ticket 44	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 45	",  Description = "	Ticket details for portfolio ticket 45	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 46	",  Description = "	Ticket details for portfolio ticket 46	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 47	",  Description = "	Ticket details for portfolio ticket 47	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 48	",  Description = "	Ticket details for portfolio ticket 48	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 49	",  Description = "	Ticket details for portfolio ticket 49	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 50	",  Description = "	Ticket details for portfolio ticket 50	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 51	",  Description = "	Ticket details for portfolio ticket 51	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 52	",  Description = "	Ticket details for portfolio ticket 52	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 53	",  Description = "	Ticket details for portfolio ticket 53	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 54	",  Description = "	Ticket details for portfolio ticket 54	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 55	",  Description = "	Ticket details for portfolio ticket 55	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 56	",  Description = "	Ticket details for portfolio ticket 56	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 57	",  Description = "	Ticket details for portfolio ticket 57	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 58	",  Description = "	Ticket details for portfolio ticket 58	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 59	",  Description = "	Ticket details for portfolio ticket 59	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 60	",  Description = "	Ticket details for portfolio ticket 60	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 61	",  Description = "	Ticket details for portfolio ticket 61	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 62	",  Description = "	Ticket details for portfolio ticket 62	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 63	",  Description = "	Ticket details for portfolio ticket 63	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 64	",  Description = "	Ticket details for portfolio ticket 64	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 65	",  Description = "	Ticket details for portfolio ticket 65	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 66	",  Description = "	Ticket details for portfolio ticket 66	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 67	",  Description = "	Ticket details for portfolio ticket 67	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 68	",  Description = "	Ticket details for portfolio ticket 68	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 69	",  Description = "	Ticket details for portfolio ticket 69	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 70	",  Description = "	Ticket details for portfolio ticket 70	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 71	",  Description = "	Ticket details for portfolio ticket 71	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 72	",  Description = "	Ticket details for portfolio ticket 72	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 73	",  Description = "	Ticket details for portfolio ticket 73	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 74	",  Description = "	Ticket details for portfolio ticket 74	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusResolvedFixed , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 75	",  Description = "	Ticket details for portfolio ticket 75	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusNew   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 76	",  Description = "	Ticket details for portfolio ticket 76	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDev   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 77	",  Description = "	Ticket details for portfolio ticket 77	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDevReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 78	",  Description = "	Ticket details for portfolio ticket 78	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInDev , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 79	",  Description = "	Ticket details for portfolio ticket 79	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInTest    , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 80	",  Description = "	Ticket details for portfolio ticket 80	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 81	",  Description = "	Ticket details for portfolio ticket 81	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusResolvedFixed , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 82	",  Description = "	Ticket details for portfolio ticket 82	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusNew   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 83	",  Description = "	Ticket details for portfolio ticket 83	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDev   , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 84	",  Description = "	Ticket details for portfolio ticket 84	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusDevReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 85	",  Description = "	Ticket details for portfolio ticket 85	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInDev , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 86	",  Description = "	Ticket details for portfolio ticket 86	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusInTest    , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 87	",  Description = "	Ticket details for portfolio ticket 87	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 88	",  Description = "	Ticket details for portfolio ticket 88	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusResolvedFixed , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 89	",  Description = "	Ticket details for portfolio ticket 89	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusNew   , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 90	",  Description = "	Ticket details for portfolio ticket 90	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDev   , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 91	",  Description = "	Ticket details for portfolio ticket 91	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusDevReady  , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 92	",  Description = "	Ticket details for portfolio ticket 92	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInDev , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 93	",  Description = "	Ticket details for portfolio ticket 93	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusInTest    , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 94	",  Description = "	Ticket details for portfolio ticket 94	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevCompleteTestReady  , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 95	",  Description = "	Ticket details for portfolio ticket 95	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusResolvedFixed , TicketType =  typeUsability   },
                        new Ticket () {Title = "	Bug Tracker Ticket 96	",  Description = "	Ticket details for portfolio ticket 96	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusNew   , TicketType =  typeFeatureDefect   },
                        new Ticket () {Title = "	Bug Tracker Ticket 97	",  Description = "	Ticket details for portfolio ticket 97	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityMedium  , TicketStatus =    statusDev   , TicketType =  typePerformance },
                        new Ticket () {Title = "	Bug Tracker Ticket 98	",  Description = "	Ticket details for portfolio ticket 98	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityHigh    , TicketStatus =    statusDevReady  , TicketType =  typePolish  },
                        new Ticket () {Title = "	Bug Tracker Ticket 99	",  Description = "	Ticket details for portfolio ticket 99	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityUrgent  , TicketStatus =    statusInDev , TicketType =  typeSecurity    },
                        new Ticket () {Title = "	Bug Tracker Ticket 100	",  Description = "	Ticket details for portfolio ticket 100	", Created =    DateTimeOffset.Now  , ProjectId =   portfolioId , TicketPriority =  priorityBreaking    , TicketStatus =    statusInTest    , TicketType =  typeUsability   },

                                //new Ticket() {Title = "Bug Tracker Ticket 1", Description = "Ticket details for bug tracker ticket 1", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 2", Description = "Ticket details for bug tracker ticket 2", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 3", Description = "Ticket details for bug tracker ticket 3", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 4", Description = "Ticket details for bug tracker ticket 4", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 5", Description = "Ticket details for bug tracker ticket 5", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 6", Description = "Ticket details for bug tracker ticket 6", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 7", Description = "Ticket details for bug tracker ticket 7", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 8", Description = "Ticket details for bug tracker ticket 8", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 9", Description = "Ticket details for bug tracker ticket 9", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 10", Description = "Ticket details for bug tracker 10", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 11", Description = "Ticket details for bug tracker 11", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 12", Description = "Ticket details for bug tracker 12", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 13", Description = "Ticket details for bug tracker 13", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 14", Description = "Ticket details for bug tracker 14", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 15", Description = "Ticket details for bug tracker 15", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 16", Description = "Ticket details for bug tracker 16", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 17", Description = "Ticket details for bug tracker 17", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 18", Description = "Ticket details for bug tracker 18", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 19", Description = "Ticket details for bug tracker 19", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 20", Description = "Ticket details for bug tracker 20", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 21", Description = "Ticket details for bug tracker 21", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 22", Description = "Ticket details for bug tracker 22", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 23", Description = "Ticket details for bug tracker 23", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 24", Description = "Ticket details for bug tracker 24", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 25", Description = "Ticket details for bug tracker 25", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 26", Description = "Ticket details for bug tracker 26", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 27", Description = "Ticket details for bug tracker 27", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 28", Description = "Ticket details for bug tracker 28", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 29", Description = "Ticket details for bug tracker 29", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Bug Tracker Ticket 30", Description = "Ticket details for bug tracker 30", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                ////MOVIE
                                //new Ticket() {Title = "Movie Ticket 1", Description = "Ticket details for movie ticket 1", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityLow, TicketStatus = statusNew, TicketType = typePolish},
                                //new Ticket() {Title = "Movie Ticket 2", Description = "Ticket details for movie ticket 2", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityMedium, TicketStatus = statusDev, TicketType = typeSecurity},
                                //new Ticket() {Title = "Movie Ticket 3", Description = "Ticket details for movie ticket 3", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeUsability},
                                //new Ticket() {Title = "Movie Ticket 4", Description = "Ticket details for movie ticket 4", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityUrgent, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Movie Ticket 5", Description = "Ticket details for movie ticket 5", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityLow, TicketStatus = statusDev,  TicketType = typePolish},
                                //new Ticket() {Title = "Movie Ticket 6", Description = "Ticket details for movie ticket 6", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityMedium, TicketStatus = statusNew,  TicketType = typeSecurity},
                                //new Ticket() {Title = "Movie Ticket 7", Description = "Ticket details for movie ticket 7", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusNew, TicketType = typeUsability},
                                //new Ticket() {Title = "Movie Ticket 8", Description = "Ticket details for movie ticket 8", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityUrgent, TicketStatus = statusDev,  TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Movie Ticket 9", Description = "Ticket details for movie ticket 9", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityLow, TicketStatus = statusNew,  TicketType = typePolish},
                                //new Ticket() {Title = "Movie Ticket 10", Description = "Ticket details for movie ticket 10", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityMedium, TicketStatus = statusNew, TicketType = typeSecurity},
                                //new Ticket() {Title = "Movie Ticket 11", Description = "Ticket details for movie ticket 11", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusDev,  TicketType = typeUsability},
                                //new Ticket() {Title = "Movie Ticket 12", Description = "Ticket details for movie ticket 12", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityUrgent, TicketStatus = statusNew,  TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Movie Ticket 13", Description = "Ticket details for movie ticket 13", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityLow, TicketStatus = statusNew, TicketType = typePolish},
                                //new Ticket() {Title = "Movie Ticket 14", Description = "Ticket details for movie ticket 14", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityMedium, TicketStatus = statusDev,  TicketType = typeSecurity},
                                //new Ticket() {Title = "Movie Ticket 15", Description = "Ticket details for movie ticket 15", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusNew,  TicketType = typeUsability},
                                //new Ticket() {Title = "Movie Ticket 16", Description = "Ticket details for movie ticket 16", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityUrgent, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Movie Ticket 17", Description = "Ticket details for movie ticket 17", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusDev,  TicketType = typeFeatureDefect},
                                //new Ticket() {Title = "Movie Ticket 18", Description = "Ticket details for movie ticket 18", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityMedium, TicketStatus = statusDev,  TicketType = typeSecurity},
                                //new Ticket() {Title = "Movie Ticket 19", Description = "Ticket details for movie ticket 19", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityHigh, TicketStatus = statusNew,  TicketType = typeUsability},
                                //new Ticket() {Title = "Movie Ticket 20", Description = "Ticket details for movie ticket 20", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriority = priorityUrgent, TicketStatus = statusNew, TicketType = typeFeatureDefect},
                };

                var dbTickets = context.Tickets.Select(c => c.Title).ToList();
                await context.Tickets.AddRangeAsync(tickets.Where(c => !dbTickets.Contains(c.Title)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Tickets.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }
    }
}