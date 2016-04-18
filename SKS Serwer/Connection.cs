using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SKS_Serwer
{
    public class Connection
    {
        public Command Command { get; private set; }
        public string IP { get; private set; }
        public string Port { get; private set; }
        public string GroupID { get; private set; }
        string[] parameters;
        TcpClient client;
        NetworkStream stream;

        public Connection(TcpClient client)
        {
            this.client = client;
            stream = this.client.GetStream();
            IP = GetIP();
            Port = GetPort();
        }

        public void ReceiveMessage()
        {
            Command = new Command(String.Empty);
            int i;
            byte[] bytes = new byte[256];
            parameters = null;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string msg = Encoding.UTF8.GetString(bytes, 0, i);
                string[] args = Regex.Split(msg, ";"); // automatyczny podział komunikatu na argumenty
                if (args.Length > 1)
                {
                    parameters = new string[args.Length - 1];
                    for (int j = 1; j < args.Length; j++)
                        parameters[j - 1] = args[j];
                }
                if (args.Length > 0)
                {
                    if (parameters != null && parameters.Length > 0)
                        Command = new Command(args[0].Trim(), parameters.Length);
                    else
                        Command = new Command(args[0].Trim());
                }
                return;
            }
        }

        public void SendMessage(Command command, params string[] parameters)
        {
            if (parameters.Length < command.ParametersCount) // jeśli liczba parametrów nie spełnia wymogów komendy
            {
                Console.WriteLine("Komenda '{0}' nie spełnia wymaganych warunków. Wysyłanie nie powiodło się.", command.Text);
            }
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameters[i];
            byte[] bytes = Encoding.UTF8.GetBytes(String.Join(";", command.Text, String.Join(";", parameters)));
            stream.Write(bytes, 0, bytes.Length);
        }

        public void Reject()
        {
            SendMessage(CommandSet.AuthFail);
            Close();
        }

        public void Close()
        {
            stream.Close();
            client.Close();
        }

        public EndPoint GetEndPoint()
        {
            return client.Client.RemoteEndPoint;
        }

        public void SetGroupID(string groupID)
        {
            GroupID = groupID;
        }

        public string this[int index]
        {
            get
            {
                return parameters[index];
            }
        }

        public static string GetIP(TcpListener listener)
        {
            return ((IPEndPoint)listener.LocalEndpoint).Address.ToString();
        }

        public static string GetPort(TcpListener listener)
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port.ToString();
        }

        public string GetIP()
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        }

        public string GetPort()
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
        }
    }
}
