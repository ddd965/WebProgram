using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class EmployeeDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Employee";
        private const string SELECT_BY_ID = "SELECT * FROM Employee WHERE EmpId = @EmpId";
        private const string INSERT = @"INSERT INTO Employee(EmpNo, EmpName, Gender, Birthday, IdCard, HireDate,
                                         DeptId, PositionId, Phone, Email, Address, PhotoPath, Status)
                                         VALUES(@EmpNo,@EmpName,@Gender,@Birthday,@IdCard,@HireDate,
                                         @DeptId,@PositionId,@Phone,@Email,@Address,@PhotoPath,@Status);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Employee SET EmpNo=@EmpNo, EmpName=@EmpName, Gender=@Gender,
                                         Birthday=@Birthday, IdCard=@IdCard, HireDate=@HireDate,
                                         DeptId=@DeptId, PositionId=@PositionId, Phone=@Phone,
                                         Email=@Email, Address=@Address, PhotoPath=@PhotoPath,
                                         Status=@Status WHERE EmpId=@EmpId";
        private const string DELETE = "DELETE FROM Employee WHERE EmpId=@EmpId";

        public static List<Employee> GetAll()
        {
            var list = new List<Employee>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static Employee GetById(int empId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@EmpId", empId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Employee e)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(e)));
        }

        public static int Update(Employee e)
        {
            var parameters = new List<SqlParameter>(GetParams(e));
            parameters.Add(new SqlParameter("@EmpId", e.EmpId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int empId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@EmpId", empId) });
        }

        public static int SetStatus(int empId, string status)
        {
            return DBHelper.ExecuteNonQuery("UPDATE Employee SET Status=@Status WHERE EmpId=@EmpId",
                new SqlParameter[] {
                    new SqlParameter("@EmpId", empId),
                    new SqlParameter("@Status", status)
                });
        }

        /// <summary>
        /// 多条件组合查询
        /// </summary>
        public static List<Employee> Query(string empNo, string empName, int? deptId, int? positionId, string status)
        {
            var sql = new StringBuilder("SELECT * FROM Employee WHERE 1=1");
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(empNo))
            {
                sql.Append(" AND EmpNo LIKE @EmpNo");
                parameters.Add(new SqlParameter("@EmpNo", "%" + empNo + "%"));
            }
            if (!string.IsNullOrEmpty(empName))
            {
                sql.Append(" AND EmpName LIKE @EmpName");
                parameters.Add(new SqlParameter("@EmpName", "%" + empName + "%"));
            }
            if (deptId.HasValue)
            {
                sql.Append(" AND DeptId = @DeptId");
                parameters.Add(new SqlParameter("@DeptId", deptId.Value));
            }
            if (positionId.HasValue)
            {
                sql.Append(" AND PositionId = @PositionId");
                parameters.Add(new SqlParameter("@PositionId", positionId.Value));
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql.Append(" AND Status = @Status");
                parameters.Add(new SqlParameter("@Status", status));
            }

            var list = new List<Employee>();
            using (var dr = DBHelper.ExecuteReader(sql.ToString(), parameters.ToArray()))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        private static Employee Map(SqlDataReader dr)
        {
            return new Employee
            {
                EmpId = (int)dr["EmpId"],
                EmpNo = dr["EmpNo"].ToString(),
                EmpName = dr["EmpName"].ToString(),
                Gender = dr["Gender"].ToString(),
                Birthday = dr["Birthday"] as DateTime?,
                IdCard = dr["IdCard"] as string,
                HireDate = (DateTime)dr["HireDate"],
                DeptId = dr["DeptId"] as int?,
                PositionId = dr["PositionId"] as int?,
                Phone = dr["Phone"].ToString(),
                Email = dr["Email"].ToString(),
                Address = dr["Address"] as string,
                PhotoPath = dr["PhotoPath"] as string,
                Status = dr["Status"].ToString(),
                CreateTime = (DateTime)dr["CreateTime"]
            };
        }

        private static SqlParameter[] GetParams(Employee e)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@EmpNo", e.EmpNo),
                new SqlParameter("@EmpName", e.EmpName),
                new SqlParameter("@Gender", e.Gender),
                new SqlParameter("@Birthday", (object)e.Birthday ?? DBNull.Value),
                new SqlParameter("@IdCard", (object)e.IdCard ?? DBNull.Value),
                new SqlParameter("@HireDate", e.HireDate),
                new SqlParameter("@DeptId", (object)e.DeptId ?? DBNull.Value),
                new SqlParameter("@PositionId", (object)e.PositionId ?? DBNull.Value),
                new SqlParameter("@Phone", e.Phone),
                new SqlParameter("@Email", e.Email),
                new SqlParameter("@Address", (object)e.Address ?? DBNull.Value),
                new SqlParameter("@PhotoPath", (object)e.PhotoPath ?? DBNull.Value),
                new SqlParameter("@Status", e.Status ?? "在职")
            };
        }
    }
}
