using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class JsonHelper : DefaultContractResolver
    {
        private string excludePropertyName = null;

        public JsonHelper(string doNotSerializePropertyName)
        {
            excludePropertyName = doNotSerializePropertyName;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            properties = properties.Where(p => !p.PropertyName.Equals(excludePropertyName)).ToList();

            return properties;
        }

        public static Models.Attribute DeserializeAttribute(JObject obj, string attributeName)
        {
            Models.Attribute attribute = null;

            try
            {
                JToken token = obj.SelectToken(attributeName);

                if (token != null)
                {
                    attribute = JsonConvert.DeserializeObject<Models.Attribute>(token.ToString());

                    attribute.Name = attributeName;
                }
            }
            catch (Exception ex)
            {
            }

            return attribute;
        }

        public static Models.Attribute DeserializeAttributeNotNull(JObject obj, string attributeName)
        {
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute == null)
                attribute = new Models.Attribute(false, attributeName);

            return attribute;
        }

        public static int GetIntValue(JObject obj, string attributeName)
        {
            int val = 0;
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute != null)
                int.TryParse(attribute.Value, out val);

            return val;
        }

        public static long GetLongValue(JObject obj, string attributeName)
        {
            long val = 0;
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute != null)
                long.TryParse(attribute.Value, out val);

            return val;
        }

        public static decimal GetDecimalValue(JObject obj, string attributeName)
        {
            decimal val = 0;
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute != null)
                decimal.TryParse(attribute.Value, out val);

            return val;
        }

        public static bool GetBoolValue(JObject obj, string attributeName)
        {
            bool val = false;
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute != null)
                val = attribute.BoolValue;

            return val;
        }

        public static string GetStringValue(JObject obj, string attributeName)
        {
            string val = "";
            Models.Attribute attribute = DeserializeAttribute(obj, attributeName);

            if (attribute != null)
            {
                val = attribute.Value;

                if (val == null)
                    val = "";
            }

            return val;
        }

        public static SpecificationAttribute DeserializeSpecificationAttribute(JObject obj, string attributeName)
        {
            SpecificationAttribute attribute = null;

            try
            {
                JToken token = obj.SelectToken(attributeName);

                if (token != null)
                {
                    attribute = JsonConvert.DeserializeObject<SpecificationAttribute>(token.ToString());

                    attribute.Name = attributeName;
                }
            }
            catch (Exception ex)
            {
            }

            return attribute;
        }

        public static SpecificationAttribute DeserializeSpecificationAttributeNotNull(JObject obj, string attributeName)
        {
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute == null)
                attribute = new SpecificationAttribute(false, attributeName);

            return attribute;
        }

        public static int GetSpecificationIntValue(JObject obj, string attributeName)
        {
            int val = 0;
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute != null)
                int.TryParse(attribute.Value, out val);

            return val;
        }

        public static long GetSpecificationLongValue(JObject obj, string attributeName)
        {
            long val = 0;
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute != null)
                long.TryParse(attribute.Value, out val);

            return val;
        }

        public static decimal GetSpecificationDecimalValue(JObject obj, string attributeName)
        {
            decimal val = 0;
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute != null)
                decimal.TryParse(attribute.Value, out val);

            return val;
        }

        public static bool GetSpecificationBoolValue(JObject obj, string attributeName)
        {
            bool val = false;
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute != null)
                val = attribute.BoolValue;

            return val;
        }

        public static string GetSpecificationStringValue(JObject obj, string attributeName)
        {
            string val = "";
            SpecificationAttribute attribute = DeserializeSpecificationAttribute(obj, attributeName);

            if (attribute != null)
            {
                val = attribute.Value;

                if (val == null)
                    val = "";
            }

            return val;
        }

        public static int GetIntValue(string tokenName, JObject obj)
        {
            int val = 0;

            int.TryParse((string)obj.SelectToken(tokenName), out val);

            return val;
        }

        public static long GetLongValue(string tokenName, JObject obj)
        {
            long val = 0;

            long.TryParse((string)obj.SelectToken(tokenName), out val);

            return val;
        }

        public static decimal GetDecimalValue(string tokenName, JObject obj)
        {
            decimal val = 0;

            decimal.TryParse((string)obj.SelectToken(tokenName), out val);

            return val;
        }

        public static bool GetBoolValue(string tokenName, JObject obj)
        {
            bool val = false;

            bool.TryParse((string)obj.SelectToken(tokenName), out val);

            return val;
        }

        public static string GetStringValue(string tokenName, JObject obj)
        {
            string val = "";

            val = (string)obj.SelectToken(tokenName);

            return val;
        }

        public static TemplateAttribute DeserializeAttribute(string attributeName, JObject obj)
        {
            TemplateAttribute attribute = null;

            try
            {
                JToken token = obj.SelectToken(attributeName);

                if (token != null)
                {
                    attribute = JsonConvert.DeserializeObject<TemplateAttribute>(token.ToString());

                    attribute.Name = attributeName;
                }
            }
            catch (Exception ex)
            {
            }

            return attribute;
        }
    }
}