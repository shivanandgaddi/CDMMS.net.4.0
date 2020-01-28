using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class NodeSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, SpecificationRole> roleList = null;
        private ISpecificationDbInterface dbInterface = null;
        private IMaterial node = null;
        private long materialItemId = 0;

        public NodeSpecification() : base()
        {
            //PopulateRoleList();
        }

        public NodeSpecification(long specificationId) : base(specificationId)
        {
            //PopulateRoleList();
        }

        public NodeSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            //PopulateRoleList();
        }

        public void PopulateRoleList()
        {
            roleList = ((NodeSpecificationDbInterface)DbInterface).GetRoleList(SpecificationId);
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.SpcnRlTypLst, "")]
        public Dictionary<long, SpecificationRole> RoleList
        {
            get
            {
                return roleList;
            }
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Gnrc, true, "")]
        public bool IsGeneric
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.NodeTypId)]
        public int NodeTypeId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NodeUseTypId)]
        public string NodeUseTypId
        {
            get;
            set;
        }
        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Prts, true, "")]
        public bool HasPorts
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Shlvs, true, "")]
        public bool HasShelves
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RO, true, "")]
        public bool IsRecordOnly
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MidPln, true, "")]
        public string MidPlaneIndicator
        {
            get;
            set;
        }

        //added  
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StrghtThru, true, "")]
        public string StraightThroughIndicator
        {
            get;
            set;
        }

        //added   
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Dpth)]
        public decimal Depth
        {
            get;
            set;
        }

        //added  
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Hght)]
        public decimal Height
        {
            get;
            set;
        }

        //added   
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Wdth)]
        public decimal Width
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.DimUom)]
        public int DimensionsUnitOfMeasure
        {
            get;
            set;
        }

        //added      
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MuxCpbl, true, "")]
        public string MultiplexingCapable
        {
            get;
            set;
        }

        //added
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PerfMonitrgCpbl, true, "")]
        public string PerformanceMonitoringCapable
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.EnniCpbl, true, "")]
        public string EnniCapable
        {
            get;
            set;
        }

        //added    
        //[JsonIgnore]
        //[MaterialJsonProperty(SpecificationType.JSON.EsPlsCrdReqr, true, "")]
        //public string ESPlsCardRequired
        //{
        //    get;
        //    set;
        //}

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NwSrvcAllw, true, "")]
        public string NewServiceAllowed
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty( SpecificationType.JSON.NdeFrmtCd)]
        public string FormatCode
        {
            get;
            set;
        }

        //added     
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.NdeFrmtValQlfrId)]
        public int FormatValueQualifier
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NdeFrmtNcludInd, true, "")]
        public string IncludeFormatCode
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StructType, true, "")]
        public string StructureType
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty( SpecificationType.JSON.SwVrsn)]
        public string SoftwareVersion
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty( SpecificationType.JSON.MtrlCd)]
        public string MTRL_cd
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MfgPrtNo)]
        public string MFG_part_no
        {
            get;
            set;
        }

        //added    
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Mfr)]
        public string MFR_cd
        {
            get;
            set;
        }

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.WllMnt, true, "")]
        public string WallMountIndicator
        {
            get;
            set;
        }

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.LblNm)]
        public string LabelName
        {
            get;
            set;
        }

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.LblPosId)]
        public int LabelPositionId
        {
            get;
            set;
        }

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RvsnNm)]
        public string RevisionName
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

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RvsnId)]
        public long RevisionId
        {
            get;
            set;
        }

        //added 
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.QoSCpbl, true, "")]
        public string QoSCapable
        {
            get;
            set;
        }

        [JsonIgnore]
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return node;
            }
            set
            {
                node = value;
            }
        }

        [JsonIgnore]
        public override long AssociatedMaterialId
        {
            get
            {
                return materialItemId;
            }
        }

        [JsonIgnore]
        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new NodeSpecificationDbInterface();

                return dbInterface;
            }
        }
        public override Dictionary<string, SpecificationAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = GetAttributes(this, true);

                if (SpecificationId > 0)
                {
                    attributes[SpecificationType.JSON.Gnrc].IsEditable = false;
                    attributes[SpecificationType.JSON.RO].IsEditable = false;

                    if (!IsCompleted)
                    {
                        attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = false;
                    }
                    else if (!IsPropagated)
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = true;

                    if (!IsDeleted)
                        attributes[SpecificationType.JSON.Dltd].IsEditable = true;

                    if (node != null)
                    {
                        SpecificationAttribute nodeAttr = new SpecificationAttribute();
                        Dictionary<string, SpecificationAttribute> dictionary = new Dictionary<string, SpecificationAttribute>();

                        nodeAttr.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();

                        foreach (string key in node.Attributes.Keys)
                        {
                            SpecificationAttribute attr = new SpecificationAttribute();

                            attr.Name = key;

                            if (node.Attributes[key].Options != null)
                            {
                                //if (MaterialType.JSON.DimUom.Equals(key))
                                //{
                                    attr.Options = node.Attributes[key].Options;
                                    attr.Value = node.Attributes[key].Value;
                                //}
                                //else
                                //{
                                //    foreach (Option option in node.Attributes[key].Options)
                                //    {
                                //        if (node.Attributes[key].Value == option.Value)
                                //        {
                                //            attr.Value = option.Text;
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                            else
                                attr.Value = node.Attributes[key].Value;

                            if (MaterialType.JSON.DimUom.Equals(key) || MaterialType.JSON.Dpth.Equals(key) || MaterialType.JSON.Hght.Equals(key) || MaterialType.JSON.Wdth.Equals(key))
                                attr.IsEditable = true;

                            dictionary.Add(key, attr);
                        }

                        if (!dictionary.ContainsKey(MaterialType.JSON.Dpth))
                            dictionary.Add(MaterialType.JSON.Dpth, new SpecificationAttribute(false, MaterialType.JSON.Dpth));

                        if (!dictionary.ContainsKey(MaterialType.JSON.Hght))
                            dictionary.Add(MaterialType.JSON.Hght, new SpecificationAttribute(false, MaterialType.JSON.Hght));

                        if (!dictionary.ContainsKey(MaterialType.JSON.Wdth))
                            dictionary.Add(MaterialType.JSON.Wdth, new SpecificationAttribute(false, MaterialType.JSON.Wdth));

                        nodeAttr.ObjectList.Add(dictionary);

                        attributes.Add("Node", nodeAttr);
                    }
                }
                else
                {
                    attributes[SpecificationType.JSON.Gnrc].IsEditable = true;
                    attributes[SpecificationType.JSON.RO].IsEditable = true;
                    attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                    attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }

                return attributes;
            }

            set
            {
                attributes = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.NODE;
            }
        }

        private string FormSpecificationName(JObject updatedSpecification)
        {
            string name = "ND-";
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            //NOT COMPLETE
            name += height + "X" + width + "X" + depth;

            return name;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            int nodeTypeId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.NodeTypId);
            bool portsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prts);
            bool shelvesInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Shlvs);
            string qosCpblInd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.QoSCpbl);
            string muxgCpbl = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MuxCpbl);
            string prfMc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PerfMonitrgCpbl);
            string eNNi = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.EnniCpbl);
            //string esPlsCard = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.EsPlsCrdReqr);
            string newSrvc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NwSrvcAllw);
            string nodeFrmtCd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdeFrmtCd);
            int nodeFrmtVal = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.NdeFrmtValQlfrId);
            string frmtNclud = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdeFrmtNcludInd);
            string nodeSpecDsc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string wallMnt = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string straightThru = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlan = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            bool gnrc = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);
            string smpleCmplx = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StructType);
            string labelNm = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.LblNm);
            int labelPosId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.LblPosId);
            string swVrsnNo = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.SwVrsn);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            if (!gnrc)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.SpecificationId));

            try
            {
                specificationId = ((NodeSpecificationDbInterface)DbInterface).CreateNodeSpecification(name, nodeTypeId, portsInd, shelvesInd, qosCpblInd, muxgCpbl, prfMc, eNNi,
                    newSrvc, nodeFrmtCd, nodeFrmtVal, frmtNclud, nodeSpecDsc, wallMnt, straightThru, midPlan, gnrc, smpleCmplx, labelNm, labelPosId, swVrsnNo);

                if (gnrc)
                    CreateGenericSpecification(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);
                else
                    CreateSpecificationRevision(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);

                //if (isPropagated)
                //    notifyNDS = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new node specification");

                throw ex;
            }

            return specificationId;
        }

        private void CreateGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            ((NodeSpecificationDbInterface)DbInterface).CreateGenericNodeSpecification(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N",
                isDeleted ? "Y" : "N", depth, height, width, uom);
        }

        private void CreateSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            string revisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            decimal weight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);
            long revisionId = 0;
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);

            revisionId = ((NodeSpecificationDbInterface)DbInterface).CreateNodeSpecificationRevision(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N",
                isDeleted ? "Y" : "N", weight, uom, isRO ? "Y" : "N", revisionName);

            ((NodeSpecificationDbInterface)DbInterface).AssociateMaterial(revisionId, materialItemId);
        }

        //private void CreateNodeSpecificationRole(JObject updatedSpecification, long specificationId)
        //{
        //    List<Dictionary<string, SpecificationAttribute>> updatedRoleList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst).ObjectList;
        //    foreach (Dictionary<string, SpecificationAttribute> updatedRole in updatedRoleList)
        //    {
        //        if (updatedRole["Slctd"].BoolValue)
        //        {
        //            ((NodeSpecificationDbInterface)DbInterface).InsertNodeSpecificationRole(specificationId,
        //                int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value));
        //        }
        //    }

        //}

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            string updatedname = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            int updatednodeTypeId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.NodeTypId);
            bool updatedportsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prts);
            bool updatedshelvesInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Shlvs);
            string updatedqosCpblInd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.QoSCpbl);
            string updatedmuxgCpbl = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MuxCpbl);
            string updatedprfMc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PerfMonitrgCpbl);
            string updatedeNNi = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.EnniCpbl);
            //string updatedesPlsCard = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.EsPlsCrdReqr);
            string updatednewSrvc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NwSrvcAllw);
            string updatednodeFrmtCd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdeFrmtCd);
            int updatednodeFrmtVal = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.NdeFrmtValQlfrId);
            string updatedfrmtNclud = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdeFrmtNcludInd);
            string updatednodeSpecDsc = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string updatedwallMnt = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string updatedstraightThru = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string updatedmidPlan = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            bool updatedgnrc = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);
            string updatedsmpleCmplx = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StructType);
            string updatedlabelNm = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.LblNm);
            int updatedlabelPosId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.LblPosId);
            string updatedswVrsnNo = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.SwVrsn);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            if (!IsGeneric)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.SpecificationId));

            if (Name != updatedname || Description != updatednodeSpecDsc || StraightThroughIndicator != updatedstraightThru || MidPlaneIndicator != updatedmidPlan ||
                LabelName != updatedlabelNm || LabelPositionId != updatedlabelPosId || NodeTypeId != updatednodeTypeId || HasPorts != updatedportsInd
                || HasShelves != updatedshelvesInd || QoSCapable != updatedqosCpblInd || MultiplexingCapable != updatedmuxgCpbl || PerformanceMonitoringCapable != updatedprfMc
                || EnniCapable != updatedeNNi || NewServiceAllowed != updatednewSrvc || FormatCode != updatednodeFrmtCd
                || FormatValueQualifier != updatednodeFrmtVal || IncludeFormatCode != updatedfrmtNclud || Description != updatednodeSpecDsc
                || WallMountIndicator != updatedwallMnt || StraightThroughIndicator != updatedstraightThru || MidPlaneIndicator != updatedmidPlan
                || IsGeneric != updatedgnrc || StructureType != updatedsmpleCmplx || LabelName != updatedlabelNm || LabelPositionId != updatedlabelPosId
                || SoftwareVersion != updatedswVrsnNo
                )
            {
                ((NodeSpecificationDbInterface)DbInterface).UpdateNodeSpecification(SpecificationId, updatedname, updatednodeTypeId, updatedportsInd, updatedshelvesInd,
                 updatedqosCpblInd, updatedmuxgCpbl, updatedprfMc, updatedeNNi, updatednewSrvc, updatednodeFrmtCd, updatednodeFrmtVal, updatedfrmtNclud
                 , updatednodeSpecDsc, updatedwallMnt, updatedstraightThru, updatedmidPlan, updatedgnrc, updatedsmpleCmplx, updatedlabelNm, updatedlabelPosId, updatedswVrsnNo);

                //notifyNDS = true;
            }


            if (IsGeneric)
                PersistGenericSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);
            else
                PersistSpecificationRevision(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);

            //PersistSpecificationRole(updatedSpecification, ref notifyNDS);

            //if (!updatedIsPropagated)
            //    notifyNDS = false;
        }

        private void PersistGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            decimal updatedDepth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal updatedHeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal updatedWidth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int updatedUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || Depth != updatedDepth || Height != updatedHeight ||
                Width != updatedWidth || DimensionsUnitOfMeasure != updatedUom)
            {
                ((NodeSpecificationDbInterface)DbInterface).UpdateGenericNOdeSpecification(SpecificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N",
                    isDeleted ? "Y" : "N", updatedDepth, updatedHeight, updatedWidth, updatedUom);

                //notifyNDS = true;
            }
        }

        private void PersistSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            string updatedRevisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            
            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || RevisionName != updatedRevisionName || IsRecordOnly != isRO)
            {
                ((NodeSpecificationDbInterface)DbInterface).UpdateNodeSpecificationRevision(RevisionId, SpecificationId, updatedRevisionName, isCompleted, isPropagated, isDeleted, isRO);

                //notifyNDS = true;
            }

            if (materialItemId > 0 && (node == null || node.MaterialItemId != materialItemId))
            {
                if (node != null)
                    ((NodeSpecificationDbInterface)DbInterface).AssociateMaterial(0, node.MaterialItemId);

                ((NodeSpecificationDbInterface)DbInterface).AssociateMaterial(RevisionId, materialItemId);

                //notifyNDS = true;
            }
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            decimal height = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Hght));
            decimal depth = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Dpth));
            decimal width = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Wdth));
            decimal plannedHeatGeneration = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtGntn));
            decimal normalCurrentDrain = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnNrm));
            decimal maxCurrentDrain = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnMx));
            decimal nodeWeight = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.Wght));
            decimal heatDissipation = decimal.Parse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtDssptn));
            int uomId = 0;
            int plannedHeatGenerationUom = 0;
            int normalCurrentDrainUom = 0;
            int maxCurrentDrainUom = 0;
            int nodeWeightUom = 0;
            int heatDissipationUom = 0;

            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Hght), out height);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Dpth), out depth);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.Wdth), out width);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtGntn), out plannedHeatGeneration);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnNrm), out normalCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnMx), out maxCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.Wght), out nodeWeight);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtDssptn), out heatDissipation);

            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtGntnUom), out plannedHeatGenerationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnNrmUom), out normalCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.ElcCurrDrnMxUom), out maxCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.WghtUom), out nodeWeightUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", MaterialType.JSON.HtDssptnUom), out heatDissipationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Node", SpecificationType.JSON.DimUom), out uomId);
            //notifyNDS = 

            if (uomId == 0)
                uomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

            ((NodeSpecificationDbInterface)DbInterface).UpdateNodeSpecificationMaterial(materialItemId, height, depth, width, uomId, plannedHeatGeneration, plannedHeatGenerationUom, normalCurrentDrain,
                normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, nodeWeight, nodeWeightUom, heatDissipation, heatDissipationUom);
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
        }
    }
}