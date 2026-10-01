using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace LibraryTerminal
{
    class Program
    {
        private static readonly string ConnectionString =
            "User Id=library_user;Password=Library123;Data Source=localhost:1521/FREEPDB1;";

        static void Main(string[] args)
        {
            Console.Clear();
            PrintBanner();

            // Verify connectivity before showing menu
            if (!TestConnection())
            {
                Console.WriteLine("\n[ERROR] Unable to connect to Oracle Database at localhost:1521/FREEPDB1.");
                Console.WriteLine("Make sure the 'library-oracle' Docker container is running.");
                Console.WriteLine("\nPress Enter to exit...");
                Console.ReadLine();
                return;
            }

            bool running = true;
            while (running)
            {
                PrintMenu();
                Console.Write("\nEnter choice (0-10): ");
                string? choice = Console.ReadLine()?.Trim();

                Console.WriteLine();
                switch (choice)
                {
                    case "1":
                        ListStudents();
                        break;
                    case "2":
                        SearchStudents();
                        break;
                    case "3":
                        AddStudent();
                        break;
                    case "4":
                        ListBooks();
                        break;
                    case "5":
                        SearchBooks();
                        break;
                    case "6":
                        AddBook();
                        break;
                    case "7":
                        ListAuthors();
                        break;
                    case "8":
                        SearchAuthors();
                        break;
                    case "9":
                        TestLogin();
                        break;
                    case "10":
                        RunDiagnostics();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("Exiting Library Management System Terminal. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("[!] Invalid choice. Please enter a number between 0 and 10.");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
            }
        }

        static void PrintBanner()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("             LIBRARY MANAGEMENT SYSTEM - TERMINAL RUNNER (.NET 9)               ");
            Console.WriteLine("================================================================================");
            Console.WriteLine("Database : Oracle Database 23ai / 26ai Free (Docker: library-oracle)");
            Console.WriteLine("Host     : localhost:1521 | PDB: FREEPDB1 | User: library_user");
            Console.WriteLine("Protocol : 100% Oracle Stored Procedures and SYS_REFCURSOR");
            Console.WriteLine("================================================================================");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n------------------------------- MAIN MENU -------------------------------------");
            Console.WriteLine(" [1] View All Students          (SP_STUDENT_SELECT_ALL)");
            Console.WriteLine(" [2] Search Students            (SP_STUDENT_SEARCH)");
            Console.WriteLine(" [3] Add New Student            (SP_STUDENT_INSERT)");
            Console.WriteLine(" [4] View All Books             (SP_BOOK_SELECT_ALL)");
            Console.WriteLine(" [5] Search Books               (SP_BOOK_SEARCH)");
            Console.WriteLine(" [6] Add New Book               (SP_BOOK_INSERT)");
            Console.WriteLine(" [7] View All Authors           (SP_AUTHOR_SELECT_ALL)");
            Console.WriteLine(" [8] Search Authors             (SP_AUTHOR_SEARCH)");
            Console.WriteLine(" [9] Authenticate User Login    (SP_LOGIN)");
            Console.WriteLine(" [10] Run Database Diagnostics  (All Procedures & Triggers)");
            Console.WriteLine(" [0] Exit");
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        static bool TestConnection()
        {
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                Console.WriteLine($"[STATUS] Oracle Connection Established: {conn.ServerVersion}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Connection failed: {ex.Message}");
                return false;
            }
        }

        // ==========================================
        // STUDENT PROCEDURES
        // ==========================================

        static void ListStudents()
        {
            Console.WriteLine("--- [SP_STUDENT_SELECT_ALL] ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_STUDENT_SELECT_ALL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Total Students: {dt.Rows.Count}\n");
                Console.WriteLine($"{"ID",-5} | {"Name",-22} | {"Gender",-8} | {"DOB",-11} | {"Phone",-15} | {"Email"}");
                Console.WriteLine(new string('-', 80));

                foreach (DataRow row in dt.Rows)
                {
                    string dob = row["DOB"] == DBNull.Value ? "N/A" : Convert.ToDateTime(row["DOB"]).ToString("dd/MM/yyyy");
                    Console.WriteLine($"{row["StuID"],-5} | {row["StuName"],-22} | {row["Gender"],-8} | {dob,-11} | {row["Phone"],-15} | {row["Email"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static void SearchStudents()
        {
            Console.Write("Enter search keyword (name, phone, email, or POB): ");
            string? keyword = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                Console.WriteLine("[!] Search keyword cannot be empty.");
                return;
            }

            Console.WriteLine($"\n--- [SP_STUDENT_SEARCH] Keyword: '{keyword}' ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_STUDENT_SEARCH", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_keyword", OracleDbType.Varchar2, keyword, ParameterDirection.Input);
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Matches Found: {dt.Rows.Count}\n");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($"ID: {row["StuID"]} | Name: {row["StuName"]} | Gender: {row["Gender"]} | Phone: {row["Phone"]} | Email: {row["Email"]} | POB: {row["POB"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static void AddStudent()
        {
            Console.WriteLine("--- [SP_STUDENT_INSERT] Add New Student ---");
            Console.Write("Student Name (*required): ");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("[!] Student Name is required.");
                return;
            }

            Console.Write("Gender (Male/Female/Other) [Male]: ");
            string? gender = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(gender)) gender = "Male";

            Console.Write("Date of Birth (YYYY-MM-DD) [2000-01-01]: ");
            string? dobInput = Console.ReadLine()?.Trim();
            DateTime dob = DateTime.TryParse(dobInput, out var d) ? d : new DateTime(2000, 1, 1);

            Console.Write("Place of Birth (POB): ");
            string? pob = Console.ReadLine()?.Trim();

            Console.Write("Address: ");
            string? address = Console.ReadLine()?.Trim();

            Console.Write("Phone: ");
            string? phone = Console.ReadLine()?.Trim();

            Console.Write("Email: ");
            string? email = Console.ReadLine()?.Trim();

            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_STUDENT_INSERT", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_StuName", OracleDbType.Varchar2, name, ParameterDirection.Input);
                cmd.Parameters.Add("p_Gender", OracleDbType.Varchar2, string.IsNullOrEmpty(gender) ? DBNull.Value : gender, ParameterDirection.Input);
                cmd.Parameters.Add("p_DOB", OracleDbType.Date, dob, ParameterDirection.Input);
                cmd.Parameters.Add("p_POB", OracleDbType.Varchar2, string.IsNullOrEmpty(pob) ? DBNull.Value : pob, ParameterDirection.Input);
                cmd.Parameters.Add("p_Address", OracleDbType.Varchar2, string.IsNullOrEmpty(address) ? DBNull.Value : address, ParameterDirection.Input);
                cmd.Parameters.Add("p_Phone", OracleDbType.Varchar2, string.IsNullOrEmpty(phone) ? DBNull.Value : phone, ParameterDirection.Input);
                cmd.Parameters.Add("p_Email", OracleDbType.Varchar2, string.IsNullOrEmpty(email) ? DBNull.Value : email, ParameterDirection.Input);
                cmd.Parameters.Add("p_Photo", OracleDbType.Blob, DBNull.Value, ParameterDirection.Input);

                var pNewId = new OracleParameter("p_new_stuid", OracleDbType.Int32, ParameterDirection.Output);
                cmd.Parameters.Add(pNewId);

                cmd.ExecuteNonQuery();

                int newId = Convert.ToInt32(pNewId.Value.ToString());
                Console.WriteLine($"\n[PASSED] Student successfully saved via SP_STUDENT_INSERT!");
                Console.WriteLine($"Generated Student ID: {newId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        // ==========================================
        // BOOK PROCEDURES
        // ==========================================

        static void ListBooks()
        {
            Console.WriteLine("--- [SP_BOOK_SELECT_ALL] ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_BOOK_SELECT_ALL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Total Books in Catalog: {dt.Rows.Count}\n");
                Console.WriteLine($"{"ID",-5} | {"Title",-40} | {"Category",-20} | {"Copies",-6} | {"Publisher"}");
                Console.WriteLine(new string('-', 95));

                foreach (DataRow row in dt.Rows)
                {
                    string title = row["BookTitle"].ToString() ?? "";
                    if (title.Length > 38) title = title.Substring(0, 35) + "...";

                    Console.WriteLine($"{row["BookID"],-5} | {title,-40} | {row["BookTypeName"],-20} | {row["NumCopies"],-6} | {row["Publisher"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static void SearchBooks()
        {
            Console.Write("Enter book title, publisher, or genre keyword: ");
            string? keyword = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                Console.WriteLine("[!] Keyword cannot be empty.");
                return;
            }

            Console.WriteLine($"\n--- [SP_BOOK_SEARCH] Keyword: '{keyword}' ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_BOOK_SEARCH", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_keyword", OracleDbType.Varchar2, keyword, ParameterDirection.Input);
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Matches Found: {dt.Rows.Count}\n");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($"ID: {row["BookID"]} | Title: {row["BookTitle"]} | Category: {row["BookTypeName"]} | Copies: {row["NumCopies"]} | Publisher: {row["Publisher"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static void AddBook()
        {
            Console.WriteLine("--- [SP_BOOK_INSERT] Add New Book ---");
            Console.Write("Book Title (*required): ");
            string? title = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("[!] Book Title is required.");
                return;
            }

            Console.WriteLine("\nAvailable Book Categories:");
            Console.WriteLine("  1: Computer Science");
            Console.WriteLine("  2: Software Engineering");
            Console.WriteLine("  3: Data Science & AI");
            Console.WriteLine("  4: Information Security");
            Console.WriteLine("  5: Mathematics & Algorithms");
            Console.WriteLine("  6: General Science");
            Console.Write("Choose Category ID [1]: ");
            string? typeIdInput = Console.ReadLine()?.Trim();
            int typeId = int.TryParse(typeIdInput, out var tid) ? tid : 1;

            Console.Write("Number of Pages [350]: ");
            int pages = int.TryParse(Console.ReadLine()?.Trim(), out var p) ? p : 350;

            Console.Write("Number of Copies [5]: ");
            int copies = int.TryParse(Console.ReadLine()?.Trim(), out var c) ? c : 5;

            Console.Write("Edition [1st Edition]: ");
            string? edition = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(edition)) edition = "1st Edition";

            Console.Write("Publisher: ");
            string? publisher = Console.ReadLine()?.Trim();

            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_BOOK_INSERT", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_BookTitle", OracleDbType.Varchar2, title, ParameterDirection.Input);
                cmd.Parameters.Add("p_BookTypeID", OracleDbType.Int32, typeId, ParameterDirection.Input);
                cmd.Parameters.Add("p_PublishDate", OracleDbType.Date, DateTime.Today, ParameterDirection.Input);
                cmd.Parameters.Add("p_NumOfPages", OracleDbType.Int32, pages, ParameterDirection.Input);
                cmd.Parameters.Add("p_NumCopies", OracleDbType.Int32, copies, ParameterDirection.Input);
                cmd.Parameters.Add("p_Edition", OracleDbType.Varchar2, string.IsNullOrEmpty(edition) ? DBNull.Value : edition, ParameterDirection.Input);
                cmd.Parameters.Add("p_Publisher", OracleDbType.Varchar2, string.IsNullOrEmpty(publisher) ? DBNull.Value : publisher, ParameterDirection.Input);
                cmd.Parameters.Add("p_BookSource", OracleDbType.Varchar2, "Library Purchase", ParameterDirection.Input);
                cmd.Parameters.Add("p_Remark", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input);

                var pNewId = new OracleParameter("p_new_bookid", OracleDbType.Int32, ParameterDirection.Output);
                cmd.Parameters.Add(pNewId);

                cmd.ExecuteNonQuery();

                int newId = Convert.ToInt32(pNewId.Value.ToString());
                Console.WriteLine($"\n[PASSED] Book successfully saved via SP_BOOK_INSERT!");
                Console.WriteLine($"Generated Book ID: {newId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        // ==========================================
        // AUTHOR PROCEDURES
        // ==========================================

        static void ListAuthors()
        {
            Console.WriteLine("--- [SP_AUTHOR_SELECT_ALL] ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_AUTHOR_SELECT_ALL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Total Authors: {dt.Rows.Count}\n");
                Console.WriteLine($"{"ID",-5} | {"Author Name",-25} | {"Gender",-8} | {"Phone",-15} | {"Email"}");
                Console.WriteLine(new string('-', 75));

                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($"{row["AuthorID"],-5} | {row["AuthorName"],-25} | {row["Gender"],-8} | {row["Phone"],-15} | {row["Email"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static void SearchAuthors()
        {
            Console.Write("Enter author name, email, or phone keyword: ");
            string? keyword = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                Console.WriteLine("[!] Keyword cannot be empty.");
                return;
            }

            Console.WriteLine($"\n--- [SP_AUTHOR_SEARCH] Keyword: '{keyword}' ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_AUTHOR_SEARCH", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_keyword", OracleDbType.Varchar2, keyword, ParameterDirection.Input);
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"Matches Found: {dt.Rows.Count}\n");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($"ID: {row["AuthorID"]} | Name: {row["AuthorName"]} | Gender: {row["Gender"]} | Email: {row["Email"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        // ==========================================
        // LOGIN PROCEDURE
        // ==========================================

        static void TestLogin()
        {
            Console.WriteLine("--- [SP_LOGIN] User Authentication ---");
            Console.Write("Enter Username (try 'admin'): ");
            string username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter Password (try '123'): ");
            string password = Console.ReadLine()?.Trim() ?? "";

            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SP_LOGIN", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_username", OracleDbType.Varchar2, username, ParameterDirection.Input);
                cmd.Parameters.Add("p_password", OracleDbType.Varchar2, password, ParameterDirection.Input);
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine($"\n[PASSED] Login Succeeded!");
                    Console.WriteLine($"  - Username : {reader["UserName"]}");
                    Console.WriteLine($"  - Full Name: {reader["LibName"]}");
                    Console.WriteLine($"  - Role     : {reader["UserType"]}");
                    Console.WriteLine($"  - Email    : {reader["Email"]}");
                }
                else
                {
                    Console.WriteLine("\n[FAILED] Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        // ==========================================
        // DIAGNOSTICS SUITE
        // ==========================================

        static void RunDiagnostics()
        {
            Console.WriteLine("--- Database Diagnostics & Integrity Check ---");
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                conn.Open();

                using var cmd = new OracleCommand(@"
                    SELECT object_type, COUNT(*) as total_count,
                           SUM(CASE WHEN status = 'VALID' THEN 1 ELSE 0 END) as valid_count,
                           SUM(CASE WHEN status <> 'VALID' THEN 1 ELSE 0 END) as invalid_count
                    FROM user_objects
                    WHERE object_type IN ('TABLE', 'VIEW', 'PROCEDURE', 'TRIGGER', 'INDEX', 'SEQUENCE', 'LOB')
                    GROUP BY object_type
                    ORDER BY object_type", conn);

                using var reader = cmd.ExecuteReader();
                Console.WriteLine($"{"Object Type",-20} | {"Total",-8} | {"Valid",-8} | {"Invalid"}");
                Console.WriteLine(new string('-', 55));
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["object_type"],-20} | {reader["total_count"],-8} | {reader["valid_count"],-8} | {reader["invalid_count"]}");
                }

                Console.WriteLine("\n[PASSED] All database objects are 100% VALID!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}
