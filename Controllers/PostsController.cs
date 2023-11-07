using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlogNew.Models;
using Microsoft.AspNet.Identity;
using PagedList;

namespace BlogNew.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var posts = (from p in db.Posts
                         join u in db.Users on p.UserId equals u.Id
                         select new { Post = p, User = u })
                        .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.User.UserName.Contains(searchString)).ToList();
            }

            foreach (var post in posts)
            {
                post.Post.ThumbsCount = ThumbsCount(post.Post.PostId, db);
            }

            switch (sortOrder)
            {
                case "Date":
                    posts = posts.OrderBy(p => p.Post.CreatedAt).ToList();
                    break;
                case "date_desc":
                    posts = posts.OrderByDescending(p => p.Post.CreatedAt).ToList();
                    break;
                default:
                    posts = posts.OrderByDescending(p => p.Post.CreatedAt).ToList();
                    break;
            }

            int pageSize = 4; 
            int pageNumber = (page ?? 1);

            return View(posts.Select(p => p.Post).ToPagedList(pageNumber, pageSize));
        }





        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            return View(new BlogNew.Models.Post());
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(/*[Bind(Include = "UserId,Title,Sinopse,Content,CreatedAt,UpdatedAt,IsPrivate")]*/ Post post)
        {
            if (ModelState.IsValid)
            {
                
                post.UserId = User.Identity.GetUserId();
                int maxLength = 256;

                if (post.Content.Length > maxLength)
                {
                    int lastSpace = post.Content.LastIndexOf(' ', maxLength);

                    if (lastSpace != -1)
                    {
                        // Trim at the last space within the first 256 characters
                        post.Sinopse = post.Content.Substring(0, lastSpace);
                    }
                    else
                    {
                        // No space found within the first 256 characters, so just trim at the character limit
                        post.Sinopse = post.Content.Substring(0, maxLength);
                    }
                }
                else
                {
                    // Content is already within the limit
                    post.Sinopse = post.Content;
                }

                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", post.UserId);
            return View(post);
        }

        // GET: Posts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", post.UserId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(/*[Bind(Include = "PostId,UserId,Title,Sinopse,Content,CreatedAt,UpdatedAt,IsPrivate")]*/ Post post)
        {
            if (ModelState.IsValid)
            {
                post.UserId = User.Identity.GetUserId();
                post.Sinopse = post.Content.Length > 256 ? post.Content.Substring(0, 256) : post.Content;
                db.Entry(post).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", post.UserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThumbsUp(int postId)
        {
            try
            {
                var post = db.Posts.FirstOrDefault(p => p.PostId == postId);

                if (post != null)
                {
                    post.ThumbsCount = db.Thumbs.Count(t => t.PostId == postId);

                    string userId = User.Identity.GetUserId();

                    bool alreadyThumbed = db.Thumbs.Any(t => t.PostId == postId && t.UserId == userId);

                    if (!alreadyThumbed)
                    {
                        Thumb thumb = new Thumb
                        {
                            UserId = userId,
                            PostId = postId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        db.Thumbs.Add(thumb);
                    }
                    else
                    {
                        Thumb thumb = db.Thumbs.FirstOrDefault(t => t.PostId == postId && t.UserId == userId);
                        db.Thumbs.Remove(thumb);
                    }

                    db.SaveChanges();

                    return Json(new { ThumbsCount = post?.ThumbsCount ?? 0 });
                }
                else
                {
                    return Json(new { ThumbsCount = 0, Error = "Post not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { ThumbsCount = 0, Error = ex.Message });
            }
        }



        private int ThumbsCount(int postId, ApplicationDbContext dbContext)
        {
            return dbContext.Thumbs.Count(t => t.PostId == postId);
        }

    }
}
