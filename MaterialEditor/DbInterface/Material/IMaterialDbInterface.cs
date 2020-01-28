using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public interface IMaterialDbInterface
    {
        //Change this to return void and pass IMaterial as the only parameter????
        //Remove MaterialDefiniton from IMaterial????
        IMaterial GetMaterial(long materialItemId, long mtrlId);

        void StartTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void Dispose();

        long InsertWorkToDo(long materialItemId, string workType, string tableName);

        long InsertMaterialItem(string productId, long iesProdId, string recordOnlyInd, string sourceOfRecord, string recordOnlyPublishedInd);

        void DeleteMaterialItemAttributes(long materialItemId, long materialItemAttributesDefId);

        void InsertMaterialItemAttributes(string updatedAttributeValue, long materialItemId, string updatedAttributeName, string cuid);

        void UpdateMaterialItemAttributes(string updatedAttributeValue, long materialItemId, long materialItemAttributesDefId, string cuid);

        void InsertUpdateMaterialItemAttributes(string attributeValue, long materialItemId, string attributeName, string cuid);

        void UpdateBaseMaterial(long materialItemId, long mtrlId, string rootPartNumber, long manufacturerId, string catalogDescription, int laborId);

        void GetReplacementMaterialLabel(IMaterial material);

        List<MaterialItem> GetReplacementMaterialInfo(long materialItemId);

        List<MaterialItem> GetChainingMaterialInfo(long materialItemId);

        void HasRevisions(IMaterial material);

        List<MaterialItem> GetRevisions(long materialId, string tableName);
    }
}
