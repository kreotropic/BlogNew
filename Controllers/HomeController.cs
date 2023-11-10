using BlogNew.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace BlogNew.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return view to Posts index
            //return View();
            return RedirectToAction("Index", "Posts");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }       
        

  
    }
}