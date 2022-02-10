using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Service_Provider_Become_a_Pro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Visitor(ContactU o)
        {
            return View("ContactUs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult CreateForlogin(Login uc)
        {

            var verify = _auc.Logins.FirstOrDefault(x => x.Username.Equals(uc.Username));
            if (verify == null)
            {
               _auc.Add(uc);
                _auc.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string errormsg = "USER is already registered...login instead !";
                TempData["ErrorMessage"] = errormsg;
                return RedirectToAction("BecomeHelper", "Home");
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
    }
}