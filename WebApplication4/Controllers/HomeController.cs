 using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication4.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using System.Reflection.Metadata;
using Syncfusion.EJ2.Navigations;
using Syncfusion.EJ2.Grids;
using Org.BouncyCastle.Crypto.Generators;
using WebApplication4.data;

namespace WebApplication4.Controllers
{

    public class HomeController : Controller
    {
        private readonly masterContext _auc;
        masterContext db = new masterContext();
        public HomeController(masterContext auc)
        {
            _auc = auc;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CreateSession()
        {
            this.HttpContext.Session.SetString("sessionkey", "sessionvalue");
            return RedirectToAction("GetSession");
        }
        public IActionResult UserRegisteration()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Price()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Faq()
        {
            return View();
        }
        public IActionResult BookingService()
        {
            return View();
        }
        public IActionResult Service_Provider_Become_a_Pro()
        {
            return View();
        }
        public IActionResult Service_provider()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateForContactus(ContactUs ct)
        {
            _auc.Add(ct);
            _auc.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateForProvider(User uc)
        {
            var verify = _auc.Users.FirstOrDefault(x => x.Email.Equals(uc.Email));
            if (verify == null)
            {
                uc.UserTypeId = 1;
                _auc.Add(uc);
                _auc.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string errormsg = "Email is already registered...login instead !";
                TempData["ErrorMessage"] = errormsg;
                return RedirectToAction("BecomeHelper", "Home");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Login2(Login2 user)
        {
            if (ModelState.IsValid)
            {
                string password = db.Users.FirstOrDefault(x => x.Email == user.username).Password;

                if (db.Users.Where(x => x.Email == user.username).Count() > 0)
                {

                    var U = db.Users.FirstOrDefault(x => x.Email == user.username);

                    Console.WriteLine("1");

                    if (user.remember == true)
                    {
                        CookieOptions cookieRemember = new CookieOptions();
                        cookieRemember.Expires = DateTime.Now.AddSeconds(604800);
                        Response.Cookies.Append("userId", Convert.ToString(U.UserId), cookieRemember);
                    }
                    HttpContext.Session.SetInt32("userId", U.UserId);

                    if (U.UserTypeId == 0)
                    {
                        return RedirectToAction("CustomerDashboard", "Customer");
                    }
                    /* else if (user.UserTypeId == 2)
                      {
                          return RedirectToAction("SPUpcomingService", "ServicePro");
                      }
                      else if (user.UserTypeId == 3)
                      {
                          return RedirectToAction("ServiceRequest", "Admin");
                      }*/

                    return RedirectToAction("CustomerDashboard", "Customer");
                }
                else
                {
                    TempData["add"] = "alert show";
                    TempData["fail"] = "username and password are invalid";
                    return RedirectToAction("Index", "Public", new { loginFail = "true" });

                }
            }

            TempData["add"] = "alert show";
            TempData["fail"] = "username and password are required";
            return RedirectToAction("Index", "Public", new { loginModal = "true" });

        }




        [HttpPost]

        public IActionResult SendMail(string email)
        {

            if (email != null)
            {
                var user = db.Users.FirstOrDefault(x => x.Email == email);


                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("Helperland",
                "ajexmex@gmail.com");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress(user.FirstName, email);
                message.To.Add(to);

                message.Subject = "Reset Password";

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = "<h1>Reset your password by click below link</h1>" +
                    "<a href='" + Url.Action("ResetPassword", "UserManagement", new { userId = user.UserId }, "https") + "'>Reset Password</a>";


                message.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("ajexmex@gmail.com", "Je#M6exex");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                return RedirectToAction("Index", "Public", new { mailSended = "true" });
            }
            return RedirectToAction("Index", "Public");
        }
        [HttpPost]
        public IActionResult ResetPassword(ResetPassword rp)
        {
            var user = new User() { UserId = rp.userId, Password = rp.password };
            db.Users.Attach(user);
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.SaveChanges();


            return RedirectToAction("Index", "Public", new { loginModal = "true" });
        }

        public IActionResult logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("userId");
            return RedirectToAction("Index", "Public", new { logoutModal = "true" });
        }

    [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateForZipcode(User zc)
        {
            var verify = _auc.Zipcodes.FirstOrDefault(x => x.ZipcodeValue.Equals(zc.ZipCode));
            if (verify != null)
            {
                zc.UserId = 0;
                HttpContext.Session.SetString("zipcode", zc.ZipCode);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _auc.Add(zc);
                _auc.SaveChanges();
                ViewBag.Message = "UserName or password is wrong";
                return RedirectToAction("About", "Home");
            }
        }

    }
}