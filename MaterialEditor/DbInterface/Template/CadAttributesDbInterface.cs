using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
//using CenturyLink.Network.Engineering.Material.Editor.Models;
//using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
//using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
//using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public class CadAttributesDbInterface
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private IAccessor dbAccessor = null;
        private const string PKG = "cad_pkg";

        public CadAttributesDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }
        public CadAttributesDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public void StartTransaction()
        {
            if (dbAccessor == null)
                dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            dbAccessor.BeginTransaction();
        }
        public void CommitTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.RollbackTransaction();
        }
        public void Dispose()
        {
            if (dbAccessor != null)
                dbAccessor.Dispose();
        }

        protected IDbDataParameter[] CreateParamsWithRefCursor(IAccessor dbi, string parms, Hashtable fields)
        {
            var list = parms.Split(',');
            var len = list.Length;
            var p = dbi.GetParameterArray(len + 1);

            for (var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();

                if (parm.EndsWith("Id"))
                {
                    //if (val == null || val.Length == 0 || val == "%")
                    if (val == null || val.Length == 0)
                    {
                        val = null;
                        p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                    }
                    else if (val == "%")
                    {
                        p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                    }
                    else
                    {
                        p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                    }
                }
                else
                {
                    p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                }
            }

            p[len] = dbi.GetParameterCursorType("retCsr", ParameterDirection.Output);

            return p;
        }

        public async Task<String> Insert_cad_attributes(List<CadAttributes> lstAttr)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;
            string resp = string.Empty;
            var res = new List<Hashtable>();

            var proc = PKG + ".insert_drawing_objects";
           
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    for (int i = 0; i < lstAttr.Count; i++)
                    {                       

                        parameters = dbManager.GetParameterArray(6);
                        parameters[0] = dbManager.GetParameter("pcad_attr_typ", DbType.String, lstAttr[i].CAD_ATTR_TYP, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pcad_attr_id", DbType.Int64, lstAttr[i].CAD_ATTR_ID, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("ptmplt_def_id", DbType.Int64, lstAttr[i].TMPLT_DEF_ID, ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameterClobType("pJosn_data", lstAttr[i].JSON_DATA, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pupdated_by", DbType.String, "developement", ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                        reader = dbManager.ExecuteDataReaderSP(proc, parameters);                        
                        while (reader.Read())
                        {
                            resp = DataReaderHelper.GetNonNullValue(reader, "resposne");
                        }

                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                    logger.Error(ex, proc + ": general error");
                    resp = ex.ToString();
                }

            });

            return resp;
        }

        public async Task<List<Hashtable>> LIST_ATTRS(long tmpltId, long tmpltDefId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;

            var rv = new List<Hashtable>(); 

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                }
                catch (Exception ex)
                {
                    error = ex;
                    logger.Error(ex, "CadAttributesDbInterface.LIST_ATTRS");
                    return;
                }

                var proc = PKG + ".LIST_ATTRS";

                try
                {
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pTMPLT_ID", DbType.Int64, tmpltId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pTMPLT_DEF_ID", DbType.Int64, tmpltDefId, ParameterDirection.Input);

                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                    while (reader.Read())
                    {
                        var rec = new GenericRec(reader);
                        rv.Add(rec.Data);
                    }
                }
                catch (Exception ex)
                {
                    error = ex;

                    var msg = String.Format("{0}({1},{2}): ", proc, tmpltId, tmpltDefId);
                    logger.Error(ex, msg);
                    rv.Add((new GenericResponse(-1, ex.Message, msg)).Data);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbManager != null)
                    {
                        dbManager.Dispose();
                    }
                }
            });

            return rv;
        }

        public async Task<List<GenericResponse>> UPDATE_ATTRS(string cuid, string action, List<CadAttributes> attrs)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;

            var rv = new List<GenericResponse>(); // list of { EC: #, MSG: "", OV: "" } per item in attrs list

            if (attrs.Count == 0)
            {
                return rv;
            }


            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                } 
                catch( Exception ex )
                {
                    error = ex;
                    logger.Error(ex, "CadAttributesDbInterface.UPDATE_ATTRS");
                    return;
                }

                for (var i = 0; i < attrs.Count; i++)
                {
                    var proc = PKG + ".UPDATE_ATTRS";

                    try
                    {

                        parameters = dbManager.GetParameterArray(8);

                        parameters[0] = dbManager.GetParameter("pCUID", DbType.String, cuid, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pACTION", DbType.String, action, ParameterDirection.Input);

                        parameters[2] = dbManager.GetParameter("pCAD_ATTR_ID", DbType.Int64, attrs[i].CAD_ATTR_ID, ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pCAD_ATTR_TYP", DbType.String, attrs[i].CAD_ATTR_TYP, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pTMPLT_ID", DbType.Int64, attrs[i].TMPLT_ID, ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pTMPLT_DEF_ID", DbType.Int64, attrs[i].TMPLT_DEF_ID, ParameterDirection.Input);

                        parameters[6] = dbManager.GetParameterClobType("pJSON_DATA", attrs[i].JSON_DATA, ParameterDirection.Input);

                        parameters[7] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                        reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                        while (reader.Read())
                        {
                            var resp = new GenericResponse(reader);
                            rv.Add(resp);
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;

                        var item = Newtonsoft.Json.JsonConvert.SerializeObject(attrs[i]);
                        logger.Error(ex, String.Format("{0}: item #{1}; {2}", proc, (i + 1).ToString(), item));

                        rv.Add(new GenericResponse(-1, ex.Message, item));
                    }
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                {
                    dbManager.Dispose();
                }
            });

            return rv;
        }
    }
}