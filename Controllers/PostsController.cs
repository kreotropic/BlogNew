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

namespace BlogNew.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        public ActionResult Index(int? scrollPosition)
        {

            var posts = db.Posts.ToList();

            foreach (var post in posts)
            {
                post.ThumbsCount = ThumbsCount(post.PostId);
            }

            if (scrollPosition.HasValue)
            {
                TempData["ScrollPosition"] = scrollPosition.Value;
            }

            return View(posts);
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
                post.Sinopse = post.Content.Length > 256 ? post.Content.Substring(0, 256) : post.Content;
                post.UserId = User.Identity.GetUserId();

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

        private int ThumbsCount(int postId)
        {
            return db.Thumbs.Count(t => t.PostId == postId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThumbsUp(int postId)
        {
            var post = db.Posts.Find(postId);

            if (post != null)
            {
                post.ThumbsCount += 1;

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
                } else
                {
                    Thumb thumb = db.Thumbs.FirstOrDefault(t => t.PostId == postId && t.UserId == userId);
                    db.Thumbs.Remove(thumb);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }


    }
}
