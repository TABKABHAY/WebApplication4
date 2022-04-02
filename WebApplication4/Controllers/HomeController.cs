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
using System.Threading.Tasks;

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
                TempData["add"] = "alert show";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string errormsg = "Email is already registered...login instead !";
                TempData["ErrorMessage"] = errormsg;
                return RedirectToAction("Service_Provider_Become_a_Pro", "Home");
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
                    Microsoft.EntityFrameworkCore.DbSet<User> users = db.Users;
                    var U = users.FirstOrDefault(x => x.Email == user.Username);

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
                    else if(U.UserTypeId == 1)
                    {
                        return RedirectToAction("ServiceProviderDashboard", "Home");
                    }
                    else if (U.UserTypeId == 2)
                      {
                          return RedirectToAction("AdminPanel", "Home");
                      }

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



                //var ServiceTable = db.ServiceRequests.Where(x => (x.UserId == user.UserId) && (x.Status == 1 || x.Status == 2)).ToList();

                var ServiceTable = db.ServiceRequests.Where(x => x.UserId == user.UserId).ToList();

                //var ServiceTable = db.ServiceRequests.Where(x=>x.UserId==user.UserId ).ToList();
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
                    "<a href='" + Url.Action("ResetPassword", "Home", new { userId = user.UserId }, "https") + "'>Reset Password</a>";


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
        [HttpPost]
        public IActionResult ResetPassword(ResetPassword rp)
        {
            var user = new User() { UserId = rp.userId, Password = rp.password };
            db.Users.Attach(user);
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.Entry(user).Property(x => x.Password).IsModified = true;
            db.SaveChanges();


            return RedirectToAction("Index", "Home", new { loginModal = "true" });
        }

        public IActionResult logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("userId");
            return RedirectToAction("Index", "Home", new { logoutModal = "true" });
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



        public IActionResult ServiceProviderDashboard()
        {

            int? Id = HttpContext.Session.GetInt32("userId");
            if (Id == null)
            {
                Id = Convert.ToInt32(Request.Cookies["userId"]);
            }

            if (Id == null)
            {
                return RedirectToAction("Index", "Home", new { loginFail = "true" });
            }
            User user = db.Users.FirstOrDefault(x => x.UserId == Id);
            int userTypeId = user.UserTypeId;
            if (userTypeId != 1)
            {
                return RedirectToAction("Index", "Home");

            }


            ViewBag.Name = user.FirstName;
            ViewBag.UserType = user.UserTypeId;


            List<ServiceProviderDashboard> dashboardTable = new List<ServiceProviderDashboard>();


            var ServiceRequest = db.ServiceRequests.Where(x => x.ZipCode == user.ZipCode && x.Status == 1).ToList();

            if (ServiceRequest.Any())
            {
                foreach (var req in ServiceRequest)
                {
                    ServiceProviderDashboard temp = new ServiceProviderDashboard();


                    temp.ServiceRequestId = req.ServiceRequestId;
                    var StartDate = req.ServiceStartDate.ToString();
                    //temp.Date = StartDate.Substring(0, 10);
                    //temp.StartTime = StartDate.Substring(11);
                    temp.Date = req.ServiceStartDate.ToString("dd/MM/yyyy");
                    temp.StartTime = req.ServiceStartDate.AddHours(0).ToString("HH:mm ");
                    var totaltime = (double)(req.ServiceHours + req.ExtraHours);
                    temp.EndTime = req.ServiceStartDate.AddHours(totaltime).ToString("HH:mm ");
                    temp.Status = (int)req.Status;
                    temp.TotalCost = req.TotalCost;
                    temp.HasPet = req.HasPets;
                    temp.Comments = req.Comments;


                    User customer = db.Users.FirstOrDefault(x => x.UserId == req.UserId);
                    temp.CustomerName = customer.FirstName + " " + customer.LastName;

                    ServiceRequestAddress Address = (ServiceRequestAddress)db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == req.ServiceRequestId);

                    temp.Address = Address.AddressLine1 + ", " + Address.AddressLine2 + ", " + Address.City + " - " + Address.PostalCode;

                    List<ServiceRequestExtra> SRExtra = db.ServiceRequestExtras.Where(x => x.ServiceRequestId == req.ServiceRequestId).ToList();

                    foreach (ServiceRequestExtra row in SRExtra)
                    {
                        if (row.ServiceExtraId == 1)
                        {
                            temp.Cabinet = true;
                        }
                        else if (row.ServiceExtraId == 2)
                        {
                            temp.Oven = true;
                        }
                        else if (row.ServiceExtraId == 3)
                        {
                            temp.Window = true;
                        }
                        else if (row.ServiceExtraId == 4)
                        {
                            temp.Fridge = true;
                        }
                        else
                        {
                            temp.Laundry = true;
                        }
                    }



                    dashboardTable.Add(temp);






                }


            }

            return View(dashboardTable);






        }




        public JsonResult GetAllDetails(ServiceProviderDashboard ID)
        {

            ServiceProviderDashboard Details = new ServiceProviderDashboard();

            ServiceRequest sr = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == ID.ServiceRequestId);

            Details.ServiceRequestId = ID.ServiceRequestId;

            Details.Date = sr.ServiceStartDate.ToString("dd/MM/yyyy");

            Details.StartTime = sr.ServiceStartDate.ToString("HH:mm");

            Details.Duration = (decimal)(sr.ServiceHours + sr.ExtraHours);
            Details.EndTime = sr.ServiceStartDate.AddHours((double)sr.SubTotal).ToString("HH:mm");
            Details.TotalCost = sr.TotalCost;
            Details.Comments = sr.Comments;
            Details.Status = (int)sr.Status;


            List<ServiceRequestExtra> SRExtra = db.ServiceRequestExtras.Where(x => x.ServiceRequestId == ID.ServiceRequestId).ToList();

            foreach (ServiceRequestExtra row in SRExtra)
            {
                if (row.ServiceExtraId == 1)
                {
                    Details.Cabinet = true;
                }
                else if (row.ServiceExtraId == 2)
                {
                    Details.Oven = true;
                }
                else if (row.ServiceExtraId == 3)
                {
                    Details.Window = true;
                }
                else if (row.ServiceExtraId == 4)
                {
                    Details.Fridge = true;
                }
                else
                {
                    Details.Laundry = true;
                }
            }

            ServiceRequestAddress Address = db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == ID.ServiceRequestId);

            Details.Address = Address.AddressLine1 + ", " + Address.AddressLine2 + ", " + Address.City + " - " + Address.PostalCode;
            Details.ZipCode = Address.PostalCode;

            User customer = db.Users.FirstOrDefault(x => x.UserId == sr.UserId);

            Details.CustomerName = customer.FirstName + " " + customer.LastName;


            return new JsonResult(Details);
        }

        /*--------- Accept Service Req------------*/
        [HttpGet]
        public string acceptService(ServiceProviderDashboard ID)
        {
            int? spId = HttpContext.Session.GetInt32("userId");
            if (spId == null)
            {
                spId = Convert.ToInt32(Request.Cookies["userId"]);
            }

            ServiceRequest serviceRequest = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == ID.ServiceRequestId);
            if (serviceRequest != null && serviceRequest.Status != 1)
            {
                return new string("Service Req Not available");
            }

            int conflict = CheckConflict((int)serviceRequest.ServiceRequestId);

            if (conflict != -1)
            {
                return conflict.ToString();
            }



            serviceRequest.Status = 2;
            serviceRequest.ServiceProviderId = spId;
            var result = db.ServiceRequests.Update(serviceRequest);
            db.SaveChanges();
            if (result != null)
            {
                return "Suceess";
            }
            else
            {
                return "error";
            }

        }


        public string ConflictDetails(ServiceProviderDashboard ID)
        {
            Console.WriteLine(ID.ServiceRequestId);

            int conflict = CheckConflict(ID.ServiceRequestId);

            ServiceRequest sr = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == conflict);


            string conflictmsg = "This Request time is conflicting with Service ID: " + sr.ServiceRequestId + " on :" + sr.ServiceStartDate;

            return conflictmsg;

        }

        public int CheckConflict(int SRID)
        {

            int? Id = HttpContext.Session.GetInt32("userId");
            if (Id == null)
            {
                Id = Convert.ToInt32(Request.Cookies["userId"]);
            }


            ServiceRequest request = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == SRID);

            String reqdate = request.ServiceStartDate.ToString("yyyy-MM-dd");
            Console.WriteLine(reqdate);

            String startDateStr = reqdate + " 00:00:00.000";
            String endDateStr = reqdate + " 23:59:59.999";

            Console.WriteLine(startDateStr);

            DateTime startDate = DateTime.ParseExact(startDateStr, "yyyy-MM-dd HH:mm:ss.fff",
                                       System.Globalization.CultureInfo.InvariantCulture);

            DateTime endDate = DateTime.ParseExact(endDateStr, "yyyy-MM-dd HH:mm:ss.fff",
                                       System.Globalization.CultureInfo.InvariantCulture);

            List<ServiceRequest> list = db.ServiceRequests.Where(x => (x.ServiceProviderId == Id) && (x.Status == 2) && (x.ServiceStartDate > startDate && x.ServiceStartDate < endDate)).ToList();

            double mins = ((double)(request.ServiceHours + request.ExtraHours)) * 60;
            DateTime endTimeRequest = request.ServiceStartDate.AddMinutes(mins + 60);

            request.ServiceStartDate = request.ServiceStartDate.AddMinutes(-60);
            Console.WriteLine(endTimeRequest);
            Console.WriteLine(request.ServiceStartDate);
            foreach (ServiceRequest booked in list)
            {
                mins = ((double)(booked.ServiceHours + booked.ExtraHours)) * 60;
                DateTime endTimeBooked = booked.ServiceStartDate.AddMinutes(mins);

                if (request.ServiceStartDate < booked.ServiceStartDate)
                {
                    if (endTimeRequest <= booked.ServiceStartDate)
                    {
                        return -1;
                    }
                    else
                    {
                        return booked.ServiceRequestId;
                    }
                }
                else
                {
                    if (request.ServiceStartDate < endTimeBooked)
                    {
                        return booked.ServiceRequestId;
                    }
                }

            }



            return -1;

        }


        public JsonResult GetUpcomingService()
        {
            int? Id = HttpContext.Session.GetInt32("userId");
            if (Id == null)
            {
                Id = Convert.ToInt32(Request.Cookies["userId"]);
            }

            User user = db.Users.FirstOrDefault(x => x.UserId == Id);

            List<ServiceProviderDashboard> UpcomingTable = new List<ServiceProviderDashboard>();

            var ServiceRequest = db.ServiceRequests.Where(x => x.Status == 2 && x.ServiceProviderId == user.UserId).ToList();

            if (ServiceRequest.Any())
            {
                foreach (var req in ServiceRequest)
                {
                    ServiceProviderDashboard temp = new ServiceProviderDashboard();


                    temp.ServiceRequestId = req.ServiceRequestId;
                    var StartDate = req.ServiceStartDate.ToString();
                    temp.Date = req.ServiceStartDate.ToString("dd/MM/yyyy");
                    temp.StartTime = req.ServiceStartDate.AddHours(0).ToString("HH:mm ");
                    var totaltime = (double)(req.ServiceHours + req.ExtraHours);
                    temp.EndTime = req.ServiceStartDate.AddHours(totaltime).ToString("HH:mm ");

                    temp.Status = (int)req.Status;

                    temp.TotalCost = req.TotalCost;

                    // temp.HasPet = req.HasPets;
                    //temp.Comments = req.Comments;


                    User customer = db.Users.FirstOrDefault(x => x.UserId == req.UserId);
                    temp.CustomerName = customer.FirstName + " " + customer.LastName;

                    ServiceRequestAddress Address = (ServiceRequestAddress)db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == req.ServiceRequestId);

                    temp.Address = Address.AddressLine1 + ", " + Address.AddressLine2 + ", " + Address.City + " , " + Address.PostalCode;



                    UpcomingTable.Add(temp);






                }


            }


            return new JsonResult(UpcomingTable);

        }





        public string cancelRequest(ServiceRequest request)
        {
            Console.WriteLine(request.ServiceRequestId);

            ServiceRequest requestObj = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == request.ServiceRequestId);

            requestObj.ServiceProviderId = null;
            requestObj.Status = 1;

            var result = db.ServiceRequests.Update(requestObj);
            db.SaveChanges();
            if (result != null)
            {
                return "Suceess";
            }
            else
            {
                return "error";
            }




        }






        public IActionResult AdminPanel()
        {
            int? Id = HttpContext.Session.GetInt32("userId");
            if (Id == null)
            {
                Id = Convert.ToInt32(Request.Cookies["userId"]);
            }

            if (Id == null)
            {
                return RedirectToAction("Index", "Home", new { loginFail = "true" });
            }
            User user = db.Users.FirstOrDefault(x => x.UserId == Id);
            int userTypeId = user.UserTypeId;
            if (userTypeId != 2)
            {
                return RedirectToAction("Index", "Home");

            }

            ViewBag.Name = user.FirstName;
            ViewBag.UserType = user.UserTypeId;

            return View();

        }


        public JsonResult GetServiceRequest(AdminServiceDetls filter)
        {


            List<AdminServiceRequest> tabledata = new List<AdminServiceRequest>();

            var serviceRequestsList = db.ServiceRequests.ToList().OrderByDescending(x => x.ServiceRequestId);

            foreach (ServiceRequest temp in serviceRequestsList)
            {


                if (CheckServiceRequest(temp, filter))
                {


                    AdminServiceRequest Dto = new AdminServiceRequest();

                    Dto.ServiceRequestId = temp.ServiceRequestId;
                    Dto.Date = temp.ServiceStartDate.ToString("dd/MM/yyyy");
                    Dto.StartTime = temp.ServiceStartDate.AddHours(0).ToString("HH':'mm ");
                    var totaltime = (double)(temp.ServiceHours + temp.ExtraHours);
                    Dto.EndTime = temp.ServiceStartDate.AddHours(totaltime).ToString("HH':'mm ");
                    Dto.Status = (int)temp.Status;
                    Dto.TotalCost = temp.TotalCost;
                    /* customer */

                    User customer = db.Users.FirstOrDefault(x => x.UserId == temp.UserId);

                    Dto.CustomerName = customer.FirstName + " " + customer.LastName;



                    /*address */

                    ServiceRequestAddress serviceRequestAddress = db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == temp.ServiceRequestId);

                    Dto.Address = serviceRequestAddress.AddressLine1 + " " + serviceRequestAddress.AddressLine2 + "," + serviceRequestAddress.City + "-" + serviceRequestAddress.PostalCode;

                    Dto.ZipCode = temp.ZipCode;


                    if (temp.ServiceProviderId != null)
                    {
                        User sp = db.Users.FirstOrDefault(x => x.UserId == temp.ServiceProviderId);

                        Dto.ServiceProvider = sp.FirstName + " " + sp.LastName;
                        Dto.UserProfilePicture = sp.UserProfilePicture;


                        decimal rating;

                        if (db.Ratings.Where(x => x.RatingTo == temp.ServiceProviderId).Count() > 0)
                        {
                            rating = db.Ratings.Where(x => x.RatingTo == temp.ServiceProviderId).Average(x => x.Ratings);
                        }
                        else
                        {
                            rating = 0;
                        }
                        Dto.AverageRating = (float)decimal.Round(rating, 1, MidpointRounding.AwayFromZero);

                    }


                    tabledata.Add(Dto);
                }

            }

            return Json(tabledata);
        }

        Boolean CheckServiceRequest(ServiceRequest req, AdminServiceDetls filter)
        {
            var user = db.Users.FirstOrDefault(x => x.UserId == req.UserId);


            if (filter.ServiceRequestId != null)
            {
                if (req.ServiceRequestId != filter.ServiceRequestId)
                {
                    return false;
                }
            }
            if (filter.ZipCode != null)
            {
                if (req.ZipCode != filter.ZipCode)
                {
                    return false;
                }
            }
            if (filter.Email != null)
            {
                var email = user.Email;
                if (!email.Contains(filter.Email))
                {
                    return false;
                }
            }
            if (filter.CustomerName != null)
            {

                var name = user.FirstName + " " + user.LastName;
                if (!name.Contains(filter.CustomerName))
                {
                    return false;
                }
            }
            if (filter.ServiceProviderName != null)
            {
                User sp = db.Users.FirstOrDefault(x => x.UserId == req.ServiceProviderId);
                if (sp != null)
                {
                    var name = sp.FirstName + " " + sp.LastName;

                    if (!name.Contains(filter.ServiceProviderName))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            if (filter.Status != null)
            {
                if (req.Status != filter.Status)
                {
                    return false;
                }
            }
            if (filter.FromDate != null)
            {
                DateTime dateTime = Convert.ToDateTime(filter.FromDate);
                if (!(req.ServiceStartDate >= dateTime))
                {
                    return false;
                }

            }
            if (filter.ToDate != null)
            {
                var reqEndDate = req.ServiceStartDate.AddHours((double)(req.ServiceHours + req.ExtraHours));

                DateTime dateTime = Convert.ToDateTime(filter.ToDate);

                if (!(reqEndDate <= dateTime))
                {
                    return false;
                }
            }


            return true;



        }




        public JsonResult GetEditPopupData(ServiceRequest Id)
        {



            AdminUpdate AdminUpdate = new AdminUpdate();

            AdminUpdate.Address = db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == Id.ServiceRequestId);

            DateTime starttime = db.ServiceRequests.Where(x => x.ServiceRequestId == Id.ServiceRequestId).Select(x => x.ServiceStartDate).FirstOrDefault();

            AdminUpdate.Date = starttime.ToString("MM-dd-yyyy");

            //AdminUpdate.StartTime = starttime.ToString("HH:mm:ss");

            AdminUpdate.StartTime = starttime.ToString("HH':'mm':'ss");




            return Json(AdminUpdate);



        }



        public JsonResult UpdateServiceReq(AdminUpdate DTO)
        {
            ServiceRequest serviceRequest = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == DTO.ServiceRequestId);

            DateTime dateTime = Convert.ToDateTime(DTO.Date);

            serviceRequest.ServiceStartDate = dateTime;






            ServiceRequestAddress serviceRequestAddress = db.ServiceRequestAddresses.FirstOrDefault(x => x.ServiceRequestId == DTO.ServiceRequestId);



            serviceRequestAddress.AddressLine1 = DTO.Address.AddressLine1;
            serviceRequestAddress.AddressLine2 = DTO.Address.AddressLine2;

            serviceRequestAddress.PostalCode = DTO.Address.PostalCode;
            serviceRequestAddress.City = DTO.Address.City;
            serviceRequestAddress.State = DTO.Address.State;

            var result2 = db.ServiceRequestAddresses.Update(serviceRequestAddress);
            db.SaveChanges();
            var result1 = db.ServiceRequests.Update(serviceRequest);
            db.SaveChanges();

            if (result1 != null && result2 != null)
            {

                _ = SendMail(serviceRequest);
                return Json("true");
            }
            else
            {
                return Json("false");
            }

        }



        [HttpPost]
        public async Task<IActionResult> CencleServiceReq(ServiceRequest cancel)
        {




            ServiceRequest cancelService = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == cancel.ServiceRequestId);
            cancelService.Status = 4;


            var result = db.ServiceRequests.Update(cancelService);
            db.SaveChanges();
            if (result != null)
            {

                await Task.Run(() =>
                {

                    if (cancelService.ServiceProviderId != null)
                    {

                        User temp = db.Users.FirstOrDefault(x => x.UserId == cancelService.ServiceProviderId);


                        MimeMessage message = new MimeMessage();

                        MailboxAddress from = new MailboxAddress("Helperland",
                        "ajexmex@gmail.com");
                        message.From.Add(from);

                        MailboxAddress to = new MailboxAddress(temp.FirstName, temp.Email);
                        message.To.Add(to);

                        message.Subject = "Service Request cancelled ";

                        BodyBuilder bodyBuilder = new BodyBuilder();
                        bodyBuilder.HtmlBody = "<h1>Service request with Id=" + cancelService.ServiceRequestId + ", has been cancled </ h1 > ";



                        message.Body = bodyBuilder.ToMessageBody();

                        SmtpClient client = new SmtpClient();
                        client.Connect("smtp.gmail.com", 587, false);
                    mailto: client.Authenticate("ajexmex@gmail.com", "Je#M6exex");
                        client.Send(message);
                        client.Disconnect(true);
                        client.Dispose();

                    }




                });




                return Ok(Json("true"));
            }

            return Ok(Json("false"));
        }




        public JsonResult GetUserData(AdminUserPart filterDto)
        {

            var user = db.Users.ToList();

            List<User> result = new List<User>();

            foreach (User temp in user)
            {
                if (CheckUserFilter(temp, filterDto))
                {

                    result.Add(temp);
                }
            }
            return Json(result);





        }

        public bool CheckUserFilter(User user, AdminUserPart filter)
        {

            //Console.WriteLine(filter.ToDate);
            //Console.WriteLine(user.CreatedDate);

            if (filter.Name != null)
            {

                var name = user.FirstName + " " + user.LastName;
                if (!name.Contains(filter.Name))
                {
                    return false;
                }
            }

            if (filter.UserType != null)
            {
                if (user.UserTypeId != filter.UserType)
                {
                    return false;
                }
            }

            if (filter.Phone != null)
            {
                var phone = user.Mobile;
                if (!phone.Contains(filter.Phone))
                {
                    return false;
                }
            }

            if (filter.PostalCode != null)
            {
                if (user.ZipCode != filter.PostalCode)
                {
                    return false;
                }
            }


            if (filter.Email != null)
            {
                var email = user.Email;
                if (!email.Contains(filter.Email))
                {
                    return false;
                }
            }
            return true;

        }



        public string UserEdit(User Id)
        {
            Console.WriteLine(Id.UserId);
            User user = db.Users.FirstOrDefault(x => x.UserId == Id.UserId);

            var resultString = "Error";

            if (user.IsApproved == false)
            {
                user.IsApproved = true;
                user.IsActive = true;

                resultString = "Service Provider Approved and Activated";
            }
            else if (user.IsActive == false)
            {
                user.IsActive = true;

                resultString = "User Activated";
            }
            else
            {
                user.IsActive = false;

                resultString = "User Deactivated";
            }

            var result = db.Users.Update(user);
            db.SaveChanges();

            if (result != null)
            {
                return resultString;
            }

            return "Error occured in DataBase, try again";


        }


        public async Task SendMail(ServiceRequest req)
        {






            List<User> users = new List<User>();

            users.Add(db.Users.FirstOrDefault(x => x.UserId == req.UserId));

            if (req.Status != 1)
            {
                users.Add(db.Users.FirstOrDefault(x => x.UserId == req.ServiceProviderId));
            }






            await Task.Run(() =>
            {
                foreach (var temp in users)
                {

                    MimeMessage message = new MimeMessage();

                    MailboxAddress from = new MailboxAddress("Helperland","ajexmex@gmail.com");
                    message.From.Add(from);

                    MailboxAddress to = new MailboxAddress(temp.FirstName, temp.Email);
                    message.To.Add(to);

                    message.Subject = "Service Request Updated";

                    BodyBuilder bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = "<h1>A service with ID number " + req.ServiceRequestId + " has been updated</h1><br>" + "<h2>With time : " + req.ServiceStartDate + "</h2>";



                    message.Body = bodyBuilder.ToMessageBody();

                    SmtpClient client = new SmtpClient();
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("ajexmex@gmail.com", "Je#M6exex");
                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();

                }


            });



        }

        public JsonResult GetAdminRefundData(ServiceRequest Id)
        {


            Console.WriteLine(Id.ServiceRequestId);
            var req = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == Id.ServiceRequestId);


            var myData = new
            {
                TotalCost = req.TotalCost,
                RefundAmount = req.RefundedAmount

            };

            return Json(myData);
        }

        public string AdminRefundUpdate(ServiceRequest req)
        {
            Console.WriteLine(req.RefundedAmount);
            Console.WriteLine(req.ServiceRequestId);


            ServiceRequest obj = db.ServiceRequests.FirstOrDefault(x => x.ServiceRequestId == req.ServiceRequestId);


            obj.RefundedAmount = req.RefundedAmount;

            var result = db.ServiceRequests.Update(obj);

            db.SaveChanges();

            if (result != null)
            {

                return "true";
            }

            return "error";
        }







    }
}