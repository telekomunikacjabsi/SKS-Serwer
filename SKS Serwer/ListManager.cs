using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace SKS_Serwer
{
    public class ListManager
    {
        private string[] disallowedDomains;
        private string[] disallowedProcesses;
        private byte[] domainsListChecksum;
        private byte[] processesListChecksum;
        private Settings settings;

        public ListManager(Settings settings)
        {
            this.settings = settings;
            if (File.Exists(settings.DomainsListPath))
            {
                disallowedDomains = File.ReadAllLines(settings.DomainsListPath, Encoding.UTF8).Where(value => RegexValidator.IsValidRegex(value)).ToArray(); // wybieramy tylko reguły które są poprawnymi wyrażeniami regularnymi
                domainsListChecksum = CalculateMD5(disallowedDomains);
            }
            else
            {
                Console.WriteLine("Lista zabronionych {0} jest pusta!", "domen");
                disallowedDomains = new string[] { String.Empty };
                domainsListChecksum = new byte[] { 0 }; // inicjalizujemy hash jako "pusty"
            }
            if (File.Exists(settings.ProcessesListPath))
            {
                disallowedProcesses = File.ReadAllLines(settings.ProcessesListPath, Encoding.UTF8).Where(value => RegexValidator.IsValidRegex(value)).ToArray();
                processesListChecksum = CalculateMD5(disallowedProcesses);
            }
            else
            {
                Console.WriteLine("Lista zabronionych {0} jest pusta!", "procesów");
                disallowedProcesses = new string[] { String.Empty };
                processesListChecksum = new byte[] { 0 };
            }
        }

        private byte[] CalculateMD5(string[] lines)
        {
            string sum = String.Join(String.Empty, lines);
            var checkSum = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(sum.ToString());
            return checkSum.ComputeHash(bytes);
        }

        public bool VerifyList(ListID listID, string checksum)
        {
            return VerifyList(listID, Encoding.UTF8.GetBytes(checksum));
        }

        private bool VerifyList(ListID listID, byte[] checksum)
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
