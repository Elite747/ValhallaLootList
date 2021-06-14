// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Xml.Serialization;

namespace ValhallaLootList.Client.Data.Containers
{
    [XmlRoot("Blob")]
    public class Blob
    {

        private string _name;
        [XmlElement("Name")]
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                simple = value.Split('.')[0];
            }
        }

        public string simple { get; set; }

        [XmlElement("Url")]
        public string url { get; set; }

        public Blob() { }

        public Blob(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
}
