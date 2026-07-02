using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class PositionBLL
    {
        public static List<Position> GetAll() => DAL.PositionDAL.GetAll();
        public static Position GetById(int positionId) => DAL.PositionDAL.GetById(positionId);
        public static int Insert(Position p) => DAL.PositionDAL.Insert(p);
        public static int Update(Position p) => DAL.PositionDAL.Update(p);
        public static int Delete(int positionId) => DAL.PositionDAL.Delete(positionId);
    }
}
