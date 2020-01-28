using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class BayInternalDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BayInternalDbInterface() : base()
        {
        }

        public BayInternalDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification bayInternal = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_specn_pkg.get_bay_itnl", parameters);

                if (reader.Read())
                {
                    string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");
                    long bayInternalId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_id", true));

                    bayInternal = new BayInternalSpecification(specificationId);

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
                    SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                    ((BayInternalSpecification)bayInternal).BayIntlUseTypeId = dbInterface.GetUseType("Bay Internal");
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

            return bayInternal;
        }

        public async Task<Dictionary<long, BayInternalDepth>> GetInternalDepthsAsync()
        {
            Dictionary<long, BayInternalDepth> depths = null;

            await Task.Run(() =>
            {
                depths = GetInternalDepths();
            });

            return depths;
        }

        public Dictionary<long, BayInternalDepth> GetInternalDepths()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalDepth> depths = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_specn_pkg.get_all_bay_itnl_depth", parameters);

                while (reader.Read())
                {
                    BayInternalDepth depth = new BayInternalDepth();

                    depth.DepthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_dpth_id", true));
                    depth.Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    depth.DepthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    depth.DepthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (depths == null)
                        depths = new Dictionary<long, BayInternalDepth>();

                    depths.Add(depth.DepthId, depth);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Bay Internal Depths");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bay Internal Depths");
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

            return depths;
        }

        public string GetBayInternalNumberByID(string id, string choice)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string package = String.Empty;
            string returnNumber = String.Empty;

            try
            {
                switch (choice)
                {
                    case "D":
                        {
                            package = "bay_specn_pkg.get_itnl_dpth_by_id";
                            break;
                        }
                    case "W":
                        {
                            package = "bay_specn_pkg.get_itnl_wdth_by_id";
                            break;
                        }
                }
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pID", DbType.Int64, int.Parse(id), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP(package, parameters);

                while (reader.Read())
                {
                    returnNumber = reader["returnnumber"].ToString().ToUpper();
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Bay Internal Depths");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bay Internal Depths");
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

            return returnNumber;
        }

        public async Task<Dictionary<long, BayInternalWidth>> GetInternalWidthsAsync()
        {
            Dictionary<long, BayInternalWidth> widths = null;

            await Task.Run(() =>
            {
                widths = GetInternalWidths();
            });

            return widths;
        }

        public Dictionary<long, BayInternalWidth> GetInternalWidths()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalWidth> widths = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_specn_pkg.get_all_bay_itnl_width", parameters);

                while (reader.Read())
                {
                    BayInternalWidth width = new BayInternalWidth();

                    width.WidthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_itnl_wdth_id", true));
                    width.Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    width.WidthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    width.NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    width.WidthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (widths == null)
                        widths = new Dictionary<long, BayInternalWidth>();

                    widths.Add(width.WidthId, width);
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

            return widths;
        }

        public void UpdateBayInternalSpecification(long bayInternalId, string name, string description, int mountingPositionQty, int mountingPositionDistanceId, string wallMountIndicator, string straightThruIndictor,
            string midPlaneIndicator, string completedIndicator, string propagatedIndicator, string deletedIndicator, long depthId, long widthId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, bayInternalId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDpthId", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pWdthId", DbType.Int64, widthId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMntngPosQty", DbType.Int32, mountingPositionQty, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pMntngPosDistId", DbType.Int32, mountingPositionDistanceId, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pWllMntInd", DbType.String, wallMountIndicator, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThruIndictor, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pMidPlnInd", DbType.String, midPlaneIndicator, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completedIndicator, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagatedIndicator, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pDelInd", DbType.String, deletedIndicator, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.update_bay_itnl", parameters);

                //PROCEDURE update_bay_itnl(pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2, pDpthId IN NUMBER, pWdthId IN NUMBER,
                //pMntngPosQty IN NUMBER, pMntngPosDistId IN NUMBER, pWllMntInd IN VARCHAR2, pStrghtThruInd IN VARCHAR2,
                //pMidPlnInd IN VARCHAR2, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_itnl ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})", bayInternalId, name, description, mountingPositionQty, mountingPositionDistanceId, wallMountIndicator,
                    straightThruIndictor, midPlaneIndicator, completedIndicator, propagatedIndicator, deletedIndicator, depthId, widthId);

                throw ex;
            }
        }

        public long CreateBayInternalSpecification(string name, string description, int mountingPositionQty, int mountingPositionDistanceId, string wallMountIndicator, string straightThruIndictor,
            string midPlaneIndicator, string completedIndicator, string propagatedIndicator, string deletedIndicator, long depthId, long widthId)
        {
            IDbDataParameter[] parameters = null;
            long bayInternalId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);
                
                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpthId", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdthId", DbType.Int64, widthId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pMntngPosQty", DbType.Int32, mountingPositionQty, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMntngPosDistId", DbType.Int32, mountingPositionDistanceId, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWllMntInd", DbType.String, wallMountIndicator, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pStrghtThruInd", DbType.String, straightThruIndictor, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pMidPlnInd", DbType.String, midPlaneIndicator, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completedIndicator, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagatedIndicator, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pDelInd", DbType.String, deletedIndicator, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("oItnlId", DbType.Int64, bayInternalId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl", parameters);

                bayInternalId = long.Parse(parameters[12].Value.ToString());

                //PROCEDURE insert_bay_itnl(pNm IN VARCHAR2, pDsc IN VARCHAR2, pDpthId IN NUMBER, pWdthId IN NUMBER,
                //pMntngPosQty IN NUMBER, pMntngPosDistId IN NUMBER, pWllMntInd IN VARCHAR2, pStrghtThruInd IN VARCHAR2,
                //pMidPlnInd IN VARCHAR2, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2, oItnlId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert bay_itnl ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})", name, description, mountingPositionQty, mountingPositionDistanceId, wallMountIndicator,
                    straightThruIndictor, midPlaneIndicator, completedIndicator, propagatedIndicator, deletedIndicator, depthId, widthId);

                throw ex;
            }

            return bayInternalId;
        }

        public void UpsertNDSSpecificationId(long specificationId, long ndsSpecificationId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pSpecId", DbType.Int64, CheckNullValue(specificationId), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pNdsSpecId", DbType.Int64, ndsSpecificationId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_nds_spec_id", parameters);

                //PROCEDURE insert_bay_itnl_nds_spec_id(pSpecId IN NUMBER, pNdsSpecId IN NUMBER)
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

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }
    }
}