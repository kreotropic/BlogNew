using BlogNew.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace BlogNew.Controllers
{
    public class ArchiveController : Controller
    {
        // GET: Archive
        public ActionResult Index()
        {
            var tree = GetArchiveTree();
            
            return View(tree);
        }
        private ArchiveTreeModel GetArchiveTree()
        {
            List<Post> posts = GetOrderedPosts();
            if (posts == null || posts.Count == 0) return new ArchiveTreeModel();

            var tree = new ArchiveTreeModel();

            //set first branch
            SetNewYearBranch(tree, posts[0]);
            
            for (int i = 1; i < posts.Count; i++)
            {
                //gets years of current and previously added post
                int currentYear = posts[i].CreatedAt.Year;
                int previousYear = posts[i-1].CreatedAt.Year;

                if (currentYear != previousYear)
                {
                    //If moved on to new year creates new year branch
                    SetNewYearBranch(tree, posts[i]);
                    continue;
                }

                //gets months of current and previously added post
                int currentMonth = posts[i].CreatedAt.Month;
                int previousMonth = posts[i - 1].CreatedAt.Month;

                if (currentMonth != previousMonth)
                {
                    //If moved on to new month creates new month branch
                    SetNewMonthBranch(tree, posts[i]);
                    continue;
                }

                //if still in the same month and year node sets new post node
                SetNewPostNode(tree, posts[i]);
            }

            return tree;
        }

        private List<Post> GetOrderedPosts()
        {
            using (var db = new ApplicationDbContext())
            {
                var query = from p in db.Posts
                            where !p.IsPrivate
                            orderby p.CreatedAt descending
                            select p;
                return query.ToList();
            };
        }

        private void SetNewYearBranch(ArchiveTreeModel tree, Post post)
        {
            //create year node and add to parent node
            var yearNode = new ArchiveYearModel();
            yearNode.Year = post.CreatedAt.Year;
            tree.Years.Add(yearNode);

            //Then set new month branch
            SetNewMonthBranch(tree, post);
        }

        private void SetNewMonthBranch(ArchiveTreeModel tree, Post post)
        {
            //create month node
            var monthNode = new ArchiveMonthModel();
            monthNode.Month = post.CreatedAt.ToString("MMMM", CultureInfo.InvariantCulture);
            //Add month node to current year node
            int lastYearIndex = tree.Years.Count - 1;
            tree.Years[lastYearIndex].Months.Add(monthNode);

            //set new Post node
            SetNewPostNode(tree, post);
        }

        private void SetNewPostNode(ArchiveTreeModel tree, Post post)
        {
            //creates post node
            var postNode = new ArchivePostModel();
            postNode.Id = post.PostId;
            postNode.Title = post.Title;
            postNode.CreatedAt = post.CreatedAt;

            //adds post node to last month node that is a child of the last year node.
            //Also increments Total number of posts for that year :)
            int lastYearIndex = tree.Years.Count - 1;
            tree.Years[lastYearIndex].TotalPosts++;
            int lastMonthIndex = tree.Years[lastYearIndex].Months.Count - 1;
            tree.Years[lastYearIndex].Months[lastMonthIndex].Posts.Add(postNode);
        }
    }
}