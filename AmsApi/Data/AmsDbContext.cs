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
        public DbSet<SubjectDate> SubjectDates { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendeeSubject> AttendeeSubjects { get; set; }  // DbSet for Many-to-Many Relationship
        public DbSet<Setting> Settings { get; set; }
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
        }
    }
}
