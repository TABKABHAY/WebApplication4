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

        public IActionResult CreateForlogin(Login uc)
        {
            var verify = _auc.Logins.FirstOrDefault(x => x.Username.Equals(uc.Username) && x.Password.Equals(uc.Password));
            if (verify != null)
            {
                uc.Id = 0;
                HttpContext.Session.SetString("Username", uc.Username);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Message = "UserName or password is wrong";
                return RedirectToAction("UserRegisteration", "Home");
            }
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
        public IActionResult Creat_for_ForgotPassword(string email)
        {

            if (email != null)
            {
                var user = db.Users.FirstOrDefault(x => x.Email == email);


                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("WebApplication4",
                "ajexmex@gmail.com");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress(user.FirstName, email);
                message.To.Add(to);

                message.Subject = "Reset Password";

                Random r = new Random();
                int num = r.Next(100000, 999999);


                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = "<h1>Enter given code and reset password</h1>" + num;
                message.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("ajexmex@gmail.com", "Je#M6exex");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                return RedirectToAction("Index", "Home", new { mailSended = "true" });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}