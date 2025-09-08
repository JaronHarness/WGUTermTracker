using SQLite;
using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Services;

public class SQLiteDatabaseService
{
    public SQLiteAsyncConnection Connection = null!;
    private bool hasDbBeenInitialized = false;
    private bool hasSeedDataBeenInserted = false;

    private async Task InitDb()
    {
        // Checks if DB has already been set up
        if (hasDbBeenInitialized) return;

        var sqlitePath = Path.Combine(FileSystem.AppDataDirectory, "termtracker.db3");
        Connection = new SQLiteAsyncConnection(
            sqlitePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache
            // ReadWrite: Opens file for reading and writing
            // Create: Creates the DB file if not already created
            // SharedCashe: Improves performance when multiple connections are used
        );

        // Create Tables
        await Connection.CreateTableAsync<Term>();
        await Connection.CreateTableAsync<Course>();
        await Connection.CreateTableAsync<Assessment>();

        hasDbBeenInitialized = true;
    }
    
    public async Task AddTermAsync(string title, DateTime startDate, DateTime endDate)
    {
        await InitDb();

        var newTerm = new Term
        {
            Title = title,
            StartDate = startDate,
            EndDate = endDate
        };
        await Connection.InsertAsync(newTerm);
    }

    public async Task<int> AddCourseAsync(string title, CourseStatus status, DateTime startDate, DateTime endDate, string notes, bool enableNotifications, string instructorName, string instructorPhone, string instructorEmail, int termIdString)
    {
        await InitDb();

        var newCourse = new Course
        {
            Title = title,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Notes = notes,
            EnableNotifications = enableNotifications,
            InstructorName = instructorName,
            InstructorPhone = instructorPhone,
            InstructorEmail = instructorEmail,
            TermId = termIdString
        };
        await Connection.InsertAsync(newCourse);
        return newCourse.Id;
    }

    public async Task<int> AddAssessmentAsync(string title, AssessmentType assessmentType, DateTime startDate, DateTime endDate, bool enableNotifications, int courseId)
    {
        await InitDb();

        var newAssessment = new Assessment
        {
            Title = title,
            Type = assessmentType,
            StartDate = startDate,
            EndDate = endDate,
            EnableNotifications = enableNotifications,
            CourseId = courseId
        };
        await Connection.InsertAsync(newAssessment);
        return newAssessment.Id;
    }

    public async Task DeleteTermAsync(int termId)
    {
        await InitDb();

        // Delete associated courses first
        var courses = await Connection.Table<Course>().Where(c => c.TermId == termId).ToListAsync();
        foreach (var course in courses)
        {
            // Delete associated assessments for each course
            var assessments = await Connection.Table<Assessment>().Where(a => a.CourseId == course.Id).ToListAsync();
            foreach (var assessment in assessments)
            {
                await Connection.DeleteAsync(assessment);
            }
            await Connection.DeleteAsync(course);
        }

        // Then delete the term
        await Connection.DeleteAsync<Term>(termId);
    }

    public async Task DeleteCourseAsync(int courseId)
    {
        await InitDb();

        var assessments = await Connection.Table<Assessment>().Where(a => a.CourseId == courseId).ToListAsync();
        foreach (var assessment in assessments)
        {
            await Connection.DeleteAsync(assessment);
        }

        await Connection.DeleteAsync<Course>(courseId);
    }

    public async Task DeleteAssessmentAsync(int assessmentId)
    {
        await InitDb();
        await Connection.DeleteAsync<Assessment>(assessmentId);
    }

    public async Task UpdateTermAsync(int termId, string title, DateTime startDate, DateTime endDate)
    {
        await InitDb();

        var term = await Connection.FindAsync<Term>(termId);
        
        term.Id = termId;
        term.Title = title;
        term.StartDate = startDate;
        term.EndDate = endDate;

        await Connection.UpdateAsync(term);
        
    }

    public async Task UpdateCourseAsync(int courseId, string title, CourseStatus status, DateTime startDate, DateTime endDate, string notes, bool enableNotifications, string instructorName, string instructorPhone, string instructorEmail, int termIdString)
    {
        await InitDb();

        var course = await Connection.FindAsync<Course>(courseId);
        
        course.Id = courseId;
        course.Title = title;
        course.Status = status;
        course.StartDate = startDate;
        course.EndDate = endDate;
        course.Notes = notes;
        course.EnableNotifications = enableNotifications;
        course.InstructorName = instructorName;
        course.InstructorPhone = instructorPhone;
        course.InstructorEmail = instructorEmail;
        course.TermId = termIdString;

        await Connection.UpdateAsync(course);
    }

    public async Task UpdateAssessmentAsync(int assessmentId, string title, AssessmentType assessmentType, DateTime startDate, DateTime endDate, bool enableNotifications, int courseId)
    {
        await InitDb();

        var assessment = await Connection.FindAsync<Assessment>(assessmentId);
        
        assessment.Id = assessmentId;
        assessment.Title = title;
        assessment.Type = assessmentType;
        assessment.StartDate = startDate;
        assessment.EndDate = endDate;
        assessment.EnableNotifications = enableNotifications;
        assessment.CourseId = courseId;

        await Connection.UpdateAsync(assessment);
    }

    public async Task SeedTermDataAsync()
    {
        if (hasSeedDataBeenInserted)
        {
            return;
        }

        hasSeedDataBeenInserted = true;

        await InitDb();
               
        if (await Connection.Table<Term>().CountAsync() > 0)
        {
            return;
        }

        var term1 = new Term
        {
            Title = "Term 1",
            StartDate = new DateTime(2025, 4, 01),
            EndDate = new DateTime(2025, 9, 30)
        };
        await Connection.InsertAsync(term1);

        await SeedCourseDataAsync(term1.Id);
    }

    public async Task SeedCourseDataAsync(int term1Id)
    {
        await InitDb();

        if (await Connection.Table<Course>().CountAsync() > 0)
        {
            return;
        }

        var course1 = new Course
        {
            Title = "C968: Mobile App Dev - C#",
            Status = CourseStatus.Active,
            StartDate = new DateTime(2025, 4, 01),
            EndDate = new DateTime(2025, 9, 30),
            Notes = "Learn mobile development using C#.",
            EnableNotifications = true,
            InstructorName = "Anika Patel",
            InstructorPhone = "555-123-4567",
            InstructorEmail = "anika.patel@strimeuniversity.edu",
            TermId = term1Id
        };
        await Connection.InsertAsync(course1);

        await SeedAssessmentDataAsync(course1.Id);
    }

    public async Task SeedAssessmentDataAsync(int course1Id)
    {
        await InitDb();

        if (await Connection.Table<Assessment>().CountAsync() > 0)
        {
            return;
        }

        var assessment1 = new Assessment
        {
            Title = "Performance Assessment 1",
            Type = AssessmentType.Performance,
            StartDate = new DateTime(2025, 4, 01),
            EndDate = new DateTime(2025, 6, 30),
            EnableNotifications = true,
            CourseId = course1Id
        };
        await Connection.InsertAsync(assessment1);

        var assessment2 = new Assessment
        {
            Title = "Objective Assessment 1",
            Type = AssessmentType.Objective,
            StartDate = new DateTime(2025, 7, 01),
            EndDate = new DateTime(2025, 9, 30),
            EnableNotifications = false,
            CourseId = course1Id
        };
        await Connection.InsertAsync(assessment2);
    }

    public async Task<List<Term>> GetAllTermsAsync()
    {
        await InitDb();

        return await Connection.Table<Term>().ToListAsync();
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        await InitDb();

        return await Connection.Table<Course>().ToListAsync();
    }

    public async Task<List<Assessment>> GetAllAssessmentsAsync()
    {
        await InitDb();

        return await Connection.Table<Assessment>().ToListAsync();
    }

    // For Testing Purposes
    public async Task ClearTablesAsync()
    {
        await InitDb();

        await Connection.DeleteAllAsync<Term>();
        await Connection.DeleteAllAsync<Course>();
        await Connection.DeleteAllAsync<Assessment>();
    }
}
