using System;
using System.IO;

namespace SKS_Serwer
{
    public class AdminWorker : Worker
    {
        Connection connection;
        ListManager listManager;

        public AdminWorker(Connection connection, ListManager listManager)
        {
            this.connection = connection;
            this.listManager = listManager;
        }

        public void DoWork()
        {
            connection.SendMessage(CommandSet.Auth, "SUCCESS");
            Console.WriteLine("Połączono administratora, grupa: \"{0}\", IP: \"{1}:{2}\"", connection.GroupID, connection.IP, connection.Port);
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

        private void UpdateList()
        {
            ListID listID = listManager.GetListID(connection[0]);
            string listContent = connection[1].Trim();
            listManager.SetListFromString(listID, listContent);
        }

        private void Disconnect()
        {
            connection.Close();
            Console.WriteLine("Rozłączono administratora, grupa: \"{0}\", IP: \"{1}:{2}\"", connection.GroupID, connection.IP, connection.Port);
        }
    }
}
