using AmsApi.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AmsApi.Data
{
    public class AmsDbContext : IdentityDbContext<AppUser> // بدل User ممكن تستخدم أي Class يمثل الـ User بتاعك
    {
        public AmsDbContext(DbContextOptions<AmsDbContext> options) : base(options) { }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<SubjectDate> SubjectDates { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendeeSubject> AttendeeSubjects { get; set; }  // DbSet for Many-to-Many Relationship

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-Many relationship between Attendee and Subject
            modelBuilder.Entity<AttendeeSubject>()
            .HasKey(at => new { at.AttendeeId, at.SubjectId });  // Composite Key for the join table

            modelBuilder.Entity<AttendeeSubject>()
                .HasOne(at => at.Attendee)
                .WithMany(a => a.AttendeeSubjects)
                .HasForeignKey(at => at.AttendeeId);  // Foreign Key to Attendee

            modelBuilder.Entity<AttendeeSubject>()
                .HasOne(at => at.Subject)
                .WithMany(s => s.AttendeeSubjects)
                .HasForeignKey(at => at.SubjectId);  // Foreign Key to Subject

            // One-to-Many relationship between Instructor and Subject
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Instructor)
                .WithMany(i => i.Subjects)
                .HasForeignKey(s => s.InstructorId);

            //Seed data admin
            modelBuilder.Entity<Admin>().HasData(
       new Admin
       {
           Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
           Name = "Mohamed Mostafa",
           Email = "Mohamed@gomai.com",
           Password = "123456",
           CreateAt = new DateTime(2025, 5, 12, 0, 0, 0, DateTimeKind.Utc),
           UpdatedAt = new DateTime(2025, 5, 12, 0, 0, 0, DateTimeKind.Utc)
       },
       new Admin
       {
           Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
           Name = "Mohamed Osman",
           Email = "Osman@gmail.com",
           Password = "12345",
           CreateAt = new DateTime(2025, 5, 12, 0, 0, 0, DateTimeKind.Utc),
           UpdatedAt = new DateTime(2025, 5, 12, 0, 0, 0, DateTimeKind.Utc)
       }    
   );

        }
    }
}
