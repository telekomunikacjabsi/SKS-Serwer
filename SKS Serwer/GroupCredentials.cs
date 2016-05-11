using System;
using System.Text.RegularExpressions;

namespace SKS_Serwer
{
    public class GroupCredentials
    {
        public string ID { get; }
        public string Password { get; }

        public GroupCredentials(string ID, string password)
        {
            this.ID = ID;
            Password = password;
        }

        public GroupCredentials(string credentialsString)
        {
            string[] args = Regex.Split(credentialsString, "::");
            if (args == null || args.Length != 2)
                throw new ArgumentOutOfRangeException();
            ID = args[0];
            Password = args[1];
        }

        public override string ToString()
        {
            return ID + "::" + Password;
        }
    }
}
