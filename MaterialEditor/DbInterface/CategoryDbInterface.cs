using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using NLog;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Utility;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface
{
    public class CategoryDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public CategoryDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public CategoryDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public async Task<List<CategoryItem>> SearchCategoryItemAsync(string materialgroupid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CategoryItem> categoryItem = null;
            CategoryType catType = new CategoryType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlGrpId", DbType.String, materialgroupid.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_CATEGORY", parameters);

                    while (reader.Read())
                    {
                        if (categoryItem == null)
                            categoryItem = new List<CategoryItem>();

                        CategoryItem category = new CategoryItem();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in catType.SAPCategory)
                        {
                            category.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                        categoryItem.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform Search Operation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return categoryItem;
        }

        public async Task<List<CategoryItem>> DeleteCategoryItemAsync(string materialgroupid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CategoryItem> categoryItem = null;
            CategoryType catType = new CategoryType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlGrpId", DbType.String, materialgroupid.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.DELETE_CATEGORY", parameters);

                    while (reader.Read())
                    {
                        if (categoryItem == null)
                            categoryItem = new List<CategoryItem>();

                        CategoryItem category = new CategoryItem();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in catType.SAPCategory)
                        {
                            category.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                        categoryItem.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform Delete Operation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return categoryItem;
        }

        public async Task<List<CategoryItem>> InsertCategoryItemAsync(string materialgroupid, string appname, string effestartdate, string effenddate,string user)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CategoryItem> categoryItem = null;
            CategoryType catType = new CategoryType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(6);

                    parameters[0] = dbManager.GetParameter("pMtlGrpId", DbType.String, materialgroupid.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("aPpName", DbType.String, appname.ToUpper(), ParameterDirection.Input);
                    string effestartdate1 = Convert.ToDateTime(effestartdate).ToString("dd-MM-yyyy");
                    parameters[2] = dbManager.GetParameter("pEffstartdate", DbType.String, effestartdate1, ParameterDirection.Input);
                    string effenddate1 = Convert.ToDateTime(effenddate).ToString("dd-MM-yyyy");
                    parameters[3] = dbManager.GetParameter("pEffenddate", DbType.String, effenddate1, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pUser", DbType.String, user.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.INSERT_CATEGORY", parameters);

                    while (reader.Read())
                    {
                        if (categoryItem == null)
                            categoryItem = new List<CategoryItem>();

                        CategoryItem category = new CategoryItem();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in catType.SAPCategory)
                        {
                            category.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                        categoryItem.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform Insert Operation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return categoryItem;
        }
        public async Task<List<CategoryItem>> UpdateCategoryItemAsync(string materialgroupid, string appname, string appnameold, string effestartdate, string effenddate,string user)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CategoryItem> categoryItem = null;
            CategoryType catType = new CategoryType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(7);

                    parameters[0] = dbManager.GetParameter("pMtlGrpId", DbType.String, materialgroupid.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("APPNAME", DbType.String, appname.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pAppnameold", DbType.String, appnameold.ToUpper(), ParameterDirection.Input);
                    string effestartdate1 = Convert.ToDateTime(effestartdate).ToString("dd-MM-yyyy");
                    parameters[3] = dbManager.GetParameter("pEffstartdate", DbType.String, effestartdate1, ParameterDirection.Input);
                    string effenddate1 = Convert.ToDateTime(effenddate).ToString("dd-MM-yyyy");
                    parameters[4] = dbManager.GetParameter("pEffenddate", DbType.String, effenddate1, ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pUser", DbType.String, user.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.UPDATE_CATEGORY", parameters);

                    while (reader.Read())
                    {
                        if (categoryItem == null)
                            categoryItem = new List<CategoryItem>();

                        CategoryItem category = new CategoryItem();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in catType.SAPCategory)
                        {
                            category.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                        categoryItem.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform Update Operation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return categoryItem;
        }



    }
}