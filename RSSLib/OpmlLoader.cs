using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace RSSLib
{
    public class OpmlLoader
    {
        private Database _db;

        public OpmlLoader(Database db)
        {
            _db = db;

        }

        public void LoadOpmlFile(string filename, bool merge)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Opml));
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                var result = (Opml)serializer.Deserialize(fileStream);

                if(!merge)
                {
                    Feed.Truncate(_db);
                    Article.Truncate(_db);
                }

                foreach(var item in result.Body.Outline)
                {
                    Feed.InsertOrUpdate(_db, new Feed() { HtmlUrl = item.HtmlUrl.Trim(), Title = item.Title.Trim(), XmlUrl = item.XmlUrl.Trim() });
                }

            }
        }
    }
}
