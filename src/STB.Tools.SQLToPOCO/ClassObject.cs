using System.Collections.Generic;

namespace STB.Tools.SQLToPOCO
{
    public class ClassObject
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<PropertyObject> Properties { get; set; } = new List<PropertyObject>();
        public string Comment { get; set; }
    }
}