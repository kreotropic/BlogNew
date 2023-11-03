using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BlogNew.Migrations;
using BlogNew.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BlogNew.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

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

        // GET: Users
        public ActionResult Index(string search)
        {
            //Gets all users and respective roles
            var usersWithRoles = 
                (from user in db.Users
                select new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    IsDisabled = user.IsDisabled,
                    RoleNames = (from userRole in user.Roles
                                join role in db.Roles on userRole.RoleId
                                equals role.Id
                                select role.Name).ToList()
                }).ToList().Select(p => new UserViewModel()
                {
                    UserId = p.UserId,
                    Username = p.Username,
                    Email = p.Email,
                    IsDisabled = p.IsDisabled,
                    Roles = string.Join(",", p.RoleNames)
                });

            //If search query inserted returns list with users that contain query value in their usernames (the search is case insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                usersWithRoles = usersWithRoles.Where(
                    u => u.Username.ToLower().Contains(search.ToLower()));
            }

            return View(usersWithRoles);
        }

        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //Add role "User" to user
                    UserManager.AddToRole(user.Id, "User");

                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(string id)
        {
            //Verify id and get user
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            //Gets user role(s)
            var userRoles = UserManager.GetRoles(user.Id).ToArray();
            string userRole = userRoles[0];

            //Create user
            var userEdit = new UserEditViewModel { UserId =  user.Id, Username = user.UserName, Email = user.Email, Role = userRole, IsDisabled = user.IsDisabled };

            //Set SelectList of Roles
            SetRolesSelectList(userEdit);            

            return View(userEdit);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Username,Email,Role,IsDisabled")] UserEditViewModel EditedUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Get user
                    var user = db.Users.Find(EditedUser.UserId);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    //See if username was edited
                    if (user.UserName != EditedUser.Username)
                    {
                        //See if username already exists in DB
                        if (db.Users.Any(u => u.UserName == EditedUser.Username))
                        {
                            //Add error, repopulate SelectList and return view
                            ModelState.AddModelError("Username", "Username already taken.");
                            SetRolesSelectList(EditedUser);
                            return View(EditedUser);
                        }
                        //If it doesn't exist than Edit
                        user.UserName = EditedUser.Username;
                    }

                    //Edits rest of fields and updates entity in db
                    user.Email = EditedUser.Email;
                    user.IsDisabled = EditedUser.IsDisabled;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    //Roles
                    //Get Users current Roles
                    var userRoles = UserManager.GetRoles(user.Id).ToArray();
                    //Remove said roles from user
                    UserManager.RemoveFromRoles(user.Id, userRoles);
                    //Add Edited Role
                    UserManager.AddToRole(user.Id, EditedUser.Role);

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    return View("~/Views/Shared/Error.cshtml");
                }                
            }
            //If something unintended happens repopulate Selectlist and return view
            SetRolesSelectList(EditedUser);
            return View(EditedUser);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            //ViewModel
            var userRoles = UserManager.GetRoles(applicationUser.Id).ToArray();

            UserViewModel userViewModel = new UserViewModel
            {
                UserId = applicationUser.Id,
                Username = applicationUser.UserName,
                Email = applicationUser.Email,
                IsDisabled = applicationUser.IsDisabled,
                Roles = string.Join(",", userRoles)
            };

            return View(userViewModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);

            db.Users.Remove(applicationUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private IQueryable<string> GetAllRolesFromDB()
        {
            //query declaration
            var query = from r in db.Roles
                        orderby r.Name
                        select r.Name;
            //execute query - returns all distinct role names
            return query.Distinct();
        }

        private void SetRolesSelectList(UserEditViewModel model)
        {
            var roles = GetAllRolesFromDB();
            foreach (var role in roles)
            {
                bool isSelected = role == model.Role;
                model.SelectRoles.Add(new SelectListItem { Text = role, Value = role, Selected = isSelected });
            }
        }
    }
}
