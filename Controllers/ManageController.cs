﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BlogNew.Models;
using System.Net;
using System.Collections.Generic;
using System.Web.Services.Description;
using PagedList;
using System.Drawing;

namespace BlogNew.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        const int PageSize = 12;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            ViewBag.IsAdmin = User.IsInRole("Admin");

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PostsAdmin(string searchTitle, string searchUser, string sortOrder, int page = 1)
        {
            //Add current search and sort parameters to view bag because of paginated returns
            ViewBag.currentTitleSearch = searchTitle;
            ViewBag.currentUserSearch = searchUser;
            ViewBag.currentSort = sortOrder;

            //Define sort argument in view to the reverse order of current argument.
            //So that next time there is a click in the same column it reverses the order.
            ViewBag.ThumbsSortArg = sortOrder == "thumbs_desc" ? "thumbs_asc" : "thumbs_desc";
            ViewBag.PrivateSortArg = sortOrder == "private" ? "public" : "private";

            //Validation necessary because by default list is ordered by date descending 
            if (String.IsNullOrEmpty(sortOrder))
            {
                ViewBag.DateSortArg = "date_asc";
            }
            else
            {
                ViewBag.DateSortArg = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            }

            var list = GetAllPosts(sortOrder, searchTitle, searchUser);

            return View(list.ToPagedList(page, PageSize));
        }

        //Manage current http context user's posts
        public ActionResult Posts(string search, string sortOrder, int page = 1)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Add current search and sort parameters to view bag because of paginated returns
            ViewBag.currentSearch = search;
            ViewBag.currentSort = sortOrder;

            //Define sort argument in view to the reverse order of current argument.
            //So that next time there is a click in the same column it reverses the order.
            ViewBag.ThumbsSortArg = sortOrder == "thumbs_desc" ? "thumbs_asc" : "thumbs_desc";
            ViewBag.PrivateSortArg = sortOrder == "private" ? "public" : "private";

            //Validation necessary because by default list is ordered by date descending 
            if (String.IsNullOrEmpty(sortOrder))
            {
                ViewBag.DateSortArg = "date_asc";
            }
            else
            {
                ViewBag.DateSortArg = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            }

            var list = GetPosts(userId, sortOrder, search);

            return View(list.ToPagedList(page, PageSize));
        }

        #region Helpers
        public static HtmlString SetSortOrderIcon(string colHeader, string currentSort)
        {
            if (string.IsNullOrWhiteSpace(currentSort))
            {
                return null;
            }

            if (colHeader != currentSort.Substring(0, colHeader.Length))
            {
                return null;
            }

            const string UpArrow = "&#x25B2";
            const string DownArrow = "&#x25BC";

            string asc = currentSort.Substring(currentSort.Length - 3);

            if (asc == "asc")
            {
                return new HtmlString(string.Format("<span>{0}</span>", UpArrow));
            }

            return new HtmlString(string.Format("<span>{0}</span>", DownArrow));
        }

        public static HtmlString SetPrivacyIcon(string currentSort)
        {
            if (string.IsNullOrWhiteSpace(currentSort))
            {
                return null;
            }

            if (currentSort == "private")
            {
                return new HtmlString("<i class=\"bi bi-eye-slash-fill\"></i>");
            }

            if (currentSort == "public")
            {
                return new HtmlString("<i class=\"bi bi-eye-fill\"></i>");
            }

            return null;
        }

        public static string AppendSearchRouteValue(string currentSearch)
        {
            if (string.IsNullOrWhiteSpace(currentSearch))
            {
                return null;
            }
            return string.Format("&search={0}", currentSearch);
        }

        public static string AppendSearchRouteValue(string currentTitleSearch, string currentUserSearch)
        {
            string value = "";
            if (!string.IsNullOrWhiteSpace(currentTitleSearch))
            {
                value += string.Format("&searchTitle={0}", currentTitleSearch);
            }
            if (!string.IsNullOrWhiteSpace(currentUserSearch))
            {
                value += string.Format("&searchUser={0}", currentUserSearch);
            }
            return value;
        }

        private List<ManagePostViewModel> GetPosts(string userId, string sortOrder, string search)
        {
            using (var db = new ApplicationDbContext())
            {
                //query declarations. When executed in the switch case below they return list of ManagePostViewModel objects created from db data
                IQueryable<Post> query1;
                if (string.IsNullOrWhiteSpace(search))
                {
                    query1 = from p in db.Posts
                             where p.UserId == userId
                             select p;
                }
                else
                {
                    query1 = from p in db.Posts
                             where p.UserId == userId 
                             && p.Title.Contains(search)
                             select p;
                }

                var query2 = query1.Select(p => new ManagePostViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    CreatedAt = p.CreatedAt,
                    Private = p.IsPrivate,
                    Thumbs = db.Thumbs.Count(t => t.PostId == p.PostId),
                });

                List<ManagePostViewModel> posts;
                //executes query with sort
                switch (sortOrder)
                {
                    case "thumbs_asc":
                        posts = query2.OrderBy(p => p.Thumbs).ToList();
                        break;
                    case "thumbs_desc":
                        posts = query2.OrderByDescending(p => p.Thumbs).ToList();
                        break;
                    case "private":
                        posts = query2.OrderByDescending(p => p.Private).ToList();
                        break;
                    case "public":
                        posts = query2.OrderBy(p => p.Private).ToList();
                        break;
                    case "date_asc":
                        posts = query2.OrderBy(p => p.CreatedAt).ToList();
                        break;
                    default:
                        posts = query2.OrderByDescending(p => p.CreatedAt).ToList();
                        break;
                }

                return posts;
            }
        }

        private IQueryable<ManagePostViewModel> SetAdminQuery(string searchTitle, string searchUser, ApplicationDbContext db)
        {
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                if (!string.IsNullOrWhiteSpace(searchUser))
                {
                    return from p in db.Posts
                           join u in db.Users on p.UserId equals u.Id
                           where p.Title.Contains(searchTitle)
                           && u.UserName.Contains(searchUser)
                           select new ManagePostViewModel
                           {
                               PostId = p.PostId,
                               Title = p.Title,
                               CreatedAt = p.CreatedAt,
                               Private = p.IsPrivate,
                               Thumbs = db.Thumbs.Count(t => t.PostId == p.PostId),
                               Author = u.UserName
                            };
                }
                else
                {
                    return from p in db.Posts
                            join u in db.Users on p.UserId equals u.Id
                            where p.Title.Contains(searchTitle)
                           select new ManagePostViewModel
                             {
                                 PostId = p.PostId,
                                 Title = p.Title,
                                 CreatedAt = p.CreatedAt,
                                 Private = p.IsPrivate,
                                 Thumbs = db.Thumbs.Count(t => t.PostId == p.PostId),
                                 Author = u.UserName
                             };
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchUser))
                {
                    return from p in db.Posts
                             join u in db.Users on p.UserId equals u.Id
                             where u.UserName.Contains(searchUser)
                           select new ManagePostViewModel
                             {
                                 PostId = p.PostId,
                                 Title = p.Title,
                                 CreatedAt = p.CreatedAt,
                                 Private = p.IsPrivate,
                                 Thumbs = db.Thumbs.Count(t => t.PostId == p.PostId),
                                 Author = u.UserName
                             };
                }
                else
                {
                    return from p in db.Posts
                             join u in db.Users on p.UserId equals u.Id
                             select new ManagePostViewModel
                             {
                                 PostId = p.PostId,
                                 Title = p.Title,
                                 CreatedAt = p.CreatedAt,
                                 Private = p.IsPrivate,
                                 Thumbs = db.Thumbs.Count(t => t.PostId == p.PostId),
                                 Author = u.UserName
                             };
                }
            }
        }

        private List<ManagePostViewModel> GetAllPosts(string sortOrder, string searchTitle, string searchUser)
        {
            using (var db = new ApplicationDbContext())
            {
                var query = SetAdminQuery(searchTitle, searchUser, db);

                List<ManagePostViewModel> posts;
                //executes query with sort
                switch (sortOrder)
                {
                    case "thumbs_asc":
                        posts = query.OrderBy(p => p.Thumbs).ToList();
                        break;
                    case "thumbs_desc":
                        posts = query.OrderByDescending(p => p.Thumbs).ToList();
                        break;
                    case "private":
                        posts = query.OrderByDescending(p => p.Private).ToList();
                        break;
                    case "public":
                        posts = query.OrderBy(p => p.Private).ToList();
                        break;
                    case "date_asc":
                        posts = query.OrderBy(p => p.CreatedAt).ToList();
                        break;
                    default:
                        posts = query.OrderByDescending(p => p.CreatedAt).ToList();
                        break;
                }

                return posts;
            }
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }


        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}