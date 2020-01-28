using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class HighLevelPartNDSHelper
    {
        private string materialCode = "";
        private long macroAssemblyDefinitionId = 0;
        private long materialItemId = 0;
        private List<long> containedInWorkToDoIds = null;
        private Dictionary<string, ContainedInItem> toDoItems = null;

        private HighLevelPartNDSHelper()
        {
        }

        public HighLevelPartNDSHelper(string mtrlCd)
        {
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();

            materialCode = mtrlCd;

            dbInterface.GetMacroAssemblyDefinitionId(this);
        }

        public void AddToDoItem(ContainedInItem item)
        {
            if (toDoItems == null)
                toDoItems = new Dictionary<string, ContainedInItem>();

            if (!toDoItems.ContainsKey(item.DefinitionId))
                toDoItems.Add(item.DefinitionId, item);
        }

        public void ProcessToDoItems()
        {
            HighLevelPartDbInterface dbInterface = null;

            if (toDoItems != null && materialItemId > 0)
            {
                dbInterface = new HighLevelPartDbInterface();

                MasterWorkToDoId = dbInterface.InsertWorkToDo(materialItemId, 0, "CATALOG_HLPN", IsNewMacroAssembly ? "INSERT" : "UPDATE", "STAGED");

                foreach (string defId in toDoItems.Keys)
                {
                    string action = "";
                    string workType = "CATALOG_HLPN_MA_CU";
                    string ndsTableName = "ma_cu_quantity_definition";
                    long id = 0;
                    bool quantityDefinitionExists = false;

                    action = toDoItems[defId].Action;

                    if (toDoItems[defId].MaterialCategory.Equals("High Level Part"))
                    {
                        workType = "CATALOG_HLPN_MA";
                        ndsTableName = "ma_quantity_definition";
                    }                    

                    if (long.TryParse(defId, out id))
                    {
                        quantityDefinitionExists = dbInterface.QuantityDefinitionExists(id, ndsTableName);

                        if (toDoItems[defId].Action.Equals("DELETE"))
                        {
                            if (IsNewMacroAssembly)
                            {
                                //skip
                            }
                            else
                            {
                                if (containedInWorkToDoIds == null) containedInWorkToDoIds = new List<long>();
                                containedInWorkToDoIds.Add(dbInterface.InsertWorkToDo(id, MasterWorkToDoId, workType, "DELETE", "STAGED"));
                            }
                        }
                        else
                        {
                            if (quantityDefinitionExists)
                                action = "UPDATE";
                            else
                                action = "INSERT";
                            

                            if (containedInWorkToDoIds == null)
                                containedInWorkToDoIds = new List<long>();

                            containedInWorkToDoIds.Add(dbInterface.InsertWorkToDo(id, MasterWorkToDoId, workType, action, "STAGED"));
                        }
                    }
                }
            }
        }

        public bool IsNewMacroAssembly
        {
            get
            {
                if (macroAssemblyDefinitionId > 0)
                    return false;
                else
                    return true;
            }
        }

        public List<long> ContainedInWorkToDoIds
        {
            get
            {
                return containedInWorkToDoIds;
            }
        }

        public string MaterialCode
        {
            get
            {
                return materialCode;
            }
        }

        public long MacroAssemblyDefinitionId
        {
            get
            {
                return macroAssemblyDefinitionId;
            }
            set
            {
                macroAssemblyDefinitionId = value;
            }
        }

        public long MaterialItemId
        {
            get
            {
                return materialItemId;
            }
            set
            {
                materialItemId = value;
            }
        }

        public long MasterWorkToDoId
        {
            get;
            set;
        }

        public Dictionary<string, ContainedInItem> ToDoItems
        {
            get
            {
                return toDoItems;
            }
        }
    }

    public class ContainedInItem
    {
        private string definitionId = "";
        private string materialCategory = "";
        private string action = "";
        private string materialItemId = "";

        public ContainedInItem()
        {
        }

        public ContainedInItem(string defId, string mtrlCtgry, string action, string mtlItmId)
        {
            definitionId = defId;
            materialCategory = mtrlCtgry;
            this.action = action;
            materialItemId = mtlItmId;
        }

        public string MaterialItemId
        {
            get
            {
                return materialItemId;
            }
            set
            {
                materialItemId = value;
            }
        }

        public string DefinitionId
        {
            get
            {
                return definitionId;
            }
            set
            {
                definitionId = value;
            }
        }

        public string MaterialCategory
        {
            get
            {
                return materialCategory;
            }
            set
            {
                materialCategory = value;
            }
        }

        public string Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }
    }
}