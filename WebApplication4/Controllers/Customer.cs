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
using WebApplication4.ViewModel;

namespace WebApplication4.Controllers
{

    public class CustomerController : Controller
    {
        private readonly masterContext _auc;
        masterContext db = new masterContext();
        public CustomerController(masterContext auc)
        {
            _auc = auc;
        }
        public IActionResult CustomerServiceHistory
    {
        get
        {
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                var id = HttpContext.Session.GetInt32("userId");
                User user = db.Users.Find(id);
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;
                if (user.UserTypeId == 0)
                {
                    return PartialView();
                }
            }
            else if (Request.Cookies["userId"] != null)
            {
                var user = db.Users.FirstOrDefault(x => x.UserId == Convert.ToInt32(Request.Cookies["userId"]));
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;
                if (user.UserTypeId == 0)
                {
                    return PartialView();
                }
            }
            return RedirectToAction("Index", "Public");


        }
    }

    public IActionResult BookService()
        {
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                var id = HttpContext.Session.GetInt32("userId");
                User user = db.Users.Find(id);
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;
                if (user.UserTypeId == 0)
                {
                    return PartialView();
                }
            }
            else if (Request.Cookies["userId"] != null)
            {
                var user = db.Users.FirstOrDefault(x => x.UserId == Convert.ToInt32(Request.Cookies["userId"]));
                ViewBag.Name = user.FirstName;
                ViewBag.UserType = user.UserTypeId;
                if (user.UserTypeId == 0)
                {
                    return PartialView();
                }
            }
            TempData["add"] = "alert show";
            TempData["fail"] = "Please Login to book service";
            return RedirectToAction("Index", "Public", new { loginFail = "true" });
        }

        [HttpPost]
        public IActionResult  ValidPostalCode(PostalCode obj)
        {
            if (ModelState.IsValid)
            {
                var list = db.Users.Where(x => (x.ZipCode == obj.postalcode) && (x.UserId == 32)).ToList();

                if (list.Count() > 0)
                {
                    return Ok(Json("true"));
                }
               // TempData["wrongZipCode"] = "Postal code you have entered is not valid.";
                return Ok(JsonResult("true"));
        }
        else
            {
                return Ok(Json("Invalid"));
            }
        }

        private object JsonResult(string v)
        {
            throw new NotImplementedException();
        }

        object Json(string v)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
        public ActionResult ScheduleService(ScheduleService schedule)
        {
           
             if (ModelState.IsValid)
             {

                
                 return Ok(Json("true"));


             }
             else
             {
               
                return Ok(Json("false"));
             }

         

        }


        [HttpGet]
        public IActionResult DetailsService(PostalCode obj)
        {

            int Id = -1;

            List<Address> Addresses = new List<Address>();
            if(HttpContext.Session.GetInt32("userId") != null)
            {
                Id = (int)HttpContext.Session.GetInt32("userId");
            }
            else if(Request.Cookies["userId"] != null)
            {
                Id = int.Parse(Request.Cookies["userId"]);
             
            }

            string postalcode = obj.postalcode;
            Console.WriteLine(obj.postalcode);
            var table = db.UserAddresses.Where(x => x.UserId == Id && x.PostalCode == postalcode).ToList();
            Console.WriteLine(table.ToString());

            foreach (var add in table)
            {
                Console.WriteLine("1");
                Address useradd = new Address();
                useradd.AddressId = add.AddressId;
                useradd.AddressLine1 = add.AddressLine1;
                useradd.AddressLine2 = add.AddressLine2;
                useradd.City = add.City;
                useradd.PostalCode = add.PostalCode;
                useradd.Mobile = add.Mobile;
                useradd.isDefault = add.IsDefault;
                
                Addresses.Add(useradd);
            }
            Console.WriteLine("2");

            return new JsonResult(Addresses);
        }


        [HttpPost]
        public IActionResult AddNewAddress(UserAddress useradd)
        {

            if (ModelState.IsValid)
            {


                Console.WriteLine("Inside Addnew address 1");
                int Id = -1;


                if (HttpContext.Session.GetInt32("userId") != null)
                {
                    Id = (int)HttpContext.Session.GetInt32("userId");
                }
                else if (Request.Cookies["userId"] != null)
                {
                    Id = int.Parse(Request.Cookies["userId"]);

                }
                Console.WriteLine("Inside Addnew address 2");
                Console.WriteLine(Id);

                useradd.UserId = Id;
                useradd.IsDefault = false;
                useradd.IsDeleted = false;
                User user = db.Users.Where(x => x.UserId == Id).FirstOrDefault();
                useradd.Email = user.Email;
                var result = db.UserAddresses.Add(useradd);
                Console.WriteLine("Inside Addnew address 3");
                db.SaveChanges();

                Console.WriteLine("Inside Addnew address 4");
                if (result != null)
                {
                    return Ok(Json("true"));
                }

                return Ok(Json("false"));

            }
            return View();
        }




        public ActionResult CompleteBooking(CompleteBooking complete)
        {
            int Id = -1;

          
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                Id = (int)HttpContext.Session.GetInt32("userId");
            }
            else if (Request.Cookies["userId"] != null)
            {
                Id = int.Parse(Request.Cookies["userId"]);

            }


            ServiceRequest add = new ServiceRequest();
            add.UserId = Id;
            add.ServiceId = Id;
            add.ServiceStartDate = complete.ServiceStartDate;
            add.ServiceHours = (double)complete.ServiceHours;
            add.ZipCode = complete.PostalCode;
            add.ServiceHourlyRate = 25;
            add.ExtraHours = complete.ExtraHours;
            add.SubTotal = (decimal)complete.SubTotal;
            add.TotalCost = (decimal)complete.TotalCost;
            add.Comments = complete.Comments;
            add.PaymentDue = false;
            add.PaymentDone = true;
            add.HasPets = complete.HasPets;
            add.CreatedDate = DateTime.Now;
            add.ModifiedDate = DateTime.Now;
            add.HasIssue = false;

            var result = db.ServiceRequests.Add(add);
            db.SaveChanges();

            UserAddress useraddr = db.UserAddresses.Where(x => x.AddressId == complete.AddressId).FirstOrDefault();

            ServiceRequestAddress srAddr = new ServiceRequestAddress();
            srAddr.AddressLine1 = useraddr.AddressLine1;
            srAddr.AddressLine2 = useraddr.AddressLine2;
            srAddr.City = useraddr.City;
            srAddr.Email = useraddr.Email;
            srAddr.Mobile = useraddr.Mobile;
            srAddr.PostalCode = useraddr.PostalCode;
            srAddr.ServiceRequestId = result.Entity.ServiceRequestId;
            srAddr.State = useraddr.State;

            var srAddrResult = db.ServiceRequestAddresses.Add(srAddr);
            db.SaveChanges();

            if (complete.Cabinet == true)
            {
                ServiceRequestExtra srExtra = new ServiceRequestExtra();
                srExtra.ServiceRequestId = result.Entity.ServiceRequestId;
                srExtra.ServiceExtraId = 1;
                db.ServiceRequestExtras.Add(srExtra);
                db.SaveChanges();
            }
            if (complete.Fridge == true)
            {
                ServiceRequestExtra srExtra = new ServiceRequestExtra();
                srExtra.ServiceRequestId = result.Entity.ServiceRequestId;
                srExtra.ServiceExtraId = 2;
                db.ServiceRequestExtras.Add(srExtra);
                db.SaveChanges();
            }
            if (complete.Oven == true)
            {
                ServiceRequestExtra srExtra = new ServiceRequestExtra();
                srExtra.ServiceRequestId = result.Entity.ServiceRequestId;
                srExtra.ServiceExtraId = 3;
                db.ServiceRequestExtras.Add(srExtra);
                db.SaveChanges();
            }
            if (complete.Laundry == true)
            {
                ServiceRequestExtra srExtra = new ServiceRequestExtra();
                srExtra.ServiceRequestId = result.Entity.ServiceRequestId;
                srExtra.ServiceExtraId = 4;
                db.ServiceRequestExtras.Add(srExtra);
                db.SaveChanges();
            }
            if (complete.Window == true)
            {
                ServiceRequestExtra srExtra = new ServiceRequestExtra();
                srExtra.ServiceRequestId = result.Entity.ServiceRequestId;
                srExtra.ServiceExtraId = 5;
                db.ServiceRequestExtras.Add(srExtra);
                db.SaveChanges();
            }
           
            if (result != null && srAddrResult != null)
            {
                return Ok(Json(result.Entity.ServiceRequestId));
            }

            return Ok(Json("false"));
        }

    }
}
