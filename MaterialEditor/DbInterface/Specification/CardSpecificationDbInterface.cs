using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class CardSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CardSpecificationDbInterface() : base()
        {
        }

        public CardSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification card = null;
            CardDbInterface cardDbInterface = null;
            MaterialDbInterface materialDbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn", parameters);

                if (reader.Read())
                {
                    //string interimIndicator = DataReaderHelper.GetNonNullValue(reader, "intm_ind");

                    card = new CardSpecification(specificationId);

                    card.Name = DataReaderHelper.GetNonNullValue(reader, "card_specn_nm");
                    card.Description = DataReaderHelper.GetNonNullValue(reader, "card_dsc");
                    card.NDSSpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "nds_id", true));

                    ((CardSpecification)card).SlotConsumptionId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_id", true));
                    //((CardSpecification)card).Position = DataReaderHelper.GetNonNullValue(reader, "card_pos_val");
                    ((CardSpecification)card).HasSlots = DataReaderHelper.GetNonNullValue(reader, "has_slts_ind") == "Y" ? true : false;
                    ((CardSpecification)card).HasPorts = DataReaderHelper.GetNonNullValue(reader, "has_prts_ind") == "Y" ? true : false;
                    ((CardSpecification)card).StraightThruIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((CardSpecification)card).CardUseTypId = GetUseType("Card", ((CardSpecification)card).RoleList);
                    ((CardSpecification)card).CardNDSUseTyp = DataReaderHelper.GetNonNullValue(reader, "use_typ");
                    //if ("N".Equals(interimIndicator))
                    //{
                    long mtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                    long materialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rme_card_mtrl_revsn_id", true));

                    card.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    card.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    card.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;

                    //((CardSpecification)card).IsInterim = false;
                    ((CardSpecification)card).RevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "card_specn_revsn_alt_id", true));
                    ((CardSpecification)card).RevisionName = DataReaderHelper.GetNonNullValue(reader, "card_specn_revsn_nm");
                    ((CardSpecification)card).IsRecordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind") == "Y" ? true : false;

                    if (mtrlId > 0 && materialItemId > 0)
                    {
                        cardDbInterface = new CardDbInterface();
                        materialDbInterface = new MaterialDbInterface();
                        MaterialItem cardMaterial = null;

                        ((CardSpecification)card).AssociatedMaterial = cardDbInterface.GetMaterial(materialItemId, mtrlId);

                        Task t = Task.Run(async () =>
                        {
                            cardMaterial = await materialDbInterface.GetMaterialItemSAPAsync(materialItemId);
                        });

                        t.Wait();

                        if (cardMaterial != null)
                        {
                            if (cardMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                                card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, cardMaterial.Attributes[MaterialType.JSON.ItmDesc]);
                            else
                                card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));

                            if (cardMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, cardMaterial.Attributes[MaterialType.JSON.PrtNo]);
                            else
                                card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                        }
                        else
                        {
                            card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));
                            card.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                        }
                    }
                    //}
                    //else
                    //{
                    //    card.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "intm_cmplt_ind") == "Y" ? true : false;
                    //    card.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "intm_prpgt_ind") == "Y" ? true : false;                        

                    //    ((CardSpecification)card).IsInterim = true;
                    //    ((CardSpecification)card).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    //    ((CardSpecification)card).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    //    ((CardSpecification)card).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    //    ((CardSpecification)card).DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    //}

                    //if (bayInternalId > 0)
                    //{
                    //    bayInternalDbInterface = new BayInternalDbInterface();

                    //    ((BaySpecification)bay).BayInternal = bayInternalDbInterface.GetSpecification(bayInternalId);
                    //}
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

            return card;
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

                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_slot_cnsmptn", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_id", true));
                    CardSpecification.SlotConsumption slotConsumption = new CardSpecification.SlotConsumption(specificationId);

                    ((CardSpecification.SlotConsumption)slotConsumption).Quantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_qty", true));
                    ((CardSpecification.SlotConsumption)slotConsumption).PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_prty_no", true));
                    ((CardSpecification.SlotConsumption)slotConsumption).SlotConsumptionType = DataReaderHelper.GetNonNullValue(reader, "card_slot_cnsmptn_typ");
                    ((CardSpecification.SlotConsumption)slotConsumption).Position = DataReaderHelper.GetNonNullValue(reader, "card_pos_val");
                    ((CardSpecification.SlotConsumption)slotConsumption).RowNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ROWNUM", true));
                    ((CardSpecification.SlotConsumption)slotConsumption).DefaultIndicator = DataReaderHelper.GetNonNullValue(reader, "dflt_ind");

                    if (consumptions == null)
                        consumptions = new Dictionary<long, CardSpecification.SlotConsumption>();

                    consumptions.Add(slotConsumption.RowNumber, slotConsumption);
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

        public Dictionary<long, SpecificationRole> GetRoleList(long cardSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, SpecificationRole> roles = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, cardSpecificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_role_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "card_role_typ_id", true));
                    SpecificationRole cardRole = new SpecificationRole(specificationId);

                    cardRole.IsSelected = DataReaderHelper.GetNonNullValue(reader, "is_selected") == "Y" ? true : false;
                    cardRole.PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_role_typ_prty_no", true));
                    cardRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "card_role_typ");
                    cardRole.RMEUseType = DataReaderHelper.GetNonNullValue(reader, "rme_use_typ");

                    if (roles == null)
                        roles = new Dictionary<long, SpecificationRole>();

                    roles.Add(cardRole.SpecificationId, cardRole);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Card Role list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Card Role list");
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

        public Dictionary<long, SpecificationRole> GetUseTypeList(long cardSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, SpecificationRole> useTypes = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, 4, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_use_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "specn_record_use_typ_id", true));
                    SpecificationRole cardRole = new SpecificationRole(specificationId);

                    cardRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "use_typ");

                    if (useTypes == null)
                        useTypes = new Dictionary<long, SpecificationRole>();

                    useTypes.Add(cardRole.SpecificationId, cardRole);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Use Type list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Use Type list");
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

            return useTypes;
        }

        public long CreateCardSpecification(string name, string description, long slotConsumptionId, bool hasSlots, bool hasPorts, string straightThruIndictor, string useType) //, bool isInterim)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pSltCnsmptnId", DbType.Int32, CheckNullValue(slotConsumptionId), ParameterDirection.Input);
                //parameters[3] = dbAccessor.GetParameter("pPosVal", DbType.String, CheckNullValue(position, true), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pSltsInd", DbType.String, hasSlots ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pPrtsInd", DbType.String, hasPorts ? "Y" : "N", ParameterDirection.Input);
                //parameters[6] = dbAccessor.GetParameter("pIntmInd", DbType.String, isInterim ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThruIndictor, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pUseTyp", DbType.String, useType, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.insert_card_specn", parameters);

                specificationId = long.Parse(parameters[7].Value.ToString());

                //PROCEDURE insert_card_specn(pNm IN VARCHAR2, pDsc IN VARCHAR2, pSltCnsmptnId IN NUMBER, 
                //pSltsInd IN VARCHAR2, pPrtsInd IN VARCHAR2, oSpecnId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create card_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", name, description, slotConsumptionId, hasSlots, hasPorts, straightThruIndictor, useType);

                throw ex;
            }

            return specificationId;
        }

        public void CreateInterimCardSpecification(long specificationId, decimal depth, decimal height, decimal width, int uomId, bool completed, bool propagated)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.insert_card_specn_intm", parameters);

                //PROCEDURE insert_card_specn_intm(pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDpth IN NUMBER,
                //pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create card_specn_intm ({0}, {1}, {2}, {3}, {4}, {5}, {6})", specificationId, depth, height, width, uomId, completed, propagated);

                throw ex;
            }
        }

        public long CreateCardSpecificationRevision(long specificationId, string revisionName, bool ro, bool deleted, bool completed, bool propagated)
        {
            IDbDataParameter[] parameters = null;
            long revisionId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, revisionName.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRo", DbType.String, ro ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pDel", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("oRevsnAltId", DbType.Int64, revisionId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.insert_card_specn_revsn_alt", parameters);

                revisionId = long.Parse(parameters[6].Value.ToString());

                //PROCEDURE insert_card_specn_revsn_alt(pId IN number, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2, 
                //pPrpgt IN VARCHAR2, pDel IN NUMBER, oRevsnAltId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create card_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5})", specificationId, revisionName, ro, deleted, completed, propagated);

                throw ex;
            }

            return revisionId;
        }

        public void UpdateCardSpecification(long specificationId, string name, string description, long slotConsumptionId, bool hasSlots, bool hasPorts, string straightThruIndictor, string useType) //, bool isInterim)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pSltCnsmptnId", DbType.Int32, CheckNullValue(slotConsumptionId), ParameterDirection.Input);
                //parameters[4] = dbAccessor.GetParameter("pPosVal", DbType.String, CheckNullValue(position, true), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pSltsInd", DbType.String, hasSlots ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPrtsInd", DbType.String, hasPorts ? "Y" : "N", ParameterDirection.Input);
                //parameters[7] = dbAccessor.GetParameter("pIntmInd", DbType.String, isInterim ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThruIndictor, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pUseTyp", DbType.String, useType, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.update_card_specn", parameters);

                //update_card_specn(pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2, pSltCnsmptnId IN NUMBER, 
                //pSltsInd IN VARCHAR2, pPrtsInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update card_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, name, description, slotConsumptionId, hasSlots, hasPorts, straightThruIndictor, useType);

                throw ex;
            }
        }

        public void UpdateInterimCardSpecification(long specificationId, decimal depth, decimal height, decimal width, int uomId, bool completed, bool propagated)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.update_card_specn_intm", parameters);

                //PROCEDURE update_card_specn_intm(pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDpth IN NUMBER, 
                //pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update card_specn_intm ({0}, {1}, {2}, {3}, {4}, {5}, {6})", specificationId, depth, height, width, uomId, completed, propagated);

                throw ex;
            }
        }

        public void UpdateCardSpecificationRevision(long revisionId, long specificationId, string revisionName, bool ro, bool deleted, bool completed, bool propagated)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pAltId", DbType.Int64, revisionId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pNm", DbType.String, revisionName.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRo", DbType.String, ro ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pDel", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.update_card_specn_revsn_alt", parameters);

                //PROCEDURE update_card_specn_revsn_alt(pAltId IN NUMBER, pId IN NUMBER, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2, 
                //pPrpgt IN VARCHAR2, pDel IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update card_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5}, {6})", revisionId, specificationId, revisionName, ro, deleted, completed, propagated);

                throw ex;
            }
        }

        public void InsertCardSpecificationRole(long specificationId, int roleTypeId, int priorityNumber)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int64, roleTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrtyNo", DbType.Int64, priorityNumber, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.insert_card_specn_role", parameters);

                //PROCEDURE insert_card_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert card_specn_role ({0}, {1}, {2})", specificationId, roleTypeId, priorityNumber);

                throw ex;
            }
        }

        public void UpdateCardSpecificationRole(long specificationId, int roleTypeId, int priorityNumber)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int64, roleTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrtyNo", DbType.Int64, priorityNumber, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.update_card_specn_role", parameters);

                //PROCEDURE update_card_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update card_specn_role ({0}, {1}, {2})", specificationId, roleTypeId, priorityNumber);

                throw ex;
            }
        }

        public void DeleteCardSpecificationRole(long specificationId, int roleTypeId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int64, roleTypeId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.delete_card_specn_role", parameters);

                //PROCEDURE delete_card_specn_role(pId IN NUMBER, pRlTypId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete card_specn_role ({0})", specificationId);

                throw ex;
            }
        }

        public void DeleteCardSpecificationRole(long specificationId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.delete_card_specn_role", parameters);

                //PROCEDURE delete_card_specn_role(pId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete card_specn_role ({0})", specificationId);

                throw ex;
            }
        }

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }

        public void AssociateMaterial(long specificationRevisionId, long materialItemId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnRvsnId", DbType.Int64, CheckNullValue(specificationRevisionId), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.associate_material", parameters);

                //PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to associate material ({0}, {1})", specificationRevisionId, materialItemId);

                throw ex;
            }
        }

        public bool UpdateCardSpecificationMaterial(long materialItemId, decimal height, decimal depth, decimal width, int uomId, decimal normalCurrentDrain,
            int normalCurrentDrainUom, decimal maxCurrentDrain, int maxCurrentDrainUom, decimal cardWeight, int cardWeightUom, decimal heatDissipation, int heatDissipationUom)
        {
            IDbDataParameter[] parameters = null;
            bool didUpdate = false;
            string output = "";

            try
            {
                parameters = dbAccessor.GetParameterArray(14);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pElcNrmnl", DbType.Decimal, CheckNullValue(normalCurrentDrain), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pElcNrmnlUom", DbType.Int32, CheckNullValue(normalCurrentDrainUom), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pElcMx", DbType.Decimal, CheckNullValue(maxCurrentDrain), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pElcMxUom", DbType.Int32, CheckNullValue(maxCurrentDrainUom), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pWt", DbType.Decimal, CheckNullValue(cardWeight), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pWtUom", DbType.Int32, CheckNullValue(cardWeightUom), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pHt", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pHtUom", DbType.Int32, CheckNullValue(heatDissipationUom), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("didUpdate", DbType.String, output, ParameterDirection.Output, 1);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.update_card_material", parameters);

                output = parameters[13].Value.ToString();

                if ("Y".Equals(output))
                    didUpdate = true;

                //PROCEDURE update_card_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, 
                //pUom IN NUMBER, pElcNrmnl IN NUMBER, pElcNrmnlUom IN NUMBER, pElcMx IN NUMBER, pElcMxUom IN NUMBER, 
                //pWt IN NUMBER, pWtUom IN NUMBER, pHt IN NUMBER, pHtUom IN NUMBER, didUpdate OUT VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update associated material ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})", materialItemId,
                    height, depth, width, uomId, normalCurrentDrain, normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, cardWeight, cardWeightUom, heatDissipation, heatDissipationUom);

                throw ex;
            }

            return didUpdate;
        }

        public void UpsertNDSSpecificationId(long specificationId, long ndsSpecificationId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pSpecnRvsnId", DbType.Int64, CheckNullValue(specificationId), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pMtlItmId", DbType.Int64, ndsSpecificationId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "card_specn_pkg.insert_nds_spec_id", parameters);

                //PROCEDURE insert_nds_spec_id(pSpecRvsnId IN NUMBER, pNdsSpecId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update NDS specification id ({0}, {1})", specificationId, ndsSpecificationId);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public string GetUseType(string specTyp, Dictionary<long, SpecificationRole> roleTypLst)
        {
            string sql = "";
            string useTyp = "";
            string gnrcSql = @"select val.alias_val from specn_typ_alias_val val,specn_typ_alias ali,specn_typ typ where ali.specn_typ_alias_id=val.specn_typ_alias_id
and typ.specn_typ_id = val.specn_typ_id and typ.specn_typ='" + specTyp + "' and ali.alias_nm='RME Record Use Type'";
            IAccessor dbManager = null;
            IDataReader reader = null;

            try
            {
                foreach (KeyValuePair<long, SpecificationRole> roleL in roleTypLst)
                {
                    if (roleL.Value.IsSelected)
                    {
                        sql = @"select val.alias_val from card_role_typ_alias_val val,card_role_typ_alias ali,card_role_typ typ
                        where val.card_role_typ_alias_id = ali.card_role_typ_alias_id 
                        and ali.alias_nm='RME Record Use Type' 
                        and val.card_role_typ_id=typ.card_role_typ_id
                        and typ.card_role_typ ='" + roleL.Value.RoleType + "'";


                        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                        if (reader.Read())
                        {
                            useTyp = DataReaderHelper.GetNonNullValue(reader, "alias_val");

                        }
                    }
                }
                if (useTyp == "")
                {
                    SpecificationDbInterface dbI = new SpecificationDbInterface();
                    useTyp = dbI.GetGenericUseType(gnrcSql);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetUseType");
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

            return useTyp;
        }

        /* added by rxjohn for Card - Plugin functionality. */
        public async Task<List<CardPluginAssignment>> GetCardtoPluginAssignAsync(int crdId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CardPluginAssignment> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("cardId", DbType.String, crdId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("CARD_SPECN_PKG.getPluginAssignforCard", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<CardPluginAssignment>();

                        CardPluginAssignment result = new CardPluginAssignment();

                        result.DefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_PLG_IN_CMPB_ID", true));
                        result.PluginSpecId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PLG_IN_SPECN_ID", true));
                        result.PluginSpecNm = reader["PLG_IN_SPECN_NM"].ToString();
                        result.PluginRoleType = reader["PLG_IN_ROLE_TYP"].ToString();
                        result.PluginCntrType = reader["CNCTR_TYP_CD"].ToString();
                        result.PartNo = reader["PART_NO"].ToString();
                        result.CleiCd = reader["CLEI_CD"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Card-Plugin Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, crdId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Card-Plugin Assignment details for Card ID: {0}", crdId);
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

        // added for update functionality for Card - Plugin assignment functionality.
        public async Task<string> UpdateCardPluginAssignAsync(string actionCode, int cardSpecId, List<CardPluginAssignment> list)
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

                    foreach (var singleSlot in list)
                    {
                        parameters = dbManager.GetParameterArray(5);

                        parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("crdSpecPlugDefId", DbType.Int32, singleSlot.DefId, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("resMess", DbType.String, response, ParameterDirection.Output, 250);
                        parameters[3] = dbManager.GetParameter("crdSpecId", DbType.Int32, cardSpecId, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pluginSpecId", DbType.Int32, singleSlot.PluginSpecId, ParameterDirection.Input);

                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "CARD_SPECN_PKG.update_cardtoPluginAssign", parameters);

                        response = parameters[2].Value.ToString();

                    }

                }
                catch (OracleException oe)
                {
                    string message = "Unable to Update Card-Plugin Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, cardSpecId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to  Update  Card-Plugin Assignment details: {0}", cardSpecId);
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

        // Added code by
        // Author : Shivanand S Gaddi
        // Date : 17/01/2019
        // Description : Card Has slot checkbox pop up window details and modification

        // This method get all the slot details for particular card id.
        public class CardHasSlots
        {
            public int cardSlotDefId { get; set; }
            public int cardSlotSpecId { get; set; }
            public int SlotSequence { get; set; }
            public string SlotType { get; set; }
            public int SlotQuantity { get; set; }
            public bool UnSelectToRemove { get; set; }
            public string GUID { get; set; }

        }
        public async Task<List<CardHasSlots>> GetSlotList(long cardSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<CardHasSlots> slotDtls = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pCardSpecnWithCardId", DbType.Int64, cardSpecificationId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_slts_def", parameters);

                    while (reader.Read())
                    {
                        if (slotDtls == null)
                            slotDtls = new List<CardHasSlots>();

                        CardHasSlots slotDtlsObj = new CardHasSlots();
                        slotDtlsObj.SlotSequence = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_seq_no", true));
                        string slotDefId = DataReaderHelper.GetNonNullValue(reader, "card_specn_with_slts_def_id");
                        slotDtlsObj.cardSlotDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_specn_with_slts_def_id", true));
                        slotDtlsObj.SlotType = reader["slot_specn_nm"].ToString();
                        slotDtlsObj.cardSlotSpecId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_specn_id", true));
                        slotDtlsObj.SlotQuantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_qty", true));
                        slotDtlsObj.UnSelectToRemove = true;

                        slotDtlsObj.GUID = Guid.NewGuid().ToString(); // global identifier so we can find both new and existing records on the UI

                        slotDtls.Add(slotDtlsObj);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card slot list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card slot list");
                    hadException = true;
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
            return slotDtls;
        }
        // End of fetching slot details for particular card.

        //This method Insert/Modify the slot details.
        public async Task<string> insertCardSlot(long cardSpecid, long slotSpecId, long seqNum, long pSlotQty, int pSlotDefid, string actionCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string response = string.Empty;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(7);

                    parameters[0] = dbManager.GetParameter("pCardSpecnId", DbType.Int64, cardSpecid, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pSlotSeqNo", DbType.Int64, seqNum, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pSlotSpecnId", DbType.Int64, slotSpecId, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pSlotQty", DbType.Int64, pSlotQty, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pSlotDefid", DbType.Int64, pSlotDefid, ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.ins_card_specn_with_slts_def", parameters);

                    response = parameters[5].Value.ToString();

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to insert Card slot list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to insert Card slot list");
                    hadException = true;
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
        //End of insert/modify slot details.

        //This method fetch the slot details for particular card after save/modify slot sequence.
        public List<CardHasSlots> getCardSlotDtls(long cardSpecid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string response = string.Empty;
            bool hadException = false;
            List<CardHasSlots> slotLstObj = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pCardSpecnWithCardId", DbType.Int64, cardSpecid, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_slts_def", parameters);

                while (reader.Read())
                {
                    if (slotLstObj == null)
                        slotLstObj = new List<CardHasSlots>();

                    CardHasSlots slotDtlsObj = new CardHasSlots();
                    slotDtlsObj.SlotSequence = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_seq_no", true));
                    string slotDefId = DataReaderHelper.GetNonNullValue(reader, "card_specn_with_slts_def_id");
                    slotDtlsObj.cardSlotDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "card_specn_with_slts_def_id", true));
                    slotDtlsObj.SlotType = reader["slot_specn_nm"].ToString();
                    slotDtlsObj.cardSlotSpecId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_specn_id", true));
                    slotDtlsObj.SlotQuantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slot_qty", true));
                    slotDtlsObj.UnSelectToRemove = true;

                    slotDtlsObj.GUID = Guid.NewGuid().ToString(); // global identifier so we can find both new and existing records on the UI

                    slotLstObj.Add(slotDtlsObj);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Card slot list");
                hadException = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Card slot list");
                hadException = true;
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
            if (hadException)
                throw new Exception();

            return slotLstObj;
        }
        // End of fetch the slot details for particular card after save/modify slot sequence.

        //This method save/update/delete slot type details.
        public async Task<List<CardHasSlots>> saveCardSlotDtls(List<CardHasSlots> list, int cardId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            //string response = string.Empty;
            List<CardHasSlots> slotLstObj = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    foreach (var SlotDtls in list)
                    {
                        if (SlotDtls.UnSelectToRemove == true)
                        {
                            parameters = dbManager.GetParameterArray(6);
                            parameters[0] = dbManager.GetParameter("pCardSpecnId", DbType.Int64, cardId, ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameter("pSlotSeqNo", DbType.Int64, SlotDtls.SlotSequence, ParameterDirection.Input);
                            parameters[2] = dbManager.GetParameter("pSlotSpecnId", DbType.Int64, SlotDtls.cardSlotSpecId, ParameterDirection.Input);
                            parameters[3] = dbManager.GetParameter("pSlotQty", DbType.Int64, SlotDtls.SlotQuantity, ParameterDirection.Input);
                            parameters[4] = dbManager.GetParameter("pCardSpecnWithSltsDefId", DbType.Int64, SlotDtls.cardSlotDefId, ParameterDirection.Input);
                            parameters[5] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                            reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.upd_card_specn_with_slts_def", parameters);

                            //response += "Slot sequence number " + SlotDtls.SlotSequence + " - " + parameters[5].Value.ToString() + "<br/>";
                        }
                        else
                        {
                            parameters = dbManager.GetParameterArray(2);
                            parameters[0] = dbManager.GetParameter("pCardSpecnWithSltsDefId", DbType.Int64, SlotDtls.cardSlotDefId, ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                            reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.del_card_specn_with_slts_def", parameters);

                            //response += parameters[1].Value.ToString() + "<br/>";
                        }
                    }

                    slotLstObj = getCardSlotDtls(cardId);

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to save Card slot list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to save Card slot list");
                    hadException = true;
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

            return slotLstObj;
        }
        //End of save/update/delete slot type details.

        //This method get Slotsequence details.
        public class ManageSlots
        {
            // mwj: normalized spelling and change EQDES and HORZDISP to string types to keep things simple...
            public int SlotSlotId { get; set; }
            public int SlotSlotNo { get; set; }
            public int SlotDefId { get; set; }
            public string SlotName { get; set; }
            public string Label { get; set; } // can be null
            public string EQDES { get; set; } // can be null
            public string HorzDisp { get; set; } // can be null
            public string RotationAngleId { get; set; }  // can be null
            public bool UnSelectToRemove { get; set; }
            public string actionCode { get; set; }

            public string GUID { get; set; }
        }


        public async Task<List<ManageSlots>> GetCardSpecnWithSlots(int slotQuantity, int slotDefId, string slotType, int SlotSpecId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<ManageSlots> slotMangDtls = new List<ManageSlots>();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("cardSlotDefId", DbType.Int64, slotDefId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_slts_slts", parameters);

                    var slotNo = 0;
                    while (reader.Read())
                    {
                        slotNo++;

                        ManageSlots slotMangDtlsObj = new ManageSlots();
                        slotMangDtlsObj.SlotSlotNo = int.Parse(DataReaderHelper.GetNonNullValue(reader, "SLOT_NO", true));
                        slotMangDtlsObj.SlotSlotId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_WITH_SLTS_SLTS_ID", true));
                        slotMangDtlsObj.SlotDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_WITH_SLTS_DEF_ID", true));
                        slotMangDtlsObj.SlotName = slotType;
                        slotMangDtlsObj.Label = reader["LABEL_NM"].ToString();
                        slotMangDtlsObj.EQDES = DataReaderHelper.GetNonNullValue(reader, "Y_COORD_NO", true);
                        slotMangDtlsObj.HorzDisp = DataReaderHelper.GetNonNullValue(reader, "X_COORD_NO", true);
                        slotMangDtlsObj.RotationAngleId = DataReaderHelper.GetNonNullValue(reader, "ROTTN_ANGL_ID", true);

                        slotMangDtlsObj.UnSelectToRemove = true;
                        slotMangDtlsObj.actionCode = "UPDATE";
                        slotMangDtlsObj.GUID = Guid.NewGuid().ToString(); // global identifier so we can find both new and existing records on the UI

                        slotMangDtls.Add(slotMangDtlsObj);

                        slotQuantity = slotQuantity - 1;

                    }

                    while (slotQuantity > 0)
                    {
                        slotNo++;

                        ManageSlots slotMangDtlsObj = new ManageSlots();
                        slotMangDtlsObj.SlotSlotNo = slotNo;
                        slotMangDtlsObj.SlotDefId = slotDefId;
                        slotMangDtlsObj.SlotName = slotType;
                        slotMangDtlsObj.Label = "";
                        slotMangDtlsObj.EQDES = "";
                        slotMangDtlsObj.HorzDisp = "";
                        slotMangDtlsObj.RotationAngleId = "0";

                        slotMangDtlsObj.UnSelectToRemove = true;
                        slotMangDtlsObj.actionCode = "INSERT";
                        slotMangDtlsObj.GUID = Guid.NewGuid().ToString(); // global identifier so we can find both new and existing records on the UI

                        slotMangDtls.Add(slotMangDtlsObj);

                        slotQuantity--;
                    }

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card slot sequence list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card slot sequence list");
                    hadException = true;
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

            return slotMangDtls;
        }
        //End of slot sequence details.

        public class cardPortsDtls
        {
            public int portDefId { get; set; }
            public int sequenceNum { get; set; }
            public string portType { get; set; }
            public string connector { get; set; }
            public string namingConv { get; set; }
            public int portCount { get; set; }
            public string assignablePort { get; set; }
            public bool UnSelectToRemove { get; set; }
            public int startingPortNo { get; set; }
            public int portOffsetVal { get; set; }
        }


        public async Task<List<cardPortsDtls>> GetCardPortsList(int cardSpecId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<cardPortsDtls> partsDtls = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pCardSpecnWithCardId", DbType.Int64, cardSpecId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_ports_def", parameters);

                    while (reader.Read())
                    {
                        if (partsDtls == null)
                            partsDtls = new List<cardPortsDtls>();

                        cardPortsDtls partsDtlsObj = new cardPortsDtls();
                        partsDtlsObj.portDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "portDefId", true));
                        partsDtlsObj.sequenceNum = int.Parse(DataReaderHelper.GetNonNullValue(reader, "sequenceNo", true));
                        partsDtlsObj.portType = reader["porttype"].ToString();
                        partsDtlsObj.connector = reader["connector"].ToString();
                        partsDtlsObj.namingConv = reader["namingconvention"].ToString();
                        partsDtlsObj.portCount = int.Parse(DataReaderHelper.GetNonNullValue(reader, "portcount", true));
                        partsDtlsObj.assignablePort = reader["assignableport"].ToString();
                        partsDtlsObj.UnSelectToRemove = true;

                        partsDtls.Add(partsDtlsObj);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card port list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card port list");
                    hadException = true;
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

            return partsDtls;
        }

        public async Task<string> getCardSlotQuantity(long slotDefId, long slotQntity)
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

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pCardSpecnWithSltsDefId", DbType.Int64, slotDefId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pCardSpecnWithSltsQntity", DbType.Int64, slotQntity, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_slts_quantity", parameters);

                    response = parameters[2].Value.ToString();

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card slot quantity");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card  slot quantity");
                    hadException = true;
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
        public async Task<string> insertUpdateManageList(List<ManageSlots> list, long cardId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string response = string.Empty;
            string procedurename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    foreach (var SlotMng in list)
                    {
                        if (SlotMng.actionCode == "DELETE")
                        {
                            var sproc = "CARD_SPECN_PKG.del_card_specn_with_slts_slts";
                            var parms = dbManager.GetParameterArray(1);
                            parms[0] = dbManager.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int32, SlotMng.SlotSlotId, ParameterDirection.Input);

                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, sproc, parms);
                        }
                        else
                        {
                            parameters = dbManager.GetParameterArray(8);
                            if (SlotMng.HorzDisp == "") { SlotMng.HorzDisp = null; }
                            if (SlotMng.EQDES == "") { SlotMng.EQDES = null; }
                            if (SlotMng.RotationAngleId == "") { SlotMng.RotationAngleId = "1"; }

                            parameters[0] = dbManager.GetParameter("pCardSpecnWithSltsDefId", DbType.Int32, SlotMng.SlotDefId, ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameter("pSlotNo", DbType.Int32, SlotMng.SlotSlotNo, ParameterDirection.Input);
                            // mwj: changed coords to DECIMAL instead of INTEGER
                            // mwj: need to be able to handle null values for Label, X, Y, Rotation Angle
                            parameters[2] = dbManager.GetParameter("pXCoordNo", DbType.Decimal, SlotMng.HorzDisp, ParameterDirection.Input);
                            parameters[3] = dbManager.GetParameter("pYCoordNo", DbType.Decimal, SlotMng.EQDES, ParameterDirection.Input);
                            parameters[4] = dbManager.GetParameter("pLabelNm", DbType.String, SlotMng.Label, ParameterDirection.Input);
                            parameters[5] = dbManager.GetParameter("pRottnAnglId", DbType.Int32, SlotMng.RotationAngleId, ParameterDirection.Input);
                            parameters[6] = dbManager.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int32, SlotMng.SlotSlotId, ParameterDirection.Input);
                            parameters[7] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);
                            procedurename = (SlotMng.actionCode == "INSERT") ? "CARD_SPECN_PKG.ins_card_specn_with_slts_slts" : "CARD_SPECN_PKG.upd_card_specn_with_slts_slts";
                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, procedurename, parameters);

                            response = parameters[7].Value.ToString();
                        }
                    }
                }


                catch (OracleException oe)
                {
                    response = oe.ToString();
                    logger.Error(oe, "Unable to retrieve Card slot list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    response = ex.ToString();
                    logger.Error(ex, "Unable to retrieve Card slot list");
                    hadException = true;
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

        public async Task<string> CardSlotSeqvalidations(string ReqType, decimal validationValue)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string response = string.Empty;
            string procedurename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);


                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pValidationType", DbType.String, ReqType, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pValidationVal", DbType.Decimal, validationValue, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);
                    procedurename = "CARD_SPECN_PKG.validate_card_specn_with_slts";
                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, procedurename, parameters);

                    response = parameters[2].Value.ToString();

                }


                catch (OracleException oe)
                {
                    response = oe.ToString();
                    logger.Error(oe, "Unable to validate the card slot");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    response = ex.ToString();
                    logger.Error(ex, "Unable to alidate the card slot ");
                    hadException = true;
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

        // mwj: this isn't a slot sequence, this is a slot type 
        public class cardSlotsSeq
        {
            // mwj: let's just track one-for-one aginst the column name; 
            public string SLOT_SPECN_ID { get; set; }
            public string SLOT_SPECN_NM { get; set; }
            public string SLOT_DSC { get; set; }
            public string SLOT_SPECN_REVSN_NM { get; set; }
            public string SLOT_SPECN_DIMS { get; set; }
            public string SLOT_SPECN_STATUS { get; set; }

            //public string ID { get; set; }
            //public string Specname { get; set; }
            //public string SpecDesc { get; set; }
            //public string SlotType { get; set; }
            //public string Dimension { get; set; }

            public string Status { get; set; }
        }


        public async Task<List<cardSlotsSeq>> GetCardSlotSeqList(string specName, string specDesc, string revName, string status)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<cardSlotsSeq> slotSeqDtls = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(5);

                    parameters[0] = dbManager.GetParameter("pSpecName", DbType.String, specName, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pSpecDesc", DbType.String, specDesc, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pSpecRevName", DbType.String, revName, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pSpecStatus", DbType.String, status, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_slts_seq", parameters);

                    while (reader.Read())
                    {
                        if (slotSeqDtls == null)
                            slotSeqDtls = new List<cardSlotsSeq>();

                        cardSlotsSeq slotSeqObj = new cardSlotsSeq();

                        slotSeqObj.SLOT_SPECN_ID = reader["SLOT_SPECN_ID"].ToString();
                        slotSeqObj.SLOT_SPECN_NM = reader["SLOT_SPECN_NM"].ToString();
                        slotSeqObj.SLOT_DSC = reader["SLOT_DSC"].ToString();
                        slotSeqObj.SLOT_SPECN_REVSN_NM = reader["SLOT_SPECN_REVSN_NM"].ToString();
                        slotSeqObj.SLOT_SPECN_DIMS = reader["SLOT_SPECN_DIMS"].ToString() == " x  x " ? "" : reader["SLOT_SPECN_DIMS"].ToString();
                        slotSeqObj.SLOT_SPECN_STATUS = reader["SLOT_SPECN_STATUS"].ToString();

                        slotSeqObj.Status = status;

                        slotSeqDtls.Add(slotSeqObj);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card Slot sequence list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card slot sequence list");
                    hadException = true;
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

            return slotSeqDtls;
        }

        public class cardPortTypeList
        {
            public string PortTypeId { get; set; }
            public string PortType { get; set; }
            public string PortRole { get; set; }
            public string Config { get; set; }
            public string Description { get; set; }
        }


        public async Task<List<cardPortTypeList>> GetCardSPortTypeList(string portType, string portRole, string description,
                                                                       string dualPort, string Transrate, string AssignPorts)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<cardPortTypeList> portTypeDtls = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(7);

                    parameters[0] = dbManager.GetParameter("pPortType", DbType.String, portType, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pRoleType", DbType.String, portRole, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pDescription", DbType.String, description, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pDualport", DbType.String, dualPort, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pTransRate", DbType.String, Transrate, ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pAssignPort", DbType.String, AssignPorts, ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_ports_type", parameters);

                    while (reader.Read())
                    {
                        if (portTypeDtls == null)
                            portTypeDtls = new List<cardPortTypeList>();

                        cardPortTypeList portTypeObj = new cardPortTypeList();
                        portTypeObj.PortTypeId = reader["id"].ToString();
                        portTypeObj.PortType = reader["portType"].ToString();
                        portTypeObj.PortRole = reader["portRole"].ToString();
                        portTypeObj.Config = reader["config"].ToString();
                        portTypeObj.Description = reader["Descr"].ToString();
                        portTypeDtls.Add(portTypeObj);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card port type list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card port type list");
                    hadException = true;
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

            return portTypeDtls;
        }

        public class cardCnctrTypeList
        {
            public int cnctrTypeId { get; set; }
            public string cnctrType { get; set; }
            public string cnctrAlias { get; set; }
            public string cnctrBidir { get; set; }
            public string cnctrVisible { get; set; }
            public string cnctrFEML { get; set; }
            public string cnctrAllowedBay { get; set; }
        }


        public async Task<List<cardCnctrTypeList>> GetCardSCnctrTypeList(string cnctrName, string cnctrAliases, string cnctrDesc)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<cardCnctrTypeList> cnctrTypeDtls = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(4);

                    parameters[0] = dbManager.GetParameter("pCnctrName", DbType.String, cnctrName, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pAliasName", DbType.String, cnctrAliases, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pDescription", DbType.String, cnctrDesc, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_ports_cnctrType", parameters);

                    while (reader.Read())
                    {
                        if (cnctrTypeDtls == null)
                            cnctrTypeDtls = new List<cardCnctrTypeList>();

                        cardCnctrTypeList cnctrTypeObj = new cardCnctrTypeList();
                        cnctrTypeObj.cnctrTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "cnctr_typ_id", true));
                        cnctrTypeObj.cnctrType = reader["cnctr_typ_cd"].ToString();
                        cnctrTypeObj.cnctrAlias = reader["aliases"].ToString();
                        cnctrTypeObj.cnctrBidir = reader["bi_drctnl_allow_ind"].ToString();
                        cnctrTypeObj.cnctrVisible = reader["vsbl_ind"].ToString();
                        cnctrTypeObj.cnctrFEML = reader["feml_ml_ind"].ToString();
                        cnctrTypeObj.cnctrAllowedBay = reader["port_allow_on_bay_ind"].ToString();
                        cnctrTypeDtls.Add(cnctrTypeObj);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card Port connector list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card Port connector list");
                    hadException = true;
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

            return cnctrTypeDtls;
        }

        public async Task<string> updateCardPortDtls(long portDefId, long portTypeId, long portCnctrTypId, string TypeName)
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

                    parameters = dbManager.GetParameterArray(5);

                    parameters[0] = dbManager.GetParameter("pPortDefId", DbType.Int64, portDefId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPortTypId", DbType.Int64, portTypeId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pCnctrTypId", DbType.Int64, portCnctrTypId, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pUpdateType", DbType.String, TypeName, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.upd_card_specn_with_ports_def", parameters);

                    response = parameters[4].Value.ToString();

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card port list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card port list");
                    hadException = true;
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

        //-- AB48512 Port Configure changes starts--
        // Added code by
        //Author : Abdulla Punnonam Kandy
        //Date : 20/02/2019
        //Description : Card Port's Details
        public class CardPortConfiguration
        {
            public string _guid { get; set; }
            public int CardPortDetailsId { get; set; }
            public int CardPortDefId { get; set; }
            public int PortNo { get; set; }
            public int Offset { get; set; }
            public string Label { get; set; }
            public string EQDES { get; set; }
            public string HorzDisp { get; set; }
            public int RotationAngleId { get; set; }
            public string FrontRearInd { get; set; }
            public bool _isDeleted { get; set; }
        }
        // Added code by
        //Author : Abdulla Punnonam Kandy
        //Date : 20/02/2019
        //Description : Get Card Port's Details
        public async Task<cardPortsDtls> GetCardPortSeqDetails(int portSpecId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            cardPortsDtls partsDtls = new cardPortsDtls();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pPortDefId", DbType.Int64, portSpecId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_prts_wth_seq_dtls", parameters);

                    while (reader.Read())
                    {
                        partsDtls.portDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "portDefId", true));
                        partsDtls.sequenceNum = int.Parse(DataReaderHelper.GetNonNullValue(reader, "sequenceNo", true));
                        partsDtls.portType = reader["porttype"].ToString();
                        partsDtls.connector = reader["connector"].ToString();
                        partsDtls.namingConv = reader["namingconvention"].ToString();
                        partsDtls.portCount = int.Parse(DataReaderHelper.GetNonNullValue(reader, "portcount", true));
                        partsDtls.assignablePort = reader["assignableport"].ToString();
                        partsDtls.startingPortNo = int.Parse(DataReaderHelper.GetNonNullValue(reader, "startingPortNo", true));
                        partsDtls.portOffsetVal = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PortOffsetVal", true));

                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card Role list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card Role list");
                    hadException = true;
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

            return partsDtls;
        }
        public async Task<List<CardPortConfiguration>> GetCardPortConfigList(int cardPortDefId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            List<CardPortConfiguration> cardPortDetailsList = new List<CardPortConfiguration>();
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pPortDefId", DbType.Int64, cardPortDefId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("card_specn_pkg.get_card_specn_with_prts_dtls", parameters);

                    while (reader.Read())
                    {
                        CardPortConfiguration cardPortDtlsObj = new CardPortConfiguration();
                        cardPortDtlsObj.CardPortDetailsId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_WITH_PRTS_PRTS_ID", true));
                        cardPortDtlsObj.CardPortDefId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "CARD_SPECN_WITH_PRTS_DEF_ID", true));
                        cardPortDtlsObj.PortNo = int.Parse(DataReaderHelper.GetNonNullValue(reader, "PORT_NO", true));
                        //cardPortDtlsObj.Offset = int.Parse(DataReaderHelper.GetNonNullValue(reader, "OFFSET", true));
                        cardPortDtlsObj.HorzDisp = reader["X_COORD_NO"].ToString();  //int.Parse(DataReaderHelper.GetNonNullValue(reader, "X_COORD_NO", true));
                        cardPortDtlsObj.EQDES = reader["Y_COORD_NO"].ToString();  //int.Parse(DataReaderHelper.GetNonNullValue(reader, "Y_COORD_NO", true));                        
                        cardPortDtlsObj.Label = reader["LABEL_NM"].ToString();
                        cardPortDtlsObj.FrontRearInd = reader["FRNT_RER_IND"].ToString();
                        cardPortDtlsObj.RotationAngleId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ROTTN_ANGL_ID", true));
                        cardPortDtlsObj._guid = Guid.NewGuid().ToString();
                        cardPortDtlsObj._isDeleted = false;
                        cardPortDetailsList.Add(cardPortDtlsObj);
                    }

                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Card Port Conifgurations list");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Card Port Conifgurations list");
                    hadException = true;
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

            return cardPortDetailsList;
        }
        public async Task<string> SaveCardPortConfigurations(List<CardPortConfiguration> list)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string response = string.Empty;
            string procedurename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    foreach (var PortConfig in list)
                    {
                        string action = PortConfig.CardPortDetailsId == 0 ? "INSERT" : "UPDATE";
                        try
                        {
                            procedurename = PortConfig.CardPortDetailsId == 0 ? "CARD_SPECN_PKG.ins_card_specn_with_prts_prts" : "CARD_SPECN_PKG.upd_card_specn_with_prts_prts";
                            parameters = dbManager.GetParameterArray(9);
                            parameters[0] = dbManager.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int32, PortConfig.CardPortDetailsId, ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameter("pCardSpecnWithPrtsDefId", DbType.Int32, PortConfig.CardPortDefId, ParameterDirection.Input);
                            parameters[2] = dbManager.GetParameter("pPortNo", DbType.Int32, PortConfig.PortNo, ParameterDirection.Input);
                            parameters[3] = dbManager.GetParameter("pXCoordNo", DbType.Int32, int.Parse(PortConfig.HorzDisp), ParameterDirection.Input);
                            parameters[4] = dbManager.GetParameter("pYCoordNo", DbType.Int32, int.Parse(PortConfig.EQDES), ParameterDirection.Input);
                            parameters[5] = dbManager.GetParameter("pLabelNm", DbType.String, PortConfig.Label, ParameterDirection.Input);
                            parameters[6] = dbManager.GetParameter("pRottnAnglId", DbType.Int32, PortConfig.RotationAngleId, ParameterDirection.Input);
                            parameters[7] = dbManager.GetParameter("pFrontRearInd", DbType.String, PortConfig.FrontRearInd, ParameterDirection.Input);
                            parameters[8] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, procedurename, parameters);

                            string stat = parameters[8].Value.ToString();
                            response = response + PortConfig._guid + ":" + action + ":" + stat + ";";
                        }
                        catch (Exception e)
                        {
                            response = response + PortConfig._guid + ":" + action + ":" + "Failed:Database Error;";
                            logger.Error(e, PortConfig.ToString() + " - Unable to save Card port config details");
                        }
                    }
                }
                catch (OracleException oe)
                {
                    response = "Failed";
                    logger.Error(oe, "Unable to save Card port config details");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    response = "Failed";
                    logger.Error(ex, "Unable to save Card port config details");
                    hadException = true;
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
        public async Task<string> DeleteCardPortConfigurations(List<CardPortConfiguration> list)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string response = string.Empty;
            string procedurename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    procedurename = "CARD_SPECN_PKG.del_card_specn_with_prts_prts";
                    foreach (var PortConfig in list)
                    {
                        try
                        {
                            parameters = dbManager.GetParameterArray(2);

                            parameters[0] = dbManager.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int64, PortConfig.CardPortDetailsId, ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameter("response", DbType.String, "", ParameterDirection.Output, 250);

                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, procedurename, parameters);

                            string stat = parameters[1].Value.ToString();
                            response = response + PortConfig._guid + ":DELETE:" + stat + ";";
                        }
                        catch (Exception e)
                        {
                            response = response + PortConfig._guid + ":DELETE:" + "Failed:Database Error;";
                            logger.Error(e, PortConfig.ToString() + " - Unable to save Card port config details");
                        }
                    }
                }
                catch (OracleException oe)
                {
                    response = "Failed";
                    logger.Error(oe, "Unable to DELETE Card port config details");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    response = "Failed";
                    logger.Error(ex, "Unable to DELETE Card port config details");
                    hadException = true;
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
        //-- AB48512 Port Configure changes ends--
    }
}