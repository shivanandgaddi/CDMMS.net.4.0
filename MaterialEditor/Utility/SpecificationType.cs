using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class SpecificationType
    {
        public enum Type {BAY, BAY_EXTENDER, BAY_INTERNAL, BAY_INTERNAL_DEPTH, BAY_INTERNAL_HEIGHT, BAY_INTERNAL_WIDTH, CARD, CARD_SLOT_CONSUMPTION, NODE, NOT_SET, 
                          PLUG_IN, PLUG_IN_ROLE_TYPE, ROLE, SHELF, SLOT,PORT };

        public class JSON
        {
            public const string SpecificationId = "id";
            public const string NDSSpecificationId = "NdsSpecId";
            public const string Name = "Nm";
            public const string Desc = "Desc";
            public const string Typ = "Typ";
            public const string DimUom = "DimUom";
            public const string MntngPosOfst = "MntngPosOfst";
            public const string StrtLbl = "StrtLbl";
            public const string WllMnt = "WllMnt";
            public const string StrghtThru = "StrghtThru";
            public const string MdlNo = "MdlNo";
            public const string MidPln = "MidPln";
            public const string DualSd = "DualSd";
            public const string RvsnNm = "RvsnNm";
            public const string RvsnId = "RvsnId";
            public const string Cmplt = "Cmplt";
            public const string Prpgtd = "Prpgtd";
            public const string Dltd = "Dltd";
            public const string MxWght = "MxWght";
            public const string MxWghtUom = "MxWghtUom";
            public const string RO = "RO";
            public const string BayRlTypId = "BayRlTypId";
            public const string BayUseTypId= "BayUseTypId";
            public const string MtrlId = "MtrlId";
            public const string PrdctId = "PrdctId";
            public const string PrtNo = "PrtNo";
            public const string Mfr = "Mfr";
            public const string NdsMfr = "NdsMfr";
            public const string MtlItmId = "MtlItmId";
            public const string Gnrc = "Gnrc";
            public const string XtnlDpth = "XtnlDpth";
            public const string XtnlHgt = "XtnlHgt";
            public const string XtnlWdth = "XtnlWdth";
            public const string XtnlDimUom = "XtnlDimUom";
            public const string BayIntlId = "BayItnlId";
            public const string BayIntlDsc = "BayItnlDsc";
            public const string BayIntlDpthId = "BayItnlDpthId";
            public const string BayIntlWdthId = "BayItnlWdthId";
            public const string BayIntlHghtId = "BayItnlHghtId";
            public const string BayIntlDpth = "BayItnlDpth";
            public const string BayIntlWdth = "BayItnlWdth";
            public const string BayIntlHght = "BayItnlHght";
            public const string BayIntlDpthUom = "BayItnlDpthUom";
            public const string BayIntlWdthUom = "BayItnlWdthUom";
            public const string BayIntlHghtUom = "BayItnlHghtUom";
            public const string BayIntlDpthUomId = "BayItnlDpthUomId";
            public const string BayIntlWdthUomId = "BayItnlWdthUomId";
            public const string BayIntlHghtUomId = "BayItnlHghtUomId";
            public const string BayIntlDpthLst = "BayItnlDpthLst";
            public const string BayIntlWdthLst = "BayItnlWdthLst";
            public const string BayIntlHghtLst = "BayItnlHghtLst";
            public const string BayIntlLst = "BayItnlLst";
            public const string BayIntlUseTypId = "BayIntlUseTypId";           

            public const string MntngPosQty = "MntngPosQty";
            public const string MntngPosDistId = "MntngPosDistId";
            public const string MntngPosDist = "MntngPosDist";
            public const string BayIntlWllMnt = "BayIntlWllMnt";
            public const string BayIntlStrghtThru = "BayIntlStrghtThru";
            public const string BayIntlMidPln = "BayIntlMidPln";
            public const string BayIntlCmplt = "BayIntlCmplt";
            public const string BayIntlPrpgtd = "BayIntlPrpgtd";
            public const string BayIntlDltd = "BayIntlDltd";
            public const string Dpth = "Dpth";
            public const string DpthUom = "DpthUom";
            public const string Wdth = "Wdth";
            public const string WdthUom = "WdthUom";
            public const string Hght = "Hght";
            public const string Nmnl = "Nmnl";
            public const string PlgInRlTypLst = "PlgInRlTypLst";
            public const string PlgInRlTyp = "PlgInRlTyp";
            public const string BiDrctnl = "BiDrctnl";
            public const string PlgInRlTypDsc = "PlgInRlTypDsc";
            public const string PlgInRlTypNm = "PlgInRlTypNm";
            public const string CnctrHgt = "CnctrHgt";
            public const string CnctrWdth = "CnctrWdth";
            public const string CnctrUom = "CnctrUom";
            public const string MxLiteXmsn = "MxLiteXmsn";
            public const string DistUom = "DistUom";
            public const string VarWvlgth = "VarWvlgth";
            public const string FrmFctr = "FrmFctr";
            public const string FnctnCd = "FnctnCd";
            public const string LoTmp = "LoTmp";
            public const string HiTmp = "HiTmp";
            public const string XmsnMed = "XmsnMed";
            public const string CnctrTyp = "CnctrTyp";
            public const string PlgInRlTypId = "PlgInRlTypId";
            public const string ChnlNo = "ChnlNo";
            public const string MultFxWvlgth = "MultFxWvlgth";
            public const string Wvlgth = "Wvlgth";
            public const string XmtRcvInd = "XmtRcvInd";
            public const string XmsnRt = "XmsnRt";
            public const string SltCnsmptnId = "SltCnsmptnId";
            public const string SltCnsmptnTyp = "SltCnsmptnTyp";
            public const string SltCnsmptnQty = "SltCnsmptnQty";
            public const string SltCnsmptnLst = "SltCnsmptnLst";
            public const string CrdPstn = "CrdPstn";
            public const string Slts = "Slts";
            public const string Prts = "Prts";
            public const string Dflt = "Dflt";
            //public const string Itrm = "Itrm";
            public const string SpcnRlTyp = "SpcnRlTyp";
            public const string SpcnRlTypLst = "SpcnRlTypLst";
            public const string SlotDefId = "SlotDefId";
            public const string SlotSpecId = "SlotSpecId";
            public const string SlotSeq = "SlotSequence";
            public const string SlotTyp = "SlotType";
            public const string SlotQty = "SlotQuantity";
            public const string unSelectToRemove = "UnSelectToRemove";
            public const string GUID = "GUID";
            public const string HasSlotLst = "HasSlotLst";
            public const string SpcnUseTypLst = "SpcnUseTypLst";
            public const string PrtyNo = "PrtyNo";
            public const string Slctd = "Slctd";
            public const string NdLvlMtrl = "NdLvlMtrl";
            public const string StrtSltNo = "StrtSltNo";
            public const string OrnttnId = "OrnttnId";
            public const string SltsRwQty = "SltsRwQty";
            public const string LblNm = "LblNm";
            public const string LblPosId = "LblPosId";
            public const string SbSlt = "SbSlt";
            public const string RwNum = "RwNum";
            public const string Clei = "Clei";
            public const string Heci = "Heci";
            //Node
            public const string NodeTypId = "NodeTypId";
            public const string Shlvs = "Shlvs";
            public const string MuxCpbl = "MuxCpbl";
            public const string PerfMonitrgCpbl = "PerfMonitrgCpbl";
            public const string EnniCpbl = "EnniCpbl";
            public const string EsPlsCrdReqr= "EsPlsCrdReqr";
            public const string NwSrvcAllw= "NwSrvcAllw";
            public const string NdeFrmtCd = "NdeFrmtCd";
            public const string NdeFrmtValQlfrId = "NdeFrmtValQlfrId";
            public const string NdeFrmtNcludInd = "NdeFrmtNcludInd";
            public const string StructType = "StructType";
            public const string SwVrsn = "SwVrsn";
            public const string MtrlCd = "MtrlCd";
            public const string MfgPrtNo = "MfgPrtNo";
            public const string QoSCpbl = "QoSCpbl";
            public const string BayXtnUseTypId = "BayXtnUseTypId";
            public const string CardUseTypId = "CardUseTypId";
            public const string NodeUseTypId = "NodeUseTypId";
            public const string PluginUseTypId = "PluginUseTypId";
            public const string ShelfUseTypId = "ShelfUseTypId";
            public const string ShelfNDSUseTyp = "ShelfNDSUseTyp";
            public const string CardNDSUseTyp = "CardNDSUseTyp";
            public const string SlotUseTypId = "SlotUseTypId";           
            public const string TransmissionRateLst = "TransmissionRateLst";
            public const string wavelengthLst = "wavelengthLst";
            public const string RecWvlgth = "RecWvlgth";
            public const string TraWvlgth = "TraWvlgth";
            public const string plugIn_id = "plugIn_id";
            //Port
            public const string PortUseTypId = "PortUseTypId";
            public const string UseTyp = "UseTyp";   
            public const string PortSrvLvl = "PortSrvLvl";
            public const string PortTyp = "PortTyp";
            public const string CnctrTypId = "CnctrTypid";
            public const string physStts = "physStts";
            public const string PortDept = "PortDept";

            //Has Slot Mange popup window
            public const string eqdes = "eqdes";
            public const string HorzDisp = "HorzDisp";
            public const string Label = "Label";
            public const string rotation = "rotation";
            public const string MangSlotLst = "MangSlotLst";

            public const string RotationAngl = "RotationAngl";
        }
    }
}