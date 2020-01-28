using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class PlugInSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, PlugInRoleTypeSpecification> plugInRoleTypeList = null;
        private ISpecificationDbInterface dbInterface = null;

        public PlugInSpecification() : base()
        {
            PopulatePlugInRoleTypeList();
        }

        public PlugInSpecification(long specificationId) : base(specificationId)
        {
            PopulatePlugInRoleTypeList();
        }

        public PlugInSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulatePlugInRoleTypeList();
        }

        public void PopulatePlugInRoleTypeList()
        {
            plugInRoleTypeList = ((PlugInSpecificationDbInterface)DbInterface).GetPlugInRoleTypes();
        }

        [JsonIgnore]
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        [JsonIgnore]
        public override long AssociatedMaterialId
        {
            get
            {
                return 0;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.PLUG_IN;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.PlgInRlTypLst, "")]
        public Dictionary<long, PlugInRoleTypeSpecification> PlugInRoleTypeList
        {
            get
            {
                return plugInRoleTypeList;
            }
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PluginUseTypId)]
        public string PluginUseTypId
        {
            get;
            set;
        }       

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MxLiteXmsn)]
        public decimal MaxLightTransmissionDistance
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.DistUom)]
        public int MaxLightTransmissionDistanceUom
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BiDrctnl)]
        public string BiDirectionalIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.VarWvlgth)]
        public string VariableWavelengthIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.FrmFctr)]
        public string FormFactorCode
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.FnctnCd)]
        public string FunctionCodeDescription
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.LoTmp)]
        public decimal LowTemperature
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.HiTmp)]
        public decimal HighTemperature
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.XmsnMed)]
        public int TransmissionMediaId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.CnctrTyp)]
        public int ConnectorTypeId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PlgInRlTypId)]
        public int PlugInRoleTypeId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.ChnlNo)]
        public int ChannelNumber
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MultFxWvlgth)]
        public string MultipleFixedWavelengthIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.Wvlgth)]
        public int WavelengthId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.TraWvlgth)]
        public int TransmitWavelengthId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.RecWvlgth)]
        public int RecieveWavelengthId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.XmtRcvInd, OptionValues = new string[] { "T", "Transmit", "R", "Receive" })]
        public string WavelengthTransmitReceiveIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.XmsnRt)]
        public int TransmissionRateId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.TransmissionRateLst)]
        public string TransmissionRateLst
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NdsMfr)]
        public override string NDSManufacturer
        {
            get;
            set;
        }

        public override Dictionary<string, SpecificationAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = GetAttributes(this, true);

                return attributes;
            }

            set
            {
                attributes = value;
            }
        }

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new PlugInSpecificationDbInterface();

                return dbInterface;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RvsnId)]
        public long RevisionId
        {
            get;
            set;
        }
        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;
            long waveLengthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.Wvlgth);
            long transmissionRateId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.XmsnRt);
            int maxDistanceUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DistUom);
            int transmissionMediaId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.XmsnMed);
            int connectorId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.CnctrTyp);
            int pluginRoleTypeId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.PlgInRlTypId);
            int channelNumber = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.ChnlNo);
            decimal maxLightDistance = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxLiteXmsn);
            decimal lowTemp = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.LoTmp);
            decimal hiTemp = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.HiTmp);
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string biDirectionalIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BiDrctnl);
            string variableWaveLengthIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.VarWvlgth);
            string transmitReceiveIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.XmtRcvInd);
            string formCode = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.FrmFctr);
            string functionDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.FnctnCd);
            string multipleWaveLengthIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MultFxWvlgth);

            try
            {
                //DbInterface.StartTransaction();

                specificationId = ((PlugInSpecificationDbInterface)DbInterface).CreatePlugInSpecification(name, maxLightDistance, maxDistanceUom, biDirectionalIndicator, variableWaveLengthIndicator,
                    formCode, functionDescription, lowTemp, hiTemp, transmissionMediaId, connectorId, pluginRoleTypeId, channelNumber, multipleWaveLengthIndicator);

                if (waveLengthId > 0)
                    ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInWaveLength(specificationId, waveLengthId, transmitReceiveIndicator);

                if (transmissionRateId > 0)
                    ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInTransmissionRate(specificationId, transmissionRateId);

                //notifyNDS = true;

                //DbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                //DbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to create new Plug-In specification");

                throw ex;
            }
            //finally
            //{
            //    DbInterface.Dispose();
            //}

            return specificationId;
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {

            // long updatedTransmissionRateId = JsonHelper.DeserializeAttribute(updatedSpecification, SpecificationType.JSON.XmsnRt);
            string updatedTransmissionRateStr = string.Empty;
            string[] updatedTransmissionRateArray = null;
            if (updatedSpecification["Transrate"].ToString() != null)
            {
                updatedTransmissionRateStr = updatedSpecification["Transrate"].ToString();

                updatedTransmissionRateStr = updatedTransmissionRateStr.Replace("\r\n  \"", string.Empty);
                updatedTransmissionRateStr = updatedTransmissionRateStr.Replace("\"", string.Empty);
                updatedTransmissionRateStr = updatedTransmissionRateStr.Replace("\r\n", string.Empty);
                updatedTransmissionRateStr = updatedTransmissionRateStr.Replace("[", string.Empty);
                updatedTransmissionRateStr = updatedTransmissionRateStr.Replace("]", string.Empty);

                updatedTransmissionRateArray = updatedTransmissionRateStr.Split(',');
            }

            int updatedMaxDistanceUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DistUom);
            int updatedTransmissionMediaId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.XmsnMed);
            int updatedConnectorId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.CnctrTyp);
            int updatedPluginRoleTypeId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.PlgInRlTypId);
            int updatedChannelNumber = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.ChnlNo);
            decimal updatedMaxLightDistance = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxLiteXmsn);
            decimal updatedLowTemp = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.LoTmp);
            decimal updatedHiTemp = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.HiTmp);
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string updatedBiDirectionalIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BiDrctnl);
            string updatedVariableWaveLengthIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.VarWvlgth);
            string updatedTransmitReceiveIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.XmtRcvInd);
            string updatedFormCode = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.FrmFctr);
            string updatedFunctionDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.FnctnCd);
            string updatedMultipleWaveLengthIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MultFxWvlgth);
            string receiverWavLen = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RecWvlgth);
            string transmittedWavLen = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.TraWvlgth);
            long updatedWaveLengthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.Wvlgth);
            //UpdatePlugInSpecification(long specificationId, string name, decimal maxLightDistance, int maxDistanceUom, string biDirectionalIndicator, string variableWaveLengthIndicator, string formCode, string functionDescription,
            //decimal lowTemp, decimal hiTemp, int transmissionId, int connectorId, int pluginRoleTypeId, int channelNumber, int multipleWaveLengthIndicator)

            if (MaxLightTransmissionDistance != updatedMaxLightDistance || TransmissionMediaId != updatedTransmissionMediaId || ConnectorTypeId != updatedConnectorId ||
                Name != updatedName || Description != updatedDescription || PlugInRoleTypeId != updatedPluginRoleTypeId || ChannelNumber != updatedChannelNumber ||
                MultipleFixedWavelengthIndicator != updatedMultipleWaveLengthIndicator || MaxLightTransmissionDistanceUom != updatedMaxDistanceUom || LowTemperature != updatedLowTemp || HighTemperature != updatedHiTemp ||
                BiDirectionalIndicator != updatedBiDirectionalIndicator || VariableWavelengthIndicator != updatedVariableWaveLengthIndicator || FormFactorCode != updatedFormCode || FunctionCodeDescription != updatedFunctionDescription)
            {
                ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInSpecification(SpecificationId, updatedName, updatedMaxLightDistance, updatedMaxDistanceUom, updatedBiDirectionalIndicator, updatedVariableWaveLengthIndicator,
                    updatedFormCode, updatedFunctionDescription, updatedLowTemp, updatedHiTemp, updatedTransmissionMediaId, updatedConnectorId, updatedPluginRoleTypeId, updatedChannelNumber, updatedMultipleWaveLengthIndicator);

                //notifyNDS = true;
            }

            if (WavelengthId != updatedWaveLengthId)
            {
                ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInWaveLength(SpecificationId, updatedWaveLengthId, "");
                //notifyNDS = true;
            }
            else
            {
                if (receiverWavLen != "" && transmittedWavLen != "")
                {
                    ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInWaveLength(SpecificationId, Convert.ToInt64(transmittedWavLen), "T");                  
                    ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInWaveLength(SpecificationId, Convert.ToInt64(receiverWavLen), "R");
                }
            }
            //if (TransmissionRateId != updatedTransmissionRateId)
            //{
            //((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInTransmissionRate(SpecificationId, updatedTransmissionRateId);
            //notifyNDS = true;
            //}
            ((PlugInSpecificationDbInterface)DbInterface).UpdatePlugInTransmissionRate(SpecificationId, updatedTransmissionRateArray);

        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {

        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
        }
    }
}