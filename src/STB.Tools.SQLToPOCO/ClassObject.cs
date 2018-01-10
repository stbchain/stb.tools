using System;
using System.Collections.Generic;

namespace STB.Tools.SQLToPOCO
{
    public class ClassObject
    {
        public string Name { get; set; }
        public List<PropertyObject> Properties { get; set; } = new List<PropertyObject>();
        public string Comment { get; set; }
    }

    public class PropertyObject
    {
        public string RawType { get; set; }

        public string PropertyType
        {
            get { return RawType+"1"; }
        }

        public string PropertyName { get; set; }
        public string DefaultValue { get; set; } = null;
        public bool IsPrimary { get; set; }
        public bool IsNotNull { get; set; }
        public string Comment { get; set; }
    }
}