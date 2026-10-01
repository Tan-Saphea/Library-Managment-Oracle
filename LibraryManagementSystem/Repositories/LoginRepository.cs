using System;
using System.Data;
using LibraryManagementSystem.Data;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Repositories
{
    public class LoginRepository
    {
        public bool ValidateLogin(string username, string password)
        {
            var pUser = new OracleParameter("p_username", OracleDbType.Varchar2, username, ParameterDirection.Input);
            var pPass = new OracleParameter("p_password", OracleDbType.Varchar2, password, ParameterDirection.Input);

            var dt = OracleDb.ExecuteCursorProcedure("SP_LOGIN", pUser, pPass);
            return dt.Rows.Count > 0;
        }
    }
}
