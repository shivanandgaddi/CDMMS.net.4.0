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
    public class ShelfSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ShelfSpecificationDbInterface() : base()
        {
        }

        public ShelfSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification shelf = null;
            ShelfDbInterface shelfDbInterface = null;
            MaterialDbInterface materialDbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("shelf_specn_pkg.get_shelf_specn", parameters);

                if (reader.Read())
                {
                    string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");

                    shelf = new ShelfSpecification(specificationId);

                    shelf.Name = DataReaderHelper.GetNonNullValue(reader, "shelf_specn_nm");
                    shelf.Description = DataReaderHelper.GetNonNullValue(reader, "shelf_specn_dsc");
                    shelf.UseType = DataReaderHelper.GetNonNullValue(reader, "use_typ");
                    shelf.NDSSpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "nds_id", true));

                    ((ShelfSpecification)shelf).ObjectOrientationId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "obj_orntn_id", true));
                    ((ShelfSpecification)shelf).StraightThroughIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((ShelfSpecification)shelf).MidPlaneIndicator = DataReaderHelper.GetNonNullValue(reader, "mid_pln_ind");
                    ((ShelfSpecification)shelf).NodeLevelMaterialIndicator = DataReaderHelper.GetNonNullValue(reader, "node_lvl_mtrl_ind");
                    ((ShelfSpecification)shelf).LabelName = DataReaderHelper.GetNonNullValue(reader, "label_nm");
                    ((ShelfSpecification)shelf).LabelPositionId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "label_pos_id", true));
                    ((ShelfSpecification)shelf).SlotsPerRowQuantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "slts_per_row_qty", true));
                    ((ShelfSpecification)shelf).StartingSlotNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "strtg_slot_no", true));
                    ((ShelfSpecification)shelf).NDSManufacturer = DataReaderHelper.GetNonNullValue(reader, "nds_mfr");
                    ((ShelfSpecification)shelf).ShelfNDSUseTyp = DataReaderHelper.GetNonNullValue(reader, "use_typ");

                    if ("N".Equals(genericIndicator))
                    {
                        long mtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        long materialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rme_shelf_mtrl_revsn_id", true));

                        shelf.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                        shelf.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                        shelf.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;

                        ((ShelfSpecification)shelf).IsGeneric = false;
                        ((ShelfSpecification)shelf).RevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_specn_revsn_alt_id", true));
                        ((ShelfSpecification)shelf).RevisionName = DataReaderHelper.GetNonNullValue(reader, "shelf_specn_revsn_nm");
                        ((ShelfSpecification)shelf).IsRecordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind") == "Y" ? true : false;

                        if (mtrlId > 0 && materialItemId > 0)
                        {
                            MaterialItem shelfMaterial = null;

                            shelfDbInterface = new ShelfDbInterface();
                            materialDbInterface = new MaterialDbInterface();

                            ((ShelfSpecification)shelf).AssociatedMaterial = shelfDbInterface.GetMaterial(materialItemId, mtrlId);

                            Task t = Task.Run(async () =>
                            {
                                shelfMaterial = await materialDbInterface.GetMaterialItemSAPAsync(materialItemId);
                            });

                            t.Wait();

                            if (shelfMaterial != null)
                            {
                                if (shelfMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                                    shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, shelfMaterial.Attributes[MaterialType.JSON.ItmDesc]);
                                else
                                    shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));

                                if (shelfMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                    shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, shelfMaterial.Attributes[MaterialType.JSON.PrtNo]);
                                else
                                    shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                            else
                            {
                                shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));
                                shelf.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                        }
                    }
                    else
                    {
                        shelf.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "gnrc_cmplt_ind") == "Y" ? true : false;
                        shelf.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "gnrc_prpgt_ind") == "Y" ? true : false;
                        shelf.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "gnrc_del_ind") == "Y" ? true : false;

                        ((ShelfSpecification)shelf).IsGeneric = true;
                        ((ShelfSpecification)shelf).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                        ((ShelfSpecification)shelf).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                        ((ShelfSpecification)shelf).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                        ((ShelfSpecification)shelf).DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    }

                    //if (bayInternalId > 0)
                    //{
                    //    bayInternalDbInterface = new BayInternalDbInterface();

                    //    ((BaySpecification)bay).BayInternal = bayInternalDbInterface.GetSpecification(bayInternalId);
                    //}
                    ((ShelfSpecification)shelf).ShelfUseTypId = GetUseType("Shelf", ((ShelfSpecification)shelf).RoleList);
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

            return shelf;
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

                reader = dbManager.ExecuteDataReaderSP("shelf_specn_pkg.get_shelf_specn_role_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_role_typ_id", true));
                    SpecificationRole shelfRole = new SpecificationRole(specificationId);

                    shelfRole.IsSelected = DataReaderHelper.GetNonNullValue(reader, "is_selected") == "Y" ? true : false;
                    shelfRole.PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_role_typ_prty_no", true));
                    shelfRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "shelf_role_typ");
                    shelfRole.RMEUseType = DataReaderHelper.GetNonNullValue(reader, "rme_use_typ");

                    if (roles == null)
                        roles = new Dictionary<long, SpecificationRole>();

                    roles.Add(shelfRole.SpecificationId, shelfRole);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Shelf Role list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Shelf Role list");
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

        public Dictionary<long, SpecificationRole> GetUseTypeList(long shelfSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, SpecificationRole> useTypes = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, 2, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("shelf_specn_pkg.get_shelf_specn_use_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "specn_record_use_typ_id", true));
                    SpecificationRole shelfRole = new SpecificationRole(specificationId);

                    shelfRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "use_typ");

                    if (useTypes == null)
                        useTypes = new Dictionary<long, SpecificationRole>();

                    useTypes.Add(shelfRole.SpecificationId, shelfRole);
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

        public void UpdateShelfSpecification(long specificationId, string name, string description, bool generic, string straightThrough, string midPlan,
            string nodeLevelMaterial, decimal startgSlotNo, int slotsRowQuantity, int orientationId, string labelName, int labelPositionId, string useType)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pGnrc", DbType.String, generic ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pStrghtThru", DbType.String, straightThrough, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMidPln", DbType.String, midPlan, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pNodeLvlInd", DbType.String, nodeLevelMaterial, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pStrtgSltNo", DbType.Decimal, startgSlotNo, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pOrntn", DbType.Int64, orientationId, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pSltsQty", DbType.Int64, CheckNullValue(slotsRowQuantity), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pLblNm", DbType.String, CheckNullValue(labelName), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pLblPosId", DbType.Int64, CheckNullValue(labelPositionId), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pUseTyp", DbType.String, useType, ParameterDirection.Input);

                //PROCEDURE update_shelf_specn (pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2, pGnrc IN VARCHAR2, pStrghtThru IN VARCHAR2,
                //pMidPln IN VARCHAR2, pNodeLvlInd IN VARCHAR2, pStrtgSltNo IN NUMBER, pOrntn IN NUMBER, pSltsQty IN NUMBER,
                //pLblNm IN VARCHAR2, pLblPosId IN NUMBER);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.update_shelf_specn", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update shelf specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                    specificationId, name, description, generic, straightThrough, midPlan, nodeLevelMaterial, startgSlotNo, slotsRowQuantity, orientationId, labelName, labelPositionId, useType);

                throw ex;
            }
        }

        public void UpdateGenericShelfSpecification(long specificationId, bool completed, bool propagated, bool deleted, decimal depth, decimal height, decimal width, int uom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDel", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pUom", DbType.Int32, uom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.update_shelf_specn_gnrc", parameters);

                //PROCEDURE update_shelf_specn_gnrc (pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2,
                //pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update shelf_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, completed, propagated, deleted, depth, height, width, uom);

                throw ex;
            }
        }

        public void UpdateShelfSpecificationRevision(long revisionAltId, long specificationId, string name, bool completed, bool propagated, bool deleted, bool RO)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(7);
                // table shelf_specn_revsn_alt
                // column shelf_specn_revsn_alt_id,shelf_specn_revsn_nm,rcrds_only_ind,cmplt_ind,prpgt_ind,del_ind,

                parameters[0] = dbAccessor.GetParameter("pAltId", DbType.Int64, revisionAltId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRo", DbType.String, RO ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pDel", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.update_shelf_specn_revsn_alt", parameters);
                // PROCEDURE update_shelf_specn_revsn_alt (pAltId IN number, pSpecnId in number, pNm IN VARCHAR2, pRo IN VARCHAR2,
                //pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update shelf_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5},{6})", revisionAltId, specificationId, name, completed, propagated, deleted, RO);
                throw ex;
            }
        }

        public void DeleteShelfSpecificationRole(long specificationId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.delete_shelf_specn_role", parameters);
                //PROCEDURE delete_shelf_specn_role(pId IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete delete_shelf_specn_role ({0})", specificationId);
                throw ex;
            }
        }

        public void InsertShelfSpecificationRole(long specificationId, int roleTypeId, int prtyNo)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int32, roleTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrtyNo", DbType.Int32, prtyNo, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.insert_shelf_specn_role", parameters);
                //PROCEDURE insert_shelf_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert insert_shelf_specn_role ({0}, {1})", specificationId, roleTypeId);

                throw ex;
            }
        }

        public long CreateShelfSpecification(string name, string description, string genericIndicator, string straightThruIndictor, string midPlaneIndicator,
            string nodeLevelMaterial, decimal startgSlotNo, int orientationId, int slotsRowQuantity, string labelName, int labelPositionId, string useType)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);

                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pGnrc", DbType.String, genericIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pStrghtThru", DbType.String, straightThruIndictor, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pMidPln", DbType.String, midPlaneIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pNodeLvlInd", DbType.String, nodeLevelMaterial, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pStrtgSltNo", DbType.Decimal, startgSlotNo, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pOrntn", DbType.Int64, orientationId, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pSltsQty", DbType.Int64, CheckNullValue(slotsRowQuantity), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pLblNm", DbType.String, CheckNullValue(labelName), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pLblPosId", DbType.Int64, CheckNullValue(labelPositionId), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pUseTyp", DbType.String, useType, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.insert_shelf_specn", parameters);
                //PROCEDURE insert_shelf_specn (pNm IN VARCHAR2, pDsc IN VARCHAR2, pGnrc IN VARCHAR2, pStrghtThru IN VARCHAR2,
                //pMidPln IN VARCHAR2, pNodeLvlInd IN VARCHAR2, pStrtgSltNo IN NUMBER, pOrntn IN NUMBER, pSltsQty IN NUMBER,
                //pLblNm IN VARCHAR2, pLblPosId IN NUMBER, oSpecnId OUT NUMBER);

                specificationId = long.Parse(parameters[12].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_shelf_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})", name, description, genericIndicator, straightThruIndictor, midPlaneIndicator,
                    nodeLevelMaterial, startgSlotNo, orientationId, slotsRowQuantity, labelName, labelPositionId);

                throw ex;
            }

            return specificationId;
        }

        public void CreateGenericShelfSpecification(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, decimal depth, decimal height, decimal width, int uom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmplt", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDel", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pUom", DbType.Int32, uom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.insert_shelf_specn_gnrc", parameters);
                //PROCEDURE insert_shelf_specn_gnrc (pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2,
                //pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_shelf_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, completionIndicator, propagationIndicator, deletionIndicator, depth, height, width, uom);
                throw ex;
            }
        }

        public long CreateShelfSpecificationRevision(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, decimal weight, int weightUom, string roIndicator, string name)
        {
            IDbDataParameter[] parameters = null;
            long revisionId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, CheckNullValue(name), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRo", DbType.String, roIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCmplt", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pDel", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("oRevsnAltId", DbType.Int64, revisionId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.insert_shelf_specn_revsn_alt", parameters);


                revisionId = long.Parse(parameters[6].Value.ToString());
                //PROCEDURE insert_shelf_specn_revsn_alt (pId IN number, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2,
                //    pPrpgt IN VARCHAR2, pDel IN VARCHAR2, oRevsnAltId OUT NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_shelf_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5})",
                    specificationId, name, roIndicator, completionIndicator, propagationIndicator, deletionIndicator);

                throw ex;
            }

            return revisionId;
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.associate_material", parameters);

                //PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to associate material ({0}, {1})", specificationRevisionId, materialItemId);

                throw ex;
            }
        }

        public bool UpdateShelfSpecificationMaterial(long materialItemId, decimal height, decimal depth, decimal width, int uomId, decimal normalCurrentDrain,
            int normalCurrentDrainUom, decimal maxCurrentDrain, int maxCurrentDrainUom, decimal shelfWeight, int shelfWeightUom, decimal heatDissipation, int heatDissipationUom)
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
                parameters[9] = dbAccessor.GetParameter("pWt", DbType.Decimal, CheckNullValue(shelfWeight), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pWtUom", DbType.Int32, CheckNullValue(shelfWeightUom), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pHt", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pHtUom", DbType.Int32, CheckNullValue(heatDissipationUom), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("didUpdate", DbType.String, output, ParameterDirection.Output, 1);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.update_shelf_material", parameters);

                output = parameters[13].Value.ToString();

                if ("Y".Equals(output))
                    didUpdate = true;

                //PROCEDURE update_shelf_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, didUpdate OUT VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update associated material ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})", materialItemId,
                    height, depth, width, uomId, normalCurrentDrain, normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, shelfWeight, shelfWeightUom, heatDissipation, heatDissipationUom);

                throw ex;
            }

            return didUpdate;
        }

        public void UpsertNDSSpecificationId(long specificationId, long ndsSpecificationId, bool isGeneric)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pSpecnRvsnId", DbType.Int64, CheckNullValue(specificationId), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pMtlItmId", DbType.Int64, ndsSpecificationId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pMtlItmId", DbType.String, isGeneric ? "Y" : "N", ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.insert_nds_spec_id", parameters);

                //PROCEDURE insert_nds_spec_id(pSpecRvsnId IN NUMBER, pNdsSpecId IN NUMBER, pGnrcInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update NDS specification id ({0}, {1}, {2})", specificationId, ndsSpecificationId, isGeneric);

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
                        sql = @"select val.alias_val from shelf_role_typ_alias_val val,shelf_role_typ_alias ali,shelf_role_typ typ
                        where val.shelf_role_typ_alias_id = ali.shelf_role_typ_alias_id 
                        and ali.alias_nm='RME Record Use Type' 
                        and val.shelf_role_typ_id=typ.shelf_role_typ_id
                        and typ.shelf_role_typ ='" + roleL.Value.RoleType + "'";


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
    }
}