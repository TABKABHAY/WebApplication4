using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4.Controllers
{
    public class Abc1 : Controller
    {
        private readonly ContactUs contactU;
        public Abc1(ContactUs ContactU)
        {
            contactU = ContactU;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
