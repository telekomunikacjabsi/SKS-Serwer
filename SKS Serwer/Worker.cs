using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SKS_Serwer
{
    class Worker
    {
        ListManager listManager;
        Groups groups;
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
            Console.WriteLine("Adres serwera: {0}:{1}", GetIP(listener), GetPort(listener));
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                new Thread(() => FinalizeConnection(client)).Start();
            }
        }

        public string GetIP(TcpListener listener)
        {
            return ((IPEndPoint)listener.LocalEndpoint).Address.ToString();
        }

        public string GetPort(TcpListener listener)
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port.ToString();
        }

        public string GetIP(TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        }

        public string GetPort(TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
        }

        private void FinalizeConnection(TcpClient connection)
        {
            NetworkStream stream = connection.GetStream();
            string sourceIP = GetIP(connection);
            if (!Regex.IsMatch(sourceIP, settings.AllowedIPs)) // weryfikacja czy połączenie nadchodzi z dozwolonej puli adresów IP
            {
                WriteMessage(stream, "AUTH", "FAIL");
                connection.Close();
                return;
            }
            string[] message = ReceiveMessage(stream);
            if (message.Length == 3) // w komunikacie oczekujemy dokładnie trzech argumentów
            {
                if (message[0] == "CONNECT")
                {
                    message[2] = message[2].Trim(); // w tym parametrze przesyłany jest identyfikator grupy
                    if (String.IsNullOrEmpty(message[2])) // jeśli id grupy jest puste zamykamy połączenie
                    {
                        WriteMessage(stream, "AUTH", "FAIL");
                        connection.Close();
                        return;
                    }
                    if (message[1] == "CLIENT")
                        WorkOnClient(connection, message[2]);
                    else if (message[1] == "ADMIN")
                        WorkOnAdmin(connection, message[2]);
                    else
                    {
                        WriteMessage(stream, "AUTH", "FAIL");
                        connection.Close();
                    }
                }
            }
            connection.Close();
        }

        private void WorkOnClient(TcpClient connection, string groupID)
        {
            NetworkStream stream = connection.GetStream();
            WriteMessage(stream, "AUTH", "SUCCESS");
            Console.WriteLine("Połączenie klienta, grupa: \"{0}\", IP: \"{1}:{2}\"", groupID, GetIP(connection), GetPort(connection));
            while (true)
            {

            }
        }

        private void WorkOnAdmin(TcpClient connection, string groupID)
        {
            NetworkStream stream = connection.GetStream();
            WriteMessage(stream, "AUTH", "SUCCESS");
            while (true)
            {

            }
        }

        private string[] ReceiveMessage(NetworkStream stream)
        {
            int i;
            byte[] bytes = new byte[256];
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string msg = Encoding.ASCII.GetString(bytes, 0, i);
                return Regex.Split(msg, ";"); // automatyczny podział komunikatu na argumenty
            }
            return new string[] { String.Empty };
        }

        private void WriteMessage(NetworkStream stream, params string[] message)
        {
            WriteMessage(stream, String.Join(";", message));
        }

        private void WriteMessage(NetworkStream stream, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
