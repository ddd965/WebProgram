using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HRMS.DAL
{
    /// <summary>
    /// 数据库访问助手类 — 封装 ADO.NET 连接、命令、事务操作
    /// </summary>
    public static class DBHelper
    {
        /// <summary>
        /// 从 Web.config 获取 HRMSConnString 连接串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["HRMSConnString"].ConnectionString;
            }
        }

        /// <summary>
        /// 创建新的 SqlConnection
        /// </summary>
        public static SqlConnection CreateConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 执行非查询 SQL（INSERT/UPDATE/DELETE），返回受影响行数
        /// </summary>
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行 SQL 返回首行首列（如 COUNT、MAX）
        /// </summary>
        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 执行查询返回 SqlDataReader（调用方负责释放连接）
        /// </summary>
        public static SqlDataReader ExecuteReader(string sql, SqlParameter[] parameters = null)
        {
            var conn = CreateConnection();
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        /// <summary>
        /// 执行查询返回 DataTable
        /// </summary>
        public static DataTable ExecuteDataTable(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// 在事务中执行多条 SQL（每句一条 NonQuery）
        /// </summary>
        public static bool ExecuteInTransaction(params (string sql, SqlParameter[] parameters)[] statements)
        {
            using (var conn = CreateConnection())
            {
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var stmt in statements)
                        {
                            using (var cmd = new SqlCommand(stmt.sql, conn, tran))
                            {
                                if (stmt.parameters != null)
                                    cmd.Parameters.AddRange(stmt.parameters);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
