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
    public class CardTemplateDbInterface : TemplateDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CardTemplateDbInterface() : base()
        {

        }

        public CardTemplateDbInterface(string dbConnectionString) : base(dbConnectionString)
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
                    template = GetOverAllTemplate(templateId);

                    if (template != null)
                        ((OverAllCardTemplate)template).TemplateBase = (BaseTemplate)GetBaseTemplate(((OverAllCardTemplate)template).BaseTemplateId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve card template id: {0}; isBase: {1}", templateId, isBaseTemplate);
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
            CardSpecificationDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_card_tmplt", parameters);

                //SELECT bst.tmplt_id, bst.shelf_specn_revsn_alt_id, t.tmplt_nm, t.tmplt_dsc, t.cmplt_ind, t.prpgt_ind,
                //t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind, ssra.shelf_specn_id
                if (reader.Read())
                {
                    template = new BaseCardTemplate(templateId);
                    dbInterface = new CardSpecificationDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "card_specn_revsn_alt_id", true));
                    // template.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_specn_id", true));
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                    template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve base Card template id: {0}", templateId);
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
            IDbDataParameter[] cardParameters = null;
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "Card", true);

                cardParameters = dbAccessor.GetParameterArray(2);

                cardParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                cardParameters[1] = dbAccessor.GetParameter("pBaySpecnRevsnAltId", DbType.Int64, specRvsnId, ParameterDirection.Input);
                //PROCEDURE insert_base_shelf_tmplt (pTmpltId IN base_shelf_tmplt.tmplt_id%TYPE, pShelfSpecnRevsnAltId IN base_shelf_tmplt.shelf_specn_revsn_alt_id%TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.insert_base_card_tmplt", cardParameters);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();

                logger.Error("Unable to create base Card Template ({0}, {1}, {2}", specRvsnId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        private ITemplate GetOverAllTemplate(long templateId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ITemplate template = null;
            CardSpecificationDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("OVERL_CARD_PKG.get_overl_card_tmplt_1", parameters);

                if (reader.Read())
                {
                    template = new OverAllCardTemplate(templateId);
                    dbInterface = new CardSpecificationDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                    ((OverAllCardTemplate)template).BaseTemplateId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "base_tmplt_id", true));
                    // ((OverAllCardTemplate)template).BayExtndrSpecnRevsnAltId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_alt_id", true));

                    template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationId);
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

        public long CreateOverAllTemplate(long baseTmpltId, string name, string description, string cuid)
        {
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "Card", false);

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

        public long UpdateOverAllTemplate(long id, long baseTmpltId, long MtrlCatId, long featTypId, string cuid)
        {
            IDbDataParameter[] cardParameters = null;

            try
            {
                cardParameters = dbAccessor.GetParameterArray(18);

                cardParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                cardParameters[1] = dbAccessor.GetParameter("pBaseTmpltId", DbType.Int64, baseTmpltId, ParameterDirection.Input);
                cardParameters[2] = dbAccessor.GetParameter("pMtrlCatId", DbType.Int64, MtrlCatId, ParameterDirection.Input);
                cardParameters[3] = dbAccessor.GetParameter("pFeatTypId", DbType.Int64, featTypId, ParameterDirection.Input);
                cardParameters[4] = dbAccessor.GetParameter("pBayExtndrSpecnRevsnAltId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[5] = dbAccessor.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[6] = dbAccessor.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[7] = dbAccessor.GetParameter("pShelfSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[8] = dbAccessor.GetParameter("pAsnblNdSpcnWthPrtsAsmtId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[9] = dbAccessor.GetParameter("pHlpMtrlRevsnId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[10] = dbAccessor.GetParameter("pComnCnfgId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[11] = dbAccessor.GetParameter("pLabelNm", DbType.String, DBNull.Value, ParameterDirection.Input);
                cardParameters[12] = dbAccessor.GetParameter("pRottnAnglId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[13] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, DBNull.Value, ParameterDirection.Input);
                cardParameters[14] = dbAccessor.GetParameter("pPortTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[15] = dbAccessor.GetParameter("pCnctrTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                cardParameters[16] = dbAccessor.GetParameter("pUserComment", DbType.String, DBNull.Value, ParameterDirection.Input);
                cardParameters[17] = dbAccessor.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "COMPLEX_TMPLT_PKG.update_complex_tmplt", cardParameters);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create overall Bay Template : {0} ", baseTmpltId);
                return 1;
            }

            return 0;
        }

        public Dictionary<long, Dictionary<string, string>> getCardWithSlotDtls(long TmpltId)
        {
            Dictionary<long, Dictionary<string, string>> slotDictLst = new Dictionary<long, Dictionary<string, string>>();
            Dictionary<string, string> slotLst = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            long slotSpecId = 0;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2); //dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, TmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_card_slot_tmplt", parameters);

                if (reader.Read())
                {
                    if (slotLst == null)
                    {
                        slotLst = new Dictionary<string, string>();
                    }
                    slotLst.Add("XMIN_SLOT", DataReaderHelper.GetNonNullValue(reader, "XMIN_SLOT") == "" ? "0" : DataReaderHelper.GetNonNullValue(reader, "XMIN_SLOT"));
                    slotLst.Add("YMIN_SLOT", DataReaderHelper.GetNonNullValue(reader, "YMIN_SLOT") == "" ? "0" : DataReaderHelper.GetNonNullValue(reader, "YMIN_SLOT"));
                    slotLst.Add("SLOT_RTN_ID", DataReaderHelper.GetNonNullValue(reader, "SLOT_RTN_ID"));
                    slotLst.Add("SLOT_SPEC_ID", DataReaderHelper.GetNonNullValue(reader, "SLOT_SPEC_ID"));
                    slotSpecId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "SLOT_SPEC_ID", true));
                }

                if (slotLst == null)
                {
                    slotLst = new Dictionary<string, string>();
                    slotLst.Add("XMIN_SLOT", "0");
                    slotLst.Add("YMIN_SLOT", "0");
                    slotLst.Add("SLOT_RTN_ID", "0");
                    slotLst.Add("SLOT_SPEC_ID", "0");
                }

                slotDictLst.Add(slotSpecId, slotLst);
            }
            catch (Exception ex)
            {
                RollbackTransaction();

                logger.Error("Unable to fetch slot details for base Card Template ({0})", TmpltId);
            }
            finally
            {
                Dispose();
            }

            return slotDictLst;
        }

        public Dictionary<long, Dictionary<string, string>> getCardWithPortDtls(long TmpltId)
        {
            Dictionary<long, Dictionary<string, string>> portDictLst = new Dictionary<long, Dictionary<string, string>>();
            Dictionary<string, string> portLst = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            long portTypId = 0;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2); //dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, TmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_card_port_dtls", parameters);

                if (reader.Read())
                {
                    if (portLst == null)
                    {
                        portLst = new Dictionary<string, string>();
                    }
                    portLst.Add("X_COORD_NO", DataReaderHelper.GetNonNullValue(reader, "X_COORD_NO") == "" ? "0" : DataReaderHelper.GetNonNullValue(reader, "X_COORD_NO"));
                    portLst.Add("Y_COORD_NO", DataReaderHelper.GetNonNullValue(reader, "Y_COORD_NO") == "" ? "0" : DataReaderHelper.GetNonNullValue(reader, "Y_COORD_NO"));
                    portLst.Add("PRTS_ALIAS", DataReaderHelper.GetNonNullValue(reader, "PRTS_ALIAS"));
                    portLst.Add("HAS_ASNBL_PRTS_IND", DataReaderHelper.GetNonNullValue(reader, "HAS_ASNBL_PRTS_IND"));
                    portLst.Add("ASNBL_PRTS", DataReaderHelper.GetNonNullValue(reader, "ASNBL_PRTS"));

                    portTypId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_TYP_ID", true));
                }

                if (portLst == null)
                {
                    portLst = new Dictionary<string, string>();
                    portLst.Add("X_COORD_NO", "0");
                    portLst.Add("Y_COORD_NO", "0");
                    portLst.Add("PRTS_ALIAS", "0");
                    portLst.Add("HAS_ASNBL_PRTS_IND", "0");
                    portLst.Add("ASNBL_PRTS", "0");
                }

                portDictLst.Add(portTypId, portLst);
            }
            catch (Exception ex)
            {
                RollbackTransaction();

                logger.Error("Unable to fetch slot details for base Card Template ({0})", TmpltId);
            }
            finally
            {
                Dispose();
            }

            return portDictLst;
        }
    }
}