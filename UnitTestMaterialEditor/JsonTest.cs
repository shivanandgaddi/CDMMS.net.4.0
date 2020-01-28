using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        public void TestCustomJsonAttribute()
        {
            ConnectorizedCable cable = new ConnectorizedCable(99, 101);
            Dictionary<string, CenturyLink.Network.Engineering.Material.Editor.Models.Attribute> attributes = null;
            string json = "";
            int featureType = 0;
            decimal d = 0;
            long l = 0;
            double dd = 0;
            DateTime dt = DateTime.Now;
            Decimal bigD = new Decimal(0);
            Double bigDD = new Double();
            Part part = new Part(); 

            cable.VariableLengthCableProductId = "ANMW-XXX";
            cable.RevisionNumber = "1B";

            featureType = cable.FeatureTypeId;

            Type objType = part.GetType();
            string pName = "";
            string mName = "";

            attributes = cable.Attributes;

            json = JsonConvert.SerializeObject(attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            foreach (PropertyInfo p in objType.GetProperties())
            {
                pName = p.PropertyType.Name;

                foreach (System.Attribute a in p.GetCustomAttributes(true))
                {
                    MaterialJsonProperty m = (MaterialJsonProperty)a;

                    mName = m.Name;
                }
            }
        }

        [TestMethod]
        public void TestDeserializeAttribute()
        {
            string json = "{\"id\":{\"value\":\"90685\",\"bool\":false,\"src\":\"unset\"},\"PrdctId\":{\"value\":\"1145066\",\"bool\":false,\"src\":\"SAP\"},\"PrtNo\":{\"value\":\"6987739\",\"bool\":false,\"src\":\"SAP\"},\"Mfg\":{\"value\":\"GLCC\",\"bool\":false,\"src\":\"SAP\"},\"Apcl\":{\"value\":\"AB\",\"bool\":false,\"src\":\"SAP\"},\"UOM\":{\"value\":\"FT\",\"bool\":false,\"src\":\"SAP\"},\"CtgryId\":{\"value\":\"112001007\",\"bool\":false,\"src\":\"SAP\"},\"ItmDesc\":{\"value\":\"CABLE ANMW 200PR 24GA ORDER BY THE FOOT\",\"bool\":false,\"src\":\"SAP\"},\"MfgDesc\":{\"value\":\"GENERAL CABLE IND\",\"bool\":false,\"src\":\"SAP\"},\"ICC\":{\"value\":\"I100\",\"bool\":false,\"src\":\"SAP\"},\"MIC\":{\"value\":\"5436\",\"bool\":false,\"src\":\"SAP\"},\"AddtlDesc\":{\"value\":\"6987739 CABLE ANMW 200PR 24GA GENERALCAB\",\"bool\":false,\"src\":\"SAP\"},\"AvgPrc\":{\"value\":\"2.95\",\"bool\":false,\"src\":\"SAP\"},\"HzrdInd\":{\"bool\":false,\"src\":\"SAP\"},\"Stck\":{\"value\":\"9\",\"bool\":false,\"src\":\"SAP\"},\"Hght\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\",\"wasUpdated\":true},\"Wdth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"Dpth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"HECI\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"AltUOM\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"MtlType\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt1\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt2\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"SetLgth\":{\"value\":\"0\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"SpecNm\":{\"value\":\"ANMW-200\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"FtrTyp\":{\"value\":\"copper_cable\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Stts\":{\"value\":\"QA\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Nebs\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"Cpr\":{\"value\":\"UNKNOWN\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"AccntCd\":{\"value\":\"--7C\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"LstDt\":{\"value\":\"2012/04/11 10:15:29\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"ApprvdInd\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"}}";
            Newtonsoft.Json.Linq.JObject obj = null;
            CenturyLink.Network.Engineering.Material.Editor.Models.Attribute attr = null;
            CenturyLink.Network.Engineering.Material.Editor.Models.Attribute attr2 = null;

            try
            {
                obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                attr = JsonHelper.DeserializeAttribute(obj, "PrdctId");
                attr2 = JsonHelper.DeserializeAttribute(obj, "xyz");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestDeserialize()
        {
            string json = "{\"mtl\":{\"id\":{\"value\":\"76325\",\"bool\":false,\"src\":\"unset\"},\"PrdctId\":{\"value\":\"1145064\",\"bool\":false,\"src\":\"SAP\"},\"PrtNo\":{\"value\":\"6987796\",\"bool\":false,\"src\":\"SAP\"},\"Mfg\":{\"value\":\"GLCC\",\"bool\":false,\"src\":\"SAP\"},\"Apcl\":{\"value\":\"AB\",\"bool\":false,\"src\":\"SAP\"},\"UOM\":{\"value\":\"FT\",\"bool\":false,\"src\":\"SAP\"},\"CtgryId\":{\"value\":\"112001007\",\"bool\":false,\"src\":\"SAP\"},\"ItmDesc\":{\"value\":\"CABLE ANMW 1500PR 24GA ORDER BY THE FOOT\",\"bool\":false,\"src\":\"SAP\"},\"MfgDesc\":{\"value\":\"GENERAL CABLE IND\",\"bool\":false,\"src\":\"SAP\"},\"ICC\":{\"value\":\"I100\",\"bool\":false,\"src\":\"SAP\"},\"MIC\":{\"value\":\"5514\",\"bool\":false,\"src\":\"SAP\"},\"AddtlDesc\":{\"value\":\"6987796 CABLE ANMW 1500PR 24GA GENERALCAB\",\"bool\":false,\"src\":\"SAP\"},\"AvgPrc\":{\"value\":\"21.46\",\"bool\":false,\"src\":\"SAP\"},\"HzrdInd\":{\"bool\":false,\"src\":\"SAP\"},\"Stck\":{\"value\":\"9\",\"bool\":false,\"src\":\"SAP\"},\"Hght\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\",\"wasUpdated\":true},\"Wdth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"Dpth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"HECI\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"AltUOM\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"MtlType\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt1\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt2\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"SetLgth\":{\"value\":\"0\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"SpecNm\":{\"value\":\"ANMW - 1500\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"FtrTyp\":{\"value\":\"copper_cable\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Stts\":{\"value\":\"QA\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Nebs\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"Cpr\":{\"value\":\"UNKNOWN\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"AccntCd\":{\"value\":\"--7C\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"LstDt\":{\"value\":\"2012 / 04 / 11 10:15:29\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"ApprvdInd\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"}},\"viewIspOrOspAttrs\":\"O\",\"exists\":true,\"__moduleId__\":\"viewmodels / materialItem\"}";
            string json2 = "{\"id\":{\"value\":\"90685\",\"bool\":false,\"src\":\"unset\"},\"PrdctId\":{\"value\":\"1145066\",\"bool\":false,\"src\":\"SAP\"},\"PrtNo\":{\"value\":\"6987739\",\"bool\":false,\"src\":\"SAP\"},\"Mfg\":{\"value\":\"GLCC\",\"bool\":false,\"src\":\"SAP\"},\"Apcl\":{\"value\":\"AB\",\"bool\":false,\"src\":\"SAP\"},\"UOM\":{\"value\":\"FT\",\"bool\":false,\"src\":\"SAP\"},\"CtgryId\":{\"value\":\"112001007\",\"bool\":false,\"src\":\"SAP\"},\"ItmDesc\":{\"value\":\"CABLE ANMW 200PR 24GA ORDER BY THE FOOT\",\"bool\":false,\"src\":\"SAP\"},\"MfgDesc\":{\"value\":\"GENERAL CABLE IND\",\"bool\":false,\"src\":\"SAP\"},\"ICC\":{\"value\":\"I100\",\"bool\":false,\"src\":\"SAP\"},\"MIC\":{\"value\":\"5436\",\"bool\":false,\"src\":\"SAP\"},\"AddtlDesc\":{\"value\":\"6987739 CABLE ANMW 200PR 24GA GENERALCAB\",\"bool\":false,\"src\":\"SAP\"},\"AvgPrc\":{\"value\":\"2.95\",\"bool\":false,\"src\":\"SAP\"},\"HzrdInd\":{\"bool\":false,\"src\":\"SAP\"},\"Stck\":{\"value\":\"9\",\"bool\":false,\"src\":\"SAP\"},\"Hght\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\",\"wasUpdated\":true},\"Wdth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"Dpth\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"HECI\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"AltUOM\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"MtlType\":{\"value\":\"\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt1\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"ConvRt2\":{\"value\":\"0\",\"bool\":false,\"src\":\"SAP\"},\"SetLgth\":{\"value\":\"0\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"SpecNm\":{\"value\":\"ANMW-200\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"FtrTyp\":{\"value\":\"copper_cable\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Stts\":{\"value\":\"QA\",\"bool\":false,\"iOro\":\"C\",\"src\":\"NDS\"},\"Nebs\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"Cpr\":{\"value\":\"UNKNOWN\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"AccntCd\":{\"value\":\"--7C\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"LstDt\":{\"value\":\"2012/04/11 10:15:29\",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"},\"ApprvdInd\":{\"value\":\"N  \",\"bool\":false,\"iOro\":\"I\",\"src\":\"COEQA\"}}";
            Object item = null;
            Newtonsoft.Json.Linq.JObject a = null;
            CenturyLink.Network.Engineering.Material.Editor.Models.Attribute attr = null;

            try
            {
                item = JsonConvert.DeserializeObject(json);
                a = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json2);
                attr = JsonConvert.DeserializeObject<CenturyLink.Network.Engineering.Material.Editor.Models.Attribute>(a.SelectToken("id").ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestSerialize()
        {
            Part p = new Part();
            Part p2 = null;
            Dictionary<string, Attribute> d = null;
            Attribute a = new Attribute();
            Attribute b = new Attribute();
            Option o = new Option();
            Option o2 = new Option();
            string json = "";

            p.Attributes = new Dictionary<string, Attribute>();

            o.Value = "copper";
            o.Text = "Copper";

            o2.Value = "fiber";
            o2.Text = "Fiber";

            a.Name = "MtlCode";
            a.Value = "123";
            a.Options = new List<Option>();

            a.Options.Add(o);
            a.Options.Add(o2);

            b.Name = "MtlGrp";
            b.Value = "Cable";
            b.Type = "List";

            p.Attributes.Add(a.Name, a);
            p.Attributes.Add(b.Name, b);

            json = JsonConvert.SerializeObject(p.Attributes, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            d = JsonConvert.DeserializeObject<Dictionary<string, Attribute>>(json);
        }
    }

    public class Part
    {
        public Dictionary<string, Attribute> Attributes
        {
            get;
            set;
        }

        public int featureType
        {
            get;
            set;
        }

        public decimal d
        {
            get;
            set;
        }

        public long l
        {
            get;
            set;
        }

        public double dd
        {
            get;
            set;
        }

        public DateTime dt
        {
            get;
            set;
        }

        public Decimal bigD
        {
            get;
            set;
        }

        public Double bigDD
        {
            get;
            set;
        }
    }

    public class Attribute
    {
        [JsonIgnore]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }

        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("options")]
        public List<Option> Options
        {
            get;
            set;
        }
    }

    public class Option
    {
        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }

        [JsonProperty("text")]
        public string Text
        {
            get;
            set;
        }
    }
}
