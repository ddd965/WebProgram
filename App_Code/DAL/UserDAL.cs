using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HRMS.Model;

namespace HRMS.DAL
{
    public class UserDAL
    {
        private const string SELECT_ALL = "SELECT * FROM [User]";
        private const string SELECT_BY_ID = "SELECT * FROM [User] WHERE UserId = @UserId";
        private const string SELECT_BY_USERNAME = "SELECT * FROM [User] WHERE UserName = @UserName";
        private const string INSERT = @"INSERT INTO [User](UserName, Password, EmpId, RoleName, IsLocked)
                                         VALUES(@UserName,@Password,@EmpId,@RoleName,@IsLocked);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE [User] SET UserName=@UserName, Password=@Password,
                                         EmpId=@EmpId, RoleName=@RoleName, IsLocked=@IsLocked
                                         WHERE UserId=@UserId";
        private const string DELETE = "DELETE FROM [User] WHERE UserId=@UserId";

        public static List<User> GetAll()
        {
            var list = new List<User>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static User GetById(int userId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@UserId", userId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static User GetByUserName(string userName)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_USERNAME,
                new SqlParameter[] { new SqlParameter("@UserName", userName) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(User u)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(u)));
        }

        public static int Update(User u)
        {
            var parameters = new List<SqlParameter>(GetParams(u));
            parameters.Add(new SqlParameter("@UserId", u.UserId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int userId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@UserId", userId) });
        }

        public static int UpdateLastLogin(int userId)
        {
            return DBHelper.ExecuteNonQuery("UPDATE [User] SET LastLoginTime=GETDATE() WHERE UserId=@UserId",
                new SqlParameter[] { new SqlParameter("@UserId", userId) });
        }

        private static User Map(SqlDataReader dr)
        {
            return new User
            {
                UserId = (int)dr["UserId"],
                UserName = dr["UserName"].ToString(),
                Password = dr["Password"].ToString(),
                EmpId = dr["EmpId"] as int?,
                RoleName = dr["RoleName"].ToString(),
                IsLocked = (bool)dr["IsLocked"],
                LastLoginTime = dr["LastLoginTime"] as DateTime?,
                CreateTime = (DateTime)dr["CreateTime"]
            };
        }

        private static SqlParameter[] GetParams(User u)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@UserName", u.UserName),
                new SqlParameter("@Password", u.Password),
                new SqlParameter("@EmpId", (object)u.EmpId ?? DBNull.Value),
                new SqlParameter("@RoleName", u.RoleName),
                new SqlParameter("@IsLocked", u.IsLocked)
            };
        }
    }
}
