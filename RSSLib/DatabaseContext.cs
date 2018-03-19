using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSLib
{
    public class DatabaseContext
    {
        private string _connectionString;

        private string _user;
        private string _password;
        private string _server;
        private string _schema;
        private int _port;

        public string Schema { get { return _schema; } set { _schema = value; } }
         

        public DatabaseContext()
        {
            _user = "root";
            _password = "password";
            _server = "127.0.0.1";
            _schema = "somedatabase";
        }

        public static DatabaseContext Parse(string str)  // Schema:User:Password:Server:Port
        {
            string[] p = str.Split(':');
            return new DatabaseContext() { _schema = p[0], _user = p.Length > 1 ? p[1] : "root", _password = p.Length > 2 ? p[2] : "password", _server = p.Length > 3 ? p[3] : "127.0.0.1", _port = (p.Length > 4 ? Int32.Parse(p[4]) : 3306) };
        }

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
            foreach(var item in _connectionString.Split(';'))
            {
                if (item.ToLower().StartsWith("database="))
                {
                    var p = item.Split('=');
                    _schema = p[1];
                    break;
                }
            }
        }

        public string ConnectionString
        {
           get
            {
                if(string.IsNullOrWhiteSpace(_connectionString))
                {
                    if (_port != 0)
                    {
                        _connectionString = string.Format("Server={0};Database={1};Uid={2};Pwd={3};Connect Timeout=30;Port={4};default command timeout=360", _server, _schema, _user, _password, _port);
                    }
                    else
                    {
                        _connectionString = string.Format("Server={0};Database={1};Uid={2};Pwd={3};Connect Timeout=30;default command timeout=360", _server, _schema, _user, _password);
                    }
                }
                return _connectionString;
            }
        }
    }
}
