using System.Text.Json;
using EduSync.Modules.Admissions.Domain;
using EduSync.Modules.Academics.Application;
using EduSync.Modules.Academics.Domain;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Parents.Application;
using EduSync.Modules.Parents.Domain;
using EduSync.Modules.Staff.Application;
using EduSync.Modules.Staff.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.Modules.Company.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.Infrastructure.Application.Admissions;
using EduSync.Infrastructure.Application.Timetable;
using EduSync.Infrastructure.Tenancy;
using FinancialYearDefaults = EduSync.Infrastructure.Tenancy.FinancialYearDefaults;
using EduSync.Modules.Attendance.Domain;
using EduSync.Modules.Exams.Domain;
using EduSync.Modules.Fees.Domain;
using EduSync.Modules.Notifications.Domain;
using EduSync.Modules.Timetable.Application;
using EduSync.Modules.Timetable.Domain;
using EduSync.Modules.Payroll.Domain;
using EduSync.Modules.Leave.Domain;
using EduSync.Modules.Library.Domain;
using EduSync.Modules.Transport.Domain;
using EduSync.Modules.Hostel.Domain;
using EduSync.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid DemoTenantGuid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    public const string DemoTenantExternalId = "demo-school-001";
    public const string DemoTenantSlug = "demo-school";
    public const string AdminEmail = "admin@school.edu";
    public const string AdminPassword = "admin123";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
        var tenantContext = (TenantContext)scope.ServiceProvider.GetRequiredService<ITenantContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Tenants.AnyAsync(cancellationToken))
        {
            tenantContext.Set(DemoTenantGuid, DemoTenantSlug, DemoTenantExternalId);
            await SeedPhase2IfMissingAsync(db, DemoTenantGuid, cancellationToken);
            await SeedPhase3IfMissingAsync(db, DemoTenantGuid, cancellationToken);
            await SeedPhase4IfMissingAsync(db, DemoTenantGuid, cancellationToken);
            await SeedPhase5IfMissingAsync(db, DemoTenantGuid, cancellationToken);
            await SeedEnquiriesIfMissingAsync(db, cancellationToken);
            await SeedFinancialYearsIfMissingAsync(db, DemoTenantGuid, cancellationToken);
            await EnsureAdminOnlyLoginAsync(db, passwordHasher, DemoTenantGuid, cancellationToken);
            return;
        }

        logger.LogInformation("Seeding demo tenant {TenantId}", DemoTenantExternalId);

        var tenant = new Tenant
        {
            Id = DemoTenantGuid,
            ExternalId = DemoTenantExternalId,
            Slug = DemoTenantSlug,
            Name = "Demo International School",
            SchoolEmail = "admin@school.edu",
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Subscription = new TenantSubscription
            {
                TenantId = DemoTenantGuid,
                PlanId = "professional",
                SeatLimit = 150,
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                FeatureFlagsJson = """{"admissions":true,"portals":true}""",
            },
        };

        db.Tenants.Add(tenant);
        db.AcademicYears.AddRange(
            new AcademicYear
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantGuid,
                Name = FinancialYearDefaults.Demo,
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                IsCurrent = true,
            },
            new AcademicYear
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantGuid,
                Name = "2025-26",
                StartDate = new DateOnly(2025, 4, 1),
                EndDate = new DateOnly(2026, 3, 31),
                IsCurrent = false,
            });

        var admin = CreateAdminUser(passwordHasher);
        admin.Memberships.Add(new TenantMembership
        {
            Id = Guid.NewGuid(),
            TenantId = DemoTenantGuid,
            UserId = admin.Id,
            Role = UserRoles.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
        });
        db.Users.Add(admin);
        db.Students.AddRange(CreateDemoStudents(DemoTenantGuid));
        await SeedPhase2IfMissingAsync(db, DemoTenantGuid, cancellationToken);
        await SeedPhase3IfMissingAsync(db, DemoTenantGuid, cancellationToken);
        await SeedPhase4IfMissingAsync(db, DemoTenantGuid, cancellationToken);
        await SeedPhase5IfMissingAsync(db, DemoTenantGuid, cancellationToken);

        await SeedEnquiriesIfMissingAsync(db, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAdminOnlyLoginAsync(
        EduSyncDbContext db,
        IPasswordHasher passwordHasher,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var normalizedAdmin = AdminEmail.ToLowerInvariant();
        var admin = await db.Users
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedAdmin, cancellationToken);

        if (admin is null)
        {
            admin = CreateAdminUser(passwordHasher);
            admin.Memberships.Add(new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = admin.Id,
                Role = UserRoles.Admin,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
            });
            db.Users.Add(admin);
        }
        else
        {
            admin.IsActive = true;
            admin.Role = UserRoles.Admin;
            if (!admin.Memberships.Any(m => m.TenantId == tenantId && m.IsActive))
            {
                admin.Memberships.Add(new TenantMembership
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = admin.Id,
                    Role = UserRoles.Admin,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                });
            }
        }

        var nonAdminUsers = await db.Users
            .Where(u => u.NormalizedEmail != normalizedAdmin)
            .ToListAsync(cancellationToken);

        if (nonAdminUsers.Count == 0)
        {
            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var nonAdminIds = nonAdminUsers.Select(u => u.Id).ToList();
        foreach (var user in nonAdminUsers)
        {
            user.IsActive = false;
        }

        var memberships = await db.TenantMemberships
            .Where(m => nonAdminIds.Contains(m.UserId) && m.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var membership in memberships)
        {
            membership.IsActive = false;
        }

        var refreshTokens = await db.RefreshTokens
            .Where(r => nonAdminIds.Contains(r.UserId) && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFinancialYearsIfMissingAsync(EduSyncDbContext db, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.AcademicYears.AnyAsync(y => y.TenantId == tenantId && y.Name == FinancialYearDefaults.Demo, cancellationToken))
        {
            foreach (var year in await db.AcademicYears.Where(y => y.TenantId == tenantId).ToListAsync(cancellationToken))
            {
                year.IsCurrent = false;
            }

            db.AcademicYears.Add(new AcademicYear
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = FinancialYearDefaults.Demo,
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                IsCurrent = true,
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedEnquiriesIfMissingAsync(EduSyncDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.SchoolEnquiries.AnyAsync(cancellationToken))
        {
            db.SchoolEnquiries.AddRange(
                new SchoolEnquiry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "1",
                    SchoolName = "Sunrise Public School",
                    ContactName = "Priya Nair",
                    Email = "priya@sunrisepublic.edu",
                    Phone = "+91 98765 00001",
                    City = "Mumbai",
                    PlanKey = "professional",
                    Status = EnquiryStatuses.New,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                },
                new SchoolEnquiry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "2",
                    SchoolName = "Green Valley Academy",
                    ContactName = "Amit Verma",
                    Email = "amit@greenvalley.edu",
                    Phone = "+91 98765 00002",
                    City = "Delhi",
                    PlanKey = "starter",
                    Status = EnquiryStatuses.Contacted,
                    Notes = "Follow-up scheduled next week.",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedPhase2IfMissingAsync(EduSyncDbContext db, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.Teachers.AnyAsync(cancellationToken))
        {
            db.Teachers.AddRange(CreateDemoTeachers(tenantId));
        }

        if (!await db.Parents.AnyAsync(cancellationToken))
        {
            db.Parents.AddRange(CreateDemoParents(tenantId));
        }

        if (!await db.Classes.AnyAsync(cancellationToken))
        {
            db.Classes.AddRange(CreateDemoClasses(tenantId));
        }

        if (!await db.Subjects.AnyAsync(cancellationToken))
        {
            db.Subjects.AddRange(CreateDemoSubjects(tenantId));
        }

        if (!await db.AdmissionApplications.AnyAsync(cancellationToken))
        {
            db.AdmissionApplications.Add(CreateSampleAdmission(tenantId));
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static IEnumerable<Teacher> CreateDemoTeachers(Guid tenantId)
    {
        yield return new Teacher
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "1", FirstName = "Rajesh", LastName = "Kumar",
            EmployeeId = "EMP001", Department = "Science", Subject = "Chemistry", Qualification = "Ph.D. Chemistry",
            ExperienceYears = 15, Email = "rajesh.k@school.edu", Phone = "+91 98765 12340", Salary = 85000,
            JoiningDate = new DateOnly(2010, 6, 15), Status = "active",
            ClassesJson = TeacherMapping.SerializeClasses(["10-A", "11-B", "12-A"]),
            AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=rajesh",
        };
        yield return new Teacher
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "2", FirstName = "Anita", LastName = "Singh",
            EmployeeId = "EMP002", Department = "Mathematics", Subject = "Mathematics", Qualification = "M.Sc. Mathematics",
            ExperienceYears = 12, Email = "anita.s@school.edu", Phone = "+91 98765 12341", Salary = 72000,
            JoiningDate = new DateOnly(2012, 8, 1), Status = "active",
            ClassesJson = TeacherMapping.SerializeClasses(["9-C", "10-A", "10-B"]),
            AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=anita",
        };
    }

    private static IEnumerable<Parent> CreateDemoParents(Guid tenantId)
    {
        yield return new Parent
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "1", FirstName = "Rajesh", LastName = "Sharma",
            Email = "rajesh.sharma@email.com", Phone = "+91 98765 43200", Occupation = "Business Owner",
            Address = "123 Green Park, Mumbai", Status = "active",
            ChildrenJson = ParentMapping.SerializeList(["Arjun Sharma"]),
            StudentIdsJson = ParentMapping.SerializeList(["1"]),
            AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=rajeshp",
        };
        yield return new Parent
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "2", FirstName = "Sunita", LastName = "Patel",
            Email = "sunita.patel@email.com", Phone = "+91 98765 43201", Occupation = "Doctor",
            Address = "456 Rose Garden, Mumbai", Status = "active",
            ChildrenJson = ParentMapping.SerializeList(["Priya Patel"]),
            StudentIdsJson = ParentMapping.SerializeList(["2"]),
        };
    }

    private static IEnumerable<SchoolClass> CreateDemoClasses(Guid tenantId)
    {
        yield return new SchoolClass
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "4", Name = "Class 10",
            SectionsJson = AcademicsMapping.SerializeSections(["A", "B"]),
            TotalStudents = 90, ClassTeacherName = "Mrs. Anita Singh",
        };
        yield return new SchoolClass
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "6", Name = "Class 12",
            SectionsJson = AcademicsMapping.SerializeSections(["Science", "Commerce"]),
            TotalStudents = 68, ClassTeacherName = "Dr. Rajesh Kumar",
        };
    }

    private static IEnumerable<Subject> CreateDemoSubjects(Guid tenantId)
    {
        yield return new Subject
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "1", Name = "Mathematics", Code = "MATH10",
            ClassName = "10-A", TeacherExternalId = "2", TeacherName = "Mrs. Anita Singh", WeeklyHours = 6, Status = "active",
        };
        yield return new Subject
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "2", Name = "Physics", Code = "PHY12",
            ClassName = "12-A", TeacherExternalId = "1", TeacherName = "Dr. Rajesh Kumar", WeeklyHours = 5, Status = "active",
        };
    }

    private static AdmissionApplication CreateSampleAdmission(Guid tenantId)
    {
        var form = new
        {
            firstName = "Aarav",
            lastName = "Mehta",
            gender = "male",
            dateOfBirth = "2018-04-12",
            classSought = "Class I",
            academicSession = "2026-27",
            fatherName = "Rohan Mehta",
            email = "rohan.mehta@email.com",
            primaryMobile = "9876543210",
        };
        var formJson = JsonSerializer.Serialize(form);
        var app = new AdmissionApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = "adm-sample-001",
            ApplicationNo = "ADM2026001001",
            Status = AdmissionStatuses.Submitted,
            CurrentStep = "review",
            SubmittedAt = DateTime.UtcNow.AddDays(-2),
        };
        AdmissionJsonHelper.ApplyFormMetadata(app, formJson);
        return app;
    }

    private static User CreateAdminUser(IPasswordHasher passwordHasher) => new()
    {
        Id = Guid.NewGuid(),
        ExternalId = "admin",
        Email = AdminEmail,
        NormalizedEmail = AdminEmail.ToLowerInvariant(),
        Name = "Admin User",
        PasswordHash = passwordHasher.Hash(AdminPassword),
        Role = UserRoles.Admin,
        IsActive = true,
    };

    private static IEnumerable<Student> CreateDemoStudents(Guid tenantId)
    {
        yield return CreateStudent(tenantId, "1", "Arjun", "Sharma", "10-A", "A", "1001", "ADM2020001",
            "arjun.s@school.edu", "+91 98765 43210", "2008-05-15", "male", "B+", "rajesh.sharma@email.com", 96, "paid");
        yield return CreateStudent(tenantId, "2", "Priya", "Patel", "8-B", "B", "802", "ADM2021015",
            "priya.p@school.edu", "+91 98765 43211", "2010-08-22", "female", "A+", "sunita.patel@email.com", 94, "pending");
        yield return CreateStudent(tenantId, "3", "Rahul", "Verma", "12-A", "A", "1201", "ADM2019008",
            "rahul.v@school.edu", "+91 98765 43212", "2006-03-10", "male", "O+", "anil.verma@email.com", 92, "paid");
    }

    private static Student CreateStudent(
        Guid tenantId,
        string externalId,
        string firstName,
        string lastName,
        string className,
        string section,
        string rollNo,
        string admissionNo,
        string email,
        string phone,
        string dob,
        string gender,
        string bloodGroup,
        string parentEmail,
        int attendance,
        string feeStatus) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        ExternalId = externalId,
        FinancialYear = FinancialYearDefaults.Demo,
        FirstName = firstName,
        LastName = lastName,
        ClassName = className,
        Section = section,
        RollNo = rollNo,
        AdmissionNo = admissionNo,
        Email = email,
        Phone = phone,
        DateOfBirth = DateOnly.Parse(dob),
        Gender = gender,
        BloodGroup = bloodGroup,
        ParentEmail = parentEmail,
        AttendancePercent = attendance,
        FeeStatus = feeStatus,
        Status = "active",
        AvatarUrl = $"https://api.dicebear.com/7.x/avataaars/svg?seed={firstName.ToLowerInvariant()}",
    };

    private static async Task SeedPhase3IfMissingAsync(EduSyncDbContext db, Guid tenantId, CancellationToken cancellationToken)
    {
        var date = new DateOnly(2024, 6, 28);
        if (!await db.AttendanceRecords.AnyAsync(cancellationToken))
        {
            db.AttendanceRecords.AddRange(
            CreateAttendance(tenantId, "1", "student", "1", "Arjun Sharma", "10-A", date, "present", "08:45", "03:30", null),
            CreateAttendance(tenantId, "2", "student", "2", "Priya Patel", "8-B", date, "present", "08:50", "03:30", null),
            CreateAttendance(tenantId, "3", "student", "3", "Rahul Verma", "12-A", date, "late", "09:15", "03:30", "Traffic delay"),
            CreateAttendance(tenantId, "4", "student", "4", "Sneha Gupta", "9-C", date, "absent", null, null, "Sick leave"));
        }

        if (!await db.FeeInvoices.AnyAsync(cancellationToken))
        {
            db.FeeInvoices.AddRange(
            CreateFee(tenantId, "1", "INV2024001", "1", "Arjun Sharma", "10-A", 120000, 120000, 0, "paid", "bank_transfer", new DateOnly(2024, 6, 30), new DateOnly(2024, 6, 15)),
            CreateFee(tenantId, "2", "INV2024002", "2", "Priya Patel", "8-B", 100000, 58000, 42000, "pending", null, new DateOnly(2024, 6, 15), null),
            CreateFee(tenantId, "4", "INV2024004", "4", "Sneha Gupta", "9-C", 110000, 44000, 66000, "overdue", null, new DateOnly(2024, 5, 31), null));
        }

        if (!await db.Exams.AnyAsync(cancellationToken))
        {
            db.Exams.AddRange(
            CreateExam(tenantId, "1", "Mid-Term Mathematics", "mid_term", "Mathematics", "10-A", new DateOnly(2024, 7, 15), "scheduled", 45),
            CreateExam(tenantId, "2", "Unit Test - Physics", "unit_test", "Physics", "12-A", new DateOnly(2024, 6, 28), "completed", 38));
        }

        if (!await db.TimetableEntries.AnyAsync(cancellationToken))
        {
            var periods = new[]
        {
            new TimetablePeriodDto("09:00-09:45", "Mathematics", "Mrs. Anita Singh", "Room 201"),
            new TimetablePeriodDto("09:45-10:30", "Physics", "Mr. Suresh Menon", "Lab 1"),
            new TimetablePeriodDto("10:45-11:30", "English", "Mr. Vikram Rao", "Room 201"),
        };
        db.TimetableEntries.Add(new TimetableEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = "1",
            ClassName = "10-A",
            Day = "Monday",
            PeriodsJson = TimetableMapping.SerializePeriods(periods),
        });
        }

        if (!await db.Notifications.AnyAsync(cancellationToken))
        {
            db.Notifications.AddRange(
            new Notification
            {
                Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "1",
                Title = "Fee Payment Reminder",
                Message = "Fee payment for Q2 is due on June 30, 2024",
                Type = "warning", TargetAudience = "parents",
                SentAt = DateTime.UtcNow.AddDays(-10), ReadCount = 1250, TotalRecipients = 2847,
            },
            new Notification
            {
                Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = "2",
                Title = "Annual Day Celebration",
                Message = "Annual Day celebration scheduled for July 15, 2024.",
                Type = "info", TargetAudience = "all",
                SentAt = DateTime.UtcNow.AddDays(-12), ReadCount = 3000, TotalRecipients = 3200,
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static AttendanceRecord CreateAttendance(
        Guid tenantId, string externalId, string entityType, string entityId, string name, string? className,
        DateOnly date, string status, string? checkIn, string? checkOut, string? remarks) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        FinancialYear = FinancialYearDefaults.Demo,
        EntityType = entityType, EntityExternalId = entityId, EntityName = name, ClassName = className,
        Date = date, Status = status, CheckIn = checkIn, CheckOut = checkOut, Remarks = remarks,
    };

    private static FeeInvoice CreateFee(
        Guid tenantId, string externalId, string invoiceNo, string studentId, string studentName, string className,
        decimal total, decimal paid, decimal pending, string status, string? method, DateOnly due, DateOnly? paidDate) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId, FinancialYear = FinancialYearDefaults.Demo,
        InvoiceNo = invoiceNo, StudentExternalId = studentId, StudentName = studentName, ClassName = className, FeeType = "tuition",
        TotalFee = total, Paid = paid, Pending = pending, Discount = 0, Fine = status == "overdue" ? 2000 : 0,
        DueDate = due, PaidDate = paidDate, Status = status, PaymentMethod = method,
    };

    private static Exam CreateExam(
        Guid tenantId, string externalId, string name, string type, string subject, string className,
        DateOnly date, string status, int students) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId, ExamName = name, ExamType = type,
        Subject = subject, ClassName = className, Date = date, StartTime = "09:00", DurationMinutes = 180,
        TotalMarks = 100, PassingMarks = 35, Room = "Hall A", Status = status, StudentsCount = students,
    };

    private static async Task SeedPhase4IfMissingAsync(EduSyncDbContext db, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.PayrollRecords.AnyAsync(cancellationToken))
        {
            db.PayrollRecords.AddRange(
            CreatePayroll(tenantId, "1", "1", "Dr. Rajesh Kumar", "Science", "June", 2024, 70000, 7000, 3500, 2000, 1500, 1000, 4200, 5800, 500, 0, 0, 0, "pending", null),
            CreatePayroll(tenantId, "5", "5", "Mr. Suresh Menon", "Science", "June", 2024, 65000, 6500, 3250, 1750, 1000, 500, 3900, 5000, 500, 0, 0, 0, "paid", new DateOnly(2024, 6, 28)));
        }

        if (!await db.LeaveRequests.AnyAsync(cancellationToken))
        {
            db.LeaveRequests.AddRange(
            CreateLeave(tenantId, "1", "4", "Ms. Deepa Nair", "History", "sick", new DateOnly(2024, 6, 20), new DateOnly(2024, 6, 25), 6, "Medical treatment", "approved", new DateOnly(2024, 6, 18)),
            CreateLeave(tenantId, "2", "2", "Mrs. Anita Singh", "Mathematics", "casual", new DateOnly(2024, 7, 1), new DateOnly(2024, 7, 2), 2, "Family function", "pending", new DateOnly(2024, 6, 25)));
        }

        if (!await db.Books.AnyAsync(cancellationToken))
        {
            var book1 = CreateBook(tenantId, "1", "The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", "Fiction", 10, 7, 3);
            var book3 = CreateBook(tenantId, "3", "Physics for Class 12", "H.C. Verma", "9788177091878", "Textbook", 50, 35, 15);
            db.Books.AddRange(book1, book3);
            db.BookIssues.Add(CreateIssue(tenantId, "1", book1, "1", "Arjun Sharma", "student", "10-A", new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 15), null, "issued", 0));
        }

        if (!await db.TransportRoutes.AnyAsync(cancellationToken))
        {
            var route1 = CreateRoute(tenantId, "1", "Route A - North Zone", "1", "MH-01-AB-1234", "Ramesh Kumar", 8, 42, 3500, "active", "15 km");
            db.TransportRoutes.Add(route1);
            db.Vehicles.Add(CreateVehicle(tenantId, "1", "MH-01-AB-1234", "bus", 50, "Ramesh Kumar", "+91 98765 11111", "MH0120150012345", "1", route1.RouteName, new DateOnly(2025, 3, 15), new DateOnly(2024, 12, 31), 42, "active"));
        }

        if (!await db.HostelRooms.AnyAsync(cancellationToken))
        {
            db.HostelRooms.AddRange(
            CreateRoom(tenantId, "1", "A-101", "Boys Block A", 4, 4, 1, "Mr. Sharma", "full", 12000),
            CreateRoom(tenantId, "2", "A-102", "Boys Block A", 4, 3, 1, "Mr. Sharma", "available", 12000));
        }

        if (!await db.InventoryItems.AnyAsync(cancellationToken))
        {
            db.InventoryItems.AddRange(
            CreateInventory(tenantId, "1", "Desktop Computer", "IT Equipment", "IT-DC-001", 45, 10, "pcs", "Computer Lab", new DateOnly(2024, 6, 10)),
            CreateInventory(tenantId, "2", "Chemistry Lab Kit", "Lab Supplies", "LAB-CH-012", 8, 15, "sets", "Lab 2", new DateOnly(2024, 5, 20), true));
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static PayrollRecord CreatePayroll(
        Guid tenantId, string externalId, string employeeId, string name, string dept, string month, int year,
        decimal basic, decimal hra, decimal da, decimal ta, decimal medical, decimal special,
        decimal pf, decimal tax, decimal ins, decimal loan, decimal other, decimal bonus,
        string status, DateOnly? paidDate)
    {
        var gross = basic + hra + da + ta + medical + special + bonus;
        var deductions = pf + tax + ins + loan + other;
        return new PayrollRecord
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
            EmployeeExternalId = employeeId, EmployeeName = name, Department = dept,
            Month = month, Year = year, BasicSalary = basic, Hra = hra, Da = da, Ta = ta,
            Medical = medical, Special = special, PfDeduction = pf, TaxDeduction = tax,
            Insurance = ins, LoanDeduction = loan, OtherDeduction = other, Bonus = bonus,
            GrossSalary = gross, TotalDeductions = deductions, NetSalary = gross - deductions,
            Status = status, PaymentDate = paidDate,
        };
    }

    private static LeaveRequest CreateLeave(
        Guid tenantId, string externalId, string employeeId, string name, string dept, string type,
        DateOnly start, DateOnly end, int days, string reason, string status, DateOnly applied) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        EmployeeExternalId = employeeId, EmployeeName = name, Department = dept,
        LeaveType = type, StartDate = start, EndDate = end, Days = days, Reason = reason,
        Status = status, AppliedOn = applied,
        ApprovedBy = status == "approved" ? "Principal" : null,
        ApprovedOn = status == "approved" ? applied.AddDays(1) : null,
    };

    private static Book CreateBook(
        Guid tenantId, string externalId, string title, string author, string isbn, string category,
        int qty, int available, int issued) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        Title = title, Author = author, Isbn = isbn, Category = category,
        Publisher = "Publisher", PublishYear = 2020, Quantity = qty, Available = available, Issued = issued,
        Location = "Shelf A-1",
    };

    private static BookIssue CreateIssue(
        Guid tenantId, string externalId, Book book, string memberId, string memberName, string memberType,
        string? className, DateOnly issueDate, DateOnly dueDate, DateOnly? returnDate, string status, decimal fine) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        BookId = book.Id, BookExternalId = book.ExternalId, BookTitle = book.Title,
        MemberExternalId = memberId, MemberName = memberName, MemberType = memberType, ClassName = className,
        IssueDate = issueDate, DueDate = dueDate, ReturnDate = returnDate, Status = status, Fine = fine, Book = book,
    };

    private static TransportRoute CreateRoute(
        Guid tenantId, string externalId, string name, string vehicleId, string vehicleNo, string driver,
        int stops, int students, decimal fare, string status, string distance) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId, RouteName = name,
        VehicleExternalId = vehicleId, VehicleNumber = vehicleNo, DriverName = driver,
        StartPoint = "School Campus", EndPoint = "Green Park", TotalStops = stops, TotalStudents = students,
        Fare = fare, MorningTime = "07:00 AM", EveningTime = "03:30 PM", Status = status, Distance = distance,
    };

    private static Vehicle CreateVehicle(
        Guid tenantId, string externalId, string number, string type, int capacity,
        string driver, string phone, string license, string routeId, string routeName,
        DateOnly insurance, DateOnly fitness, int students, string status) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        VehicleNumber = number, VehicleType = type, Capacity = capacity,
        DriverName = driver, DriverPhone = phone, DriverLicense = license,
        RouteExternalId = routeId, RouteName = routeName,
        InsuranceExpiry = insurance, FitnessExpiry = fitness,
        CurrentStudents = students, Status = status, GpsStatus = "online", LastLocation = "Green Park",
    };

    private static HostelRoom CreateRoom(
        Guid tenantId, string externalId, string roomNo, string block, int capacity, int occupied,
        int floor, string warden, string status, decimal fee) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        RoomNo = roomNo, Block = block, Capacity = capacity, Occupied = occupied,
        Floor = floor, Warden = warden, Status = status, MonthlyFee = fee,
    };

    private static InventoryItem CreateInventory(
        Guid tenantId, string externalId, string name, string category, string sku,
        int qty, int minStock, string unit, string location, DateOnly restocked, bool lowStock = false) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        Name = name, Category = category, Sku = sku, Quantity = qty, MinStock = minStock,
        Unit = unit, Location = location, LastRestocked = restocked,
        Status = lowStock ? "low-stock" : "in-stock",
    };

    private static async Task SeedPhase5IfMissingAsync(EduSyncDbContext db, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.TransportAssignments.AnyAsync(cancellationToken))
        {
            db.TransportAssignments.AddRange(
                CreateAssignment(tenantId, "1", "1", "Arjun Sharma", "1", 5, "both", new DateOnly(2023, 6, 1), "active", "A-12"),
                CreateAssignment(tenantId, "2", "2", "Priya Patel", "2", 3, "both", new DateOnly(2023, 6, 1), "active", "B-08"));
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static TransportAssignment CreateAssignment(
        Guid tenantId, string externalId, string studentId, string studentName, string routeId,
        int stopOrder, string shift, DateOnly enrolled, string status, string? seat) => new()
    {
        Id = Guid.NewGuid(), TenantId = tenantId, ExternalId = externalId,
        StudentExternalId = studentId, StudentName = studentName, RouteExternalId = routeId,
        PickupStopOrder = stopOrder, Shift = shift, EnrolledSince = enrolled, Status = status, SeatNumber = seat,
    };
}
