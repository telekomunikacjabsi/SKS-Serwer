using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SKS_Serwer
{
    public class ListManager
    {
        private string[] disallowedDomains;
        private string[] disallowedProcesses;
        private string domainsListChecksum;
        private string processesListChecksum;
        private Settings settings;

        public ListManager(Settings settings)
        {
            this.settings = settings;
            if (File.Exists(settings.DomainsListPath))
            {
                disallowedDomains = File.ReadAllLines(settings.DomainsListPath, Encoding.UTF8).Where(value => RegexValidator.IsValidRegex(value)).ToArray(); // wybieramy tylko reguły które są poprawnymi wyrażeniami regularnymi
                domainsListChecksum = CalculateMD5Hash(disallowedDomains);
            }
            else
            {
                Console.WriteLine("Lista zabronionych {0} jest pusta!", "domen");
                disallowedDomains = new string[] { String.Empty };
                domainsListChecksum = "0";
            }
            if (File.Exists(settings.ProcessesListPath))
            {
                disallowedProcesses = File.ReadAllLines(settings.ProcessesListPath, Encoding.UTF8).Where(value => RegexValidator.IsValidRegex(value)).ToArray();
                processesListChecksum = CalculateMD5Hash(disallowedProcesses);
            }
            else
            {
                Console.WriteLine("Lista zabronionych {0} jest pusta!", "procesów");
                disallowedProcesses = new string[] { String.Empty };
                processesListChecksum = "0";
            }
        }

        private string CalculateMD5Hash(string[] array)
        {
            string input = StringArrayToString(array);
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private string StringArrayToString(string[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string value in array)
            {
                builder.Append(value);
                builder.Append('.');
            }
            return builder.ToString();
        }

        private bool VerifyList(ListID listID, string checksum)
        {
            if (listID == ListID.Domains)
                return checksum == domainsListChecksum;
            else if (listID == ListID.Processes)
                return checksum == processesListChecksum;
            return false;
        }

        public void VerifyList(Connection connection)
        {
            ListID listID = GetListID(connection[0]);
            if (listID != ListID.Unknown)
            {
                bool result;
                string listString = String.Empty;
                lock (ThreadLocker.Lock)
                {
                    string checksum = connection[1];
                    result = VerifyList(listID, checksum);
                    if (!result) // jeśli listy się nie zgadzają, tzn. klient ma nieaktualną listę
                        listString = GetListString(listID);
                }
                if (result)
                    connection.SendMessage(CommandSet.OK);
                else
                    connection.SendMessage(CommandSet.List, listID.ToString(), listString); // w przypadku posiadania złej listy przez klienta lub admina jest ona automatycznie odsyłana
            }
            else
                connection.SendMessage(CommandSet.OK);
        }

        private bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
            {
                Console.WriteLine("LENGTH");
                return false;
            }
            for (int i = 0; i < a1.Length; i++)
            {
                int a = a1[i];
                int b = a2[i];
                Console.WriteLine(a + " - " + b + " - " + (a == b));
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }

        public string GetListString(ListID listID)
        {
            string[] lines = null;
            if (listID == ListID.Domains)
            {
                if (disallowedDomains == null)
                    return String.Empty;
                lines = disallowedDomains;
            }
            else if (listID == ListID.Processes)
            {
                if (disallowedProcesses == null)
                    return String.Empty;
                lines = disallowedProcesses;
            }
            return String.Join(Environment.NewLine, lines);
        }

        public void SetListFromString(ListID listID, string listString)
        {
            listString = listString.Trim();
            if (listID == ListID.Domains)
            {
                disallowedDomains = listString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                File.WriteAllText(settings.DomainsListPath, listString, Encoding.UTF8);
            }
            else if (listID == ListID.Processes)
            {
                disallowedProcesses = listString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                File.WriteAllText(settings.ProcessesListPath, listString, Encoding.UTF8);
            }
        }

        public ListID GetListID(string s)
        {
            int id;
            bool success = Int32.TryParse(s, out id);
            if (success)
            {
                if (Enum.IsDefined(typeof(ListID), id))
                    return (ListID)id;
                else return ListID.Unknown;
            }
            else
                return ListID.Unknown;
        }
    }

    public enum ListID
    { Domains, Processes, Unknown };
}
