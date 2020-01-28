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
    public class BaySpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BaySpecificationDbInterface() : base()
        {
        }

        public BaySpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification bay = null;
            BayDbInterface bayDbInterface = null;
            MaterialDbInterface materialDbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_specn_pkg.get_bay_specn", parameters);

                if (reader.Read())
                {
                    string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");
                    long bayInternalId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_id", true));
                    int roleTypeId = 0;

                    int.TryParse(DataReaderHelper.GetNonNullValue(reader, "bay_role_typ_id", true), out roleTypeId);

                    bay = new BaySpecification(specificationId);

                    bay.Name = DataReaderHelper.GetNonNullValue(reader, "bay_specn_nm");
                    bay.Description = DataReaderHelper.GetNonNullValue(reader, "bay_specn_dsc");
                    bay.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    bay.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    bay.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    bay.NDSSpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "nds_id", true));
                    ((BaySpecification)bay).MountingPositionOffset = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "mntng_pos_ofst_no", true));
                    ((BaySpecification)bay).MountingPositionOffsetUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mntng_pos_ofst_uom_id", true));
                    ((BaySpecification)bay).LabelStartingPosition = DataReaderHelper.GetNonNullValue(reader, "strt_label_ind");
                    ((BaySpecification)bay).WallMountIndicator = DataReaderHelper.GetNonNullValue(reader, "wll_mnt_allow_ind");
                    ((BaySpecification)bay).StraightThruIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((BaySpecification)bay).MidPlaneIndicator = DataReaderHelper.GetNonNullValue(reader, "mid_pln_ind");
                    ((BaySpecification)bay).DualSDIndicator = DataReaderHelper.GetNonNullValue(reader, "dual_sd_ind");
                    ((BaySpecification)bay).RoleTypeId = roleTypeId;
                    ((BaySpecification)bay).BayInternalId = bayInternalId;
                    
                    if (((BaySpecification)bay).RoleTypeId!=0) {
                        SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                        ((BaySpecification)bay).BayUseTypeId = dbInterface.GetUseType("Bay",roleTypeId);
                    }
                    if ("N".Equals(genericIndicator))
                    {
                        long mtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        long materialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rme_bay_mtrl_revsn_id", true));

                        ((BaySpecification)bay).IsGeneric = false;
                        ((BaySpecification)bay).RevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_specn_revsn_alt_id", true));
                        ((BaySpecification)bay).RevisionName = DataReaderHelper.GetNonNullValue(reader, "bay_specn_revsn_nm");
                        ((BaySpecification)bay).MaxWeightCapacity = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "max_wt_cpcty_no", true));
                        ((BaySpecification)bay).MaxWeightCapacityUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "max_wt_cpcty_uom_id", true));
                        ((BaySpecification)bay).IsRecordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind") == "Y" ? true : false;

                        if (mtrlId > 0 && materialItemId > 0)
                        {
                            MaterialItem bayMaterial = null;

                            bayDbInterface = new BayDbInterface();
                            materialDbInterface = new MaterialDbInterface();

                            ((BaySpecification)bay).AssociatedMaterial = bayDbInterface.GetMaterial(materialItemId, mtrlId);

                            Task t = Task.Run(async () =>
                            {
                                bayMaterial = await materialDbInterface.GetMaterialItemSAPAsync(materialItemId);
                            });

                            t.Wait();

                            if (bayMaterial != null)
                            {
                                if (bayMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                                {
                                    bay.AssociatedMaterial.Attributes.Add("ItmDesc", bayMaterial.Attributes["ItmDesc"]);
                                }
                                else
                                    bay.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));

                                if (bayMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                    bay.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, bayMaterial.Attributes[MaterialType.JSON.PrtNo]);
                                else
                                    bay.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                            else
                            {
                                bay.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));
                                bay.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                        }
                    }
                    else
                    {
                        ((BaySpecification)bay).IsGeneric = true;
                        ((BaySpecification)bay).ExternalDepth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_dpth_no", true));
                        ((BaySpecification)bay).ExternalHeight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_hgt_no", true));
                        ((BaySpecification)bay).ExternalWidth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_wdth_no", true));
                        ((BaySpecification)bay).ExternalDimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_dim_uom_id", true));
                    }

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

            return bay;
        }

        public Dictionary<long, BayInternalSpecification> GetBayInternals()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalSpecification> internals = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_specn_pkg.get_all_bay_itnl", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_id", true));
                    BayInternalSpecification bayInternal = new BayInternalSpecification(specificationId, false);

                    bayInternal.Name = DataReaderHelper.GetNonNullValue(reader, "bay_itnl_nm");
                    bayInternal.Description = DataReaderHelper.GetNonNullValue(reader, "bay_itnl_dsc");
                    bayInternal.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    bayInternal.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    bayInternal.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    ((BayInternalSpecification)bayInternal).InternalDepthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_dpth_id", true));
                    ((BayInternalSpecification)bayInternal).InternalWidthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_wdth_id", true));
                    ((BayInternalSpecification)bayInternal).MountingPositionQuantity = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mntng_pos_qty"));
                    ((BayInternalSpecification)bayInternal).WallMountIndicator = DataReaderHelper.GetNonNullValue(reader, "wll_mnt_allow_ind");
                    ((BayInternalSpecification)bayInternal).StraightThruIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((BayInternalSpecification)bayInternal).MountingPositionDistanceId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mntng_pos_dist_id"));
                    ((BayInternalSpecification)bayInternal).MidPlaneIndicator = DataReaderHelper.GetNonNullValue(reader, "mid_pln_ind");
                    ((BayInternalSpecification)bayInternal).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    ((BayInternalSpecification)bayInternal).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    ((BayInternalSpecification)bayInternal).DepthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dpth_uom_str");
                    ((BayInternalSpecification)bayInternal).WidthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "wdth_uom_str");
                    ((BayInternalSpecification)bayInternal).MountingPositionDistance = DataReaderHelper.GetNonNullValue(reader, "mpd_uom");

                    if (internals == null)
                        internals = new Dictionary<long, BayInternalSpecification>();

                    internals.Add(bayInternal.SpecificationId, bayInternal);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Bay Internal Widths");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bay Internal Widths");
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

            return internals;
        }

        public void UpdateBaySpecification(long specificationId, string name, string description, decimal mountingPostOffset, int mountingPostOffsetUom, string startingLabel, string wallMountIndicator, string straightThruIndictor,
            string midPlaneIndicator, long bayInternalId, string dualSDIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(11);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pMntngPosOfst", DbType.Decimal, mountingPostOffset, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pMntngPosOfstUom", DbType.Int32, mountingPostOffsetUom, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pStrtLblInd", DbType.String, startingLabel, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWllMntInd", DbType.String, wallMountIndicator, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThruIndictor, ParameterDirection.Input);
                //parameters[8] = dbAccessor.GetParameter("pModelNo", DbType.String, CheckNullValue(modelNumber), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pMidPlnInd", DbType.String, midPlaneIndicator, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pBayItnlId", DbType.Int64, CheckNullValue(bayInternalId), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pDualSdInd", DbType.String, dualSDIndicator, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_specn", parameters);

                //PROCEDURE update_bay_specn(pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2, pMntngPosOfst IN NUMBER,
                //pMntngPosOfstUom IN NUMBER, pStrtLblInd IN VARCHAR2, pWllMntInd IN VARCHAR2, pStrghtThruInd IN VARCHAR2,
                //pMidPlnInd IN VARCHAR2, pBayItnlId IN VARCHAR2, pDualSdInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})", specificationId, name, description, mountingPostOffset, mountingPostOffsetUom, startingLabel,
                    wallMountIndicator, straightThruIndictor, midPlaneIndicator, bayInternalId, dualSDIndicator);

                throw ex;
            }
        }

        public void UpdateGenericBaySpecification(long specificationId, bool completed, bool propagated, bool deleted, decimal depth, decimal height, decimal width, int uom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDelInd", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pDimUomId", DbType.Int32, uom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_specn_gnrc", parameters);

                //PROCEDURE update_bay_specn_gnrc(pId IN NUMBER, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2,
                //pDpth IN NUMBER, pHgt IN NUMBER, pWdth IN NUMBER, pDimUomId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, completed, propagated, deleted, depth, height, width, uom);

                throw ex;
            }
        }

        public void UpdateBaySpecificationRevision(long revisionAltId, string name, bool completed, bool propagated, bool deleted, decimal maxWeight, int uom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pRevsnAltId", DbType.Int64, revisionAltId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDelInd", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMxWt", DbType.Decimal, CheckNullValue(maxWeight), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pMxWtUom", DbType.Int32, CheckNullValue(uom), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_specn_revsn_alt", parameters);

                //PROCEDURE update_bay_specn_revsn_alt(pRevsnAltId IN NUMBER, pNm IN VARCHAR2, pCmpltInd IN VARCHAR2,
                //pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2, pMxWt IN NUMBER, pMxWtUom IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5}, {6})", revisionAltId, name, completed, propagated, deleted, maxWeight, uom);

                throw ex;
            }
        }

        public void UpdateBaySpecificationRole(long specificationId, int roleTypeId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int32, roleTypeId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_specn_role", parameters);

                //PROCEDURE update_bay_specn_role(pId IN NUMBER, pRlTypId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_specn_role ({0}, {1})", specificationId, roleTypeId);

                throw ex;
            }
        }

        public bool UpdateBaySpecificationMaterial(long materialItemId, decimal height, decimal depth, decimal width, int dimUomId, decimal plannedHeatGeneration, int plannedHeatGenerationUom, decimal normalCurrentDrain,
            int normalCurrentDrainUom, decimal maxCurrentDrain, int maxCurrentDrainUom, decimal bayWeight, int bayWeightUom, decimal heatDissipation, int heatDissipationUom)
        {
            IDbDataParameter[] parameters = null;
            bool didUpdate = false;
            string output = "";

            try
            {
                parameters = dbAccessor.GetParameterArray(16);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pHght", DbType.Decimal, height, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pUom", DbType.Int32, dimUomId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPlndHt", DbType.Decimal, CheckNullValue(plannedHeatGeneration), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPlndHtUom", DbType.Int32, CheckNullValue(plannedHeatGenerationUom), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pElcNrmnl", DbType.Decimal, CheckNullValue(normalCurrentDrain), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pElcNrmnlUom", DbType.Int32, CheckNullValue(normalCurrentDrainUom), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pElcMx", DbType.Decimal, CheckNullValue(maxCurrentDrain), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pElcMxUom", DbType.Int32, CheckNullValue(maxCurrentDrainUom), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pWt", DbType.Decimal, CheckNullValue(bayWeight), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pWtUom", DbType.Int32, CheckNullValue(bayWeightUom), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pHt", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pHtUom", DbType.Int32, CheckNullValue(heatDissipationUom), ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("didUpdate", DbType.String, output, ParameterDirection.Output, 1);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_material", parameters);

                output = parameters[15].Value.ToString();

                if ("Y".Equals(output))
                    didUpdate = true;

                //PROCEDURE update_bay_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, 
                //pUom IN NUMBER, pPlndHt IN NUMBER, pPlndHtUom IN NUMBER, pElcNrmnl IN NUMBER,
                //pElcNrmnlUom IN NUMBER, pElcMx IN NUMBER, pElcMxUom IN NUMBER, pWt IN NUMBER, pWtUom IN NUMBER, 
                //pHt IN NUMBER, pHtUom IN NUMBER, didUpdate OUT VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update associated material ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14})", materialItemId, height, depth, width, dimUomId, plannedHeatGeneration, 
                    plannedHeatGenerationUom, normalCurrentDrain, normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, bayWeight, bayWeightUom, heatDissipation, heatDissipationUom);

                throw ex;
            }

            return didUpdate;
        }

        public long CreateBaySpecification(string name, string description, decimal mountingPostOffset, int mountingPostOffsetUom, string startingLabel, string wallMountIndicator, string straightThruIndictor,
            string midPlaneIndicator, long bayInternalId, string dualSDIndicator, string genericIndicator)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(12);
                
                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMntngPosOfst", DbType.Decimal, mountingPostOffset, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pGnrcInd", DbType.String, genericIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pMntngPosOfstUom", DbType.Int32, mountingPostOffsetUom, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pStrtLblInd", DbType.String, CheckNullValue(startingLabel), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWllMntInd", DbType.String, CheckNullValue(wallMountIndicator), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, CheckNullValue(straightThruIndictor), ParameterDirection.Input);
                //parameters[8] = dbAccessor.GetParameter("pModelNo", DbType.String, CheckNullValue(modelNumber), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pMidPlnInd", DbType.String, CheckNullValue(midPlaneIndicator), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pBayItnlId", DbType.Int64, CheckNullValue(bayInternalId), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pDualSdInd", DbType.String, CheckNullValue(dualSDIndicator), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_specn", parameters);

                specificationId = long.Parse(parameters[11].Value.ToString());

                //PROCEDURE insert_bay_specn(pNm IN VARCHAR2, pDsc IN VARCHAR2, pMntngPosOfst IN NUMBER, pGnrcInd IN VARCHAR2,
                //pMntngPosOfstUom IN NUMBER, pStrtLblInd IN VARCHAR2, pWllMntInd IN VARCHAR2, pStrghtThruInd IN VARCHAR2,
                //pMidPlnInd IN VARCHAR2, pBayItnlId IN VARCHAR2, pDualSdInd IN VARCHAR2, oSpecnId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})", name, description, mountingPostOffset, genericIndicator, mountingPostOffsetUom, startingLabel,
                    wallMountIndicator, straightThruIndictor, midPlaneIndicator, bayInternalId, dualSDIndicator);

                throw ex;
            }

            return specificationId;
        }

        public void CreateGenericBaySpecification(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, decimal depth, decimal height, decimal width, int uom)
        {
            IDbDataParameter[] parameters = null;
            
            try
            {
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDelInd", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pDimUomId", DbType.Int32, uom, ParameterDirection.Input);                

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_specn_gnrc", parameters);

                //PROCEDURE insert_bay_specn_gnrc(pId IN number, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2,
                //pDpth IN NUMBER, pHgt IN NUMBER, pWdth IN NUMBER, pDimUomId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, completionIndicator, propagationIndicator, deletionIndicator, depth, height, width, uom);

                throw ex;
            }
        }

        public long CreateBaySpecificationRevision(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, decimal weight, int weightUom, string roIndicator, string name)
        {
            IDbDataParameter[] parameters = null;
            long revisionId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(9);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, CheckNullValue(name), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDelInd", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMxWt", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pMxWtUom", DbType.Int32, CheckNullValue(weightUom), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pRo", DbType.String, roIndicator, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("oRevsnAltId", DbType.Int64, revisionId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_specn_revsn_alt", parameters);

                revisionId = long.Parse(parameters[8].Value.ToString());

                //PROCEDURE insert_bay_specn_revsn_alt(pSpecnId IN NUMBER, pNm IN VARCHAR2, pCmpltInd IN VARCHAR2,
                //pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2, pMxWt IN NUMBER, pMxWtUom IN NUMBER, pRo IN VARCHAR2, oRevsnAltId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId, completionIndicator, propagationIndicator, deletionIndicator, weight, weightUom, roIndicator, name);

                throw ex;
            }

            return revisionId;
        }

        public void CreateBaySpecificationRole(long specificationId, long roleTypeId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int64, roleTypeId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_specn_role", parameters);

                //PROCEDURE insert_bay_specn_role(pSpecnId IN NUMBER, pRlTypId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_specn_role ({0}, {1})", specificationId, roleTypeId);

                throw ex;
            }
        }

        public void InsertBayInternalDepth(decimal depth, int uomId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_dpth", parameters);

                //PROCEDURE insert_bay_itnl_dpth(pDpth IN NUMBER, pUom IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_itnl_dpth ({0}, {1})", depth, uomId);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public void InsertBayInternalWidth(decimal width, int uomId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_wdth", parameters);

                //PROCEDURE insert_bay_itnl_wdth(pWdth IN NUMBER, pUom IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_itnl_wdth ({0}, {1})", width, uomId);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.associate_material", parameters);

                //PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to associate material ({0}, {1})", specificationRevisionId, materialItemId);

                throw ex;
            }
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

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_nds_spec_id", parameters);

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
    }
}