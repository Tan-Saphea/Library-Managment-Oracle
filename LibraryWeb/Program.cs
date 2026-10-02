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

        cmd.Parameters.Add("p_stu_name", OracleDbType.Varchar2, root.GetProperty("stuName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
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

        cmd.Parameters.Add("p_stu_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_stu_name", OracleDbType.Varchar2, root.GetProperty("stuName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

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
        cmd.Parameters.Add("p_stu_id", OracleDbType.Int32, id, ParameterDirection.Input);
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

app.MapPost("/api/booktypes", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOKTYPE_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        string typeName = root.GetProperty("bookTypeName").GetString()!;
        cmd.Parameters.Add("p_book_type_name", OracleDbType.Varchar2, typeName, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add(pNewId);

        cmd.ExecuteNonQuery();
        int newId = Convert.ToInt32(pNewId.Value.ToString());
        return Results.Ok(new { success = true, bookTypeID = newId });
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

        cmd.Parameters.Add("p_book_title", OracleDbType.Varchar2, root.GetProperty("bookTitle").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_book_type_id", OracleDbType.Int32, root.GetProperty("bookTypeID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_publish_date", OracleDbType.Date, root.TryGetProperty("publishDate", out var pd) && DateTime.TryParse(pd.GetString(), out var pdate) ? pdate : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_num_of_pages", OracleDbType.Int32, root.GetProperty("numOfPages").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_num_copies", OracleDbType.Int32, root.GetProperty("numCopies").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_edition", OracleDbType.Varchar2, root.TryGetProperty("edition", out var ed) ? ed.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_publisher", OracleDbType.Varchar2, root.TryGetProperty("publisher", out var pub) ? pub.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_book_source", OracleDbType.Varchar2, root.TryGetProperty("bookSource", out var bs) ? bs.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, root.TryGetProperty("remark", out var rem) ? rem.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
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

        cmd.Parameters.Add("p_book_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_book_title", OracleDbType.Varchar2, root.GetProperty("bookTitle").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_book_type_id", OracleDbType.Int32, root.GetProperty("bookTypeID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_publish_date", OracleDbType.Date, root.TryGetProperty("publishDate", out var pd) && DateTime.TryParse(pd.GetString(), out var pdate) ? pdate : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_num_of_pages", OracleDbType.Int32, root.GetProperty("numOfPages").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_num_copies", OracleDbType.Int32, root.GetProperty("numCopies").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_edition", OracleDbType.Varchar2, root.TryGetProperty("edition", out var ed) ? ed.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_publisher", OracleDbType.Varchar2, root.TryGetProperty("publisher", out var pub) ? pub.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_book_source", OracleDbType.Varchar2, root.TryGetProperty("bookSource", out var bs) ? bs.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, root.TryGetProperty("remark", out var rem) ? rem.GetString() : (object)DBNull.Value, ParameterDirection.Input);

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
        cmd.Parameters.Add("p_book_id", OracleDbType.Int32, id, ParameterDirection.Input);
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

app.MapPost("/api/authors", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_AUTHOR_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_author_name", OracleDbType.Varchar2, root.GetProperty("authorName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add(pNewId);

        cmd.ExecuteNonQuery();
        int newId = Convert.ToInt32(pNewId.Value.ToString());
        return Results.Ok(new { success = true, authorID = newId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/authors/{id:int}", async (int id, HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_AUTHOR_UPDATE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_author_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_author_name", OracleDbType.Varchar2, root.GetProperty("authorName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/authors/{id:int}", (int id) =>
{
    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_AUTHOR_DELETE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_author_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// BORROWS & CIRCULATION (SP_BORROW_*, SP_RETURN_*)
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

app.MapPost("/api/borrows", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var trans = conn.BeginTransaction();

        // 1. Insert Borrow Header
        using var cmdBorrow = new OracleCommand("SP_BORROW_INSERT", conn);
        cmdBorrow.Transaction = trans;
        cmdBorrow.CommandType = CommandType.StoredProcedure;
        cmdBorrow.BindByName = true;

        int stuId = root.GetProperty("stuID").GetInt32();
        int libId = root.GetProperty("libID").GetInt32();
        int bookId = root.GetProperty("bookID").GetInt32();
        int qty = root.TryGetProperty("qtyBorrow", out var qEl) ? qEl.GetInt32() : 1;
        string? remark = root.TryGetProperty("remark", out var rEl) ? rEl.GetString() : null;

        cmdBorrow.Parameters.Add("p_stu_id", OracleDbType.Int32, stuId, ParameterDirection.Input);
        cmdBorrow.Parameters.Add("p_lib_id", OracleDbType.Int32, libId, ParameterDirection.Input);
        cmdBorrow.Parameters.Add("p_borrow_date", OracleDbType.Date, DateTime.Today, ParameterDirection.Input);
        cmdBorrow.Parameters.Add("p_remark", OracleDbType.Varchar2, (object?)remark ?? DBNull.Value, ParameterDirection.Input);

        var pBorrowId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
        cmdBorrow.Parameters.Add(pBorrowId);
        cmdBorrow.ExecuteNonQuery();

        int borrowId = Convert.ToInt32(pBorrowId.Value.ToString());

        // 2. Insert Line Item (Triggers check & deduct stock automatically)
        using var cmdItem = new OracleCommand("SP_BORROWBOOK_INSERT", conn);
        cmdItem.Transaction = trans;
        cmdItem.CommandType = CommandType.StoredProcedure;
        cmdItem.BindByName = true;

        cmdItem.Parameters.Add("p_borrow_id", OracleDbType.Int32, borrowId, ParameterDirection.Input);
        cmdItem.Parameters.Add("p_book_id", OracleDbType.Int32, bookId, ParameterDirection.Input);
        cmdItem.Parameters.Add("p_qty_borrow", OracleDbType.Int32, qty, ParameterDirection.Input);
        cmdItem.ExecuteNonQuery();

        trans.Commit();
        return Results.Ok(new { success = true, borrowID = borrowId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// RETURNS (SP_RETURN_*, SP_RETURN_SELECT_ALL)
// -------------------------------------------------------------
app.MapGet("/api/returns", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_RETURN_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            returnID = Convert.ToInt32(r["ReturnID"]),
            borrowID = Convert.ToInt32(r["BorrowID"]),
            student = r["Student"]?.ToString(),
            book = r["Book"]?.ToString(),
            librarian = r["Librarian"]?.ToString(),
            qtyReturn = Convert.ToInt32(r["QtyReturn"]),
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

app.MapPost("/api/returns", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_RETURN_BOOK", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        int borrowId = root.GetProperty("borrowID").GetInt32();
        int bookId = root.GetProperty("bookID").GetInt32();
        int libId = root.GetProperty("libID").GetInt32();
        int qtyReturn = root.TryGetProperty("qtyReturn", out var qrEl) ? qrEl.GetInt32() : 1;
        string? remark = root.TryGetProperty("remark", out var rmEl) ? rmEl.GetString() : "Returned via system";

        cmd.Parameters.Add("p_borrow_id", OracleDbType.Int32, borrowId, ParameterDirection.Input);
        cmd.Parameters.Add("p_book_id", OracleDbType.Int32, bookId, ParameterDirection.Input);
        cmd.Parameters.Add("p_lib_id", OracleDbType.Int32, libId, ParameterDirection.Input);
        cmd.Parameters.Add("p_qty_return", OracleDbType.Int32, qtyReturn, ParameterDirection.Input);
        cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, (object?)remark ?? DBNull.Value, ParameterDirection.Input);

        var pReturnId = new OracleParameter("p_new_return_id", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add(pReturnId);
        cmd.ExecuteNonQuery();

        int returnId = Convert.ToInt32(pReturnId.Value.ToString());
        return Results.Ok(new { success = true, returnID = returnId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// LIBRARIANS (SP_LIBRARIAN_*)
// -------------------------------------------------------------
app.MapGet("/api/librarians", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_LIBRARIAN_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            libID = Convert.ToInt32(r["LibID"]),
            libName = r["LibName"]?.ToString(),
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

app.MapPost("/api/librarians", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_LIBRARIAN_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_lib_name", OracleDbType.Varchar2, root.GetProperty("libName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add(pNewId);

        cmd.ExecuteNonQuery();
        int newId = Convert.ToInt32(pNewId.Value.ToString());
        return Results.Ok(new { success = true, libID = newId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/librarians/{id:int}", async (int id, HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_LIBRARIAN_UPDATE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_lib_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.Parameters.Add("p_lib_name", OracleDbType.Varchar2, root.GetProperty("libName").GetString(), ParameterDirection.Input);
        cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, root.TryGetProperty("gender", out var g) ? g.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_dob", OracleDbType.Date, root.TryGetProperty("dob", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, root.TryGetProperty("pob", out var p) ? p.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_address", OracleDbType.Varchar2, root.TryGetProperty("address", out var a) ? a.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, root.TryGetProperty("phone", out var ph) ? ph.GetString() : (object)DBNull.Value, ParameterDirection.Input);
        cmd.Parameters.Add("p_email", OracleDbType.Varchar2, root.TryGetProperty("email", out var em) ? em.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        byte[]? photoBytes = null;
        if (root.TryGetProperty("photo", out var photoEl) && !string.IsNullOrEmpty(photoEl.GetString()))
        {
            photoBytes = Convert.FromBase64String(photoEl.GetString()!);
        }
        cmd.Parameters.Add("p_photo", OracleDbType.Blob, (object?)photoBytes ?? DBNull.Value, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/librarians/{id:int}", (int id) =>
{
    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_LIBRARIAN_DELETE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_lib_id", OracleDbType.Int32, id, ParameterDirection.Input);
        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// USER ACCOUNTS (SP_USER_*)
// -------------------------------------------------------------
app.MapGet("/api/users", () =>
{
    try
    {
        var dt = ExecuteRefCursor("SP_USER_SELECT_ALL");
        var list = dt.AsEnumerable().Select(r => new
        {
            libID = Convert.ToInt32(r["LibID"]),
            userName = r["UserName"]?.ToString(),
            userType = r["UserType"]?.ToString(),
            libName = r["LibName"]?.ToString(),
            email = r["Email"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/users", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_USER_SAVE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        int libId = root.GetProperty("libID").GetInt32();
        string username = root.GetProperty("userName").GetString()!;
        string password = root.GetProperty("userPassword").GetString()!;
        string role = root.TryGetProperty("userType", out var ut) ? ut.GetString()! : "Librarian";

        cmd.Parameters.Add("p_lib_id", OracleDbType.Int32, libId, ParameterDirection.Input);
        cmd.Parameters.Add("p_user_name", OracleDbType.Varchar2, username, ParameterDirection.Input);
        cmd.Parameters.Add("p_user_password", OracleDbType.Varchar2, password, ParameterDirection.Input);
        cmd.Parameters.Add("p_user_type", OracleDbType.Varchar2, role, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// -------------------------------------------------------------
// BOOK AUTHORS (SP_BOOKAUTHOR_*)
// -------------------------------------------------------------
app.MapGet("/api/bookauthors/{bookId:int}", (int bookId) =>
{
    try
    {
        var pBook = new OracleParameter("p_book_id", OracleDbType.Int32, bookId, ParameterDirection.Input);
        var dt = ExecuteRefCursor("SP_BOOKAUTHOR_SELECT_BY_BOOK", pBook);
        var list = dt.AsEnumerable().Select(r => new
        {
            bookID = Convert.ToInt32(r["BookID"]),
            authorID = Convert.ToInt32(r["AuthorID"]),
            authorName = r["AuthorName"]?.ToString(),
            authorDate = r["AuthorDate"] == DBNull.Value ? null : Convert.ToDateTime(r["AuthorDate"]).ToString("yyyy-MM-dd"),
            remark = r["Remark"]?.ToString()
        });
        return Results.Ok(list);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/bookauthors", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOKAUTHOR_INSERT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_book_id", OracleDbType.Int32, root.GetProperty("bookID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_author_id", OracleDbType.Int32, root.GetProperty("authorID").GetInt32(), ParameterDirection.Input);
        cmd.Parameters.Add("p_author_date", OracleDbType.Date, DateTime.Today, ParameterDirection.Input);
        cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, root.TryGetProperty("remark", out var rem) ? rem.GetString() : (object)DBNull.Value, ParameterDirection.Input);

        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/bookauthors/{bookId:int}/{authorId:int}", (int bookId, int authorId) =>
{
    try
    {
        using var conn = new OracleConnection(ConnectionString);
        conn.Open();
        using var cmd = new OracleCommand("SP_BOOKAUTHOR_DELETE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("p_book_id", OracleDbType.Int32, bookId, ParameterDirection.Input);
        cmd.Parameters.Add("p_author_id", OracleDbType.Int32, authorId, ParameterDirection.Input);
        cmd.ExecuteNonQuery();
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

