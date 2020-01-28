using System;
using System.Collections;
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
    public class BayTemplateDbInterface : TemplateDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BayTemplateDbInterface() : base()
        {
        }

        public BayTemplateDbInterface(string dbConnectionString) : base(dbConnectionString)
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
                else
                {
                    template = GetComplexTemplate(templateId);

                    if (template != null)
                        ((OverAllBayTemplate)template).TemplateBase = (BaseTemplate)GetBaseTemplate(((OverAllBayTemplate)template).BaseTemplateId);
                }
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
            BaySpecificationDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_bay_tmplt", parameters);

                //SELECT bbt.tmplt_id, bbt.bay_specn_revsn_alt_id, bbt.frnt_rer_ind, bbt.rottn_angl_id, t.tmplt_nm, t.tmplt_dsc,
                //t.cmplt_ind, t.prpgt_ind, t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind, bsra.bay_specn_id
                if (reader.Read())
                {
                    template = new BaseBayTemplate(templateId);
                    dbInterface = new BaySpecificationDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_specn_revsn_alt_id", true));
                    template.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_specn_id", true));
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                    ((BaseBayTemplate)template).SetFrontRearIndicator(DataReaderHelper.GetNonNullValue(reader, "frnt_rer_ind"));
                    ((BaseBayTemplate)template).SetRotationAngleId(int.Parse(DataReaderHelper.GetNonNullValue(reader, "rottn_angl_id", true)));

                    // call to GetSpecification takes way too long to run...
                    //template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationRevisionId);
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

        private ITemplate GetComplexTemplate(long templateId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ITemplate template = null;
            BaySpecificationDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("overl_bay_pkg.get_overl_bay_tmplt", parameters);

                //SELECT t.tmplt_id, t.tmplt_nm, t.tmplt_dsc, t.cmplt_ind, t.prpgt_ind, t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind,
                //o.base_bay_tmplt_id, o.bay_extndr_specn_revsn_alt_id
                if (reader.Read())
                {
                    template = new OverAllBayTemplate(templateId);
                    dbInterface = new BaySpecificationDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                    ((OverAllBayTemplate)template).BaseTemplateId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "base_tmplt_id", true));
                    ((OverAllBayTemplate)template).BayExtndrSpecnRevsnAltId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_alt_id", true));
                    ((OverAllBayTemplate)template).rtnAnglDgrNo = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rottn_angl_dgr_no", true));

                    template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationId);
                    template.BayChoices = GetContainmentChoices(2);
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

        public long CreateBaseTemplate(long specRvsnId, string name, string description)
        {
            IDbDataParameter[] bayParameters = null;
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "Bay", true);

                bayParameters = dbAccessor.GetParameterArray(4);

                bayParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                bayParameters[1] = dbAccessor.GetParameter("pBaySpecnRevsnAltId", DbType.Int64, specRvsnId, ParameterDirection.Input);
                bayParameters[2] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[3] = dbAccessor.GetParameter("pRottnAnglId", DbType.String, DBNull.Value, ParameterDirection.Input);
                //PROCEDURE insert_base_bay_tmplt(pTmpltId IN base_bay_tmplt.tmplt_id% TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id % TYPE,
                //pFrntRerInd IN base_bay_tmplt.frnt_rer_ind % TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id % TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.insert_base_bay_tmplt", bayParameters);

                CommitTransaction();
            }
            catch(Exception ex)
            {
                RollbackTransaction();

                logger.Error("Unable to create base Bay Template ({0}, {1}, {2}", specRvsnId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        public long CreateOverAllTemplate(long baseTmpltId, string name, string description,string cuid)
        {           
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "Bay", false);

                if (id > 0)
                {
                    CreateOverAllTemplate(id, baseTmpltId, cuid,0);
                    CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction();

                logger.Error(ex, "Unable to create overall Bay Template ({0}, {1}, {2}", baseTmpltId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        public long UpdateOverAllTemplate(long id,long baseTmpltId, long MtrlCatId, long featTypId,string cuid,long rtnAngleId,long BayExtSpRevAltId)
        {
            IDbDataParameter[] bayParameters = null;           

            try
            {
                bayParameters = dbAccessor.GetParameterArray(18);

                bayParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                bayParameters[1] = dbAccessor.GetParameter("pBaseTmpltId", DbType.Int64, baseTmpltId, ParameterDirection.Input);
                bayParameters[2] = dbAccessor.GetParameter("pMtrlCatId", DbType.Int64, MtrlCatId, ParameterDirection.Input);
                bayParameters[3] = dbAccessor.GetParameter("pFeatTypId", DbType.Int64, featTypId, ParameterDirection.Input);
                bayParameters[4] = dbAccessor.GetParameter("pBayExtndrSpecnRevsnAltId", DbType.Int64, BayExtSpRevAltId, ParameterDirection.Input);
                bayParameters[5] = dbAccessor.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[6] = dbAccessor.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[7] = dbAccessor.GetParameter("pShelfSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[8] = dbAccessor.GetParameter("pAsnblNdSpcnWthPrtsAsmtId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[9] = dbAccessor.GetParameter("pHlpMtrlRevsnId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[10] = dbAccessor.GetParameter("pComnCnfgId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[11] = dbAccessor.GetParameter("pLabelNm", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[12] = dbAccessor.GetParameter("pRottnAnglId", DbType.Int64, rtnAngleId, ParameterDirection.Input);
                bayParameters[13] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[14] = dbAccessor.GetParameter("pPortTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[15] = dbAccessor.GetParameter("pCnctrTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                bayParameters[16] = dbAccessor.GetParameter("pUserComment", DbType.String, DBNull.Value, ParameterDirection.Input);
                bayParameters[17] = dbAccessor.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "COMPLEX_TMPLT_PKG.update_complex_tmplt", bayParameters);
              
            }
            catch (Exception ex)
            {             
                logger.Error(ex, "Unable to create overall Bay Template : {0} ", baseTmpltId);
                return 1;
            }          

            return 0;
        }
        public void UpdateBaseTemplate(long id, long specnRvsnAltId, string frontRear, int rotationAngleId)
        {
            IDbDataParameter[] bayParameters = null;

            try
            {
                bayParameters = dbAccessor.GetParameterArray(4);

                bayParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                bayParameters[1] = dbAccessor.GetParameter("pBaySpecnRevsnAltId", DbType.Int64, specnRvsnAltId, ParameterDirection.Input);
                bayParameters[2] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, CheckNullValue(frontRear), ParameterDirection.Input);
                bayParameters[3] = dbAccessor.GetParameter("pRottnAnglId", DbType.Int64, rotationAngleId, ParameterDirection.Input);
                //PROCEDURE update_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id%TYPE,
                //pFrntRerInd IN base_bay_tmplt.frnt_rer_ind % TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id % TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.update_base_bay_tmplt", bayParameters);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Unable to update base BAY template {0}, {1}, {2}, {3}", id, specnRvsnAltId, frontRear, rotationAngleId);

                throw ex;
            }
        }

        public async Task<List<string>> GetBayChoices(string featureTypeID, string templateID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            //List<CenturyLink.Network.Engineering.Material.Editor.Models.Option> optionsList = new List<CenturyLink.Network.Engineering.Material.Editor.Models.Option>();
            string packageAndProc = String.Empty;
            List<string> list = new List<string>();
            string optionText = String.Empty;
            string optionValue = String.Empty;

            await Task.Run(() =>
            {
                try
                {
                    if (featureTypeID == "1")
                    {
                        packageAndProc = "overl_bay_pkg.get_bay_extndr_for_bay";
                    }
                    else if (featureTypeID == "6")
                    {
                        packageAndProc = "overl_shelf_pkg.get_shelf_for_bay";
                    }
                    else if (featureTypeID == "5")
                    {
                        packageAndProc = "overl_node_pkg.get_node_for_bay";
                    }
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pId", DbType.Int64, int.Parse(templateID), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP(packageAndProc, parameters);
                    if (featureTypeID == "1")
                    {
                        list.Add("0~" + "<No Bay Extender>");
                    }
                    else if (featureTypeID == "6")
                    {
                        list.Add("0~" + "<No Shelf>");
                    }
                    else if (featureTypeID == "5")
                    {
                        list.Add("0~" + "<No Node>");
                    }

                    while (reader.Read())
                    {
                        if (featureTypeID == "1")
                        {
                            optionText = reader["specn_revsn_nm"].ToString();
                            optionValue = reader["specn_revsn_alt_id"].ToString();
                        }
                        else if (featureTypeID == "6" || featureTypeID == "5")
                        {
                            optionText = reader["tmplt_nm"].ToString();
                            optionValue = reader["tmplt_id"].ToString();
                        }
                        string width = reader["width"].ToString();
                        string depth = reader["depth"].ToString();
                        string height = reader["height"].ToString();
                        list.Add(optionValue + "~" + optionText + "~width:" + width + "~depth:" + depth + "~height:" + height);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve bay choices for template ID and feature type: {0}, {1}", templateID, featureTypeID);
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
            return list;
        }

        public List<CenturyLink.Network.Engineering.Material.Editor.Models.Option> GetContainmentChoices(long featTypId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CenturyLink.Network.Engineering.Material.Editor.Models.Option> featureTypes = new List<CenturyLink.Network.Engineering.Material.Editor.Models.Option>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, featTypId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("tmplt_pkg.get_feat_typ_containment_rules", parameters);

                featureTypes.Add(new CenturyLink.Network.Engineering.Material.Editor.Models.Option("",""));
                while (reader.Read())
                {
                    string optionText = reader["feat_typ"].ToString();
                    string optionValue = reader["feat_typ_id"].ToString();
                    featureTypes.Add(new CenturyLink.Network.Engineering.Material.Editor.Models.Option(optionValue, optionText));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve feature types from containment rules: {0}", featTypId);
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

        public List<BayExtenderClass> GetBayExtenderForBay(long templateId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<BayExtenderClass> bayExtenders = new List<BayExtenderClass>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("overl_bay_pkg.get_bay_extndr_for_bay", parameters);

                if (reader.Read())
                {
                    BayExtenderClass bayExtender = new BayExtenderClass();
                    bayExtender.ID = long.Parse(reader["bay_extndr_specn_id"].ToString());
                    bayExtender.Name = reader["bay_extndr_specn_revsn_nm"].ToString();
                    bayExtender.Width = long.Parse(reader["intl_wdth_no"].ToString());
                    bayExtender.Height = long.Parse(reader["intl_hgt_no"].ToString());
                    bayExtender.Depth = long.Parse(reader["intl_dpth_no"].ToString());
                    bayExtenders.Add(bayExtender);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve base extenders for bay template id: {0}", templateId);
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

            return bayExtenders;
        }
        public class BayExtenderClass
        {
            public long ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public long Height
            {
                get;
                set;
            }

            public long Width
            {
                get;
                set;
            }

            public long Depth
            {
                get;
                set;
            }
        }
    }
    
}