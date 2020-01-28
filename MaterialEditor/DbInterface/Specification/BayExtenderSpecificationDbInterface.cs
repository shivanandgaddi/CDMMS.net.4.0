using System;
using System.Collections.Generic;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class BayExtenderSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BayExtenderSpecificationDbInterface() : base()
        {
        }

        public BayExtenderSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification bayExtender = null;
            BayExtenderDbInterface bayDbInterface = null;
            MaterialDbInterface materialDbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_bay_extndr_specn", parameters);

                if (reader.Read())
                {
                    string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");

                    bayExtender = new BayExtenderSpecification(specificationId);

                    bayExtender.Name = DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_nm");
                    bayExtender.Description = DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_dsc");
                    bayExtender.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    bayExtender.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    bayExtender.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    bayExtender.NDSSpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "nds_id", true));

                    if ("N".Equals(genericIndicator))
                    {
                        long mtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        long materialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rme_bay_extndr_mtrl_revsn_id", true));

                        ((BayExtenderSpecification)bayExtender).IsGeneric = false;
                        ((BayExtenderSpecification)bayExtender).RevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_alt_id", true));
                        ((BayExtenderSpecification)bayExtender).RevisionName = DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_nm");
                        ((BayExtenderSpecification)bayExtender).IsRecordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind") == "Y" ? true : false;

                        if (mtrlId > 0 && materialItemId > 0)
                        {
                            MaterialItem bayExtenderMaterial = null;

                            bayDbInterface = new BayExtenderDbInterface();
                            materialDbInterface = new MaterialDbInterface();

                            ((BayExtenderSpecification)bayExtender).AssociatedMaterial = bayDbInterface.GetMaterial(materialItemId, mtrlId);

                            ((BayExtenderSpecification)bayExtender).BayInternalDepthId = ((BayExtender)((BayExtenderSpecification)bayExtender).AssociatedMaterial).DepthId;
                            ((BayExtenderSpecification)bayExtender).BayInternalHeightId = ((BayExtender)((BayExtenderSpecification)bayExtender).AssociatedMaterial).HeightId;
                            ((BayExtenderSpecification)bayExtender).BayInternalWidthId = ((BayExtender)((BayExtenderSpecification)bayExtender).AssociatedMaterial).WidthId;

                            Task t = Task.Run(async () =>
                            {
                                bayExtenderMaterial = await materialDbInterface.GetMaterialItemSAPAsync(materialItemId);
                            });

                            t.Wait();

                            if (bayExtenderMaterial != null)
                            {
                                if (bayExtenderMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                                    bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, bayExtenderMaterial.Attributes[MaterialType.JSON.ItmDesc]);
                                else
                                    bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));

                                if (bayExtenderMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                    bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, bayExtenderMaterial.Attributes[MaterialType.JSON.PrtNo]);
                                else
                                    bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                            else
                            {
                                bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));
                                bayExtender.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                        }
                    }
                    else
                    {
                        ((BayExtenderSpecification)bayExtender).IsGeneric = true;
                        ((BayExtenderSpecification)bayExtender).BayInternalDepthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dpth_id", true));
                        ((BayExtenderSpecification)bayExtender).BayInternalHeightId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_hgt_id", true));
                        ((BayExtenderSpecification)bayExtender).BayInternalWidthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_wdth_id", true));
                    }
                    
                    SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                    ((BayExtenderSpecification)bayExtender).BayXtnUseTypeId = dbInterface.GetUseType("Bay Extender");

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

            return bayExtender;
        }

        public void InsertBayExtenderDepth(decimal depth, int uomId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extnd_dpth", parameters);


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
      

        public async Task<Dictionary<long, BayInternalHeight>> GetExtndInternalHeightAsync()
        {
            Dictionary<long, BayInternalHeight> height = null;

            await Task.Run(() =>
            {
                height = GetBayInternalHeights();
            });

            return height;
        }
        public Dictionary<long, BayInternalHeight> GetBayInternalHeights()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalHeight> heights = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_hgt", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_hgt_id", true));
                    BayInternalHeight bayInternalHeight = new BayInternalHeight(specificationId);

                    bayInternalHeight.SpecificationId = specificationId;

                    ((BayInternalHeight)bayInternalHeight).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_hgt_no", true));
                    ((BayInternalHeight)bayInternalHeight).HeightUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalHeight)bayInternalHeight).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalHeight)bayInternalHeight).HeightUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (heights == null)
                        heights = new Dictionary<long, BayInternalHeight>();

                    heights.Add(bayInternalHeight.SpecificationId, bayInternalHeight);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Bay Internal Heights");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bay Internal Heights");
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

            return heights;
        }

        public async Task<Dictionary<long, BayInternalWidth>> GetExtndInternalWidthAsync()
        {
            Dictionary<long, BayInternalWidth> width = null;

            await Task.Run(() =>
            {
                width = GetBayInternalWidths();
            });

            return width;
        }
        public Dictionary<long, BayInternalWidth> GetBayInternalWidths()
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

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_wdth", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_wdth_id", true));
                    BayInternalWidth bayInternalWidth = new BayInternalWidth(specificationId);

                    bayInternalWidth.SpecificationId = specificationId;

                    ((BayInternalWidth)bayInternalWidth).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_wdth_no", true));
                    ((BayInternalWidth)bayInternalWidth).WidthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalWidth)bayInternalWidth).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalWidth)bayInternalWidth).WidthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (widths == null)
                        widths = new Dictionary<long, BayInternalWidth>();

                    widths.Add(bayInternalWidth.SpecificationId, bayInternalWidth);
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
        public async Task<Dictionary<long, BayInternalDepth>> GetExtndInternalDepthAsync()
        {
            Dictionary<long, BayInternalDepth> depths = null;

            await Task.Run(() =>
            {
                depths = GetBayInternalDepths();
            });

            return depths;
        }
        public Dictionary<long, BayInternalDepth> GetBayInternalDepths()
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

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_dpth", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_dpth_id", true));
                    BayInternalDepth bayInternalDepth = new BayInternalDepth(specificationId);

                    bayInternalDepth.SpecificationId = specificationId;

                    ((BayInternalDepth)bayInternalDepth).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dpth_no", true));
                    ((BayInternalDepth)bayInternalDepth).DepthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalDepth)bayInternalDepth).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalDepth)bayInternalDepth).DepthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (depths == null)
                        depths = new Dictionary<long, BayInternalDepth>();

                    depths.Add(bayInternalDepth.SpecificationId, bayInternalDepth);
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
        public void InsertBayExtenderWidth(decimal width, int uomId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extnd_wdth", parameters);


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
        public void InsertBayExtenderHeight(decimal height, int uomId)
        {
            IDbDataParameter[] parameters = null;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extnd_hgt", parameters);


            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_itnl_wdth ({0}, {1})", height, uomId);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }       


        public Dictionary<long, BayInternalDepth> GetBayInternalDepth()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalDepth> internals = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_dpth", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_dpth_id", true));
                    BayInternalDepth bayInternalDepth = new BayInternalDepth(specificationId);

                    bayInternalDepth.SpecificationId = specificationId;

                    ((BayInternalDepth)bayInternalDepth).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dpth_no", true));
                    ((BayInternalDepth)bayInternalDepth).DepthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalDepth)bayInternalDepth).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalDepth)bayInternalDepth).DepthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (internals == null)
                        internals = new Dictionary<long, BayInternalDepth>();

                    internals.Add(bayInternalDepth.SpecificationId, bayInternalDepth);
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

            return internals;
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
                            package = "bay_extndr_specn_pkg.get_intl_dpth_by_id";
                            break;
                        }
                    case "W":
                        {
                            package = "bay_extndr_specn_pkg.get_intl_wdth_by_id";
                            break;
                        }
                    case "H":
                        {
                            package = "bay_extndr_specn_pkg.get_itnl_hgt_by_id";
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

        public Dictionary<long, BayInternalHeight> GetBayInternalHeight()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalHeight> internals = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_hgt", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_hgt_id", true));
                    BayInternalHeight bayInternalHeight = new BayInternalHeight(specificationId);

                    bayInternalHeight.SpecificationId = specificationId;

                    ((BayInternalHeight)bayInternalHeight).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_hgt_no", true));
                    ((BayInternalHeight)bayInternalHeight).HeightUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalHeight)bayInternalHeight).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalHeight)bayInternalHeight).HeightUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (internals == null)
                        internals = new Dictionary<long, BayInternalHeight>();

                    internals.Add(bayInternalHeight.SpecificationId, bayInternalHeight);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Bay Internal Heights");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bay Internal Heights");
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

        public Dictionary<long, BayInternalWidth> GetBayInternalWidth()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, BayInternalWidth> internals = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("bay_extndr_specn_pkg.get_all_bay_extndr_intl_wdth", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_wdth_id", true));
                    BayInternalWidth bayInternalWidth = new BayInternalWidth(specificationId);

                    bayInternalWidth.SpecificationId = specificationId;

                    ((BayInternalWidth)bayInternalWidth).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_wdth_no", true));
                    ((BayInternalWidth)bayInternalWidth).WidthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "itnl_dim_uom_id", true));
                    ((BayInternalWidth)bayInternalWidth).NominalIndicator = DataReaderHelper.GetNonNullValue(reader, "nmnl_ind");
                    ((BayInternalWidth)bayInternalWidth).WidthUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");

                    if (internals == null)
                        internals = new Dictionary<long, BayInternalWidth>();

                    internals.Add(bayInternalWidth.SpecificationId, bayInternalWidth);
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

        public void UpdateBayExtenderSpecification(long specificationId, string name, string description)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.update_bay_extndr_specn", parameters);

                //PROCEDURE update_bay_extndr_specn(pId IN NUMBER, pNm IN VARCHAR2, pDsc IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_extndr_specn ({0}, {1}, {2})", specificationId, name, description);

                throw ex;
            }
        }

        public void UpdateGenericBayExtenderSpecification(long specificationId, bool completed, bool propagated, bool deleted, long depthId, long heightId, long widthId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDelInd", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpth", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHgt", DbType.Int64, heightId, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdth", DbType.Int64, widthId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.update_bay_extndr_specn_gnrc", parameters);

                //PROCEDURE update_bay_extndr_specn_gnrc(pId IN NUMBER, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2,
                //pDelInd IN VARCHAR2, pDpthId IN NUMBER, pHghtId IN NUMBER, pWdthId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_extndr_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6})", specificationId, completed, propagated, deleted, depthId, heightId, widthId);

                throw ex;
            }
        }

        public void UpdateBayExtenderSpecificationRevision(long revisionAltId, string name, bool completed, bool propagated, bool deleted)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(5);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, revisionAltId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDelInd", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.update_bay_extndr_specn_revsn", parameters);

                //PROCEDURE update_bay_extndr_specn_revsn(pId IN NUMBER, pNm IN VARCHAR2, pCmpltInd IN VARCHAR2,
                //pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update bay_extndr_specn_revsn_alt ({0}, {1}, {2}, {3}, {4})", revisionAltId, name, completed, propagated, deleted);

                throw ex;
            }
        }
        //Bay extender
        //public void InsertBayExtenderDepth(decimal depth, int uomId)
        //{
        //    IDbDataParameter[] parameters = null;
        //    IAccessor dbManager = null;

        //    try
        //    {
        //        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
        //        parameters = dbManager.GetParameterArray(2);

        //        parameters[0] = dbManager.GetParameter("pDpth", DbType.Int64, depth, ParameterDirection.Input);
        //        parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

        //       // dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_dpth", parameters);

        //        //PROCEDURE insert_bay_itnl_dpth(pDpth IN NUMBER, pUom IN NUMBER)
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to create bay_itnl_dpth ({0}, {1})", depth, uomId);

        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (dbManager != null)
        //            dbManager.Dispose();
        //    }
        //}

        //public void InsertBayExtenderWidth(decimal width, int uomId)
        //{
        //    IDbDataParameter[] parameters = null;
        //    IAccessor dbManager = null;

        //    try
        //    {
        //        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
        //        parameters = dbManager.GetParameterArray(2);

        //        parameters[0] = dbManager.GetParameter("pWdth", DbType.Int64, width, ParameterDirection.Input);
        //        parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

        //       // dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_wdth", parameters);

        //        //PROCEDURE insert_bay_itnl_wdth(pWdth IN NUMBER, pUom IN NUMBER)
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to create bay_itnl_wdth ({0}, {1})", width, uomId);

        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (dbManager != null)
        //            dbManager.Dispose();
        //    }
        //}
        //public void InsertBayExtenderHeight(decimal height, int uomId)
        //{
        //    IDbDataParameter[] parameters = null;
        //    IAccessor dbManager = null;

        //    try
        //    {
        //        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
        //        parameters = dbManager.GetParameterArray(2);

        //        parameters[0] = dbManager.GetParameter("pWdth", DbType.Int64, width, ParameterDirection.Input);
        //        parameters[1] = dbManager.GetParameter("pUom", DbType.Int64, uomId, ParameterDirection.Input);

        //        // dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.insert_bay_itnl_wdth", parameters);

        //        //PROCEDURE insert_bay_itnl_wdth(pWdth IN NUMBER, pUom IN NUMBER)
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to create bay_itnl_wdth ({0}, {1})", width, uomId);

        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (dbManager != null)
        //            dbManager.Dispose();
        //    }
        //}

        //Bayextender
        public bool UpdateBayExtenderSpecificationMaterial(long materialItemId, long heightId, long depthId, long widthId)
        {
            IDbDataParameter[] parameters = null;
            bool didUpdate = false;
            string output = "";

            try
            {
                parameters = dbAccessor.GetParameterArray(5);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pHght", DbType.Int64, heightId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpth", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Int64, widthId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("didUpdate", DbType.String, output, ParameterDirection.Output, 1);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.update_bay_extndr_material", parameters);

                output = parameters[4].Value.ToString();

                if ("Y".Equals(output))
                    didUpdate = true;

                //PROCEDURE update_bay_extndr_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, didUpdate OUT VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update associated material ({0}, {1}, {2}, {3})", materialItemId, heightId, depthId, widthId);

                throw ex;
            }

            return didUpdate;
        }

        public long CreateBayExtenderSpecification(string name, string description, string genericIndicator)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pGnrcInd", DbType.String, genericIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extndr_specn", parameters);

                specificationId = long.Parse(parameters[3].Value.ToString());

                //PROCEDURE insert_bay_extndr_specn(pNm IN VARCHAR2, pDsc IN VARCHAR2, pGnrcInd IN VARCHAR2, oSpecnId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_extndr_specn ({0}, {1}, {2})", name, description, genericIndicator);

                throw ex;
            }

            return specificationId;
        }

        public void CreateGenericBayExtenderSpecification(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, long depthId, long heightId, long widthId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pDelInd", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDpthId", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pHghtId", DbType.Int64, heightId, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pWdthId", DbType.Int64, widthId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extndr_specn_gnrc", parameters);

                //PROCEDURE insert_bay_extndr_specn_gnrc(pId IN NUMBER, pCmpltInd IN VARCHAR2, pPrpgtInd IN VARCHAR2,
                //pDelInd IN VARCHAR2, pDpthId IN NUMBER, pHghtId IN NUMBER, pWdthId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_extndr_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6})", specificationId, completionIndicator, propagationIndicator, deletionIndicator, depthId, heightId, widthId);

                throw ex;
            }
        }

        public long CreateBayExtenderSpecificationRevision(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, string roIndicator, string name)
        {
            IDbDataParameter[] parameters = null;
            long revisionId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, CheckNullValue(name), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDelInd", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pRo", DbType.String, roIndicator, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("oRevsnAltId", DbType.Int64, revisionId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_bay_extndr_specn_revsn", parameters);

                revisionId = long.Parse(parameters[6].Value.ToString());

                //PROCEDURE insert_bay_extndr_specn_revsn(pId IN NUMBER, pNm IN VARCHAR2, pCmpltInd IN VARCHAR2,
                //pPrpgtInd IN VARCHAR2, pDelInd IN VARCHAR2, pRo IN VARCHAR2, oRevsnAltId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create bay_extndr_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5})", specificationId, completionIndicator, propagationIndicator, deletionIndicator, roIndicator, name);

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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.associate_material", parameters);

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

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.insert_nds_spec_id", parameters);

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