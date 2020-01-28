using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class MaterialFactory
    {
        private MaterialFactory()
        {
        }

        public static IMaterial GetMaterialInstance(long materialItemId, NameValueCollection attributes)
        {
            IMaterial material = null;
            IMaterialDbInterface dbInterface = null;
            long mtrlId = 0;

            //If creating a new RO material set mtrl_id = -1 before passing attributes into this method.

            //values.Add("mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
            //values.Add("mtrl_cat_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cat_id", true));
            //values.Add("feat_typ_id", DataReaderHelper.GetNonNullValue(reader, "feat_typ_id", true));
            //values.Add("rcrds_only_ind", DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind"));
            //values.Add("record_only_ind", DataReaderHelper.GetNonNullValue(reader, "record_only_ind"));

            if (attributes != null)
            {
                long.TryParse(attributes["mtrl_id"], out mtrlId);

                switch (attributes["mtrl_id"])
                {
                    case "0":
                        material = new MaterialItem(materialItemId);                        

                        break;
                    default:
                        switch (attributes["mtrl_cat_id"])
                        {
                            case "1":
                                dbInterface = new HighLevelPartDbInterface();

                                break;
                            case "2":
                                dbInterface = new MinorMaterialDbInterface();

                                break;
                            case "3":
                                switch (attributes["feat_typ_id"])
                                {
                                    case "1":
                                        dbInterface = new BayExtenderDbInterface();

                                        break;
                                    case "2":
                                        dbInterface = new BayDbInterface();

                                        break;
                                    case "5":
                                        dbInterface = new NodeDbInterface();

                                        break;
                                    case "6":
                                        dbInterface = new ShelfDbInterface();

                                        break;
                                    case "7":
                                        dbInterface = new CardDbInterface();

                                        break;
                                    case "8":
                                        dbInterface = new PlugInDbInterface();

                                        break;
                                    case "9":
                                        dbInterface = new ConnectorizedCableDbInterface();

                                        break;
                                    case "10":
                                        dbInterface = new BulkCableDbInterface();

                                        break;
                                    case "11":
                                        dbInterface = new VariableLengthCableDbInterface();

                                        break;
                                    case "12":
                                    case "13":
                                    case "14":
                                    case "15":
                                    case "16":
                                    case "17":
                                    case "18":
                                    case "19":
                                    case "20":
                                    case "21":
                                    case "22":
                                    case "23":
                                    case "24":
                                    case "25":
                                    case "26":
                                    case "27":
                                    case "28":
                                    case "29":
                                    case "30":
                                        dbInterface = new NonRackMountedEquipmentDbInterface();

                                        break;
                                }

                                break;
                            default:
                                material = new MaterialItem(materialItemId);
                                break;
                        }

                        if(material == null && dbInterface != null)
                            material = dbInterface.GetMaterial(materialItemId, mtrlId);

                        break;
                }

                if ("Y".Equals(attributes["record_only_ind"]) || "Y".Equals(attributes["rcrds_only_ind"]))
                    material.IsRecordOnly = true;

                if (dbInterface != null)
                {
                    dbInterface.GetReplacementMaterialLabel(material);

                    dbInterface.HasRevisions(material);
                }
                else
                {
                    Models.Attribute hasRvsns = new Models.Attribute(MaterialType.JSON.HasRvsns, false, "unset");
                    Models.Attribute rtPrtNbr = new Models.Attribute(MaterialType.JSON.RtPrtNbr, "");
                    Models.Attribute ftrTyp = new Models.Attribute(MaterialType.JSON.FtrTyp, "");
                    ReferenceDbInterface refDbInterface = new ReferenceDbInterface();

                    Task t = Task.Run(async () =>
                    {
                        ftrTyp.Options = await refDbInterface.GetListOptionsForAttribute(ftrTyp.Name, null);
                    });

                    t.Wait();

                    hasRvsns.IsEditable = false;

                    material.Attributes.Add(hasRvsns.Name, hasRvsns);
                    material.Attributes.Add(rtPrtNbr.Name, rtPrtNbr);
                    material.Attributes.Add(ftrTyp.Name, ftrTyp);
                }
            }

            return material;
        }

        public static IMaterial GetMaterialClass(long materialItemId, int materialCategoryId, int featureTypeId)
        {
            return GetMaterialClass(materialItemId, 0, materialCategoryId, featureTypeId);
        }

        public static IMaterial GetMaterialClass(int materialCategoryId, int featureTypeId)
        {
            return GetMaterialClass(0, 0, materialCategoryId, featureTypeId);
        }

        public static IMaterial GetMaterialClass(int materialCategoryId, int featureTypeId, long materialId)
        {
            return GetMaterialClass(0, materialId, materialCategoryId, featureTypeId);
        }

        public static IMaterial GetMaterialClass(long materialItemId, long materialId, int materialCategoryId, int featureTypeId)
        {
            IMaterial material = null;
            

            switch (materialCategoryId)
            {
                case 1:
                    material = new HighLevelPart(materialItemId, materialId);

                    break;
                case 2:
                    material = new MinorMaterial(materialItemId, materialId);

                    break;
                case 3:
                    switch (featureTypeId)
                    {
                        case 1:
                            material = new BayExtender(materialItemId, materialId);

                            break;
                        case 2:
                            material = new Bay(materialItemId, materialId);

                            break;
                        case 5:
                            material = new Node(materialItemId, materialId);

                            break;
                        case 6:
                            material = new Shelf(materialItemId, materialId);

                            break;
                        case 7:
                            material = new Card(materialItemId, materialId);

                            break;
                        case 8:
                            material = new PlugIn(materialItemId, materialId);

                            break;
                        case 9:
                            material = new ConnectorizedCable(materialItemId, materialId);

                            break;
                        case 10:
                            material = new BulkCable(materialItemId, materialId);

                            break;
                        case 11:
                            material = new VariableLengthCable(materialItemId, materialId);

                            break;
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                            material = new NonRackMountedEquipment(materialItemId, materialId, featureTypeId);

                            break;
                    }

                    break;
                default:
                    material = new MaterialItem(materialItemId);
                    break;
            }            

            return material;
        }
    }
}