using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.TypeLibrary;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class MaterialItemManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private enum DATABASE_ACTION { NONE, INSERT, UPDATE, DELETE };

        public MaterialItemManager()
        {
        }

        public string MaterialSpecificationType
        {
            get;
            set;
        }

        public async Task<IMaterial> GetActiveMaterialItemAsync(long materialItemId, string source)
        {
            IMaterial activeMaterial = null;
            MaterialItem losdbMaterial = null;
            MaterialItem sapMaterial = null;
            MaterialItem recordOnlyMaterial = null;
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            MaterialDbInterface sapDbInterface = new MaterialDbInterface();
            MaterialDbInterface additionalAttributesDbInterface = new MaterialDbInterface();
            MaterialDbInterface overrideAttributesDbInterface = new MaterialDbInterface();
            MaterialItemManager manager = new MaterialItemManager();
            LOSDBMaterialManager losdbManager = null;
            string sourceOfRecord = string.Empty;
            List<Task> tasks = new List<Task>();
            List<Models.Attribute> additionalAttributes = null;
            List<Models.Attribute> overrideAttributes = null;
            MaterialItem activeItem = null;

            //How to determine the active material:
            //1. Default values are always from SAP.
            //2. Check for the source of record in material_item.
            //  a. If it is LOSDB, the LOSDB material overrides equivalent SAP values and can not be manually overridden.
            //3. SAP values (which are not also LOSDB values if source of record is LOSDB) may be overridden manually. Values are stored in material_item_attributes.
            //4. NDS and COEFM additional attributes. Values are stored in material_item_attributes.

            if ("active".Equals(source))
            {
                sourceOfRecord = "SAP";
                sourceOfRecord = dbInterface.GetMaterialSourceOfRecord(materialItemId);
            }
            else if ("ro".Equals(source) && materialItemId == 0)
            {
                return GetRecordOnlyMaterial();
            }

            await Task.Run(() =>
            {
                try
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        NameValueCollection materialProperties = await dbInterface.GetMaterialIdCategoryAndFeatureTypesAsync(materialItemId);

                        activeMaterial = MaterialFactory.GetMaterialInstance(materialItemId, materialProperties);
                    }));

                    if ("active".Equals(source))
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            sapMaterial = await sapDbInterface.GetMaterialItemSAPAsync(materialItemId);
                        }));

                        tasks.Add(Task.Run(async () =>
                        {
                            losdbManager = new LOSDBMaterialManager();

                            losdbMaterial = await losdbManager.GetMaterialItemAsync(materialItemId);
                        }));
                    }
                    else if ("ro".Equals(source))
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            recordOnlyMaterial = await sapDbInterface.GetMaterialItemRecordOnlyAsync(materialItemId);
                        }));
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        overrideAttributes = await overrideAttributesDbInterface.GetAttributeOverridesAsync(materialItemId, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP));
                    }));

                    tasks.Add(Task.Run(async () =>
                    {
                        additionalAttributes = await additionalAttributesDbInterface.GetAdditionalAttributesAsync(materialItemId);
                    }));                    

                    Task.WaitAll(tasks.ToArray());
                }
                catch (AggregateException ae)
                {
                    logger.Error("Material item id {0}: " + ae.Message, materialItemId);
                }

                try
                {
                    if (losdbMaterial != null && losdbMaterial.Attributes != null)
                    {
                        if (activeItem == null)
                        {
                            activeItem = new MaterialItem(materialItemId);

                            activeItem.Attributes = losdbMaterial.Attributes;
                            activeItem.CompatibleSevenCLEIItem = losdbMaterial.CompatibleSevenCLEIItem;
                        }

                        if ("LOSDB".Equals(sourceOfRecord) && activeItem.Attributes != null)
                        //else if ("LOSDB".Equals(sourceOfRecord))
                        {
                            if (losdbMaterial.Attributes.ContainsKey("EqptDscr"))
                            {
                                if (activeItem.Attributes.ContainsKey("ItmDesc"))
                                {
                                    activeItem.Attributes["ItmDesc"].Value = losdbMaterial.Attributes["EqptDscr"].Value;
                                    activeItem.Attributes["ItmDesc"].Source = sourceOfRecord;
                                }
                                else
                                {
                                    Models.Attribute attr = new Models.Attribute("ItmDesc", losdbMaterial.Attributes["EqptDscr"].Value, MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB));

                                    attr.Source = sourceOfRecord;

                                    activeItem.Attributes.Add(attr.Name, attr);
                                }
                            }

                            if (losdbMaterial.Attributes.ContainsKey("MfgNm"))
                            {
                                if (activeItem.Attributes.ContainsKey("MfgDesc"))
                                {
                                    activeItem.Attributes["MfgDesc"].Value = losdbMaterial.Attributes["MfgNm"].Value;
                                    activeItem.Attributes["MfgDesc"].Source = sourceOfRecord;
                                }
                                else
                                {
                                    Models.Attribute attr = new Models.Attribute("MfgDesc", losdbMaterial.Attributes["MfgNm"].Value, MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB));

                                    attr.Source = sourceOfRecord;

                                    activeItem.Attributes.Add(attr.Name, attr);
                                }
                            }
                            if (losdbMaterial.Attributes.ContainsKey("Mfg"))
                            {
                                if (activeItem.Attributes.ContainsKey("Mfg"))
                                {
                                    activeItem.Attributes["Mfg"].Value = losdbMaterial.Attributes["Mfg"].Value;
                                    activeItem.Attributes["Mfg"].Source = sourceOfRecord;
                                }
                                else
                                {
                                    Models.Attribute attr = new Models.Attribute("Mfg", losdbMaterial.Attributes["Mfg"].Value, MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB));

                                    attr.Source = sourceOfRecord;

                                    activeItem.Attributes.Add(attr.Name, attr);
                                }
                            }

                            if (losdbMaterial.Attributes.ContainsKey("CLEI"))
                            {
                                if (activeItem.Attributes.ContainsKey("HECI"))
                                {
                                    activeItem.Attributes["HECI"].Value = losdbMaterial.Attributes["CLEI"].Value;
                                    activeItem.Attributes["HECI"].Source = sourceOfRecord;
                                }
                                else
                                {
                                    Models.Attribute attr = new Models.Attribute("HECI", losdbMaterial.Attributes["CLEI"].Value, MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB));

                                    attr.Source = sourceOfRecord;

                                    activeItem.Attributes.Add(attr.Name, attr);
                                }
                            }
                        }
                    }

                    if (sapMaterial != null && sapMaterial.Attributes != null)
                    {
                        //activeMaterial = new MaterialItem(materialItemId);
                        if (activeMaterial.Attributes == null || activeMaterial.Attributes.Count == 0)
                            activeMaterial.Attributes = sapMaterial.Attributes;
                        else
                        {
                            foreach (string key in sapMaterial.Attributes.Keys)
                            {
                                
                                if (!activeMaterial.Attributes.ContainsKey(key))
                                {
                                    Models.Attribute attr = null;

                                    if (MaterialType.JSON.HzrdInd.Equals(key))
                                        attr = new Models.Attribute(key, sapMaterial.Attributes[key].BoolValue, sapMaterial.Attributes[key].Source);
                                    else
                                        attr = new Models.Attribute(key, sapMaterial.Attributes[key].Value, sapMaterial.Attributes[key].Source);

                                    attr.MaterialItemAttributesDefId = sapMaterial.Attributes[key].MaterialItemAttributesDefId;

                                    activeMaterial.Attributes.Add(key, attr);
                                }
                            }
                        }

                        if (activeMaterial.Attributes.ContainsKey("MfgDesc") && activeMaterial.Attributes.ContainsKey("MfgNm"))
                        {
                            activeMaterial.Attributes["MfgDesc"].Value = activeMaterial.Attributes["MfgNm"].Value;
                        }
                        // Get the correct manufacturer name from the mfr table in case it is different than the original SAP manufacturer name
                        if (activeMaterial.Attributes.ContainsKey("MfgId") && activeMaterial.Attributes.ContainsKey("MfgDesc"))
                        {
                            activeMaterial.Attributes["MfgDesc"].Value = dbInterface.GetManufacturerName(int.Parse(activeMaterial.Attributes["MfgId"].Value));
                        }

                        // Check to see if LOSDB material has overriding attributes
                        if ("LOSDB".Equals(sourceOfRecord) && activeItem != null && activeItem.Attributes != null)
                        {
                            #region commented out
                            //bool foundOne = false;
                            if (activeItem.Attributes.ContainsKey("Dpth") && activeItem.Attributes["Dpth"].Value != "0")
                            {
                                activeMaterial.Attributes["Dpth"].Value = activeItem.Attributes["Dpth"].Value;
                                //foundOne = true;
                            }
                            if (activeItem.Attributes.ContainsKey("Wdth") && activeItem.Attributes["Wdth"].Value != "0")
                                {
                                activeMaterial.Attributes["Wdth"].Value = activeItem.Attributes["Wdth"].Value;
                                //foundOne = true;
                                }
                            if (activeItem.Attributes.ContainsKey("Hght") && activeItem.Attributes["Hght"].Value != "0")
                            {
                                activeMaterial.Attributes["Hght"].Value = activeItem.Attributes["Hght"].Value;
                                //foundOne = true;
                            }
                            //if (foundOne)
                            //{
                            //    if (activeItem.Attributes.ContainsKey("DimUom") && activeItem.Attributes["DimUom"].Value != String.Empty
                            //        && activeItem.Attributes["DimUom"].Value != "NA" && activeItem.Attributes["DimUom"].Value != "NS")
                            //    {
                            //        if (!activeMaterial.Attributes.ContainsKey("DimUom"))
                            //{
                            //            activeMaterial.Attributes.Add("DimUom", activeItem.Attributes["DimUom"]);
                            //}
                            //        else
                            //{
                            //            //activeMaterial.Attributes["DimUom"].Value = activeItem.Attributes["DimUom"].Value;
                            //        }
                            //    }
                            //}

                            //TJV 12/6/18 - commented out
                            //if (activeItem.Attributes.ContainsKey("Wght") && activeItem.Attributes["Wght"].Value != "0")
                            //{
                            //    if (activeMaterial.Attributes.ContainsKey("Wght"))
                            //{
                            //        activeMaterial.Attributes["Wght"].Value = activeItem.Attributes["Wght"].Value;
                            //        if (activeItem.Attributes.ContainsKey("WghtUom") && activeItem.Attributes["WghtUom"].Value != String.Empty
                            //            && activeItem.Attributes["WghtUom"].Value != "NA" && activeItem.Attributes["WghtUom"].Value != "NS")
                            //{
                            //            if (activeMaterial.Attributes.ContainsKey("WghtUom"))
                            //    {
                            //                activeMaterial.Attributes["WghtUom"].Value = activeItem.Attributes["WghtUom"].Value;
                            //            }
                            //        }
                            //    }
                            //}
                            #endregion

                            if (activeItem.Attributes.ContainsKey("Wght") && activeItem.Attributes["Wght"].Value != String.Empty)
                            {
                                if (activeMaterial.Attributes.ContainsKey("Wght"))
                                {
                                    activeMaterial.Attributes["Wght"].Value = activeItem.Attributes["Wght"].Value;
                                }
                                    if (activeItem.Attributes.ContainsKey("WghtUom") && activeItem.Attributes["WghtUom"].Value != String.Empty
                                        && activeItem.Attributes["WghtUom"].Value != "NA" && activeItem.Attributes["WghtUom"].Value != "NS")
                                    {
                                        if (activeMaterial.Attributes.ContainsKey("WghtUom"))
                                        {
                                            activeMaterial.Attributes["WghtUom"].Value = activeItem.Attributes["WghtUom"].Value;
                                        }
                                    }
                                }

                            if (activeItem.Attributes.ContainsKey("EqptDscr") && activeItem.Attributes["EqptDscr"].Value != String.Empty)
                            {
                                activeMaterial.Attributes["ItmDesc"].Value = activeItem.Attributes["EqptDscr"].Value;
                            }
                            if (activeItem.Attributes.ContainsKey("MfgNm") && activeItem.Attributes["MfgNm"].Value != String.Empty)
                            {
                                activeMaterial.Attributes["MfgDesc"].Value = activeItem.Attributes["MfgNm"].Value;
                            }
                            if (activeItem.Attributes.ContainsKey("Mfg") && activeItem.Attributes["Mfg"].Value != String.Empty)
                            {
                                activeMaterial.Attributes["Mfg"].Value = activeItem.Attributes["Mfg"].Value;
                            }
                            if (activeItem.Attributes.ContainsKey("CLEI") && activeItem.Attributes["CLEI"].Value != String.Empty)
                            {
                                activeMaterial.Attributes["HECI"].Value = activeItem.Attributes["CLEI"].Value;
                            }
                            if (activeItem.CompatibleSevenCLEIItem != null && activeItem.CompatibleSevenCLEIItem.CompatibleClei7 != null)
                            {
                                if (activeItem.CompatibleSevenCLEIItem.CompatibleClei7.Count > 0)
                                {
                                    activeMaterial.Attributes["HECI"].Value = activeItem.CompatibleSevenCLEIItem.CompatibleClei7[0].ToString();
                                }
                                else
                                {
                                    if (activeItem.Attributes["CLEI"].Value != String.Empty)
                                    {
                                        try
                                        {
                                            activeMaterial.Attributes["HECI"].Value = activeItem.Attributes["CLEI"].Value.Substring(0, 7);
                                        }
                                        catch { }
                                    }
                                }
                            }
                            if (activeItem.CompatibleSevenCLEIItem.CompatibleClei7 == null)
                            {
                                if (activeItem.Attributes["CLEI"].Value != String.Empty)
                                {
                                    try
                                    {
                                        activeMaterial.Attributes["HECI"].Value = activeItem.Attributes["CLEI"].Value.Substring(0, 7);
                                    }
                                    catch { }
                                }
                            }

                            if (activeItem.Attributes.ContainsKey(MaterialType.JSON.HzrdInd) && activeItem.Attributes[MaterialType.JSON.HzrdInd].Value != String.Empty)
                            {
                                if (activeItem.Attributes[MaterialType.JSON.HzrdInd].Value == "0" || activeItem.Attributes[MaterialType.JSON.HzrdInd].Value == "N")
                                {
                                    activeMaterial.Attributes[MaterialType.JSON.HzrdInd].BoolValue = false;
                                }
                                else
                                {
                                    activeMaterial.Attributes[MaterialType.JSON.HzrdInd].BoolValue = true;
                                }
                            }
                        }

                        if (overrideAttributes != null)
                        {
                            foreach (Models.Attribute attr in overrideAttributes)
                            {
                                if (activeMaterial.Attributes.ContainsKey(attr.Name))
                                {
                                    if (MaterialType.JSON.HzrdInd.Equals(attr.Name))
                                        activeMaterial.Attributes[attr.Name].BoolValue = attr.BoolValue;
                                    else
                                        activeMaterial.Attributes[attr.Name].Value = attr.Value;

                                    activeMaterial.Attributes[attr.Name].MaterialItemAttributesDefId = attr.MaterialItemAttributesDefId;
                                    activeMaterial.Attributes[attr.Name].Source = attr.Source;
                                }
                            }
                        }
                        if (activeMaterial.MaterialCategoryId == 3 && activeMaterial.SpecificationId != 0)  // check only if there is an associated spec
                        {
                            // check to see if there are overriding dimeensions (the topmost level of overrides) in the spec
                            if ( activeMaterial.FeatureTypeId  <= 7) //1 bay extender, 2 bay, 5 node, 6 shelf, 7 card
                            {
                                StringCollection dimensions = new StringCollection();
                                SpecificationDbInterface specDbInterface = new SpecificationDbInterface();
                                dimensions = specDbInterface.GetSpecDimensions(activeMaterial.MaterialId, activeMaterial.FeatureTypeId);
                                if (dimensions.Count == 3)
                                {
                                    if (dimensions[0].ToString() != "0")
                                    {
                                        activeMaterial.Attributes["Dpth"].Value = dimensions[0].ToString();
                                        activeMaterial.Attributes["Wdth"].Value = dimensions[1].ToString();
                                        activeMaterial.Attributes["Hght"].Value = dimensions[2].ToString();
                                    }
                                }
                            }
                        }
                    }
                    else if ("ro".Equals(source) || activeMaterial.IsRecordOnly)
                    {
                        string[] results = sapDbInterface.GetProductId(materialItemId);

                        if (results != null)
                        {
                            activeMaterial.MaterialCode = results[0];
                            activeMaterial.IsRecordOnlyPublished = "Y".Equals(results[1]) ? true : false; 

                            if(!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.PrdctId))
                                activeMaterial.Attributes.Add(MaterialType.JSON.PrdctId, new Models.Attribute(MaterialType.JSON.PrdctId, activeMaterial.MaterialCode));

                            if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.ROPblshd))
                                activeMaterial.Attributes.Add(MaterialType.JSON.ROPblshd, new Models.Attribute(MaterialType.JSON.ROPblshd, activeMaterial.IsRecordOnlyPublished));
                        }

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.RtPrtNbr))
                        {
                            if (activeMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                activeMaterial.Attributes.Add(MaterialType.JSON.RtPrtNbr, new Models.Attribute(MaterialType.JSON.RtPrtNbr, activeMaterial.Attributes[MaterialType.JSON.PrtNo].Value));
                            else
                                activeMaterial.Attributes.Add(MaterialType.JSON.RtPrtNbr, new Models.Attribute(MaterialType.JSON.RtPrtNbr, ""));
                        }

                        if (recordOnlyMaterial != null && recordOnlyMaterial.Attributes != null)
                        {
                            foreach (string key in recordOnlyMaterial.Attributes.Keys)
                            {
                                if (!activeMaterial.Attributes.ContainsKey(key))
                                {
                                    Models.Attribute attr = null;

                                    if (MaterialType.JSON.HzrdInd.Equals(key))
                                        attr = new Models.Attribute(key, recordOnlyMaterial.Attributes[key].BoolValue);
                                    else
                                        attr = new Models.Attribute(key, recordOnlyMaterial.Attributes[key].Value);

                                    activeMaterial.Attributes.Add(key, attr);
                                }
                            }
                        }

                        if (activeMaterial.RecordOnlyAttributeNames != null)
                        {
                            foreach (string key in activeMaterial.RecordOnlyAttributeNames)
                            {
                                if (!activeMaterial.Attributes.ContainsKey(key))
                                {
                                    Models.Attribute attr = null;

                                    if (MaterialType.JSON.HzrdInd.Equals(key))
                                        attr = new Models.Attribute(key, false);
                                    else
                                        attr = new Models.Attribute(key, "");

                                    activeMaterial.Attributes.Add(key, attr);
                                }
                            }
                        }

                        //if(!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.UOM))
                        //    activeMaterial.Attributes.Add(MaterialType.JSON.UOM, new Models.Attribute(MaterialType.JSON.UOM, ""));

                        //if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.HzrdInd))
                        //    activeMaterial.Attributes.Add(MaterialType.JSON.HzrdInd, new Models.Attribute(MaterialType.JSON.HzrdInd, false));

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.RO))
                            activeMaterial.Attributes.Add(MaterialType.JSON.RO, new Models.Attribute(MaterialType.JSON.RO, true));
                        else if (!activeMaterial.Attributes[MaterialType.JSON.RO].BoolValue)
                            activeMaterial.Attributes[MaterialType.JSON.RO].BoolValue = true;

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.Hght))
                            activeMaterial.Attributes.Add(MaterialType.JSON.Hght, new Models.Attribute(MaterialType.JSON.Hght, "0"));

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.Dpth))
                            activeMaterial.Attributes.Add(MaterialType.JSON.Dpth, new Models.Attribute(MaterialType.JSON.Dpth, "0"));

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.Wdth))
                            activeMaterial.Attributes.Add(MaterialType.JSON.Wdth, new Models.Attribute(MaterialType.JSON.Wdth, "0"));

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.MtrlId))
                            activeMaterial.Attributes.Add(MaterialType.JSON.MtrlId, new Models.Attribute(MaterialType.JSON.MtrlId, "0"));

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.MfgId))
                        {
                            if (activeMaterial.Attributes.ContainsKey(MaterialType.JSON.Mfg))
                            {
                                int id = ReferenceDbInterface.GetManufacturerId(activeMaterial.Attributes[MaterialType.JSON.Mfg].Value);

                                activeMaterial.Attributes.Add(MaterialType.JSON.MfgId, new Models.Attribute(MaterialType.JSON.MfgId, id.ToString()));
                            }
                            else
                                activeMaterial.Attributes.Add(MaterialType.JSON.MfgId, new Models.Attribute(MaterialType.JSON.MfgId, "0"));
                        }

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.CLEI))
                            activeMaterial.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, ""));

                        if(activeMaterial.Attributes[MaterialType.JSON.MtrlId].Value == "0")
                            activeMaterial.Attributes[MaterialType.JSON.CLEI].IsEditable = false;
                        else
                        {
                            if(activeMaterial is VariableLengthCable)
                                activeMaterial.Attributes[MaterialType.JSON.CLEI].IsEditable = false;
                            else
                                activeMaterial.Attributes[MaterialType.JSON.CLEI].IsEditable = true;
                        }

                        //if (overrideAttributes != null)
                        //{
                        //    foreach (Models.Attribute attr in overrideAttributes)
                        //    {
                        //        if (!activeMaterial.Attributes.ContainsKey(attr.Name))
                        //        {
                        //            activeMaterial.Attributes.Add(attr.Name, attr);
                        //        }
                        //    }
                        //}

                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                        {
                            if (activeMaterial.Attributes.ContainsKey(MaterialType.JSON.RtPrtNbr))
                            {
                                activeMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.RtPrtNbr, activeMaterial.Attributes[MaterialType.JSON.RtPrtNbr].Value));
                            }
                        }
                        if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                        {
                            if (activeMaterial.Attributes.ContainsKey(MaterialType.JSON.CtlgDesc))
                            {
                                activeMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.CtlgDesc, activeMaterial.Attributes[MaterialType.JSON.CtlgDesc].Value));
                            }
                        }
                    }

                    if (additionalAttributes != null)
                    {
                        foreach (Models.Attribute attr in additionalAttributes)
                        {
                            if (!activeMaterial.Attributes.ContainsKey(attr.Name))
                            {
                                if (activeMaterial.AdditionalAttributeNames != null && activeMaterial.AdditionalAttributeNames.ContainsKey(attr.Name))
                                    attr.IsEditable = activeMaterial.AdditionalAttributeNames[attr.Name].IsEditable;

                                activeMaterial.Attributes.Add(attr.Name, attr);
                            }
                            else if (attr.Options != null && attr.Options.Count > 0 && (activeMaterial.Attributes[attr.Name].Options == null || activeMaterial.Attributes[attr.Name].Options.Count == 0))
                                activeMaterial.Attributes[attr.Name].Options = attr.Options;
                        }
                    }

                    if (!activeMaterial.Attributes.ContainsKey(MaterialType.JSON.CtlgDesc))
                        activeMaterial.Attributes.Add(MaterialType.JSON.CtlgDesc, new Models.Attribute(MaterialType.JSON.CtlgDesc, ""));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception occurred while getting active material [{0}]", materialItemId);

                    activeMaterial = null;
                }
            });

            return activeMaterial;
        }

        private IMaterial GetRecordOnlyMaterial()
        {
            IMaterial roMaterial = new MaterialItem();
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();
            Models.Attribute mtlCtgry = new Models.Attribute(true, MaterialType.JSON.MtlCtgry);
            Models.Attribute ftrTyp = new Models.Attribute(true, MaterialType.JSON.FtrTyp);
            Models.Attribute setLgthUom = new Models.Attribute(true, MaterialType.JSON.SetLgthUom);
            Models.Attribute lbrId = new Models.Attribute(true, MaterialType.JSON.LbrId);
            Models.Attribute stts = new Models.Attribute(true, MaterialType.JSON.Stts);
            Models.Attribute lctnPosInd = new Models.Attribute(true, MaterialType.JSON.LctnPosInd);
            Models.Attribute cblTypId = new Models.Attribute(true, MaterialType.JSON.CblTypId);
            Models.Attribute plgInRlTyp = new Models.Attribute(true, MaterialType.JSON.PlgInRlTyp);

            GetListOptions(mtlCtgry, dbInterface);
            GetListOptions(ftrTyp, dbInterface);
            GetListOptions(setLgthUom, dbInterface);
            GetListOptions(lbrId, dbInterface);
            GetListOptions(stts, dbInterface);
            GetListOptions(lctnPosInd, dbInterface);
            GetListOptions(cblTypId, dbInterface);
            GetListOptions(plgInRlTyp, dbInterface);

            roMaterial.Attributes = new Dictionary<string, Models.Attribute>();

            roMaterial.Attributes.Add(MaterialType.JSON.MaterialItemId, new Models.Attribute(false, MaterialType.JSON.MaterialItemId));
            roMaterial.Attributes.Add(MaterialType.JSON.PrdctId, new Models.Attribute(false, MaterialType.JSON.PrdctId));
            roMaterial.Attributes.Add(MaterialType.JSON.Apcl, new Models.Attribute(MaterialType.JSON.Apcl, "RO"));
            roMaterial.Attributes.Add(MaterialType.JSON.MtlCtgry, mtlCtgry);
            roMaterial.Attributes.Add(MaterialType.JSON.FtrTyp, ftrTyp);
            roMaterial.Attributes.Add(MaterialType.JSON.SpecNm, new Models.Attribute(true, MaterialType.JSON.SpecNm));
            roMaterial.Attributes.Add(MaterialType.JSON.SetLgth, new Models.Attribute(true, MaterialType.JSON.SetLgth));
            roMaterial.Attributes.Add(MaterialType.JSON.SetLgthUom, setLgthUom);
            roMaterial.Attributes.Add(MaterialType.JSON.LbrId, lbrId);
            roMaterial.Attributes.Add(MaterialType.JSON.RtPrtNbr, new Models.Attribute(true, MaterialType.JSON.RtPrtNbr));
            roMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(true, MaterialType.JSON.PrtNo));
            roMaterial.Attributes.Add(MaterialType.JSON.Mfg, new Models.Attribute(true, MaterialType.JSON.Mfg));
            roMaterial.Attributes.Add(MaterialType.JSON.MfgId, new Models.Attribute(true, MaterialType.JSON.MfgId));
            roMaterial.Attributes.Add(MaterialType.JSON.UOM, new Models.Attribute(true, MaterialType.JSON.UOM));
            roMaterial.Attributes.Add(MaterialType.JSON.CtlgDesc, new Models.Attribute(true, MaterialType.JSON.CtlgDesc));
            roMaterial.Attributes.Add(MaterialType.JSON.HzrdInd, new Models.Attribute(MaterialType.JSON.HzrdInd, false));
            roMaterial.Attributes.Add(MaterialType.JSON.Hght, new Models.Attribute(true, MaterialType.JSON.Hght));
            roMaterial.Attributes.Add(MaterialType.JSON.Wdth, new Models.Attribute(true, MaterialType.JSON.Wdth));
            roMaterial.Attributes.Add(MaterialType.JSON.Dpth, new Models.Attribute(true, MaterialType.JSON.Dpth));
            roMaterial.Attributes.Add(MaterialType.JSON.Stts, stts);
            roMaterial.Attributes.Add(MaterialType.JSON.AmpsDrn, new Models.Attribute(true, MaterialType.JSON.AmpsDrn));
            roMaterial.Attributes.Add(MaterialType.JSON.NumMtgSpcs, new Models.Attribute(true, MaterialType.JSON.NumMtgSpcs));
            roMaterial.Attributes.Add(MaterialType.JSON.MtgPltSz, new Models.Attribute(true, MaterialType.JSON.MtgPltSz));
            roMaterial.Attributes.Add(MaterialType.JSON.MxEqpPos, new Models.Attribute(true, MaterialType.JSON.MxEqpPos));
            roMaterial.Attributes.Add(MaterialType.JSON.Gge, new Models.Attribute(true, MaterialType.JSON.Gge));
            roMaterial.Attributes.Add(MaterialType.JSON.GgeUnt, new Models.Attribute(true, MaterialType.JSON.GgeUnt));
            roMaterial.Attributes.Add(MaterialType.JSON.EqpWght, new Models.Attribute(true, MaterialType.JSON.EqpWght));
            roMaterial.Attributes.Add(MaterialType.JSON.AccntCd, new Models.Attribute(MaterialType.JSON.AccntCd, "--7C"));
            roMaterial.Attributes.Add(MaterialType.JSON.PrdTyp, new Models.Attribute(true, MaterialType.JSON.PrdTyp));
            roMaterial.Attributes.Add(MaterialType.JSON.EqptCls, new Models.Attribute(true, MaterialType.JSON.EqptCls));
            roMaterial.Attributes.Add(MaterialType.JSON.LctnPosInd, lctnPosInd);
            roMaterial.Attributes.Add(MaterialType.JSON.PrtNbrTypCd, new Models.Attribute(true, MaterialType.JSON.PrtNbrTypCd));
            roMaterial.Attributes.Add(MaterialType.JSON.PosSchm, new Models.Attribute(true, MaterialType.JSON.PosSchm));
            roMaterial.Attributes.Add(MaterialType.JSON.MntPosHght, new Models.Attribute(true, MaterialType.JSON.MntPosHght));
            roMaterial.Attributes.Add(MaterialType.JSON.IntrlHght, new Models.Attribute(true, MaterialType.JSON.IntrlHght));
            roMaterial.Attributes.Add(MaterialType.JSON.IntrlDpth, new Models.Attribute(true, MaterialType.JSON.IntrlDpth));
            roMaterial.Attributes.Add(MaterialType.JSON.IntrlWdth, new Models.Attribute(true, MaterialType.JSON.IntrlWdth));
            roMaterial.Attributes.Add(MaterialType.JSON.LstDt, new Models.Attribute(true, MaterialType.JSON.LstDt));
            roMaterial.Attributes.Add(MaterialType.JSON.LstUid, new Models.Attribute(true, MaterialType.JSON.LstUid));
            roMaterial.Attributes.Add(MaterialType.JSON.RO, new Models.Attribute(MaterialType.JSON.RO, true));
            roMaterial.Attributes.Add(MaterialType.JSON.ROPblshd, new Models.Attribute(MaterialType.JSON.ROPblshd, false));
            roMaterial.Attributes.Add(MaterialType.JSON.CblTypId, cblTypId);
            roMaterial.Attributes.Add(MaterialType.JSON.PlgInRlTyp, plgInRlTyp);
            roMaterial.Attributes.Add(MaterialType.JSON.MtrlId, new Models.Attribute(MaterialType.JSON.MtrlId, "0"));
            roMaterial.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, "0"));

            return roMaterial;
        }

        private void GetListOptions(Models.Attribute attr, ReferenceDbInterface dbInterface)
        {
            Task t = Task.Run(async () =>
            {
                NameValueCollection parameters = null;

                if (attr.Name.Equals(MaterialType.JSON.LbrId))
                {
                    parameters = new NameValueCollection();

                    parameters.Add("Id", "0");
                    parameters.Add("retcsr", ReferenceDbInterface.CURSOR);
                }

                attr.Options = await dbInterface.GetListOptionsForAttribute(attr.Name, parameters);
            });

            t.Wait();
        }

        public async Task<long[]> PersistMaterialItem(JObject updatedMaterialItem, long materialItemId)
        {
            MaterialDbInterface dbInterface = null;
            IMaterial currentMaterialItem = null;
            long workToDoId = 0;
            long specificationWorkToDoId = 0;
            long mtrlId = 0;
            long[] materialIdArray = null;
            int materialCategoryId = 0;
            int featureTypeId = 0;
            int scenario = 0;
            bool notifyNDS = false;
            bool notifyNDSSpecification = false;
            string recordOnly = "";
            string recordOnlyPublished = "";

            try
            {
                //Scenarios: 
                //1. Brand new material coming from New and Updated Parts screen. A future TODO item is to convert the New and Updated Parts screen to follow this flow as well.
                //2. Brand new Record Only material.
                //3. Update to an existing material. This is an old material that is NOT being converted to the new MTRL tables.
                //4. Update to an existing material. This is an old material that IS being converted to the new MTRL tables.
                //5. Update to an existing material. This is a material that is currently in the new MTRL tables.

                recordOnly = ((string)updatedMaterialItem.SelectToken(MaterialType.JSON.RO + ".bool")).ToLower();
                recordOnlyPublished = ((string)updatedMaterialItem.SelectToken(MaterialType.JSON.ROPblshd + ".bool")).ToLower();
                mtrlId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MtrlId);
                materialCategoryId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MtlCtgry);
                featureTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.FtrTyp);

                if (mtrlId > 0)
                    scenario = 5;
                else if (materialItemId > 0)
                {
                    if (materialCategoryId == 1 || materialCategoryId == 2 || (materialCategoryId == 3 && featureTypeId > 0))
                        scenario = 4;
                    else
                        scenario = 3;
                }
                else if (materialItemId == 0) //Material is coming from either New and Updated Parts or Record Only and it is a brand new material
                {
                    if ("true".Equals(recordOnly))
                        scenario = 2;
                    else
                    {
                        //Note: Currently we will only be receiving items from Record Only screen. 
                        string productId = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrdctId);

                        if (string.IsNullOrEmpty(productId))
                        {
                            logger.Error("Unable to parse product id from JSON: {0}", updatedMaterialItem.ToString());

                            throw new Exception();
                        }

                        scenario = 1;
                    }
                }

                if (scenario == 1 || scenario == 2 || scenario == 4)
                {
                    if (scenario == 4)
                        ValidateUniqueRootPartNumber(updatedMaterialItem);

                    IMaterial updatedMaterial = MaterialFactory.GetMaterialClass(materialItemId, materialCategoryId, featureTypeId);

                    materialIdArray = updatedMaterial.PersistObject(currentMaterialItem, updatedMaterialItem, 0, ref notifyNDS, ref notifyNDSSpecification);

                    if (currentMaterialItem != null)
                        materialItemId = currentMaterialItem.MaterialItemId;

                    if (updatedMaterial is Material.Editor.Models.Material.Material)
                        MaterialSpecificationType = ((Material.Editor.Models.Material.Material)updatedMaterial).SpecificationType;

                    FinishLOSDBAssociation(materialItemId, "", "");  // if necessary
                }
                else if (scenario == 3 || scenario == 5)
                {
                    currentMaterialItem = await GetActiveMaterialItemAsync(materialItemId, "active");

                    if (currentMaterialItem.MaterialCategoryId == materialCategoryId && currentMaterialItem.FeatureTypeId == featureTypeId)
                    {
                        currentMaterialItem.PersistMaterialUpdates(updatedMaterialItem, ref notifyNDS, ref notifyNDSSpecification);

                        if (currentMaterialItem is Material.Editor.Models.Material.Material)
                            MaterialSpecificationType = ((Material.Editor.Models.Material.Material)currentMaterialItem).SpecificationType;
                    }
                    else
                    {
                        IMaterial updatedMaterial = MaterialFactory.GetMaterialClass(materialItemId, materialCategoryId, featureTypeId);

                        updatedMaterial.IsRecordOnly = currentMaterialItem.IsRecordOnly;
                        updatedMaterial.IsRecordOnlyPublished = currentMaterialItem.IsRecordOnlyPublished;

                        if (updatedMaterial is Material.Editor.Models.Material.Material)
                            MaterialSpecificationType = ((Material.Editor.Models.Material.Material)updatedMaterial).SpecificationType;

                        if(!currentMaterialItem.IsRecordOnly && !(currentMaterialItem is MaterialRevision) && updatedMaterial is MaterialRevision)
                            materialIdArray = updatedMaterial.PersistObject(currentMaterialItem, updatedMaterialItem, 0, ref notifyNDS, ref notifyNDSSpecification);
                        //if (currentMaterialItem is MaterialRevision && updatedMaterial is MaterialRevision)
                        else
                            materialIdArray = updatedMaterial.PersistObject(currentMaterialItem, updatedMaterialItem, mtrlId, ref notifyNDS, ref notifyNDSSpecification);
                    }
                }
                else
                {
                    logger.Error("Unable to determine the correct scenario: {0}", updatedMaterialItem.ToString());

                    throw new Exception("Unable to determine the correct scenario");
                }

                if (materialItemId == 0 && materialIdArray != null)
                    materialItemId = materialIdArray[0];

                if (notifyNDSSpecification)
                {
                    if (dbInterface == null)
                        dbInterface = new MaterialDbInterface();

                    specificationWorkToDoId = dbInterface.InsertWorkToDo(currentMaterialItem.SpecificationRevisionId > 0 ? currentMaterialItem.SpecificationRevisionId : currentMaterialItem.SpecificationId, "CATALOG_SPEC", "UPDATE");
                }

                if (notifyNDS)
                {
                    if(dbInterface == null)
                        dbInterface = new MaterialDbInterface();

                    workToDoId = dbInterface.InsertWorkToDo(materialItemId, "CATALOG_UI", null);
                }                

                if (materialIdArray == null)
                    materialIdArray = new long[] { materialItemId, 0, workToDoId, specificationWorkToDoId };
                else
                {
                    materialIdArray[2] = workToDoId;

                    if (materialIdArray.Length == 4)
                        materialIdArray[3] = specificationWorkToDoId;
                }

                return materialIdArray;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbInterface != null)
                    dbInterface.Dispose();
            }
        }

        private void ValidateUniqueRootPartNumber(JObject updatedMaterialItem)
        {
            // mford - figured this would be the easy way not to get the root part number popup on the gui
            //MaterialDbInterface dbInterface = null;
            //MaterialEditorException mee = null;
            //List<MaterialItem> existingParts = null;
            //string partNbr = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RtPrtNbr);

            //if (string.IsNullOrEmpty(partNbr))
            //    partNbr = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrtNo);

            //dbInterface = new MaterialDbInterface();

            //existingParts = dbInterface.GetExistingRootParts(partNbr);

            //if (existingParts != null)
            //{
            //    mee = new MaterialEditorException("Root part number already exists.");

            //    mee.ErrorCode = MaterialEditorException.ERROR_CODE.RT_PART_NO_EXISTS;
            //    mee.DataJSON = JsonConvert.SerializeObject(existingParts, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            //    throw mee;
            //}
        }

        public async Task<long[]> PersistMaterialRevision(JObject updatedMaterialItem, long materialItemId)
        {
            MaterialDbInterface dbInterface = null;
            long workToDoId = 0;
            long specificationWorkToDoId = 0;
            long[] workToDoIdArray = null;
            int materialCategoryId = 0;
            int featureTypeId = 0;
            bool notifyNDSSpecification = false;
            try
            {
                materialCategoryId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MtlCtgry);
                featureTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.FtrTyp);
                    IMaterial updatedMaterial = MaterialFactory.GetMaterialClass(materialItemId, materialCategoryId, featureTypeId);
                    updatedMaterial.CreateRevision(updatedMaterialItem, ref notifyNDSSpecification);
                    if (dbInterface == null)
                        dbInterface = new MaterialDbInterface();

                    workToDoId = dbInterface.InsertWorkToDo(materialItemId, "CATALOG_UI", null);

                if (notifyNDSSpecification)
                {
                    if (dbInterface == null)
                        dbInterface = new MaterialDbInterface();

                    specificationWorkToDoId = dbInterface.InsertWorkToDo(JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId), "CATALOG_SPEC", "UPDATE");
                }

                if (workToDoIdArray == null)
                    workToDoIdArray = new long[] { workToDoId, specificationWorkToDoId };
                return workToDoIdArray;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbInterface != null)
                    dbInterface.Dispose();
            }
        }

        public async Task<string> DeleteLOSDBAssociation(long materialItemId)
        {
            string status = "SUCCESS";
            MaterialDbInterface materialDbInterface = new MaterialDbInterface();
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            FeatureType featureType = null;
            string mtrlId = "";

            try
            {
                mtrlId = newUpdatedPartsDbInterface.NonAsyncGetMaterialIDFromMaterialItemID(materialItemId);
                featureType = materialDbInterface.GetFeatureByMtrlID(mtrlId);

                materialDbInterface.StartTransaction();

                if (featureType == null)
                {
                    status = await materialDbInterface.DeleteLOSDBAssociations(materialItemId, mtrlId, null, null);
                }
                else
                {
                status = await materialDbInterface.DeleteLOSDBAssociations(materialItemId, mtrlId, featureType.CDMMSRevisionTableName, featureType.CDMMSAliasValTableName);
                }

                if ("SUCCESS".Equals(status))
                    materialDbInterface.CommitTransaction();
                else
                    materialDbInterface.RollbackTransaction();
            }
            catch (Exception ex)
            {
                materialDbInterface.RollbackTransaction();
            }
            finally
            {
                materialDbInterface.Dispose();
            }

            return status;
        }

        public string FinishLOSDBAssociation (long materialItemId, string prodId, string equipmentCatalogItemId)
        {
            MaterialDbInterface materialDbInterface = new MaterialDbInterface();
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            string returnString = "";
            string materialCode = materialDbInterface.GetMaterialCode(materialItemId);
            string materialId = newUpdatedPartsDbInterface.NonAsyncGetMaterialIDFromMaterialItemID(materialItemId);

            int materialCatalogID = materialDbInterface.GetMaterialCatalogID(materialId);

            if (materialCatalogID == 3 || materialCatalogID == 2 || materialCatalogID == 1) // low level material
            {

            // If the material_item record has a value for ies_eqpt_prd_id make the association to the LOSDB part in the mtrl table
            // 1.  Get the mtrl_id and ies_eqpt_prd_id (if there is one)
            // 2.  Insert a record into mtrl_alias_val
            // 3.  Add any other LOSDB electrical, etc, items into revision tables

            if (string.IsNullOrEmpty(prodId))
            {
                string[] equipmentIds = newUpdatedPartsDbInterface.NonAsyncGetEquipmentProdID(materialItemId);

                prodId = equipmentIds[0];
                equipmentCatalogItemId = equipmentIds[1];
            }

            if (!string.IsNullOrEmpty(prodId) && !string.IsNullOrEmpty(equipmentCatalogItemId) && !string.IsNullOrEmpty(materialId))
            {
                string revisionIdColumnName = "";
                string aliasIdColumnName = "";
                FeatureType featureType = null;

                    // Check first to make sure this material doesn't already have an association that would give us
                    // a unique constraint violation.

                    string productID = newUpdatedPartsDbInterface.CheckExistingAssocation(materialId);
                    if (productID != String.Empty)
                    {
                        // There is already an association for this part that shouldn't be there if we are at
                        // this point in the code, so go ahead and blow it away and then continue.
                        newUpdatedPartsDbInterface.DeleteExistingAssocation(materialId);
                    }

                    returnString = newUpdatedPartsDbInterface.NonAsyncAssociateLosdbtoSap(materialItemId.ToString(), prodId, equipmentCatalogItemId);

                // Get feature type
                featureType = materialDbInterface.GetFeatureByMtrlID(materialId);

                    if (featureType != null)
                    {
                // get key data from ies_invntry, ies_ea_clei_extn, ies_ea_main_extn and ies_ea_electrical_extn based on the prod_id
                if (featureType.CDMMSAliasValTableName.ToUpper() == "MTRL_ALIAS_VAL" && featureType.FeatureTypeID <= 10)
                {
                    revisionIdColumnName = "mtrl_id";
                    aliasIdColumnName = "mtrl_alias_id";

                    materialDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIdColumnName, aliasIdColumnName,
                        materialItemId, "7", equipmentCatalogItemId);
                }
                else
                {
                    revisionIdColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                    aliasIdColumnName = "rme_mtrl_revsn_alias_id";

                    materialDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIdColumnName, aliasIdColumnName,
                        materialItemId, "3", equipmentCatalogItemId);

                            EquipmentObject equipmentObject = new EquipmentObject();
                            equipmentObject = materialDbInterface.GetEquipmentObject(prodId);

                            // Update clei_cd in the rme_*_mtrl_revsn table
                            materialDbInterface.UpdateMaterialRevisionCleiCode(featureType.CDMMSRevisionTableName,
                                materialItemId.ToString(), equipmentObject.CLEICode, revisionIdColumnName);
                }

                // Get the list of electrical keys 
                List<string> electricalKeys = materialDbInterface.GetElectricalKeys(prodId, equipmentCatalogItemId);

                revisionIdColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                aliasIdColumnName = "rme_mtrl_revsn_alias_id";

                foreach (string electricalKey in electricalKeys)
                {
                    materialDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIdColumnName, aliasIdColumnName,
                        materialItemId, "7", electricalKey);
                }

                // Get the list of comp clei keys
                List<string> compCleiKeys = materialDbInterface.GetCompCleiKeys(prodId, equipmentCatalogItemId);

                revisionIdColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                aliasIdColumnName = "rme_mtrl_revsn_alias_id";

                foreach (string compCleiKey in compCleiKeys)
                {
                    materialDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIdColumnName, aliasIdColumnName,
                        materialItemId, "5", compCleiKey);
                }

                // Get the list of clei codes from main extn
                List<string> cleiCodes = materialDbInterface.GetCleiCodes(prodId, equipmentCatalogItemId);

                revisionIdColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                aliasIdColumnName = "rme_mtrl_revsn_alias_id";

                foreach (string cleiCode in cleiCodes)
                {
                    materialDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIdColumnName, aliasIdColumnName,
                        materialItemId, "6", cleiCode);

                    // Get the LOSDB attributes that have meaning to the spec pages and insert/update into rme_*_mtrl using the materialId.
                    // For example, the height/width/depth in bay_extender, bay, card, node and shelf

                    if (featureType.FeatureTypeID == 1 || featureType.FeatureTypeID == 2 || featureType.FeatureTypeID == 5
                        || featureType.FeatureTypeID == 6 || featureType.FeatureTypeID == 7)  // Bay extender 1, bay 2, node 5, shelf 6, card 7
                    {
                        MaterialItem materialItem = new MaterialItem();
                        materialItem.Attributes = new Dictionary<string, Models.Attribute>();
                        bool valuesFound = false;

                        // Get the attributes from ies_ea_main_extn
                        materialDbInterface.GetLOSDBMainExtension(materialItem, cleiCode, ref valuesFound);
                        if (valuesFound)
                        {
                            MaterialType materialType = new MaterialType();
                            string depth = String.Empty;
                            string width = String.Empty;
                            string height = String.Empty;
                            string dimUomCd = String.Empty;
                            string depthColumn = String.Empty;
                            string heightColumn = String.Empty;
                            string widthColumn = String.Empty;
                            string dimUomColumn = String.Empty;
                            long dimUomId = 0L;
                                    string specnAltIDColumn = String.Empty;
                                    string specnIDColumn = String.Empty;
                                    string specnRevsnAltTable = String.Empty;

                            depth = materialItem.Attributes["Dpth"].Value;
                            width = materialItem.Attributes["Wdth"].Value;
                            height = materialItem.Attributes["Hght"].Value;
                            dimUomCd = materialItem.Attributes["DimUom"].Value;

                            switch (featureType.FeatureTypeID)
                            {
                                        case 5:
                                            specnAltIDColumn = "node_specn_revsn_alt_id";
                                            specnIDColumn = "node_specn_id";
                                            specnRevsnAltTable = "bay_extndr_specn_revsn_alt";
                                            break;
                                        case 6:
                                            specnAltIDColumn = "shelf_specn_revsn_alt_id";
                                            specnIDColumn = "shelf_specn_id";
                                            specnRevsnAltTable = "shelf_specn_revsn_alt";
                                            break;
                                        case 7:
                                            specnAltIDColumn = "card_specn_revsn_alt_id";
                                            specnIDColumn = "card_specn_id";
                                            specnRevsnAltTable = "card_specn_revsn_alt";
                                            break;
                                    }

                                    switch (featureType.FeatureTypeID)
                                    {
                                case 1:
                                            specnAltIDColumn = "bay_extndr_specn_revsn_alt_id";
                                            specnIDColumn = "bay_extndr_specn_id";
                                    depthColumn = "itnl_dpth_id";
                                    heightColumn = "itnl_hgt_id";
                                    widthColumn = "itnl_wdth_id";
                                    long internalDepthID = materialDbInterface.GetBayExtenderItnlDepth(depth, ref dimUomId);
                                    long internalHeightID = materialDbInterface.GetBayExtenderItnlHeight(height, ref dimUomId);
                                    long internalWidthID = materialDbInterface.GetBayExtenderItnlWidth(width, ref dimUomId);
                                    if (internalDepthID > 0 && internalHeightID > 0 && internalWidthID > 0)
                                    {
                                        bool exists = materialDbInterface.RmeRecordExist(long.Parse(materialId), featureType.CDMMSRTTableName);
                                        if (!exists)
                                        {
                                            materialDbInterface.InsertRmeBayExtender(long.Parse(materialId), internalDepthID, internalHeightID, internalWidthID);
                                        }
                                        else
                                        {
                                                    string specnRevsnAltID = materialDbInterface.RmeSpecnAltIDExist(long.Parse(materialId), featureType.CDMMSRTTableName, specnAltIDColumn);
                                                    if (specnRevsnAltID != String.Empty && specnRevsnAltID != null)
                                                    {
                                                        string check = materialDbInterface.CheckPropAndComp(specnRevsnAltID, specnRevsnAltTable, specnIDColumn);
                                                        if (check != "Y")
                                                        {
                                                            //materialDbInterface.UpdateRmeBayExtender(long.Parse(materialId), internalDepthID, internalHeightID, internalWidthID);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //materialDbInterface.UpdateRmeBayExtender(long.Parse(materialId), internalDepthID, internalHeightID, internalWidthID);
                                                    }
                                        }
                                    }
                                    break;
                                case 2:
                                            specnAltIDColumn = "bay_specn_revsn_alt_id";
                                            specnIDColumn = "bay_specn_id";
                                            specnRevsnAltTable = "bay_specn_revsn_alt";
                                    depthColumn = "xtnl_dpth_no";
                                    heightColumn = "xtnl_hgt_no";
                                    widthColumn = "xtnl_wdth_no";
                                    dimUomColumn = "xtnl_dim_uom_id";
                                    // get dim_uom_id from table dim_uom
                                    dimUomId = materialDbInterface.GetDimUomId(dimUomCd);
                                    if (dimUomId > 0)
                                    {
                                        bool exists = materialDbInterface.RmeRecordExist(long.Parse(materialId), featureType.CDMMSRTTableName);
                                        if (!exists)
                                        {
                                            materialDbInterface.InsertRmeBay(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                        }
                                        else
                                        {
                                                    string specnRevsnAltID = materialDbInterface.RmeSpecnAltIDExist(long.Parse(materialId), featureType.CDMMSRTTableName, specnAltIDColumn);
                                                    if (specnRevsnAltID != String.Empty && specnRevsnAltID != null)
                                                    {
                                                        string check = materialDbInterface.CheckPropAndComp(specnRevsnAltID, specnRevsnAltTable, specnIDColumn);
                                                        if (check != "Y")
                                                        {
                                                            //materialDbInterface.UpdateRme(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                            //dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //materialDbInterface.UpdateRme(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                        //    dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                                    }
                                        }
                                    }
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                    depthColumn = "dpth_no";
                                    heightColumn = "hgt_no";
                                    widthColumn = "wdth_no";
                                    dimUomColumn = "dim_uom_id";
                                    // get dim_uom_id from table dim_uom
                                    dimUomId = materialDbInterface.GetDimUomId(dimUomCd);
                                    if (dimUomId > 0)
                                    {
                                        bool exists = materialDbInterface.RmeRecordExist(long.Parse(materialId), featureType.CDMMSRTTableName);
                                        if (!exists)
                                        {
                                            materialDbInterface.InsertRme(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                        }
                                        else
                                        {
                                                    string specnRevsnAltID = materialDbInterface.RmeSpecnAltIDExist(long.Parse(materialId), featureType.CDMMSRTTableName, specnAltIDColumn);
                                                    if (specnRevsnAltID != String.Empty && specnRevsnAltID != null)
                                                    {
                                                        string check = materialDbInterface.CheckPropAndComp(specnRevsnAltID, specnRevsnAltTable, specnIDColumn);
                                                        if (check != "Y")
                                                        {
                                                            //materialDbInterface.UpdateRme(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                            //dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //materialDbInterface.UpdateRme(long.Parse(materialId), featureType.CDMMSRTTableName, depth, width, height,
                                                        //dimUomId, depthColumn, heightColumn, widthColumn, dimUomColumn);
                                                    }
                                        }
                                    }
                                    break;
                            } //End switch
                                }
                            }
                        }
                    }
                }
            }

            return returnString;
        }

        public List<PartialMatch> CheckPartialMatches (string rootPartNumber, string clmc)
        {
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            List<PartialMatch> partialMatches = new List<PartialMatch>();

            List<string> revisionTableNames = new List<string>();
            revisionTableNames = newUpdatedPartsDbInterface.GetRevisionTableNames(rootPartNumber, clmc);

            if (revisionTableNames.Count >= 1)
            {
                // Only look for records where feat_typ.cdmms_revsn_tbl_nm is null.  Non-revisionable material can be handled in MI
                // More than 1 match.  Get a list of partial matches to display to the user to choose from

                // Get the list of possible matches.
                foreach( string revisionTableName in revisionTableNames)
                {
                    List<PartialMatch> tempPartialMatches = new List<PartialMatch>();
                    tempPartialMatches = newUpdatedPartsDbInterface.GetPartialMatches(rootPartNumber, clmc, revisionTableName);
                    foreach (PartialMatch partialMatch in tempPartialMatches)
                    {
                        partialMatches.Add(partialMatch);
                    }
                }
            }
            else
            {
                // No matches, so proceeed as usual
            }
            return partialMatches;
        }

        private void PersistSAPUpdates(long materialItemId, MaterialItem currentMaterialItem, JObject updatedMaterialItem, string cuid, MaterialDbInterface dbInterface, ref bool notifyNDS)
        {
            MaterialType mtlType = new MaterialType();            

            //Apply the following logic to each SAP field except PRODUCT_ID
            //If currentValue == updatedValue
            //      If currentValue.source == SAP && updatedValue.source == OVERRIDE --> this was an overridden value but now it has been updated to equal the SAP value
            //          delete row from material_item_attributes
            //Else
            //      If updatedValue.source == OVERRIDE
            //          update existing row in material_item_attributes      
            //      Else
            //          insert new row into material_item_attributes

            foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.SAPMaterial)
            {
                string key = keyValue.Key;
                Models.Attribute updatedAttribute = JsonHelper.DeserializeAttribute(updatedMaterialItem, key);
                DATABASE_ACTION action = DATABASE_ACTION.NONE;

                if (updatedAttribute != null)
                {
                    if (!MaterialType.JSON.PrdctId.Equals(key))
                    {
                        if (MaterialType.JSON.HzrdInd.Equals(key)) //A true value from the GUI (i.e. checkbox is checked) equals 'X' in the database
                        {
                            action = DetermineDatabaseAction(currentMaterialItem.Attributes[key].BoolValue.ToString(), updatedAttribute.BoolValue.ToString(), currentMaterialItem.Attributes[key].Source, updatedAttribute.Source);

                            if (action == DATABASE_ACTION.INSERT || action == DATABASE_ACTION.UPDATE)
                            {
                                if (updatedAttribute.BoolValue)
                                    updatedAttribute.Value = "X";
                                else
                                    updatedAttribute.Value = "";
                            }
                        }
                        else
                            action = DetermineDatabaseAction(currentMaterialItem.Attributes[key].Value, updatedAttribute.Value, currentMaterialItem.Attributes[key].Source, updatedAttribute.Source);

                        switch (action)
                        {
                            case DATABASE_ACTION.DELETE:
                                dbInterface.DeleteMaterialItemAttributes(materialItemId, ConvertStringToLong(updatedAttribute.MaterialItemAttributesDefId));
                                notifyNDS = true;

                                break;
                            case DATABASE_ACTION.INSERT:
                                dbInterface.InsertMaterialItemAttributes(updatedAttribute.Value, materialItemId, updatedAttribute.Name, cuid);
                                notifyNDS = true;

                                break;
                            case DATABASE_ACTION.UPDATE:
                                dbInterface.UpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, ConvertStringToLong(updatedAttribute.MaterialItemAttributesDefId), cuid);
                                notifyNDS = true;

                                break;
                        }
                    }
                }
            }
        }

        private void PersistAdditionalAttributeUpdates(long materialItemId, MaterialItem currentMaterialItem, JObject updatedMaterialItem, string cuid, MaterialDbInterface dbInterface, ref bool notifyNDS)
        {
            List<string> attributes = dbInterface.GetAdditionalAttributeNames();
  
            if (attributes != null)
            {
                foreach (string name in attributes)
                {
                    Models.Attribute updatedAttribute = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, name);
                    DATABASE_ACTION action = DATABASE_ACTION.NONE;

                    action = DetermineDatabaseAction(currentMaterialItem.Attributes[name].Value, updatedAttribute.Value, currentMaterialItem.Attributes[name].MaterialItemAttributesDefId, updatedAttribute.MaterialItemAttributesDefId, name);

                    switch (action)
                    {
                        case DATABASE_ACTION.DELETE:
                            dbInterface.DeleteMaterialItemAttributes(materialItemId, ConvertStringToLong("0".Equals(updatedAttribute.MaterialItemAttributesDefId) ? currentMaterialItem.Attributes[name].MaterialItemAttributesDefId : updatedAttribute.MaterialItemAttributesDefId));

                            if (currentMaterialItem.Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                notifyNDS = true;

                            break;
                        case DATABASE_ACTION.INSERT:
                            dbInterface.InsertMaterialItemAttributes(updatedAttribute.Value, materialItemId, updatedAttribute.Name, cuid);

                            if (currentMaterialItem.Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                notifyNDS = true;

                            break;
                        case DATABASE_ACTION.UPDATE:
                            dbInterface.UpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, ConvertStringToLong("0".Equals(updatedAttribute.MaterialItemAttributesDefId) ? currentMaterialItem.Attributes[name].MaterialItemAttributesDefId : updatedAttribute.MaterialItemAttributesDefId), cuid);

                            if (currentMaterialItem.Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                notifyNDS = true;

                            break;
                    }
                }
            }
            else
                throw new Exception("Was unable to retrieve list of additional attributes.");
        }

        private DATABASE_ACTION DetermineDatabaseAction(string currentValue, string updatedValue, string currentSource, string updatedSource)
        {
            DATABASE_ACTION action = DATABASE_ACTION.NONE;

            if (currentValue == updatedValue)
            {
                if (currentSource == MaterialType.SourceSystem(SOURCE_SYSTEM.SAP) && updatedSource == MaterialType.JSON.OVERRIDE)
                    action = DATABASE_ACTION.DELETE;
            }
            else
            {
                if (updatedSource == MaterialType.JSON.OVERRIDE)
                {
                    if (string.IsNullOrEmpty(updatedValue) || "0".Equals(updatedValue))
                        action = DATABASE_ACTION.DELETE; //The override value was cleared/deleted from the GUI. Assume the user wants to revert back to the SAP value.
                    else
                        action = DATABASE_ACTION.UPDATE;
                }
                else
                    action = DATABASE_ACTION.INSERT;
            }

            return action;
        }

        private DATABASE_ACTION DetermineDatabaseAction(string currentValue, string updatedValue, string currentDefId, string updatedDefId, string attributeName)
        {
            DATABASE_ACTION action = DATABASE_ACTION.NONE;

            if (currentValue != updatedValue)
            {
                if (string.IsNullOrEmpty(updatedValue) || ("0".Equals(updatedValue) && !"Stts".Equals(attributeName)))
                    action = DATABASE_ACTION.DELETE;
                else
                {
                    if (ConvertStringToLong(currentDefId) > 0 || ConvertStringToLong(updatedDefId) > 0)
                        action = DATABASE_ACTION.UPDATE;
                    else
                        action = DATABASE_ACTION.INSERT;
                }
            }

            return action;
        }

        private long ConvertStringToLong(string stringToConvert)
        {
            long convertedLong = long.MinValue;
            bool conversionSuccessful = long.TryParse(stringToConvert, out convertedLong);

            if (!conversionSuccessful)
                throw new Exception(string.Format("Unable to convert [{0}] to long.", stringToConvert));

            return convertedLong;
        }

        public async Task<string> PersistMaterialRevisions(JArray updatedRevisions)
        {
            MaterialDbInterface dbInterface = null;
            string status = "";

            try
            {
                dbInterface = new MaterialDbInterface();

                dbInterface.StartTransaction();

                for (int i = 0; i < updatedRevisions.Count; i++)
                {
                    //PROCEDURE UPDATE_REVISION_DATA(pMtlItmId IN NUMBER, pOldMtrlId IN NUMBER, pNewMtrlId IN NUMBER, pRvsnNo IN VARCHAR2, 
                    //pBaseRvsnInd IN VARCHAR2, pCurrRvsnInd IN VARCHAR2, pRetRvsnInd IN VARCHAR2, oStatus OUT VARCHAR2)
                    long materialItemId = long.Parse(updatedRevisions[i].Value<JObject>("Attributes").GetValue("material_item_id").Value<string>("value"));
                    long currentMtrlId = long.Parse(updatedRevisions[i].Value<JObject>("Attributes").GetValue("curr_mtrl_id").Value<string>("value"));
                    long updatedMtrlId = long.Parse(updatedRevisions[i].Value<JObject>("Attributes").GetValue("updtd_mtrl_id").Value<string>("value"));
                    string revisionNumber = updatedRevisions[i].Value<JObject>("Attributes").GetValue("rvsn").Value<string>("value");
                    string baseRevisionInd = updatedRevisions[i].Value<JObject>("Attributes").GetValue("baseRvsn").Value<string>("value");
                    string currentRevisionInd = updatedRevisions[i].Value<JObject>("Attributes").GetValue("currRvsn").Value<string>("value");

                    status = await dbInterface.UpdateMaterialRevisions(materialItemId, currentMtrlId, updatedMtrlId, revisionNumber, baseRevisionInd, currentRevisionInd, "N");

                    if ("ERROR".Equals(status))
                        break;
                }

                if ("ERROR".Equals(status))
                    dbInterface.RollbackTransaction();
                else
                    dbInterface.CommitTransaction();

                return status;
            }
            catch (Exception ex)
            {
                dbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (dbInterface != null)
                    dbInterface.Dispose();
            }
        }

        public async Task<string> PersistRootPartNumber(JObject mtrl)
        {
            MaterialDbInterface dbInterface = null;
            string status = "SUCCESS";

            await Task.Run(() =>
            {
                try
                {
                    dbInterface = new MaterialDbInterface();

                    dbInterface.StartTransaction();

                    long mtrlId = mtrl.Value<long>("mtrlId");
                    string rootPartNumber = mtrl.Value<string>("rtPrtNbr");
                    string clmc = mtrl.Value<string>("clmc");
                    string specName = mtrl.Value<string>("specNm");

                    dbInterface.UpdateRootPartNumber(mtrlId, rootPartNumber);

                    dbInterface.CommitTransaction();

                    // Need to check for a spec name change
                    //string specID = String.Empty;
                    //string specTableName = String.Empty;
                    //string NDSID = String.Empty;
                    //string specType = String.Empty;
                    //string propagatedInd = String.Empty;
                    //string returnInfoString = String.Empty;

                    //if (clmc + "-" + rootPartNumber != specName)
                    //{
                    //    // The spec name is different, so need to update spec name for each material_item_id (possibly > 1) in material_item_attributes
                    //    // as well as spec and revision tables if the spec exists
                    //    List<string> materialItemIDList = new List<string>();
                    //    NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
                    //    materialItemIDList = newUpdatedPartsDbInterface.GetMaterialItemIDFromMaterialID(mtrlId.ToString());
                    //    foreach (string materialItemID in materialItemIDList)
                    //    {
                    //        newUpdatedPartsDbInterface.GetSpecInfo(specName, ref specID, ref specTableName, ref NDSID, ref specType, ref propagatedInd, ref returnInfoString);

                    //        newUpdatedPartsDbInterface.UpdateSpec(specID, specTableName, clmc + "-" + rootPartNumber, "", materialItemID);
                    //    }
                    //}

                    // Need to update spec downstream to NDS so create a work_to_do
                    //if (specID != String.Empty  && propagatedInd == "Y") // this material has a spec
                    //{
                    //    WorkDbInterface workDbInterface = new WorkDbInterface();
                    //    ChangeSet changeSet = new ChangeSet();
                    //    ChangeRecord changeRecord = new ChangeRecord();
                    //    changeSet.ChangeSetId = long.Parse(specID);
                    //    changeSet.ProjectId = 0L;
                    //    changeRecord.PniId = NDSID;
                    //    long workToDoID = 0L;
                    //    if (NDSID == String.Empty)
                    //    {
                    //        // this would be a case where there is a spec id but no nds id, and this is propagated, which would be odd if
                    //        // the spec was never sent to nds in the first place, so send this as an insert
                    //        changeRecord.TableName = "INSERT";
                    //        changeRecord.PniId = "0";
                    //        workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                    //    }
                    //    else if (NDSID == "0")
                    //    {
                    //        // this is a bay or bay_extndr and has no bay_specn_alias_val or bay_extndr_specn_alias_val table??
                    //    }
                    //    else
                    //    {
                    //        // this has an nds id so send an udpate
                    //        changeRecord.TableName = "UPDATE";
                    //        workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                    //    }
                    //    if (workToDoID > 0)
                    //    {
                    //        // send string back to calling javascript so referenceDataHelper can be called
                    //        status = specID + "/" + workToDoID + "/" + specType;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    dbInterface.RollbackTransaction();
                    
                    throw ex;
                }
                finally
                {
                    if (dbInterface != null)
                        dbInterface.Dispose();
                }
            });

            return status;
        }

        public async Task<string> PersistSpecName(JObject mtrl)
        {
            string status = "SUCCESS";
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            WorkDbInterface workDbInterface = new WorkDbInterface();
            string specID = String.Empty;
            string specTableName = String.Empty;
            string NDSID = String.Empty;
            string specType = String.Empty;
            string propagatedInd = String.Empty;
            string returnInfoString = String.Empty;
            bool doNothing = false;

            await Task.Run(() =>
            {
                try
                {
                    long mtrlId = mtrl.Value<long>("mtrlId");
                    string oldSpecName = mtrl.Value<string>("oldSpec").Trim().ToUpper();
                    string newSpecName = mtrl.Value<string>("newSpec").Trim().ToUpper();

                    newUpdatedPartsDbInterface.GetSpecInfo(oldSpecName, ref specID, ref specTableName, ref NDSID, ref specType, ref propagatedInd, ref returnInfoString);

                    if (returnInfoString == "SUCCESS")
                    {
                        if (oldSpecName == newSpecName)
                        {
                            doNothing = true;
                        }
                        // scenario 1, oldspec is null and newspec is populated
                        else if (oldSpecName == String.Empty && newSpecName != String.Empty)
                        {
                            // insert a record into material_item_attributes
                            newUpdatedPartsDbInterface.InsertSpec(newSpecName, mtrlId.ToString());
                        }
                        // scenario 2, oldspec and newspec are different
                        // scenario 3, check to see if newspec is a spec that is created
                        else if (oldSpecName != String.Empty && oldSpecName != newSpecName)
                        {
                            // update record in material_item_attributes
                            newUpdatedPartsDbInterface.UpdateSpec(specID, specTableName, newSpecName, "", mtrlId.ToString());
                        }

                        // scenario 4, spec is created and in NDS
                        // Need to update spec downstream to NDS so create a work_to_do
                        if (specID != String.Empty && propagatedInd == "Y" && !doNothing) // this material has a propagated spec
                        {

                            ChangeSet changeSet = new ChangeSet();
                            ChangeRecord changeRecord = new ChangeRecord();
                            changeSet.ChangeSetId = long.Parse(specID);
                            changeSet.ProjectId = 0L;
                            changeRecord.PniId = NDSID;
                            long workToDoID = 0L;
                            if (NDSID == String.Empty)
                            {
                                // this would be a case where there is a spec id but no nds id, and propagated, which would be odd if
                                // the spec was never sent to nds in the first place, so send this as an insert
                                changeRecord.TableName = "INSERT";
                                changeRecord.PniId = "0";
                                workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                            }
                            else if (NDSID == "0")
                            {
                                // this is a bay or bay_extndr and has no bay_specn_alias_val or bay_extndr_specn_alias_val table??
                            }
                            else
                            {
                                // this has an nds id so send an udpate
                                changeRecord.TableName = "UPDATE";
                                workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                            }
                            if (workToDoID > 0)
                            {
                                // send string back to calling javascript so referenceDataHelper can be called
                                status = specID + "/" + workToDoID + "/" + specType;
                            }
                        }
                    }
                    else
                    {
                        status = "ERROR";
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                }
            });

            return status;
        }

        private string GetAliasColumnName(string aliasTableName)
        {
            string aliasColumnName = "";

            if (aliasTableName.ToUpper() == "MTRL_ALIAS_VAL")
            {
                aliasColumnName = "mtrl_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("RV_ALS_VAL"))
            {
                aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 10) + "revsn_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("REVSN_ALIAS_VAL"))
            {
                aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 15) + "revsn_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("ALIAS_VAL"))
            {
                if (aliasTableName.ToUpper() == "RME_PLG_IN_MTRL_RVSN_ALIAS_VAL")
                {
                    aliasColumnName = "RME_PLG_IN_MTRL_REVSN_ID";
                }
                else
                {
                    aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 9) + "id";
                }
            }

            return aliasColumnName;
        }
    }
}