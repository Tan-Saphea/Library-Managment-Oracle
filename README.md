# Library Management System

A university assignment project implementing a complete Library Management System utilizing C# Windows Forms (.NET 8) and Oracle Database Free.

The application follows a clean 3-tier architecture with programmatic WinForms controls and enforces a strict database-first design: all database interactions and CRUD operations are executed exclusively through Oracle Stored Procedures and SYS_REFCURSOR output parameters, without any direct or inline SQL queries inside C#.

---

## Table of Contents

- [Architectural Overview](#architectural-overview)
- [Technology Stack](#technology-stack)
- [Database Schema and Objects](#database-schema-and-objects)
- [Stored Procedures and Triggers](#stored-procedures-and-triggers)
- [Database Setup Instructions](#database-setup-instructions)
- [Connection Configuration](#connection-configuration)
- [C# Application Structure](#c-application-structure)
- [Build and Run Instructions](#build-and-run-instructions)
- [Feature Details](#feature-details)
- [Automated Database Verification](#automated-database-verification)
- [Common Oracle Errors and Troubleshooting](#common-oracle-errors-and-troubleshooting)

---

## Architectural Overview

The application is structured into distinct layers to enforce separation of concerns:

```
[WinForms UI Layer]
  FrmLogin | FrmMain | FrmStudent | FrmAuthor | FrmBook
         |
         v
[Repository Layer]
  StudentRepository | AuthorRepository | BookRepository | LoginRepository
         |
         v
[Data Access Layer]
  OracleDb (Oracle.ManagedDataAccess.Core)
         |
         v
[Oracle Database Layer]
  Stored Procedures (CommandType.StoredProcedure)
  Triggers (Stock Validation & Stock Auto-Update)
  Tables (Identity PKs, Foreign Keys, BLOB storage)
```

### Architectural Principles

1. **No Inline SQL in C#**: Every Insert, Update, Delete, Select All, and Search operation delegates to an Oracle Stored Procedure.
2. **Deterministic Data Binding**: Query results from Stored Procedures are returned as `SYS_REFCURSOR` output parameters and bound using `OracleDbType.RefCursor`.
3. **Parameter Binding by Name**: All `OracleCommand` instances explicitly set `cmd.BindByName = true` to prevent parameter order mismatches.
4. **BLOB Management**: Student and Author profile pictures are stored as Oracle `BLOB` objects with bidirectional byte array conversion handling null values safely.
5. **Programmatic WinForms UI**: Forms are built programmatically with standard WinForms controls (`Segoe UI`, 10pt) without external `.Designer.cs` dependencies, ensuring 100% build reliability across developer environments.

---

## Technology Stack

- **Desktop Framework**: C# Windows Forms on .NET 8 (`net8.0-windows`)
- **Database Engine**: Oracle Database 23ai / 26ai Free (Docker container `library-oracle`)
- **Database Connectivity**: `Oracle.ManagedDataAccess.Core` (Official Oracle ODP.NET Core)
- **Database Management**: Oracle SQL Developer / SQL*Plus
- **Containerization**: Docker Desktop

---

## Database Schema and Objects

The database schema comprises 10 normalized tables configured under the `LIBRARY_USER` schema:

| Table Name | Description | Key Constraints |
| :--- | :--- | :--- |
| `BookType` | Book classifications / categories | `BookTypeID` (PK Identity), `BookTypeName` (Unique) |
| `Author` | Book authors | `AuthorID` (PK Identity), `Photo` (BLOB) |
| `Student` | Enrolled library student members | `StuID` (PK Identity), `Photo` (BLOB) |
| `Librarian` | Library administrative staff | `LibrarianID` (PK Identity), `Email` (Unique) |
| `Book` | Book catalog and inventory quantities | `BookID` (PK Identity), `BookTypeID` (FK), `NumCopies >= 0` |
| `BookAuthor` | Many-to-many relationship between books and authors | Composite PK (`BookID`, `AuthorID`) |
| `Borrow` | Transaction header for book borrowing | `BorrowID` (PK Identity), `StuID` (FK), `LibrarianID` (FK) |
| `BorrowBook` | Line items for borrowed books | `BorrowBookID` (PK Identity), `BorrowID` (FK), `BookID` (FK) |
| `BOOK_RETURN` | Return transaction and condition records | `ReturnID` (PK Identity), `BorrowID` (FK), `BookID` (FK) |
| `APP_USER` | System user credentials | `UserID` (PK Identity), `Username` (Unique), `LibrarianID` (FK) |

---

## Stored Procedures and Triggers

### Stored Procedures

All repository operations interact with the following stored procedures in `sql/02_Stored_Procedures.sql`:

- **Student**:
  - `SP_STUDENT_INSERT`: Inserts student record with optional BLOB photo and returns generated `p_StuID`.
  - `SP_STUDENT_UPDATE`: Updates student details including contact and photo.
  - `SP_STUDENT_DELETE`: Deletes student by ID.
  - `SP_STUDENT_SELECT_ALL`: Returns cursor of all student records.
  - `SP_STUDENT_SELECT_BY_ID`: Returns cursor of a single student by ID.
  - `SP_STUDENT_SEARCH`: Case-insensitive wildcard search across ID, name, email, phone, and POB.

- **Author**:
  - `SP_AUTHOR_INSERT`: Inserts author with optional BLOB photo and returns generated `p_AuthorID`.
  - `SP_AUTHOR_UPDATE`: Updates author details.
  - `SP_AUTHOR_DELETE`: Deletes author by ID.
  - `SP_AUTHOR_SELECT_ALL`: Returns cursor of all authors.
  - `SP_AUTHOR_SELECT_BY_ID`: Returns cursor of author by ID.
  - `SP_AUTHOR_SEARCH`: Case-insensitive wildcard search across ID, name, email, and phone.

- **Book**:
  - `SP_BOOK_INSERT`: Inserts book record linked to a BookType and returns generated `p_BookID`.
  - `SP_BOOK_UPDATE`: Updates book record attributes and inventory counts.
  - `SP_BOOK_DELETE`: Deletes book by ID.
  - `SP_BOOK_SELECT_ALL`: Returns cursor with joined `BookTypeName`.
  - `SP_BOOK_SELECT_BY_ID`: Returns cursor of single book record.
  - `SP_BOOK_SEARCH`: Wildcard search across title, publisher, edition, and category name.

- **Lookup & Authentication**:
  - `SP_BOOKTYPE_SELECT_ALL`: Returns list of available book types for UI ComboBoxes.
  - `SP_LIBRARIAN_SELECT_ALL`: Returns list of active library staff.
  - `SP_LOGIN`: Validates username and password against `APP_USER` and returns RefCursor with user details.

- **Transactions**:
  - `SP_BORROW_CREATE`: Atomic transaction header creation.
  - `SP_BORROW_ADD_BOOK`: Adds line item and triggers automatic stock validation.
  - `SP_RETURN_BOOK`: Records return, marks borrow line items, and restores inventory.

### Triggers

- `TRG_BORROWBOOK_VALIDATE_STOCK`: Before insert check verifying available `NumCopies > 0`, raising `ORA-20001` if out of stock.
- `TRG_BORROWBOOK_DECREASE_STOCK`: After insert trigger automatically decrementing `Book.NumCopies` by 1.
- `TRG_RETURNBOOK_INCREASE_STOCK`: After insert trigger automatically incrementing `Book.NumCopies` by 1 upon return confirmation.

---

## Database Setup Instructions

The SQL scripts are located in the `sql/` directory and must be executed in numerical order:

```
sql/
├── 01_Create_Database.sql    # DDL: 10 tables, constraints, indexes
├── 02_Stored_Procedures.sql  # 24 Stored Procedures with RefCursors
├── 03_Triggers.sql           # Inventory validation & stock management triggers
├── 04_Views.sql              # Analytical views for reporting
├── 05_Other_Objects.sql      # Schema metadata and validation queries
├── 06_Sample_Data.sql        # Initial deterministic test data
└── 07_Test_Database.sql      # Complete verification test suite
```

### Option A: Execute in Oracle SQL Developer

1. Open **Oracle SQL Developer**.
2. Connect to the `LibraryDB` connection (`library_user` / `Library123`).
3. Open and run each script using **Run Script (F5)** in the order listed above:
   - `01_Create_Database.sql`
   - `02_Stored_Procedures.sql`
   - `03_Triggers.sql`
   - `04_Views.sql`
   - `05_Other_Objects.sql`
   - `06_Sample_Data.sql`
   - `07_Test_Database.sql`

### Option B: Execute via Docker SQL*Plus

```bash
# Connect to container and run scripts
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/01_Create_Database.sql
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/02_Stored_Procedures.sql
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/03_Triggers.sql
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/04_Views.sql
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/05_Other_Objects.sql
docker exec -i library-oracle sqlplus library_user/Library123@FREEPDB1 < sql/06_Sample_Data.sql
```

---

## Connection Configuration

The application centralizes the database connection string in `LibraryManagementSystem/Data/OracleDb.cs`:

```csharp
private static readonly string ConnectionString =
    "User Id=library_user;Password=Library123;Data Source=localhost:1521/FREEPDB1;";
```

### Default Credentials

- **Oracle User**: `library_user`
- **Oracle Password**: `Library123`
- **Host**: `localhost`
- **Port**: `1521`
- **Pluggable Database (PDB)**: `FREEPDB1`
- **Default Application Login**:
  - **Username**: `admin`
  - **Password**: `123`

---

## C# Application Structure

```
LibraryManagementSystem/
│
├── Data/
│   └── OracleDb.cs              # Centralized connection factory and helpers
│
├── Models/
│   ├── Student.cs               # Student domain model
│   ├── Author.cs                # Author domain model
│   ├── Book.cs                  # Book domain model
│   └── BookType.cs              # BookType domain model
│
├── Repositories/
│   ├── StudentRepository.cs     # SP-based CRUD for Students
│   ├── AuthorRepository.cs      # SP-based CRUD for Authors
│   ├── BookRepository.cs        # SP-based CRUD for Books & BookTypes
│   └── LoginRepository.cs       # SP_LOGIN authentication
│
├── Forms/
│   ├── FrmLogin.cs              # Clean modal login form
│   ├── FrmMain.cs               # Main shell with sidebar navigation
│   ├── FrmStudent.cs            # Student management form with photo support
│   ├── FrmAuthor.cs             # Author management form with photo support
│   └── FrmBook.cs               # Book management form with category binding
│
├── Program.cs                   # Application entry point
└── LibraryManagementSystem.csproj
```

---

## Build and Run Instructions

### Prerequisites

- .NET 8 SDK installed
- Running Oracle Database Free container (`localhost:1521/FREEPDB1`)
- Windows environment or Windows targeting enabled for WinForms

### Command Line (.NET CLI)

```bash
# Navigate to project directory
cd LibraryManagementSystem

# Restore NuGet dependencies
dotnet restore

# Build project
dotnet build

# Run application (on Windows)
dotnet run
```

### Visual Studio 2022

1. Open Visual Studio 2022.
2. Select **Open a project or solution** and select `LibraryManagementSystem/LibraryManagementSystem.csproj`.
3. Press **F5** or click **Start** to run the application.

---

## Feature Details

### 1. Login Form (`FrmLogin`)
- Standard desktop credentials dialog with masked password input (`UseSystemPasswordChar = true`).
- Authenticates against `SP_LOGIN`.
- On successful validation, opens `FrmMain` as the primary workspace.

### 2. Main Navigation (`FrmMain`)
- Sidebar layout with navigation buttons: `Dashboard`, `Students`, `Authors`, `Books`, and `Exit`.
- Embedded child forms inside the main content panel with `TopLevel = false`, `FormBorderStyle = FormBorderStyle.None`, and `Dock = DockStyle.Fill`.
- Reuses open form instances to avoid memory leaks and duplicate windows.

### 3. Student Management (`FrmStudent`)
- Input controls: ID (read-only), Full Name, Gender, Date of Birth, Place of Birth, Address, Phone, Email, and Photo.
- Photo Upload: Supports `.jpg`, `.jpeg`, and `.png` with automatic conversion to Oracle `BLOB`.
- Actions:
  - **New**: Prepares form for a fresh student entry.
  - **Save**: Calls `SP_STUDENT_INSERT`.
  - **Update**: Calls `SP_STUDENT_UPDATE`.
  - **Delete**: Prompts for confirmation and invokes `SP_STUDENT_DELETE`.
  - **Clear**: Resets input controls.
  - **Search**: Dynamic case-insensitive search via `SP_STUDENT_SEARCH`.
  - **Grid Click**: Populates input controls and previews the student photo.

### 4. Author Management (`FrmAuthor`)
- Mirrors the layout of student management for UI consistency.
- Manages author profile and photograph via `SP_AUTHOR_*` procedures.

### 5. Book Management (`FrmBook`)
- Controls: ID (read-only), Title, Book Type (ComboBox dynamically populated from `SP_BOOKTYPE_SELECT_ALL`), Publish Date, Pages count (`NumericUpDown`), Copies count (`NumericUpDown`), Edition, Publisher, Book Source, and Remark.
- Enforces non-negative page and copy count validation.
- Full CRUD operations via `SP_BOOK_*` procedures.

---

## Automated Database Verification

The test script `sql/07_Test_Database.sql` performs end-to-end verification directly inside Oracle:

1. **Object Validation**: Checks that all 10 tables, 24 stored procedures, 3 triggers, and 5 views compile with `STATUS = 'VALID'`.
2. **CRUD Verification**: Tests insertion, selection, update, search, and deletion for Student, Author, and Book procedures.
3. **Trigger Verification**: Verifies automatic stock reduction on borrowing and automatic stock replenishment on return.
4. **Constraint Verification**: Ensures borrowing fails with `ORA-20001` when book inventory drops to 0.
5. **Authentication Verification**: Validates login success for valid credentials and rejection for invalid passwords.

Run the test suite in SQL Developer by pressing **F5** on `sql/07_Test_Database.sql`.

---

## Common Oracle Errors and Troubleshooting

### 1. `ORA-01017: invalid username/password; logon denied`
- **Cause**: Incorrect database username or password.
- **Fix**: Verify `User Id=library_user;Password=Library123;` in `Data/OracleDb.cs`.

### 2. `ORA-12541: TNS:no listener`
- **Cause**: Oracle Database container is stopped or port 1521 is not mapped.
- **Fix**: Run `docker ps` to ensure the `library-oracle` container is running and port `1521` is active.

### 3. `ORA-00942: table or view does not exist`
- **Cause**: Tables have not yet been created in the connected schema.
- **Fix**: Run `sql/01_Create_Database.sql` in Oracle SQL Developer under `library_user`.

### 4. `ORA-04091: table is mutating, trigger/function may not see it`
- **Cause**: A row-level trigger attempts to query the same table being modified.
- **Fix**: This project avoids mutating table issues by updating `Book.NumCopies` directly and isolating transaction completion calculations inside `SP_RETURN_BOOK`.

### 5. `ORA-02292: integrity constraint violated - child record found`
- **Cause**: Attempting to delete a record that is referenced by active foreign keys (e.g. deleting a Student who has active Borrow transactions).
- **Fix**: Delete or return associated borrowing records before deleting the student or book.
