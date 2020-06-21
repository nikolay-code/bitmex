using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Bitmex.NET.MainApp
{
    #region -- Configuration Class --
    /// <summary>
    /// This Configuration class is basically just a set of 
    /// properties with a couple of static methods to manage
    /// the serialization to and deserialization from a
    /// simple XML file.
    /// </summary>

    [XmlType("Account")]
    public class Account
    {
        //[XmlAttribute("APIKey")]
        public string APIKey;

        //[XmlAttribute("APISecret")]
        public string APISecret;

        public string Type;
        //[XmlAttribute("IsReal")]
        public string IsAPIupdated;

        public string IsReal;

        //[XmlAttribute("Size")]
        public string Size;

        //[XmlAttribute("Leverage")]
        public string Leverage;
    }


    [Serializable]
    public class Configuration
    {
        [XmlArray("Accounts")]
        public Account[] Items;

        public Configuration()
        {
        }
        
        public static void Serialize(string file, Configuration c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }
        public static Configuration Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(Configuration));
            StreamReader reader = File.OpenText(file);
            Configuration c = (Configuration)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
      
    }
    #endregion

}
