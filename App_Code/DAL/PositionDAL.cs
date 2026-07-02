using System.Collections.Generic;
using System.Data.SqlClient;
using HRMS.Model;

namespace HRMS.DAL
{
    public class PositionDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Position";
        private const string SELECT_BY_ID = "SELECT * FROM Position WHERE PositionId = @PositionId";
        private const string INSERT = @"INSERT INTO Position(PositionName, Level, Descn)
                                         VALUES(@PositionName, @Level, @Descn); SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Position SET PositionName=@PositionName, Level=@Level,
                                         Descn=@Descn WHERE PositionId=@PositionId";
        private const string DELETE = "DELETE FROM Position WHERE PositionId=@PositionId";

        public static List<Position> GetAll()
        {
            var list = new List<Position>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static Position GetById(int positionId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@PositionId", positionId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Position p)
        {
            return System.Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(p)));
        }

        public static int Update(Position p)
        {
            var parameters = new List<SqlParameter>(GetParams(p));
            parameters.Add(new SqlParameter("@PositionId", p.PositionId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int positionId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@PositionId", positionId) });
        }

        private static Position Map(SqlDataReader dr)
        {
            return new Position
            {
                PositionId = (int)dr["PositionId"],
                PositionName = dr["PositionName"].ToString(),
                Level = dr["Level"] as int?,
                Descn = dr["Descn"] as string
            };
        }

        private static SqlParameter[] GetParams(Position p)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@PositionName", p.PositionName),
                new SqlParameter("@Level", (object)p.Level ?? System.DBNull.Value),
                new SqlParameter("@Descn", (object)p.Descn ?? System.DBNull.Value)
            };
        }
    }
}
