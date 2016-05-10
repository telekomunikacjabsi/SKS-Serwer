using System;
using System.Collections.Generic;
using System.IO;

namespace SKS_Serwer
{
    public class AdminWorker : Worker
    {
        Connection connection;
        Groups groups;
        ListManager listManager;
        string groupID;

        public AdminWorker(Connection connection, Groups groups, ListManager listManager)
        {
            this.connection = connection;
            this.groups = groups;
            this.listManager = listManager;
            groupID = String.Empty;
        }

        public void DoWork(string groupID)
        {
            this.groupID = groupID;
            connection.SendMessage(CommandSet.Auth, "SUCCESS");
            Console.WriteLine("Połączono administratora, grupa: \"{0}\", IP: \"{1}:{2}\"", groupID, connection.IP, connection.Port);
            while (true)
            {
                try
                {
                    connection.ReceiveMessage();
                    if (connection.Command == CommandSet.Disconnect)
                    {
                        Disconnect();
                        return;
                    }
                    else if (connection.Command == CommandSet.VerifyList)
                        listManager.VerifyList(connection);
                    else if (connection.Command == CommandSet.List)
                        UpdateList();
                    else if (connection.Command == CommandSet.Users)
                        SendUsersList();
                    else
                    {
                        Disconnect();
                        return;
                    }
                }
                catch (IOException)
                {
                    Disconnect();
                    return;
                }
            }
        }

        private void SendUsersList()
        {
            string listString = String.Empty;
            List<Client> clients = groups.GetClients(groupID);
            for(int i = 0; i < clients.Count; i++)
            {
                Client client = clients[i];
                listString += client.IP + ":" + client.AdminPort + ";";
            }
            listString = listString.TrimEnd(';');
            connection.SendMessage(CommandSet.Users, listString);
        }

        private void UpdateList()
        {
            ListID listID = listManager.GetListID(connection[0]);
            string listContent = connection[1];
            lock (ThreadLocker.Lock)
            {
                listManager.SetListFromString(listID, listContent);
            }
        }

        private void Disconnect()
        {
            connection.Close();
            Console.WriteLine("Rozłączono administratora, grupa: \"{0}\", IP: \"{1}:{2}\"", groupID, connection.IP, connection.Port);
        }
    }
}
