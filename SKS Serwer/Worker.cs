using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace SKS_Serwer
{
    class Worker
    {
        ListManager listManager; // udostępnia dostęp do list zabronionych domen i procesów
        Groups groups; // grupuje klientów według identyfikatora grupy
        Settings settings;
        object threadLocker;

        public Worker()
        {
            settings = new Settings();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, settings.Port);
            TcpListener listener = new TcpListener(ip);
            try
            {
                listener.Start();
            }
            catch (SocketException)
            {
                throw new Exception("Port " + settings.Port + " jest już zajęty. Uruchamianie serwera nie powiodło się.");
            }
            listManager = new ListManager(settings);
            groups = new Groups();
            threadLocker = new object();
            new Thread(() => AcceptClients(listener)).Start();
        }

        private void AcceptClients(TcpListener listener)
        {
            Console.WriteLine("Serwer uruchomiono pomyślnie!");
            Console.WriteLine("Adres serwera: {0}:{1}", Connection.GetIP(listener), Connection.GetPort(listener));
            while (true)
            {
                Connection clientConnection = new Connection(listener.AcceptTcpClient());
                new Thread(() => FinalizeConnection(clientConnection)).Start();
            }
        }

        private void FinalizeConnection(Connection connection)
        {
            if (!Regex.IsMatch(connection.IP, settings.AllowedIPs)) // weryfikacja czy połączenie nadchodzi z dozwolonej puli adresów IP
            {
                connection.Reject();
                return;
            }
            connection.ReceiveMessage();
            if (connection.Command == CommandSet.Connect)
            {
                string type = connection[0];
                string groupID = connection[1].Trim();
                if (String.IsNullOrEmpty(groupID)) // jeśli id grupy jest puste zamykamy połączenie
                {
                    connection.Reject();
                    return;
                }
                connection.SetGroupID(groupID);
                if (type == "CLIENT")
                {
                    connection.SendMessage(CommandSet.Auth, "SUCCESS");
                    WorkOnClient(connection);
                }
                else if (type == "ADMIN")
                {
                    connection.SendMessage(CommandSet.Auth, "FAIL");
                    WorkOnAdmin(connection);
                }
                else
                    connection.Reject();
            }
            else
                connection.Reject();
        }

        private void WorkOnClient(Connection connection)
        {
            Console.WriteLine("Połączono klienta, grupa: \"{0}\", IP: \"{1}:{2}\"", connection.GroupID, connection.IP, connection.Port);
            lock (threadLocker)
            {
                groups.AddClient(connection);
            }
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
                        VerifyList(connection);
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

        private void SetPort(Connection connection) // ustawia port na którym klient chce aby admin się z nim połączył
        {
            int port = 9000;
            bool parseResult = Int32.TryParse(connection[0], out port);
            if (parseResult)
                connection.SecondPort = connection[0];
        }

        private void Disconnect(Connection connection)
        {
            Console.WriteLine("Rozłączono klienta, grupa: \"{0}\", IP: \"{1}:{2}\"", connection.GroupID, connection.IP, connection.Port);
            lock (threadLocker)
            {
                groups.RemoveClient(connection);
            }
            connection.Close();
        }

        private void VerifyList(Connection connection)
        {
            int listID = listManager.GetListID(connection[0]);
            if (listID != -1)
            {
                bool result;
                string listString = String.Empty;
                lock (threadLocker)
                {
                    result = listManager.VerifyList(listID, connection[1]);
                    if (!result) // jeśli listy się nie zgadzają, tzn. klient ma nieaktualną listę
                        listString = listManager.GetListString((ListID)listID);
                }
                if (result)
                    connection.SendMessage(CommandSet.OK);
                else
                    connection.SendMessage(CommandSet.List, listID.ToString(), listString); // w przypadku posiadania złej listy przez klienta jest ona automatycznie odsyłana
            }
        }

        private void WorkOnAdmin(Connection connection)
        {
            while (true)
            {

            }
        }

        private void UnknownCommand(Connection connection, string commandText)
        {
            Console.WriteLine("Nieznana komenda '{0}' od klienta, grupa: \"{1}\", IP: \"{2}:{3}\"", commandText, connection.GroupID, connection.IP, connection.Port);
        }
    }
}
