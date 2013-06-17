using System;
using System.Linq;
using System.Xml;

namespace Assets.Scripts.Shared
{
    public static class XmlHelperExtensions
    {
        //Required (since no default value was provided).
        public static string ReadValue(this XmlNode node, string attributeName)
        {
            if (node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName) == null)
                throw new Exception(
                    string.Format("The required attribute '{0}' was missing on element '{1}' on line {2}",
                                  attributeName,
                                  node.Name,
                                  (node as IXmlLineInfo).HasLineInfo()
                                      ? (node as IXmlLineInfo).LineNumber.ToString()
                                      : "UNKNOWN"));

            return ReadValue(node, attributeName, String.Empty);
        }

        //Optional
        public static string ReadValue(this XmlNode node, string attributeName, string defaultValue)
        {
            if (node == null)
                return defaultValue;

            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            return attr.Value;
        }

        public static int ReadIntValue(this XmlNode node, string attributeName, int defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            int value;

            try
            {
                value = int.Parse(attr.Value);
            }
            catch (Exception exc) //ArgumentNullException || FormatException || OverFlowException
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }
            return value;
        }

        public static int ParseIntValue(this string value, int defaultValue)
        {
            int outValue = defaultValue;

            int.TryParse(value, out outValue);

            return outValue;
        }

        public static long ReadLongValue(this XmlNode node, string attributeName, long defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            long value;

            try
            {
                value = long.Parse(attr.Value);
            }
            catch (Exception exc) //ArgumentNullException || FormatException || OverFlowException
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }
            return value;
        }

        public static long ParseLongValue(this string value, int defaultValue)
        {
            long outValue = defaultValue;

            long.TryParse(value, out outValue);

            return outValue;
        }

        public static double ParseDoubleValue(this string value, double defaultValue)
        {
            double outValue = defaultValue;

            double.TryParse(value, out outValue);

            return outValue;
        }

        public static double ReadDoubleValue(this XmlNode node, string attributeName, double defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            double value;

            try
            {
                value = double.Parse(attr.Value);
            }
            catch (Exception exc)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber), exc);
            }

            return value;
        }

        public static float ParseFloatValue(this string value, float defaultValue)
        {
            float outValue = defaultValue;

            float.TryParse(value, out outValue);

            return outValue;
        }

        public static float ReadDecimalValue(this XmlNode node, string attributeName, float defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            float value;

            try
            {
                value = float.Parse(attr.Value);
            }
            catch (Exception exc)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }

            return value;
        }

        public static TimeSpan ParseTimeSpanValue(this string value, TimeSpan defaultValue)
        {
            TimeSpan outValue = defaultValue;

            TimeSpan.TryParse(value, out outValue);

            return outValue;
        }

        public static TimeSpan ReadTimeSpanValue(this XmlNode node, string attributeName, TimeSpan defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            TimeSpan value;

            try
            {
                value = TimeSpan.Parse(attr.Value);
            }
            catch (Exception exc)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }

            return value;
        }

        public static bool ReadBoolValue(this XmlNode node, string attributeName, bool defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            bool value;

            try
            {
                value = bool.Parse(attr.Value);
            }
            catch (Exception exc)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }

            return value;
        }

        public static T ReadEnumValue<T>(this XmlNode node, string attributeName, T defaultValue)
        {
            XmlAttribute attr = node.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName);

            if (attr == null)
                return defaultValue;

            T value;

            try
            {
                value = (T)Enum.Parse(typeof(T), attr.Value);
            }
            catch (Exception exc)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid value for attribute '{1}' on line {2}",
                                                          attr.Value, attributeName, ((IXmlLineInfo)node).LineNumber),
                                            exc);
            }

            return value;
        }
    }
}