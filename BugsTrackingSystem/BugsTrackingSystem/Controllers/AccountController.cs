﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BugsTrackingSystem.Models;
using AsignarServices.Data;

namespace BugsTrackingSystem.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly Lazy<AsignarDataService> _dataService = new Lazy<AsignarDataService>(() => new AsignarDataService());

        public AccountController()
        {

        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                int? userId = _dataService.Value.ValidateUser(model);
                if (userId != null)
                {
                    var userToken = new FormsAuthenticationTicket(1, userId.Value.ToString(), DateTime.Now, DateTime.Now.AddMinutes(10),
                        false, _dataService.Value.GetRoleByUserId(userId.Value));
                    var headerToken = FormsAuthentication.Encrypt(userToken);

                    if (!string.IsNullOrEmpty(headerToken))
                    {
                        Response.Cookies.Add(new HttpCookie("Auth", headerToken));
                        return RedirectToAction("Home", "Manage");
                    }
                    else
                    {
                        ModelState.AddModelError("", "failed to create token");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect login or password.");
                }
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            _dataService.Value.Dispose();
            base.Dispose(disposing);
        }

    }
}