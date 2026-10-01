using System;
using System.Collections.Generic;
using System.Data;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Repositories
{
    public class AuthorRepository
    {
        public List<Author> GetAll()
        {
            var list = new List<Author>();
            var dt = OracleDb.ExecuteCursorProcedure("SP_AUTHOR_SELECT_ALL");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToAuthor(row));
            }
            return list;
        }

        public Author? GetById(int id)
        {
            var pId = new OracleParameter("p_author_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_AUTHOR_SELECT_BY_ID", pId);
            if (dt.Rows.Count > 0)
            {
                return MapRowToAuthor(dt.Rows[0]);
            }
            return null;
        }

        public int Insert(Author author)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_AUTHOR_INSERT", conn);

            cmd.Parameters.Add("p_author_name", OracleDbType.Varchar2, author.AuthorName, ParameterDirection.Input);
            cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Gender), ParameterDirection.Input);
            cmd.Parameters.Add("p_dob", OracleDbType.Date, OracleDb.ToDbValue(author.DOB), ParameterDirection.Input);
            cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, OracleDb.ToDbValue(author.POB), ParameterDirection.Input);
            cmd.Parameters.Add("p_address", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Address), ParameterDirection.Input);
            cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Phone), ParameterDirection.Input);
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Email), ParameterDirection.Input);
            cmd.Parameters.Add("p_photo", OracleDbType.Blob, OracleDb.ToDbValue(author.Photo), ParameterDirection.Input);

            var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
            cmd.Parameters.Add(pNewId);

            cmd.ExecuteNonQuery();

            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                author.AuthorID = Convert.ToInt32(pNewId.Value.ToString());
            }
            return author.AuthorID;
        }

        public void Update(Author author)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_AUTHOR_UPDATE", conn);

            cmd.Parameters.Add("p_author_id", OracleDbType.Decimal, author.AuthorID, ParameterDirection.Input);
            cmd.Parameters.Add("p_author_name", OracleDbType.Varchar2, author.AuthorName, ParameterDirection.Input);
            cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Gender), ParameterDirection.Input);
            cmd.Parameters.Add("p_dob", OracleDbType.Date, OracleDb.ToDbValue(author.DOB), ParameterDirection.Input);
            cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, OracleDb.ToDbValue(author.POB), ParameterDirection.Input);
            cmd.Parameters.Add("p_address", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Address), ParameterDirection.Input);
            cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Phone), ParameterDirection.Input);
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2, OracleDb.ToDbValue(author.Email), ParameterDirection.Input);
            cmd.Parameters.Add("p_photo", OracleDbType.Blob, OracleDb.ToDbValue(author.Photo), ParameterDirection.Input);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_AUTHOR_DELETE", conn);

            cmd.Parameters.Add("p_author_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            cmd.ExecuteNonQuery();
        }

        public List<Author> Search(string keyword)
        {
            var list = new List<Author>();
            var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, keyword ?? string.Empty, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_AUTHOR_SEARCH", pKeyword);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToAuthor(row));
            }
            return list;
        }

        private static Author MapRowToAuthor(DataRow row)
        {
            return new Author
            {
                AuthorID = Convert.ToInt32(row["AUTHORID"]),
                AuthorName = row["AUTHORNAME"]?.ToString() ?? string.Empty,
                Gender = row["GENDER"] == DBNull.Value ? null : row["GENDER"]?.ToString(),
                DOB = row["DOB"] == DBNull.Value ? null : Convert.ToDateTime(row["DOB"]),
                POB = row["POB"] == DBNull.Value ? null : row["POB"]?.ToString(),
                Address = row["ADDRESS"] == DBNull.Value ? null : row["ADDRESS"]?.ToString(),
                Phone = row["PHONE"] == DBNull.Value ? null : row["PHONE"]?.ToString(),
                Email = row["EMAIL"] == DBNull.Value ? null : row["EMAIL"]?.ToString(),
                Photo = row["PHOTO"] == DBNull.Value ? null : (byte[])row["PHOTO"]
            };
        }
    }
}
