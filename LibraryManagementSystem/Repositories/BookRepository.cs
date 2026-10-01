using System;
using System.Collections.Generic;
using System.Data;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository
    {
        public List<Book> GetAll()
        {
            var list = new List<Book>();
            var dt = OracleDb.ExecuteCursorProcedure("SP_BOOK_SELECT_ALL");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToBook(row));
            }
            return list;
        }

        public Book? GetById(int id)
        {
            var pId = new OracleParameter("p_book_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_BOOK_SELECT_BY_ID", pId);
            if (dt.Rows.Count > 0)
            {
                return MapRowToBook(dt.Rows[0]);
            }
            return null;
        }

        public int Insert(Book book)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_BOOK_INSERT", conn);

            cmd.Parameters.Add("p_book_title", OracleDbType.Varchar2, book.BookTitle, ParameterDirection.Input);
            cmd.Parameters.Add("p_book_type_id", OracleDbType.Decimal, book.BookTypeID, ParameterDirection.Input);
            cmd.Parameters.Add("p_publish_date", OracleDbType.Date, OracleDb.ToDbValue(book.PublishDate), ParameterDirection.Input);
            cmd.Parameters.Add("p_num_of_pages", OracleDbType.Decimal, book.NumOfPages, ParameterDirection.Input);
            cmd.Parameters.Add("p_num_copies", OracleDbType.Decimal, book.NumCopies, ParameterDirection.Input);
            cmd.Parameters.Add("p_edition", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Edition), ParameterDirection.Input);
            cmd.Parameters.Add("p_publisher", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Publisher), ParameterDirection.Input);
            cmd.Parameters.Add("p_book_source", OracleDbType.Varchar2, OracleDb.ToDbValue(book.BookSource), ParameterDirection.Input);
            cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Remark), ParameterDirection.Input);

            var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
            cmd.Parameters.Add(pNewId);

            cmd.ExecuteNonQuery();

            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                book.BookID = Convert.ToInt32(pNewId.Value.ToString());
            }
            return book.BookID;
        }

        public void Update(Book book)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_BOOK_UPDATE", conn);

            cmd.Parameters.Add("p_book_id", OracleDbType.Decimal, book.BookID, ParameterDirection.Input);
            cmd.Parameters.Add("p_book_title", OracleDbType.Varchar2, book.BookTitle, ParameterDirection.Input);
            cmd.Parameters.Add("p_book_type_id", OracleDbType.Decimal, book.BookTypeID, ParameterDirection.Input);
            cmd.Parameters.Add("p_publish_date", OracleDbType.Date, OracleDb.ToDbValue(book.PublishDate), ParameterDirection.Input);
            cmd.Parameters.Add("p_num_of_pages", OracleDbType.Decimal, book.NumOfPages, ParameterDirection.Input);
            cmd.Parameters.Add("p_num_copies", OracleDbType.Decimal, book.NumCopies, ParameterDirection.Input);
            cmd.Parameters.Add("p_edition", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Edition), ParameterDirection.Input);
            cmd.Parameters.Add("p_publisher", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Publisher), ParameterDirection.Input);
            cmd.Parameters.Add("p_book_source", OracleDbType.Varchar2, OracleDb.ToDbValue(book.BookSource), ParameterDirection.Input);
            cmd.Parameters.Add("p_remark", OracleDbType.Varchar2, OracleDb.ToDbValue(book.Remark), ParameterDirection.Input);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_BOOK_DELETE", conn);

            cmd.Parameters.Add("p_book_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            cmd.ExecuteNonQuery();
        }

        public List<Book> Search(string keyword)
        {
            var list = new List<Book>();
            var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, keyword ?? string.Empty, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_BOOK_SEARCH", pKeyword);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToBook(row));
            }
            return list;
        }

        public List<BookType> GetBookTypes()
        {
            var list = new List<BookType>();
            var dt = OracleDb.ExecuteCursorProcedure("SP_BOOKTYPE_SELECT_ALL");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new BookType
                {
                    BookTypeID = Convert.ToInt32(row["BOOKTYPEID"]),
                    BookTypeName = row["BOOKTYPENAME"]?.ToString() ?? string.Empty
                });
            }
            return list;
        }

        private static Book MapRowToBook(DataRow row)
        {
            return new Book
            {
                BookID = Convert.ToInt32(row["BOOKID"]),
                BookTitle = row["BOOKTITLE"]?.ToString() ?? string.Empty,
                BookTypeID = Convert.ToInt32(row["BOOKTYPEID"]),
                BookTypeName = row.Table.Columns.Contains("BOOKTYPENAME") ? row["BOOKTYPENAME"]?.ToString() : null,
                PublishDate = row["PUBLISHDATE"] == DBNull.Value ? null : Convert.ToDateTime(row["PUBLISHDATE"]),
                NumOfPages = row["NUMOFPAGES"] == DBNull.Value ? 0 : Convert.ToInt32(row["NUMOFPAGES"]),
                NumCopies = Convert.ToInt32(row["NUMCOPIES"]),
                Edition = row["EDITION"] == DBNull.Value ? null : row["EDITION"]?.ToString(),
                Publisher = row["PUBLISHER"] == DBNull.Value ? null : row["PUBLISHER"]?.ToString(),
                BookSource = row["BOOKSOURCE"] == DBNull.Value ? null : row["BOOKSOURCE"]?.ToString(),
                Remark = row["REMARK"] == DBNull.Value ? null : row["REMARK"]?.ToString()
            };
        }
    }
}
