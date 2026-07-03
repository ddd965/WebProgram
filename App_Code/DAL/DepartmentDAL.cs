using System.Collections.Generic;
using System.Data.SqlClient;
using HRMS.Model;

namespace HRMS.DAL
{
    /// <summary>
    /// Department 数据访问层
    /// </summary>
    public class DepartmentDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Department";
        private const string SELECT_BY_ID = "SELECT * FROM Department WHERE DeptId = @DeptId";
        private const string INSERT = @"INSERT INTO Department(DeptName, ParentId, ManagerId, Descn)
                                         VALUES(@DeptName, @ParentId, @ManagerId, @Descn); SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Department SET DeptName=@DeptName, ParentId=@ParentId,
                                         ManagerId=@ManagerId, Descn=@Descn WHERE DeptId=@DeptId";
        private const string DELETE = "DELETE FROM Department WHERE DeptId=@DeptId";

        public static List<Department> GetAll()
        {
            var list = new List<Department>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read())
                    list.Add(Map(dr));
            }
            return list;
        }

        public static Department GetById(int deptId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@DeptId", deptId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Department d)
        {
            return System.Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(d)));
        }

        public static int Update(Department d)
        {
            var parameters = new List<SqlParameter>(GetParams(d));
            parameters.Add(new SqlParameter("@DeptId", d.DeptId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int deptId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@DeptId", deptId) });
        }

        /// <summary>
        /// 将所有部门扁平化（强制 ParentId=NULL），保证九部门平级并列
        /// 若执行过就不再重复执行（通过输出受影响行数判断）
        /// </summary>
        public static int FlattenAllDepartments()
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE Department SET ParentId = NULL WHERE ParentId IS NOT NULL");
        }

        public static List<Department> GetSubDepartments(int parentId)
        {
            var list = new List<Department>();
            using (var dr = DBHelper.ExecuteReader("SELECT * FROM Department WHERE ParentId = @ParentId",
                new SqlParameter[] { new SqlParameter("@ParentId", parentId) }))
            {
                while (dr.Read())
                    list.Add(Map(dr));
            }
            return list;
        }

        private static Department Map(SqlDataReader dr)
        {
            return new Department
            {
                DeptId = (int)dr["DeptId"],
                DeptName = dr["DeptName"].ToString(),
                ParentId = dr["ParentId"] as int?,
                ManagerId = dr["ManagerId"] as int?,
                Descn = dr["Descn"] as string,
                CreateTime = (System.DateTime)dr["CreateTime"]
            };
        }

        private static SqlParameter[] GetParams(Department d)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@DeptName", d.DeptName),
                new SqlParameter("@ParentId", (object)d.ParentId ?? System.DBNull.Value),
                new SqlParameter("@ManagerId", (object)d.ManagerId ?? System.DBNull.Value),
                new SqlParameter("@Descn", (object)d.Descn ?? System.DBNull.Value)
            };
        }
    }
}
