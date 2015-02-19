using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NewMusicSunshine.Core;

namespace NewMusicSunshine.Website.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.User user)
        {
            if (ModelState.IsValid)
            {
                if (user.IsValid(user.UserName, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login data is incorrect!");
                }
            }
            return View(user);
        }
        
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Register(Models.NewUser user)
        {
            if (ModelState.IsValid)
            {
                if (user.IsDuplicate(user.UserName))
                {
                    ModelState.AddModelError("", "Username aleady exists.");
                }
                else
                {
                    var result = CreateUser(user);
                    if (result < 1)
                    {
                        ModelState.AddModelError("", "There was a problem creating your username.");
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return View(user);
        }

        private int CreateUser(Models.User user)
        {
            var cs = ConfigurationManager.ConnectionStrings["SunshineDbConnection"].ConnectionString;

            using (var cn = new SqlConnection(cs))
            {
                string _sql = @"INSERT INTO [dbo].[System_Users] ([Username], [Password], [Email]) VALUES (@u, @p, @e)";
                var cmd = new SqlCommand(_sql, cn);
                cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar)).Value = user.UserName;
                cmd.Parameters.Add(new SqlParameter("@p", SqlDbType.NVarChar)).Value = SHA1.Encode(user.Password);
                cmd.Parameters.Add(new SqlParameter("@e", SqlDbType.NVarChar)).Value = user.EmailAddress;
                cn.Open();
                var result = cmd.ExecuteNonQuery();
                return result;
            }
        }
    }
}
