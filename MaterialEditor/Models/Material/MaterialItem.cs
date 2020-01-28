using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public class MaterialItem : Material
    {
        private int featureTypeId = 0;
        private int materialCategoryId = 0;
        private IMaterialDbInterface dbInterface = null;
        private StringCollection sapAttributes = null;
        private Dictionary<string, Models.Attribute> additionalAttributes = null;
        private StringCollection recordOnlyAttributeNames = null;

        public MaterialItem() : base()
        {
            Attributes = new Dictionary<string, Attribute>();
        }

        public MaterialItem(long materialItemId) : base(materialItemId)
        {
            Models.Attribute attr = new Models.Attribute("id", materialItemId.ToString(), "unset");
            Models.Attribute mtrlId = new Models.Attribute("MtrlId", "0");
            Attributes = new Dictionary<string, Attribute>();

            Attributes.Add(attr.Name, attr);
            Attributes.Add(mtrlId.Name, mtrlId);
        }

        [JsonIgnore]
        public override StringCollection RecordOnlyAttributeNames
        {
            get
            {
                if (recordOnlyAttributeNames == null)
                {
                    recordOnlyAttributeNames = new StringCollection();

                    recordOnlyAttributeNames.Add(MaterialType.JSON.SpecNm);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.SetLgth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Apcl);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.UOM);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.SetLgthUom);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Stts);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.AmpsDrn);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.NumMtgSpcs);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MtgPltSz);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MxEqpPos);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Gge);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.HzrdInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.GgeUnt);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Hght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Wdth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Dpth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.EqpWght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.AccntCd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MtlType);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PrdTyp);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.EqptCls);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LctnPosInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PrtNbrTypCd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LctnPosInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MntPosHght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlHght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlDpth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlWdth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PosSchm);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LstUid);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LstDt);
                    
                }

                return recordOnlyAttributeNames;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.HasRvsns)]
        public override bool HasRevisions
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public override Dictionary<string, Models.Attribute> AdditionalAttributeNames
        {
            get
            {
                if (additionalAttributes == null)
                    additionalAttributes = ((MaterialDbInterface)DbInterface).GetAdditionalAttributes();

                return additionalAttributes;
            }
        }

        public override Dictionary<string, Attribute> Attributes
        {
            get;
            set;
            //get
            //{
            //    if (attributes == null)
            //        attributes = GetAttributes(this);

            //    if (attributes != null && attributes.ContainsKey(MaterialType.JSON.HasRvsns))
            //        attributes[MaterialType.JSON.HasRvsns].IsEditable = true;

            //    return attributes;
            //}
            //set
            //{
            //    attributes = value;
            //}
        }

        [JsonIgnore]
        public override IMaterialDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new MaterialDbInterface();

                return dbInterface;
            }
        }

        //[JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.FtrTyp, false, true)]
        public override int FeatureTypeId
        {
            get
            {
                return featureTypeId;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.MtlCtgry, false, true)]
        public override int MaterialCategoryId
        {
            get
            {
                return materialCategoryId;
            }
        }

        [JsonIgnore]
        public override StringCollection SAPOverrideAttributeNames
        {
            get
            {
                if (sapAttributes == null)
                    sapAttributes = MaterialType.GetSAPMaterialAttributes();

                return sapAttributes;
            }
        }

        public override void PersistUpdates(JObject updatedMaterialItem, string cuid, ref bool notifyNDS, ref bool notifyNDSSpecification)
        {
            
        }

        public override long[] PersistObject(IMaterial currentMaterialItem, JObject updatedMaterialItem, long mtrlId, ref bool notifyNDS, ref bool notifyNDSSpecification)
        {
            //TODO: complete this method when the New and Updated Parts screen is updated to use the material classes
            return new long[] { 0, 0 };
        }

        public override void CreateRevision(JObject updatedMaterialItem, ref bool notifyNDSSpecification)
        {
            
        }

        public class ElectricalItemObject
        {
            public string InputvoltageMinimum
            {
                get;
                set;
            }
            public string InputVoltageMaximum
            {
                get;
                set;
            }
            public string NormalElectricalCurrent
            {
                get;
                set;
            }
            public string MaximumElectricalCurrent
            {
                get;
                set;
            }
        }

        public List<ElectricalItemObject> ElectricalItems
        {
            get;
            set;
        }

        public class CompatibleSevenItemObject
        {
            public List<string> CompatibleClei7
            {
                get;
                set;
            }
        }

        public CompatibleSevenItemObject CompatibleSevenCLEIItem
        {
            get;
            set;
        }

        public long MfrID
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public string SpecName
        {
            get;
            set;
        }

        public string CLMC
        {
            get;
            set;
        }
    }
}