using System;
using System.Collections.Generic;
using System.Text;

namespace NessusApis
{
    class UserInfo
    {
        private string _username;
        private string _password;

        public UserInfo(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public string username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string password
        {
            get { return _password; }
            set { _password = value; }
        }

    }
}
