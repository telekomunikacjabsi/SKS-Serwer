using System.Collections.Generic;
using System.Net;

namespace SKS_Serwer
{
    public class Groups
    {
        private List<Group> groups;

        public Groups()
        {
            groups = new List<Group>();
        }

        public void AddClient(Connection connection)
        {
            if (!groups.Exists(group => group.ID == connection.GroupID))
            {
                groups.Add(new Group(connection.GroupID));
            }
            int index = groups.FindIndex(group => group.ID == connection.GroupID);
            if (index != -1)
                groups[index].Clients.Add(connection.GetEndPoint());
        }

        public void RemoveClient(Connection connection)
        {
            int index = groups.FindIndex(group => group.ID == connection.GroupID);
            if (index != -1)
                groups[index].Clients.Remove(connection.GetEndPoint());
        }

        public List<EndPoint> GetClients(string groupID)
        {
            int index = groups.FindIndex(group => group.ID == groupID);
            if (index != -1)
                return groups[index].Clients;
            return new List<EndPoint>();
        }

        private class Group
        {
            public string ID { get; private set; }
            public List<EndPoint> Clients { get; private set; }

            public Group(string ID)
            {
                this.ID = ID;
                Clients = new List<EndPoint>();
            }
        }
    }
}
