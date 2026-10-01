using System;
using System.Collections.Generic;
using System.Data;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Repositories
{
    public class StudentRepository
    {
        public List<Student> GetAll()
        {
            var list = new List<Student>();
            var dt = OracleDb.ExecuteCursorProcedure("SP_STUDENT_SELECT_ALL");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToStudent(row));
            }
            return list;
        }

        public Student? GetById(int id)
        {
            var pId = new OracleParameter("p_stu_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_STUDENT_SELECT_BY_ID", pId);
            if (dt.Rows.Count > 0)
            {
                return MapRowToStudent(dt.Rows[0]);
            }
            return null;
        }

        public int Insert(Student student)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_STUDENT_INSERT", conn);

            cmd.Parameters.Add("p_stu_name", OracleDbType.Varchar2, student.StuName, ParameterDirection.Input);
            cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Gender), ParameterDirection.Input);
            cmd.Parameters.Add("p_dob", OracleDbType.Date, OracleDb.ToDbValue(student.DOB), ParameterDirection.Input);
            cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, OracleDb.ToDbValue(student.POB), ParameterDirection.Input);
            cmd.Parameters.Add("p_address", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Address), ParameterDirection.Input);
            cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Phone), ParameterDirection.Input);
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Email), ParameterDirection.Input);
            cmd.Parameters.Add("p_photo", OracleDbType.Blob, OracleDb.ToDbValue(student.Photo), ParameterDirection.Input);

            var pNewId = new OracleParameter("p_new_id", OracleDbType.Decimal, ParameterDirection.Output);
            cmd.Parameters.Add(pNewId);

            cmd.ExecuteNonQuery();

            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                student.StuID = Convert.ToInt32(pNewId.Value.ToString());
            }
            return student.StuID;
        }

        public void Update(Student student)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_STUDENT_UPDATE", conn);

            cmd.Parameters.Add("p_stu_id", OracleDbType.Decimal, student.StuID, ParameterDirection.Input);
            cmd.Parameters.Add("p_stu_name", OracleDbType.Varchar2, student.StuName, ParameterDirection.Input);
            cmd.Parameters.Add("p_gender", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Gender), ParameterDirection.Input);
            cmd.Parameters.Add("p_dob", OracleDbType.Date, OracleDb.ToDbValue(student.DOB), ParameterDirection.Input);
            cmd.Parameters.Add("p_pob", OracleDbType.Varchar2, OracleDb.ToDbValue(student.POB), ParameterDirection.Input);
            cmd.Parameters.Add("p_address", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Address), ParameterDirection.Input);
            cmd.Parameters.Add("p_phone", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Phone), ParameterDirection.Input);
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2, OracleDb.ToDbValue(student.Email), ParameterDirection.Input);
            cmd.Parameters.Add("p_photo", OracleDbType.Blob, OracleDb.ToDbValue(student.Photo), ParameterDirection.Input);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = OracleDb.GetConnection();
            using var cmd = OracleDb.CreateStoredProcCommand("SP_STUDENT_DELETE", conn);

            cmd.Parameters.Add("p_stu_id", OracleDbType.Decimal, id, ParameterDirection.Input);
            cmd.ExecuteNonQuery();
        }

        public List<Student> Search(string keyword)
        {
            var list = new List<Student>();
            var pKeyword = new OracleParameter("p_keyword", OracleDbType.Varchar2, keyword ?? string.Empty, ParameterDirection.Input);
            var dt = OracleDb.ExecuteCursorProcedure("SP_STUDENT_SEARCH", pKeyword);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToStudent(row));
            }
            return list;
        }

        private static Student MapRowToStudent(DataRow row)
        {
            return new Student
            {
                StuID = Convert.ToInt32(row["STUID"]),
                StuName = row["STUNAME"]?.ToString() ?? string.Empty,
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
