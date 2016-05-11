using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SKS_Serwer
{
    public class Groups
    {
        private List<Group> groups;
        private List<GroupCredentials> knownPasswords;

        public Groups()
        {
            groups = new List<Group>();
            knownPasswords = new List<GroupCredentials>();
            LoadGroupsLog();
        }

        public bool IsAdminConnected(string groupID)
        {
            int index = GetGroupIndex(groupID);
            if (index != -1)
            {
                return groups[index].ConnectedAdmin != null;
            }
            return false;
        }

        public void AssociateAdmin(AdminWorker admin, string groupID)
        {
            if (!GroupExists(groupID))
            {
                CreateGroup(groupID);
            }
            int index = GetGroupIndex(groupID);
            if (index != -1)
            {
                groups[index].ConnectedAdmin = admin;
            }
        }

        public void DisassociateAdmin(string groupID)
        {
            int index = GetGroupIndex(groupID);
            if (index != -1)
            {
                groups[index].ConnectedAdmin = null;
                TryRemoveGroup(index);
            }
        }

        private bool GroupExists(string groupID)
        {
            return groups.Exists(group => group.ID == groupID);
        }

        private void CreateGroup(string groupID) // wywołanie przez admina
        {
            Group newGroup = new Group(groupID);
            groups.Add(newGroup);
        }

        private void CreateGroup(string groupID, string groupPassword) // wywołanie przez klienta
        {
            Group newGroup = new Group(groupID, groupPassword);
            groups.Add(newGroup);
            UpdateLastKnownPassword(newGroup.ID, newGroup.Password);
        }

        private void TryRemoveGroup(int index)
        {
            int clientsCount = groups[index].GetClientsCount();
            if (clientsCount == 0 && !IsAdminConnected(groups[index].ID))
            {
                groups.RemoveAt(index);
            }
        }

        public void AddClient(Client client)
        {
            if (!GroupExists(client.GroupID))
            {
                CreateGroup(client.GroupID, client.GroupPassword);
            }
            int index = groups.FindIndex(group => group.ID == client.GroupID);
            if (index != -1)
                groups[index].AddClient(client);
        }

        public void RemoveClient(Client client)
        {
            int index = GetGroupIndex(client.GroupID);
            if (index != -1)
            {
                groups[index].RemoveClient(client);
                TryRemoveGroup(index);
            }
        }

        private int GetGroupIndex(string groupID)
        {
            return groups.FindIndex(group => group.ID == groupID);
        }

        public List<Client> GetClients(string groupID)
        {
            int index = GetGroupIndex(groupID);
            if (index != -1)
                return groups[index].GetClients();
            return new List<Client>();
        }

        public bool VerifyPassword(string groupID, string groupPassword)
        {
            int index = GetGroupIndex(groupID);
            if (index != -1)
                return groups[index].Password == groupPassword;
            else
                return VerifyLastKnownPassword(groupID, groupPassword);
        }

        private void LoadGroupsLog()
        {
            string groupsLogFilePath = "groupsLog.txt";
            if (File.Exists(groupsLogFilePath))
            {
                string fileContent = File.ReadAllText(groupsLogFilePath, Encoding.UTF8);
                string[] array = fileContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (array != null && array.Length > 0)
                {
                    foreach (string item in array)
                    {
                        try
                        {
                            GroupCredentials credentials = new GroupCredentials(item);
                            knownPasswords.Add(credentials);
                        }
                        catch { }
                    }
                }
            }
        }

        private void SaveGroupsLog()
        {
            string groupsLogFilePath = "groupsLog.txt";
            StringBuilder fileContent = new StringBuilder();
            foreach (GroupCredentials item in knownPasswords)
            {
                fileContent.AppendLine(item.ToString());
            }
            string content = fileContent.ToString().TrimEnd();
            File.WriteAllText(groupsLogFilePath, content, Encoding.UTF8);
        }

        private bool VerifyLastKnownPassword(string groupID, string groupPassword)
        {
            int index = knownPasswords.FindIndex(credentials => credentials.ID == groupID && credentials.Password == groupPassword);
            return index != -1;
        }

        private void UpdateLastKnownPassword(string groupID, string groupPassword)
        {
            bool alreadyExists = VerifyLastKnownPassword(groupID, groupPassword);
            if (!alreadyExists)
            {
                knownPasswords.RemoveAll(credentials => credentials.ID == groupID);
                knownPasswords.Add(new GroupCredentials(groupID, groupPassword));
                SaveGroupsLog();
            }
        }
    }
}
