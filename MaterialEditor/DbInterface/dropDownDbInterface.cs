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
    public class dropDownsDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public dropDownsDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public dropDownsDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public async Task<List<Drop>> GettypeCD(string appname, string approvedstatus)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDtypeCD = null;
            DropdownType DDType = new DropdownType();
            Models.Attribute svalue = new Models.Attribute();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.MATERIAL_TYPE_CD_ATTRIBUTES", parameters);
                    while (reader.Read())
                    {
                        if (DDtypeCD == null)
                            DDtypeCD = new List<Drop>();
                        Drop droplist = new Drop();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.TypeCD)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCS_TABLE));
                        }
                        if (approvedstatus == "ALL")
                        { DDtypeCD.Add(droplist); }
                        else if (droplist.Attributes.TryGetValue("approvedstatus", out svalue))
                        {
                            if (svalue.Value == approvedstatus) { DDtypeCD.Add(droplist); }
                        }


                    }

                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get material_type_cd_attributes ({0}, )", appname);

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

            return DDtypeCD;
        }

        public async Task<List<Drop>> Getflowthru(string originatingfieldname, string flowthruind)

        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDflowthru = null;
            DropdownType DDType = new DropdownType();
            Models.Attribute svalue = new Models.Attribute();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.MATERIAL_FLOW_THRU_ATTRIBUTES", parameters);
                    while (reader.Read())
                    {
                        if (DDflowthru == null)
                            DDflowthru = new List<Drop>();
                        Drop droplist = new Drop();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.FlowThru)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCSH_TABLE));
                        }
                        if (flowthruind == "ALL") { DDflowthru.Add(droplist); }
                        else if (droplist.Attributes.TryGetValue("flowthruind", out svalue))
                        {
                            if (svalue.Value == flowthruind) { DDflowthru.Add(droplist); }
                        }

                    }


                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get update_mat_flow_thru({0} )", originatingfieldname);

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

            return DDflowthru;
        }

        public async Task<List<Drop>> Updatematerialflowthru(string flowthruid, string originatingfieldname, string fielddescription, string flowthruind, string originatingsystem, string user)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDflowthru = null;
            DropdownType DDType = new DropdownType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(7);

                //PROCEDURE UPDATE_MAT_FLOW_THRU(pflowthruid in MATERIAL_FLOW_THRU.FLOW_THRU_ID % TYPE,
                //poriginatingfieldname IN MATERIAL_FLOW_THRU.ORIGINATING_FIELD_NM % TYPE,
                //pfielddescription IN MATERIAL_FLOW_THRU.FIELD_DESCRIPTION % TYPE,
                //pUser in MATERIAL_FLOW_THRU.LAST_UPDTD_USERID % TYPE,
                //pflowthruind IN MATERIAL_FLOW_THRU.FLOW_THRU_IND % TYPE,
                //poriginatingsystem IN MATERIAL_FLOW_THRU.ORIGINATING_SYSTEM % TYPE, 
                //RETCSR OUT REF_CURSOR)

                    parameters[0] = dbManager.GetParameter("pflowthruid", DbType.String, flowthruid.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("poriginatingfieldname", DbType.String, originatingfieldname.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pfielddescription", DbType.String, fielddescription.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pUser", DbType.String, user.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pflowthruind", DbType.String, flowthruind.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("poriginatingsystem", DbType.String, originatingsystem.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.UPDATE_MAT_FLOW_THRU", parameters);

                    while (reader.Read())
                    {
                        if (DDflowthru == null)
                            DDflowthru = new List<Drop>();

                        Drop droplist = new Drop();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.FlowThru)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCSH_TABLE));
                        }

                        DDflowthru.Add(droplist);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get update_mat_flow_thru({0},{1},{2},{3},{4})", flowthruid, originatingfieldname, fielddescription, flowthruind, originatingsystem);

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

            return DDflowthru;
        }

        public async Task<List<Drop>> DeleteMaterialtypecd(string appname)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDtypecd = null;
            DropdownType DDType = new DropdownType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("PAPPNAME", DbType.String, appname.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.DELETE_TYPECD", parameters);

                    while (reader.Read())
                    {
                        if (DDtypecd == null)
                            DDtypecd = new List<Drop>();

                        Drop droplist = new Drop();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.TypeCD)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCS_TABLE));
                        }
                        DDtypecd.Add(droplist);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get delete_typecd ({0} )", appname);

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

            return DDtypecd;
        }

        public async Task<List<Drop>> InsertmaterialtypeCD(string appname, string effestartdate, string effenddate, string user)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDtypecd = null;
            DropdownType DDType = new DropdownType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(5);


                    parameters[0] = dbManager.GetParameter("aPpName", DbType.String, appname.ToUpper(), ParameterDirection.Input);
                    string effestartdate1 = Convert.ToDateTime(effestartdate).ToString("dd-MM-yyyy");
                    parameters[1] = dbManager.GetParameter("pEffstartdate", DbType.String, effestartdate1, ParameterDirection.Input);
                    string effenddate1 = Convert.ToDateTime(effenddate).ToString("dd-MM-yyyy");
                    parameters[2] = dbManager.GetParameter("pEffenddate", DbType.String, effenddate1, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pUser", DbType.String, user.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.INSERT_MATERIALTYPECD", parameters);

                    while (reader.Read())
                    {
                        if (DDtypecd == null)
                            DDtypecd = new List<Drop>();

                        Drop droplist = new Drop();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.TypeCD)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCS_TABLE));
                        }
                        DDtypecd.Add(droplist);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get insert_materialtypecd ({0},{1},{2} )", appname, effestartdate, effenddate);

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

            return DDtypecd;
        }

        public async Task<List<Drop>> UpdatematerialtypeCD(string materialid, string appname, string effestartdate, string effenddate, string lastupdated, string user)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Drop> DDtypecd = null;
            DropdownType DDType = new DropdownType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(5);
                    parameters[0] = dbManager.GetParameter("pappName", DbType.String, appname.ToUpper(), ParameterDirection.Input);
                    string effestartdate1 = Convert.ToDateTime(effestartdate).ToString("dd-MM-yyyy");
                    parameters[1] = dbManager.GetParameter("pEffstartdate", DbType.String, effestartdate1, ParameterDirection.Input);
                    string effenddate1 = Convert.ToDateTime(effenddate).ToString("dd-MM-yyyy");
                    parameters[2] = dbManager.GetParameter("pEffenddate", DbType.String, effenddate1, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pUser", DbType.String, user.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.UPDATE_MAT_TYPE", parameters);

                    while (reader.Read())
                    {
                        if (DDtypecd == null)
                            DDtypecd = new List<Drop>();

                        Drop droplist = new Drop();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in DDType.TypeCD)
                        {
                            droplist.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), DropdownType.DD_MCS_TABLE));
                        }
                        DDtypecd.Add(droplist);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get update_mat_type ({0},{1},{2},{3},{4} )", materialid, appname, effestartdate, effenddate, lastupdated);
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

            return DDtypecd;
        }



    }
}
