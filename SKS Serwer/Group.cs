using System.Collections.Generic;

namespace SKS_Serwer
{
    public class Group
    {
        public string ID { get; }
        public string Password { get; private set; }
        public AdminWorker ConnectedAdmin { get; set; }
        private List<Client> clients;

        public Group(string ID)
        {
            this.ID = ID.ToLower();
            Init();
            Password = null;
        }

        public Group(string ID, string password)
        {
            this.ID = ID.ToLower();
            Password = password;
            Init();
        }

        private void Init()
        {
            clients = new List<Client>();
            ConnectedAdmin = null;
        }

        public void AddClient(Client client)
        {
            if (Password == null)
                Password = client.GroupPassword;
            clients.Add(client);
            if (ConnectedAdmin != null)
                ConnectedAdmin.NotifyNewClient(client);
        }

        public void RemoveClient(Client client)
        {
            clients.Remove(client);
        }

        public int GetClientsCount()
        {
            return clients.Count;
        }

        public List<Client> GetClients()
        {
            return clients;
        }
    }
}
