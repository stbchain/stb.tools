﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace STB.Tools.SQLToPOCO
{
    public class ClassObject
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<PropertyObject> Properties { get; set; } = new List<PropertyObject>();
        public string Comment { get; set; }
    }

    public class PropertyObject
    {
        public string RawType { get; set; }

        public string PropertyType
        {
            get
            {
                var type = RawType.Split("(").FirstOrDefault();
                var pType = "";
                switch (type)
                {
                    case "CHAR":
                    case "VARCHAR":
                    case "TEXT":
                    case "LONGTEXT":
                        return "string";
                    case "timestamp":
                    case "TIMESTAMP":
                        pType = "DateTime";
                        break;
                    case "INT":
                    case "INTEGER":
                        pType = "int";
                        break;
                    case "TINYINT":
                        pType = "byte";
                        break;
                }

                if (string.IsNullOrWhiteSpace(pType))
                    return $"error:{RawType}";
                if (IsNotNull)
                    pType += "?";
                return pType;
            }
        }

        public string PropertyName { get; set; }
        public string DefaultValue { get; set; } = null;
        public bool IsPrimary { get; set; }
        public bool IsNotNull { get; set; }
        public string Comment { get; set; }
    }
}