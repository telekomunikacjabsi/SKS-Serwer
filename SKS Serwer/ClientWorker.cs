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

        public ClientWorker(Connection connection, Groups groups, ListManager listManager)
        {
            this.connection = connection;
            this.groups = groups;
            this.listManager = listManager;
        }

        public void DoWork(string groupID, string groupPassword)
        {
            client = new Client(groupID, connection.IP);
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
                    else if (connection.Command == CommandSet.Port)
                        SetPort(connection);
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

        private void SetPort(Connection connection) // ustawia port na którym klient chce aby admin się z nim połączył
        {
            int port = 9000;
            bool parseResult = Int32.TryParse(connection[0], out port);
            if (parseResult)
                client.AdminPort = connection[0];
        }
    }
}
