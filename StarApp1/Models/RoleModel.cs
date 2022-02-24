using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Security.Claims;

namespace StarApp1.Models
{
    public class RoleModel
    {
        [Key]
        public int RoleId { get; set; }
        
        public List<SelectListItem> RoleName { get; set; }
        public ClaimsIdentity? Login { get; internal set; }
    }
    //public class Class1
    //{
    //    string cn = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    //    public List<RoleModel> DropRole()
    //    {
    //        SqlConnection db = new SqlConnection(cn);
    //        string query = "SELECT RoleName, RoleId FROM LqpRoles";
    //    SqlCommand cmd = new SqlCommand(query, db);
    //        db.Open();

    //        List<RoleModel> list = new List<RoleModel>();
    //        using (IDataReader dataReader = cmd.ExecuteReader())
    //        {
    //            while (dataReader.Read())
    //            {
    //                RoleModel obj = new RoleModel();
    //                if (dataReader["RoleId"] != DBNull.Value)
    //                {
    //                    if (dataReader["RoleId "] != DBNull.Value) { obj.RoleId = (int)dataReader["RoleId"]; }
    //                    if (dataReader["RoleName"] != DBNull.Value) { obj.RoleName = (string)dataReader["RoleName"]; }
    //                    list.Add(obj);
    //                }
    //            }
    //            return list;
    //        }
    //    }
    //}

}
