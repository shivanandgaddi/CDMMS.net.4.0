using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;
using NLog;

namespace CenturyLink.Network.Engineering.LOSDB.Service.DBInterface
{
    class LOSDBDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public LOSDBDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public List<AuditObject> GetAuditObjects (string FlowThruIndicator, string NDSRequiredIndicator, string ActionCode)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<AuditObject> auditObjects = new List<AuditObject>();

            // Only care about objects that have CDMMS tables
            string @sql = "select a.audit_da_id, a.audit_ies_col_def_id, a.audit_ies_alias_def_id, a.audit_tbl_pk_col_nm, " +
                          "a.audit_tbl_pk_col_val, a.actn_cd, a.old_col_val, a.new_col_val, " +
                          "b.audit_tbl_def_id, b.audit_col_nm, b.flo_thru_ind, b.cdmms_tbl_nm, b.cdmms_col_nm, b.nds_reqr_ind, " +
                          "c.audit_tbl_nm, c.all_rcrds_ind, d.alias_tbl_nm, d.alias_val_tbl_nm " +
                          "from audit_ies_da a, audit_ies_col_def b, audit_ies_tbl_def c, audit_ies_alias_def d " +
                          "where a.audit_ies_col_def_id = b.audit_ies_col_def_id " +
                          "and b.audit_tbl_def_id = c.audit_tbl_def_id " +
                          "and a.audit_ies_alias_def_id = d.audit_ies_alias_def_id " +
                          "and b.cdmms_tbl_nm is not null " +
                          "and b.flo_thru_ind = '" + FlowThruIndicator + "' and nds_reqr_ind = '" + NDSRequiredIndicator + "' " +
                          "and a.actn_cd = '" + ActionCode + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    AuditObject auditObject = new AuditObject();
                    auditObject.AuditDaID = long.Parse(reader["audit_da_id"].ToString());
                    auditObject.AuditIesColDefID = long.Parse(reader["audit_ies_col_def_id"].ToString());
                    auditObject.AuditIesAliasDefID = long.Parse(reader["audit_ies_alias_def_id"].ToString());
                    auditObject.AuditTablePkColumnName = reader["audit_tbl_pk_col_nm"].ToString();
                    auditObject.AuditTablePkColumnValue = reader["audit_tbl_pk_col_val"].ToString();
                    auditObject.ActionCode = reader["actn_cd"].ToString();
                    auditObject.OldColumnValue = reader["old_col_val"].ToString();
                    auditObject.NewColumnValue = reader["new_col_val"].ToString();
                    auditObject.AuditTableDefID = reader["audit_tbl_def_id"].ToString();
                    auditObject.AuditColumnName = reader["audit_col_nm"].ToString();
                    auditObject.FlowThruIndicator = reader["flo_thru_ind"].ToString();
                    auditObject.CDMMSTableName = reader["cdmms_tbl_nm"].ToString();
                    auditObject.CDMMSColumnName = reader["cdmms_col_nm"].ToString();
                    auditObject.NDSRequiredIndicator = reader["nds_reqr_ind"].ToString();
                    auditObject.AuditTableName = reader["audit_tbl_nm"].ToString();
                    auditObject.AllRecordsIndicator = reader["all_rcrds_ind"].ToString();
                    auditObject.AliasTableName = reader["alias_tbl_nm"].ToString();
                    auditObject.AliasValueTableName = reader["alias_val_tbl_nm"].ToString();

                    auditObjects.Add(auditObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetAuditObjects - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetAuditObjects({0}, {1})", FlowThruIndicator, NDSRequiredIndicator);
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

            return auditObjects;
        }

        public AuditObject GetAuditObjectByID(string auditIesDaId)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            AuditObject auditObject = new AuditObject();

            string @sql = "select a.audit_da_id, a.audit_ies_col_def_id, a.audit_ies_alias_def_id, a.audit_tbl_pk_col_nm, " +
                          "a.audit_tbl_pk_col_val, a.actn_cd, a.old_col_val, a.new_col_val, " +
                          "a.audit_prnt_tbl_pk_col_nm, a.audit_prnt_tbl_pk_col_val, " +
                          "b.audit_tbl_def_id, b.audit_col_nm, b.flo_thru_ind, b.cdmms_tbl_nm, b.cdmms_col_nm, b.nds_reqr_ind, " +
                          "c.audit_tbl_nm, c.all_rcrds_ind, d.alias_tbl_nm, d.alias_val_tbl_nm " +
                          "from audit_ies_da a, audit_ies_col_def b, audit_ies_tbl_def c, audit_ies_alias_def d " +
                          "where a.audit_ies_col_def_id = b.audit_ies_col_def_id " +
                          "and b.audit_tbl_def_id = c.audit_tbl_def_id " +
                          "and a.audit_ies_alias_def_id = d.audit_ies_alias_def_id " +
                          "and b.cdmms_tbl_nm is not null " +
                          "and a.audit_da_id = " + auditIesDaId;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    auditObject.AuditDaID = long.Parse(reader["audit_da_id"].ToString());
                    auditObject.AuditIesColDefID = long.Parse(reader["audit_ies_col_def_id"].ToString());
                    auditObject.AuditIesAliasDefID = long.Parse(reader["audit_ies_alias_def_id"].ToString());
                    auditObject.AuditTablePkColumnName = reader["audit_tbl_pk_col_nm"].ToString();
                    auditObject.AuditTablePkColumnValue = reader["audit_tbl_pk_col_val"].ToString();
                    auditObject.ActionCode = reader["actn_cd"].ToString();
                    auditObject.OldColumnValue = reader["old_col_val"].ToString();
                    auditObject.NewColumnValue = reader["new_col_val"].ToString();
                    auditObject.AuditTableDefID = reader["audit_tbl_def_id"].ToString();
                    auditObject.AuditColumnName = reader["audit_col_nm"].ToString();
                    auditObject.FlowThruIndicator = reader["flo_thru_ind"].ToString();
                    auditObject.CDMMSTableName = reader["cdmms_tbl_nm"].ToString();
                    auditObject.CDMMSColumnName = reader["cdmms_col_nm"].ToString();
                    auditObject.NDSRequiredIndicator = reader["nds_reqr_ind"].ToString();
                    auditObject.AuditTableName = reader["audit_tbl_nm"].ToString();
                    auditObject.AllRecordsIndicator = reader["all_rcrds_ind"].ToString();
                    auditObject.AliasTableName = reader["alias_tbl_nm"].ToString();
                    auditObject.AliasValueTableName = reader["alias_val_tbl_nm"].ToString();
                    auditObject.AuditParentTablePKColumnName = reader["audit_prnt_tbl_pk_col_nm"].ToString();
                    auditObject.AuditParentTablePkColumnValue = reader["audit_prnt_tbl_pk_col_val"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetAuditObjects - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetAuditObjects({0}, {1})");
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

            return auditObject;
        }

        public AuditObject GetDeleteObject(string auditIesColDefID, string auditTablePkColVal)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            AuditObject auditObject = new AuditObject();

            string @sql = "select a.audit_da_id, a.audit_ies_col_def_id, a.audit_ies_alias_def_id, a.audit_tbl_pk_col_nm, " +
                          "a.audit_tbl_pk_col_val, a.actn_cd, a.old_col_val, a.new_col_val, " +
                          "b.audit_tbl_def_id, b.audit_col_nm, b.flo_thru_ind, b.cdmms_tbl_nm, b.cdmms_col_nm, b.nds_reqr_ind, " +
                          "c.audit_tbl_nm, c.all_rcrds_ind, d.alias_tbl_nm, d.alias_val_tbl_nm " +
                          "from audit_ies_da a, audit_ies_col_def b, audit_ies_tbl_def c, audit_ies_alias_def d " +
                          "where a.audit_ies_col_def_id = b.audit_ies_col_def_id " +
                          "and b.audit_tbl_def_id = c.audit_tbl_def_id " +
                          "and a.audit_ies_alias_def_id = d.audit_ies_alias_def_id " +
                          "and a.actn_cd = 'D' " +
                          "and a.audit_ies_col_def_id = " + auditIesColDefID +
                          " and a.audit_tbl_pk_col_val = '" + auditTablePkColVal + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    auditObject.AuditDaID = long.Parse(reader["audit_da_id"].ToString());
                    auditObject.AuditIesColDefID = long.Parse(reader["audit_ies_col_def_id"].ToString());
                    auditObject.AuditIesAliasDefID = long.Parse(reader["audit_ies_alias_def_id"].ToString());
                    auditObject.AuditTablePkColumnName = reader["audit_tbl_pk_col_nm"].ToString();
                    auditObject.AuditTablePkColumnValue = reader["audit_tbl_pk_col_val"].ToString();
                    auditObject.ActionCode = reader["actn_cd"].ToString();
                    auditObject.OldColumnValue = reader["old_col_val"].ToString();
                    auditObject.NewColumnValue = reader["new_col_val"].ToString();
                    auditObject.AuditTableDefID = reader["audit_tbl_def_id"].ToString();
                    auditObject.AuditColumnName = reader["audit_col_nm"].ToString();
                    auditObject.FlowThruIndicator = reader["flo_thru_ind"].ToString();
                    auditObject.CDMMSTableName = reader["cdmms_tbl_nm"].ToString();
                    auditObject.CDMMSColumnName = reader["cdmms_col_nm"].ToString();
                    auditObject.NDSRequiredIndicator = reader["nds_reqr_ind"].ToString();
                    auditObject.AuditTableName = reader["audit_tbl_nm"].ToString();
                    auditObject.AllRecordsIndicator = reader["all_rcrds_ind"].ToString();
                    auditObject.AliasTableName = reader["alias_tbl_nm"].ToString();
                    auditObject.AliasValueTableName = reader["alias_val_tbl_nm"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetAuditObjects - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetAuditObjects({0}, {1})");
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

            return auditObject;
        }

        public List<AuditObject> GetNonVendorAuditObjects(string FlowThruIndicator, string NDSRequiredIndicator, string ActionCode)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<AuditObject> auditObjects = new List<AuditObject>();

            // Only care about objects that have CDMMS tables
            string @sql = "select a.audit_da_id, a.audit_ies_col_def_id, a.audit_ies_alias_def_id, a.audit_tbl_pk_col_nm, " +
                          "a.audit_tbl_pk_col_val, a.actn_cd, a.old_col_val, a.new_col_val, " +
                          "b.audit_tbl_def_id, b.audit_col_nm, b.flo_thru_ind, b.cdmms_tbl_nm, b.cdmms_col_nm, b.nds_reqr_ind, " +
                          "c.audit_tbl_nm, c.all_rcrds_ind, d.alias_tbl_nm, d.alias_val_tbl_nm " +
                          "from audit_ies_da a, audit_ies_col_def b, audit_ies_tbl_def c, audit_ies_alias_def d " +
                          "where a.audit_ies_col_def_id = b.audit_ies_col_def_id " +
                          "and b.audit_tbl_def_id = c.audit_tbl_def_id " +
                          "and a.audit_ies_alias_def_id = d.audit_ies_alias_def_id " +
                          "and b.flo_thru_ind = '" + FlowThruIndicator + "' and nds_reqr_ind = '" + NDSRequiredIndicator + "' " +
                          "and a.actn_cd = '" + ActionCode + "' and b.audit_tbl_def_id <> 1";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    AuditObject auditObject = new AuditObject();
                    auditObject.AuditDaID = long.Parse(reader["audit_da_id"].ToString());
                    auditObject.AuditIesColDefID = long.Parse(reader["audit_ies_col_def_id"].ToString());
                    auditObject.AuditIesAliasDefID = long.Parse(reader["audit_ies_alias_def_id"].ToString());
                    auditObject.AuditTablePkColumnName = reader["audit_tbl_pk_col_nm"].ToString();
                    auditObject.AuditTablePkColumnValue = reader["audit_tbl_pk_col_val"].ToString();
                    auditObject.ActionCode = reader["actn_cd"].ToString();
                    auditObject.OldColumnValue = reader["old_col_val"].ToString();
                    auditObject.NewColumnValue = reader["new_col_val"].ToString();
                    auditObject.AuditTableDefID = reader["audit_tbl_def_id"].ToString();
                    auditObject.AuditColumnName = reader["audit_col_nm"].ToString();
                    auditObject.FlowThruIndicator = reader["flo_thru_ind"].ToString();
                    auditObject.CDMMSTableName = reader["cdmms_tbl_nm"].ToString();
                    auditObject.CDMMSColumnName = reader["cdmms_col_nm"].ToString();
                    auditObject.NDSRequiredIndicator = reader["nds_reqr_ind"].ToString();
                    auditObject.AuditTableName = reader["audit_tbl_nm"].ToString();
                    auditObject.AllRecordsIndicator = reader["all_rcrds_ind"].ToString();
                    auditObject.AliasTableName = reader["alias_tbl_nm"].ToString();
                    auditObject.AliasValueTableName = reader["alias_val_tbl_nm"].ToString();

                    auditObjects.Add(auditObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetNonVendorAuditObjects - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetNonVendorAuditObjects({0}, {1})", FlowThruIndicator, NDSRequiredIndicator);
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

            return auditObjects;
        }

        public List<FeatureType> GetFeatureTypeObject(string auditTablePKColumnName, string auditTablePKColumnValue)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<FeatureType> featureTypes = new List<FeatureType>();

            string @sql = "select feat_typ.cdmms_rt_tbl_nm, feat_typ.cdmms_revsn_tbl_nm, feat_typ.cdmms_alias_val_tbl_nm, " +
                            "mtrl_alias_val.alias_val" +
                            "from feat_typ, mtrl, mtrl_alias, mtrl_alias_val, audit_ies_da " +
                            "where feat_typ.feat_typ_id = mtrl.feat_typ_id " +
                            "and mtrl.mtrl_id = mtrl_alias_val.mtrl_id " +
                            "and mtrl_alias.mtrl_alias_id = mtrl_alias_val.mtrl_alias_id " +
                            "and mtrl_alias_val.alias_val = audit_ies_da.audit_tbl_pk_col_val " +
                            "and audit_ies_da.audit_tbl_pk_col_nm = mtrl_alias.attr_nm " +
                            "and audit_ies_da.audit_tbl_ok_col_nm = '" + auditTablePKColumnName + "' " +
                            "and audit_ies_da.audit_tbl_pk_col_val = '" + auditTablePKColumnValue + "' ";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    FeatureType featureType = new FeatureType();
                    featureType.CDMMSRTTableName = reader["cdmms_rt_tbl_nm"].ToString();
                    featureType.CDMMSRevisionTableName = reader["cdmms_revsn_tbl_nm"].ToString();
                    featureType.CDMMSAliasValTableName = reader["cdmms_alias_val_tbl_nm"].ToString();
                    featureType.AliasVal = reader["alias_val"].ToString();
                    featureTypes.Add(featureType);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetFeatureTypeObject - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetFeatureTypeObject({0}, {1})", auditTablePKColumnName, auditTablePKColumnValue);
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

            return featureTypes;
        }

        public string GetVendorName(string vendorCode)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string vendorName = "";

            string @sql = "select ies_vndr_cd.vndr_nm from ies_vndr_cd where ies_vndr_cd.vndr_cd = '" + vendorCode + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    vendorName = reader["vndr_nm"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetVendorName - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetVendorName({0})", vendorCode);
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

            return vendorName;
        }

        public List<VendorObject> GetNewVendors()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<VendorObject> vendorObjects = new List<VendorObject>();

            string @sql = "select vndr_cd, vndr_nm from ies_vndr_cd_trans where actn = 'A'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    VendorObject vendorObject = new VendorObject();
                    vendorObject.VendorCode = reader["vndr_cd"].ToString();
                    vendorObject.VendorName = reader["vndr_nm"].ToString();
                    vendorObjects.Add(vendorObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetNewVendors - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.GetNewVendors()");
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

            return vendorObjects;
        }

        public int UpdateVendorName(string vendorCode, string vendorName)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = @"update mfr set mfr_nm = '" + vendorName + "' where mfr_cd = '" + vendorCode + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateVendorName - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateVendorName({0}, {1})", vendorCode, vendorName);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public void InsertVendor(List<VendorObject>vendorObjects)
        {
            IAccessor dbManager = null;

            string sql = "";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                foreach (VendorObject vendorObject in vendorObjects)
                {
                    sql = @"insert into mfr (mfr_nm, mfr_cd, user_entr_ind, del_ind) values ('" + vendorObject.VendorName + "', '"+ vendorObject.VendorCode + "', 'N', 'N')";

                    dbManager.ExecuteNonQuery(CommandType.Text, sql);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating InsertVendor - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.InsertVendor()");
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public int UpdateMaterial(string tableName, string columnName, string oldValue, string newValue)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = @"update '" + tableName + "' set '" + columnName + "' = '" + newValue + "' where columnname = '" + oldValue + "'";
            // MIKE TODO This needs a where clause for ID

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateMaterial - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateMaterial({0}, {1}, {2}, {3})", tableName, columnName, oldValue, newValue);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateMaterialItem(string materialID, string eqpProdID, string equipmentCatalogItemID)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = @"update material_item set ies_eqpt_prod_id = " + eqpProdID + ", ies_eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "'" + " where material_item_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateMaterial - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateMaterialItem({0}, {1})", materialID, eqpProdID);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public List<LOSDBMainExtn> GetLOSDBMainExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<LOSDBMainExtn> losdbMainExtnCollection = new List<LOSDBMainExtn>();

            string @sql = "select action, cleicode, physicaldescription, stenciling, height_met_unit, height_met_value, " +
                "height_english_unit, height_english_value, width_met_unit, width_met_value, width_english_unit, width_english_value, " +
                "depth_met_unit, depth_met_value, depth_english_unit, depth_english_value, weight_met_unit, weight_met_value, " +
                "weight_english_unit, weight_english_value, maxpowerusage_unit, maxpowerusage_value, " +
                "operatingtemp_met_min_unit, operatingtemp_met_min_value, operatingtemp_met_max_unit, operatingtemp_met_max_value, " +
                "operatingtemp_eng_min_unit, operatingtemp_eng_min_value, operatingtemp_eng_max_unit, operatingtemp_eng_max_value, " +
                "storagetemp_met_min_unit, storagetemp_met_min_value, storagetemp_met_max_unit, storagetemp_met_max_value, " +
                "storagetemp_english_min_unit, storagetemp_english_min_value, storagetemp_english_max_unit, storagetemp_english_max_value, " +
                "humidity_min_unit, humidity_min_value, humidity_max_unit, humidity_max_value, " +
                "altitude_met_min_unit, altitude_met_min_value, altitude_met_max_unit, altitude_met_max_value, " +
                "altitude_english_min_unit, altitude_english_min_value, altitude_english_max_unit, altitude_english_max_value, " +
                "alarmcapable, pcnchange, hazardousmaterialindicator, orderingcode, " +
                "maxheatdissipation_met_unit, maxheatdissipation_met_value, maxheatdissipation_eng_unit, maxheatdissipation_eng_value " +
                "downloadablesoftware, manualintervention, " +
                "framespacing_met_unit, framespacing_met_value, framespacing_english_unit, framespacing_english_value, date_inserted " +
                "from losdb_main_extn";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBMainExtn losdbMainExtn = new LOSDBMainExtn();

                    losdbMainExtn.Action = reader["action"].ToString();
                    losdbMainExtn.CleiCode = reader["cleicode"].ToString();
                    losdbMainExtn.PhysicalDescription = reader["physicaldescription"].ToString();
                    losdbMainExtn.Stenciling = reader["stenciling"].ToString();
                    losdbMainExtn.HeightMetUnit = reader["height_met_unit"].ToString();
                    losdbMainExtn.HeightMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["height_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.HeightEnglishUnit = reader["height_english_unit"].ToString();
                    losdbMainExtn.HeightEnglishValue = (reader.IsDBNull(0)) ? float.Parse(reader["height_english_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.WidthMetUnit = reader["width_met_unit"].ToString();
                    losdbMainExtn.WidthMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["width_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.WidthEnglishUnit = reader["width_english_untit"].ToString();
                    losdbMainExtn.WidthEnglishValue = (reader.IsDBNull(0)) ? float.Parse(reader["width_english_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.DepthMetUnit = reader["depth_met_unit"].ToString();
                    losdbMainExtn.DepthMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["depth_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.DepthEnglishUnit = reader["depth_english_unit"].ToString();
                    losdbMainExtn.DepthEnglishValue = (reader.IsDBNull(0)) ? float.Parse(reader["depth_english_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.WeightMetUnit = reader["weight_met_unit"].ToString();
                    losdbMainExtn.WeightMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["weight_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.WeightEnglishUnit = reader["weight_english_unit"].ToString();
                    losdbMainExtn.WeightEnglishValue = (reader.IsDBNull(0)) ? float.Parse(reader["weight_english_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.MaxPowerUsageUnit = reader["maxpowerusage_unit"].ToString();
                    losdbMainExtn.MaxPowerUsageValue = (reader.IsDBNull(0)) ? float.Parse(reader["maxpowerusage_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.OperatingTempMetMinUnit = reader["operatingtemp_met_min_unit"].ToString();
                    losdbMainExtn.OperatingTempMetMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["operatingtemp_met_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.OperatingTempMetMaxUnit = reader["operatingtemp_met_max_unit"].ToString();
                    losdbMainExtn.OperatingTempMetMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["operatingtemp_met_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.OperatingTempEngMinUnit = reader["operatingtemp_eng_min_unit"].ToString();
                    losdbMainExtn.OperatingTempEngMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["operatingtemp_eng_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.OperatingTempEngMaxUnit = reader["operatingtemp_eng_max_unit"].ToString();
                    losdbMainExtn.OperatingTempEngMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["operatingtemp_eng_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.StorageTempMetMinUnit = reader["storagetemp_met_min_unit"].ToString();
                    losdbMainExtn.StorageTempMetMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["storagetemp_met_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.StorageTempMetMaxUnit = reader["storagetemp_met_max_unit"].ToString();
                    losdbMainExtn.StorageTempMetMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["storagetemp_met_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.StorageTempEnglishMinUnit = reader["storagetemp_english_min_unit"].ToString();
                    losdbMainExtn.StorageTempEnglishMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["storagetemp_english_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.StorageTempEnglishMaxUnit = reader["storagetemp_english_max_unit"].ToString();
                    losdbMainExtn.StorageTempEnglishMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["storagetemp_english_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.HumidityMinUnit = reader["humidity_min_unit"].ToString();
                    losdbMainExtn.HumidityMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["humidity_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.HumidityMaxUnit = reader["humidity_max_unit"].ToString();
                    losdbMainExtn.HumidityMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["humidity_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.AltitudeMetMinUnit = reader["altitude_met_min_unit"].ToString();
                    losdbMainExtn.AltitudeMetMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["altitude_met_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.AltitudeMetMaxUnit = reader["altitude_met_max_unit"].ToString();
                    losdbMainExtn.AltitudeMetMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["altitude_met_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.AltitudeEnglishMinUnit = reader["altitude_english_min_unit"].ToString();
                    losdbMainExtn.AltitudeEnglishMinValue = (reader.IsDBNull(0)) ? float.Parse(reader["altitude_english_min_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.AltitudeEnglishMaxUnit = reader["altitude_english_max_unit"].ToString();
                    losdbMainExtn.AltitudeEnglishMaxValue = (reader.IsDBNull(0)) ? float.Parse(reader["altitude_english_max_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.AlarmCapable = reader["alarmcapable"].ToString();
                    losdbMainExtn.PCNChange = reader["pcnchange"].ToString();
                    losdbMainExtn.OrderingCode = reader["orderingcode"].ToString();
                    losdbMainExtn.MaxHeatDissipationMetUnit = reader["maxheatdissipation_met_unit"].ToString();
                    losdbMainExtn.MaxHeatDissipationMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["maxheatdissipation_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.MaxHeatDissipationEngUnit = reader["maxheatdissipation_eng_unit"].ToString();
                    losdbMainExtn.MaxHeatDissipationEngValue = (reader.IsDBNull(0)) ? float.Parse(reader["maxheatdissipation_eng_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.FrameSpacingMetUnit = reader["framespacing_met_unit"].ToString();
                    losdbMainExtn.FrameSpacingMetValue = (reader.IsDBNull(0)) ? float.Parse(reader["framespacing_met_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.FrameSpacingEnglishUnit = reader["framespacing_english_unit"].ToString();
                    losdbMainExtn.FrameSpacingEnglishValue = (reader.IsDBNull(0)) ? float.Parse(reader["framespacing_english_value"].ToString().Trim()) : default(float);
                    losdbMainExtn.DateInserted = (reader.IsDBNull(0)) ? DateTime.Parse(reader["date_inserted"].ToString().Trim()) : default(DateTime);

                    losdbMainExtnCollection.Add(losdbMainExtn);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in LOSDB_MAIN_EXTN - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetLOSDBMainExtn()", "");
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

            return losdbMainExtnCollection;
        }

        public List<LOSDBMsdExtn> GetLOSDBMsdExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<LOSDBMsdExtn> losdbMsdExtnCollection = new List<LOSDBMsdExtn>();

            string @sql = "select cleicode, msdsrefno, msdsrefname, msdsrefphoneno, msdsrefurl, date_inserted from losdb_msd_extn";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBMsdExtn losdbMsdExtn = new LOSDBMsdExtn();

                    losdbMsdExtn.CleiCode = reader["cleicode"].ToString();
                    losdbMsdExtn.MSDSRefNo = reader["msdsrefno"].ToString();
                    losdbMsdExtn.MSDSRefName = reader["msdsrefname"].ToString();
                    losdbMsdExtn.MSDSRefPhoneNo = reader["msdsrefphoneno"].ToString();
                    losdbMsdExtn.MSDSRefUrl = reader["msdsrefurl"].ToString();
                    losdbMsdExtn.DateInserted = (reader.IsDBNull(0)) ? DateTime.Parse(reader["date_inserted"].ToString().Trim()) : default(DateTime);

                    losdbMsdExtnCollection.Add(losdbMsdExtn);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in LOSDB_MAIN_EXTN - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetLOSDBMsdExtn()", "");
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

            return losdbMsdExtnCollection;
        }

        public List<LOSDBCleiExtn> GetLOSDBCleiExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<LOSDBCleiExtn> losdbCleiExtnCollection = new List<LOSDBCleiExtn>();

            string @sql = "select cleicode, compatibleequipmentclei7, date_inserted from losdb_clei_extn";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBCleiExtn losdbCleiExtn = new LOSDBCleiExtn();

                    losdbCleiExtn.CleiCode = reader["cleicode"].ToString();
                    losdbCleiExtn.CompatibleEquipmentClei7 = reader["compatibleequipmentclei7"].ToString();
                    losdbCleiExtn.DateInserted = (reader.IsDBNull(0)) ? DateTime.Parse(reader["date_inserted"].ToString().Trim()) : default(DateTime);

                    losdbCleiExtnCollection.Add(losdbCleiExtn);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in LOSDB_MAIN_EXTN - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetLOSDBCleiExtn()", "");
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

            return losdbCleiExtnCollection;
        }

        public List<LOSDBElectricalExtn> GetLOSDBElectricalExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<LOSDBElectricalExtn> losdbElectricalExtnCollection = new List<LOSDBElectricalExtn>();

            string @sql = "select cleicode, er_inputvoltagefrom_unit, er_inputvoltagefrom_value, " +
                "er_inputvoltageto_unit, er_inputvoltageto_value, er_inputvoltagefreqfrom_unit, er_inputvoltagefreqfrom_value " +
                "er_inputvoltagefreqto_unit, er_inputvoltagefreqto_value, er_inputcurrentfrom_unit, er_inputcurrentfrom_value " +
                "er_inputcurrentto_unit, er_inputcurrentto_value, er_freeform_type, date_inserted";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    LOSDBElectricalExtn losdbElectricalExtn = new LOSDBElectricalExtn();

                    losdbElectricalExtn.CleiCode = reader["cleicode"].ToString();
                    losdbElectricalExtn.ERInputVoltageFromUnit = reader["er_inputvoltagefrom_unit"].ToString();
                    losdbElectricalExtn.ERInputVoltageFromValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltagefrom_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERInputVoltageToUnit = reader["er_inputvoltageto_unit"].ToString();
                    losdbElectricalExtn.ERInputVoltageToValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltageto_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERInputVoltageFreqFromUnit = reader["er_inputvoltagefreqfrom_unit"].ToString();
                    losdbElectricalExtn.ERInputVoltageFreqFromValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltagefreqfrom_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERInputVoltageFreqToUnit = reader["er_inputvoltagefreqto_unit"].ToString();
                    losdbElectricalExtn.ERInputVoltageFreqToValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltagefreqto_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERInputCurrentFromUnit = reader["er_inputvoltagefrom_unit"].ToString();
                    losdbElectricalExtn.ERInputCurrentFromValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltagefrom_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERInputCurrentToUnit = reader["er_inputvoltageto_unit"].ToString();
                    losdbElectricalExtn.ERInputCurrentToValue = (reader.IsDBNull(0)) ? float.Parse(reader["er_inputvoltageto_value"].ToString().Trim()) : default(float);
                    losdbElectricalExtn.ERFreeFormType = reader["er_freeform_type"].ToString();
                    losdbElectricalExtn.DateInserted = (reader.IsDBNull(0)) ? DateTime.Parse(reader["date_inserted"].ToString().Trim()) : default(DateTime);

                    losdbElectricalExtnCollection.Add(losdbElectricalExtn);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in LOSDB_MAIN_EXTN - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetLOSDBElectricalExtn()", "");
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

            return losdbElectricalExtnCollection;
        }

        public List<VendorObject> GetDeletedVendors()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<VendorObject> vendorObjects = new List<VendorObject>();

            string @sql = "select vndr_cd, vndr_nm from ies_vndr_cd where vndr_nm in " +
                            "(select distinct(audit_tbl_pk_col_val) as vndr_nm from audit_ies_da " +
                            "where actn_cd = 'D' and creat_acty_tmstmp > (sysdate-5) " +
                            "and audit_tbl_pk_col_nm = 'VENDR_CD')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    VendorObject vendorObject = new VendorObject();

                    vendorObject.VendorCode = reader["vndr_cd"].ToString();
                    vendorObject.VendorName = reader["vndr_nm"].ToString();

                    vendorObjects.Add(vendorObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedVendors - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedVendors()", "");
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

            return vendorObjects;
        }

        public List<EquipmentObject> GetDeletedEquipment()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<EquipmentObject> equipmentObjects = new List<EquipmentObject>();

            string @sql = "select vndr_cd, drwg, drwg_iss, descr from ies_eqpt where prod_id in " +
                           "(select distinct(audit_tbl_pk_col_val) as prod_id " +
                           "from audit_ies_da where actn_cd = 'D' and creat_acty_tmstmp > (sysdate-5) " +
                           "and audit_tbl_pk_col_nm = 'PROD_ID')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    EquipmentObject equipmentObject = new EquipmentObject();

                    equipmentObject.VendorCode = reader["vndr_cd"].ToString();
                    equipmentObject.Drawing = reader["drwg"].ToString();
                    equipmentObject.DrawingISS = reader["drwg_iss"].ToString();
                    equipmentObject.Description = reader["descr"].ToString();

                    equipmentObjects.Add(equipmentObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedEquipment - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedEquipment()", "");
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

            return equipmentObjects;
        }

        public List<InventoryObject> GetDeletedInventory()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<InventoryObject> inventoryObjects = new List<InventoryObject>();

            string @sql = "select clei_cd, ls_or_srs, descr, ordg_cd from ies_invntry where eqpt_ctlg_item_id in " +
                            "(select distinct(audit_tbl_pk_col_val) as eqpt_ctlg_item_id " +
                            "from audit_ies_da where actn_cd = 'D' and creat_acty_tmstmp > (sysdate-5) " +
                            "and audit_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    InventoryObject inventoryObject = new InventoryObject();

                    inventoryObject.CleiCode = reader["clei_cd"].ToString();
                    inventoryObject.LsOrSrs = reader["ls_or_srs"].ToString();
                    inventoryObject.Description = reader["descr"].ToString();
                    inventoryObject.OrderingCode = reader["ordg_cd"].ToString();

                    inventoryObjects.Add(inventoryObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedInventory - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedInventory()", "");
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

            return inventoryObjects;
        }

        public List<CompCleiExtnObject> GetDeletedCompCleiExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<CompCleiExtnObject> compCleiExtnObjects = new List<CompCleiExtnObject>();

            string @sql = "select cleicode, compatibleequipmentclei7 from ies_ea_comp_clei_extn where comp_clei_key in " +
                    "(select distinct audit_prnt_tbl_pk_col_val as comp_clei_key " +
                    "from audit_ies_da where actn_cd = 'D' and creat_acty_tmstmp > (sysdate-5) " +
                    "and audit_tbl_pk_col_nm = 'COMP_CLEI_KEY' " +
                    "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                    "and audit_prnt_tbl_pk_col_val not in " +
                    "(select distinct audit_prnt_tbl_pk_col_val " +
                    "from audit_ies_da where actn_cd = 'A' " +
                    "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                    "and creat_acty_tmstmp > (sysdate-5) and audit_tbl_pk_col_nm = 'COMP_CLEI_KEY'))";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    CompCleiExtnObject compCleiExtnObject = new CompCleiExtnObject();

                    compCleiExtnObject.CleiCode = reader["cleicode"].ToString();
                    compCleiExtnObject.CompatibleEquipmentClei7 = reader["compatibleequipmentclei7"].ToString();

                    compCleiExtnObjects.Add(compCleiExtnObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedCompCleiExtn - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedCompCleiExtn()", "");
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

            return compCleiExtnObjects;
        }

        public List<ElectricalExtnObject> GetDeletedElectricalExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<ElectricalExtnObject> electricalExtnObjects = new List<ElectricalExtnObject>();

            string @sql = "select cleicode, er_inputvoltagefrom_unit, er_inputvoltagefrom_value, " +
                            "er_inputvoltageto_unit, er_inputvoltageto_value, " +
                            "er_inputvoltagefreqfrom_unit, er_inputvoltagefreqfrom_value, " +
                            "er_inputvoltagefreqto_unit, er_inputvoltagefreqto_value, " +
                            "er_inputcurrentfrom_unit, er_inputcurrentfrom_value, " +
                            "er_inputcurrentto_unit, er_inputcurrentto_value, " +
                            "er_freeform_type " +
                            "from ies_ea_electrical_extn where cleicode in " +
                            "(select distinct audit_prnt_tbl_pk_col_val as cleicode " +
                            "from audit_ies_da where actn_cd = 'D' and audit_tbl_pk_col_nm = 'ELECTRICAL_KEY' " +
                            "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                            "and creat_acty_tmstmp > (sysdate - 5) " +
                            "and audit_prnt_tbl_pk_col_val not in " +
                            "(select distinct audit_prnt_tbl_pk_col_val " +
                            "from audit_ies_da where actn_cd = 'A' and creat_acty_tmstmp > (sysdate - 5) " +
                            "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                            "and audit_tbl_pk_col_nm = 'ELECTRICAL_KEY')); ";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    ElectricalExtnObject electricalExtnObject = new ElectricalExtnObject();

                    electricalExtnObject.CleiCode = reader["cleicode"].ToString();
                    electricalExtnObject.ERInputVoltageFromUnit = reader["er_inputvoltagefrom_unit"].ToString();
                    electricalExtnObject.ERInputVoltageFromValue = reader["er_inputvoltagefrom_value"].ToString();
                    electricalExtnObject.ERInputVoltageToUnit = reader["er_inputvoltageto_unit"].ToString();
                    electricalExtnObject.ERInputVoltageToValue = reader["er_inputvoltageto_value"].ToString();
                    electricalExtnObject.ERInputVoltageFreqFromUnit = reader["er_inputvoltagefreqfrom_unit"].ToString();
                    electricalExtnObject.ERInputVoltageFreqFromValue = reader["er_inputvoltagefreqfrom_value"].ToString();
                    electricalExtnObject.ERInputVoltageFreqToUnit = reader["er_inputvoltagefreqto_unit"].ToString();
                    electricalExtnObject.ERInputVoltageFreqToValue = reader["er_inputvoltagefreqto_value"].ToString();
                    electricalExtnObject.ERInputCurrentFromUnit = reader["er_inputcurrentfrom_unit"].ToString();
                    electricalExtnObject.ERInputCurrentFromValue = reader["er_inputcurrentfrom_value"].ToString();
                    electricalExtnObject.ERInputCurrentToUnit = reader["er_inputcurrentto_unit"].ToString();
                    electricalExtnObject.ERInputCurrentToValue = reader["er_inputcurrentto_value"].ToString();
                    electricalExtnObject.ERFreeFormType = reader["er_freeform_type"].ToString();

                    electricalExtnObjects.Add(electricalExtnObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedElectricalExtn - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedElectricalExtn()", "");
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

            return electricalExtnObjects;
        }

        public List<MainExtnObject> GetDeletedMainExtn()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<MainExtnObject> mainExtnObjects = new List<MainExtnObject>();

            string @sql = "select cleicode, physicaldescription, stenciling " +
                            "from ies_ea_main_extn where cleicode in " +
                            "(select distinct audit_prnt_tbl_pk_col_val as cleicode " +
                            "from audit_ies_da where actn_cd = 'D' and audit_tbl_pk_col_nm = 'CLEICODE' " +
                            "and creat_acty_tmstmp > (sysdate - 5) " +
                            "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                            "and audit_prnt_tbl_pk_col_val not in " +
                            "(select distinct audit_prnt_tbl_pk_col_val " +
                            "from audit_ies_da where actn_cd = 'A' and creat_acty_tmstmp > (sysdate - 5) " +
                            "and audit_prnt_tbl_pk_col_nm = 'EQPT_CTLG_ITEM_ID' " +
                            "and audit_tbl_pk_col_nm = 'CLEICODE')); ";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    MainExtnObject mainExtnObject = new MainExtnObject();

                    mainExtnObject.CleiCode = reader["cleicode"].ToString();
                    mainExtnObject.PhysicalDescription = reader["physicaldescription"].ToString();
                    mainExtnObject.Stenciling = reader["stenciling"].ToString();

                    mainExtnObjects.Add(mainExtnObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDeletedMainExtn - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetDeletedMainExtn()", "");
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

            return mainExtnObjects;
        }

        public List<EquipmentObject> GetNewEquipment(bool initialDataLoad)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<EquipmentObject> equipmentObjects = new List<EquipmentObject>();

            string sql = "select a.prod_id, a.eqpt_ctlg_item_id, a.vndr_cd, a.drwg, a.drwg_iss, a.part_no, b.ordg_cd, b.ls_or_srs, c.stenciling, c.orderingcode, NULL as passed " +
                            "from ies_eqpt_trans a, ies_invntry b, ies_ea_main_extn c " +
                            "where a.prod_id = b.prod_id and b.clei_cd = c.cleicode(+) and a.actn = 'A' " +
                            "order by part_no desc, ordg_cd desc, ls_or_srs desc, ordg_cd desc, stenciling desc, drwg desc, drwg_iss desc";

            if (initialDataLoad)
            {
                sql = @"select a.prod_id, a.eqpt_ctlg_item_id, a.vndr_cd, a.drwg, a.drwg_iss, a.part_no, b.ordg_cd, b.ls_or_srs, c.stenciling, c.orderingcode, 'Step 1' as passed
                        from ies_eqpt a, ies_invntry b, ies_ea_main_extn c, ies_ea_comp_clei_extn d, mtl_item_sap e
                        where a.prod_id = b.prod_id 
                        and d.compatibleequipmentclei7 = e.heci
                        and a.vndr_cd = e.mfg_id
                        and b.clei_cd = c.cleicode(+)
                        and b.clei_cd = d.cleicode(+)
                        union
                        select a.prod_id, a.eqpt_ctlg_item_id, a.vndr_cd, a.drwg, a.drwg_iss, a.part_no, b.ordg_cd, b.ls_or_srs, c.stenciling, c.orderingcode, 'Step 2' as passed
                        from ies_eqpt a, ies_invntry b, ies_ea_main_extn c, mtl_item_sap e
                        where a.prod_id = b.prod_id 
                        and e.heci = substr(b.clei_cd, 0, 7)
                        and a.vndr_cd = e.mfg_id
                        and b.clei_cd = c.cleicode(+)
                        order by prod_id, part_no desc, ls_or_srs desc, ordg_cd desc, stenciling desc, drwg desc, drwg_iss desc";
            }

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    EquipmentObject equipmentObject = new EquipmentObject();

                    equipmentObject.ProdID = reader["prod_id"].ToString();
                    equipmentObject.EquipmentCatalogItemID = reader["eqpt_ctlg_item_id"].ToString();
                    equipmentObject.VendorCode = reader["vndr_cd"].ToString();
                    equipmentObject.Drawing = reader["drwg"].ToString();
                    equipmentObject.DrawingISS = reader["drwg_iss"].ToString();
                    equipmentObject.PartNumber = reader["part_no"].ToString();
                    equipmentObject.OrderingCode = reader["ordg_cd"].ToString();
                    equipmentObject.LsOrSrs = reader["ls_or_srs"].ToString();
                    equipmentObject.Stenciling = reader["stenciling"].ToString();
                    equipmentObject.AlternateOrderingCode = reader["orderingcode"].ToString();
                    equipmentObject.Passed = reader["passed"].ToString();
                    equipmentObjects.Add(equipmentObject);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetNewEquipment - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetNewEquipment({0})", "");
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

            return equipmentObjects;
        }

        public string GetPartNumberLikeMatch(string something)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialID = "";

            string @sql = "select mtrl_id from mtrl where rt_part_no like '" + something + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialID = reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPartNumberLikeMatch - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetPartNumberLikeMatch({0})", "");
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

            return materialID;
        }

        public string GetPartNumberLikeMinusEqualMatch(string something)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialID = "";

            string @sql = "with items as " +
                    "(select mtrl_id, SUBSTR(rt_part_no, 0, LENGTH(rt_part_no) - 1) " +
                    "as rt_part_no from mtrl where rt_part_no like '%=') " +
                    "select mtrl_id, rt_part_no from items where rt_part_no like '" + something + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialID = reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPartNumberLikeMinusEqualMatch - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetPartNumberLikeMinusEqualMatch({0})", "");
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

            return materialID;
        }

        public string GetPartNumberEqualMatch(string something)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialID = "";

            string @sql = "select mtrl_id from mtrl where rt_part_no = '" + something + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialID = reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPartNumberEqualMatch - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetPartNumberEqualMatch({0})", "");
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

            return materialID;
        }

        public string GetPartNumberMinusEqualEqualMatch(string something)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialID = "";

            string @sql = "with items as " +
                    "(select mtrl_id, SUBSTR(rt_part_no, 0, LENGTH(rt_part_no) - 1) " +
                    "as rt_part_no from mtrl where rt_part_no like '%=') " +
                    "select mtrl_id, rt_part_no from items where rt_part_no = '" + something + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialID = reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPartNumberMinusEqualEqualMatch - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetPartNumberMinusEqualEqualMatch({0})", "");
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

            return materialID;
        }

        public void UpdateSAPMaterialWithAssociatedPart(string materialID, string eqpProdID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int32, Int32.Parse(materialID), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pKey", DbType.Int32, Int32.Parse(eqpProdID), ParameterDirection.Input);

                dbManager.ExecuteScalarSP("MTRL_PKG.INSERT_MTRL_ALIAS_VAL", parameters);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateSAPMaterialWithAssociatedPart - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateSAPMaterialWithAssociatedPart({0}, {1})", materialID, eqpProdID);
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public string GetSAPMaterialCode(string losdbProdID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialCode = "";

            string @sql = "select product_id from material_item where ies_eqpt_prod_id = '" + losdbProdID + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialCode = reader["product_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetSAPMaterialCode - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetSAPMaterialCode({0}, {1})", "");
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

            return materialCode;
        }

        public List<string> GetListOfAddsWithDelete()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> IDs = new List<string>();

            string @sql = @"SELECT AID.AUDIT_DA_ID
                            FROM AUDIT_IES_DA AID
                            WHERE AID.ACTN_CD IN ('A') AND
                            AID.USER_APRV_IND IS NULL AND
                            AID.AUDIT_PRNT_TBL_PK_COL_NM IS NOT NULL AND
                            AID.USER_APRV_IND IS NULL
                            AND EXISTS
                            (SELECT 1 FROM AUDIT_IES_DA AID1
                            WHERE AID1.AUDIT_TBL_PK_COL_VAL = AID.AUDIT_TBL_PK_COL_VAL
                            AND AID1.USER_APRV_IND IS NULL
                            AND AID1.AUDIT_IES_COL_DEF_ID = AID.AUDIT_IES_COL_DEF_ID
                            AND TRUNC(AID1.CREAT_ACTY_TMSTMP) = TRUNC(AID.CREAT_ACTY_TMSTMP)
                            AND AID1.ACTN_CD = 'D' AND AID1.AUDIT_PRNT_TBL_PK_COL_NM IS NOT NULL)";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    string ID = reader["audit_da_id"].ToString();
                    IDs.Add(ID);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetListOfAddsWithDelete - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetListOfAddsWithDelete()", "");
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

            return IDs;
        }

        public FeatureType GetFeatureByMtrlID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            FeatureType feature = new FeatureType();

            string @sql = "select feat_typ_id, cdmms_rt_tbl_nm, cdmms_revsn_tbl_nm, cdmms_alias_val_tbl_nm from feat_typ " +
                            "where feat_typ_id = " +
                            "(select feat_typ_id from mtrl where mtrl_id = " + materialID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    feature.FeatureTypeID = long.Parse(reader["feat_typ_id"].ToString());
                    feature.CDMMSRTTableName = reader["cdmms_rt_tbl_nm"].ToString();
                    feature.CDMMSRevisionTableName = reader["cdmms_revsn_tbl_nm"].ToString();
                    feature.CDMMSAliasValTableName = reader["cdmms_alias_val_tbl_nm"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetFeatureByMtrlID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetFeatureByMtrlID({0})", "");
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

            return feature;
        }

        public List<string> GetInventoryEquipmentCatalogItemIDs(string prodID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> equipmentIDs = new List<string>();

            string @sql = "select eqpt_ctlg_item_id from ies_invntry where prod_id = " + prodID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    equipmentIDs.Add(reader["eqpt_ctlg_item_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetInventoryEquipmentCatalogItemIDs - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetInventoryEquipmentCatalogItemIDs({0})", "");
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

            return equipmentIDs;
        }

        public void InsertEquipmentIDAliasVal(string aliasTableName, string revisionIDColumnName, string aliasIDColumnName, 
            string revisionID, string revisionAliasID, string aliasVal)
        {
            IAccessor dbManager = null;
            dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            string sql = @"insert into " + aliasTableName + " (" + revisionIDColumnName + "," + aliasIDColumnName + ",alias_val) " +
                            "values (" + revisionID + "," + revisionAliasID + ",'" + aliasVal + "')";

            try
            {
                    dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating InsertEquipmentIDAliasVal - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.InsertEquipmentIDAliasVal");
            }
            finally
            {
                dbManager.Dispose();
            }
        }
        public string GetRevisionID(string revisionIDColumnName, string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string revisionID = "";

            string revisionTableName = revisionIDColumnName.Substring(0, revisionIDColumnName.Length - 3);

            string @sql = "select distinct " + revisionIDColumnName + " from " + revisionTableName + " where mtrl_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    revisionID = reader[revisionIDColumnName].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetRevisionID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetRevisionID({0}, {1})", "");
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

            return revisionID;
        }

        public string GetMaterialCatID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialCatID = "";

            string @sql = "select mtrl_cat_id from mtrl where mtrl_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialCatID = reader["mtrl_cat_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetMaterialCatID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetMaterialCatID({0})", "");
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

            return materialCatID;
        }

        public List<string> GetElectricalKeys(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> electricalKeys = new List<string>();

            string @sql = "select electrical_key from ies_ea_electrical_extn where cleicode in " +
                            "(select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = " + equipmentCatalogItemID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    electricalKeys.Add(reader["electrical_key"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetElectricalKeys - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetElectricalKeys({0}, {1})", "");
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

            return electricalKeys;
        }

        public List<string> GetCompCleiKeys(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> compCleiKeys = new List<string>();

            string @sql = "select comp_clei_key from ies_ea_comp_clei_extn where cleicode in " +
                            "(select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = " + equipmentCatalogItemID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    compCleiKeys.Add(reader["comp_clei_key"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetCompCleiKeys - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetCompCleiKeys({0}, {1})", "");
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

            return compCleiKeys;
        }

        public List<string> GetCleiCodes(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> cleiCodes = new List<string>();

            string @sql = "select cleicode from ies_ea_main_extn where cleicode in " +
                            "(select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = " + equipmentCatalogItemID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    cleiCodes.Add(reader["cleicode"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetCleiCodes - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetCleiCodes({0}, {1})", "");
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

            return cleiCodes;
        }

        public string GetHlpRevisionID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string hlpRevisionID = "";

            string @sql = "select hlp_mtrl_revsn_id from hlp_mtrl_revsn where mtrl_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    hlpRevisionID = reader["hlp_mtrl_revsn_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetHlpRevisionID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetHlpRevisionID({0})", "");
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

            return hlpRevisionID;
        }

        public void InsertHighLevelPart(string materialID,string aliasID, string eqpProdID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);
                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int32, Int32.Parse(materialID), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pKey", DbType.Int32, Int32.Parse(aliasID), ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pKey", DbType.String, eqpProdID, ParameterDirection.Input);

                dbManager.ExecuteScalarSP("HLP_MTRL_PKG.INSERT_REVISION_ALIAS", parameters);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating InsertHighLevelPart - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.InsertHighLevelPart({0}, {1})", materialID, eqpProdID);
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public int InsertMatchTemp(string prodid, string vendorcode, string drawing, string drawingiss,
            string partnumber, string orderingcode, string lsorsrs, string alternateorderingcode,
            string materialid, string passed)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = "insert into losdb_matches_temp (prodid, vendorcode, drawing, drawingiss, " +
                        "partnumber, orderingcode, lsorsrs, alternateorderingcode, materialid, passed) " +
                        "values ('" + prodid + "','" + vendorcode + "','" + drawing + "','" + drawingiss +
                        "','" + partnumber + "','" + orderingcode + "','" + lsorsrs + "','" +
                        alternateorderingcode + "','" + materialid + "','" + passed + "')";
                            

            try
            {

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating InsertMatchTemp - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.InsertMatchTemp({0}, {1})", "", "");
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public void DoMaterialItemUpdates()
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            IAccessor dbManager1 = null;

            string sql = @"select b.material_item_id, a.alias_val 
                            from mtrl_alias_val a, material_id_mapping_vw b
                            where mtrl_alias_id = 1
                            and a.mtrl_id = b.mtrl_id";
            string sqlupdate = "";
            int count = 0;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                dbManager1 = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    string materialItemID = reader["material_item_id"].ToString();
                    string iesEqptProdID = reader["alias_val"].ToString();

                    sqlupdate = @"update material_item set ies_eqpt_prod_id = " + iesEqptProdID +
                                " where material_item_id = " + materialItemID;
                    dbManager1.ExecuteNonQuery(CommandType.Text, sqlupdate);
                    count++;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in DoMaterialItemUpdates - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.DoMaterialItemUpdates()", "");
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

                dbManager1.Dispose();
            }

            string mike = count.ToString();
        }
    }
}
