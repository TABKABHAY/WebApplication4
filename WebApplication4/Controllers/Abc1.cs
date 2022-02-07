using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4.Controllers
{
    public class Abc1 : Controller
    {
        private readonly ContactU contactU;
        public Abc1(ContactU ContactU)
        {
            contactU = ContactU;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
