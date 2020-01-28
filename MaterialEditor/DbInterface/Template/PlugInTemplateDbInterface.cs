using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public class PlugInTemplateDbInterface : TemplateDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PlugInTemplateDbInterface() : base()
        {
        }

        public PlugInTemplateDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ITemplate GetTemplate(long templateId, bool isBaseTemplate)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            ITemplate template = null;

            try
            {
                if (isBaseTemplate)
                    template = GetBaseTemplate(templateId);                
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Unable to retrieve bay template id: {0}; isBase: {1}", templateId, isBaseTemplate);
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

            return template;
        }

        private ITemplate GetBaseTemplate(long templateId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ITemplate template = null;
            PlugInSpecificationDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_plg_in_tmplt", parameters);
              
                if (reader.Read())
                {
                    template = new BasePlugInTemplate(templateId);
                    dbInterface = new PlugInSpecificationDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "plgin_specn_revsn_alt_id", true));
                    template.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_id", true));
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;                    

                    template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationRevisionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve base bay template id: {0}", templateId);
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

            return template;
        }       

        public long CreateBaseTemplate(long specId, string name, string description)
        {
            IDbDataParameter[] bayParameters = null;
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "Plug-In", true);

                bayParameters = dbAccessor.GetParameterArray(4);

                bayParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                bayParameters[1] = dbAccessor.GetParameter("pPlgInSpecId", DbType.Int64, specId, ParameterDirection.Input);
                bayParameters[2] = dbAccessor.GetParameter("pCnctrTypId", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[3] = dbAccessor.GetParameter("pBiDrctnlInd", DbType.String, DBNull.Value, ParameterDirection.Input);
               
                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.insert_base_plg_in_tmplt", bayParameters);

                CommitTransaction();
            }
            catch(Exception ex)
            {
                RollbackTransaction();

                logger.Error("Unable to create base Bay Template ({0}, {1}, {2}", specId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        public void UpdateBaseTemplate(long id, long specnRvsnAltId, string frontRear, int rotationAngleId)
        {
            IDbDataParameter[] bayParameters = null;

            try
            {
                bayParameters = dbAccessor.GetParameterArray(4);              

                bayParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                bayParameters[1] = dbAccessor.GetParameter("pPlgInRoleTypId", DbType.Int64, specnRvsnAltId, ParameterDirection.Input);
                bayParameters[2] = dbAccessor.GetParameter("pCnctrTypId", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[3] = dbAccessor.GetParameter("pBiDrctnlInd", DbType.String, DBNull.Value, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.update_base_plg_in_tmplt", bayParameters);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Unable to update base BAY template {0}, {1}, {2}, {3}", id, specnRvsnAltId, frontRear, rotationAngleId);

                throw ex;
            }
        }       
    }
}