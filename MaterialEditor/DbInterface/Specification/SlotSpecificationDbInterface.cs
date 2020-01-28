using System;
using System.Collections.Generic;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;
// added by rxjohn
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.Models;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class SlotSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SlotSpecificationDbInterface() : base()
        {
        }

        public SlotSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification slot = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("slot_specn_pkg.get_slot_specn", parameters);

                if (reader.Read())
                {
                    slot = new SlotSpecification(specificationId);

                    slot.Name = DataReaderHelper.GetNonNullValue(reader, "slot_specn_nm");
                    slot.Description = DataReaderHelper.GetNonNullValue(reader, "slot_dsc");
                    slot.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    slot.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    slot.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;

                    ((SlotSpecification)slot).SlotConsumptionId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_id", true));
                    ((SlotSpecification)slot).SubSlotInd = DataReaderHelper.GetNonNullValue(reader, "sbslt_ind");
                    ((SlotSpecification)slot).StraightThruIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((SlotSpecification)slot).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    ((SlotSpecification)slot).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    ((SlotSpecification)slot).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    ((SlotSpecification)slot).DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    ((SlotSpecification)slot).RevisionName = DataReaderHelper.GetNonNullValue(reader, "slot_specn_revsn_nm");
                    SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                    ((SlotSpecification)slot).SlotUseTypId = dbInterface.GetUseType("Slot");
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve specification id: {0}";

                //hadException = true;

                logger.Error(oe, message, specificationId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to retrieve specification id: {0}", specificationId);
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

            return slot;
        }

        public Dictionary<long, CardSpecification.SlotConsumption> GetSlotConsumptionList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, CardSpecification.SlotConsumption> consumptions = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_only_card_slot_cnsmptn", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_id", true));
                    CardSpecification.SlotConsumption slotConsumption = new CardSpecification.SlotConsumption(specificationId);

                    ((CardSpecification.SlotConsumption)slotConsumption).Quantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_qty", true));
                    ((CardSpecification.SlotConsumption)slotConsumption).PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_prty_no", true));
                    ((CardSpecification.SlotConsumption)slotConsumption).SlotConsumptionType = DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_typ");
                    ((CardSpecification.SlotConsumption)slotConsumption).DefaultIndicator = DataReaderHelper.GetNonNullValue(reader, "dflt_ind");

                    if (consumptions == null)
                        consumptions = new Dictionary<long, CardSpecification.SlotConsumption>();

                    consumptions.Add(slotConsumption.SpecificationId, slotConsumption);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Card Slot Consumption list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Card Slot Consumption list");
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

            return consumptions;
        }

        public Dictionary<long, SpecificationRole> GetRoleList(long slotSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, SpecificationRole> roles = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, slotSpecificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("slot_specn_pkg.get_slot_specn_role_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_role_typ_id", true));
                    SpecificationRole slotRole = new SpecificationRole(specificationId);

                    slotRole.IsSelected = DataReaderHelper.GetNonNullValue(reader, "is_selected") == "Y" ? true : false;
                    slotRole.PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_role_typ_prty_no", true));
                    slotRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "slot_role_typ");

                    if (roles == null)
                        roles = new Dictionary<long, SpecificationRole>();

                    roles.Add(slotRole.SpecificationId, slotRole);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Slot Role list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Slot Role list");
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

            return roles;
        }

        public void UpdateSlotSpecification(long specificationId, string name, string description, int slotConsmptnId, string subslotInd, decimal depth,
         decimal height, decimal width, int uom, bool completionInd, bool propagationInd, bool deleteInd, string straightThrough, string revisionName)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(14);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pSltCnsmptnId", DbType.Int64, slotConsmptnId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pSbSltInd", DbType.String, subslotInd.ToUpper(), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pUom", DbType.Int64, uom, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pCmplt", DbType.String, completionInd ? "Y" : "N", ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagationInd ? "Y" : "N", ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pDel", DbType.String, deleteInd ? "Y" : "N", ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThrough.ToUpper(), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pRvsnNm", DbType.String, revisionName.ToUpper(), ParameterDirection.Input);
                
                //PROCEDURE update_slot(pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2, pSltCnsmptnId IN NUMBER, pSbSltInd IN VARCHAR2,
                //pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2,
                //pDel IN VARCHAR2, pStrghtThruInd IN VARCHAR2, pRvsnNm IN VARCHAR2);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "slot_specn_pkg.update_slot", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update update_slot specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})",
                    specificationId, name, description, slotConsmptnId, subslotInd, depth, height, width, uom, completionInd, propagationInd, deleteInd, straightThrough, revisionName);

                throw ex;
            }
        }

        public void DeleteSlotSpecificationRole(long specificationId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "slot_specn_pkg.delete_slot_specn_role", parameters);
                //PROCEDURE delete_slot_specn_role(pId IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete delete_slot_specn_role ({0})", specificationId);
                throw ex;
            }
        }

        public void InsertSlotSpecificationRole(long specificationId, int roleTypeId, int prtyNo)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int32, roleTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrtyNo", DbType.Int32, prtyNo, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "slot_specn_pkg.insert_slot_specn_role", parameters);
                //PROCEDURE insert_slot_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert insert_slot_specn_role ({0}, {1})", specificationId, roleTypeId);

                throw ex;
            }
        }
        
        public long CreateSlotSpecification(string name, string description, int slotConsmptnId, string subslotInd, decimal depth,
         decimal height, decimal width, int uom, bool completionInd, bool propagationInd, bool deleteInd, string straightThrough, string revisionName)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(14);

                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pSltCnsmptnId", DbType.Int64, slotConsmptnId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pSbSltInd", DbType.String, subslotInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pUom", DbType.Int64, uom, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pCmplt", DbType.String, completionInd ? "Y" : "N", ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagationInd ? "Y" : "N", ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pDel", DbType.String, deleteInd ? "Y" : "N", ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThrough, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pRvsnNm", DbType.String, revisionName.ToUpper(), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "slot_specn_pkg.insert_slot", parameters);

                specificationId = long.Parse(parameters[13].Value.ToString());

                //PROCEDURE insert_slot(pNm IN VARCHAR2, pDsc IN VARCHAR2, pSltCnsmptnId IN NUMBER, pSbSltInd IN VARCHAR2,
                //pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2,
                //pDel IN VARCHAR2, pStrghtThruInd IN VARCHAR2, pRvsnNm IN VARCHAR2, oSpecnId OUT NUMBER);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_slot ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                    name, description, slotConsmptnId, subslotInd, depth, height, width, uom, completionInd, propagationInd, deleteInd, straightThrough, revisionName);
                
                throw ex;
            }

            return specificationId;
        }

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }

        /* added by rxjohn for displaying Alias details */ 
        public async Task<List<SpecificationAlias>> GetAliasAsync(int Id, string specificationType)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<SpecificationAlias> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("specId", DbType.String, Id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("specType", DbType.String, specificationType, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.GET_ALIAS", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SpecificationAlias>();

                        SpecificationAlias result = new SpecificationAlias();

                        result.AliasName = reader["alias_nm"].ToString();
                        result.AliasValue = reader["alias_val"].ToString();
                        result.NTEValue = reader["nte_val"].ToString();
                        result.AppName = reader["appl_nm"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Alias details : {0}, {1}";

                    hadException = true;

                    logger.Error(oe, message, Id, specificationType);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Alias details: {0}, {1}", Id, specificationType);
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

            if (hadException)
                throw new Exception();

            return results;
        }

        public async Task<List<SlotAssignment>> GetSlotAssignAsync(int shfId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<SlotAssignment> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("specId", DbType.String, shfId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.GET_SLOTASSIGNMENT", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SlotAssignment>();

                        SlotAssignment result = new SlotAssignment();

                        result.DefId = reader["SHELF_SPECN_WITH_SLTS_DEF_ID"].ToString();
                        result.SlotSpecID = reader["SLOT_SPECN_ID"].ToString();
                        result.SlotSpecNm = reader["SLOT_SPECN_NM"].ToString();
                        result.SeqNo =  reader["SLOT_SEQ_NO"].ToString();
                        result.SeqQty = reader["SLOT_QTY"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Slot Assignment details for Shelf ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, shfId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Slot Assignment details: {0}", shfId);
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

            if (hadException)
                throw new Exception();

            return results;
        }

        public async Task<string> UpdateSlotAssignAsync(string actionCode, int shelfSpecId , List<SlotAssignment> list)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            
            bool hadException = false;
            string response = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    // Need to open a transaction, since there is collection to update to DB.
                    dbManager.BeginTransaction();

                    foreach (var singleSlot in list)
                    {
                        parameters = dbManager.GetParameterArray(7);

                        parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("shfSpecSltDefId", DbType.Int32, singleSlot.DefId.Contains("NewItem") ? "0" : singleSlot.DefId, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);
                        parameters[3] = dbManager.GetParameter("shfSpecId", DbType.Int32, shelfSpecId, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("slotSpecId", DbType.Int32, singleSlot.SlotSpecID, ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("sltSeqNo", DbType.Int32, singleSlot.SeqNo, ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("sltQty", DbType.Int32, singleSlot.SeqQty, ParameterDirection.Input);

                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_SHELFTOSLOTASSIGN", parameters);

                        response = parameters[2].Value.ToString();
                    
                    }

                    dbManager.CommitTransaction();

                }
                catch (OracleException oe)
                {
                    string message = "Unable to Update Slot Assignment details for Shelf ID : {0}";
                    
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(oe, message, actionCode);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(ex, "Unable to  Update  Slot Assignment details: {0}", actionCode);
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

            if (hadException)
                throw new Exception();

            return response;
           
        }

        // added new method for delete functionality.

        public async Task<string> deleteSlotAssignAsync(int slotDefId, string actionCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string response = string.Empty;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    dbManager.BeginTransaction();

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("shfSpecSltDefId", DbType.Int32, slotDefId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);

                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_SHELFTOSLOTASSIGN", parameters);

                    response = parameters[2].Value.ToString();

                    dbManager.CommitTransaction();
                }
                catch (OracleException oe)
                {
                    string message = "Error in deleteSlotAssignAsync for slot Def ID : {0}";

                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(oe, message, slotDefId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(ex, "Error in deleteSlotAssignAsync for slot Def ID : {0}", slotDefId);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }

            });

            if (hadException)
                throw new Exception();

            return response;
        }

        // added for card to slot Assignment functionality.
        public async Task<string> UpdateCardSlotAssignAsync(string actionCode, int cardSpecId, List<SlotAssignment> list)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            //List<SlotAssignment> results = null;
            bool hadException = false;
            string response = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    // bebin transaction, as we have collection to loop and update to DB
                    dbManager.BeginTransaction();

                    foreach (var singleSlot in list)
                    {
                        parameters = dbManager.GetParameterArray(7);

                        parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("crdSpecSltDefId", DbType.Int32, singleSlot.DefId.Contains("NewItem") ? "0" : singleSlot.DefId, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);
                        parameters[3] = dbManager.GetParameter("crdSpecId", DbType.Int32, cardSpecId, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("slotSpecId", DbType.Int32, singleSlot.SlotSpecID, ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("sltSeqNo", DbType.Int32, singleSlot.SeqNo, ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("sltQty", DbType.Int32, singleSlot.SeqQty, ParameterDirection.Input);

                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_CARDTOSLOTASSIGN", parameters);

                        response = parameters[2].Value.ToString();

                    }

                    dbManager.CommitTransaction();

                }
                catch (OracleException oe)
                {
                    string message = "Unable to Update Slot Assignment details for Shelf ID : {0}";

                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(oe, message, actionCode);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(ex, "Unable to  Update  Slot Assignment details: {0}", actionCode);
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

            if (hadException)
                throw new Exception();

            return response;
        }

        //added for card - slot assignment functionality.
        public async Task<List<SlotAssignment>> GetCardtoSlotAssignAsync(int crdId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<SlotAssignment> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("cardId", DbType.String, crdId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.GETSLOTASSIGNFORCARD", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SlotAssignment>();

                        SlotAssignment result = new SlotAssignment();

                        result.DefId = reader["CARD_SPECN_WITH_SLTS_DEF_ID"].ToString();
                        result.SlotSpecID = reader["SLOT_SPECN_ID"].ToString();
                        result.SlotSpecNm = reader["SLOT_SPECN_NM"].ToString();
                        result.SeqNo = reader["SLOT_SEQ_NO"].ToString();
                        result.SeqQty = reader["SLOT_QTY"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Slot Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, crdId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Slot Assignment details for Card ID: {0}", crdId);
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

            if (hadException)
                throw new Exception();

            return results;
        }

        // added for card - slot assignment - delete functionality
        public async Task<string> deleteCardSlotAssignAsync(int slotDefId, string actionCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string response = string.Empty;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    dbManager.BeginTransaction();

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("crdSpecSltDefId", DbType.Int32, slotDefId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);

                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_CARDTOSLOTASSIGN", parameters);

                    response = parameters[2].Value.ToString();

                    dbManager.CommitTransaction();

                }
                catch (OracleException oe)
                {
                    string message = "Error in deleteSlotAssignAsync for slot Def ID : {0}";

                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(oe, message, slotDefId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(ex, "Error in deleteSlotAssignAsync for slot Def ID : {0}", slotDefId);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (hadException)
                throw new Exception();

            return response;
        }

        //added by rxjohn for Port sequence screen to get Port types
        public async Task<List<Option>> GetPortTypes(int portRoleTyp)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("portRoleTypeId", DbType.Int32, portRoleTyp, ParameterDirection.Input); 
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.get_port_type", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionValue = reader["PORT_TYP_ID"].ToString().ToUpper();
                        string optionText = reader["PORT_TYP"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get PortTypes for Port Sequence screen.");
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

            return options;
        }

        //added by rxjohn for Port sequence screen to get connector types
        public async Task<List<Option>> GetConnectorTypes()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.get_connrt_type", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionValue = reader["CNCTR_TYP_ID"].ToString().ToUpper();
                        string optionText = reader["CNCTR_TYP_CD"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get connector type for Port Sequence screen.");
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

            return options;
        }

        // added for card to port Assignment functionality.
        public async Task<string> UpdateCardPortAssignAsync(string actionCode, int cardSpecId, List<PortAssignment> list)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            //List<SlotAssignment> results = null;
            bool hadException = false;
            string response = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    dbManager.BeginTransaction();

                    foreach (var singleSlot in list)
                    {
                        parameters = dbManager.GetParameterArray(11);

                        parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("crdSpecPrtDefId", DbType.Int32, singleSlot.DefId, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);
                        parameters[3] = dbManager.GetParameter("crdSpecId", DbType.Int32, cardSpecId, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("seqNo", DbType.Int32, singleSlot.PortSeqNo, ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("portTypeId", DbType.Int32, singleSlot.PortTypId, ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("qty", DbType.Int32, singleSlot.PortQty, ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("connectType", DbType.Int32, singleSlot.ConnectorTypId, ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("assignablePort", DbType.String, singleSlot.HasAssignedPort, ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("portName", DbType.String, singleSlot.PortName, ParameterDirection.Input);
                        parameters[10] = dbManager.GetParameter("offSet", DbType.Int32, singleSlot.PortNoOffset, ParameterDirection.Input);
                        
                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_CARDTOPORTASSIGN", parameters);

                        response = parameters[2].Value.ToString();

                    }

                    dbManager.CommitTransaction();

                }
                catch (OracleException oe)
                {
                    string message = "Unable to Update Slot Assignment details for Shelf ID : {0}";

                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(oe, message, actionCode);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;
                    dbManager.RollbackTransaction();
                    logger.Error(ex, "Unable to  Update  Slot Assignment details: {0}", actionCode);
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

            if (hadException)
                throw new Exception();

            return response;
        }

        //added for card - port sequence functionality.
        public async Task<List<PortAssignment>> GetCardtoPortAssignAsync(int crdId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<PortAssignment> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("cardId", DbType.String, crdId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.getPortAssignforCard", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<PortAssignment>();

                        PortAssignment result = new PortAssignment();

                        result.DefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_WITH_PRTS_DEF_ID", true)); 
                        result.PortSeqNo = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_SEQ_NO", true)); 
                        result.PortTypId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_TYP_ID", true));
                        result.PortTypNm = reader["PORT_TYP"].ToString();
                        result.PortQty = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_QTY", true));
                        result.PortNoOffset = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_NO_OFST_VAL", true));
                        result.ConnectorTypId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CNCTR_TYP_ID", true));
                        result.ConnectorTypNm = reader["CNCTR_TYP_CD"].ToString();
                        result.PortName = reader["NW_PORT_NM"].ToString();
                        result.HasAssignedPort = reader["HAS_ASNBL_PRTS_IND"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Slot Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, crdId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Slot Assignment details for Card ID: {0}", crdId);
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

            if (hadException)
                throw new Exception();

            return results;
        }

        // added for card - slot assignment - delete functionality
        public async Task<string> deleteCardPortAssignAsync(int portDefId, string actionCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string response = string.Empty;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("crdSpecPrtDefId", DbType.Int32, portDefId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);

                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.UPDATE_CARDTOPORTASSIGN", parameters);

                    response = parameters[2].Value.ToString();

                }
                catch (OracleException oe)
                {
                    string message = "Error in deleteSlotAssignAsync for slot Def ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, portDefId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Error in deleteCardPortAssignAsync for port Def ID : {0}", portDefId);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (hadException)
                throw new Exception();

            return response;
        }

        //added for getting qualified port for placing cards functionality.
        public async Task<List<SlotAssignment>> GetQualifiedSlotsForCardAsync(float dpth, float hght, float wdth)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<SlotAssignment> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(4);

                    parameters[0] = dbManager.GetParameter("dpth", DbType.String, dpth, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("heght", DbType.String, hght, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("wdth", DbType.String, wdth, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.getQualifiedSlotCard", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SlotAssignment>();

                        SlotAssignment result = new SlotAssignment();

                        result.SlotSpecID = reader["slot_specn_id"].ToString();
                        result.SlotSpecNm = reader["slot_specn_nm"].ToString();
                        result.Depth = float.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                        result.Height = float.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                        result.Width = float.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                        result.DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                       
                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get QualifiedSlotsCard :";

                    hadException = true;

                    logger.Error(oe, message, dpth);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get QualifiedSlotsCard details :", dpth);
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

            if (hadException)
                throw new Exception();

            return results;
        }

        // added for allocating card to qualified slot .
        public async Task<string> UpdateCardtoQualifiedSlotsAssignAsync(string actionCode, int cardSpecId, int sltSpecId, int crdSrtPos)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            //List<SlotAssignment> results = null;
            bool hadException = false;
            string response = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(6);

                    parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("sltSpecCrdCmpId", DbType.Int32, 0, ParameterDirection.Input); // pass input as zero for insert
                    parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);
                    parameters[3] = dbManager.GetParameter("sltSpecId", DbType.Int32, sltSpecId, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("crdSpecId", DbType.Int32, cardSpecId, ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("crdPosId", DbType.Int32, crdSrtPos, ParameterDirection.Input);

                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SLOT_SPECN_PKG.update_cardtoQualSlot", parameters);

                    response = parameters[2].Value.ToString();


                }
                catch (OracleException oe)
                {
                    string message = "Unable to Update Slot Assignment details for Shelf ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, actionCode);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to  Update  Slot Assignment details: {0}", actionCode);
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

            if (hadException)
                throw new Exception();

            return response;
        }

        //added for card - position functionality.
        public async Task<List<Option>> GetCardPositionTypes()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("SLOT_SPECN_PKG.getCardPositionTypes", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                        }

                        string optionValue = reader["CARD_POS_ID"].ToString().ToUpper();
                        string optionText = reader["CARD_POS_VAL"].ToString().ToUpper();
                        string defaultText = reader["DFLT_IND"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText, defaultText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get connector type for Port Sequence screen.");
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

            return options;
        }
    }
}