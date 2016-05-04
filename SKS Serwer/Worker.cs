using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace SKS_Serwer
{
    public class Worker
    {
        private Groups groups; // grupuje klientów według identyfikatora grupy
        private ListManager listManager; // udostępnia dostęp do list zabronionych domen i procesów
        private Settings settings;

        public void Start()
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
                string groupPassword = connection[2].Trim();
                Console.WriteLine(groupPassword);
                if (String.IsNullOrEmpty(groupID)) // jeśli id grupy jest puste zamykamy połączenie
                {
                    connection.Reject();
                    return;
                }
                if (type == "CLIENT")
                    new ClientWorker(connection, groups, listManager).DoWork(groupID, groupPassword); // dalsza obsługa połączenia z klientem
                else if (type == "ADMIN")
                {
                    lock (ThreadLocker.Lock)
                    {
                        if (!groups.VerifyPassword(groupID, groupPassword)) // jeśli hasło podane przez administratora nie jest prawidłowe
                        {
                            connection.Reject();
                            return;
                        }
                    }
                    new AdminWorker(connection, groups, listManager).DoWork(groupID); // dalsza obsługa połączenia z administratorem

                }
                else
                    connection.Reject();
            }
            else
                connection.Reject();
        }
    }
}
