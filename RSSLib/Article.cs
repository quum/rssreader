using System;
using System.Collections.Generic;
using System.Linq;

namespace RSSLib
{
    public class Article
    {
        public int FeedId { get; set; }
        public string Link { get; set; }
        public int Id { get; set; }
        public string Pubdate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Read { get; set; }
        public DateTime Inserted { get; set; }
        public DateTime Updated { get; set; }

        public static void InsertOrUpdate(Database db, Article item)
        {
            int count = Insert(db, item);
            if (count == 0)
            {
                db.ExecuteNonQuery(string.Format("update `{0}`.`article` set `pubdate` = @Pubdate,`title` = @Title,`content` = @Content,`read` = @Read,`updated` = current_timestamp() where `feedId` = @FeedId AND `link` = @Link ", db.Schema), item);
            }
        }

        public static int Insert(Database db, Article item)
        {
            return db.ExecuteNonQuery(string.Format("insert ignore into `{0}`.`article` (`feedId`,`link`,`pubdate`,`title`,`content`,`read`,`inserted`,`updated`) values (@FeedId,@Link,@Pubdate,@Title,@Content,@Read,current_timestamp(),current_timestamp())", db.Schema), item);
        }

        public static List<Article> QueryByFeedId(Database db, int feedId)
        {
            return db.Query<Article>($"select * from `{db.Schema}`.`article` where `feedId` = @FeedId", new { FeedId = feedId });
        }

        public static HashSet<int> FeedsWithUnreadArticles(Database db)
        {
            return new HashSet<int>(db.GetInt32List($"select distinct `feedId` from `{db.Schema}`.`article` where `read` = 0"));
        }

        public static void MarkAsReadByFeedId(Database db, int feedId)
        {
            db.ExecuteNonQuery($"update `{db.Schema}`.`article` set `read` = 1  where `feedId` = @FeedId", new { FeedId = feedId });
        }

        public static void MarkAsReadById(Database db, int id)
        {
            db.ExecuteNonQuery($"update `{db.Schema}`.`article` set `read` = 1 where `id` = @Id", new { Id = id });
        }

        public static void DeleteByFeedId(Database db, int feedId)
        {
            db.ExecuteNonQuery($"delete from `{db.Schema}`.`article` where `feedId` = @FeedId", new { FeedId = feedId });
        }

        public static void Truncate(Database db)
        {
            db.ExecuteNonQuery($"truncate table `{db.Schema}`.`article`");
        }
    }
}