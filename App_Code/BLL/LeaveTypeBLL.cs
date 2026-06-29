using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class LeaveTypeBLL
    {
        public static List<LeaveType> GetAll() => DAL.LeaveTypeDAL.GetAll();
        public static LeaveType GetById(int leaveTypeId) => DAL.LeaveTypeDAL.GetById(leaveTypeId);
        public static int Insert(LeaveType lt) => DAL.LeaveTypeDAL.Insert(lt);
        public static int Update(LeaveType lt) => DAL.LeaveTypeDAL.Update(lt);
        public static int Delete(int leaveTypeId) => DAL.LeaveTypeDAL.Delete(leaveTypeId);
    }
}
