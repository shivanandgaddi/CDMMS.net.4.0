using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class PortSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;  
        private ISpecificationDbInterface dbInterface = null;

        public PortSpecification() : base()
        {
           
        }

        public PortSpecification(long specificationId) : base(specificationId)
        {
           
        }

        public PortSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
           
        }    

        [MaterialJsonProperty(SpecificationType.JSON.PortUseTypId)]
        public int PortUseTypId
        {
            get;
            set;
        }
       
        [MaterialJsonProperty(SpecificationType.JSON.UseTyp)]
        public string PortUseTyp
        {
            get;
            set;
        }
       
        [MaterialJsonProperty(SpecificationType.JSON.RvsnNm)]
        public string SpecNm
        {
            get;
            set;
        }
        
        [MaterialJsonProperty(SpecificationType.JSON.Gnrc, true, "")]
        public bool IsGeneric
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

        [MaterialJsonProperty(SpecificationType.JSON.PortTyp)]
        public string PortTyp
        {
            get;
            set;
        }

        [MaterialJsonProperty(SpecificationType.JSON.PortSrvLvl)]
        public string PortSrvLvl
        {
            get;
            set;
        }

        [MaterialJsonProperty(SpecificationType.JSON.physStts)]
        public string PhysStts
        {
            get;
            set;
        }

        [MaterialJsonProperty(SpecificationType.JSON.PortDept)]
        public string PortDept
        {
            get;
            set;
        }

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new PortSpecificationDbInterface();

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
                    if (!IsCompleted)
                    {
                        attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = false;
                    }
                    else if (!IsPropagated)
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = true;

                    if (!IsDeleted)
                        attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }
                else
                {
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
                return SpecificationType.Type.PORT;
            }
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

        public override string NDSManufacturer
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            string updatedModelName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedSpecName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);

            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            int updatedPortUseTypId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.PortUseTypId);

            string updatedPortTyp = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortTyp);
            string updatedPortSrvLvl = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortSrvLvl);
            int updatedportCnctrId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.CnctrTyp);
            string updatedPortPhysStts = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.physStts);
            string updatedPortDept = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortDept);

            if (Name != updatedModelName || SpecNm != updatedSpecName || Description != updatedDescription || IsCompleted != updatedIsCompleted
                || IsPropagated != updatedIsPropagated || IsDeleted != updatedIsDeleted || PortUseTypId != updatedPortUseTypId
                || PortTyp != updatedPortTyp || PortSrvLvl != updatedPortSrvLvl || ConnectorTypeId != updatedportCnctrId || PhysStts != updatedPortPhysStts || PortDept != updatedPortDept)
            {
                ((PortSpecificationDbInterface)DbInterface).UpdatePortSpecification(SpecificationId, updatedModelName, updatedSpecName, updatedDescription,
                                                                                    updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, updatedPortUseTypId,
                                                                                    updatedPortTyp, updatedPortSrvLvl, updatedportCnctrId, updatedPortPhysStts, updatedPortDept);

                notifyNDS = true;
            }

            //PersistSpecificationRole(updatedSpecification, ref notifyNDS);

            if (!updatedIsPropagated)
                notifyNDS = false;
        }       

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;

            string Modelname = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string revisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            int PortUseTypId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.PortUseTypId);

            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            string PortTyp = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortTyp);
            string PortSrvLvl = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortSrvLvl);
            int portCnctrId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.CnctrTyp);
            string PortPhysStts = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.physStts);
            string PortDept = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.PortDept);

            try
            {
                specificationId = ((PortSpecificationDbInterface)DbInterface).CreatePortSpecification(revisionName, Modelname, description, PortUseTypId, 
                                                                                                      isCompleted, isPropagated, isDeleted,
                                                                                                      PortTyp, PortSrvLvl, portCnctrId, PortPhysStts, PortDept);

                // CreateSlotSpecificationRole(updatedSpecification, specificationId);

                if (isPropagated)
                    notifyNDS = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new Slot specification");

                throw ex;
            }

            return specificationId;
        }
      

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {

        }

        public override void PersistNDSSpecificationId(long ndsId)
        {

        }
    }
}