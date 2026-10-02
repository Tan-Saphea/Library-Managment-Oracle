using System;
using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5050");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

const string ConnectionString = "User Id=library_user;Password=Library123;Data Source=localhost:1521/FREEPDB1;";

// Helper to execute SP returning SYS_REFCURSOR
DataTable ExecuteRefCursor(string spName, params OracleParameter[] parameters)
{
    using var conn = new OracleConnection(ConnectionString);
    conn.Open();
    using var cmd = new OracleCommand(spName, conn);
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.BindByName = true;

    foreach (var p in parameters)
        cmd.Parameters.Add(p);

    var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
    cmd.Parameters.Add(cursorParam);

    using var da = new OracleDataAdapter(cmd);
    var dt = new DataTable();
    da.Fill(dt);
    return dt;
}

// -------------------------------------------------------------
// AUTHENTICATION (SP_LOGIN)
// -------------------------------------------------------------
app.MapPost("/api/login", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    string username = doc.RootElement.GetProperty("username").GetString() ?? "";
    string password = doc.RootElement.GetProperty("password").GetString() ?? "";

    try
    {
        var pUser = new OracleParameter("p_username", OracleDbType.Varchar2, username, ParameterDirection.Input);
        var pPass = new OracleParameter("p_password", OracleDbType.Varchar2, password, ParameterDirection.Input);
        var dt = ExecuteRefCursor("SP_LOGIN", pUser, pPass);

        if (dt.Rows.Count > 0)
        {
            var row = dt.Rows[0];
            return Results.Ok(new
            {
                success = true,
                username = row["UserName"]?.ToString(),
                fullName = row["LibName"]?.ToString(),
                role = row["UserType"]?.ToString(),
                email = row["Email"]?.ToString()
            });
        }
        return Results.Ok(new { success = false, message = "Invalid username or password." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, message = ex.Message });
    }
});

// -------------------------------------------------------------
// STUDENTS (SP_STUDENT_*)
// -------------------------------------------------------------
app.MapGet("/api/students", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_STUDENT_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            stuID = Convert.ToInt32(r["StuID"]),
            stuName = r["StuName"]?.ToString(),
            gender = r["Gender"]?.ToString(),
            dob = r["DOB"] == DBNull.Value ? null : Convert.ToDateTime(r["DOB"]).ToString("yyyy-MM-dd"),
            pob = r["POB"]?.ToString(),
            address = r["Address"]?.ToString(),
            phone = r["Phone"]?.ToString(),
            email = r["Email"]?.ToString(),
            photo = r["Photo"] == DBNull.Value ? null : Convert.ToBase64String((byte[])r["Photo"])
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/students/search", (string? q) =>
{
    try
    {
        var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, q ?? "", ParameterDirection.Input);
        var dt = ExecuteRefCursor("SP_STUDENT_SEARCH", pKeyword);
        var list = dt.AsEnumerable().Select(r => new
        {
            stuID = Convert.ToInt32(r["StuID"]),
            stuName = r["StuName"]?.ToString(),
            gender = r["Gender"]?.ToString(),
            dob = r["DOB"] == DBNull.Value ? null : Convert.ToDateTime(r["DOB"]).ToString("yyyy-MM-dd"),
            pob = r["POB"]?.ToString(),
            address = r["Address"]?.ToString(),
            phone = r["Phone"]?.ToString(),
            email = r["Email"]?.ToString(),
            photo = r["Photo"] == DBNull.Value ? null : Convert.ToBase64String((byte[])r["Photo"])
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/students", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_STUDENT_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_StuName", OracleDbType.Varchar2, root.GetProperty("stuName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_Gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_DOB", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_POB", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_Photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_stuid", OracleDbType.Int32, ParameterDirection.Output);
        cmd.Parameters.Add(pNewId);

        cmd.ExecuteNonQuery();
        int newId = Convert.ToInt32(pNewId.Value.ToString());
        return Results.Ok(new { success = true, stuID = newId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/students/{id:int}", async (int id, HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_STUDENT_UPDATE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_StuID", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_StuName", OracleDbType.Varchar2, root.GetProperty("stuName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_Gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_DOB", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_POB", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_Photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/students/{id:int}", (int id) =>
{
    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_STUDENT_DELETE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_StuID", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// BOOKS & BOOKTYPES (SP_BOOK_*, SP_BOOKTYPE_SELECT_ALL)
// -------------------------------------------------------------
app.MapGet("/api/booktypes", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_BOOKTYPE_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            bookTypeID = Convert.ToInt32(r["BookTypeID"]),
            bookTypeName = r["BookTypeName"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/books", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_BOOK_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            bookID = Convert.ToInt32(r["BookID"]),
            bookTitle = r["BookTitle"]?.ToString(),
            bookTypeID = Convert.ToInt32(r["BookTypeID"]),
            bookTypeName = r["BookTypeName"]?.ToString(),
            publishDate = r["PublishDate"] == DBNull.Value ? null : Convert.ToDateTime(r["PublishDate"]).ToString("yyyy-MM-dd"),
            numOfPages = Convert.ToInt32(r["NumOfPages"]),
            numCopies = Convert.ToInt32(r["NumCopies"]),
            edition = r["Edition"]?.ToString(),
            publisher = r["Publisher"]?.ToString(),
            bookSource = r["BookSource"]?.ToString(),
            remark = r["Remark"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/books/search", (string? q) =>
{
    try
    {
        var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, q ?? "", ParameterDirection.Input);
        var dt = ExecuteRefCursor("SP_BOOK_SEARCH", pKeyword);
        var list = dt.AsEnumerable().Select(r => new
        {
            bookID = Convert.ToInt32(r["BookID"]),
            bookTitle = r["BookTitle"]?.ToString(),
            bookTypeID = Convert.ToInt32(r["BookTypeID"]),
            bookTypeName = r["BookTypeName"]?.ToString(),
            publishDate = r["PublishDate"] == DBNull.Value ? null : Convert.ToDateTime(r["PublishDate"]).ToString("yyyy-MM-dd"),
            numOfPages = Convert.ToInt32(r["NumOfPages"]),
            numCopies = Convert.ToInt32(r["NumCopies"]),
            edition = r["Edition"]?.ToString(),
            publisher = r["Publisher"]?.ToString(),
            bookSource = r["BookSource"]?.ToString(),
            remark = r["Remark"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/books", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOK_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_BookTitle", OracleDbType.Varchar2, root.GetProperty("bookTitle").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_BookTypeID", OracleDbType.Int32, root.GetProperty("bookTypeID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_PublishDate", OracleDbType.Date, root.TryGetProperty("publishDate", out var pd) && DateTime.TryParse(pd.GetString(), out var pdate) ? pdate : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_NumOfPages", OracleDbType.Int32, root.GetProperty("numOfPages").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_NumCopies", OracleDbType.Int32, root.GetProperty("numCopies").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_Edition", OracleDbType.Varchar2, root.TryGetProperty("edition", out var ed) ? ed.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Publisher", OracleDbType.Varchar2, root.TryGetProperty("publisher", out var pub) ? pub.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_BookSource", OracleDbType.Varchar2, root.TryGetProperty("bookSource", out var bs) ? bs.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Remark", OracleDbType.Varchar2, root.TryGetProperty("remark", out var rem) ? rem.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_bookid", OracleDbType.Int32, ParameterDirection.Output);
        cmd.Parameters.Add(pNewId);

        cmd.ExecuteNonQuery();
        int newId = Convert.ToInt32(pNewId.Value.ToString());
        return Results.Ok(new { success = true, bookID = newId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/books/{id:int}", async (int id, HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOK_UPDATE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_BookID", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_BookTitle", OracleDbType.Varchar2, root.GetProperty("bookTitle").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_BookTypeID", OracleDbType.Int32, root.GetProperty("bookTypeID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_PublishDate", OracleDbType.Date, root.TryGetProperty("publishDate", out var pd) && DateTime.TryParse(pd.GetString(), out var pdate) ? pdate : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_NumOfPages", OracleDbType.Int32, root.GetProperty("numOfPages").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_NumCopies", OracleDbType.Int32, root.GetProperty("numCopies").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_Edition", OracleDbType.Varchar2, root.TryGetProperty("edition", out var ed) ? ed.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Publisher", OracleDbType.Varchar2, root.TryGetProperty("publisher", out var pub) ? pub.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_BookSource", OracleDbType.Varchar2, root.TryGetProperty("bookSource", out var bs) ? bs.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_Remark", OracleDbType.Varchar2, root.TryGetProperty("remark", out var rem) ? rem.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOK_DELETE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_BookID", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// AUTHORS (SP_AUTHOR_*)
// -------------------------------------------------------------
app.MapGet("/api/authors", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_AUTHOR_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            authorID = Convert.ToInt32(r["AuthorID"]),
            authorName = r["AuthorName"]?.ToString(),
            gender = r["Gender"]?.ToString(),
            dob = r["DOB"] == DBNull.Value ? null : Convert.ToDateTime(r["DOB"]).ToString("yyyy-MM-dd"),
            pob = r["POB"]?.ToString(),
            address = r["Address"]?.ToString(),
            phone = r["Phone"]?.ToString(),
            email = r["Email"]?.ToString(),
            photo = r["Photo"] == DBNull.Value ? null : Convert.ToBase64String((byte[])r["Photo"])
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/authors/search", (string? q) =>
{
    try
    {
        var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, q ?? "", ParameterDirection.Input);
        var dt = ExecuteRefCursor("SP_AUTHOR_SEARCH", pKeyword);
        var list = dt.AsEnumerable().Select(r => new
        {
            authorID = Convert.ToInt32(r["AuthorID"]),
            authorName = r["AuthorName"]?.ToString(),
            gender = r["Gender"]?.ToString(),
            dob = r["DOB"] == DBNull.Value ? null : Convert.ToDateTime(r["DOB"]).ToString("yyyy-MM-dd"),
            pob = r["POB"]?.ToString(),
            address = r["Address"]?.ToString(),
            phone = r["Phone"]?.ToString(),
            email = r["Email"]?.ToString(),
            photo = r["Photo"] == DBNull.Value ? null : Convert.ToBase64String((byte[])r["Photo"])
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// BORROWS & CIRCULATION (SP_BORROW_SELECT_ALL)
// -------------------------------------------------------------
app.MapGet("/api/borrows", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_BORROW_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            borrowID = Convert.ToInt32(r["BorrowID"]),
            stuID = Convert.ToInt32(r["StuID"]),
            stuName = r["StuName"]?.ToString(),
            libID = Convert.ToInt32(r["LibID"]),
            libName = r["LibName"]?.ToString(),
            borrowDate = r["BorrowDate"] == DBNull.Value ? null : Convert.ToDateTime(r["BorrowDate"]).ToString("yyyy-MM-dd"),
            bookID = Convert.ToInt32(r["BookID"]),
            bookTitle = r["BookTitle"]?.ToString(),
            qtyBorrow = Convert.ToInt32(r["QtyBorrow"]),
            isReturned = Convert.ToInt32(r["IsReturned"]) == 1,
            returnDate = r["ReturnDate"] == DBNull.Value ? null : Convert.ToDateTime(r["ReturnDate"]).ToString("yyyy-MM-dd"),
            remark = r["Remark"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

