# EduSync — Entity Relationship Diagrams (ERD)

One **SQL Server database**, many **schemas** (modules). Almost every business table is **tenant-scoped** via `TenantId`.

**How to read these diagrams**

| Line style | Meaning |
|------------|---------|
| Solid (`--`) | **Database foreign key** enforced in SQL |
| Dotted (`..`) | **Logical link** — string `ExternalId` / JSON; app validates, SQL does not enforce |

**Common columns on tenant tables** (not repeated on every box):

- `TenantId`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `RowVersion`, `IsDeleted`

---

## 1. High-level — schemas in one database

```mermaid
flowchart TB
    subgraph tenancy [tenancy]
        Tenants
        TenantSubscriptions
        AcademicYears
        Branches
    end

    subgraph identity [identity]
        Users
        TenantMemberships
        BranchMemberships
        RefreshTokens
    end

    subgraph people [people]
        students_Students
        students_Enrollments
        staff_Teachers
        staff_TeacherAssignments
        parents_Parents
        parents_StudentParents
    end

    subgraph academics [academics + ops]
        academics_Classes
        academics_Subjects
        admissions_Registrations
        admissions_Applications
        attendance_Records
        exams_Exams
        exams_ExamResults
        assignments_Assignments
        assignments_StudentAssignments
        timetable_Entries
    end

    subgraph finance [finance + hr]
        fees_Invoices
        fees_Payments
        payroll_Records
        leave_Requests
    end

    subgraph facilities [facilities]
        library_Books
        library_Issues
        transport_Vehicles
        transport_Routes
        transport_Assignments
        hostel_Rooms
        hostel_Allocations
        inventory_Items
    end

    subgraph platform [platform]
        notifications_Notifications
        uploads_Files
        jobs_Executions
        events_Messages
        audit_Logs
        webhooks_Subscriptions
        webhooks_Deliveries
        compliance_Policies
    end

    Tenants --> people
    Tenants --> academics
    Tenants --> finance
    Tenants --> facilities
    Tenants --> platform
    Users --> TenantMemberships
```

---

## 2. Tenancy & identity (who can log in)

```mermaid
erDiagram
    Tenants ||--|| TenantSubscriptions : "FK TenantId"
    Tenants ||--o{ AcademicYears : "FK TenantId"
    Users ||--o{ TenantMemberships : "FK UserId"
    Users ||--o{ RefreshTokens : "FK UserId"
    Tenants ||..o{ TenantMemberships : "TenantId column"

    Tenants {
        uuid Id PK
        string ExternalId UK
        string Slug UK
        string Name
        string Status
    }

    TenantSubscriptions {
        uuid Id PK
        uuid TenantId FK
        string PlanId
        int SeatLimit
        datetime ExpiresAt
    }

    AcademicYears {
        uuid Id PK
        uuid TenantId FK
        string Name
        bool IsCurrent
    }

    Users {
        uuid Id PK
        string ExternalId UK
        string Email UK
        string Name
        string Role
        string PasswordHash
    }

    TenantMemberships {
        uuid Id PK
        uuid UserId FK
        uuid TenantId
        string Role
        bool IsActive
    }

    RefreshTokens {
        uuid Id PK
        uuid UserId FK
        string TokenHash UK
        datetime ExpiresAt
    }
```

**Notes**

- JWT role comes from **`TenantMemberships.Role`**, not `Users.Role`.
- `Users` are global; **membership** ties a user to one school (tenant).

---

## 3. People — students, teachers, parents

```mermaid
erDiagram
    Tenants ||..o{ Students : "TenantId"
    Tenants ||..o{ Teachers : "TenantId"
    Tenants ||..o{ Parents : "TenantId"
    Parents ||..o{ Students : "ChildrenJson / StudentIdsJson"

    Students {
        uuid Id PK
        uuid TenantId
        string ExternalId UK
        string AdmissionNo UK
        string FirstName
        string LastName
        string ClassName
        string Section
        string Email
        string Status
    }

    Teachers {
        uuid Id PK
        uuid TenantId
        string ExternalId UK
        string EmployeeId UK
        string Department
        string ClassesJson
    }

    Parents {
        uuid Id PK
        uuid TenantId
        string ExternalId UK
        string Email
        string ChildrenJson
        string StudentIdsJson
    }
```

**Notes**

- Parent ↔ student is **denormalized JSON**, not a join table.
- Portal users often match `Users.ExternalId` to `Students.ExternalId` or parent record.

---

## 4. Academics & daily school operations

```mermaid
erDiagram
    Tenants ||..o{ Classes : "TenantId"
    Tenants ||..o{ Subjects : "TenantId"
    Tenants ||..o{ Applications : "TenantId"
    Tenants ||..o{ AttendanceRecords : "TenantId"
    Tenants ||..o{ Exams : "TenantId"
    Tenants ||..o{ TimetableEntries : "TenantId"
    Tenants ||..o{ Notifications : "TenantId"

    Classes ||..o{ Subjects : "ClassName"
    Students ||..o{ AttendanceRecords : "EntityExternalId"
    Teachers ||..o{ AttendanceRecords : "EntityExternalId"
    Students ||..o{ Exams : "ClassName"
    Classes ||..o{ TimetableEntries : "ClassName"

    Classes {
        uuid Id PK
        string ExternalId
        string Name
        string SectionsJson
    }

    Subjects {
        uuid Id PK
        string ExternalId
        string Code
        string ClassName
        string TeacherExternalId
    }

    Applications {
        uuid Id PK
        string ExternalId
        string ApplicationNo UK
        string Status
        string FormDataJson
        string DocumentsJson
    }

    AttendanceRecords {
        uuid Id PK
        string EntityType
        string EntityExternalId
        date Date
        string Status
    }

    Exams {
        uuid Id PK
        string ClassName
        string Subject
        date ExamDate
        string Status
    }

    TimetableEntries {
        uuid Id PK
        string ClassName
        string Day
        string PeriodsJson
    }

    Notifications {
        uuid Id PK
        string TargetAudience
        string Title
        string Message
    }
```

---

## 5. Finance & HR

```mermaid
erDiagram
    Tenants ||..o{ Invoices : "TenantId"
    Invoices ||--o{ Payments : "FK FeeInvoiceId"
    Students ||..o{ Invoices : "StudentExternalId"
    Students ||..o{ Payments : "StudentExternalId"
    Tenants ||..o{ PayrollRecords : "TenantId"
    Teachers ||..o{ PayrollRecords : "EmployeeExternalId"
    Tenants ||..o{ LeaveRequests : "TenantId"
    Teachers ||..o{ LeaveRequests : "EmployeeExternalId"

    Invoices {
        uuid Id PK
        string ExternalId
        string InvoiceNo UK
        string StudentExternalId
        decimal TotalFee
        decimal Paid
        decimal Pending
        string Status
        string FeeItemsJson
    }

    Payments {
        uuid Id PK
        uuid FeeInvoiceId FK
        string StudentExternalId
        decimal Amount
        datetime PaidAt
    }

    PayrollRecords {
        uuid Id PK
        string EmployeeExternalId
        string Month
        int Year
        decimal NetSalary
        string Status
    }

    LeaveRequests {
        uuid Id PK
        string EmployeeExternalId
        date FromDate
        date ToDate
        string Status
    }
```

**Notes**

- **Only solid FK in finance:** `Payments` → `Invoices`.
- Student on invoice is a **string reference**, not FK.

---

## 6. Library, transport, hostel, inventory

```mermaid
erDiagram
    Tenants ||..o{ Books : "TenantId"
    Books ||--o{ BookIssues : "FK BookId"
    Students ||..o{ BookIssues : "MemberExternalId"
    Tenants ||..o{ Vehicles : "TenantId"
    Tenants ||..o{ Routes : "TenantId"
    Tenants ||..o{ TransportAssignments : "TenantId"
    Students ||..o{ TransportAssignments : "StudentExternalId"
    Routes ||..o{ TransportAssignments : "RouteExternalId"
    Tenants ||..o{ HostelRooms : "TenantId"
    HostelRooms ||--o{ HostelAllocations : "FK RoomId"
    Students ||..o{ HostelAllocations : "StudentExternalId"
    Tenants ||..o{ InventoryItems : "TenantId"

    Books {
        uuid Id PK
        string ExternalId
        string Title
        int AvailableCopies
    }

    BookIssues {
        uuid Id PK
        uuid BookId FK
        string MemberExternalId
        string MemberType
        date IssueDate
        date DueDate
        string Status
    }

    Vehicles {
        uuid Id PK
        string RegistrationNo
        int Capacity
    }

    Routes {
        uuid Id PK
        string StopsJson
        string Status
    }

    TransportAssignments {
        uuid Id PK
        string StudentExternalId
        string RouteExternalId
        string Status
    }

    HostelRooms {
        uuid Id PK
        string Block
        int Capacity
    }

    HostelAllocations {
        uuid Id PK
        uuid RoomId FK
        string StudentExternalId
        date AllocatedOn
    }

    InventoryItems {
        uuid Id PK
        string Sku UK
        int Quantity
        string Category
    }
```

**Solid FKs here:** `BookIssues` → `Books`, `HostelAllocations` → `HostelRooms`.

---

## 7. Platform & integration

```mermaid
erDiagram
    Tenants ||..o{ StoredFiles : "TenantId"
    Tenants ||..o{ JobExecutions : "TenantId"
    Tenants ||..o{ OutboxMessages : "TenantId optional"
    Tenants ||..o{ AuditLogs : "TenantId"
    Tenants ||..o{ WebhookSubscriptions : "TenantId"
    WebhookSubscriptions ||--o{ WebhookDeliveries : "FK SubscriptionId"
    Tenants ||..o{ RetentionPolicies : "TenantId"

    StoredFiles {
        uuid Id PK
        string ExternalId
        string FileName
        string StoragePath
        string Category
    }

    JobExecutions {
        uuid Id PK
        string JobType
        string Status
        datetime StartedAt
    }

    OutboxMessages {
        uuid Id PK
        uuid TenantId
        string EventType
        string Payload
        string Status
    }

    AuditLogs {
        uuid Id PK
        string Path
        string Action
        datetime OccurredAt
    }

    WebhookSubscriptions {
        uuid Id PK
        string Url
        string EventTypes
    }

    WebhookDeliveries {
        uuid Id PK
        uuid SubscriptionId FK
        string Status
        int HttpStatus
    }

    RetentionPolicies {
        uuid Id PK
        string EntityType UK
        int RetentionDays
    }
```

---

## 8. Full picture — tenant at the center

```mermaid
erDiagram
    Tenants ||--|| TenantSubscriptions : has
    Tenants ||--o{ AcademicYears : has
    Users ||--o{ TenantMemberships : member
    Tenants ||..o{ TenantMemberships : scoped
    Users ||--o{ RefreshTokens : has

    Tenants ||..o{ Students : owns
    Tenants ||..o{ Teachers : owns
    Tenants ||..o{ Parents : owns
    Tenants ||..o{ Classes : owns
    Tenants ||..o{ Subjects : owns
    Tenants ||..o{ Applications : owns
    Tenants ||..o{ AttendanceRecords : owns
    Tenants ||..o{ Invoices : owns
    Invoices ||--o{ Payments : has
    Tenants ||..o{ Exams : owns
    Tenants ||..o{ TimetableEntries : owns
    Tenants ||..o{ Notifications : owns
    Tenants ||..o{ PayrollRecords : owns
    Tenants ||..o{ LeaveRequests : owns
    Tenants ||..o{ Books : owns
    Books ||--o{ BookIssues : has
    Tenants ||..o{ Vehicles : owns
    Tenants ||..o{ Routes : owns
    Tenants ||..o{ TransportAssignments : owns
    Tenants ||..o{ HostelRooms : owns
    HostelRooms ||--o{ HostelAllocations : has
    Tenants ||..o{ InventoryItems : owns
    Tenants ||..o{ StoredFiles : owns
    Tenants ||..o{ JobExecutions : owns
    Tenants ||..o{ OutboxMessages : owns
    Tenants ||..o{ AuditLogs : owns
    Tenants ||..o{ WebhookSubscriptions : owns
    WebhookSubscriptions ||--o{ WebhookDeliveries : has
    Tenants ||..o{ RetentionPolicies : owns

    Students ||..o{ Invoices : StudentExternalId
    Students ||..o{ AttendanceRecords : EntityExternalId
    Students ||..o{ TransportAssignments : StudentExternalId
    Parents ||..o{ Students : JSON children
```

---

## 9. Design patterns in this model

### Multi-tenancy

- Every school = row in `tenancy.Tenants`.
- Business data filtered by **`TenantId`** (EF global query filter).
- API sends **`X-Tenant-Id`** → resolves to `TenantId`.
- Optional **`X-Branch-Id`** → `BranchId` for branch-scoped entities.
- Optional **`X-Academic-Year-Id`** / **`X-Financial-Year`** → current academic year context.

### External IDs

- Frontend uses string ids like `"1"`, `"admin"` → stored as **`ExternalId`** per tenant.
- Unique per tenant: `(TenantId, ExternalId)`.

### Modular monolith

- Each module = SQL **schema** (`students`, `fees`, …).
- Few **cross-schema FKs**; modules link via **`ExternalId` strings** to stay loosely coupled.

### JSON columns

| Table | JSON field | Purpose |
|-------|------------|---------|
| `admissions.Applications` | `FormDataJson`, `DocumentsJson` | Application wizard (still JSON) |
| `academics.Classes` | `SectionsJson` | Sections A/B/C |
| `fees.Invoices` | `FeeItemsJson` | Line items |
| `timetable.Entries` | `PeriodsJson` | Period schedule |
| `transport.Routes` | `StopsJson` | Bus stops |

### Real FKs (enforced in SQL)

| Child | Parent |
|-------|--------|
| `identity.TenantMemberships` | `identity.Users` |
| `identity.RefreshTokens` | `identity.Users` |
| `tenancy.TenantSubscriptions` | `tenancy.Tenants` |
| `tenancy.AcademicYears` | `tenancy.Tenants` |
| `fees.Payments` | `fees.Invoices` |
| `library.Issues` | `library.Books` |
| `hostel.Allocations` | `hostel.Rooms` |
| `webhooks.Deliveries` | `webhooks.Subscriptions` |
| `identity.BranchMemberships` | `tenancy.Branches`, `identity.Users` |
| `students.Enrollments` | `students.Students` |
| `parents.StudentParents` | `students.Students`, `parents.Parents` |
| `assignments.StudentAssignments` | `assignments.Assignments` |
| `admissions.AdmissionApprovals` | `admissions.Applications` |

**Normalized (replaces JSON):** parent ↔ student links use `parents.StudentParents`; teacher class assignments use `staff.TeacherAssignments`; student class/section uses `students.Enrollments`.

---

## 10. ERP core — branches, enrollments, admissions (phases 11–12)

```mermaid
erDiagram
    Tenants ||--o{ Branches : "FK TenantId"
    Tenants ||--o{ AcademicYears : "FK TenantId"
    Branches ||--o{ BranchMemberships : "FK BranchId"
    Users ||--o{ BranchMemberships : "FK UserId"

    Students ||--o{ Enrollments : "FK StudentId"
    Branches ||..o{ Enrollments : "BranchId"
    AcademicYears ||..o{ Enrollments : "AcademicYearId"

    Registrations ||..o| Applications : "RegistrationId"
    Applications ||--o{ AdmissionApprovals : "FK"
    Applications ||..o| Students : "ApprovedStudentExternalId"

    Parents ||--o{ StudentParents : "FK ParentId"
    Students ||--o{ StudentParents : "FK StudentId"

    Exams ||..o{ ExamResults : "ExamExternalId"
    Students ||..o{ ExamResults : "StudentExternalId"

    Assignments ||--o{ StudentAssignments : "FK AssignmentId"
    Students ||--o{ StudentAssignments : "FK StudentId"

    Branches {
        uuid Id PK
        uuid TenantId FK
        string ExternalId
        string Code
        string Name
    }

    Enrollments {
        uuid Id PK
        uuid StudentId FK
        uuid BranchId
        uuid AcademicYearId
        string ClassName
        string Section
        string RollNo
        string EnrollmentStatus
    }

    Registrations {
        uuid Id PK
        string RegistrationNo
        string Status
        uuid AcademicYearId
    }

    ExamResults {
        uuid Id PK
        string ExamExternalId
        string StudentExternalId
        decimal MarksObtained
        string Grade
    }
```

**Lifecycle flows**

1. **Registration** → submit → (optional verify) → convert → **AdmissionApplication** (draft)
2. **Admission** → submit → approve → **Student** + **Enrollment** + outbox `admission.approved`
3. **Promotion** → `PromotionBatch` closes prior enrollment, opens new year enrollment
4. **Assignments** → teacher creates → `StudentAssignment` rows per enrolled student → portal submit

---

## 11. View in tools

- **Markdown preview** in VS Code / GitHub renders Mermaid diagrams in this file.
- **Azure Data Studio / SSMS**: connect to DB → Database Diagrams (physical tables only).
- **Table list**: [SCHEMA.md](SCHEMA.md)

---

## Related

- [SCHEMA.md](SCHEMA.md) — table list by schema
- [CODEBASE_GUIDE.md](CODEBASE_GUIDE.md) — where entities and handlers live
- [ENDPOINTS.md](ENDPOINTS.md) — REST routes for branches, registrations, assignments, exam results

*Last updated: phases 11–12 ERP architecture (branches, enrollments, exam results, assignments).*
