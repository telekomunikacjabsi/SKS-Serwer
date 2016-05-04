using System;
using System.IO;
using System.Xml;

namespace SKS_Serwer
{
    public class Settings
    {
        public int Port { get; private set; }
        public string DomainsListPath { get; private set; }
        public string ProcessesListPath { get; private set; }
        public string AllowedIPs { get; private set; }

        public Settings()
        {
            Port = -1;
            if (!File.Exists("settings.xml"))
            {
                Console.WriteLine("Brak pliku 'settings.xml'. Wczytuję ustawienia domyślne.");
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                using (XmlWriter writer = XmlWriter.Create("settings.xml", settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Settings");
                    writer.WriteElementString("Port", "5000");
                    writer.WriteElementString("DomainsListPath", "domains.txt");
                    writer.WriteElementString("ProcessesListPath", "processes.txt");
                    writer.WriteElementString("AllowedIPsRegex", ".*");
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            using (XmlReader reader = new XmlTextReader("settings.xml"))
            {
                string currentElement = String.Empty;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        currentElement = reader.Name;
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (currentElement == "Port")
                        {
                            try
                            {
                                Port = Int32.Parse(reader.Value);
                            }
                            catch
                            {
                                Console.WriteLine("Wartość {0} nie jest wartością odpowiednią dla ustawienia \"Port\"", reader.Value);
                            }
                        }
                        else if (currentElement == "DomainsListPath")
                            DomainsListPath = reader.Value;
                        else if (currentElement == "ProcessesListPath")
                            ProcessesListPath = reader.Value;
                        else if (currentElement == "AllowedIPsRegex")
                        {
                            if (!RegexValidator.IsValidRegex(reader.Value))
                            {
                                Console.WriteLine("Wartość \"{0}\" zdefiniowana dla dozwolonych adresów IP nie jest poprawnym wyrażeniem regularnym!", reader.Value);
                                AllowedIPs = ".*";
                            }
                            else
                                AllowedIPs = reader.Value;
                        }
                    }
                }
            }
            if (Port <=0 || Port > 65535)
            {
                if (Port <= 0)
                {
                    Port = 1;
                    Console.WriteLine("Wartość 'Port' wykraczała poza dozwolony zakres. Ustawiono wartość Port = {0}", Port);
                }
                if (Port > 65535)
                {
                    Port = 65535;
                    Console.WriteLine("Wartość 'Port' wykraczała poza dozwolony zakres. Ustawiono wartość Port = {0}", Port);
                }
            }
        }
    }
}
