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
    public class NodeSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public NodeSpecificationDbInterface() : base()
        {
        }

        public NodeSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification node = null;
            NodeDbInterface nodeDbInterface = null;
            MaterialDbInterface materialDbInterface = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("node_specn_pkg.get_node_specn", parameters);

                if (reader.Read())
                {
                    string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");

                    node = new NodeSpecification(specificationId);

                    node.Name = DataReaderHelper.GetNonNullValue(reader, "node_specn_nm");
                    node.Description = DataReaderHelper.GetNonNullValue(reader, "node_specn_dsc");
                    ((NodeSpecification)node).NodeTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "node_typ_id", true));
                    SpecificationDbInterface dbI = new SpecificationDbInterface();
                    ((NodeSpecification)node).NodeUseTypId = dbI.GetUseType("Node", ((NodeSpecification)node).NodeTypeId);
                    if (DataReaderHelper.GetNonNullValue(reader, "has_prts_ind") == "Y")
                    { ((NodeSpecification)node).HasPorts = true; }
                    else { ((NodeSpecification)node).HasPorts = false; }
                    if (DataReaderHelper.GetNonNullValue(reader, "has_shlvs_ind") == "Y")
                    { ((NodeSpecification)node).HasShelves = true; }
                    else { ((NodeSpecification)node).HasShelves = false; }
                    ((NodeSpecification)node).QoSCapable = DataReaderHelper.GetNonNullValue(reader, "qos_cpbl_ind");
                    ((NodeSpecification)node).MidPlaneIndicator = DataReaderHelper.GetNonNullValue(reader, "mid_pln_ind");
                    ((NodeSpecification)node).MultiplexingCapable = DataReaderHelper.GetNonNullValue(reader, "muxg_cpbl_ind");
                    ((NodeSpecification)node).PerformanceMonitoringCapable = DataReaderHelper.GetNonNullValue(reader, "prfmc_mntrg_cpbl_ind");
                    ((NodeSpecification)node).EnniCapable = DataReaderHelper.GetNonNullValue(reader, "enni_cpbl_ind");
                    //((NodeSpecification)node).ESPlsCardRequired = DataReaderHelper.GetNonNullValue(reader, "es_pls_card_reqr_ind");
                    ((NodeSpecification)node).NewServiceAllowed = DataReaderHelper.GetNonNullValue(reader, "new_srvc_allow_ind");
                    ((NodeSpecification)node).StraightThroughIndicator = DataReaderHelper.GetNonNullValue(reader, "strght_thru_ind");
                    ((NodeSpecification)node).WallMountIndicator = DataReaderHelper.GetNonNullValue(reader, "wll_mnt_allow_ind");
                    ((NodeSpecification)node).LabelName = DataReaderHelper.GetNonNullValue(reader, "label_nm");
                    ((NodeSpecification)node).LabelPositionId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "label_pos_id", true));
                    ((NodeSpecification)node).FormatCode = DataReaderHelper.GetNonNullValue(reader, "node_frmt_cd");
                    ((NodeSpecification)node).FormatValueQualifier = int.Parse(DataReaderHelper.GetNonNullValue(reader, "node_frmt_val_qlfr_id", true));
                    ((NodeSpecification)node).IncludeFormatCode = DataReaderHelper.GetNonNullValue(reader, "node_frmt_nclud_ind");
                    ((NodeSpecification)node).StructureType = DataReaderHelper.GetNonNullValue(reader, "smple_cmplx_strctrd_ind");
                    ((NodeSpecification)node).MTRL_cd = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((NodeSpecification)node).MFR_cd = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    ((NodeSpecification)node).MFG_part_no = DataReaderHelper.GetNonNullValue(reader, "mfg_part_no");
                    ((NodeSpecification)node).SoftwareVersion = DataReaderHelper.GetNonNullValue(reader, "sw_vrsn_no");


                    if ("N".Equals(genericIndicator))
                    {
                        long mtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        long materialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "rme_node_mtrl_revsn_id", true));

                        node.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                        node.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                        node.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                        ((NodeSpecification)node).IsGeneric = false;
                        ((NodeSpecification)node).RevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "node_specn_revsn_alt_id", true));
                        ((NodeSpecification)node).RevisionName = DataReaderHelper.GetNonNullValue(reader, "node_specn_revsn_nm");
                        ((NodeSpecification)node).IsRecordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind") == "Y" ? true : false;

                        if (mtrlId > 0 && materialItemId > 0)
                        {
                            MaterialItem nodeMaterial = null;

                            nodeDbInterface = new NodeDbInterface();
                            materialDbInterface = new MaterialDbInterface();

                            ((NodeSpecification)node).AssociatedMaterial = nodeDbInterface.GetMaterial(materialItemId, mtrlId);

                            Task t = Task.Run(async () =>
                            {
                                nodeMaterial = await materialDbInterface.GetMaterialItemSAPAsync(materialItemId);
                            });

                            t.Wait();

                            if (nodeMaterial != null)
                            {
                                if (nodeMaterial.Attributes.ContainsKey(MaterialType.JSON.ItmDesc))
                                    node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, nodeMaterial.Attributes[MaterialType.JSON.ItmDesc]);
                                else
                                    node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));

                                if (nodeMaterial.Attributes.ContainsKey(MaterialType.JSON.PrtNo))
                                    node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, nodeMaterial.Attributes[MaterialType.JSON.PrtNo]);
                                else
                                    node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                            else
                            {
                                node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.ItmDesc, new Models.Attribute(MaterialType.JSON.ItmDesc, ""));
                                node.AssociatedMaterial.Attributes.Add(MaterialType.JSON.PrtNo, new Models.Attribute(MaterialType.JSON.PrtNo, ""));
                            }
                        }
                    }
                    else
                    {
                        node.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "gnrc_cmplt_ind") == "Y" ? true : false;
                        node.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "gnrc_prpgt_ind") == "Y" ? true : false;
                        node.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "gnrc_del_ind") == "Y" ? true : false;
                        ((NodeSpecification)node).IsGeneric = true;
                        ((NodeSpecification)node).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                        ((NodeSpecification)node).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                        ((NodeSpecification)node).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                        ((NodeSpecification)node).DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve specification id: {0}";
                logger.Error(oe, message, specificationId);
            }
            catch (Exception ex)
            {
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

            return node;
        }

        public Dictionary<long, SpecificationRole> GetRoleList(long nodeSpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, SpecificationRole> roles = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, nodeSpecificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("node_specn_pkg.get_node_specn_role_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "NODE_ROLE_TYP_ID", true));
                    SpecificationRole nodeRole = new SpecificationRole(specificationId);

                    nodeRole.IsSelected = DataReaderHelper.GetNonNullValue(reader, "is_selected") == "Y" ? true : false;
                    nodeRole.PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "node_typ_role_prty_no", true));
                    nodeRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "node_role_typ");

                    if (roles == null)
                        roles = new Dictionary<long, SpecificationRole>();

                    roles.Add(nodeRole.SpecificationId, nodeRole);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Node Role list");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Node Role list");
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
        //public Dictionary<long, NodeInternalSpecification> GetNodeInternals()
        //{
        //    IAccessor dbManager = null;
        //    IDbDataParameter[] parameters = null;
        //    IDataReader reader = null;
        //    Dictionary<long, NodeInternalSpecification> internals = null;

        //    try
        //    {
        //        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

        //        parameters = dbManager.GetParameterArray(1);

        //        parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

        //        reader = dbManager.ExecuteDataReaderSP("node_specn_pkg.get_all_node_itnl", parameters);

        //        while (reader.Read())
        //        {
        //        }
        //    }
        //    catch (OracleException oe)
        //    {
        //        logger.Error(oe, "Unable to retrieve Bay Internal Widths");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to retrieve Bay Internal Widths");
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }

        //        if (dbManager != null)
        //            dbManager.Dispose();
        //    }

        //    return internals;
        //}

        public void UpdateNodeSpecification(long specificationId, string name, int nodeTypeId, bool portsInd, bool shelvesInd, string qosCpblInd, string muxgCpbl,
            string prfMc, string eNNi, string newSrvc, string nodeFrmtCd, int nodeFrmtVal, string frmtNclud, string nodeSpecDsc, string wallMnt,
            string straightThru, string midPlan, bool gnrc, string smpleCmplx, string labelNm, int labelPosId, string swVrsnNo)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(22);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pNdTypId", DbType.Int32, nodeTypeId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPrtsInd", DbType.String, portsInd ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pShlvInd", DbType.String, shelvesInd ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pQosCpblInd", DbType.String, qosCpblInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pMuxgCpbl", DbType.String, muxgCpbl, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pPrfmc", DbType.String, prfMc, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pEnni", DbType.String, eNNi, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pNewSrvc", DbType.String, newSrvc, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pNodeFrmtCd", DbType.String, nodeFrmtCd, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pNodeFrmtVal", DbType.Int32, nodeFrmtVal, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pFrmtNclud", DbType.String, frmtNclud, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pNodeSpecDsc", DbType.String, nodeSpecDsc, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pWallMnt", DbType.String, wallMnt, ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pStraightThru", DbType.String, straightThru, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pMidPln", DbType.String, midPlan, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pGnrc", DbType.String, gnrc ? "Y" : "N", ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("pSmpleCmplx", DbType.String, smpleCmplx, ParameterDirection.Input);
                parameters[19] = dbAccessor.GetParameter("pLabelNm", DbType.String, labelNm, ParameterDirection.Input);
                parameters[20] = dbAccessor.GetParameter("pLabelPosId", DbType.Int32, labelPosId, ParameterDirection.Input);
                parameters[21] = dbAccessor.GetParameter("pSwVrsnNo", DbType.String, swVrsnNo, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.update_node_specn", parameters);

                //PROCEDURE update_node_specn(pId IN NUMBER, pNm IN VARCHAR2, pNdTypId IN number, pPrtsInd IN VARCHAR2, pShlvInd IN VARCHAR2,
                //pQosCpblInd IN VARCHAR2, pMuxgCpbl IN VARCHAR2, pPrfmc IN VARCHAR2, pEnni IN VARCHAR2, pNewSrvc IN VARCHAR2,
                //pNodeFrmtCd IN VARCHAR2, pNodeFrmtVal IN NUMBER, pFrmtNclud IN VARCHAR2, pNodeSpecDsc IN VARCHAR2, pWallMnt IN VARCHAR2,
                //pStraightThru IN VARCHAR2, pMidPln IN VARCHAR2, pGnrc IN VARCHAR2, pSmpleCmplx in varchar2, pLabelNm in varchar2, pLabelPosId IN NUMBER,
                //pSwVrsnNo IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update node_specn ({0})", specificationId);

                throw ex;
            }
        }

        public void UpdateGenericNOdeSpecification(long specificationId, string completionIndicator, string propagationIndicator,
            string deletionIndicator, decimal depth, decimal height, decimal width, int uom)
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.update_node_specn_gnrc", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update node_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId,
                    completionIndicator, propagationIndicator, deletionIndicator, depth, height, width, uom);

                throw ex;
            }

        }

        public void UpdateNodeSpecificationRevision(long revisionAltId, long specificationId, string name, bool completed, bool propagated, bool deleted, bool RO)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(7);
                parameters[0] = dbAccessor.GetParameter("pAltId", DbType.Int64, revisionAltId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRo", DbType.String, RO ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCmplt", DbType.String, completed ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagated ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pDel", DbType.String, deleted ? "Y" : "N", ParameterDirection.Input);
                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.update_node_specn_revsn_alt", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update update_node_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5},{6})", revisionAltId, specificationId, name, completed, propagated, deleted, RO);
                throw ex;
            }
        }
        //public void DeleteNodeSpecificationRole(long specificationId)
        //{
        //    IDbDataParameter[] parameters = null;

        //    try
        //    {
        //        parameters = dbAccessor.GetParameterArray(1);
        //        parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
        //        dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.delete_node_specn_role", parameters);
                
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to delete delete_node_specn_role ({0})", specificationId);
        //        throw ex;
        //    }
        //}
        //public void InsertNodeSpecificationRole(long specificationId, int roleTypeId, int prtyNo)
        //{
        //    IDbDataParameter[] parameters = null;

        //    try
        //    {
        //        parameters = dbAccessor.GetParameterArray(3);
        //        parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
        //        parameters[1] = dbAccessor.GetParameter("pRlTypId", DbType.Int32, roleTypeId, ParameterDirection.Input);
        //        parameters[2] = dbAccessor.GetParameter("pPrtyNo", DbType.Int32, prtyNo, ParameterDirection.Input);
        //        dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.insert_node_specn_role", parameters);
               
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to insert insert_node_specn_role ({0}, {1})", specificationId, roleTypeId);

        //        throw ex;
        //    }
        //}
        public long CreateNodeSpecification(string name, int nodeTypeId, bool portsInd, bool shelvesInd, string qosCpblInd, string muxgCpbl,
            string prfMc, string eNNi, string newSrvc, string nodeFrmtCd, int nodeFrmtVal, string frmtNclud, string nodeSpecDsc, string wallMnt,
            string straightThru, string midPlan, bool gnrc, string smpleCmplx, string labelNm, int labelPosId, string swVrsnNo)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(22);

                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNdTypId", DbType.Int32, nodeTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPrtsInd", DbType.String, portsInd ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pShlvInd", DbType.String, shelvesInd ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pQosCpblInd", DbType.String, qosCpblInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMuxgCpbl", DbType.String, muxgCpbl, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrfmc", DbType.String, prfMc, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pEnni", DbType.String, eNNi, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pNewSrvc", DbType.String, newSrvc, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pNodeFrmtCd", DbType.String, nodeFrmtCd, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pNodeFrmtVal", DbType.Int32, nodeFrmtVal, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pFrmtNclud", DbType.String, frmtNclud, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pNodeSpecDsc", DbType.String, nodeSpecDsc, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pWallMnt", DbType.String, wallMnt, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pStraightThru", DbType.String, straightThru, ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pMidPln", DbType.String, midPlan, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pGnrc", DbType.String, gnrc ? "Y" : "N", ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pSmpleCmplx", DbType.String, smpleCmplx, ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("pLabelNm", DbType.String, labelNm, ParameterDirection.Input);
                parameters[19] = dbAccessor.GetParameter("pLabelPosId", DbType.Int32, labelPosId, ParameterDirection.Input);
                parameters[20] = dbAccessor.GetParameter("pSwVrsnNo", DbType.String, swVrsnNo, ParameterDirection.Input);
                parameters[21] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.insert_node_specn", parameters);

                specificationId = long.Parse(parameters[21].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_node_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11},{12},{13}, {14}, {15}, {16}, {17}, {18}, {19}, {20})",
                    name, nodeTypeId, portsInd, shelvesInd, qosCpblInd, muxgCpbl, prfMc, eNNi, newSrvc, nodeFrmtCd, nodeFrmtVal, frmtNclud, nodeSpecDsc,
                    wallMnt, straightThru, midPlan, gnrc, smpleCmplx, labelNm, labelPosId, swVrsnNo);

                throw ex;
            }

            return specificationId;
        }

        public void CreateGenericNodeSpecification(long specificationId, string completionIndicator, string propagationIndicator,
            string deletionIndicator, decimal depth, decimal height, decimal width, int uom)
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.insert_node_specn_gnrc", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create node_specn_gnrc ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", specificationId,
                    completionIndicator, propagationIndicator, deletionIndicator, depth, height, width, uom);

                throw ex;
            }
        }

        public long CreateNodeSpecificationRevision(long specificationId, string completionIndicator, string propagationIndicator, string deletionIndicator, 
            decimal weight, int weightUom, string roIndicator, string name)
        {
            IDbDataParameter[] parameters = null;
            long revisionId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRo", DbType.String, roIndicator, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCmplt", DbType.String, completionIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pPrpgt", DbType.String, propagationIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pDel", DbType.String, deletionIndicator, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("oRevsnAltId", DbType.Int64, revisionId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.insert_node_specn_revsn_alt", parameters);

                revisionId = long.Parse(parameters[6].Value.ToString());

                //PROCEDURE insert_node_specn_revsn_alt(pId IN NUMBER, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2,
                //pPrpgt IN VARCHAR2, pDel IN VARCHAR2, oRevsnAltId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_node_specn_revsn_alt ({0}, {1}, {2}, {3}, {4}, {5})",
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.associate_material", parameters);

                //PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to associate material ({0}, {1})", specificationRevisionId, materialItemId);

                throw ex;
            }
        }

        public bool UpdateNodeSpecificationMaterial(long materialItemId, decimal height, decimal depth, decimal width, int uomId, decimal plannedHeatGeneration, int plannedHeatGenerationUom, decimal normalCurrentDrain,
            int normalCurrentDrainUom, decimal maxCurrentDrain, int maxCurrentDrainUom, decimal nodeWeight, int nodeWeightUom, decimal heatDissipation, int heatDissipationUom)
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
                parameters[4] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPlndHt", DbType.Decimal, CheckNullValue(plannedHeatGeneration), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPlndHtUom", DbType.Int32, CheckNullValue(plannedHeatGenerationUom), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pElcNrmnl", DbType.Decimal, CheckNullValue(normalCurrentDrain), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pElcNrmnlUom", DbType.Int32, CheckNullValue(normalCurrentDrainUom), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pElcMx", DbType.Decimal, CheckNullValue(maxCurrentDrain), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pElcMxUom", DbType.Int32, CheckNullValue(maxCurrentDrainUom), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pWt", DbType.Decimal, CheckNullValue(nodeWeight), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pWtUom", DbType.Int32, CheckNullValue(nodeWeightUom), ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pHt", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pHtUom", DbType.Int32, CheckNullValue(heatDissipationUom), ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("didUpdate", DbType.String, output, ParameterDirection.Output, 1);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "node_specn_pkg.update_node_material", parameters);

                output = parameters[15].Value.ToString();

                if ("Y".Equals(output))
                    didUpdate = true;

                //PROCEDURE update_node_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, 
                //pPlndHt IN NUMBER, pPlndHtUom IN NUMBER, pElcNrmnl IN NUMBER,
                //pElcNrmnlUom IN NUMBER, pElcMx IN NUMBER, pElcMxUom IN NUMBER, pWt IN NUMBER, pWtUom IN NUMBER, 
                //pHt IN NUMBER, pHtUom IN NUMBER, didUpdate OUT VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update associated material ({0}, {1}, {2}, {3}, {4}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14})", materialItemId, height, depth, width, uomId, plannedHeatGeneration,
                    plannedHeatGenerationUom, normalCurrentDrain, normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, nodeWeight, nodeWeightUom, heatDissipation, heatDissipationUom);

                throw ex;
            }

            return didUpdate;
        }

    
    }
}