using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
        private const int IndexPageSize = 8;

        public ActionResult Index(string search, int page = 1)
        {
            //Add current search filter to view bag because of paginated returns
            ViewBag.currentSearch = search;

            //query - returns all public posts and their respective authors
            var query = from p in db.Posts
                        join u in db.Users on p.UserId equals u.Id
                        where !p.IsPrivate
                        orderby (p.CreatedAt) descending
                        select new PostIndexViewModel { Post = p, User = u };

            //if search query inserted than returns filtered list, else return all public posts
            List<PostIndexViewModel> posts;
            if (!String.IsNullOrEmpty(search))
            {
                posts = query.Where(x => x.Post.Title.Contains(search)).ToList();
            }
            else
            {
                posts = query.ToList();
            }

            //Returns appropriate view if no results found
            if (posts.Count == 0)
            {
                return View("~/Views/Posts/NoPosts.cshtml");
            }

            //See if current user has liked and sets Thumb counter for each post
            foreach (var p in posts)
            {
                Post post = p.Post;
                p.HasUserLiked = HasCurrentUserLiked(post.PostId);
                post.ThumbsCount = db.Thumbs.Count(t => t.PostId == post.PostId);
            }

            return View(posts.ToPagedList(page, IndexPageSize));
        }







        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Include(p => p.User).FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
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
                int maxLength = 350;

                string contentWithoutImages = Regex.Replace(post.Content, "<img[^>]*>", "", RegexOptions.IgnoreCase);

                string[] lines = contentWithoutImages.Split('\n');
                lines = lines
                    .Select(line => line.Trim()) 
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line != "<p>&nbsp;</p>")
                    .ToArray();

                contentWithoutImages = string.Join("\n", lines);

                if (contentWithoutImages.Length > maxLength)
                {
                    int lastSpace = contentWithoutImages.LastIndexOf(' ', maxLength);

                    if (lastSpace != -1)
                    {
                        post.Sinopse = contentWithoutImages.Substring(0, lastSpace);
                    }
                    else
                    {
                        post.Sinopse = contentWithoutImages.Substring(0, maxLength);
                    }
                }
                else
                {
                    post.Sinopse = contentWithoutImages;
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

            //If current logged in user is different from post author and is not an admin return not found
            if (post.UserId != User.Identity.GetUserId() && !User.IsInRole("Admin"))
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
        [Authorize]
        public ActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                //Gets post object from DB
                Post postDB = db.Posts.Find(post.PostId);
                if (postDB == null)
                {
                    return HttpNotFound();
                }
                //If for whatever reason user associated with post is different from current logged in user and not an admin return error page
                if (postDB.UserId != User.Identity.GetUserId() && !User.IsInRole("Admin"))
                {
                    return View("~/Views/Shared/Error.cshtml");
                }

                db.Posts.Attach(postDB);

                int maxLength = 350;

                string contentWithoutImages = Regex.Replace(post.Content, "<img[^>]*>", "", RegexOptions.IgnoreCase);

                string[] lines = contentWithoutImages.Split('\n');
                lines = lines
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line != "<p>&nbsp;</p>")
                    .ToArray();

                contentWithoutImages = string.Join("\n", lines);

                if (contentWithoutImages.Length > maxLength)
                {
                    int lastSpace = contentWithoutImages.LastIndexOf(' ', maxLength);

                    if (lastSpace != -1)
                    {
                        post.Sinopse = contentWithoutImages.Substring(0, lastSpace);
                    }
                    else
                    {
                        post.Sinopse = contentWithoutImages.Substring(0, maxLength);
                    }
                }
                else
                {
                    post.Sinopse = contentWithoutImages;
                }

                postDB.Title = post.Title;
                postDB.Content = post.Content;
                postDB.UpdatedAt = DateTime.UtcNow;
                postDB.IsPrivate = post.IsPrivate;

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

            // If current logged-in user is different from post author and not an admin, return not found
            if (post.UserId != User.Identity.GetUserId() && !User.IsInRole("Admin"))
            {
                return HttpNotFound();
            }

            // Get the username of the post author
            string authorUsername = db.Users.Where(u => u.Id == post.UserId).Select(u => u.UserName).FirstOrDefault();

            // Pass the author username to the view
            ViewBag.AuthorUsername = authorUsername;

            return View(post);
        }


        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            //If for whatever reason user associated with post is different from current logged in user and not an admin return error page
            if (post.UserId != User.Identity.GetUserId() && !User.IsInRole("Admin"))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
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

                    System.Diagnostics.Debug.WriteLine(post.ThumbsCount);


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

                    return Json(new { ThumbsCount = post?.ThumbsCount ?? 0, AlreadyThumbed = alreadyThumbed });
                }
                else
                {
                    return Json(new { ThumbsCount = 0, AlreadyThumbed = false, Error = "Post not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { ThumbsCount = 0, AlreadyThumbed = false, Error = ex.Message });
            }
        }

        private bool HasCurrentUserLiked(int postId)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return false;
            }
            return db.Thumbs.Any(t => t.UserId == userId && t.PostId == postId);
        }








        private int ThumbsCount(int postId, ApplicationDbContext dbContext)
        {
            return dbContext.Thumbs.Count(t => t.PostId == postId);
        }

    }
}
