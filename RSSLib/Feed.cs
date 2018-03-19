using System;
using System.Collections.Generic;
using System.Linq;

namespace RSSLib
{
    public class Feed
    {
        public int Id { get; set; }
        public string XmlUrl { get; set; }
        public string Title { get; set; }
        public string HtmlUrl { get; set; }
        public int Error { get; set; }
        public DateTime Inserted { get; set; }
        public DateTime Updated { get; set; }


        public static void InsertOrUpdate(Database db, Feed item)
        {
            var count = db.ExecuteNonQuery(string.Format("insert ignore into `{0}`.`feed` (`xmlUrl`,`title`,`htmlUrl`,`error`,`inserted`,`updated`) values (@XmlUrl,@Title,@HtmlUrl,@Error,current_timestamp(),current_timestamp())", db.Schema), item);
            if (count == 0)
            {
                db.ExecuteNonQuery(string.Format("update `{0}`.`feed` set `title` = @Title,`htmlUrl` = @HtmlUrl,`error` = @Error,`updated` = current_timestamp() where `xmlUrl` = @XmlUrl ", db.Schema), item);
            }
        }

        public static void Truncate(Database db)
        {
            db.ExecuteNonQuery($"truncate table `{db.Schema}`.`feed`");
        }

        public static List<Feed> QueryAll(Database db)
        {
            return db.Query<Feed>($"select * from `{db.Schema}`.`feed`");
        }

        public static Feed QueryById(Database db , int id)
        {
            return db.Query<Feed>($"select * from `{db.Schema}`.`feed` where `id` = @Id", new { Id = id }).FirstOrDefault();
        }

        public static Feed QueryByXmlUrl(Database db, string xmlUrl)
        {
            return db.Query<Feed>($"select * from `{db.Schema}`.`feed` where `xmlurl` = @XmlUrl", new { XmlUrl = xmlUrl }).FirstOrDefault();
        }

        public static void DeleteById(Database db, int id)
        {
            db.ExecuteNonQuery($"delete from `{db.Schema}`.`feed` where `id` = @Id", new { Id = id });
        }
    }
}
