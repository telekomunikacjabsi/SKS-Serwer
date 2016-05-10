using System;
using System.IO;

namespace SKS_Serwer
{
    public class ClientWorker
    {
        Connection connection;
        Groups groups;
        ListManager listManager;
        Client client;

        public ClientWorker(Connection connection, Groups groups, ListManager listManager, string adminPort)
        {
            client = new Client();
            this.connection = connection;
            this.groups = groups;
            this.listManager = listManager;
            SetPort(adminPort);
        }

        private void SetPort(string port)
        {
            int _port = 9000;
            bool parseResult = Int32.TryParse(port, out _port);
            if (parseResult)
                client.AdminPort = port;
        }

        public void DoWork(string groupID, string groupPassword)
        {
            client.GroupID = groupID;
            client.IP = connection.IP;
            lock (ThreadLocker.Lock)
            {
                groups.AddClient(client, groupPassword);
            }
            connection.SendMessage(CommandSet.Auth, "SUCCESS");
            Console.WriteLine("Połączono klienta, grupa: \"{0}\", IP: \"{1}:{2}\"", groupID, connection.IP, connection.Port);
            while (true)
            {
                try
                {
                    connection.ReceiveMessage();
                    if (connection.Command == CommandSet.Disconnect)
                    {
                        Disconnect(connection);
                        return;
                    }
                    else if (connection.Command == CommandSet.VerifyList)
                        listManager.VerifyList(connection);
                    else
                    {
                        Disconnect(connection);
                        return;
                    }
                }
                catch (IOException)
                {
                    Disconnect(connection);
                    return;
                }
            }
        }

        private void Disconnect(Connection connection)
        {
            lock (ThreadLocker.Lock)
            {
                groups.RemoveClient(client);
            }
            connection.Close();
            Console.WriteLine("Rozłączono klienta, grupa: \"{0}\", IP: \"{1}:{2}\"", client.GroupID, connection.IP, connection.Port);
        }
    }
}
