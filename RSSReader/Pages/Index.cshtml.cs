using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RSSLib;

namespace RSSReader.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public List<Feed> Feeds { get; set; }
        public HashSet<int> FeedsWithNew { get; set; }
        
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private Database getDb()
        {
            return new Database(DatabaseContext.Parse(_configuration["Database:Schema"]));
        }


        public void OnGet()
        {
            Feeds = Feed.QueryAll(getDb()).OrderBy(x => x.Title).ToList();
           
            FeedsWithNew = Article.FeedsWithUnreadArticles(getDb());
        }

         
        public IActionResult OnGetArticles(int feedId)
        {
            var data = Article.QueryByFeedId(getDb(), feedId).OrderByDescending(x => x.Inserted);
            return new JsonResult(data);
        }

        public IActionResult OnGetFeed(int feedId)
        {
            return new JsonResult(Feed.QueryById(getDb(), feedId));
        }

        public IActionResult OnPostDeleteFeed(int feedId)
        {
            Article.DeleteByFeedId(getDb(), feedId);
            Feed.DeleteById(getDb(), feedId);
            return new JsonResult(new { ok = 1 });
        }
     
        public IActionResult OnPostMarkFeedAsRead(int feedId)
        {
            Article.MarkAsReadByFeedId(getDb(), feedId);
            return new JsonResult(new { ok = 1 });
        }

        public IActionResult OnPostMarkArticleAsRead(int id)
        {
            Article.MarkAsReadById(getDb(), id);
            return new JsonResult(new { ok = 1 });
        }

        public IActionResult OnPostAddFeed(string url)
        {           
            Feed.InsertOrUpdate(getDb(), new Feed() { XmlUrl = url });
            var feed = Feed.QueryByXmlUrl(getDb(), url);
            var feedScanner = new FeedScanner(getDb());
            feedScanner.ScanOne(feed);
            return new JsonResult(new { id = feed.Id });
        }

        public IActionResult OnPostEditFeed(int id, string name)
        {
            var feed = Feed.QueryById(getDb(), id);
            feed.Title = name;
            Feed.InsertOrUpdate(getDb(), feed);
            return new JsonResult(new { ok = 1 });
        }
    }
}
