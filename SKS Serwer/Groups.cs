using System.Collections.Generic;

namespace SKS_Serwer
{
    public class Groups
    {
        private List<Group> groups;

        public Groups()
        {
            groups = new List<Group>();
        }

        public void AddClient(Client client, string groupPassword)
        {
            if (!groups.Exists(group => group.ID == client.GroupID))
            {
                groups.Add(new Group(client.GroupID, groupPassword));
            }
            int index = groups.FindIndex(group => group.ID == client.GroupID);
            if (index != -1)
                groups[index].Clients.Add(client);
        }

        public void RemoveClient(Client client)
        {
            int index = groups.FindIndex(group => group.ID == client.GroupID);
            if (index != -1)
            {
                groups[index].Clients.Remove(client);
                if (groups[index].Clients.Count == 0)
                    groups.RemoveAt(index);
            }
        }

        public List<Client> GetClients(string groupID)
        {
            int index = groups.FindIndex(group => group.ID == groupID);
            if (index != -1)
                return groups[index].Clients;
            return new List<Client>();
        }

        public bool VerifyPassword(string groupID, string groupPassword)
        {
            int index = groups.FindIndex(group => group.ID == groupID);
            if (index != -1)
                return groups[index].Password == groupPassword;
            else
                return false;
        }

        private class Group
        {
            public string ID { get; }
            public string Password { get;  }
            public List<Client> Clients { get; }

            public Group(string ID, string password)
            {
                this.ID = ID;
                Password = password;
                Clients = new List<Client>();
            }
        }
    }
}
