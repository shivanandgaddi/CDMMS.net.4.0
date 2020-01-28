using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class BayInternalSpecificationTest
    {
        [TestMethod]
        public void TestBayInternalSpecification()
        {
            ISpecification bi = null;
            BayInternalDbInterface dbInterface = new BayInternalDbInterface();
            long id = 1019;
            Dictionary<string, SpecificationAttribute> attributes = null;
            Dictionary<long, BayInternalWidth> internalWidthList = null;
            string json = "";

            try
            {
                bi = dbInterface.GetSpecification(id);

                attributes = bi.Attributes;

                json = JsonConvert.SerializeObject(attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                internalWidthList = dbInterface.GetInternalWidths();

                json = JsonConvert.SerializeObject(internalWidthList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestBaySpecification()
        {
            ISpecification bs = null;
            BaySpecificationDbInterface dbInterface = new BaySpecificationDbInterface();
            long id = 119;
            Dictionary<string, SpecificationAttribute> attributes = null;
            Dictionary<long, BayInternalWidth> internalWidthList = null;
            string json = "";

            try
            {
                bs = dbInterface.GetSpecification(id);

                attributes = bs.Attributes;

                json = JsonConvert.SerializeObject(attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                //internalWidthList = dbInterface.GetInternalWidths();

                //json = JsonConvert.SerializeObject(internalWidthList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                //Type st = bs.GetType();

                //PropertyInfo pi = st.GetProperty("BayInternalList");

                //Type t = pi.PropertyType.GetInterface("System.Collections.IDictionary");

                //if (t != null)
                //{
                //    System.Collections.IDictionary dict = (System.Collections.IDictionary)pi.GetValue(bs);

                //    if (dict != null)
                //    {
                //        System.Collections.IDictionaryEnumerator e = dict.GetEnumerator();

                //        if (e != null)
                //        {
                //            List<Dictionary<string, SpecificationAttribute>> al = new List<Dictionary<string, SpecificationAttribute>>();
                //            object key;
                //            Specification obj;

                //            while (e.MoveNext())
                //            {
                //                key = e.Key;
                //                obj = (Specification)e.Value;

                //                SpecificationAttribute sa = new SpecificationAttribute(true, "BayIntrls");

                //                al.Add(obj.Attributes);

                //                sa.ObjectList = al;

                //                attributes.Add(sa.Name, sa);
                //            }

                //            json = JsonConvert.SerializeObject(attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
