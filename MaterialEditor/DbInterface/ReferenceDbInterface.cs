using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface
{
    public class ReferenceDbInterface
    {
        private static Logger logger;// = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        public static readonly string CURSOR = "CURSOR";

        public ReferenceDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            logger = LogManager.GetCurrentClassLogger();
        }

        public ReferenceDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public async Task<List<Option>> GetListOptions(string sql)
        {
            List<Option> options = await GetListOptions(sql, null);

            return options;
        }

        public async Task<List<Option>> GetListOptions(string sql, IDbDataParameter[] parameters)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    if (parameters == null)
                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql);
                    else
                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql, parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionText = reader["option_text"].ToString();
                        string optionValue = reader["option_value"].ToString();
                        string optionDefaultIndicator = DataReaderHelper.GetNonNullValue(reader, "dflt_ind");

                        if (string.IsNullOrEmpty(optionDefaultIndicator))
                            options.Add(new Option(optionValue, optionText));
                        else
                            options.Add(new Option(optionValue, optionText, optionDefaultIndicator));
                    }
                }
                catch (Exception ex)
                {
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

            return options;
        }

        public async Task<List<Option>> GetListOptionsSP(string sql, IDbDataParameter[] parameters)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    if (parameters != null)
                    {
                        reader = dbManager.ExecuteDataReaderSP(sql, parameters);

                        while (reader.Read())
                        {
                            if (options == null)
                            {
                                options = new List<Option>();

                                options.Add(new Option("", ""));
                            }

                            string optionText = reader["option_text"].ToString();
                            string optionValue = reader["option_value"].ToString();
                            string optionDefaultIndicator = DataReaderHelper.GetNonNullValue(reader, "dflt_ind");

                            if (string.IsNullOrEmpty(optionDefaultIndicator))
                                options.Add(new Option(optionValue, optionText));
                            else
                                options.Add(new Option(optionValue, optionText, optionDefaultIndicator));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get labor id list for material item id {0}", parameters[0].Value.ToString());

                    if (options == null)
                    {
                        options = new List<Option>();

                        options.Add(new Option("", ""));
                    }


                    //options.Add(new Option("999", "999 - NO LABOR CODE REQUIRED"));
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

            return options;
        }

        public async Task<List<Option>> GetListOptionsForAttribute(string attributeName, NameValueCollection parameters = null)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] thisParameters = null;
            IDbDataParameter[] nextParameters = null;
            List<Option> options = null;
            string sql = "";
            bool isStoredProcedure = false;

            await Task.Run(async () =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    thisParameters = dbManager.GetParameterArray(2);

                    thisParameters[0] = dbManager.GetParameter("pName", DbType.String, attributeName, ParameterDirection.Input);
                    thisParameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_ATTRIBUTE_SQL", thisParameters);

                    if (reader.Read())
                    {
                        sql = reader["sql"].ToString();

                        if (parameters != null && parameters.Count > 0)
                        {
                            nextParameters = dbManager.GetParameterArray(parameters.Count);

                            for (int i = 0; i < parameters.Count; i++)
                            {
                                if (CURSOR.Equals(parameters[i]))
                                {
                                    nextParameters[i] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                                    isStoredProcedure = true;
                                }
                                else
                                    nextParameters[i] = dbManager.GetParameter("p" + i, DbType.String, parameters[i], ParameterDirection.Input);
                            }                            
                        }

                        if (isStoredProcedure)
                            options = await GetListOptionsSP(sql, nextParameters);
                        else
                            options = await GetListOptions(sql, nextParameters);
                    }
                }
                catch (Exception ex)
                {
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

            return options;
        }

        public async Task<List<Option>> GetOptions(string jsonName, NameValueCollection parameters = null)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] thisParameters = null;
            IDbDataParameter[] nextParameters = null;
            List<Option> options = null;
            string sql = "";

            await Task.Run(async () =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    thisParameters = dbManager.GetParameterArray(2);

                    thisParameters[0] = dbManager.GetParameter("pName", DbType.String, jsonName, ParameterDirection.Input);
                    thisParameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("REFERENCE_PKG.GET_OPTIONS_REF_SQL", thisParameters);

                    if (reader.Read())
                    {
                        sql = reader["sql"].ToString();

                        if (parameters != null && parameters.Count > 0)
                        {
                            nextParameters = dbManager.GetParameterArray(parameters.Count);

                            for (int i = 0; i < parameters.Count; i++)
                            {
                                nextParameters[i] = dbManager.GetParameter("p" + i, DbType.String, parameters[i], ParameterDirection.Input);
                            }
                        }

                        options = await GetListOptions(sql, nextParameters);
                    }
                }
                catch (Exception ex)
                {
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

            return options;
        }

        public async Task<string> ValidateSessionId(string sessionId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string result = "Invalid";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pSessionId", DbType.String, sessionId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("status", DbType.String, result, ParameterDirection.Output, 50);

                    dbManager.ExecuteScalarSP("MATERIAL_PKG.VALIDATE_USER", parameters);

                    if (parameters[1].Value != null && !"null".Equals(parameters[1].Value.ToString()))
                        result = parameters[1].Value.ToString();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to validate session id: {0}";

                    logger.Error(oe, message, sessionId);
                    EventLogger.LogAlarm(oe, string.Format(message, sessionId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to validate session id: {0}", sessionId);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return result;
        }

        public async Task<List<AutocompleteOption>> GetListOptionsForAutoComplete(string name, string value, NameValueCollection parameters = null)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] thisParameters = null;
            List<AutocompleteOption> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    thisParameters = dbManager.GetParameterArray(3);

                    thisParameters[0] = dbManager.GetParameter("pName", DbType.String, name, ParameterDirection.Input);
                    thisParameters[1] = dbManager.GetParameter("pValue", DbType.String, value.ToUpper(), ParameterDirection.Input);
                    thisParameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_AUTO_COMPLETE_DATA", thisParameters);

                    while (reader.Read())
                    {
                        if (options == null)
                            options = new List<AutocompleteOption>();

                        string optionText = reader["option_label"].ToString();
                        string optionValue = reader["option_value"].ToString();

                        options.Add(new AutocompleteOption(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "GetListOptionsForAutoComplete({0}, {1})", name, value);
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

            return options;
        }

        public async Task<string> CheckLocatableFeatureType(int stored_value)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string locType = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pFeatTypId", DbType.String, stored_value, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("REFERENCE_PKG.GET_LOCATION_TYPE", parameters);

                    if(reader.Read())
                        locType = DataReaderHelper.GetNonNullValue(reader, "loc_type");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get locatable type for feature type ({0})", stored_value);

                    throw ex;
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

            return locType;
        }

        public Dictionary<string, MaterialCategory.Category> GetMaterialCategories()
        {
            Dictionary<string, MaterialCategory.Category> categories = new Dictionary<string, MaterialCategory.Category>();
            IAccessor dbManager = null;
            IDataReader reader = null;
            string sql = @"SELECT mtrl_cat_id, mtrl_cat_typ, mtrl_cat_dsc, cdmms_rt_tbl_nm, cdmms_revsn_tbl_nm
                            FROM mtrl_cat";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    MaterialCategory.Category category = new MaterialCategory.Category();

                    category.Id = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_cat_id", true));
                    category.Type = DataReaderHelper.GetNonNullValue(reader, "mtrl_cat_typ");
                    category.RevisionTable = DataReaderHelper.GetNonNullValue(reader, "cdmms_revsn_tbl_nm");
                    category.RootTable = DataReaderHelper.GetNonNullValue(reader, "cdmms_rt_tbl_nm");

                    categories.Add(category.Type, category);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetMaterialCategories({0}, {1})");
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

            return categories;
        }

        public Dictionary<string, FeatureType.Feature> GetFeatureTypes()
        {
            Dictionary<string, FeatureType.Feature> features = new Dictionary<string, FeatureType.Feature>();
            IAccessor dbManager = null;
            IDataReader reader = null;
            string sql = @"SELECT a.feat_typ_id, a.feat_typ, c.alias_val, d.value, a.rme_ind, a.cabl_ind, a.rcrds_only_allow_ind, a.ordbl_allow_ind,
                            a.symblgy_ind, a.dim_hgt_ind, a.dim_wdth_ind, a.dim_dpth_ind, a.dim_lgth_ind, a.cntnd_in_allow_ind, a.cdmms_rt_tbl_nm, a.cdmms_revsn_tbl_nm
                            FROM feat_typ a, feat_typ_alias b, feat_typ_alias_val c, enum_cl_feature_type d
                            WHERE a.feat_typ_id = c.feat_typ_id
                            AND b.feat_typ_alias_id = c.feat_typ_alias_id
                            AND b.alias_nm = 'Feature Type'
                            AND c.alias_val = to_char(d.stored_value)
                            UNION ALL
                            SELECT a.feat_typ_id, a.feat_typ, NULL AS alias_val, NULL AS value, a.rme_ind, a.cabl_ind, a.rcrds_only_allow_ind, a.ordbl_allow_ind,
                            a.symblgy_ind, a.dim_hgt_ind, a.dim_wdth_ind, a.dim_dpth_ind, a.dim_lgth_ind, a.cntnd_in_allow_ind, a.cdmms_rt_tbl_nm, a.cdmms_revsn_tbl_nm
                            FROM feat_typ a
                            WHERE a.feat_typ_id NOT IN (SELECT c.feat_typ_id
                            FROM feat_typ_alias b, feat_typ_alias_val c, enum_cl_feature_type d
                            WHERE b.feat_typ_alias_id = c.feat_typ_alias_id
                            AND b.alias_nm = 'Feature Type'
                            AND c.alias_val = to_char(d.stored_value))";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    FeatureType.Feature feature = new FeatureType.Feature();

                    feature.Id = int.Parse(DataReaderHelper.GetNonNullValue(reader, "feat_typ_id", true));
                    feature.Type = DataReaderHelper.GetNonNullValue(reader, "feat_typ");
                    feature.DepthIndicator = DataReaderHelper.GetNonNullValue(reader, "dim_dpth_ind");
                    feature.CableIndicator = DataReaderHelper.GetNonNullValue(reader, "cabl_ind");
                    feature.ContainedInAllowIndicator = DataReaderHelper.GetNonNullValue(reader, "cntnd_in_allow_ind");
                    feature.HeightIndicator = DataReaderHelper.GetNonNullValue(reader, "dim_hgt_ind");
                    feature.LengthIndicator = DataReaderHelper.GetNonNullValue(reader, "dim_lgth_ind");
                    feature.NdsId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "alias_val", true));
                    feature.NdsType = DataReaderHelper.GetNonNullValue(reader, "value");
                    feature.OrderableAllowIndicator = DataReaderHelper.GetNonNullValue(reader, "ordbl_allow_ind");
                    feature.RecordsOnlyAllowIndicator = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_allow_ind");
                    feature.RMEIndicator = DataReaderHelper.GetNonNullValue(reader, "rme_ind");
                    feature.SymbologyIndicator = DataReaderHelper.GetNonNullValue(reader, "symblgy_ind");
                    feature.WidthIndicator = DataReaderHelper.GetNonNullValue(reader, "dim_wdth_ind");
                    feature.RevisionTableName = DataReaderHelper.GetNonNullValue(reader, "cdmms_revsn_tbl_nm");
                    feature.RootTableName = DataReaderHelper.GetNonNullValue(reader, "cdmms_rt_tbl_nm");

                    features.Add(feature.Type, feature);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetMaterialCategories({0}, {1})");
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

            return features;
        }

        public static int GetDimensionsUnitOfMeasureId(string uom)
        {
            string dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            int id = 0;

            try
            {
                //PROCEDURE GET_DIM_UOM(pUom IN VARCHAR2, pUseDefault IN VARCHAR2, oUomId OUT NUMBER)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pUom", DbType.String, uom, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("pUseDefault", DbType.String, "Y", ParameterDirection.Input, 50);
                parameters[2] = dbManager.GetParameter("oUomId", DbType.Int32, id, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("REFERENCE_PKG.GET_DIM_UOM", parameters);

                id = int.Parse(parameters[2].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetDimensionsUnitOfMeasureId: {0}; ", uom);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return id;
        }

        public static int GetManufacturerId(string manufacturer)
        {
            string dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            int id = 0;

            try
            {
                //PROCEDURE GET_MFR(pMfrCd IN VARCHAR2, oMfrId OUT NUMBER)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMfrCd", DbType.String, manufacturer, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("oMfrId", DbType.Int32, id, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("REFERENCE_PKG.GET_MFR", parameters);

                id = int.Parse(parameters[1].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetManufacturerId: {0}; ", manufacturer);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return id;
        }

        public static string GetWallMountAllowedDefaultIndicator(string value, string tableName)
        {
            string dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string indicator = "N";

            try
            {
                //PROCEDURE GET_WLL_MNT_ALLOW_DFLT_IND(pValue IN VARCHAR2, pTblNm IN VARCHAR2, oDfltInd OUT VARCHAR2)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pValue", DbType.String, value, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("pTblNm", DbType.String, tableName, ParameterDirection.Input, 50);
                parameters[2] = dbManager.GetParameter("oDfltInd", DbType.String, indicator, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("REFERENCE_PKG.GET_WLL_MNT_ALLOW_DFLT_IND", parameters);

                indicator = parameters[2].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetWallMountAllowedDefaultIndicator: {0}, {1} ", value, tableName);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return indicator;
        }

        public static string GetStraightThruDefaultIndicator(string value, string tableName)
        {
            string dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string indicator = "N";

            try
            {
                //PROCEDURE GET_STRGHT_THRU_DFLT_IND(pValue IN VARCHAR2, pTblNm IN VARCHAR2, oDfltInd OUT VARCHAR2)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pValue", DbType.String, value, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("pTblNm", DbType.String, tableName, ParameterDirection.Input, 50);
                parameters[2] = dbManager.GetParameter("oDfltInd", DbType.String, indicator, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("REFERENCE_PKG.GET_STRGHT_THRU_DFLT_IND", parameters);

                indicator = parameters[2].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetStraightThruDefaultIndicator: {0}, {1} ", value, tableName);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return indicator;
        }

        public static string GetMidPlaneDefaultIndicator(string value, string tableName)
        {
            string dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string indicator = "N";

            try
            {
                //PROCEDURE GET_MID_PLN_DFLT_IND(pValue IN VARCHAR2, pTblNm IN VARCHAR2, oDfltInd OUT VARCHAR2)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pValue", DbType.String, value, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("pTblNm", DbType.String, tableName, ParameterDirection.Input, 50);
                parameters[2] = dbManager.GetParameter("oDfltInd", DbType.String, indicator, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("REFERENCE_PKG.GET_MID_PLN_DFLT_IND", parameters);

                indicator = parameters[2].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetMidPlaneDefaultIndicator: {0}, {1} ", value, tableName);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return indicator;
        }
    }
}