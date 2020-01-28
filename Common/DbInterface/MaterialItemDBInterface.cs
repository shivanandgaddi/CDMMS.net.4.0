using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.TypeLibrary;
using NLog;

namespace CenturyLink.Network.Engineering.Common.DbInterface
{
    public class MaterialItemDBInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public MaterialItemDBInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public int InsertIntoMtlPSTableNew(CatalogItem dtoCatalogPart, string reviewIndicator)
        {
            int returnValue = -1;
            IAccessor dbManager = null;

            string sql = @"insert into mtl_item_sap (batch_id, stage_id, product_id, mfg_part_no, mfg_id, apcl_cd, unit_of_msrmt, category_id, " +
                            "item_desc, mfg_desc, item_current_status, icc_cd, mic_cd, additional_desc, avg_price_amt, hazard_ind, " +
                            "nstock_del_int_dur, last_updtd_usr_id, last_updtd_tmstmp, review_ind, zheight, zwidth, zdepth) " +
                            "values (:batch_id, :stage_id, :product_id, :mfg_part_no, :mfg_id, :apcl_cd, :unit_of_msrmt, :category_id, " +
                            ":item_desc, :mfg_desc, :item_current_status, :icc_cd, :mic_cd, :additional_desc, :avg_price_amt, :hazard_ind, " +
                            ":nstock_del_int_dur, 'CATALOG_SVC', SYSDATE, :review_ind, :zheight, :zwidth, :zdepth) ";

            ArrayList paramNames = new ArrayList();
            ArrayList paramValues = new ArrayList();

            try
            {
                paramNames.Add("batch_id");
                paramNames.Add("stage_id");
                paramNames.Add("product_id");
                paramNames.Add("mfg_part_no");
                paramNames.Add("mfg_id");
                paramNames.Add("apcl_cd");
                paramNames.Add("unit_of_msrmt");
                paramNames.Add("category_id");
                paramNames.Add("item_desc");
                paramNames.Add("mfg_desc");
                paramNames.Add("item_current_status");
                paramNames.Add("icc_cd");
                paramNames.Add("mic_cd");
                paramNames.Add("additional_desc");
                paramNames.Add("avg_price_amt");
                paramNames.Add("hazard_ind");
                paramNames.Add("nstock_del_int_dur");
                paramNames.Add("review_ind");
                paramNames.Add("zheight");
                paramNames.Add("zwidth");
                paramNames.Add("zdepth");

                paramValues.Add(dtoCatalogPart.BatchID.ToString());
                paramValues.Add(dtoCatalogPart.StageID.ToString());
                paramValues.Add(dtoCatalogPart.MATNR);  // product id
                paramValues.Add(dtoCatalogPart.MFRPN);  // manufacturing part number
                paramValues.Add(dtoCatalogPart.MFRNR);  // manufacturing id
                paramValues.Add(dtoCatalogPart.APCL_CODE);
                paramValues.Add(dtoCatalogPart.MEINS);  // unit of measurement
                paramValues.Add(dtoCatalogPart.MATKL);  // category id
                paramValues.Add(dtoCatalogPart.ZMAKTX);  // material description
                paramValues.Add(dtoCatalogPart.NAME1);  // manufacturing description
                paramValues.Add(dtoCatalogPart.LVORM);  // item status
                paramValues.Add(dtoCatalogPart.ICC_CODE);
                paramValues.Add(dtoCatalogPart.AIC_CODE);
                paramValues.Add(dtoCatalogPart.PO_TEXT);  // material description
                paramValues.Add(dtoCatalogPart.VERPR);  // average price amount
                paramValues.Add(dtoCatalogPart.KZUMW);  // hazard indicator
                paramValues.Add(dtoCatalogPart.PLIFZ);  // delete duration
                paramValues.Add(reviewIndicator);
                paramValues.Add(dtoCatalogPart.ZHEIGHT);
                paramValues.Add(dtoCatalogPart.ZWIDTH);
                paramValues.Add(dtoCatalogPart.ZDEPTH);

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql, paramNames, paramValues);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while inserting into mtl_item_sap - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.InsertIntoMtlPSTableNew({0}, {1})", dtoCatalogPart.MFRPN);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateMtlPSTableNew(CatalogItem dtoCatalogPart)
        {
            int returnValue = -1;
            IAccessor dbManager = null;

            ArrayList paramNames = new ArrayList();
            ArrayList paramValues = new ArrayList();

            string sql = @"update mtl_item_sap set " +
                "mfg_part_no = :mfg_part_no, mfg_id = :mfg_id, apcl_cd = :apcl_cd, unit_of_msrmt = :unit_of_msrmt, " +
                "category_id = :category_id, item_desc = :item_desc, mfg_desc = :mfg_desc, item_current_status = :item_current_status, " +
                "icc_cd = :icc_cd, mic_cd = :mic_cd, additional_desc = :additional_desc, avg_price_amt = :avg_price_amt, " +
                "hazard_ind = :hazard_ind, nstock_del_int_dur = :nstock_del_int_dur, review_ind = :review_ind, zheight = :zheight, " +
                "zwidth = :zwidth, zdepth = :zdepth, last_updtd_usr_id = 'CATALOG_SVC', last_updtd_tmstmp = sysdate where product_id = :product_id ";

            try
            {
                paramNames.Add("product_id");
                paramNames.Add("mfg_part_no");
                paramNames.Add("mfg_id");
                paramNames.Add("apcl_cd");
                paramNames.Add("unit_of_msrmt");
                paramNames.Add("category_id");
                paramNames.Add("item_desc");
                paramNames.Add("mfg_desc");
                paramNames.Add("item_current_status");
                paramNames.Add("icc_cd");
                paramNames.Add("mic_cd");
                paramNames.Add("additional_desc");
                paramNames.Add("avg_price_amt");
                paramNames.Add("hazard_ind");
                paramNames.Add("nstock_del_int_dur");
                paramNames.Add("review_ind");
                paramNames.Add("zheight");
                paramNames.Add("zwidth");
                paramNames.Add("zdepth");
                
                paramValues.Add(dtoCatalogPart.MATNR);  // product id
                paramValues.Add(dtoCatalogPart.MFRPN);  // manufacturing part number
                paramValues.Add(dtoCatalogPart.MFRNR);  // manufacturing id
                paramValues.Add(dtoCatalogPart.APCL_CODE);
                paramValues.Add(dtoCatalogPart.MEINS);  // unit of measurement
                paramValues.Add(dtoCatalogPart.MATKL);  // category id
                paramValues.Add(dtoCatalogPart.ZMAKTX);  // material description
                paramValues.Add(dtoCatalogPart.NAME1);  // manufacturing description
                paramValues.Add(dtoCatalogPart.LVORM);  // item status
                paramValues.Add(dtoCatalogPart.ICC_CODE);
                paramValues.Add(dtoCatalogPart.AIC_CODE);  // mic_cd
                paramValues.Add(dtoCatalogPart.PO_TEXT);  // material description
                paramValues.Add(dtoCatalogPart.VERPR);  // average price amount
                paramValues.Add(dtoCatalogPart.KZUMW);  // hazard indicator
                paramValues.Add(dtoCatalogPart.PLIFZ);  // delete duration
                paramValues.Add(dtoCatalogPart.ReviewIndicator);
                paramValues.Add(dtoCatalogPart.ZHEIGHT);
                paramValues.Add(dtoCatalogPart.ZWIDTH);
                paramValues.Add(dtoCatalogPart.ZDEPTH);

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql, paramNames, paramValues);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating mtl_item_sap - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.UpdateMtlPSTableNew({0}, {1})", dtoCatalogPart.MFRPN);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public CatalogItem GetMtlPSTableNew(string productID)
        {
            CatalogItem catalogItem = new CatalogItem();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select batch_id, stage_id, product_id, mfg_part_no, mfg_id, apcl_cd, unit_of_msrmt, " +
                "category_id, item_desc, mfg_desc, item_current_status, icc_cd, mic_cd, additional_desc, avg_price_amt, " +
                "hazard_ind, nstock_del_int_dur, review_ind, heci, alt_uom, dt_created, mtl_type, last_updtd_tmstmp, " +
                "zheight, zwidth, zdepth from mtl_item_sap where product_id = '" + productID + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    catalogItem.BatchID = long.Parse(reader["batch_id"].ToString());
                    catalogItem.StageID = long.Parse(reader["stage_id"].ToString());
                    catalogItem.MATNR = reader["product_id"].ToString();
                    catalogItem.MFRPN = reader["mfg_part_no"].ToString();
                    catalogItem.MFRNR = reader["mfg_id"].ToString();
                    catalogItem.APCL_CODE = reader["apcl_cd"].ToString();
                    catalogItem.MEINS = reader["unit_of_msrmt"].ToString();
                    catalogItem.MATKL = reader["category_id"].ToString();
                    catalogItem.ZMAKTX = reader["item_desc"].ToString();
                    catalogItem.NAME1 = reader["mfg_desc"].ToString();
                    catalogItem.LVORM = reader["item_current_status"].ToString();
                    catalogItem.ICC_CODE = reader["icc_cd"].ToString();
                    catalogItem.AIC_CODE = reader["mic_cd"].ToString();
                    catalogItem.PO_TEXT = reader["mfg_desc"].ToString();
                    catalogItem.VERPR = reader["avg_price_amt"].ToString();
                    catalogItem.KZUMW = reader["hazard_ind"].ToString();
                    catalogItem.PLIFZ = reader["nstock_del_int_dur"].ToString();
                    catalogItem.ReviewIndicator = reader["review_ind"].ToString();
                    catalogItem.ZHEIGHT = reader["zheight"].ToString();
                    catalogItem.ZWIDTH = reader["zwidth"].ToString();
                    catalogItem.ZDEPTH = reader["zdepth"].ToString();
                    catalogItem.HECI_CODE = reader["heci"].ToString();
                    catalogItem.BSTME = reader["alt_uom"].ToString();
                    if (reader["dt_created"] != null && reader["dt_created"].ToString() != String.Empty)
                    {
                        catalogItem.ERSDA = DateTime.Parse(reader["dt_created"].ToString());
                    }
                    catalogItem.MTART = reader["mtl_type"].ToString();
                    if (reader["last_updtd_tmstmp"] != null && reader["last_updtd_tmstmp"].ToString() != String.Empty)
                    {
                        catalogItem.LastUpdatedTimeStmp = DateTime.Parse(reader["last_updtd_tmstmp"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while reading mtl_item_sap - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMtlPSTableNew({0}, {1})", productID);
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

            return catalogItem;
        }

        public string GetMTLID_PRODID (string aliasTableName, string aliasValTableName, string aliasPKColumnName, string auditTablePKColumnValue)
        {
            string returnString = "";
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select distinct a.alias_val, a.mtrl_id from mtrl_alias_val a, mtrl b " +
                " where a.mtrl_id = (select distinct c.mtrl_id from " + aliasTableName + " c, " + aliasValTableName + " d " +
                " where c." + aliasPKColumnName + " = d." + aliasPKColumnName + " and d.alias_val = '" + auditTablePKColumnValue + "')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnString = reader["alias_val"].ToString() + "," + reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return returnString;
        }

        public void UpdateNeedsToBeReviewed(long batchID, long stagingID)
        {
            IAccessor dbManager = null;

            string sql = @"update material_catalog_staging set needs_to_be_reviewed = 'N' " +
                " where batch_id = " + batchID.ToString() + " and stage_id = " + stagingID.ToString();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating material_catalog_staging - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.UpdateNeedsToBeReviewed({0}, {1})", batchID.ToString(), stagingID.ToString());
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public List<IESEaCompCleiExtn> GetIESEaCompCleiExtn(string heci)
        {
            List<IESEaCompCleiExtn> iesEaCompCleiExtnList = new List<IESEaCompCleiExtn>();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select comp_clei_key, cleicode, compatibleequipmentclei7 " +
                            "from ies_ea_comp_clei_extn " +
                            "where compatibleequipmentclei7 = '" + heci.ToUpper() + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    IESEaCompCleiExtn iesEaCompCleiExten = new IESEaCompCleiExtn();
                    iesEaCompCleiExten.CompCLEIKey = int.Parse(reader["comp_clei_key"].ToString());
                    iesEaCompCleiExten.CLEICode = reader["cleicode"].ToString();
                    iesEaCompCleiExten.CompatibleEquipmentCLEI7 = reader["compatibleequipmentclei7"].ToString();
                    iesEaCompCleiExtnList.Add(iesEaCompCleiExten);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return iesEaCompCleiExtnList;
        }

        public List<LOSDBItem> GetLOSDBItems(string cleicode)
        {
            List<LOSDBItem> losdbItems = new List<LOSDBItem>();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select i.prod_id, i.eqpt_ctlg_item_id, i.ordg_cd, i.ls_or_srs, e.part_no, e.drwg, e.drwg_iss " +
                            "from ies_invntry i, ies_eqpt e " +
                            "where i.prod_id = e.prod_id " +
                            "and i.clei_cd = '" + cleicode.ToUpper() + "' " +
                            "order by e.part_no desc, i.ordg_cd desc, i.ls_or_srs desc, e.drwg desc, e.drwg_iss desc";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBItem losdbItem = new LOSDBItem();
                    losdbItem.OrderingCode = reader["ordg_cd"].ToString();
                    losdbItem.LsOrSrs = reader["ls_or_srs"].ToString();
                    losdbItem.PartNumber = reader["part_no"].ToString();
                    losdbItem.ProdID = reader["prod_id"].ToString();
                    losdbItem.Drawing = reader["drwg"].ToString();
                    losdbItem.DrawingISS = reader["drwg_iss"].ToString();
                    losdbItem.EquipmentCatalogItemID = reader["eqpt_ctlg_item_id"].ToString();
                    losdbItems.Add(losdbItem);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return losdbItems;
        }

        public List<LOSDBItem> GetLOSDBEquipment(string vendorCode)
        {
            List<LOSDBItem> losdbItems = new List<LOSDBItem>();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select prod_id, part_no, vndr_cd " +
                            "from ies_eqpt " +
                            "where vndr_cd = '" + vendorCode.ToUpper() + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBItem losdbItem = new LOSDBItem();
                    losdbItem.PartNumber = reader["part_no"].ToString();
                    losdbItem.ProdID = reader["prod_id"].ToString();
                    losdbItem.Vendorcode = reader["vndr_cd"].ToString();
                    losdbItems.Add(losdbItem);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return losdbItems;
        }

        public LOSDBItem GetLOSDBItems2(string prodID)
        {
            LOSDBItem losdbItem = new LOSDBItem();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select i.prod_id, i.eqpt_ctlg_item_id, i.ordg_cd, i.ls_or_srs, e.part_no, e.drwg, e.drwg_iss " +
                            "from ies_invntry i, ies_eqpt e " +
                            "where i.prod_id = e.prod_id " +
                            "and i.prod_id = " + prodID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    losdbItem.OrderingCode = reader["ordg_cd"].ToString();
                    losdbItem.LsOrSrs = reader["ls_or_srs"].ToString();
                    losdbItem.PartNumber = reader["part_no"].ToString();
                    losdbItem.ProdID = reader["prod_id"].ToString();
                    losdbItem.Drawing = reader["drwg"].ToString();
                    losdbItem.DrawingISS = reader["drwg_iss"].ToString();
                    losdbItem.EquipmentCatalogItemID = reader["eqpt_ctlg_item_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return losdbItem;
        }

        public List<LOSDBItem> GetLOSDBItems3(string vendorCode)
        {
            List<LOSDBItem> losdbItems = new List<LOSDBItem>();
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select i.prod_id, i.eqpt_ctlg_item_id, i.ordg_cd, i.ls_or_srs, e.part_no, e.drwg, e.drwg_iss " +
                            "from ies_invntry i, ies_eqpt e " +
                            "where i.prod_id = e.prod_id " +
                            "and e.vndr_cd = '" + vendorCode + "' " +
                            "order by e.part_no desc, i.ordg_cd desc, i.ls_or_srs desc, e.drwg desc, e.drwg_iss desc";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBItem losdbItem = new LOSDBItem();
                    losdbItem.OrderingCode = reader["ordg_cd"].ToString();
                    losdbItem.LsOrSrs = reader["ls_or_srs"].ToString();
                    losdbItem.PartNumber = reader["part_no"].ToString();
                    losdbItem.ProdID = reader["prod_id"].ToString();
                    losdbItem.Drawing = reader["drwg"].ToString();
                    losdbItem.DrawingISS = reader["drwg_iss"].ToString();
                    losdbItem.EquipmentCatalogItemID = reader["eqpt_ctlg_item_id"].ToString();
                    losdbItems.Add(losdbItem);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return losdbItems;
        }

        public bool DoesPartAssociationExist(string prodID, string eProdID, string equipmentCatalogItemID)
        {
            bool returnValue = false;
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select product_id from possible_part_associations " +
                            "where product_id = '" + prodID + "' and ies_eqpt_prod_id = " + eProdID + " and ies_eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1}, {2} )", prodID, eProdID, equipmentCatalogItemID);
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

            return returnValue;
        }

        public int InsertPossibleAssociation(string prodID, string eProdID, string equipmentCatalogItemID)
        {
            int returnValue = -1;
            IAccessor dbManager = null;

            string sql = @"insert into possible_part_associations (product_id, ies_eqpt_prod_id, ies_eqpt_ctlg_item_id) " +
                            "values ('" + prodID + "', " + eProdID + ",'" + equipmentCatalogItemID + "')";

            try
            {

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while inserting InsertPossibleAssociation - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.InsertPossibleAssociation({0}, {1}, {2})", prodID, eProdID, equipmentCatalogItemID);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }
    }
}
