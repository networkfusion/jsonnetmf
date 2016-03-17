﻿using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace PervasiveDigital.Json
{
    public class JObject : JToken
    {
        private readonly Hashtable _members = new Hashtable();

        public JProperty this[string name]
        {
            get { return (JProperty)_members[name.ToLower()]; }
            set
            {
                if (name.ToLower()!=value.Name.ToLower())
                    throw new ArgumentException("index value must match property name");
                _members.Add(value.Name.ToLower(), value);
            }
        }

        public void Add(string name, JToken value)
        {
            _members.Add(name.ToLower(), new JProperty(name, value));
        }

        public static JObject Serialize(Type type, object oSource)
        {
            var result = new JObject();

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                switch (f.MemberType)
                {
                    case MemberTypes.Field:
                    case MemberTypes.Property:
                        if (f.FieldType.IsValueType || f.FieldType == typeof (string))
                            result._members.Add(f.Name.ToLower(),
                                new JProperty(f.Name, JValue.Serialize(f.FieldType, f.GetValue(oSource))));
                        else
                        {
                            if (f.FieldType.IsArray)
                                result._members.Add(f.Name.ToLower(), new JProperty(f.Name, JArray.Serialize(f.FieldType, f.GetValue(oSource))));
                            else
                                result._members.Add(f.Name.ToLower(), new JProperty(f.Name, JObject.Serialize(f.FieldType, f.GetValue(oSource))));
                        }
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public override string ToString()
        {
            EnterSerialization();
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(Indent(true) + "{");
                bool first = true;
                foreach (var member in _members.Values)
                {
                    if (!first)
                        sb.AppendLine(",");
                    first = false;
                    sb.Append(Indent() + ((JProperty)member).ToString());
                }
                sb.AppendLine();
                Outdent();
                sb.AppendLine(Indent() + "}");
                return sb.ToString();
            }
            finally
            {
                ExitSerialization();
            }
        }
    }
}
