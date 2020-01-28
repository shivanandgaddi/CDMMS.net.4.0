using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class NewUpdatedPartsType
    {
        public static string NUP_MCS_TABLE = "material_catalog_staging";
        public static string NUP_MCSH_TABLE = "material_catalog_staging_hist";
        public static string NUP_MIS_TABLE = "mtl_item_sap";
        public static string NUP_MIV_TABLE = "material_id_mapping_vw";
        public static string NUP_AUDIT_IES_TABLE = "AUDIT_IES_DA_EXTENDED_VW";
        public static string IES_EA_MAIN_EXTN = "ies_ea_main_extn";
        public static string IES_EA_COMP_EXPN = "ies_ea_comp_clei_extn";
        public static string IES_INV = "ies_invntry";
        public static string IES_EQPT = "ies_eqpt";
        public static string AUDIT_IES_DA = "audit_ies_da";
        public static string rme_bay_extn = "rme_bay_extndr_mtrl_revsn";
        public static string rme_bay_mtrl = "rme_bay_mtrl_revsn";
        public static string rme_bulk = "rme_bulk_cabl_mtrl_revsn";
        public static string rme_card = "rme_card_mtrl_revsn";
        public static string rme_card_alias = "rme_card_mtrl_revsn_alias_val";
        public static string rme_card_mtl = " rme_card_mtrl";
        public static string rme_cnt_card = "rme_cnctrzd_cabl_mtrl_revsn";
        public static string rme_cnt_card_cbl = "rme_cnctrzd_cabl_mtrl";
        public static string rme_cnt_card_alias = "rme_cnctrzd_cabl_revsn_als_val";
        public static string rme_node = "rme_node_mtrl_revsn";
        public static string rme_plg = "rme_plg_in_mtrl_revsn";
        public static string rme_shelf = "rme_shelf_mtrl_revsn";
        public static string rme_var = "rme_var_lgth_cabl_mtrl";
        public static string mtrl = "mtrl";
        public static string mfr = "mfr";
        public static string commonrevison = "rmetables";
        public static string rme_shelf_mtrl = "rme_shelf_mtrl";
        public static string rme_shelf_mtrl_alias = "rme_shelf_mtrl_revsn_alias_val";
        public static string rme_plg_mtrl = "rme_plg_in_mtrl";
        public static string rme_plg_alias = "rme_plg_in_mtrl_rvsn_alias_val";
        public static string rme_bay_extn_mtrl = "rme_bay_extndr_mtrl";
        public static string rme_bay_mtrl_r = "rme_bay_mtrl";
        public static string rme_bay_extn_alias = "rme_bay_extndr_mtrl_rv_als_val";
        public static string rme_node_mtrl = "rme_node_mtrl";
        public static string rme_bulk_alias = "rme_bulk_cabl_mtrl_rvs_als_val";
        public static string rme_bulk_cbl = "rme_bulk_cabl_mtrl";

        private Dictionary<string, DatabaseDefinition> newupdatedparts = null;
        private Dictionary<string, DatabaseDefinition> newupdatedpartslosdb = null;
        private Dictionary<string, DatabaseDefinition> stagingParts = null;
        private Dictionary<string, DatabaseDefinition> updatedStagingPartsSap = null;
        private Dictionary<string, DatabaseDefinition> updatedStagingPartsLosdb = null;
        private Dictionary<string, DatabaseDefinition> possibleMatch = null;
        private Dictionary<string, DatabaseDefinition> revisionpart = null;

        private Dictionary<string, DatabaseDefinition> rmeBayextn = null;
        private Dictionary<string, DatabaseDefinition> rmeBaymtrl = null;
        private Dictionary<string, DatabaseDefinition> rmeBulk = null;
        private Dictionary<string, DatabaseDefinition> rmeCard = null;
        private Dictionary<string, DatabaseDefinition> rmeCntCard = null;
        private Dictionary<string, DatabaseDefinition> rmeNode = null;
        private Dictionary<string, DatabaseDefinition> rmePlg = null;
        private Dictionary<string, DatabaseDefinition> rmeShelf = null;
        private Dictionary<string, DatabaseDefinition> rmeVar = null;

        public Dictionary<string, DatabaseDefinition> Rmeshelf
        {
            get
            {
                if (rmeShelf == null)
                    Getrmeshelf();

                return rmeShelf;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmevar
        {
            get
            {
                if (rmeVar == null)
                    Getrmevar();

                return rmeVar;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmeplg
        {
            get
            {
                if (rmePlg == null)
                    Getrmeplg();

                return rmePlg;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmenode
        {
            get
            {
                if (rmeNode == null)
                    Getrmenode();

                return rmeNode;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmecntcard
        {
            get
            {
                if (rmeCntCard == null)
                    Getrmecntcard();

                return rmeCntCard;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmebulk
        {
            get
            {
                if (rmeBulk == null)
                    Getrmebulk();

                return rmeBulk;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmecard
        {
            get
            {
                if (rmeCard == null)
                    Getrmecard();

                return rmeCard;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmebaymtrl
        {
            get
            {
                if (rmeBaymtrl == null)
                    Getrmebatmtrl();

                return rmeBaymtrl;
            }
        }
        public Dictionary<string, DatabaseDefinition> RevisionPart
        {
            get
            {
                if (revisionpart == null)
                    GetRevisionpart();

                return revisionpart;
            }
        }
        public Dictionary<string, DatabaseDefinition> Rmebayextn
        {
            get
            {
                if (rmeBayextn == null)
                    Getrmebayextn();

                return rmeBayextn;
            }
        }

        public Dictionary<string, DatabaseDefinition> NewUpdatedParts
        {
            get
            {
                if (newupdatedparts == null)
                    GetNewUpdatedPartsDefinition();

                return newupdatedparts;
            }
        }
        public Dictionary<string, DatabaseDefinition> NewUpdatedPartslosdb
        {
            get
            {
                if (newupdatedpartslosdb == null)
                    GetNewUpdatedPartsDefinitionlosdb();

                return newupdatedpartslosdb;
            }
        }
        public Dictionary<string, DatabaseDefinition> StagingParts
        {
            get
            {
                if (stagingParts == null)
                    GetStagingPartsDefinition();

                return stagingParts;
            }
        }
        public Dictionary<string, DatabaseDefinition> UpdatedStagingPartsSap
        {
            get
            {
                if (updatedStagingPartsSap == null)
                    GetUpdatedStagingPartsDefinitionSap();

                return updatedStagingPartsSap;
            }
        }

        public Dictionary<string, DatabaseDefinition> UpdatedStagingPartsLosdb
        {
            get
            {
                if (updatedStagingPartsLosdb == null)
                    GetUpdatedStagingPartsDefinitionLosdb();

                return updatedStagingPartsLosdb;
            }
        }
        public Dictionary<string, DatabaseDefinition> PossibleMatch
        {
            get
            {
                if (possibleMatch == null)
                    GetPossibleMatch();

                return possibleMatch;
            }
        }

        private void Getrmeplg()
        {

            rmePlg = new Dictionary<string, DatabaseDefinition>();
            rmePlg.Add("mtrlidplg", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmePlg.Add("mfrids", new DatabaseDefinition(mtrl, "mfr_id"));
            rmePlg.Add("rtpartnos", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmePlg.Add("mtrlcats", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmePlg.Add("rcrdolys", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmePlg.Add("mtrldescs", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmePlg.Add("lbrids", new DatabaseDefinition(mtrl, "lbr_id"));
            rmePlg.Add("featids", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmePlg.Add("dpthnos", new DatabaseDefinition(rme_plg_mtrl, "plg_in_role_typ_id"));
            rmePlg.Add("hgtnos", new DatabaseDefinition(rme_plg, "revsn_no"));
            rmePlg.Add("wdthnos", new DatabaseDefinition(rme_plg, "base_revsn_ind"));
            rmePlg.Add("dimuoids", new DatabaseDefinition(rme_plg, "ret_revsn_ind"));
            rmePlg.Add("hetnos", new DatabaseDefinition(rme_plg, "mtrl_cd"));
            rmePlg.Add("hetuoms", new DatabaseDefinition(rme_plg, "curr_revsn_ind"));
            rmePlg.Add("revsnos", new DatabaseDefinition(rme_plg, "ordbl_mtrl_stus_id"));
            rmePlg.Add("baserevs", new DatabaseDefinition(rme_plg, "clei_cd"));
            rmePlg.Add("retrvsns", new DatabaseDefinition(rme_plg_alias, "rme_ordbl_mtrl_revsn_alias_id"));
            rmePlg.Add("mtrlcd", new DatabaseDefinition(rme_plg_alias, "alias_val"));
            rmePlg.Add("mfrcdplg", new DatabaseDefinition(mfr, "mfr_cd"));


        }
        private void Getrmeshelf()
        {

            rmeShelf = new Dictionary<string, DatabaseDefinition>();
            rmeShelf.Add("mtrlids", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeShelf.Add("mfrids", new DatabaseDefinition(mtrl, "mfr_id"));
            rmeShelf.Add("rtpartnos", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeShelf.Add("mtrlcats", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeShelf.Add("rcrdolys", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeShelf.Add("mtrldescs", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeShelf.Add("lbrids", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeShelf.Add("featids", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeShelf.Add("dpthnos", new DatabaseDefinition(rme_shelf_mtrl, "dpth_no"));
            rmeShelf.Add("hgtnos", new DatabaseDefinition(rme_shelf_mtrl, "hgt_no"));
            rmeShelf.Add("wdthnos", new DatabaseDefinition(rme_shelf_mtrl, "wdth_no"));
            rmeShelf.Add("dimuoids", new DatabaseDefinition(rme_shelf_mtrl, "dim_uom_id"));
            rmeShelf.Add("hetnos", new DatabaseDefinition(rme_shelf, "het_dssptn_no"));
            rmeShelf.Add("hetuoms", new DatabaseDefinition(rme_shelf, "het_dssptn_uom_id"));
            rmeShelf.Add("revsnos", new DatabaseDefinition(rme_shelf, "revsn_no"));
            rmeShelf.Add("baserevs", new DatabaseDefinition(rme_shelf, "base_revsn_ind"));
            rmeShelf.Add("retrvsns", new DatabaseDefinition(rme_shelf, "ret_revsn_ind"));
            rmeShelf.Add("mtrlcd", new DatabaseDefinition(rme_shelf, "mtrl_cd"));
            rmeShelf.Add("currevsids", new DatabaseDefinition(rme_shelf, "curr_revsn_ind"));
            rmeShelf.Add("ordblmtrls", new DatabaseDefinition(rme_shelf, "ordbl_mtrl_stus_id"));
            rmeShelf.Add("cleicds", new DatabaseDefinition(rme_shelf, "clei_cd"));
            rmeShelf.Add("rmealiasids", new DatabaseDefinition(rme_shelf_mtrl_alias, "rme_mtrl_revsn_alias_id"));
            rmeShelf.Add("aliasvals", new DatabaseDefinition(rme_shelf_mtrl_alias, "alias_val"));
            rmeShelf.Add("shelfwts", new DatabaseDefinition(rme_shelf, "shelf_wt_no"));
            rmeShelf.Add("shelfuoms", new DatabaseDefinition(rme_shelf, "shelf_wt_uom_id"));
            rmeShelf.Add("mfrcds", new DatabaseDefinition(mfr, "mfr_cd"));


        }
        private void Getrmevar()
        {
            rmeVar = new Dictionary<string, DatabaseDefinition>();
            rmeVar.Add("rtpartnovar", new DatabaseDefinition(mtrl, "rt_part_no "));
            rmeVar.Add("rcrdolyvar", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeVar.Add("mtrldscvar", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeVar.Add("lbridvar", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeVar.Add("mfridvar", new DatabaseDefinition(mfr, "mfr_id"));
            rmeVar.Add("mfrcdvar", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeVar.Add("setlngtvar", new DatabaseDefinition(rme_var, "set_lgth_uom_id"));
            rmeVar.Add("cbltypvar", new DatabaseDefinition(rme_var, "cabl_typ_id"));
            rmeVar.Add("mtrlcdvar", new DatabaseDefinition(rme_var, "mtrl_cd"));

        }

        private void GetNewUpdatedPartsDefinition()
        {
            newupdatedparts = new Dictionary<string, DatabaseDefinition>();

            newupdatedparts.Add("materialcode", new DatabaseDefinition(NUP_MCS_TABLE, "MTL_CD"));
            newupdatedparts.Add("heci", new DatabaseDefinition(NUP_MCS_TABLE, "HECI"));
            newupdatedparts.Add("clmc", new DatabaseDefinition(NUP_MCS_TABLE, "MANUFACTURER"));
            newupdatedparts.Add("mfg_part_no", new DatabaseDefinition(NUP_MCS_TABLE, "MFG_PART_NO"));
            newupdatedparts.Add("item_status", new DatabaseDefinition(NUP_MCS_TABLE, "ITEM_STATUS"));
            newupdatedparts.Add("last_chg_dt", new DatabaseDefinition(NUP_MCS_TABLE, "LAST_CHG_DT"));
            newupdatedparts.Add("mtl_desc", new DatabaseDefinition(NUP_MCS_TABLE, "MTL_DESC"));
            newupdatedparts.Add("type", new DatabaseDefinition(NUP_MCS_TABLE, "TYPE"));
            newupdatedparts.Add("clei7", new DatabaseDefinition(NUP_MCS_TABLE, "COMPATIBLEEQUIPMENTCLEI7"));
            newupdatedparts.Add("needs_to_be_reviewed", new DatabaseDefinition(NUP_MCS_TABLE, "NEEDS_TO_BE_REVIEWED"));
            newupdatedparts.Add("has_losdb", new DatabaseDefinition(NUP_MCS_TABLE, "HAS_LOSDB"));



        }
        private void GetNewUpdatedPartsDefinitionlosdb()
        {
            newupdatedpartslosdb = new Dictionary<string, DatabaseDefinition>();


            newupdatedpartslosdb.Add("auditdaid", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_DA_ID"));
            newupdatedpartslosdb.Add("usraprvtmstp", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "USER_APRV_TMSTMP"));
            newupdatedpartslosdb.Add("usraprvind", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "USER_APRV_IND"));
            newupdatedpartslosdb.Add("usraprvtxt", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "USER_APRV_TXT"));
            newupdatedpartslosdb.Add("AuditTablePkColumnName", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_TBL_PK_COL_NM"));
            newupdatedpartslosdb.Add("CDMMSColumnName", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "CDMMS_COL_NM"));
            newupdatedpartslosdb.Add("NewColumnValue", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "NEW_COL_VAL"));
            newupdatedpartslosdb.Add("CDMMSTableName", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "CDMMS_TBL_NM"));
            newupdatedpartslosdb.Add("losdbprodid", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_DA_ID"));
            newupdatedpartslosdb.Add("AuditTablePkColumnValue", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_TBL_PK_COL_VAL"));
            newupdatedpartslosdb.Add("AuditParentTablePKColumnName", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_PRNT_TBL_PK_COL_NM"));
            newupdatedpartslosdb.Add("AuditParentTablePkColumnValue", new DatabaseDefinition(NUP_AUDIT_IES_TABLE, "AUDIT_PRNT_TBL_PK_COL_VAL"));


        }
        private void GetUpdatedStagingPartsDefinitionSap()
        {
            updatedStagingPartsSap = new Dictionary<string, DatabaseDefinition>();
            updatedStagingPartsSap.Add("mfg_part_no", new DatabaseDefinition(NUP_MIS_TABLE, "MFG_PART_NO"));
            updatedStagingPartsSap.Add("heci", new DatabaseDefinition(NUP_MIS_TABLE, "HECI"));
            updatedStagingPartsSap.Add("materialcode", new DatabaseDefinition(NUP_MIS_TABLE, "PRODUCT_ID"));
            updatedStagingPartsSap.Add("item_status", new DatabaseDefinition(NUP_MIS_TABLE, "ITEM_CURRENT_STATUS"));
            updatedStagingPartsSap.Add("mtl_grp", new DatabaseDefinition(NUP_MIS_TABLE, "CATEGORY_ID"));
            updatedStagingPartsSap.Add("icc_cd", new DatabaseDefinition(NUP_MIS_TABLE, "ICC_CD"));
            updatedStagingPartsSap.Add("uom", new DatabaseDefinition(NUP_MIS_TABLE, "UNIT_OF_MSRMT"));
            updatedStagingPartsSap.Add("mvg_avg_price_amt", new DatabaseDefinition(NUP_MIS_TABLE, "AVG_PRICE_AMT"));
            updatedStagingPartsSap.Add("mtl_desc", new DatabaseDefinition(NUP_MIS_TABLE, "ITEM_DESC"));
            updatedStagingPartsSap.Add("po_text", new DatabaseDefinition(NUP_MIS_TABLE, "ADDITIONAL_DESC"));
            updatedStagingPartsSap.Add("hazard_ind", new DatabaseDefinition(NUP_MIS_TABLE, "HAZARD_IND"));
            updatedStagingPartsSap.Add("aic_cd", new DatabaseDefinition(NUP_MIS_TABLE, "MIC_CD"));
            updatedStagingPartsSap.Add("pland_del_tm", new DatabaseDefinition(NUP_MIS_TABLE, "NSTOCK_DEL_INT_DUR"));
            updatedStagingPartsSap.Add("last_chg_dt", new DatabaseDefinition(NUP_MIS_TABLE, "LAST_UPDTD_TMSTMP"));
            updatedStagingPartsSap.Add("clmc", new DatabaseDefinition(NUP_MIS_TABLE, "MFG_ID"));
            updatedStagingPartsSap.Add("mfg_name", new DatabaseDefinition(NUP_MIS_TABLE, "MFG_DESC"));



        }
        private void GetUpdatedStagingPartsDefinitionLosdb()
        {
            updatedStagingPartsLosdb = new Dictionary<string, DatabaseDefinition>();


            updatedStagingPartsLosdb.Add("height_unit", new DatabaseDefinition(IES_EA_MAIN_EXTN, "HEIGHT_ENGLISH_UNIT"));
            updatedStagingPartsLosdb.Add("height_uom", new DatabaseDefinition(IES_EA_MAIN_EXTN, "HEIGHT_ENGLISH_VALUE"));
            updatedStagingPartsLosdb.Add("depth_unit", new DatabaseDefinition(IES_EA_MAIN_EXTN, "DEPTH_ENGLISH_UNIT"));
            updatedStagingPartsLosdb.Add("depth_uom", new DatabaseDefinition(IES_EA_MAIN_EXTN, "DEPTH_ENGLISH_VALUE"));
            updatedStagingPartsLosdb.Add("width_unit", new DatabaseDefinition(IES_EA_MAIN_EXTN, "WIDTH_ENGLISH_UNIT"));
            updatedStagingPartsLosdb.Add("width_uom", new DatabaseDefinition(IES_EA_MAIN_EXTN, "WIDTH_ENGLISH_VALUE"));
            updatedStagingPartsLosdb.Add("clei7", new DatabaseDefinition(IES_EA_COMP_EXPN, "COMPATIBLEEQUIPMENTCLEI7"));
            updatedStagingPartsLosdb.Add("vendor_cd", new DatabaseDefinition(IES_EQPT, "vndr_cd"));
            updatedStagingPartsLosdb.Add("vendor_cd1", new DatabaseDefinition(IES_EQPT, "vndr_cd"));
            updatedStagingPartsLosdb.Add("clei_cd", new DatabaseDefinition(IES_INV, "clei_cd"));
            updatedStagingPartsLosdb.Add("part_no", new DatabaseDefinition(IES_EQPT, "part_no"));



        }

        private void GetRevisionpart()
        {
            revisionpart = new Dictionary<string, DatabaseDefinition>();

            revisionpart.Add("materialcoder", new DatabaseDefinition(commonrevison, "mtrl_cd"));
            revisionpart.Add("revsnor", new DatabaseDefinition(commonrevison, "revsn_no"));
            revisionpart.Add("base_rev", new DatabaseDefinition(commonrevison, "base_revsn_ind"));
            revisionpart.Add("curr_revrr", new DatabaseDefinition(commonrevison, "curr_revsn_ind"));
            revisionpart.Add("clei_coder", new DatabaseDefinition(commonrevison, "clei_cd"));
            revisionpart.Add("mtrl_descr", new DatabaseDefinition(commonrevison, "mtrl_dsc"));
            revisionpart.Add("mfrcdr", new DatabaseDefinition(commonrevison, "mfr_cd"));
            revisionpart.Add("mfridr", new DatabaseDefinition(commonrevison, "mfr_id"));
            revisionpart.Add("partnor", new DatabaseDefinition(commonrevison, "rt_part_no"));
            revisionpart.Add("specnamer", new DatabaseDefinition(commonrevison, "mfr_id"));

        }

        private void GetPossibleMatch()
        {
            possibleMatch = new Dictionary<string, DatabaseDefinition>();

            possibleMatch.Add("checkmatched", new DatabaseDefinition(IES_EQPT, "checkmatched"));
            possibleMatch.Add("clei_cd_pbl", new DatabaseDefinition(IES_EQPT, "heci"));
            possibleMatch.Add("vendor_cd_pbl", new DatabaseDefinition(IES_EQPT, "mfg_id"));
            possibleMatch.Add("part_no_pbl", new DatabaseDefinition(IES_EQPT, "mfg_part_no"));
            possibleMatch.Add("descpbl", new DatabaseDefinition(IES_EQPT, "item_desc"));
            possibleMatch.Add("prodt_id", new DatabaseDefinition(IES_EQPT, "product_ids"));
            possibleMatch.Add("ies_eqpt_ctlg_item_id", new DatabaseDefinition(IES_EQPT, "ies_eqpt_ctlg_item_id"));
            possibleMatch.Add("pcnchange", new DatabaseDefinition(IES_EQPT, "pcnchange"));

        }
        private void Getrmebayextn()
        {

            rmeBayextn = new Dictionary<string, DatabaseDefinition>();

            rmeBayextn.Add("mtrlidbayextn", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeBayextn.Add("mfrbayextn", new DatabaseDefinition(mfr, "mfr_id"));
            rmeBayextn.Add("mfrbayextn", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeBayextn.Add("rtpartnobayextn", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeBayextn.Add("mtrlcatbayextn", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeBayextn.Add("mtrldescbayextn", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeBayextn.Add("lbridbayextn", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeBayextn.Add("featidbayextn", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeBayextn.Add("dpthnobayextn", new DatabaseDefinition(rme_bay_extn_mtrl, "dpth_no"));
            rmeBayextn.Add("hgtnobayextn", new DatabaseDefinition(rme_bay_extn_mtrl, "hgt_no"));
            rmeBayextn.Add("wdthnobayextn", new DatabaseDefinition(rme_bay_extn_mtrl, "wdth_no"));
            rmeBayextn.Add("dimuoidbayextn", new DatabaseDefinition(rme_bay_extn_mtrl, "dim_uom_id"));
            rmeBayextn.Add("revnobayextn", new DatabaseDefinition(rme_bay_extn, "revsn_no"));
            rmeBayextn.Add("baservnobayextn", new DatabaseDefinition(rme_bay_extn, "base_revsn_ind"));
            rmeBayextn.Add("currevnobayextn", new DatabaseDefinition(rme_bay_extn, "curr_revsn_ind"));
            rmeBayextn.Add("retrvnobayextn", new DatabaseDefinition(rme_bay_extn, "ret_revsn_ind"));
            rmeBayextn.Add("ordblbayextn", new DatabaseDefinition(rme_bay_extn, "ordbl_mtrl_stus_id"));
            rmeBayextn.Add("cleicdbayextn", new DatabaseDefinition(rme_bay_extn, "clei_cd"));
            rmeBayextn.Add("rcdolybayextn", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeBayextn.Add("mfrcdbayextn", new DatabaseDefinition(rme_bay_extn, "mtrl_cd"));

        }
        private void Getrmebatmtrl()
        {
            rmeBaymtrl = new Dictionary<string, DatabaseDefinition>();
            rmeBaymtrl.Add("mtrlidbayextn", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeBaymtrl.Add("mfridbayextn", new DatabaseDefinition(mfr, "mfr_id"));
            rmeBaymtrl.Add("mfrcdbayextn", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeBaymtrl.Add("rtpartnobayextn", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeBaymtrl.Add("mtrlcatbayextn", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeBaymtrl.Add("mtrldescbayextn", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeBaymtrl.Add("lbridbayextn", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeBaymtrl.Add("featidbayextn", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeBaymtrl.Add("dpthnobayextn", new DatabaseDefinition(rme_bay_mtrl_r, "xtnl_dpth_no"));
            rmeBaymtrl.Add("hgtnobayextn", new DatabaseDefinition(rme_bay_mtrl_r, "xtnl_hgt_no"));
            rmeBaymtrl.Add("wdthnobayextn", new DatabaseDefinition(rme_bay_mtrl_r, "xtnl_wdth_no"));
            rmeBaymtrl.Add("dimuoidbayextn", new DatabaseDefinition(rme_bay_mtrl_r, "xtnl_dim_uom_id"));
            rmeBaymtrl.Add("revnobayextn", new DatabaseDefinition(rme_bay_mtrl_r, "cab_ind"));
            rmeBaymtrl.Add("baservnobayextn", new DatabaseDefinition(rme_bay_mtrl, "het_dssptn_no"));
            rmeBaymtrl.Add("currevnobayextn", new DatabaseDefinition(rme_bay_mtrl, "het_dssptn_uom_id"));
            rmeBaymtrl.Add("retrvnobayextn", new DatabaseDefinition(rme_bay_mtrl, "revsn_no"));
            rmeBaymtrl.Add("ordblbayextn", new DatabaseDefinition(rme_bay_mtrl, "base_revsn_ind"));
            rmeBaymtrl.Add("cleicdbayextn", new DatabaseDefinition(rme_bay_mtrl, "curr_revsn_ind"));
            rmeBaymtrl.Add("rcdolybayextn", new DatabaseDefinition(rme_bay_mtrl, "ret_revsn_ind"));
            rmeBaymtrl.Add("mfrcdbayextn", new DatabaseDefinition(rme_bay_mtrl, "ordbl_mtrl_stus_id"));
            rmeBaymtrl.Add("revnobayextn", new DatabaseDefinition(rme_bay_mtrl, "clei_cd"));
            rmeBaymtrl.Add("baservnobayextn", new DatabaseDefinition(rme_bay_mtrl, "plnd_het_gntn_no"));
            rmeBaymtrl.Add("currevnobayextn", new DatabaseDefinition(rme_bay_mtrl, "plnd_het_gntn_uom_id"));
            rmeBaymtrl.Add("retrvnobayextn", new DatabaseDefinition(rme_bay_mtrl, "elc_curr_norm_drn_no"));
            rmeBaymtrl.Add("ordblbayextn", new DatabaseDefinition(rme_bay_mtrl, "elc_curr_norm_drn_uom_id"));
            rmeBaymtrl.Add("cleicdbayextn", new DatabaseDefinition(rme_bay_mtrl, "elc_curr_max_drn_no"));
            rmeBaymtrl.Add("rcdolybayextn", new DatabaseDefinition(rme_bay_mtrl, "elc_curr_max_drn_uom_id"));
            rmeBaymtrl.Add("mfrcdbayextn", new DatabaseDefinition(rme_bay_mtrl, "bay_wt_no"));
            rmeBaymtrl.Add("cleicdbayextn", new DatabaseDefinition(rme_bay_mtrl, "bay_wt_uom_id"));
            rmeBaymtrl.Add("rcdolybayextn", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeBaymtrl.Add("mfrcdbayextn", new DatabaseDefinition(rme_bay_mtrl, "mtrl_cd"));
        }
        private void Getrmebulk()
        {
            rmeBulk = new Dictionary<string, DatabaseDefinition>();
            rmeBulk.Add("mtrlidbulk", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeBulk.Add("mfridbulk", new DatabaseDefinition(mtrl, "mfr_id"));
            rmeBulk.Add("mfrcdbulk", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeBulk.Add("rtpartnobulk", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeBulk.Add("mtrlcatbulk", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeBulk.Add("mtrldescbulk", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeBulk.Add("lbridbulk", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeBulk.Add("featidbulk", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeBulk.Add("specidbulk", new DatabaseDefinition(rme_bulk_cbl, "specn_id"));
            rmeBulk.Add("cabtypbulk", new DatabaseDefinition(rme_bulk_cbl, "cabl_typ_id"));
            rmeBulk.Add("rvnobulk", new DatabaseDefinition(rme_bulk, "revsn_no"));
            rmeBulk.Add("baservbulk", new DatabaseDefinition(rme_bulk, "base_revsn_ind"));
            rmeBulk.Add("retevbulk", new DatabaseDefinition(rme_bulk, "ret_revsn_ind"));
            rmeBulk.Add("currvbulk", new DatabaseDefinition(rme_bulk, "curr_revsn_ind"));
            rmeBulk.Add("orblbulk", new DatabaseDefinition(rme_bulk, "ordbl_mtrl_stus_id"));
            rmeBulk.Add("bulkcblbulk", new DatabaseDefinition(rme_bulk, "bulk_cabl_specn_revsn_alt_id"));
            rmeBulk.Add("cleicdbulk", new DatabaseDefinition(rme_bulk, "clei_cd"));
            rmeBulk.Add("rmealisbulk", new DatabaseDefinition(rme_bulk_alias, "rme_mtrl_revsn_alias_id"));
            rmeBulk.Add("aliasvalbulk", new DatabaseDefinition(rme_bulk_alias, "alias_val"));
            rmeBulk.Add("rcrolybulk", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeBulk.Add("mtrlcdbulk", new DatabaseDefinition(mfr, "mtrl_cd"));


        }
        private void Getrmecntcard()
        {
            rmeCntCard = new Dictionary<string, DatabaseDefinition>();

            rmeCntCard.Add("mtrlidcnzcard", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeCntCard.Add("mfridcnzcard", new DatabaseDefinition(mtrl, "mfr_id"));
            rmeCntCard.Add("mfrcdcnzcard", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeCntCard.Add("rtpartnobayextn", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeCntCard.Add("mtrlcatbayextn", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeCntCard.Add("mtrldescbayextn", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeCntCard.Add("lbridbayextn", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeCntCard.Add("featidbayextn", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeCntCard.Add("varlngtcnzcard", new DatabaseDefinition(rme_cnt_card_cbl, "var_lgth_cabl_mtrl_id"));
            rmeCntCard.Add("setlgthcnzcard", new DatabaseDefinition(rme_cnt_card_cbl, "set_lgth_no"));
            rmeCntCard.Add("setlgthuomcnzcard", new DatabaseDefinition(rme_cnt_card_cbl, "set_lgth_uom_id"));
            rmeCntCard.Add("cabltypcnzcard", new DatabaseDefinition(rme_cnt_card_cbl, "cabl_typ_id"));
            rmeCntCard.Add("retrvcnzcard", new DatabaseDefinition(rme_cnt_card, "ret_revsn_ind"));
            rmeCntCard.Add("currvcnzcard", new DatabaseDefinition(rme_cnt_card, "curr_revsn_ind"));
            rmeCntCard.Add("revnocnzcard", new DatabaseDefinition(rme_cnt_card, "revsn_no"));
            rmeCntCard.Add("baservcnzcard", new DatabaseDefinition(rme_cnt_card, "base_revsn_ind"));
            rmeCntCard.Add("orblcnzcard", new DatabaseDefinition(rme_cnt_card, "ordbl_mtrl_stus_id"));
            rmeCntCard.Add("cleicdcnzcard", new DatabaseDefinition(rme_cnt_card, "clei_cd"));
            rmeCntCard.Add("rtpartnocnzcard", new DatabaseDefinition(rme_cnt_card, "rt_part_no"));
            rmeCntCard.Add("rcrdolycnzcard", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeCntCard.Add("mtrlcdcnzcard", new DatabaseDefinition(rme_cnt_card, "mtrl_cd"));

        }

        private void Getrmecard()
        {
            rmeCard = new Dictionary<string, DatabaseDefinition>();


            rmeCard.Add("mtrlidcard", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeCard.Add("mfridcard", new DatabaseDefinition(mtrl, "mfr_id"));
            rmeCard.Add("mfrcdcard", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeCard.Add("rcdolycard", new DatabaseDefinition(mtrl, "rcrds_only_ind"));
            rmeCard.Add("mfrcdcard", new DatabaseDefinition(mfr, "mtrl_cd"));
            rmeCard.Add("rtpartnocard", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeCard.Add("mtrlcatcard", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeCard.Add("mtrldesccard", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeCard.Add("lbridcard", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeCard.Add("featidcard", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeCard.Add("dpthnocard", new DatabaseDefinition(rme_card_mtl, "dpth_no"));
            rmeCard.Add("hgtnocard", new DatabaseDefinition(rme_card_mtl, "hgt_no"));
            rmeCard.Add("wdthnocard", new DatabaseDefinition(rme_card_mtl, "wdth_no"));
            rmeCard.Add("dimuoidcard", new DatabaseDefinition(rme_card_mtl, "dim_uom_id"));
            rmeCard.Add("hetdespcard", new DatabaseDefinition(rme_card, "het_dssptn_no"));
            rmeCard.Add("hetuomcard", new DatabaseDefinition(rme_card, "het_dssptn_uom_id"));
            rmeCard.Add("revnocard", new DatabaseDefinition(rme_card, "revsn_no"));
            rmeCard.Add("baservcard", new DatabaseDefinition(rme_card, "base_revsn_ind"));
            rmeCard.Add("retrvcard", new DatabaseDefinition(rme_card, "ret_revsn_ind"));
            rmeCard.Add("mtrlcdcard", new DatabaseDefinition(rme_card, "mtrl_cd"));
            rmeCard.Add("currvcard", new DatabaseDefinition(rme_card, "curr_revsn_ind"));
            rmeCard.Add("ordblcard", new DatabaseDefinition(rme_card, "ordbl_mtrl_stus_id"));
            rmeCard.Add("cleicdcard", new DatabaseDefinition(rme_card, "clei_cd"));
            rmeCard.Add("elecurnomnocard", new DatabaseDefinition(rme_card, "elc_curr_norm_drn_no"));
            rmeCard.Add("elecnormcard", new DatabaseDefinition(rme_card, "elc_curr_norm_drn_uom_id"));
            rmeCard.Add("elecucard", new DatabaseDefinition(rme_card, "elc_curr_max_drn_no"));
            rmeCard.Add("elecuruomcard", new DatabaseDefinition(rme_card, "elc_curr_max_drn_uom_id"));
            rmeCard.Add("rmemtlcard", new DatabaseDefinition(rme_card_alias, "rme_mtrl_revsn_alias_id"));
            rmeCard.Add("alisvalcard", new DatabaseDefinition(rme_card_alias, "alias_val"));
            rmeCard.Add("crdwtcard", new DatabaseDefinition(rme_card, "card_wt_no"));
            rmeCard.Add("crduomcard", new DatabaseDefinition(rme_card, "card_wt_uom_id"));




        }
        private void Getrmenode()
        {
            rmeNode = new Dictionary<string, DatabaseDefinition>();

            rmeNode.Add("mtrlidnode", new DatabaseDefinition(mtrl, "mtrl_id"));
            rmeNode.Add("mfridnode", new DatabaseDefinition(mfr, "mfr_id"));
            rmeNode.Add("mfrcdnode", new DatabaseDefinition(mfr, "mfr_cd"));
            rmeNode.Add("rtpartnonode", new DatabaseDefinition(mtrl, "rt_part_no"));
            rmeNode.Add("mtrlcatnode", new DatabaseDefinition(mtrl, "mtrl_cat_id"));
            rmeNode.Add("mtrldescnode", new DatabaseDefinition(mtrl, "mtrl_dsc"));
            rmeNode.Add("lbridnode", new DatabaseDefinition(mtrl, "lbr_id"));
            rmeNode.Add("featidnode", new DatabaseDefinition(mtrl, "feat_typ_id"));
            rmeNode.Add("dpthnonode", new DatabaseDefinition(rme_node_mtrl, "dpth_no"));
            rmeNode.Add("hgtnonode", new DatabaseDefinition(rme_node_mtrl, "hgt_no"));
            rmeNode.Add("wdthnonode", new DatabaseDefinition(rme_node_mtrl, "wdth_no"));
            rmeNode.Add("dimuoidnode", new DatabaseDefinition(rme_node_mtrl, "dim_uom_id"));
            rmeNode.Add("hetnonode", new DatabaseDefinition(rme_node, "het_dssptn_no"));
            rmeNode.Add("hetuomnode", new DatabaseDefinition(rme_node, "het_dssptn_uom_id"));
            rmeNode.Add("retrvnonode", new DatabaseDefinition(rme_node, "revsn_no"));
            rmeNode.Add("baservnode", new DatabaseDefinition(rme_node, "base_revsn_ind"));
            rmeNode.Add("currvnode", new DatabaseDefinition(rme_node, "curr_revsn_ind"));
            rmeNode.Add("retvnode", new DatabaseDefinition(rme_node, "ret_revsn_ind"));
            rmeNode.Add("ordblnode", new DatabaseDefinition(rme_node, "ordbl_mtrl_stus_id"));
            rmeNode.Add("cleicdnode", new DatabaseDefinition(rme_node, "clei_cd"));
            rmeNode.Add("plndnode", new DatabaseDefinition(rme_node, "plnd_het_gntn_no"));
            rmeNode.Add("pnduomnode", new DatabaseDefinition(rme_node, "plnd_het_gntn_uom_id"));
            rmeNode.Add("nodewtnode", new DatabaseDefinition(rme_node, "node_wt_no"));
            rmeNode.Add("nodewtuomnode", new DatabaseDefinition(rme_node, "node_wt_uom_id"));
            rmeNode.Add("mtrlcdnode", new DatabaseDefinition(rme_node, "mtrl_cd"));
            rmeNode.Add("rcdolynode", new DatabaseDefinition(mtrl, "rcrds_only_ind"));

        }


        private void GetStagingPartsDefinition()
        {
            stagingParts = new Dictionary<string, DatabaseDefinition>();


            stagingParts.Add("mfg_part_no", new DatabaseDefinition(NUP_MCSH_TABLE, "MFG_PART_NO"));
            stagingParts.Add("heci", new DatabaseDefinition(NUP_MCSH_TABLE, "HECI"));
            stagingParts.Add("clmc", new DatabaseDefinition(NUP_MCSH_TABLE, "MANUFACTURER"));
            stagingParts.Add("materialcode", new DatabaseDefinition(NUP_MCSH_TABLE, "MTL_CD"));
            stagingParts.Add("item_status", new DatabaseDefinition(NUP_MCSH_TABLE, "ITEM_STATUS"));
            stagingParts.Add("mtl_grp", new DatabaseDefinition(NUP_MCSH_TABLE, "MTL_GRP"));
            stagingParts.Add("icc_cd", new DatabaseDefinition(NUP_MCSH_TABLE, "ICC_CD"));
            stagingParts.Add("uom", new DatabaseDefinition(NUP_MCSH_TABLE, "UOM"));
            stagingParts.Add("mvg_avg_price_amt", new DatabaseDefinition(NUP_MCSH_TABLE, "MVG_AVG_PRICE_AMT"));
            stagingParts.Add("mtl_desc", new DatabaseDefinition(NUP_MCSH_TABLE, "MTL_DESC"));
            stagingParts.Add("po_text", new DatabaseDefinition(NUP_MCSH_TABLE, "PO_TEXT"));
            stagingParts.Add("hazard_ind", new DatabaseDefinition(NUP_MCSH_TABLE, "HAZARD_IND"));
            stagingParts.Add("aic_cd", new DatabaseDefinition(NUP_MCSH_TABLE, "AIC_CD"));
            stagingParts.Add("pland_del_tm", new DatabaseDefinition(NUP_MCSH_TABLE, "PLAND_DEL_TM"));
            stagingParts.Add("last_chg_dt", new DatabaseDefinition(NUP_MCSH_TABLE, "LAST_CHG_DT"));
            stagingParts.Add("mfg_name", new DatabaseDefinition(NUP_MIS_TABLE, "MFG_NM"));




        }


    }

}