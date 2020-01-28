using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public abstract class MaterialRevision : Material
    {
        public static string MTRL_REVSN_TABLE = "MTRL_REVSN";
        public static readonly string REVSN_NO = "REVSN_NO";
        public static readonly string BASE_REVSN_IND = "BASE_REVSN_IND";
        public static readonly string CURR_REVSN_IND = "CURR_REVSN_IND";
        public static readonly string RET_REVSN_IND = "RET_REVSN_IND";
        public static readonly string CLEI_CD = "CLEI_CD";
        public static readonly string ORDBL_MTRL_STUS_ID = "ORDBL_MTRL_STUS_ID";
        //private string revisionNumber = "";
        //private Dictionary<string, Attribute> attributes = null;
        private Dictionary<string, DatabaseDefinition> materialDefinition = null;

        public MaterialRevision()
        {
        }

        public MaterialRevision(long materialItemId) : base(materialItemId)
        {
            
        }

        public MaterialRevision(long materialItemId, long materialId) : base(materialItemId, materialId)
        {
            
        }

        public abstract string RevisionTableName
        {
            get;
        }

        [MaterialJsonProperty(MaterialType.JSON.HasRvsns, true, "")]
        public override bool HasRevisions
        {
            get;
            set;
        }

        public async Task<List<MaterialItem>> GetRevisions()
        {
            List<MaterialItem> revisions = null;

            await Task.Run(() =>
            {
                revisions = DbInterface.GetRevisions(MaterialId, RevisionTableName);
            });

            return revisions;
        }

        public bool RevisionDataChanged(string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, ref bool notifyNDS)
        {
            bool didChange = false;

            if (RevisionNumberChanged(revisionNumber) || !BaseRevisionInd.Equals(baseRevisionInd) || !CurrentRevisionInd.Equals(currentRevisionInd) || !RetiredRevisionInd.Equals(retiredRevisionInd) || OrderableMaterialStatusId != orderableMaterialStatusId)
                didChange = true;

            if (string.IsNullOrEmpty(CLEI) && string.IsNullOrEmpty(clei))
            {
                //do nothing
            }
            else if(!CLEI.Equals(clei))
            {
                didChange = true;
                notifyNDS = true;
            }

            return didChange;
        }

        private bool RevisionNumberChanged(string revisionNumber)
        {
            bool didChange = false;

            if (string.IsNullOrEmpty(RevisionNumber) && !string.IsNullOrEmpty(revisionNumber))
                didChange = true;
            else if (!string.IsNullOrEmpty(RevisionNumber) && string.IsNullOrEmpty(revisionNumber))
                didChange = true;
            else if (!string.IsNullOrEmpty(RevisionNumber) && !string.IsNullOrEmpty(revisionNumber))
            {
                if (!RevisionNumber.Equals(revisionNumber))
                    didChange = true;
            }

            return didChange;
        }

        [MaterialJsonProperty(MaterialType.JSON.Rvsn)]
        public string RevisionNumber
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.BaseRvsnInd)]
        public string BaseRevisionInd
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.CurrRvsnInd)]
        public string CurrentRevisionInd
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.RetRvsnInd)]
        public string RetiredRevisionInd
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.CLEI)]
        public string CLEI
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.OrdblId)]
        public int OrderableMaterialStatusId
        {
            get;
            set;
        }

        public override Dictionary<string, DatabaseDefinition> MaterialDefinition
        {
            get
            {
                if (materialDefinition == null)
                {
                    materialDefinition = base.MaterialDefinition;

                    materialDefinition.Add(MaterialType.JSON.Rvsn, new DatabaseDefinition(MTRL_REVSN_TABLE, REVSN_NO));
                    materialDefinition.Add(MaterialType.JSON.BaseRvsnInd, new DatabaseDefinition(MTRL_REVSN_TABLE, BASE_REVSN_IND));
                    materialDefinition.Add(MaterialType.JSON.CurrRvsnInd, new DatabaseDefinition(MTRL_REVSN_TABLE, CURR_REVSN_IND));
                    materialDefinition.Add(MaterialType.JSON.RetRvsnInd, new DatabaseDefinition(MTRL_REVSN_TABLE, RET_REVSN_IND));
                    materialDefinition.Add(MaterialType.JSON.OrdblId, new DatabaseDefinition(MTRL_REVSN_TABLE, ORDBL_MTRL_STUS_ID));
                    materialDefinition.Add(MaterialType.JSON.CLEI, new DatabaseDefinition(MTRL_REVSN_TABLE, CLEI_CD));
                }

                return materialDefinition;
            }
        }
    }
}