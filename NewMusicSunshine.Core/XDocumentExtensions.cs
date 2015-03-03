using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMusicSunshine.Core
{
    class XDocumentExtensions
    {
        public static string ValueOrEmpty(this XElement xelement)
        {
            return xelement != null ? xelement.Value : string.Empty;
        }

        public static XElement ElementOrEmpty(this XDocument xDocument, string name)
        {
            return xDocument.Element(name) ?? new XElement(name);
        }

        public static XElement ElementOrEmpty(this XElement xelement, string name)
        {
            return xelement.Element(name) ?? new XElement(name);
        }

        public static XContainer ContainerOrEmpty(this XContainer xelement, string name)
        {
            return xelement.Element(name) ?? new XElement(name);
        }

        public static XAttribute AttributeOrEmpty(this XElement xelement, string name)
        {
            return xelement.Attribute(name) ?? new XAttribute(name, "");
        }
    }
}
