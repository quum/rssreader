# rssreader

When google reader closed, I moved to digg reader. Now that is closing down too I thought rather than sign up with another news aggregation provider I would just write my own.

There are two components to be deployed

* RSSReader is the ASP .NET Core 2 server component that uses Razor Pages

* RSSUtil is a background utility that you can use to import any existing feeds you have in OPML format. Its main job is to sweep all the feeds you are subscribed to and update the database as new articles arrive.

As this is written using .NET Core 2, you can deploy it on Windows or Linux. 

Most likely you will want to deploy on Linux as the hosting is so much cheaper.

## Prerequisites

* [ASP.NET Core 2](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x) 

* mysql

* apache or nginx

## Create the database

mysql -u root --password=password --execute="create schema rssdb"

## Importing OPML

dotnet RSSUtil.dll -d rssdb -i nameofopml.xml

(database is specified as schema:user:password:port. user default=root, password default=password, server default=127.0.01, port default = 3306)

## Scanning for new articles 

dotnet RSSUtil.dll -d rssdb -s

(periodically run from crontab)

## Setting up the web site

RSSReader uses the .NET Core Kestrel webserver. You can't expose that to the Internet. 

You first set up RSSReader as a service, for example using systemd:

`[Unit]`
`Description=my RSS reader`
`[Service]`
`WorkingDirectory=/apps/rssreader`
`ExecStart=/usr/bin/dotnet /apps/rssreader/RSSReader.dll --urls "http://+:5001"`
`Restart=always`
`RestartSec=10`
`SyslogIdentifier=dotnet-rssreader`
`User=www-data`
`Environment=ASPNETCORE_ENVIRONMENT=Production`
`[Install]`
`WantedBy=multi-user.target`

Then you have configure a reverse proxy in to point to this app, for example in Apache:

`<VirtualHost *:80>`
`ServerName myrss.example.com`
`ProxyPreserveHost On`
`ProxyPass / http://127.0.0.1:5001/`
`ProxyPassReverse / http://127.0.0.1:5001/`
`ErrorLog /var/log/apache2/aspnetcoredemo-error.log`
`CustomLog /var/log/apache2/aspnetcodedemo-access.log common`
`</VirtualHost>`





