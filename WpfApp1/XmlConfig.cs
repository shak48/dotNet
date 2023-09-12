using System;
using System.IO;
using System.Xml.Serialization;

public class XmlConfig
{
    public int BaudRate { get; set; }
    public string FilePath { get; set; }

    public static XmlConfig LoadConfig(string filePath)
    {
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlConfig));
                return (XmlConfig)serializer.Deserialize(fs);
            }
        }
        catch (Exception ex)
        {
            // Handle any configuration loading errors here
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return null;
        }
    }
}
