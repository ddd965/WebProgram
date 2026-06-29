using System;
using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class UserBLL
    {
        public static List<User> GetAll() => DAL.UserDAL.GetAll();
        public static User GetById(int userId) => DAL.UserDAL.GetById(userId);
        public static User GetByUserName(string userName) => DAL.UserDAL.GetByUserName(userName);
        public static int Insert(User u) => DAL.UserDAL.Insert(u);
        public static int Update(User u) => DAL.UserDAL.Update(u);
        public static int Delete(int userId) => DAL.UserDAL.Delete(userId);

        /// <summary>
        /// 用户登录验证（返回 User 实体，null 表示失败）
        /// </summary>
        public static User Login(string userName, string password)
        {
            var user = DAL.UserDAL.GetByUserName(userName);
            if (user == null) return null;
            if (user.IsLocked) return null;
            if (user.Password != password) return null;

            // 更新最后登录时间
            DAL.UserDAL.UpdateLastLogin(user.UserId);
            return user;
        }
    }
}
