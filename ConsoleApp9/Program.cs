using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var db = new CompanyContext())
            {
                db.Database.EnsureCreated();

                if (!db.Companies.Any())
                {
                    Company company1 = new Company { Name = "Microsoft" };
                    Company company2 = new Company { Name = "Google" };

                    Employee emp1 = new Employee { FullName = "Игорь", Company = company1 };
                    Employee emp2 = new Employee { FullName = "Ольга", Company = company1 };
                    Employee emp3 = new Employee { FullName = "Степан", Company = company2 };

                    Project pr1 = new Project { Title = "CRM System" };
                    Project pr2 = new Project { Title = "Mobile App" };

                    emp1.Projects.Add(pr1);
                    emp1.Projects.Add(pr2);

                    emp2.Projects.Add(pr1);

                    emp3.Projects.Add(pr2);

                    db.AddRange(company1, company2);
                    db.AddRange(emp1, emp2, emp3);
                    db.AddRange(pr1, pr2);

                    db.SaveChanges();
                }

                Console.Write("Введите название компании ");
                string companyName = Console.ReadLine();

                var projects = db.Employees
                    .Where(e => e.Company.Name == companyName)
                    .SelectMany(e => e.Projects)
                    .Distinct()
                    .ToList();

                Console.WriteLine("Проекты:");

                foreach (var project in projects)
                {
                    Console.WriteLine(project.Title);
                }
            }

            Console.ReadKey();
        }
    }
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }

        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
    }
    public class Project
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }

        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
    public class CompanyContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=CompanyDB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Projects)
                .WithMany(p => p.Employees)
                .UsingEntity(j => j.ToTable("EmployeeProjects"));
        }
    }
}
