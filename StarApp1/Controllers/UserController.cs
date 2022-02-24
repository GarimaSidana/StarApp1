
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using StarApp1.Models;
using System.Web;
using Microsoft.AspNetCore.Session;
using System.Data.OleDb;

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace StarApp1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public static List<AllowanceDashboardViewModel> listEmployee2 = new List<AllowanceDashboardViewModel>();

        public IConfiguration Configuration;
        public IConfiguration Session;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<AuthenticationHandler> _logger;
        public UserController(IConfiguration _configuration,IConfiguration session, IHttpContextAccessor httpContext, ILogger<AuthenticationHandler> logger)
        {
            Session = session;
            Configuration = _configuration;
            _httpContext = httpContext;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        
        public IActionResult SignIn()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel model, string ReturnUrl = "")           //user login
        {
            string msg = "";
            var list = new List<LoginViewModel>();
            if (!ModelState.IsValid)
            {
                SqlConnection con = ConnectToSql();
                con.Open();
                string SqlQuery = "select UserId,RoleId,Password from UserInfo where UserId=@UserId /*and Password=@Password*/";
                SqlCommand cmd = new SqlCommand(SqlQuery, con);
                cmd.Parameters.AddWithValue("@UserId", model.UserName);
                cmd.Parameters.AddWithValue("@Password", model.Password);
                SqlDataReader sdr = cmd.ExecuteReader();
                var user = new LoginViewModel();
                while (sdr.Read())
                {
                    Session["name"] = model.UserName;
                    Session["Password"]=model.Password;
                    user.UserName = (string)sdr["UserId"];
                    user.Password = (string)sdr["Password"];
                    user.RoleId = Convert.ToInt32(sdr["RoleId"]);
                    list.Add(user);

                }
                con.Close();
                AuthenticationProperties authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,

                    ExpiresUtc = DateTimeOffset.Now.AddMinutes(15),

                   // ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt32(Configuration["SessionTimeoutInMinutes"])),

                    IsPersistent = model.RememberMe,

                    IssuedUtc = DateTime.UtcNow,

                    //RedirectUri = ReturnUrl


                };
                
                List<Claim> claims = new List<Claim> {new Claim(ClaimTypes.Name, model.UserName) };

                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await _httpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),authProperties);

                //if (string.Compare(Crypto.Hash(model.Password),user.Password ) == 0)
                //{
                //    int timeout = model.RememberMe ? 525600 : 10; // 525600 min = 1 year
                //    var ticket = new FormsAuthenticationTicket(model.UserName, model.RememberMe, timeout);
                //    string encrypted = FormsAuthentication.Encrypt(ticket);
                //    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                //    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                //    cookie.HttpOnly = true;
                //    Response.Cookies.Add(cookie);


                //    if (Url.IsLocalUrl(ReturnUrl))
                //    {
                //        return Redirect(ReturnUrl);
                //    }
                //    else
                //    {
                //        if (list.Count > 0)
                //        {
                //            if (user.RoleId == 1)
                //            {
                //                return RedirectToAction("UserAdmin");

                //            }
                //            else

                //            {
                //                return RedirectToAction("Dashboard");
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    msg = "Invalid Credentials";

                //}

            }
            
            
            ViewBag.Message = msg;
            return RedirectToAction("Dashboard");



        }
        
        //[HttpPost]
        //public ActionResult Logout()
        //{
        //    FormsAuthentication.SignOut();
        //    return RedirectToAction("SignIn");
        //}
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SignUp()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.RoleName = PopulateRole();
            return View(model);

        }
        [HttpPost]
        [AllowAnonymous]
        
        //[ValidateAntiForgeryToken]
        public IActionResult SignUp(RegisterViewModel model)   //User Registration
        {
            bool status = false;
            string msg = "";

            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email already Exist ?
                var isExist = IsEmailExist(model.UserName);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already requested for access");
                    return View(model);
                }
                #endregion
                //#region Generate Activation Code 
                //model.ActivationCode = Guid.NewGuid();
                //#endregion

                #region  Password Hashing 
                model.Password = Crypto.Hash(model.Password);
                model.ConfirmPassword = Crypto.Hash(model.ConfirmPassword);
                #endregion
                model.RoleName = PopulateRole();
                #region Inserting Data in Db
                var selectedItem = model.RoleName.Find(p => p.Value == model.RoleId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;

                }

                SqlConnection cn = ConnectToSql();
                cn.Open();
                string querry = "insert into UserInfo values(@userId,@Name,@RoleId,3,CURRENT_TIMESTAMP,NULL,@activatedby,@password)";
                SqlCommand cmd = new SqlCommand(querry, cn);

                cmd.Parameters.AddWithValue("@userId", model.UserName);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                cmd.Parameters.AddWithValue("@activatedby", model.Name);
                cmd.Parameters.AddWithValue("@password", model.Password);

                int count = cmd.ExecuteNonQuery();
                #endregion
                if (count == 1)
                {
                    msg = "User registerd successfully :)";
                    status = true;
                }

            }
            
            else
            {
                msg = "Invalid Request";
            }
            ViewBag.Message = msg;
            ViewBag.Status = status;
            return View(model);
        }
        [NonAction]
        public bool IsEmailExist(string userName)
        {
            SqlConnection con = ConnectToSql();
            con.Open();
            string SqlQuery = "select UserId from UserInfo where UserId=@UserId";
            SqlCommand cmd = new SqlCommand(SqlQuery, con);
            cmd.Parameters.AddWithValue("@UserId", userName);
            
            SqlDataReader sdr = cmd.ExecuteReader();
            return sdr.HasRows?true:false;
        }

        [HttpGet]
        
        
        public IActionResult UserAdmin(UserAdmin model)       // User Admin Dashboard
        {
            SqlConnection cn = ConnectToSql();
            cn.Open();
            string querry = "select a.UserId , a.Name , a.ActiveTime , b.RoleName , c.Status From UserInfo as a join LqpRoles as b on a.RoleId = b.RoleId join LqpStatus as c on a.StatusId = c.StatusId ";
            SqlCommand cmd = new SqlCommand(querry, cn);
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                List<UserAdmin> listEmployee = new List<UserAdmin>();
                while (sdr.Read())
                {
                    var employee = new UserAdmin();
                    employee.UserName = Convert.ToString(sdr["UserId"]);
                    employee.Name = Convert.ToString(sdr["Name"]);
                    employee.ActiveFrom = Convert.ToString(sdr["ActiveTime"]);
                    employee.Role = Convert.ToString(sdr["RoleName"]);
                    employee.Status = Convert.ToString(sdr["Status"]);
                    listEmployee.Add(employee);

                }
                cn.Close();
                return View(listEmployee);
            }

        }
        [HttpPost]
        
        public IActionResult UserAdmin()
        {
            SqlConnection cn = ConnectToSql();
            cn.Open();
            string querry = "select a.UserId , a.Name , a.ActiveTime , b.RoleName , c.Status From UserInfo as a join LqpRoles as b on a.RoleId = b.RoleId join LqpStatus as c on a.StatusId = c.StatusId ";
            SqlCommand cmd = new SqlCommand(querry, cn);
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                List<UserAdmin> listEmployee = new List<UserAdmin>();
                while (sdr.Read())
                {
                    var employee = new UserAdmin();
                    employee.UserName = Convert.ToString(sdr["UserId"]);
                    employee.Name = Convert.ToString(sdr["Name"]);
                    employee.ActiveFrom = Convert.ToString(sdr["ActiveTime"]);
                    employee.Role = Convert.ToString(sdr["RoleName"]);
                    employee.Status = Convert.ToString(sdr["Status"]);
                    listEmployee.Add(employee);

                }
                cn.Close();
                return View(listEmployee);
            }


        }                   //Admin Dashboard
        [HttpGet]
        
        
        public IActionResult Dashboard()
        {
            AllowanceDashboardViewModel model = new AllowanceDashboardViewModel();
            model.Period = PopulatePeriod();
            model.Project = PopulateProject();
            SqlConnection cn = ConnectToSql();
            cn.Open();
            string querry = "Select Resource_name , Resource_Id, Hours ,Approval_Status from export_data where Resource_Id is not null  ";

            SqlCommand cmd = new SqlCommand(querry, cn);


            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                List<AllowanceDashboardViewModel> listEmployee = new List<AllowanceDashboardViewModel>();
                while (sdr.Read())
                {

                    var employee = new AllowanceDashboardViewModel();
                    employee.Name = Convert.ToString(sdr["Resource_Name"]);
                    employee.SAPid = Convert.ToInt32(sdr["Resource_Id"]);
                    employee.Hours = Convert.ToInt32(sdr["Hours"]);
                    employee.LeaveHours = 0;
                    employee.AfternoonShiftDays = Convert.ToInt32(sdr["Hours"]) / 8;
                    employee.NightShiftDays = 0;
                    employee.TotalDays = employee.AfternoonShiftDays + employee.NightShiftDays;
                    employee.TransportAllowance = employee.TotalDays * 150;
                    employee.TotalAllowance = employee.TotalDays * 300;
                    employee.ApprovalStatus = Convert.ToString(sdr["Approval_Status"]);

                    listEmployee.Add(employee);

                }
                cn.Close();
                ViewBag.listEmployee = listEmployee;



            }
            return View(model);
            //return View();
        }                   //User Dashboard
        [HttpPost]
        
        
        public IActionResult Dashboard(AllowanceDashboardViewModel model)
        {
            model.Period = PopulatePeriod();

            model.Project = PopulateProject();

            var selectedItem = model.Period.Find(p => p.Value == model.PeriodStart.ToString());
            var selectedItem1 = model.Project.Find(p => p.Value == model.ProjectId.ToString());
            if (selectedItem != null && selectedItem1 != null)
            {
                selectedItem.Selected = true;
                selectedItem1.Selected = true;

            }

            SqlConnection cn = ConnectToSql();
            cn.Open();
            string querry = "Select Resource_name , Resource_Id, Hours, Approval_Status,Project_Id,Period_Start from export_data where Period_Start = @periodStart  AND Project_Id = @ProjectId ";

            SqlCommand cmd = new SqlCommand(querry, cn);
            cmd.Parameters.AddWithValue("@periodStart", model.PeriodStart);
            cmd.Parameters.AddWithValue("@ProjectId", model.ProjectId);
            List<AllowanceDashboardViewModel> listEmployee1 = new List<AllowanceDashboardViewModel>();
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {

                while (sdr.Read())
                {

                    var employee = new AllowanceDashboardViewModel();
                    employee.Name = Convert.ToString(sdr["Resource_Name"]);
                    employee.SAPid = Convert.ToInt32(sdr["Resource_Id"]);
                    employee.Hours = Convert.ToInt32(sdr["Hours"]);
                    employee.LeaveHours = 0;
                    employee.AfternoonShiftDays = Convert.ToInt32(sdr["Hours"]) / 8;
                    employee.NightShiftDays = 0;
                    employee.TotalDays = employee.AfternoonShiftDays + employee.NightShiftDays;
                    employee.TransportAllowance = employee.TotalDays * 150;
                    employee.TotalAllowance = employee.TotalDays * 300;

                    employee.PeriodStart = Convert.ToDateTime(sdr["Period_Start"]);
                    employee.ProjectId = Convert.ToString(sdr["Project_Id"]);
                    employee.ApprovalStatus = Convert.ToString(sdr["Approval_Status"]);

                    listEmployee1.Add(employee);

                }
                cn.Close();
                listEmployee2 = listEmployee1;
                ViewBag.listEmployee = listEmployee1;



            }

            return View(model);
        }


        public List<SelectListItem> PopulatePeriod()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            using (SqlConnection con = ConnectToSql())
            {
                string query = "select distinct concat (Period_Start,' TO  ',Period_End ) as Period , Period_Start from export_data where Period_Start is not null";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows && items != null)
                        {
                            while (sdr.Read())
                            {

                                items.Add(new SelectListItem
                                {
                                    Value = sdr["Period_Start"].ToString(),
                                    Text = sdr["Period"].ToString()
                                });


                            }

                        }
                    }
                    con.Close();
                }
            }
            return items;

        }        //Dropdown For Period

        public List<SelectListItem> PopulateProject()
        {
            List<SelectListItem> items1 = new List<SelectListItem>();

            using (SqlConnection con = ConnectToSql())
            {
                string query = "select distinct Project_Id , Project_Name from export_data where Project_Name is not null";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows && items1 != null)
                        {
                            while (sdr.Read())
                            {

                                items1.Add(new SelectListItem
                                {
                                    Value = sdr["Project_Id"].ToString(),
                                    Text = sdr["Project_Name"].ToString()
                                });


                            }

                        }
                    }
                    con.Close();
                }
            }
            return items1;

        }       //Dropdown for Project


        private List<SelectListItem> PopulateRole()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            string constr = this.Configuration.GetConnectionString("MYConnector");
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = " SELECT RoleName, RoleId FROM LqpRoles";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["RoleName"].ToString(),
                                    Value = sdr["RoleId"].ToString()
                                });
                            }

                        }

                    }
                    con.Close();
                }
            }
            return items;

        }         //Dropdown for Role

        private SqlConnection ConnectToSql()
        {
            string constr = this.Configuration.GetConnectionString("MYConnector");
            SqlConnection con = new SqlConnection(constr);

            return con;
        }                //Connection with Db
        
        public IActionResult getdata(string UserName)
        {
            SqlConnection con = ConnectToSql();
            con.Open();
            string querry = "Update UserInfo set StatusId = 2, ActiveTime = default  where UserId=@UserName";
            SqlCommand cmd = new SqlCommand(querry, con);
            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("UserAdmin");

        }       //Decline of Access
        
        public IActionResult getApprove(string UserName)
        {
            SqlConnection con = ConnectToSql();
            con.Open();
            string querry = "Update UserInfo set StatusId = 1,ActiveTime = CURRENT_TIMESTAMP where UserId=@UserName";
            SqlCommand cmd = new SqlCommand(querry, con);
            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("UserAdmin");
        }    //Approval of Access
        

       
        //[HttpPost]
        //public ActionResult UploadFile(HttpPostedFileBase file)
        //{
        //    HttpPostedFileBase obj = new HttpPostedFileBase();
        //    obj = file;
        //    try
        //    {
        //        if (file.ContentLength > 0)
        //        {
        //            string _FileName = Path.GetFileName(file.FileName);
        //            string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
        //            file.SaveAs(_path);
        //        }
        //        ViewBag.Message = "File Uploaded Successfully!!";
        //        return View();
        //    }
        //    catch
        //    {
        //        ViewBag.Message = "File upload failed!!";
        //        return View();
        //    }
        //}

        public IActionResult ExportToCSV()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Name,SAPid,Hours,Leave Hours,Afternoon Shift Days,Night Shift Days ,Total Days,Transport Allowance,Total Allowance");
            foreach (var user in listEmployee2)
            {
                if (user.ApprovalStatus == "Approved")
                    builder.AppendLine($"{user.Name},{user.SAPid},{user.Hours},{user.LeaveHours},{user.AfternoonShiftDays},{user.NightShiftDays},{user.TotalDays},{user.TransportAllowance},{user.TotalAllowance}");
                else if (user.ApprovalStatus == "Awaiting Approval")
                {
                    SqlConnection con = ConnectToSql();
                    con.Open();
                    string querry = "Update export_data set Approval_Status = 'Approved'  where Resource_name=@UserName and Period_Start = @periodStart And Project_Id=@ProjectId and Approval_Status='Awaiting Approval'";
                    SqlCommand cmd = new SqlCommand(querry, con);
                    cmd.Parameters.AddWithValue("@UserName", user.Name);
                    cmd.Parameters.AddWithValue("@PeriodStart", user.PeriodStart);
                    cmd.Parameters.AddWithValue("@ProjectId", user.ProjectId);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    builder.AppendLine($"{user.Name},{user.SAPid},{user.Hours},{user.LeaveHours},{user.AfternoonShiftDays},{user.NightShiftDays},{user.TotalDays},{user.TransportAllowance},{user.TotalAllowance}");
                }

            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");

        }                  //Approval of reports and download csv
        [HttpPost]
        public IActionResult ImportToDb(IFormFile file)
        {

            String Resoursename;
            String ResourseId;
            DateTime PeriodStart;
            DateTime PeriodEnd;
            int Hours;
            String ApprovalStatus;
            String TimesheetNumber;
            String Vertical;
            String Horizontal;
            String SubHorizontal;
            String CoustmerId;
            String CoustmerName;
            String ProjectId;
            String ProjectName;
            String ProjectManager;

            string path = Path.GetFullPath(file.FileName).ToString();
            path = path.Replace(" ", "");
            string csvstoragepath = path; //mySettings.CSV_StoragePath + Convert.ToString(clientid, ic) + "\\" + Convert.ToString(fileid, ic) + ".csv";
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<FileModelDto> cList = new List<FileModelDto>();
            //using (var reader = new StreamReader(csvstoragepath))
            //using (var csv = new CsvReader(reader, config))
            //{
            //    cList = csv.GetRecords<FileModelDto>().ToList();
            //    //cList.ForEach(x => x.ClientId = clientid.ToString());
            //    //cList.ForEach(x => x.FileId = fileid.ToString());
            //}

            //file.CopyTo("~/ExcelFile/" + path);
            //String ExcelFile = Server.MapPath("~/ExcelFile/") + path;
            OleDbConnection mycon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = '" + csvstoragepath+/*ExcelFile +*/ "'; Extended Properties =\"Excel 12.0; HDR=YES;\"");

            mycon.Open();
            OleDbCommand cmd = new OleDbCommand("select * from [Sheet1$]", mycon);
            OleDbDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                // Response.Write("<br/>"+dr[0].ToString());
                Resoursename = dr[0].ToString();
                ResourseId = dr[1].ToString();
                PeriodStart = Convert.ToDateTime(dr[2].ToString());
                PeriodEnd = Convert.ToDateTime(dr[3].ToString());
                Hours = Convert.ToInt32(dr[4]);
                ApprovalStatus = dr[5].ToString();
                TimesheetNumber = dr[6].ToString();
                Vertical = dr[7].ToString();
                Horizontal = dr[8].ToString();
                SubHorizontal = dr[9].ToString();
                CoustmerId = dr[10].ToString();
                CoustmerName = dr[11].ToString();
                ProjectId = dr[12].ToString();
                ProjectName = dr[13].ToString();
                ProjectManager = dr[14].ToString();


                savedata(Resoursename, ResourseId, PeriodStart, PeriodEnd, Hours, ApprovalStatus, TimesheetNumber, Vertical, Horizontal, SubHorizontal, CoustmerId, CoustmerName, ProjectId, ProjectName, ProjectManager);


            }
            //"Data Has Been Saved Successfully";
            mycon.Close();
            return RedirectToAction("Dashboard");


        }
        private void savedata(String Resoursename,
                        String ResourseId,
                        DateTime PeriodStart,
                        DateTime PeriodEnd,
                        int Hours,
                        String ApprovalStatus,
                        String TimesheetNumber,
                        String Vertical,
                        String Horizontal,
                        String SubHorizontal,
                        String CoustmerId,
                        String CoustmerName,
                        String ProjectId,
                        String ProjectName,
                        String ProjectManager)
        {
            String query = "insert into sample_import values(@Resoursename, @ResourseId, @PeriodStart, @PeriodEnd, @Hours, @ApprovalStatus, @TimesheetNumber, @Vertical, @Horizontal, @SubHorizontal,@CustmerId, @CustmerName,@ProjectId, @ProjectName, @ProjectManager)";
            
            SqlConnection con = ConnectToSql();
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@Resoursename", Resoursename);
            cmd.Parameters.AddWithValue("@ResourseId", ResourseId);
            cmd.Parameters.AddWithValue("@PeriodStart", PeriodStart);
            cmd.Parameters.AddWithValue("@PeriodEnd", PeriodEnd);
            cmd.Parameters.AddWithValue("@Hours", Hours);
            cmd.Parameters.AddWithValue("@ApprovalStatus", ApprovalStatus);
            cmd.Parameters.AddWithValue("@TimesheetNumber", TimesheetNumber);
            cmd.Parameters.AddWithValue("@Vertical", Vertical);
            cmd.Parameters.AddWithValue("@Horizontal", Horizontal);
            cmd.Parameters.AddWithValue("@SubHorizontal", SubHorizontal);
            cmd.Parameters.AddWithValue("@CustmerId", CoustmerId);
            cmd.Parameters.AddWithValue("@CustmerName", CoustmerName);
            cmd.Parameters.AddWithValue("@ProjectId", ProjectId);
            cmd.Parameters.AddWithValue("@ProjectName", ProjectName);
            cmd.Parameters.AddWithValue("@ProjectManager", ProjectManager);

            cmd.ExecuteNonQuery();
            con.Close();

        }
    }

}
