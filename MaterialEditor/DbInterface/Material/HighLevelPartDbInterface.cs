using System;
using System.Collections;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class HighLevelPartDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public HighLevelPartDbInterface() : base()
        {
        }

        public HighLevelPartDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial highLevelPart = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_high_level_part", parameters);

                if (reader.Read())
                {
                    highLevelPart = new HighLevelPart(materialItemId, mtrlId);

                    highLevelPart.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    highLevelPart.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    highLevelPart.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    highLevelPart.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    highLevelPart.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    highLevelPart.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((HighLevelPart)highLevelPart).BaseRevisionInd = DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind");
                    ((HighLevelPart)highLevelPart).CurrentRevisionInd = DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind");
                    ((HighLevelPart)highLevelPart).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((HighLevelPart)highLevelPart).RetiredRevisionInd = DataReaderHelper.GetNonNullValue(reader, "ret_revsn_ind");
                    ((HighLevelPart)highLevelPart).RevisionNumber = DataReaderHelper.GetNonNullValue(reader, "revsn_no");
                    ((HighLevelPart)highLevelPart).MaterialUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_uom_id", true));

                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");

                    if ("Y".Equals(recordOnly))
                        highLevelPart.IsRecordOnly = true;
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve material id: {0}";

                //hadException = true;

                logger.Error(oe, message, materialItemId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to retrieve material id: {0}", materialItemId);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }

            return highLevelPart;
        }

        public void UpdateRevisionData(long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, int orderableMaterialStatusId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pBaseRvsnInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCurrRvsnInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pRetRvsnInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pOrdbleId", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.UPDATE_REVISION_DATA", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update hlp_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void UpdateHighLevelPart(long mtrlId, int materialUomId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pUomId", DbType.Int64, CheckNullValue(materialUomId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.update_hlp_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update hlp_mtrl_pkg ({0}, {1})", mtrlId, materialUomId);

                throw ex;
            }
        }

        public long[] CreateHighLevelPart(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, int uomId, long materialItemId, string revisionNumber, string baseRevisionInd,
            string currentRevisionInd, string retiredRevisionInd, int orderableMaterialStatusId, string materialCode, long inMtrlId)
        {
            long mtrlId = 0;
            long outMaterialItemId = 0;
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE create_hlp(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pUomId IN NUMBER,
                //pMtlItmId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, 
                //pRevsnRetInd IN VARCHAR2, pOrdblId IN NUMBER, oMtrlId OUT NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(22);

                parameters[0] = dbAccessor.GetParameter("pMfrId", DbType.Int64, mfrId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCatId", DbType.Int32, materialCategoryId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRoInd", DbType.String, recordOnly ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pRetInd", DbType.String, retiredInd, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pLbrId", DbType.Int32, laborId, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pFeatTyp", DbType.Int64, CheckNullValue(featureTypeId), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pSpecnInitInd", DbType.String, specificationInitInd, ParameterDirection.Input);

                if (uomId == 0)
                    parameters[11] = dbAccessor.GetParameter("pUomId", DbType.Int32, null, ParameterDirection.Input);
                else
                    parameters[11] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);

                parameters[12] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pRevsnNo", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pBaseInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pCurrInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pRvsnRetInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[18] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[18] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[19] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[20] = dbAccessor.GetParameter("oMtrlId", DbType.Int64, mtrlId, ParameterDirection.Output);
                parameters[21] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.create_hlp", parameters);

                mtrlId = long.Parse(parameters[20].Value.ToString());
                outMaterialItemId = long.Parse(parameters[21].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create hlp_mtrl ({0})", materialItemId);

                throw ex;
            }

            return new long[] { outMaterialItemId, mtrlId, 0 };
        }


        //HLPN pop up screen from Material Inventory page.
        public async Task<List<Dictionary<string, Models.Attribute>>> GetContainedInTermsHLPList(int materialItemId)
        {
            List<Dictionary<string, Models.Attribute>> citList = new List<Dictionary<string, Models.Attribute>>();
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<string, Models.Attribute> cit = null;
            List<string> partNumbers = new List<string>();
            string commonConfig = GetCommonConfigString(materialItemId);

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.String, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("HLP_MTRL_PKG.get_contained_in_parts_def", parameters);

                    while (reader.Read())
                    {
                        Models.Attribute isSelected = new Models.Attribute("is_selected", true);
                        Models.Attribute isUpdated = new Models.Attribute("is_updated", false);
                        Models.Attribute refDefId = new Models.Attribute("refDefId", DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_def_id", true));
                        Models.Attribute hlpId = new Models.Attribute("hlpcdmms_Id", DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_id", true));
                        Models.Attribute commonConfigString = new Models.Attribute("comn_cnfg_string", commonConfig);

                        string savedhlpId = DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_id", true);
                        int intRefDefId = int.Parse(refDefId.Value);
                        int intFtrTypId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "feat_typ_id", true));
                        bool hasXYCoord = false;

                        if (intFtrTypId == 5 || intFtrTypId == 6 || intFtrTypId == 7)  // only for node, shelf and card
                        {
                            hasXYCoord = true;
                        }

                        cit = new Dictionary<string, Models.Attribute>();
                        
                        Models.Attribute savedHlpId = new Models.Attribute("savedHlpId", savedhlpId);
                        Models.Attribute cinId = new Models.Attribute("cin_cdmms_Id", DataReaderHelper.GetNonNullValue(reader, "cntnd_in_id", true));
                        Models.Attribute materialCode = new Models.Attribute("material_code", DataReaderHelper.GetNonNullValue(reader, "cntnd_in_mtrl_cd"));
                        Models.Attribute isRevision = new Models.Attribute("is_revision", DataReaderHelper.GetNonNullValue(reader, "cntnd_in_revsn_lvl_ind"));
                        Models.Attribute existingisRevision = new Models.Attribute("existing_is_revision", DataReaderHelper.GetNonNullValue(reader, "cntnd_in_revsn_lvl_ind"));
                        Models.Attribute quantity = new Models.Attribute("quantity", DataReaderHelper.GetNonNullValue(reader, "qty", true));
                        Models.Attribute existingquantity = new Models.Attribute("existing_quantity", DataReaderHelper.GetNonNullValue(reader, "qty", true));
                        Models.Attribute spareQuantity = new Models.Attribute("spare_quantity", DataReaderHelper.GetNonNullValue(reader, "spr_qty", true));
                        Models.Attribute existingspareQuantity = new Models.Attribute("existing_spare_quantity", DataReaderHelper.GetNonNullValue(reader, "spr_qty", true));

                        Models.Attribute ycoord;
                        Models.Attribute xcoord;
                        Models.Attribute existingycoord;
                        Models.Attribute existingxcoord;
                        if (hasXYCoord)
                        {
                            ycoord = new Models.Attribute("ycoord", DataReaderHelper.GetNonNullValue(reader, "y_coord_no", true));
                            xcoord = new Models.Attribute("xcoord", DataReaderHelper.GetNonNullValue(reader, "x_coord_no", true));
                            existingycoord = new Models.Attribute("existingycoord", DataReaderHelper.GetNonNullValue(reader, "y_coord_no", true));
                            existingxcoord = new Models.Attribute("existingxcoord", DataReaderHelper.GetNonNullValue(reader, "x_coord_no", true));
                        }
                        else
                        {
                            ycoord = new Models.Attribute("ycoord", "0");
                            xcoord = new Models.Attribute("xcoord", "0");
                            existingycoord = new Models.Attribute("existingycoord", "0");
                            existingxcoord = new Models.Attribute("existingxcoord", "0");
                        }
                        Models.Attribute hasXY = new Models.Attribute("hasXY", hasXYCoord);

                        string totQuantity = (int.Parse(quantity.Value) + int.Parse(spareQuantity.Value)).ToString();
                        string parentID = DataReaderHelper.GetNonNullValue(reader, "prnt_hlp_mtrl_revsn_def_id");

                        Models.Attribute totalQuantity = new Models.Attribute("total_quantity", totQuantity);
                        Models.Attribute existingtotalQuantity = new Models.Attribute("existing_total_quantity", totQuantity);
                        Models.Attribute placement = new Models.Attribute("placement_front_rear", DataReaderHelper.GetNonNullValue(reader, "frnt_rer_ind"));
                        Models.Attribute existingplacement = new Models.Attribute("existing_placement_front_rear", DataReaderHelper.GetNonNullValue(reader, "frnt_rer_ind"));
                        Models.Attribute manufacturerCode = new Models.Attribute("clmc", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute rootPartNumber = new Models.Attribute("part_number", DataReaderHelper.GetNonNullValue(reader, "mfg_part_no"));
                        Models.Attribute materialCategory = new Models.Attribute("material_category", DataReaderHelper.GetNonNullValue(reader, "mtrl_cat_typ"));
                        Models.Attribute featureType = new Models.Attribute("feature_type", DataReaderHelper.GetNonNullValue(reader, "feat_typ"));
                        Models.Attribute mtrlUOM = new Models.Attribute("mtrlUOM", DataReaderHelper.GetNonNullValue(reader, "mtrl_uom_cd"));
                        Models.Attribute hasPossibleParent = new Models.Attribute("hasPossibleParent", DataReaderHelper.GetNonNullValue(reader, "has_possible_parent"));
                        Models.Attribute aliasVal = new Models.Attribute("aliasVal", DataReaderHelper.GetNonNullValue(reader, "nds_def_id"));                            
                        Models.Attribute parentHlpID = new Models.Attribute("parent_id", parentID);
                        Models.Attribute existingparentHlpID = new Models.Attribute("existing_parent_id", parentID);

                        // If the part has value in prnt_hlp_mtrl_revsn_def_id then get the associated part number to default the dropdown
                        string parentPartNumber = "";

                        if (parentID != String.Empty)
                            parentPartNumber = GetPartNumberFromDefID(materialItemId, intFtrTypId, Int32.Parse(parentID));
                            
                        Models.Attribute parentPart = new Models.Attribute("parent_part", parentPartNumber);

                        // If the part has a prnt_hlp_mtrl_revsn_def_id it is a candidate to be a contained-in part.
                        // Get the potential candidates already assigned as a list for the related-to dropdown.
                        partNumbers = new List<string>();
                        Hashtable hashTableParts = new Hashtable();

                        if (hasPossibleParent.Value == "Y")
                        {
                            partNumbers = GetRelatedToPartNumbers2(materialItemId, intFtrTypId, ref hashTableParts);

                            partNumbers.Insert(0, "");
                        }

                        Models.Attribute relatedTo = new Models.Attribute("related_to", partNumbers);
                        Models.Attribute htParts = new Models.Attribute("ht_parts", hashTableParts);

                        List<string> childrenIDs = new List<string>();

                        if (intFtrTypId == 2 || intFtrTypId == 5 || intFtrTypId == 6 || intFtrTypId == 7)
                        {
                            childrenIDs = GetChildrenDefIDs(intRefDefId);
                        }

                        bool childrenbool = false;

                        if (childrenIDs.Count > 0)
                        {
                            childrenbool = true;
                        }

                        Models.Attribute hasChildren = new Models.Attribute("has_children", childrenbool);

                        cit.Add(savedHlpId.Name, savedHlpId);
                        cit.Add(cinId.Name, cinId);
                        cit.Add(placement.Name, placement);
                        cit.Add(existingplacement.Name, existingplacement);
                        cit.Add(isRevision.Name, isRevision);
                        cit.Add(existingisRevision.Name, existingisRevision);
                        cit.Add(quantity.Name, quantity);
                        cit.Add(existingquantity.Name, existingquantity);
                        cit.Add(spareQuantity.Name, spareQuantity);
                        cit.Add(existingspareQuantity.Name, existingspareQuantity);
                        cit.Add(totalQuantity.Name, totalQuantity);
                        cit.Add(existingtotalQuantity.Name, existingtotalQuantity);
                        cit.Add(materialCode.Name, materialCode);
                        cit.Add(rootPartNumber.Name, rootPartNumber);
                        cit.Add(manufacturerCode.Name, manufacturerCode);
                        cit.Add(materialCategory.Name, materialCategory);
                        cit.Add(featureType.Name, featureType);
                        cit.Add(mtrlUOM.Name, mtrlUOM);
                        cit.Add(hasPossibleParent.Name, hasPossibleParent);
                        cit.Add(aliasVal.Name, aliasVal);
                        cit.Add(relatedTo.Name, relatedTo);
                        cit.Add(parentPart.Name, parentPart);
                        cit.Add(parentHlpID.Name, parentHlpID);
                        cit.Add(existingparentHlpID.Name, existingparentHlpID);
                        cit.Add(htParts.Name, htParts);
                        cit.Add(hasChildren.Name, hasChildren);
                        cit.Add(xcoord.Name, xcoord);
                        cit.Add(ycoord.Name, ycoord);
                        cit.Add(existingxcoord.Name, existingxcoord);
                        cit.Add(existingycoord.Name, existingycoord);
                        cit.Add(hasXY.Name, hasXY);
                        cit.Add(commonConfigString.Name, commonConfigString);

                            Models.Attribute hasLevel;

                        if (CheckContainedInTermsAvailablity(cinId.Value.ToString(), dbManager))
                        {
                            hasLevel = new Models.Attribute("has_level", true);
                        }
                        else
                        {
                            hasLevel = new Models.Attribute("has_level", false);
                        }

                        cit.Add(hasLevel.Name, hasLevel);

                        cit.Add(refDefId.Name, refDefId);
                        cit.Add(isSelected.Name, isSelected);
                        cit.Add(isUpdated.Name, isUpdated);
                        cit.Add(hlpId.Name, hlpId);

                        citList.Add(cit);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve contained-in terms list for id: {0}";
                    logger.Error(oe, message, materialItemId);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve contained-in terms list for id: {0}", materialItemId);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return citList;
        }
        public async Task<List<MaterialItem>> SearchMaterialsForHLP(string cdmmsid, string productId, string partNumber, string clmc, string description)
        {
            List<MaterialItem> searchResultList = new List<MaterialItem>();
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialItem searchResult = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(6);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.String, cdmmsid, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPartNo", DbType.String, partNumber, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pDesc", DbType.String, description, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pMatCode", DbType.String, productId, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pCLMC", DbType.String, clmc, ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("HLP_MTRL_PKG.search_high_level_part", parameters);
                    while (reader.Read())
                    {
                        searchResult = new MaterialItem();
                        Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                        Models.Attribute cin_cdmms_Id = new Models.Attribute("cin_cdmms_Id", DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                        Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "product_id"));
                        Models.Attribute rootPartNumber = new Models.Attribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "mfg_part_no"));
                        Models.Attribute itemDescription = new Models.Attribute("item_description", DataReaderHelper.GetNonNullValue(reader, "item_desc"));
                        Models.Attribute manufacturerCode = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfg_id"));
                        Models.Attribute materialCategory = new Models.Attribute("mtl_ctgry", DataReaderHelper.GetNonNullValue(reader, "mtl_ctgry"));
                        Models.Attribute featureType = new Models.Attribute("ftr_typ", DataReaderHelper.GetNonNullValue(reader, "ftr_typ"));
                        Models.Attribute mtrlID = new Models.Attribute("material_ID", DataReaderHelper.GetNonNullValue(reader, "mtrl_id"));
                        searchResult.Attributes.Add(id.Name, id);
                        searchResult.Attributes.Add(cin_cdmms_Id.Name, cin_cdmms_Id);
                        searchResult.Attributes.Add(materialCode.Name, materialCode);
                        searchResult.Attributes.Add(rootPartNumber.Name, rootPartNumber);
                        searchResult.Attributes.Add(itemDescription.Name, itemDescription);
                        searchResult.Attributes.Add(manufacturerCode.Name, manufacturerCode);
                        searchResult.Attributes.Add(materialCategory.Name, materialCategory);
                        searchResult.Attributes.Add(featureType.Name, featureType);
                        searchResult.Attributes.Add(mtrlID.Name, mtrlID);
                        searchResultList.Add(searchResult);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve contained-in terms list for id: {0}";
                    logger.Error(oe, message, cdmmsid);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve contained-in terms list for id: {0}", cdmmsid);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return searchResultList;
        }

        public async Task<string> UpdateContainedInTermsList(string prntMtrlCd, string cuid, JArray citLstNew, JArray citLstExisting, string catalogDescription)
        {
            string hlpMtrlCd = "", matCat, ftyTyp, rvsnInd, existingrvsnInd, frntRer, existingfrntRer;
            int qty, existingqty, sprQty, existingsprQty, refDefId, parentID, existingparentID;
            decimal xCoordNo, yCoordNo, existingXCoordNo, existingYCoordNo;
            long containedInCDMMSId = 0;
            string status = "{\"wtd_id\": ";
            HighLevelPartNDSHelper helper = new HighLevelPartNDSHelper(prntMtrlCd);
            List<HLPAuditObject> hlpAuditObjects = new List<HLPAuditObject>();
            AuditDbInterface auditDbInterface = new AuditDbInterface();

            await Task.Run(async() =>
            {
                try
                {
                    StartTransaction();

                    for (int i = 0; i < citLstNew.Count; i++)
                    {
                        long defId = 0;

                        containedInCDMMSId = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("cin_cdmms_Id").Value<long>("value");

                        if (string.IsNullOrEmpty(citLstNew[i].Value<JObject>("Attributes").Value<JObject>("product_id").Value<string>("value")))
                            hlpMtrlCd = "";
                        else
                            hlpMtrlCd = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("product_id").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstNew[i].Value<JObject>("Attributes").Value<JObject>("mtl_ctgry").Value<string>("value")))
                            matCat = "";
                        else
                            matCat = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("mtl_ctgry").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstNew[i].Value<JObject>("Attributes").Value<JObject>("ftr_typ").Value<string>("value")))
                            ftyTyp = "";
                        else
                            ftyTyp = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("ftr_typ").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstNew[i].Value<JObject>("Attributes").Value<JObject>("revisions").Value<string>("value")))
                            rvsnInd = "";
                        else
                            rvsnInd = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("revisions").Value<string>("value");

                        bool IsSelected = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("is_selected").Value<bool>("bool");

                        long partCDMMSID = citLstNew[i].Value<JObject>("Attributes").Value<JObject>("material_item_id").Value<long>("value");

                        if (IsSelected)
                        {
                            defId = InsertContainedInTerms(prntMtrlCd, hlpMtrlCd, matCat, ftyTyp, rvsnInd, "F", 0, 0, 0, false);

                            #region If this HLP is contained-in to a common config, insert into comn_cnfg_cntnd_hlp_mtrl_def
                            List<int> defIds = new List<int>();
                            defIds = GetCommonConfigIdList(containedInCDMMSId);
                            foreach (int mydefId in defIds)
                            {
                                // Insert reference into each HLP reference contained in a common config
                                InsertHLPCommonConfigRef(mydefId, defId);
                            }
                            #endregion

                            if (defId > 0 && containedInCDMMSId > 0)
                            {
                                helper.AddToDoItem(new ContainedInItem(defId.ToString(), matCat, "INSERT", containedInCDMMSId.ToString()));

                                #region Audit for adding a HLP contained-in part
                                string auditTable = GetAuditTable(matCat, ftyTyp);
                                string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "cntnd_in_id");
                                HLPAuditObject hlpAuditObject = new HLPAuditObject();
                                hlpAuditObject.actncd = "A";
                                hlpAuditObject.auditcoldefid = auditColDefID;
                                hlpAuditObject.auditprnttblpkcolnm = "";
                                hlpAuditObject.auditprnttblpkcolval = "";
                                hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                                hlpAuditObject.audittblpkcolval = defId.ToString();
                                hlpAuditObject.cmnttxt = cuid + " added material with CDMMS ID " + partCDMMSID.ToString() + " to \"" + catalogDescription + "\" on " + DateTime.Now.ToString();
                                hlpAuditObject.cuid = cuid;
                                hlpAuditObject.oldcolval = "";
                                hlpAuditObject.newcolval = containedInCDMMSId.ToString();
                                hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                                hlpAuditObjects.Add(hlpAuditObject);
                                #endregion
                            }
                        }
                    }

                    for (int i = 0; i < citLstExisting.Count; i++)
                    {
                        containedInCDMMSId = citLstExisting[i].Value<JObject>("cin_cdmms_Id").Value<long>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("material_code").Value<string>("value")))
                            hlpMtrlCd = "";
                        else
                            hlpMtrlCd = citLstExisting[i].Value<JObject>("material_code").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("material_category").Value<string>("value")))
                            matCat = "";
                        else
                            matCat = citLstExisting[i].Value<JObject>("material_category").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("feature_type").Value<string>("value")))
                            ftyTyp = "";
                        else
                            ftyTyp = citLstExisting[i].Value<JObject>("feature_type").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("is_revision").Value<string>("value")))
                            rvsnInd = "";
                        else
                            rvsnInd = citLstExisting[i].Value<JObject>("is_revision").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existing_is_revision").Value<string>("value"))) existingrvsnInd = "";
                        else existingrvsnInd = citLstExisting[i].Value<JObject>("existing_is_revision").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("placement_front_rear").Value<string>("value")))
                            frntRer = "";
                        else
                            frntRer = citLstExisting[i].Value<JObject>("placement_front_rear").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existing_placement_front_rear").Value<string>("value"))) existingfrntRer = "";
                        else existingfrntRer = citLstExisting[i].Value<JObject>("existing_placement_front_rear").Value<string>("value");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("xcoord").Value<string>("value")))
                            xCoordNo = 0;
                        else
                            xCoordNo = decimal.Parse(citLstExisting[i].Value<JObject>("xcoord").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existingxcoord").Value<string>("value"))) existingXCoordNo = 0;
                        else existingXCoordNo = decimal.Parse(citLstExisting[i].Value<JObject>("existingxcoord").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("ycoord").Value<string>("value")))
                            yCoordNo = 0;
                        else
                            yCoordNo = decimal.Parse(citLstExisting[i].Value<JObject>("ycoord").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existingycoord").Value<string>("value"))) existingYCoordNo = 0;
                        else existingYCoordNo = decimal.Parse(citLstExisting[i].Value<JObject>("existingycoord").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("quantity").Value<string>("value")))
                            qty = 0;
                        else
                            qty = int.Parse(citLstExisting[i].Value<JObject>("quantity").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existing_quantity").Value<string>("value"))) existingqty = 0;
                        else existingqty = int.Parse(citLstExisting[i].Value<JObject>("existing_quantity").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("spare_quantity").Value<string>("value")))
                            sprQty = 0;
                        else
                            sprQty = int.Parse(citLstExisting[i].Value<JObject>("spare_quantity").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existing_spare_quantity").Value<string>("value"))) existingsprQty = 0;
                        else existingsprQty = int.Parse(citLstExisting[i].Value<JObject>("existing_spare_quantity").Value<string>("value"));

                        bool IsSelected = citLstExisting[i].Value<JObject>("is_selected").Value<bool>("bool");
                        bool IsUpdated = citLstExisting[i].Value<JObject>("is_updated").Value<bool>("bool");

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("refDefId").Value<string>("value")))
                            refDefId = 0;
                        else
                            refDefId = int.Parse(citLstExisting[i].Value<JObject>("refDefId").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("parent_id").Value<string>("value")))
                            parentID = 0;
                        else
                            parentID = int.Parse(citLstExisting[i].Value<JObject>("parent_id").Value<string>("value"));

                        if (string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("existing_parent_id").Value<string>("value"))) existingparentID = 0;
                        else existingparentID = int.Parse(citLstExisting[i].Value<JObject>("existing_parent_id").Value<string>("value"));

                        string savedHlpId = citLstExisting[i].Value<JObject>("savedHlpId").Value<string>("value");   

                        #region Audit for a change to revision indicator
                        if (rvsnInd != existingrvsnInd)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "cntnd_in_revsn_lvl_ind");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", Revision from " + existingrvsnInd + " to " + rvsnInd + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingrvsnInd;
                            hlpAuditObject.newcolval = rvsnInd;
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to Front/Rear
                        if (frntRer != existingfrntRer)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "frnt_rer_ind");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", Front/Rear from " + existingfrntRer + " to " + frntRer + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingfrntRer;
                            hlpAuditObject.newcolval = frntRer;
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to Quantity
                        if (qty != existingqty)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "cntnd_in_mtrl_qty");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", Quantity from " + existingqty.ToString() + " to " + qty.ToString() + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingqty.ToString();
                            hlpAuditObject.newcolval = qty.ToString();
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to Spare Quantity
                        if (sprQty != existingsprQty)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "cntnd_in_mtrl_spr_qty");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", Spare Quantity from " + existingsprQty.ToString() + " to " + sprQty.ToString() + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingsprQty.ToString();
                            hlpAuditObject.newcolval = sprQty.ToString();
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to HORZ DISP
                        if (xCoordNo != existingXCoordNo)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "x_coord_no");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", HORZ DISP from " + existingXCoordNo.ToString() + " to " + xCoordNo.ToString() + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingXCoordNo.ToString();
                            hlpAuditObject.newcolval = xCoordNo.ToString();
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to EQDES
                        if (yCoordNo != existingYCoordNo)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "y_coord_no");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", EQDES from " + existingYCoordNo.ToString() + " to " + yCoordNo.ToString() + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingYCoordNo.ToString();
                            hlpAuditObject.newcolval = yCoordNo.ToString();
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        #region Audit for a change to Parent ID
                        if (parentID != existingparentID)
                        {
                            string auditTable = GetAuditTable(matCat, ftyTyp);
                            string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "prnt_hlp_mtrl_revsn_def_id");
                            HLPAuditObject hlpAuditObject = new HLPAuditObject();
                            hlpAuditObject.actncd = "C";
                            hlpAuditObject.auditcoldefid = auditColDefID;
                            hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                            hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                            hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                            hlpAuditObject.audittblpkcolval = refDefId.ToString();
                            hlpAuditObject.cmnttxt = cuid + " changed \"" + catalogDescription + "\", Related To ID from " + existingparentID.ToString() + " to " + parentID.ToString() + " on " + DateTime.Now.ToString();
                            hlpAuditObject.cuid = cuid;
                            hlpAuditObject.oldcolval = existingparentID.ToString();
                            hlpAuditObject.newcolval = parentID.ToString();
                            hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                            hlpAuditObjects.Add(hlpAuditObject);
                        }
                        #endregion

                        if (IsSelected)
                        {
                            if (IsUpdated)
                            {
                                UpdateContainedInTerms(refDefId, parentID, frntRer, rvsnInd, qty, sprQty, matCat, ftyTyp, xCoordNo, yCoordNo);
                                helper.AddToDoItem(new ContainedInItem(refDefId.ToString(), matCat, "UPDATE", containedInCDMMSId.ToString()));
                            }
                        }
                        else
                        {
                            // before the delete, check to see if this has children
                            bool haskids = string.IsNullOrEmpty(citLstExisting[i].Value<JObject>("has_children").Value<string>("value"));
                            List<string> kids = new List<string>();

                            kids = GetChildrenDefIDs(refDefId);

                            foreach (string defid in kids)
                            {
                                // call update_child_parent_id to null out the prnt_hlp__mtrl_revsn_def_id so there are no key constraint
                                // errors deleting the part requested
                                updateChildParentDefID(Int32.Parse(defid));
                            }

                            bool hasAlias = HasAlias(refDefId);

                            if (!hasAlias)
                            {
                                #region Delete HLP record from any that may be in a common config
                                DeleteHLPCommonConfigRef(refDefId);  // if there aren't any this just won't delete anything
                                #endregion

                                DeleteContainedInTerms(refDefId, hlpMtrlCd);  

                                #region Audit for deleting a HLP contained-in part
                                string partNumber = citLstExisting[i].Value<JObject>("part_number").Value<string>("value");

                                string auditTable = GetAuditTable(matCat, ftyTyp);
                                string auditColDefID = await auditDbInterface.GetAuditColDefID(auditTable, "cntnd_in_id");
                                HLPAuditObject hlpAuditObject = new HLPAuditObject();
                                hlpAuditObject.actncd = "D";
                                hlpAuditObject.auditcoldefid = auditColDefID;
                                hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_def_id";
                                hlpAuditObject.auditprnttblpkcolval = refDefId.ToString();
                                hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                                hlpAuditObject.audittblpkcolval = refDefId.ToString();
                                hlpAuditObject.cmnttxt = cuid + " deleted material " + partNumber + " from \"" + catalogDescription + "\" on " + DateTime.Now.ToString();
                                hlpAuditObject.cuid = cuid;
                                hlpAuditObject.oldcolval = containedInCDMMSId.ToString();
                                hlpAuditObject.newcolval = "";
                                hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                                hlpAuditObjects.Add(hlpAuditObject);

                                hlpAuditObject = new HLPAuditObject();
                                hlpAuditObject.actncd = "D";
                                hlpAuditObject.auditcoldefid = auditColDefID;
                                hlpAuditObject.auditprnttblpkcolnm = "hlp_mtrl_revsn_id";
                                hlpAuditObject.auditprnttblpkcolval = savedHlpId;
                                hlpAuditObject.audittblpkcolnm = "hlp_mtrl_revsn_def_id";
                                hlpAuditObject.audittblpkcolval = refDefId.ToString();
                                hlpAuditObject.cmnttxt = "";
                                hlpAuditObject.cuid = cuid;
                                hlpAuditObject.oldcolval = "";
                                hlpAuditObject.newcolval = "";
                                hlpAuditObject.materialitemid = containedInCDMMSId.ToString();
                                hlpAuditObjects.Add(hlpAuditObject);
                                #endregion
                            }

                            helper.AddToDoItem(new ContainedInItem(refDefId.ToString(), matCat, "DELETE", containedInCDMMSId.ToString()));
                        }
                    }

                    await InsertAudits(hlpAuditObjects);

                    CommitTransaction();

                    helper.ProcessToDoItems();

                    status += helper.MasterWorkToDoId + "}";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to update Contained in terms HLP reference list Material Code - ({0})", hlpMtrlCd);

                    throw ex;
                }
                finally
                {
                    Dispose();
                }
            });

            return status;
        }

        public async Task<string> InsertAudits(List<HLPAuditObject> hlpAuditObjects)
        {
            string status = String.Empty;
            AuditDbInterface auditDbInterface = new AuditDbInterface();

            await Task.Run(async() =>
            {
                foreach (HLPAuditObject auditObject in hlpAuditObjects)
                {
                    status = await auditDbInterface.InsertAudit(long.Parse(auditObject.auditcoldefid), auditObject.audittblpkcolnm, auditObject.audittblpkcolval,
                        auditObject.auditprnttblpkcolnm, auditObject.auditprnttblpkcolval, auditObject.actncd, auditObject.oldcolval,
                        auditObject.newcolval, auditObject.cuid, auditObject.cmnttxt);
                }
            });
            return status;
        }

        private string GetAuditTable (string matcat, string feature)
        {
            string auditTable = String.Empty;

            if (feature != String.Empty)
            {
                switch (feature)
                {
                    case "Card":
                        auditTable = "hlp_cntnd_in_card_mtrl";
                        break;
                    case "Bay":
                        auditTable = "hlp_cntnd_in_bay_mtrl";
                        break;
                    case "Bay Extender":
                        auditTable = "hlp_cntnd_in_bay_extndr_mtrl";
                        break;
                    case "Node":
                        auditTable = "hlp_cntnd_in_node_mtrl";
                        break;
                    case "Shelf":
                        auditTable = "hlp_cntnd_in_shelf_mtrl";
                        break;
                    case "Plug-In":
                        auditTable = "hlp_cntnd_in_plg_in_mtrl";
                        break;
                    case "Connectorized/Set Length":
                        auditTable = "hlp_cntnd_in_cnctrzd_cabl_mtrl";
                        break;
                    case "Bulk":
                        auditTable = "hlp_cntnd_in_bulk_cabl_mtrl";
                        break;
                    //case "Variable Length":
                    //    auditTable = "hlp_cntnd_in_something";
                    //    break;
                    default:
                        auditTable = "hlp_cntnd_in_non_rme_mtrl";
                        break;
                }

            }
            else
            {
                switch (matcat)
                {
                    case "High Level Part":
                        auditTable = "hlp_cntnd_in_hlp_mtrl";
                        break;
                    case "Minor Material":
                        auditTable = "hlp_cntnd_in_mnr_mtrl";
                        break;
                    default:
                        auditTable = "hlp_cntnd_in_mnr_mtrl";
                        break;
                }
            }

            return auditTable;
        }

        public bool QuantityDefinitionExists(long revisionDefinitionId, string type)
        {
            bool exists = false;
            int count = 0;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pDefId", DbType.Int64, revisionDefinitionId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pTyp", DbType.String, type, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("oCnt", DbType.Int64, count, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.quantity_definition_exists", parameters);

                count = int.Parse(parameters[2].Value.ToString());

                if (count > 0)
                    exists = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve quantity definition count: {0}, {1}", revisionDefinitionId, type);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return exists;
        }

        public async Task<string> CloneContainedInTermsList(string prntMtrlCd, JArray citLst)
        {
            string hlpMtrlCd = "", matCat, ftyTyp, rvsnInd, frntRer;
            int qty, sprQty, relatedTo;
            string status = "";

            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    for (int i = 0; i < citLst.Count; i++)
                    {
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("material_code").Value<string>("value")))
                            hlpMtrlCd = "";
                        else
                            hlpMtrlCd = citLst[i].Value<JObject>("material_code").Value<string>("value");
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("material_category").Value<string>("value")))
                            matCat = "";
                        else
                            matCat = citLst[i].Value<JObject>("material_category").Value<string>("value");
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("feature_type").Value<string>("value")))
                            ftyTyp = "";
                        else
                            ftyTyp = citLst[i].Value<JObject>("feature_type").Value<string>("value");
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("is_revision").Value<string>("value")))
                            rvsnInd = "";
                        else
                            rvsnInd = citLst[i].Value<JObject>("is_revision").Value<string>("value");
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("placement_front_rear").Value<string>("value")))
                            frntRer = "";
                        else
                            frntRer = citLst[i].Value<JObject>("placement_front_rear").Value<string>("value");
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("quantity").Value<string>("value")))
                            qty = 0;
                        else
                            qty = int.Parse(citLst[i].Value<JObject>("quantity").Value<string>("value"));
                        if (string.IsNullOrEmpty(citLst[i].Value<JObject>("spare_quantity").Value<string>("value")))
                            sprQty = 0;
                        else
                            sprQty = int.Parse(citLst[i].Value<JObject>("spare_quantity").Value<string>("value"));
                        
                            relatedTo = 0;
                        
                        bool IsSelected = citLst[i].Value<JObject>("is_selected").Value<bool>("bool");
                        if (IsSelected)
                        {
                            InsertContainedInTerms(prntMtrlCd, hlpMtrlCd, matCat, ftyTyp, rvsnInd,frntRer,relatedTo,qty,sprQty,true);
                        }
                    }
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to clone Contained in terms HLP reference list to Material Code - ({0})", hlpMtrlCd);
                }
                finally
                {
                    Dispose();
                }
            });
            return status;
        }

        public long InsertContainedInTerms(string prntMtrlCd, string hlpMtrlCd, string matCat, string ftyTyp, string rvsnInd,
            string frontRear, int prntDefId, int mtrlQty, int mtrlSprQty, bool isClone)
        {
            IDbDataParameter[] parameters = null;
            long defId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(11);

                parameters[0] = dbAccessor.GetParameter("pPrntMtlCd", DbType.String, prntMtrlCd, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtlCd", DbType.String, hlpMtrlCd, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCinMtrlCtg", DbType.String, matCat, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCinFtrTyp", DbType.String, ftyTyp, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCinRvsn", DbType.String, rvsnInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, frontRear, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrntDefId", DbType.String, CheckNullValue(prntDefId), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pCinMtrlQty", DbType.Int64, mtrlQty, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pCinMtrlSprQty", DbType.Int64, mtrlSprQty, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pIsClone", DbType.Boolean, isClone, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("oDefId", DbType.Int64, defId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.insert_hlp_contained_in_parts", parameters);
                //PROCEDURE insert_hlp_contained_in_parts(pPrntMtlCd IN VARCHAR2, pMtlCd IN VARCHAR2, pCinMtrlCtg IN VARCHAR2, 
                //pCinFtrTyp IN VARCHAR2, pCinRvsn IN VARCHAR2, pFrntRerInd IN VARCHAR2, pPrntDefId IN NUMBER,
                //pCinMtrlQty IN NUMBER, pCinMtrlSprQty IN NUMBER, pIsClone IN BOOLEAN, oDefId OUT NUMBER);

                defId = long.Parse(parameters[10].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert cdmms id to contained in terms list for hlp number ({0}, {1})", prntMtrlCd, hlpMtrlCd);
                throw ex;
            }
            finally
            {
            }

            return defId;
        }
        public void UpdateContainedInTerms(int refDefId, int relatedTo, string frntRer, string rvsnInd, int qty, int sprQty, string matCat, string ftrTyp, decimal xCoordNo, decimal yCoordNo)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                // IN NUMBER, IN NUMBER,  IN VARCHAR2,  IN VARCHAR2, IN NUMBER, IN NUMBER, IN VARCHAR2,  IN VARCHAR2
                parameters = dbAccessor.GetParameterArray(10);
                parameters[0] = dbAccessor.GetParameter("pRevDefId", DbType.Int64, refDefId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRelatedTo", DbType.Int64, CheckNullValue(relatedTo), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pFrntRer", DbType.String, frntRer, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRvsn", DbType.String, rvsnInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pQty", DbType.Int64, qty, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pSprQty", DbType.Int64, sprQty, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pMtrlCtg", DbType.String, matCat, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pFtrTyp", DbType.String, ftrTyp, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pXCoord", DbType.Decimal, xCoordNo, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pYCoord", DbType.Decimal, yCoordNo, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.update_hlp_contained_in_parts", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update contained in terms list for hlp number ({0}, {1},{2})", refDefId, matCat, ftrTyp);
                throw ex;
            }
            finally
            {
            }

        }

        public bool CheckContainedInTermsAvailablity(string cdmmsid, IAccessor dbManager)
        {
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            try
            {
                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.String, cdmmsid, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("HLP_MTRL_PKG.check_contained_in_parts", parameters);
                if (reader.Read())
                {
                    return true;
                }
                else { return false; }
            }
            catch
            {
                return false;
            }
        }

        public void DeleteContainedInTerms(int refDefid, string mtrlCode)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(1);
                parameters[0] = dbAccessor.GetParameter("pRevDefId", DbType.Int64, refDefid, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.delete_hlp_contained_in_parts", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete cdmms id to contained in terms list for hlp number ({0})", mtrlCode);
                throw ex;
            }
            finally
            {
            }
        }

        public async Task<string> ValidateChildHLP(string prntFeatTyp, JArray HlpLst)
        {
            string status = "success";
            string featTyp;
            await Task.Run(() =>
            {
                try
                {
                    if (HlpLst != null)
                    {
                        StartTransaction();

                        for (int i = 0; i < HlpLst.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(HlpLst[i].Value<JObject>("Attributes").Value<JObject>("ftr_typ").Value<string>("value")))
                            {
                                featTyp = HlpLst[i].Value<JObject>("Attributes").Value<JObject>("ftr_typ").Value<string>("value").ToString();
                                if (ValidContainedInFeatureType(featTyp))
                                {
                                    if (!ValidChildFeatureType(prntFeatTyp, featTyp))
                                    {
                                        status = "Not a valid child feature type. ID: " + HlpLst[i].Value<JObject>("material_item_id").Value<string>("value").ToString();
                                        break;
                                    }
                                }
                                else
                                {
                                    status = "Not a valid feature type. ID: " + HlpLst[i].Value<JObject>("material_item_id").Value<string>("value").ToString();
                                    break;
                                }
                            }
                        }
                        CommitTransaction();
                    }
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to validate HLP in ValidateChildHLP");
                }
                finally
                { Dispose(); }
            });
            return status;
        }
        public bool ValidContainedInFeatureType(string featTyp)
        {
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string validCinFeatTyp;
            try
            {
                parameters = dbAccessor.GetParameterArray(2);
                parameters[0] = dbAccessor.GetParameter("pFtrTyp", DbType.String, featTyp, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameterCursorType("retcsr", ParameterDirection.Output);
                reader = dbAccessor.ExecuteDataReaderSP("hlp_mtrl_pkg.valid_contained_in_ftrtyp", parameters);
                while (reader.Read())
                {
                    validCinFeatTyp = DataReaderHelper.GetNonNullValue(reader, "cntnd_in_allow_ind");
                    if (validCinFeatTyp.ToUpper() == "Y") { return true; }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        public bool ValidChildFeatureType(string prntFeatTyp, string featTyp)
        {
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string childFeatTyp = "";
            try
            {
                parameters = dbAccessor.GetParameterArray(2);
                parameters[0] = dbAccessor.GetParameter("pPrntFtrTyp", DbType.String, prntFeatTyp, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameterCursorType("retcsr", ParameterDirection.Output);
                reader = dbAccessor.ExecuteDataReaderSP("hlp_mtrl_pkg.valid_child_ftrtyp", parameters);
                if (reader.Read())
                {
                    while (reader.Read())
                    {
                        childFeatTyp = DataReaderHelper.GetNonNullValue(reader, "child_feat_typ_id");
                        if (featTyp == childFeatTyp)
                        {
                            return true;
                        }
                    }
                }
                else { return true; }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return false;
        }
        public async Task<bool> ValidHlpMaterialCode(string MaterialCode)
        {
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            bool valid=false;

            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(2);
                    parameters[0] = dbAccessor.GetParameter("pMtrlCd", DbType.String, MaterialCode, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbAccessor.ExecuteDataReaderSP("hlp_mtrl_pkg.valid_hlp_mtrlcd", parameters);
                    if (reader.Read())
                    {
                        valid= true;
                    }
                 
                }
                catch (Exception ex)
                {
                    
                    logger.Error(ex, "Unable to verify valid HLP material code: {0}", MaterialCode);
                    valid =false;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                }
            });
            return valid;
        }
        public async Task<string> GetCdmmsIdOfMtrlCd(string MaterialCode)
        {
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string cdmmsId="";
            await Task.Run(() =>
            {
                try
                {
                    
                    parameters = dbAccessor.GetParameterArray(2);
                    parameters[0] = dbAccessor.GetParameter("pMtrlCd", DbType.String, MaterialCode, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbAccessor.ExecuteDataReaderSP("hlp_mtrl_pkg.get_id_hlpmtrlcd", parameters);
                    if (reader.Read())
                    {
                        cdmmsId = DataReaderHelper.GetNonNullValue(reader, "material_item_id");
                    }
                   
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get cdmmsd of valid HLP material code: {0}", MaterialCode);
                    cdmmsId = "";
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                }

            });
            return cdmmsId;
        }

        public string GetCommonConfigString(int materialItemId)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string commonConfigString = "";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_common_config_string", parameters);
                if (reader.Read())
                {
                    commonConfigString = DataReaderHelper.GetNonNullValue(reader, "comn_cnfg_string");
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get common config string: {0}", materialItemId);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            
            return commonConfigString;
        }

        public async Task<List<string>> GetRelatedToPartNumbers(int materialItemId, int featureTypeID)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            List<string> partNumbers = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(3);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pCinFtrTypId", DbType.Int32, featureTypeID, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_related_to_part_numbers", parameters);
                    while (reader.Read())
                    {
                        partNumbers.Add(DataReaderHelper.GetNonNullValue(reader, "part_no"));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get related to part number for CDMMS ID: {0}", materialItemId);
                    partNumbers = new List<string>();
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });
            return partNumbers;
        }

        public List<string> GetRelatedToPartNumbers2(int materialItemId, int featureTypeID, ref Hashtable htParts)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters2 = null;
            List<string> partNumbers = new List<string>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters2 = dbManager.GetParameterArray(3);

                parameters2[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters2[1] = dbManager.GetParameter("pCinFtrTypId", DbType.Int32, featureTypeID, ParameterDirection.Input);
                parameters2[2] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_related_to_part_numbers", parameters2);

                while (reader.Read())
                {    
                    partNumbers.Add(DataReaderHelper.GetNonNullValue(reader, "part_no"));
                    htParts.Add( DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_def_id"),DataReaderHelper.GetNonNullValue(reader, "part_no"));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get related to part number for CDMMS ID: {0}", materialItemId);
                partNumbers = new List<string>();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }

            return partNumbers;
        }

        public List<string> GetChildrenDefIDs(int defID)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            List<string> childrenIDs = new List<string>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pDefID", DbType.Int32, defID, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_children_def_ids", parameters);
                while (reader.Read())
                {
                    childrenIDs.Add(DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_def_id"));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get related to part number for def id: {0}", defID);
                childrenIDs = new List<string>();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }
            return childrenIDs;
        }

        public bool HasAlias(int defID)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            bool hasAlias = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pDefID", DbType.Int32, defID, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_alias", parameters);
                while (reader.Read())
                {
                    string alias = reader["alias_val"].ToString();
                    if (alias != String.Empty)
                    {
                        hasAlias = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get related to part number for def id: {0}", defID);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }
            return hasAlias;
        }

        public void updateChildParentDefID(int defID)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pDefID", DbType.Int32, defID, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.update_child_parent_id", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update prnt_hlp_mtrl_revsn_def_id ({0})", defID);

                throw ex;
            }
        }

        public string GetPartNumberFromDefID(int materialItemId, int featureTypeID, int defID)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string partNumber = "";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pDefId", DbType.Int32, defID, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_part_number_from_def_id", parameters);

                while (reader.Read())
                {
                    partNumber = (DataReaderHelper.GetNonNullValue(reader, "part_no"));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get related to part number for CDMMS ID: {0}", materialItemId);
                partNumber = "";
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }

            return partNumber;
        }

        //HLPN pop up screen from Material Inventory page.

        public void GetMacroAssemblyDefinitionId(HighLevelPartNDSHelper helper)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtrlCd", DbType.String, helper.MaterialCode, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_ma_def_ids", parameters);

                while (reader.Read())
                {
                    helper.MaterialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "hlp_mtrl_revsn_id", true));
                    helper.MacroAssemblyDefinitionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "alias_val", true));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get MA Def id for material code: {0}", helper.MaterialCode);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public List<int> GetCommonConfigIdList(long materialItemId)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            List<int> defIds = new List<int>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("hlp_mtrl_pkg.get_comn_cnfg_def_ids", parameters);

                while (reader.Read())
                {
                    defIds.Add(int.Parse(reader["comn_cnfg_def_id"].ToString()));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get common config def ids for CDMMS ID: {0}", materialItemId);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }
            return defIds;
        }

        public void InsertHLPCommonConfigRef(int defID, long revsnDefId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pDefID", DbType.Int32, defID, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRevsnDefId", DbType.Int32, revsnDefId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.insert_hlp_comn_cnfg_ref", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update InsertHLPCommonConfigRef ({0}, {1})", defID, revsnDefId);

                throw ex;
            }
        }

        public void DeleteHLPCommonConfigRef(int defID)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pDefID", DbType.Int32, defID, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "hlp_mtrl_pkg.delete_hlp_comn_cnfg_refs", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update DeleteHLPCommonConfigRef ({0})", defID);

                throw ex;
            }
        }
    }

    public class HLPAuditObject
    {
        public string columnname { get; set; }
        public string tablename { get; set; }
        public string auditcoldefid { get; set; }
        public string audittblpkcolnm { get; set; }
        public string audittblpkcolval { get; set; }
        public string auditprnttblpkcolnm { get; set; }
        public string auditprnttblpkcolval { get; set; }
        public string actncd { get; set; }
        public string oldcolval { get; set; }
        public string newcolval { get; set; }
        public string cuid { get; set; }
        public string cmnttxt { get; set; }
        public string materialitemid { get; set; }
    }
}