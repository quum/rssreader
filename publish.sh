rm -f /tmp/rssutil.tar.gz /tmp/rssreader.tar.gz
dotnet publish
tar cfz /tmp/rssutil.tar.gz -C ./RSSUtil/bin/Debug/netcoreapp2.0/publish .
tar cfz /tmp/rssreader.tar.gz -C ./RSSReader/bin/Debug/netcoreapp2.0/publish .
ls -l /tmp/rssutil.tar.gz /tmp/rssreader.tar.gz


