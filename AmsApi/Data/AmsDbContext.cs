namespace AmsApi.Data
{
    public class AmsDbContext : DbContext
    {
        public AmsDbContext(DbContextOptions<AmsDbContext> options) : base(options) { }

        public DbSet<Attendee> Attendees => Set<Attendee>();
        public DbSet<Subject> Subjects => Set<Subject>();

        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<SubjectDate> SubjectDates => Set<SubjectDate>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
    }
}
