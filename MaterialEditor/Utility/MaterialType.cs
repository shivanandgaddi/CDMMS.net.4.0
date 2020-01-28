using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public enum SOURCE_SYSTEM { SAP, LOSDB, NDS, RECORDS_ONLY };

    public class MaterialType
    {
        public static readonly string MTL_CTGRY_PART_MATERIAL_TYPE = "Spec Part";
        public static readonly string PRODUCT_ID = "PRODUCT_ID";
        public static readonly string MFG_PART_NO = "MFG_PART_NO";
        public static readonly string MFG_ID = "MFG_ID";
        public static readonly string APCL_CD = "APCL_CD";
        public static readonly string UNIT_OF_MSRMT = "UNIT_OF_MSRMT";
        public static readonly string CATEGORY_ID = "CATEGORY_ID";
        public static readonly string ITEM_DESC = "ITEM_DESC";
        public static readonly string MFG_DESC = "MFG_DESC";
        public static readonly string ICC_CD = "ICC_CD";
        public static readonly string MIC_CD = "MIC_CD";
        public static readonly string ADDITIONAL_DESC = "ADDITIONAL_DESC";
        public static readonly string AVG_PRICE_AMT = "AVG_PRICE_AMT";
        public static readonly string HAZARD_IND = "HAZARD_IND";
        public static readonly string NSTOCK_DEL_INT_DUR = "NSTOCK_DEL_INT_DUR";
        public static readonly string ZHEIGHT = "ZHEIGHT";
        public static readonly string ZWIDTH = "ZWIDTH";
        public static readonly string ZDEPTH = "ZDEPTH'";
        public static readonly string HECI = "HECI";
        public static readonly string ALT_UOM = "ALT_UOM";
        public static readonly string MTL_TYPE = "MTL_TYPE";
        public static readonly string CONV_RATE1 = "CONV_RATE1";
        public static readonly string CONV_RATE2 = "CONV_RATE2";
        public static readonly string DT_CREATED = "DT_CREATED";
        public static readonly string ITEM_CURRENT_STATUS = "ITEM_CURRENT_STATUS";
        public static readonly string ItemStatus = "ItemStatus";

        //MTRL Tables
        public static readonly string XTNL_DPTH_NO = "XTNL_DPTH_NO";
        public static readonly string XTNL_HGT_NO = "XTNL_HGT_NO";
        public static readonly string XTNL_WDTH_NO = "XTNL_WDTH_NO";
        public static readonly string XTNL_DIM_UOM_ID = "XTNL_DIM_UOM_ID";
        public static readonly string CAB_IND = "CAB_IND";
        private static string SAP_TABLE = "MTL_ITEM_SAP";
        private static string LOSDB_EQUIPMENT_TABLE = "IES_EQPT";
        private static string LOSDB_ASSET_TABLE = "IES_ASSET";
        private static string LOSDB_INVENTORY_TABLE = "IES_INVNTRY";
        private static string LOSDB_VENDOR_TABLE = "IES_VNDR_CD";
        private static string RME_BAY_MTRL_TABLE = "RME_XXX_MTRL";
        private static string RME_CNCTRZD_CABL_TABLE = "RME_CNCTRZD_CABL";
        private Dictionary<string, DatabaseDefinition> sapMaterial = null;
        private Dictionary<string, DatabaseDefinition> losdbMaterial = null;
        private Dictionary<string, DatabaseDefinition> iesEquipment = null;
        private Dictionary<string, DatabaseDefinition> iesInventory = null;
        private Dictionary<string, DatabaseDefinition> iesCompatibleClei = null;
        private Dictionary<string, DatabaseDefinition> iesElectrical = null;
        private Dictionary<string, DatabaseDefinition> iesMainExtension = null;

        private static string ROTTN_ANGL_TABLE = "ROTTN_ANGL_DGR_NO";

        public static string SourceSystem(SOURCE_SYSTEM system)
        {
            return System.Enum.GetName(typeof(SOURCE_SYSTEM), system);
        }

        public Dictionary<string, DatabaseDefinition> SAPMaterial
        {
            get
            {
                if (sapMaterial == null)
                    GetSAPMaterialDefinition();

                return sapMaterial;
            }
        }

        public Dictionary<string, DatabaseDefinition> LOSDBMaterial
        {
            get
            {
                if (losdbMaterial == null)
                    GetLOSDBMaterialDefinition();

                return losdbMaterial;
            }
        }

        public Dictionary<string, DatabaseDefinition> IESEquipment
        {
            get
            {
                if (iesEquipment == null)
                    GetIESEquipmentDefinition();

                return iesEquipment;
            }
        }

        public Dictionary<string, DatabaseDefinition> IESInventory
        {
            get
            {
                if (iesInventory == null)
                    GetIESInventoryDefinition();

                return iesInventory;
            }
        }

        public Dictionary<string, DatabaseDefinition> IESCompatibleCLEI
        {
            get
            {
                if (iesCompatibleClei == null)
                    GetIESCompatibleCLEIDefinition();

                return iesCompatibleClei;
            }
        }

        public Dictionary<string, DatabaseDefinition> IESElectrical
        {
            get
            {
                if (iesElectrical == null)
                    GetIESElectricalDefinition();

                return iesElectrical;
            }
        }

        public Dictionary<string, DatabaseDefinition> IESMainExtension
        {
            get
            {
                if (iesMainExtension == null)
                    GetIESMainExtensionDefinition();

                return iesMainExtension;
            }
        }

        public static Dictionary<string, DatabaseDefinition> RmeBayMtrl
        {
            get
            {
                Dictionary<string, DatabaseDefinition> mtrl = new Dictionary<string, DatabaseDefinition>();

                mtrl.Add(JSON.Dpth, new DatabaseDefinition(RME_BAY_MTRL_TABLE, XTNL_DPTH_NO));
                mtrl.Add(JSON.Hght, new DatabaseDefinition(RME_BAY_MTRL_TABLE, XTNL_HGT_NO));
                mtrl.Add(JSON.Wdth, new DatabaseDefinition(RME_BAY_MTRL_TABLE, XTNL_WDTH_NO));
                mtrl.Add(JSON.UOM, new DatabaseDefinition(RME_BAY_MTRL_TABLE, XTNL_DIM_UOM_ID));
                mtrl.Add(JSON.CabInd, new DatabaseDefinition(RME_BAY_MTRL_TABLE, CAB_IND));

                return mtrl;
            }
        }

        public static StringCollection GetSAPMaterialAttributes()
        {
            StringCollection attributes = new StringCollection();

            attributes.Add(JSON.PrdctId);
            attributes.Add(JSON.PrtNo);
            attributes.Add(JSON.Mfg);
            attributes.Add(JSON.Apcl);
            attributes.Add(JSON.UOM);
            attributes.Add(JSON.CtgryId);
            attributes.Add(JSON.ItmDesc);
            attributes.Add(JSON.MfgDesc);
            attributes.Add(JSON.ICC);
            attributes.Add(JSON.MIC);
            attributes.Add(JSON.AddtlDesc);
            attributes.Add(JSON.AvgPrc);
            attributes.Add(JSON.HzrdInd);
            attributes.Add(JSON.Stck);
            attributes.Add(JSON.Hght);
            attributes.Add(JSON.Wdth);
            attributes.Add(JSON.Dpth);
            attributes.Add(JSON.HECI);
            attributes.Add(JSON.AltUOM);
            attributes.Add(JSON.MtlType);
            attributes.Add(JSON.ConvRt1);
            attributes.Add(JSON.ConvRt2);
            attributes.Add(JSON.DtCreated);
            attributes.Add(ItemStatus);

            return attributes;
        }

        public static StringCollection GetRecordOnlyAttributes()
        {
            StringCollection attributes = new StringCollection();

            attributes.Add(JSON.SpecNm);
            attributes.Add(JSON.SpecId);
            attributes.Add(JSON.SetLgth);
            attributes.Add(JSON.Apcl);
            attributes.Add(JSON.UOM);
            attributes.Add(JSON.SetLgthUom);
            attributes.Add(JSON.Stts);
            attributes.Add(JSON.AmpsDrn);
            attributes.Add(JSON.NumMtgSpcs);
            attributes.Add(JSON.MtgPltSz);
            attributes.Add(JSON.MxEqpPos);
            attributes.Add(JSON.Gge);
            attributes.Add(JSON.HzrdInd);
            attributes.Add(JSON.GgeUnt);
            attributes.Add(JSON.Hght);
            attributes.Add(JSON.Wdth);
            attributes.Add(JSON.Dpth);
            attributes.Add(JSON.EqpWght);
            attributes.Add(JSON.AccntCd);
            attributes.Add(JSON.MtlType);
            attributes.Add(JSON.PrdTyp);
            attributes.Add(JSON.EqptCls);
            attributes.Add(JSON.LctnPosInd);
            attributes.Add(JSON.PrtNbrTypCd);
            attributes.Add(JSON.LctnPosInd);
            attributes.Add(JSON.MntPosHght);
            attributes.Add(JSON.IntrlHght);
            attributes.Add(JSON.IntrlDpth);
            attributes.Add(JSON.IntrlWdth);
            attributes.Add(JSON.PosSchm);
            attributes.Add(JSON.LstUid);            
            attributes.Add(JSON.LstDt);

            return attributes;
        }

        private void GetSAPMaterialDefinition()
        {
            sapMaterial = new Dictionary<string, DatabaseDefinition>();

            sapMaterial.Add(JSON.PrdctId, new DatabaseDefinition(SAP_TABLE, PRODUCT_ID));
            sapMaterial.Add(JSON.PrtNo, new DatabaseDefinition(SAP_TABLE, MFG_PART_NO));
            sapMaterial.Add(JSON.Mfg, new DatabaseDefinition(SAP_TABLE, MFG_ID));
            sapMaterial.Add(JSON.Apcl, new DatabaseDefinition(SAP_TABLE, APCL_CD));
            sapMaterial.Add(JSON.UOM, new DatabaseDefinition(SAP_TABLE, UNIT_OF_MSRMT));
            sapMaterial.Add(JSON.CtgryId, new DatabaseDefinition(SAP_TABLE, CATEGORY_ID));
            sapMaterial.Add(JSON.ItmDesc, new DatabaseDefinition(SAP_TABLE, ITEM_DESC));
            sapMaterial.Add(JSON.MfgDesc, new DatabaseDefinition(SAP_TABLE, MFG_DESC));
            sapMaterial.Add(JSON.ICC, new DatabaseDefinition(SAP_TABLE, ICC_CD));
            sapMaterial.Add(JSON.MIC, new DatabaseDefinition(SAP_TABLE, MIC_CD));
            sapMaterial.Add(JSON.AddtlDesc, new DatabaseDefinition(SAP_TABLE, ADDITIONAL_DESC));
            sapMaterial.Add(JSON.AvgPrc, new DatabaseDefinition(SAP_TABLE, AVG_PRICE_AMT, true));
            sapMaterial.Add(JSON.HzrdInd, new DatabaseDefinition(SAP_TABLE, HAZARD_IND));
            sapMaterial.Add(JSON.Stck, new DatabaseDefinition(SAP_TABLE, NSTOCK_DEL_INT_DUR, true));
            sapMaterial.Add(JSON.Hght, new DatabaseDefinition(SAP_TABLE, ZHEIGHT, true));
            sapMaterial.Add(JSON.Wdth, new DatabaseDefinition(SAP_TABLE, ZWIDTH, true));
            sapMaterial.Add(JSON.Dpth, new DatabaseDefinition(SAP_TABLE, ZDEPTH, true));
            sapMaterial.Add(JSON.HECI, new DatabaseDefinition(SAP_TABLE, HECI));
            sapMaterial.Add(JSON.AltUOM, new DatabaseDefinition(SAP_TABLE, ALT_UOM));
            sapMaterial.Add(JSON.MtlType, new DatabaseDefinition(SAP_TABLE, MTL_TYPE));
            sapMaterial.Add(JSON.ConvRt1, new DatabaseDefinition(SAP_TABLE, CONV_RATE1, true));
            sapMaterial.Add(JSON.ConvRt2, new DatabaseDefinition(SAP_TABLE, CONV_RATE2, true));
            sapMaterial.Add(JSON.DtCreated, new DatabaseDefinition(SAP_TABLE, DT_CREATED));
            sapMaterial.Add(ItemStatus, new DatabaseDefinition(SAP_TABLE, ITEM_CURRENT_STATUS));
        }

        private void GetLOSDBMaterialDefinition()
        {
            losdbMaterial = new Dictionary<string, DatabaseDefinition>();

            losdbMaterial.Add("PrdctId", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "PROD_ID", true));
            losdbMaterial.Add("Vndr", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "VNDR_CD"));
            losdbMaterial.Add("PrtNo", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "PART_NO"));
            losdbMaterial.Add("Desc", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "DESCR"));
            losdbMaterial.Add("SysNm1", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "SYS_NM_1"));
            losdbMaterial.Add("SysNm2", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "SYS_NM_2"));
            losdbMaterial.Add("SysNm3", new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "SYS_NM_3"));
            losdbMaterial.Add("AsstId", new DatabaseDefinition(LOSDB_ASSET_TABLE, "ASSET_ID", true));
            losdbMaterial.Add("CPR", new DatabaseDefinition(LOSDB_ASSET_TABLE, "CPR"));
            losdbMaterial.Add("CPRNm", new DatabaseDefinition(LOSDB_ASSET_TABLE, "CPR_BLCK_NM"));
            losdbMaterial.Add("Acct", new DatabaseDefinition(LOSDB_ASSET_TABLE, "ACCT"));
            losdbMaterial.Add("FRC", new DatabaseDefinition(LOSDB_ASSET_TABLE, "FRC"));
            losdbMaterial.Add("ECN", new DatabaseDefinition(LOSDB_ASSET_TABLE, "ECN"));
            losdbMaterial.Add("CLEI", new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "CLEI_CD"));
            losdbMaterial.Add("EqptCtlgItm", new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "EQPT_CTLG_ITM_GR"));
            losdbMaterial.Add("VndrNm", new DatabaseDefinition(LOSDB_VENDOR_TABLE, "VNDR_NM"));
        }

        private void GetIESEquipmentDefinition()
        {
            iesEquipment = new Dictionary<string, DatabaseDefinition>();

            iesEquipment.Add(JSON.ProdId, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "PROD_ID", true));
            iesEquipment.Add(JSON.Mfg, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "VNDR_CD"));
            iesEquipment.Add(JSON.MfgNm, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "VNDR_NM"));
            iesEquipment.Add(JSON.Drwg, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "DRWG"));
            iesEquipment.Add(JSON.DrwgIss, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "DRWG_ISS"));
            iesEquipment.Add(JSON.PrtNo, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "PART_NO"));
            iesEquipment.Add(JSON.EqptDscr, new DatabaseDefinition(LOSDB_EQUIPMENT_TABLE, "DESCR"));
        }

        private void GetIESInventoryDefinition()
        {
            iesInventory = new Dictionary<string, DatabaseDefinition>();

            iesInventory.Add(JSON.ProdId, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "PROD_ID", true));
            iesInventory.Add(JSON.EqptCtlgItmId, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "EQPT_CTLG_ITEM_ID"));
            iesInventory.Add(JSON.CLEI, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "CLEI_CD"));
            iesInventory.Add(JSON.LsOrSrs, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "LS_OR_SRS"));
            iesInventory.Add(JSON.OrdStat, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ORD_STAT"));
            iesInventory.Add(JSON.CtlgDesc, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "DESCR"));
            iesInventory.Add(JSON.OrdgCd, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ORDG_CD"));
        }

        private void GetIESCompatibleCLEIDefinition()
        {
            iesCompatibleClei = new Dictionary<string, DatabaseDefinition>();

            iesCompatibleClei.Add(JSON.CmpCleiKey, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "COMP_CLEI_KEY", true));
            iesCompatibleClei.Add(JSON.CLEISvn, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "COMPATIBLEEQUIPMENTCLEI7"));
        }

        private void GetIESElectricalDefinition()
        {
            iesElectrical = new Dictionary<string, DatabaseDefinition>();

            iesElectrical.Add(JSON.VltgMinUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTVOLTAGEFROM_UNIT"));
            iesElectrical.Add(JSON.VltgMin, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTVOLTAGEFROM_VALUE", true));
            iesElectrical.Add(JSON.VltgMxUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTVOLTAGETO_UNIT"));
            iesElectrical.Add(JSON.VltgMx, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTVOLTAGETO_VALUE", true));
            iesElectrical.Add(JSON.ElcCurrDrnNrmUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTCURRENTFROM_UNIT"));
            iesElectrical.Add(JSON.ElcCurrDrnNrm, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTCURRENTFROM_VALUE", true));
            iesElectrical.Add(JSON.ElcCurrDrnMxUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTCURRENTTO_UNIT"));
            iesElectrical.Add(JSON.ElcCurrDrnMx, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "ER_INPUTCURRENTTO_VALUE", true));
        }

        private void GetIESMainExtensionDefinition()
        {
            iesMainExtension = new Dictionary<string, DatabaseDefinition>();

            iesMainExtension.Add(JSON.RotationAng, new DatabaseDefinition(ROTTN_ANGL_TABLE, "ROTTN_ANGL_DGR_NO"));
            iesMainExtension.Add(JSON.DimUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "HEIGHT_ENGLISH_UNIT"));
            iesMainExtension.Add(JSON.Hght, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "HEIGHT_ENGLISH_VALUE", true));
            //iesMainExtension.Add(JSON.VltgMxUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "WIDTH_ENGLISH_UNIT"));
            iesMainExtension.Add(JSON.Wdth, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "WIDTH_ENGLISH_VALUE", true));
            //iesMainExtension.Add(JSON.ElcCurrDrnNrmUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "DEPTH_ENGLISH_UNIT"));
            iesMainExtension.Add(JSON.Dpth, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "DEPTH_ENGLISH_VALUE", true));
            iesMainExtension.Add(JSON.WghtUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "WEIGHT_ENGLISH_UNIT"));
            iesMainExtension.Add(JSON.Wght, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "WEIGHT_ENGLISH_VALUE", true));
            iesMainExtension.Add(JSON.HtGntnUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "MAXPOWERUSAGE_UNIT"));
            iesMainExtension.Add(JSON.HtGntn, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "MAXPOWERUSAGE_VALUE", true));
            iesMainExtension.Add(JSON.HtDssptnUom, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "MAXHEATDISSIPATION_ENG_UNIT"));
            iesMainExtension.Add(JSON.HtDssptn, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "MAXHEATDISSIPATION_ENG_VALUE", true));
            iesMainExtension.Add(JSON.HazInd, new DatabaseDefinition(LOSDB_INVENTORY_TABLE, "HAZARDOUSMATERIALINDICATOR", true));
        }

        public class JSON
        {
            public const string MaterialItemId = "id";
            public const string OVERRIDE = "OVRRD";
            public const string PrdctId = "PrdctId";
            public const string PrtNo = "PrtNo";
            public const string Mfg = "Mfg";
            public const string MfgNm = "MfgNm";

            public const string MfgId = "MfgId";
            public const string Apcl = "Apcl";
            public const string UOM = "UOM";
            public const string CtgryId = "CtgryId";
            public const string ItmDesc = "ItmDesc";
            public const string MfgDesc = "MfgDesc";
            public const string ICC = "ICC";
            public const string MIC = "MIC";
            public const string AddtlDesc = "AddtlDesc";
            public const string AvgPrc = "AvgPrc";
            public const string HzrdInd = "HzrdInd";
            public const string Stck = "Stck";
            public const string Hght = "Hght";
            public const string Wdth = "Wdth";
            public const string Dpth = "Dpth";
            public const string IntlHghtId = "IntlHghtId";
            public const string IntlWdthId = "IntlWdthId";
            public const string IntlDpthId = "IntlDpthId";
            public const string HECI = "HECI";
            public const string AltUOM = "AltUOM";
            public const string MtlType = "MtlType";
            public const string ConvRt1 = "ConvRt1";
            public const string ConvRt2 = "ConvRt2";
            public const string DtCreated = "DtCreated";
            public const string ItemStatus = "ItemStatus";
            public const string CtlgDesc = "CtlgDesc";
            public const string FtrTyp = "FtrTyp";
            public const string NdsFtrTyp = "NdsFtrTyp";
            public const string SetLgth = "SetLgth";
            public const string SetLgthUom = "SetLgthUom";
            public const string SpecNm = "SpecNm";
            public const string SpecRvsnNm = "SpecRvsnNm";
            public const string SpecId = "SpecId";
            public const string SpecRvsnId = "SpecRvsnId";
            public const string SpecTyp = "SpecTyp";
            public const string SpecPrpgt = "SpecPrpgt";
            public const string HvSpec = "HvSpec";
            public const string Stts = "Stts";
            public const string AccntCd = "AccntCd";
            public const string AmpsDrn = "AmpsDrn";
            public const string Cpr = "Cpr";
            public const string EqpWght = "EqpWght";
            public const string EqptCls = "EqptCls";
            public const string FrmNm = "FrmNm";
            public const string Gge = "Gge";
            public const string GgeUnt = "GgeUnt";
            public const string KpOrJnk = "KpOrJnk";
            public const string LbrId = "LbrId";
            public const string LbrIdCoe = "LbrIdCoe";
            public const string LctnPosInd = "LctnPosInd";
            public const string Lgth = "Lgth";
            public const string MntPosHght = "MntPosHght";
            public const string MtgPltSz = "MtgPltSz";
            public const string MxEqpPos = "MxEqpPos";
            public const string NdNbr = "NdNbr";
            public const string Nebs = "Nebs";
            public const string NumMtgSpcs = "NumMtgSpcs";
            public const string Ordrblty = "Ordrblty";
            public const string PosSchm = "PosSchm";
            public const string PrdTyp = "PrdTyp";
            public const string PrtNbrTypCd = "PrtNbrTypCd";
            public const string PsbNbr = "PsbNbr";
            public const string VltDsc = "VltDsc";
            public const string Vltge = "Vltge";
            public const string MtrlId = "MtrlId";
            public const string MtlCtgry = "MtlCtgry";
            public const string VarCbl = "VarCbl";
            public const string VarCblId = "VarCblId";
            public const string CabInd = "CabInd";
            public const string Rvsn = "Rvsn";
            public const string BaseRvsnInd = "BaseRvsnInd";
            public const string CurrRvsnInd = "CurrRvsnInd";
            public const string RetRvsnInd = "RetRvsnInd";
            public const string OrdblId = "OrdblId";
            public const string CblTypId = "CblTypId";
            public const string RtPrtNbr = "RtPrtNbr";
            public const string RplcdByMtlItmId = "RplcdByMtlItmId";
            public const string RplcdByPrtNbr = "RplcdByPrtNbr";
            public const string RplcsMtlItmId = "RplcsMtlItmId";
            public const string RplcsPrtNbr = "RplcsPrtNbr";
            public const string HasRvsns = "HasRvsns";
            public const string HtDssptn = "HtDssptn";
            public const string HtDssptnUom = "HtDssptnUom";
            public const string HazInd = "HazInd";
            public const string ElcCurrDrnNrm = "ElcCurrDrnNrm";
            public const string ElcCurrDrnNrmUom = "ElcCurrDrnNrmUom";
            public const string ElcCurrDrnMx = "ElcCurrDrnMx";
            public const string ElcCurrDrnMxUom = "ElcCurrDrnMxUom";
            public const string Wght = "Wght";
            public const string HtGntn = "HtGntn";
            public const string HtGntnUom = "HtGntnUom";
            public const string CbntInd = "CbntInd";
            public const string RO = "RO";
            public const string ROPblshd = "ROPblshd";
            public const string CLEI = "CLEI";
            public const string PlgInRlTyp = "PlgInRlTyp";
            public const string CblInd = "CblInd";
            public const string DimUom = "DimUom";
            public const string ElcCurrUom = "ElcCurrUom";
            public const string ElcPwrUom = "ElcPwrUom";
            public const string WghtUom = "WghtUom";
            public const string RetInd = "RetInd";
            public const string PrpgtnInd = "PrpgtnInd";
            public const string CmpltnInd = "CmpltnInd";
            public const string SpecInitInd = "SpecInitInd";
            public const string RvsnSpecId = "RvsnSpecId";
            public const string IntrlHght = "IntrlHght";
            public const string IntrlDpth = "IntrlDpth";
            public const string IntrlWdth = "IntrlWdth";
            public const string LstUid = "LstUid";
            public const string LstDt = "LstDt";

            //LOSDB
            public const string ProdId = "ProdId";
            public const string Drwg = "Drwg";
            public const string DrwgIss = "DrwgIss";
            public const string EqptCtlgItmId = "EqptCtlgItmId";
            public const string LsOrSrs = "LsOrSrs";
            public const string EqptDscr = "EqptDscr";
            public const string OrdStat = "OrdStat";
            public const string OrdgCd = "OrdgCd";
            public const string CmpCleiKey = "CmpCleiKey";
            public const string CLEISvn = "CLEISvn";
            public const string VltgMin = "VltgMin";
            public const string VltgMinUom = "VltgMinUom";
            public const string VltgMx = "VltgMx";
            public const string VltgMxUom = "VltgMxUom";

            //Added for Has slots pop window in card specification for Rotation Angle.
            public const string RotationAng = "RotationAng";
        }
    }

    public class DatabaseDefinition
    {
        private DatabaseDefinition()
        {
        }

        public DatabaseDefinition(string table, string column)
        {
            Table = table;
            Column = column;
            IsNumber = false;
        }

        public DatabaseDefinition(string table, string column, bool isNumber)
        {
            Table = table;
            Column = column;
            IsNumber = isNumber;
        }

        public string Table
        {
            get;
            set;
        }

        public string Column
        {
            get;
            set;
        }

        public bool IsNumber
        {
            get;
            set;
        }
    }

    public class PartialMatch
    {
        public bool CheckedRow { get; set; }
        public long MaterialItemID { get; set; }
        public long MaterialID { get; set; }
        public string RootPartNumber { get; set; }
        public string MfgPartNumber { get; set; }
        public string CLMC { get; set; }
        public string ProductID { get; set; }
        public string Description { get; set; }
        public string MaterialCategory { get; set; }
        public string FeatureType { get; set; }
        public string RevisionNumber { get; set; }
        public string CDMMSRevisionTableName { get; set; }
        public string Message { get; set; }
        public string HECI { get; set; }
        public string CLEI { get; set; }
    }
}