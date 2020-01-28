using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class AssemblyUnitType
    {
        private Dictionary<string, DatabaseDefinition> laborid = null;
        private Dictionary<string, DatabaseDefinition> laboridMtrlClsfn = null;
        private Dictionary<string, DatabaseDefinition> laboridAUnit = null;
        private Dictionary<string, DatabaseDefinition> aUnit = null;
        private Dictionary<string, DatabaseDefinition> calculationOperations = null;
        private Dictionary<string, DatabaseDefinition> aUnitslaboridpu = null;
        private Dictionary<string, DatabaseDefinition> fireworksNm = null;
        public static string GET_LABOR_ID = "get_labor_id";
        public static string GET_MTRL_CLSFCTN_FOR_LBR_ID = "get_mtrl_clsfctn_for_lbr_id";
        public static string GET_ASSEMBLY_UNITS_FOR_LBR_ID = "get_assembly_units_for_lbr_id";
        public static string SEARCH_ASSEMBLY_UNITS = "search_assembly_units";
        public static string GET_OPERATIONS_FOR_CALCULATION = "get_operations_for_calculation";
        public static string GET_AU_FOR_LBR_ID_POPUP = "get_au_for_lbr_id_popup";
        public static string SEARCH_FIREWORKS = "search_fireworks";


        public Dictionary<string, DatabaseDefinition> LaborId
        {
            get
            {
                if (laborid == null)
                    GetLaborIdAttributes();

                return laborid;
            }
        }

        public Dictionary<string, DatabaseDefinition> LaborIdMtrlclsfctn
        {
            get
            {
                if (laboridMtrlClsfn == null)
                    GetLaborIdMtrlClsfctn();

                return laboridMtrlClsfn;
            }
        }

        private void GetLaborIdAttributes()
        {
            laborid = new Dictionary<string, DatabaseDefinition>();

            laborid.Add("lbrtitle", new DatabaseDefinition(GET_LABOR_ID, "LBR_TITLE_NM"));
            laborid.Add("lbrdesc", new DatabaseDefinition(GET_LABOR_ID, "LBR_DSC"));
        }

        private void GetLaborIdMtrlClsfctn()
        {
            laboridMtrlClsfn = new Dictionary<string, DatabaseDefinition>();

            laboridMtrlClsfn.Add("lbrclsfctnid", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "lbr_clsfctn_id"));
            laboridMtrlClsfn.Add("selected", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "selected"));
            laboridMtrlClsfn.Add("mtrlcattyp", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "mtrl_cat_typ"));
            laboridMtrlClsfn.Add("feattyp", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "feat_typ"));
            laboridMtrlClsfn.Add("cabltyp", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "cabl_typ"));
            laboridMtrlClsfn.Add("mtrlclsfctnid", new DatabaseDefinition(GET_MTRL_CLSFCTN_FOR_LBR_ID, "mtrl_clsfctn_id"));

        }

        public Dictionary<string, DatabaseDefinition> LaborIdAUnit
        {
            get
            {
                if (laboridAUnit == null)
                    GetLaborIdAUnit();

                return laboridAUnit;
            }
        }

        private void GetLaborIdAUnit()
        {
            laboridAUnit = new Dictionary<string, DatabaseDefinition>();

            laboridAUnit.Add("aunm", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "au_nm"));
            laboridAUnit.Add("calcnm", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "calc_nm"));
            laboridAUnit.Add("uomnm", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "uom_nm"));
            laboridAUnit.Add("retind", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "ret_ind"));
            laboridAUnit.Add("mtplrno", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "mtplr_no"));
            laboridAUnit.Add("alternative", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "alternative"));
            laboridAUnit.Add("alttoau", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "alt_to_au"));
            laboridAUnit.Add("lbridauid", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "lbr_id_au_id"));
            laboridAUnit.Add("lbridaualtid", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "lbr_id_au_alt_id"));
            laboridAUnit.Add("isselected", new DatabaseDefinition(GET_ASSEMBLY_UNITS_FOR_LBR_ID, "is_selected"));
            
        }

        public Dictionary<string, DatabaseDefinition> AUnits
        {
            get
            {
                if (aUnit == null)
                    GetAUnit();

                return aUnit;
            }
        }

        private void GetAUnit()
        {
            aUnit = new Dictionary<string, DatabaseDefinition>();

            aUnit.Add("auid", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "au_id"));
            aUnit.Add("aunm", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "au_nm"));
            aUnit.Add("calcnm", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "calc_nm"));
            aUnit.Add("aucalcid", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "au_calc_id"));
            aUnit.Add("uomnm", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "uom_nm"));
            aUnit.Add("retind", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "ret_ind"));
            aUnit.Add("mtplrno", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "mtplr_no"));
            aUnit.Add("alternative", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "alternative"));
            aUnit.Add("alttoau", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "alt_to_au"));
            aUnit.Add("default", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "default"));
            aUnit.Add("alttoauid", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "alt_to_au_id"));
            aUnit.Add("lbridauid", new DatabaseDefinition(SEARCH_ASSEMBLY_UNITS, "lbr_id_au_id"));


        }

        //SELECT ac.au_calc_id, aco.au_calc_op_id, aco.ordr_of_op_no, ao.op_nm, ao.au_op_id, 'Y' AS isSelected
        public Dictionary<string, DatabaseDefinition> CalculationOperations
        {
            get
            {
                if (calculationOperations == null)
                    GetCalculationOperations();

                return calculationOperations;
            }
        }

        private void GetCalculationOperations()
        {
            calculationOperations = new Dictionary<string, DatabaseDefinition>();

            calculationOperations.Add("auCalcId", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "au_calc_id"));
            calculationOperations.Add("auCalcOpId", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "au_calc_op_id"));
            calculationOperations.Add("ordrOfOprtn", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "ordr_of_op_no"));
            calculationOperations.Add("opNm", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "op_nm"));
            calculationOperations.Add("auOpId", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "au_op_id"));
            calculationOperations.Add("isSelected", new DatabaseDefinition(GET_OPERATIONS_FOR_CALCULATION, "isSelected"));
        }
        public Dictionary<string, DatabaseDefinition> AUnitforLaborIDPU
        {
            get
            {
                if (aUnitslaboridpu == null)
                    GetAUnitsforLaborIdPu();

                return aUnitslaboridpu;
            }
        }

        private void GetAUnitsforLaborIdPu()
        {
            aUnitslaboridpu = new Dictionary<string, DatabaseDefinition>();
            aUnitslaboridpu.Add("lbridauid", new DatabaseDefinition(GET_AU_FOR_LBR_ID_POPUP, "lbr_id_au_id"));
            aUnitslaboridpu.Add("auid", new DatabaseDefinition(GET_AU_FOR_LBR_ID_POPUP, "au_id"));
            aUnitslaboridpu.Add("aunm", new DatabaseDefinition(GET_AU_FOR_LBR_ID_POPUP, "au_nm"));
        }
        public Dictionary<string, DatabaseDefinition> FireworksName
        {
            get
            {
                if (fireworksNm == null)
                    GetFireworksName();

                return fireworksNm;
            }
        }

        private void GetFireworksName()
        {
            fireworksNm = new Dictionary<string, DatabaseDefinition>();
            fireworksNm.Add("assemblyunitsk", new DatabaseDefinition(SEARCH_FIREWORKS, "assembly_unit_sk"));
            fireworksNm.Add("description", new DatabaseDefinition(SEARCH_FIREWORKS, "description"));
            fireworksNm.Add("dateexpired", new DatabaseDefinition(SEARCH_FIREWORKS, "date_expired"));
            fireworksNm.Add("section", new DatabaseDefinition(SEARCH_FIREWORKS, "section"));
        }
    }
}