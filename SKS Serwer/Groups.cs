using System.Collections.Generic;
using System.Net;

namespace SKS_Serwer
{
    class Groups
    {
        private List<Group> groups;

        public Groups()
        {
            groups = new List<Group>();
        }

        public void AddClient(EndPoint ipEndPoint, string groupID)
        {
            if (!groups.Exists(group => group.ID == groupID))
            {
                groups.Add(new Group(groupID));
            }
            int index = groups.FindIndex(group => group.ID == groupID);
            if (index != -1)
                groups[index].Clients.Add(ipEndPoint);
        }

        public void RemoveClient(EndPoint ipEndPoint, string groupID)
        {
            int index = groups.FindIndex(group => group.ID == groupID);
            if (index != -1)
                groups[index].Clients.Remove(ipEndPoint);
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
