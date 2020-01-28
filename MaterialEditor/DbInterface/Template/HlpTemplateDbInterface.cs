using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public class HlpTemplateDbInterface : TemplateDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public HlpTemplateDbInterface() : base()
        {

        }

        public HlpTemplateDbInterface(string dbConnectionString) : base(dbConnectionString)
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
                        ((OverAllHlpTemplate)template).TemplateBase = (BaseTemplate)GetBaseTemplate(((OverAllHlpTemplate)template).BaseTemplateId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve shelf template id: {0}; isBase: {1}", templateId, isBaseTemplate);
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
            HighLevelPartDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_base_hlp_tmplt", parameters);

                //SELECT bst.tmplt_id, bst.shelf_specn_revsn_alt_id, t.tmplt_nm, t.tmplt_dsc, t.cmplt_ind, t.prpgt_ind,
                //t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind, ssra.shelf_specn_id
                if (reader.Read())
                {
                    template = new BaseHlpTemplate(templateId);
                    dbInterface = new HighLevelPartDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "HLP_MTRL_REVSN_ID", true));
                    template.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "MTRL_ID", true));
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                   // template.AssociatedSpecification = dbInterface.GetSpecification(template.SpecificationId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve base shelf template id: {0}", templateId);
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

        private ITemplate GetOverAllTemplate(long templateId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ITemplate template = null;
            HighLevelPartDbInterface dbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, templateId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("OVERL_HLP_PKG.get_overl_hlp_tmplt", parameters);

                //SELECT t.tmplt_id, t.tmplt_nm, t.tmplt_dsc, t.cmplt_ind, t.prpgt_ind, t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind,
                //o.base_bay_tmplt_id, o.bay_extndr_specn_revsn_alt_id
                if (reader.Read())
                {
                    template = new OverAllHlpTemplate(templateId);
                    dbInterface = new HighLevelPartDbInterface();

                    template.Description = DataReaderHelper.GetNonNullValue(reader, "tmplt_dsc");
                    template.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    template.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    template.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    template.IsRetired = DataReaderHelper.GetNonNullValue(reader, "ret_tmplt_ind") == "Y" ? true : false;
                    template.Name = DataReaderHelper.GetNonNullValue(reader, "tmplt_nm");
                    template.UpdateInProgress = DataReaderHelper.GetNonNullValue(reader, "updt_in_prgs_ind") == "Y" ? true : false;

                    ((OverAllHlpTemplate)template).BaseTemplateId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "base_tmplt_id", true));
                    //((OverAllHlpTemplate)template).BayExtndrSpecnRevsnAltId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_alt_id", true));

                    //template.AssociatedSpecification = dbInterface.GetMaterial(template.SpecificationId);
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
            IDbDataParameter[] hlpParameters = null;
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "HIGH LEVEL PART", true);

                hlpParameters = dbAccessor.GetParameterArray(2);

                hlpParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                hlpParameters[1] = dbAccessor.GetParameter("pHlpRevsnAltId", DbType.Int64, specRvsnId, ParameterDirection.Input);
                //PROCEDURE insert_base_shelf_tmplt (pTmpltId IN base_shelf_tmplt.tmplt_id%TYPE, pShelfSpecnRevsnAltId IN base_shelf_tmplt.shelf_specn_revsn_alt_id%TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "base_tmplt_pkg.insert_base_hlp_tmplt", hlpParameters);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                id = 0;
                logger.Error("Unable to create base Shelf Template ({0}, {1}, {2}", specRvsnId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        public long CreateOverAllTemplate(long baseTmpltId, string name, string description, string cuid,int hlpRevId)
        {
            long id = 0;

            try
            {
                StartTransaction();

                id = CreateTemplateInTmpltTbl(name, description, "HIGH LEVEL PART", false);

                if (id > 0)
                {
                    CreateOverAllTemplate(id, baseTmpltId, cuid, hlpRevId);
                    CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction();

                logger.Error(ex, "Unable to create overall Shelf Template ({0}, {1}, {2}", baseTmpltId, name, description);
            }
            finally
            {
                Dispose();
            }

            return id;
        }

        public long UpdateOverAllTemplate(long id, long baseTmpltId, long MtrlCatId, long featTypId, string cuid)
        {
            IDbDataParameter[] shelfParameters = null;

            try
            {
                shelfParameters = dbAccessor.GetParameterArray(18);

                shelfParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                shelfParameters[1] = dbAccessor.GetParameter("pBaseTmpltId", DbType.Int64, baseTmpltId, ParameterDirection.Input);
                shelfParameters[2] = dbAccessor.GetParameter("pMtrlCatId", DbType.Int64, MtrlCatId, ParameterDirection.Input);
                shelfParameters[3] = dbAccessor.GetParameter("pFeatTypId", DbType.Int64, featTypId, ParameterDirection.Input);
                shelfParameters[4] = dbAccessor.GetParameter("pBayExtndrSpecnRevsnAltId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[5] = dbAccessor.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[6] = dbAccessor.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[7] = dbAccessor.GetParameter("pShelfSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[8] = dbAccessor.GetParameter("pAsnblNdSpcnWthPrtsAsmtId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[9] = dbAccessor.GetParameter("pHlpMtrlRevsnId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[10] = dbAccessor.GetParameter("pComnCnfgId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[11] = dbAccessor.GetParameter("pLabelNm", DbType.String, DBNull.Value, ParameterDirection.Input);
                shelfParameters[12] = dbAccessor.GetParameter("pRottnAnglId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[13] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, DBNull.Value, ParameterDirection.Input);
                shelfParameters[14] = dbAccessor.GetParameter("pPortTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[15] = dbAccessor.GetParameter("pCnctrTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                shelfParameters[16] = dbAccessor.GetParameter("pUserComment", DbType.String, DBNull.Value, ParameterDirection.Input);
                shelfParameters[17] = dbAccessor.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "COMPLEX_TMPLT_PKG.update_complex_tmplt", shelfParameters);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create overall Bay Template : {0} ", baseTmpltId);
                return 1;
            }

            return 0;
        }

        public List<Slot> GetSlots(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Slot> slots = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pShlfSpcnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("base_tmplt_pkg.get_shelf_slot_specn", parameters);

                //SELECT sswsd.slot_specn_id, ss.hgt_no, ss.wdth_no, sswss.slot_no, sswss.x_coord_no, sswss.y_coord_no, sswss.label_nm, 
                //du.dim_uom_cd, sswss.shelf_specn_with_slts_slts_id
                while (reader.Read())
                {
                    Slot slot = new Slot();

                    if(slots == null)
                        slots = new List<Slot>();

                    slot.Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    slot.LabelName = DataReaderHelper.GetNonNullValue(reader, "label_nm");
                    slot.SlotNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_no", true));
                    slot.SlotSpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_specn_id", true));
                    slot.SlotsSlotsId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_specn_with_slts_slts_id", true));
                    slot.UnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");
                    slot.Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    slot.X = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "x_coord_no", true));
                    slot.Y = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "y_coord_no", true));

                    slots.Add(slot);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve shelf template slots: {0}", specificationId);
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

            return slots;
        }
    }
}