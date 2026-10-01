using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Data
{
    public static class OracleDb
    {
        // Centralized Oracle Database Connection String for Docker container
        public static string ConnectionString { get; set; } =
            "User Id=library_user;Password=Library123;Data Source=localhost:1521/FREEPDB1;";

        public static OracleConnection GetConnection()
        {
            var conn = new OracleConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static OracleCommand CreateStoredProcCommand(string procName, OracleConnection conn)
        {
            var cmd = new OracleCommand(procName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true
            };
            return cmd;
        }

        public static DataTable ExecuteCursorProcedure(string procName, params OracleParameter[] parameters)
        {
            var dt = new DataTable();
            using var conn = GetConnection();
            using var cmd = CreateStoredProcCommand(procName, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            bool hasCursor = false;
            foreach (OracleParameter p in cmd.Parameters)
            {
                if (p.OracleDbType == OracleDbType.RefCursor)
                {
                    hasCursor = true;
                    break;
                }
            }

            if (!hasCursor)
            {
                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                cmd.Parameters.Add(cursorParam);
            }

            using var adapter = new OracleDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string procName, params OracleParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = CreateStoredProcCommand(procName, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd.ExecuteNonQuery();
        }

        public static object ToDbValue(object? value)
        {
            return value ?? DBNull.Value;
        }
    }
}
