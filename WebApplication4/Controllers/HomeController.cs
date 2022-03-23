using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using WebApplication4.Models;
using Microsoft.AspNetCore.Http;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using WebApplication4.data;
using WebApplication4.ViewModel;
using System.Collections.Generic;

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
                if (db.Users.Where(x => x.Email == user.Username && x.Password == user.Password).Count() > 0)
                {
                    var U = db.Users.FirstOrDefault(x => x.Email == user.Username );

                    Console.WriteLine("1");

                    if (user.Remember == true)
                    {
                        CookieOptions cookieRemember = new CookieOptions();
                        cookieRemember.Expires = DateTime.Now.AddSeconds(604800);
                        Response.Cookies.Append("userId", Convert.ToString(U.UserId), cookieRemember);
                    }
                    HttpContext.Session.SetInt32("userId", U.UserId);

                    if (U.UserTypeId == 0)
                    {
                        return RedirectToAction("CustomerDashboard", "Home");
                    }
                    /* else if (user.UserTypeId == 2)
                      {
                          return RedirectToAction("SPUpcomingService", "ServicePro");
                      }
                      else if (user.UserTypeId == 3)
                      {
                          return RedirectToAction("ServiceRequest", "Admin");
                      }*/

                    return RedirectToAction("CustomerDashboard", "Home");
                }
                else
                {
                    TempData["add"] = "alert show";
                    TempData["fail"] = "username and password are invalid";
                    return RedirectToAction("Index", "Home", new { loginFail = "true" });

                }
            }

            TempData["add"] = "alert show";
            TempData["fail"] = "username and password are required";
            return RedirectToAction("Index", "Home", new { loginModal = "true" });

        }

        public IActionResult CustomerDashboard()
        {

            var userTypeId = -1;
            User user = null;

            if (HttpContext.Session.GetInt32("userId") != null)
            {

                user = db.Users.Find(HttpContext.Session.GetInt32("userId"));
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;

                userTypeId = user.UserTypeId;



            }
            else if (Request.Cookies["userId"] != null)
            {
                user = db.Users.FirstOrDefault(x => x.UserId == Convert.ToInt32(Request.Cookies["userId"]));
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;
                userTypeId = user.UserTypeId;
            }
            if (userTypeId == 0)
            {
                List<CustomerDashboard> dashboard = new List<CustomerDashboard>();



                //var ServiceTable = _db.ServiceRequests.Where(x => (x.UserId == user.UserId) && (x.Status == 1 || x.Status == 2)).ToList();

                var ServiceTable = db.ServiceRequests.Where(x => x.UserId == user.UserId).ToList();

                //var ServiceTable = _db.ServiceRequests.Where(x=>x.UserId==user.UserId ).ToList();
                if (ServiceTable.Any())  /*ServiceTable.Count()>0*/
                {
                    foreach (var service in ServiceTable)
                    {

                        CustomerDashboard dash = new CustomerDashboard();
                        dash.ServiceRequestId = service.ServiceRequestId;
                        var StartDate = service.ServiceStartDate.ToString();
                        //dash.Date = StartDate.Substring(0, 10);
                        //dash.StartTime = StartDate.Substring(11);
                        dash.Date = service.ServiceStartDate.ToString("dd/MM/yyyy");
                        dash.StartTime = service.ServiceStartDate.AddHours(0).ToString("HH:mm ");
                        var totaltime = (double)(service.ServiceHours + service.ExtraHours);
                        dash.EndTime = service.ServiceStartDate.AddHours(totaltime).ToString("HH:mm ");
                        dash.Status = (int)service.Status;
                        dash.TotalCost = service.TotalCost;

                        if (service.ServiceProviderId != null)
                        {

                            User sp = db.Users.Where(x => x.UserId == service.ServiceProviderId).FirstOrDefault();

                            dash.ServiceProvider = sp.FirstName + " " + sp.LastName;
                            dash.UserProfilePicture = "/image/" + sp.UserProfilePicture;
                            decimal rating;

                            if (db.Ratings.Where(x => x.RatingTo == service.ServiceProviderId).Count() > 0)
                            {
                                rating = db.Ratings.Where(x => x.RatingTo == service.ServiceProviderId).Average(x => x.Ratings);
                            }
                            else
                            {
                                rating = 0;
                            }
                            dash.AverageRating = (float)decimal.Round(rating, 1, MidpointRounding.AwayFromZero);


                        }

                        dashboard.Add(dash);

                    }
                }

                return PartialView(dashboard);
            }


            return RedirectToAction("Index", "Home", new { loginFail = "true" });


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
            return RedirectToAction("Index", "HomeController");
        }
        [HttpPost]
        public IActionResult ResetPassword(ResetPassword rp)
        {
            var user = new User() { UserId = rp.userId, Password = rp.password };
            db.Users.Attach(user);
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.SaveChanges();


            return RedirectToAction("Index", "HomeController", new { loginModal = "true" });
        }

        public IActionResult logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("userId");
            return RedirectToAction("Index", "HomeController", new { logoutModal = "true" });
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
                return RedirectToAction("Index", "HomeController");
            }
            else
            {
                _auc.Add(zc);
                _auc.SaveChanges();
                ViewBag.Message = "UserName or password is wrong";
                return RedirectToAction("About", "HomeController");
            }
        }

    }
}