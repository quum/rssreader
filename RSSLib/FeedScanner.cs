using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using CodeHollow.FeedReader;
using System.Threading.Tasks;
using System.IO;

namespace RSSLib
{
    public class FeedScanner
    {
        private Database _db;

        public FeedScanner(Database db)
        {
            _db = db;      
        }

        private async Task processUrl(Feed feed)
        {
            var error = 0;
            var link = feed.HtmlUrl;
        
            try
            {
                var data = await FeedReader.ReadAsync(feed.XmlUrl);
                link = data.Link;
              
              
                foreach (var item in data.Items)
                {
                    Article.Insert(_db, new Article()
                    {
                        Content = item.Content,
                        Link = item.Link,
                        Pubdate = item.PublishingDateString,
                        Read = 0,
                        Title = item.Title,
                        FeedId = feed.Id
                    });
                }
            }
            catch
            {
                error = 1;
            }
            if (feed.Error != error || feed.HtmlUrl != link )
            {
                feed.Error = error;
              
                feed.HtmlUrl = link.Replace("/feeds/posts/default", string.Empty);
                Feed.InsertOrUpdate(_db, feed);
            }
        }

        public void Scan()
        {
            var tasks = new List<Task>();
            foreach (var item in Feed.QueryAll(_db).OrderBy(x => x.Title))
            {
                tasks.Add(processUrl(item));
            }
            Task.WhenAll(tasks).Wait();
        }

        public void ScanOne(Feed feed)
        {
            processUrl(feed).Wait();
        }
    }
}
