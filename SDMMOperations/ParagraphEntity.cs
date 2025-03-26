using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDMMOperations
{
    public class ParagraphEntity
    {
        public string name;
        public OpenXmlElement element;
        public string id;

        public ParagraphEntity(string name, OpenXmlElement element, string id)
        {
            this.name = name;
            this.element = element;
            this.id = id;
        }

    }
}
