CREATE OR REPLACE PACKAGE node_specn_pkg AS
  TYPE ref_cursor IS REF CURSOR;
  PROCEDURE get_node_specn(pId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_node_specn_role_typ(pId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_node_specn_use_typ(pId IN NUMBER, retcsr OUT ref_cursor);
 FUNCTION get_search_query(pClss IN VARCHAR2, pId IN VARCHAR2, pNm IN VARCHAR2, pDsc IN VARCHAR2, pStts IN VARCHAR2, pmodelNm IN VARCHAR2) RETURN VARCHAR2;
 PROCEDURE update_node_specn(pId IN NUMBER, pNm IN VARCHAR2,pNdTypId IN number,pPrtsInd IN VARCHAR2,pShlvInd IN VARCHAR2,
   pQosCpblInd IN VARCHAR2,pMuxgCpbl IN VARCHAR2,pPrfmc IN VARCHAR2,pEnni IN VARCHAR2, pNewSrvc IN VARCHAR2,
   pNodeFrmtCd IN VARCHAR2,pNodeFrmtVal IN NUMBER,pFrmtNclud IN VARCHAR2,pNodeSpecDsc IN VARCHAR2,pWallMnt IN VARCHAR2,
   pStraightThru IN VARCHAR2,pMidPln IN VARCHAR2,pGnrc IN VARCHAR2,pSmpleCmplx in varchar2,pLabelNm in varchar2, pLabelPosId IN NUMBER,
   pSwVrsnNo IN VARCHAR2, pUseTyp in VARCHAR2);
   PROCEDURE insert_node_specn (pNm IN VARCHAR2,pNdTypId IN number,pPrtsInd IN VARCHAR2,pShlvInd IN VARCHAR2,
   pQosCpblInd IN VARCHAR2,pMuxgCpbl IN VARCHAR2,pPrfmc IN VARCHAR2,pEnni IN VARCHAR2, pNewSrvc IN VARCHAR2,
   pNodeFrmtCd IN VARCHAR2,pNodeFrmtVal IN NUMBER,pFrmtNclud IN VARCHAR2,pNodeSpecDsc IN VARCHAR2,pWallMnt IN VARCHAR2,
   pStraightThru IN VARCHAR2,pMidPln IN VARCHAR2,pGnrc IN VARCHAR2,pSmpleCmplx in varchar2,pLabelNm in varchar2, pLabelPosId IN NUMBER,
   pSwVrsnNo IN VARCHAR2, pUseTyp IN VARCHAR2, oSpecnId OUT NUMBER);
   PROCEDURE insert_node_specn_gnrc (pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2,
    pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER);
  PROCEDURE update_node_specn_gnrc (pId IN number, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2,
    pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER);
      PROCEDURE insert_node_specn_revsn_alt (pId IN number, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2,
    pPrpgt IN VARCHAR2, pDel IN VARCHAR2, oRevsnAltId OUT NUMBER);
  PROCEDURE update_node_specn_revsn_alt (pAltId IN number, pSpecnId in number, pNm IN VARCHAR2, pRo IN VARCHAR2,
    pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2, pDel IN VARCHAR2);
    PROCEDURE delete_node_specn_role(pId IN NUMBER);
  PROCEDURE insert_node_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER);
  PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER);
  PROCEDURE update_node_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER,
    pPlndHt IN NUMBER, pPlndHtUom IN NUMBER, pElcNrmnl IN NUMBER,
    pElcNrmnlUom IN NUMBER, pElcMx IN NUMBER, pElcMxUom IN NUMBER, pWt IN NUMBER, pWtUom IN NUMBER,
    pHt IN NUMBER, pHtUom IN NUMBER, didUpdate OUT VARCHAR2);
  PROCEDURE get_node_tirks_unit_info(pSpecnNm IN VARCHAR2, retcsr OUT ref_cursor);
  PROCEDURE get_port_sequences(pSpecnId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_pluggable_port_assignments(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_non_plug_port_assignments(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_pluggable_msdp_ports(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_non_plug_msdp_ports(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_non_plug_msdp_required(pDualNdId IN NUMBER, reqrd OUT VARCHAR2);
  PROCEDURE get_plgble_prt_asgn_frm_term(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE get_non_plg_prt_asgn_frm_term(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor);
  PROCEDURE insert_nds_spec_id(pSpecRvsnId IN NUMBER, pNdsSpecId IN NUMBER, pGnrcInd IN VARCHAR2);
  FUNCTION get_material_search_query(pMtlCd IN VARCHAR2, pId IN VARCHAR2, pPrtNo IN VARCHAR2, pDsc IN VARCHAR2, pClmc IN VARCHAR2, pRo IN VARCHAR2) RETURN VARCHAR2;
END node_specn_pkg;
/
CREATE OR REPLACE PACKAGE BODY node_specn_pkg
IS
FUNCTION get_search_query(pClss IN VARCHAR2,pId   IN VARCHAR2,pNm   IN VARCHAR2,pDsc  IN VARCHAR2, pStts IN VARCHAR2, pmodelNm IN VARCHAR2)
  RETURN VARCHAR2
AS
  finalQuery    VARCHAR2(4000);
  regBaseQuery  VARCHAR2(4000);
  gnrcBaseQuery VARCHAR2(4000);
  idWhere       VARCHAR2(100);
  nmWhere       VARCHAR2(1000);
  modelNmWhere  VARCHAR2(1000);
  gNmWhere      VARCHAR2(1000);
  dscWhere      VARCHAR2(2000);
  cmpltWhere    VARCHAR2(100);
  prpgtWhere    VARCHAR2(100);
  delWhere      VARCHAR2(100);
  roWhere       VARCHAR2(100);
BEGIN
  regBaseQuery    := 'SELECT ns.node_specn_id as specn_id, a.node_specn_revsn_nm as specn_nm, ns.node_specn_dsc as specn_dsc,
a.cmplt_ind, a.prpgt_ind, a.del_ind, ''NODE'' AS specTyp,
decode(a.rcrds_only_ind, ''Y'', ''Record Only'', NULL) AS specClss, ''NODE'' AS enumSpecTyp, ns.node_specn_nm as specn_model_nm
FROM node_specn ns, node_specn_revsn_alt a
WHERE ns.node_specn_id = a.node_specn_id
AND ns.gnrc_ind = ''N''';
  gnrcBaseQuery   := 'SELECT ns.node_specn_id as specn_id, ns.node_specn_nm as specn_nm, ns.node_specn_dsc as specn_dsc,
a.cmplt_ind, a.prpgt_ind, a.del_ind, ''NODE'' AS specTyp, ''Generic'' AS specClss, ''NODE'' AS enumSpecTyp, null as specn_model_nm
FROM node_specn ns, node_specn_gnrc a
WHERE ns.node_specn_id = a.node_specn_id
AND ns.gnrc_ind = ''Y''';
  idWhere         := ' AND ns.node_specn_id LIKE ''%' || pId || '%''';
  nmWhere         := ' AND a.node_specn_revsn_nm LIKE ''%' || pNm || '%''';
  modelNmWhere    := ' AND ns.node_specn_nm LIKE ''%' || pmodelNm || '%''';
  gNmWhere        := ' AND ns.node_specn_nm LIKE ''%' || pNm || '%''';
  dscWhere        := ' AND ns.node_specn_dsc LIKE ''%' || pDsc || '%''';
  cmpltWhere      := ' AND a.cmplt_ind = ''Y''';
  prpgtWhere      := ' AND a.prpgt_ind = ''Y''';
  delWhere        := ' AND a.del_ind = ''Y''';
  roWhere         := ' AND a.rcrds_only_ind = ''Y''';
  IF(pId          != '%') THEN
    regBaseQuery  := regBaseQuery || idWhere;
    gnrcBaseQuery := gnrcBaseQuery || idWhere;
  END IF;
  IF(pNm          != '%') THEN
    regBaseQuery  := regBaseQuery || nmWhere;
    gnrcBaseQuery := gnrcBaseQuery || gNmWhere;
  END IF;
  IF(pModelNm     != '%') THEN
    regBaseQuery  := regBaseQuery || modelNmWhere;
    gnrcBaseQuery := gnrcBaseQuery || gNmWhere;
  END IF;
  IF(pDsc         != '%') THEN
    regBaseQuery  := regBaseQuery || dscWhere;
    gnrcBaseQuery := gnrcBaseQuery || dscWhere;
  END IF;
  IF(pStts         = 'Completed') THEN
    regBaseQuery  := regBaseQuery || cmpltWhere;
    gnrcBaseQuery := gnrcBaseQuery || cmpltWhere;
  ELSIF(pStts      = 'Propagated') THEN
    regBaseQuery  := regBaseQuery || prpgtWhere;
    gnrcBaseQuery := gnrcBaseQuery || prpgtWhere;
  ELSIF(pStts      = 'Deleted') THEN
    regBaseQuery  := regBaseQuery || delWhere;
    gnrcBaseQuery := gnrcBaseQuery || delWhere;
  END IF;
  IF(pClss      = 'Record Only') THEN
    finalQuery := regBaseQuery || roWhere;
  ELSIF(pClss   = 'Generic') THEN
    finalQuery := gnrcBaseQuery;
  ELSIF(pClss   = 'Interim') THEN
    finalQuery := NULL;
  ELSE
    IF (pmodelNm <> '%')
      THEN 
        finalQuery := regBaseQuery;
      ELSE
        finalQuery := regBaseQuery || ' UNION ' || gnrcBaseQuery;
      END IF;
  END IF;
  RETURN finalQuery;
END get_search_query;

PROCEDURE get_node_specn(pId IN NUMBER,retcsr OUT ref_cursor) AS
BEGIN
  OPEN retcsr FOR
  SELECT n.node_specn_nm,n.node_typ_id,n.has_prts_ind,n.has_shlvs_ind,n.qos_cpbl_ind,n.muxg_cpbl_ind,
  n.prfmc_mntrg_cpbl_ind,n.enni_cpbl_ind, n.new_srvc_allow_ind,n.node_frmt_cd,n.node_frmt_val_qlfr_id,
  n.node_frmt_nclud_ind,n.node_specn_dsc,n.wll_mnt_allow_ind,n.strght_thru_ind,n.mid_pln_ind,n.gnrc_ind,n.smple_cmplx_strctrd_ind,
  n.label_nm, n.label_pos_id,n.sw_vrsn_no,g.cmplt_ind AS gnrc_cmplt_ind,g.prpgt_ind AS gnrc_prpgt_ind,g.del_ind AS gnrc_del_ind,
  g.dpth_no, g.hgt_no, g.wdth_no, g.dim_uom_id, a.node_specn_revsn_nm,a.rcrds_only_ind, a.cmplt_ind, a.prpgt_ind, a.del_ind,
  nvl(mtl.rme_node_mtrl_revsn_id, roMtl.rme_node_mtrl_revsn_id) AS rme_node_mtrl_revsn_id,
  nvl(mtl.mtrl_id, roMtl.mtrl_id) AS mtrl_id, nvl(mtl.mtrl_cd, roMtl.mtrl_cd) AS mtrl_cd,
  nvl(mtl.mfg_part_no, roMtl.rt_part_no) AS mfg_part_no, nvl(mtl.mfr_cd, roMtl.mfr_cd) AS mfr_cd, a.node_specn_revsn_alt_id
  , usetyp.use_typ
  FROM node_specn n, node_specn_gnrc g, node_specn_revsn_alt a
  , specn_record_use_typ usetyp
  ,  (SELECT rev.mtrl_id,rev.rme_node_mtrl_revsn_id,ms.mfg_part_no,mfr.mfr_cd,rev.node_specn_revsn_alt_id,rev.mtrl_cd
    FROM rme_node_mtrl_revsn rev,mtl_item_sap ms, mtrl m, mfr
    WHERE rev.mtrl_cd = ms.product_id
    AND rev.mtrl_id = m.mtrl_id
    AND m.mfr_id = mfr.mfr_id) mtl
  , (SELECT rev.mtrl_id, rev.rme_node_mtrl_revsn_id, m.rt_part_no, mfr.mfr_cd, rev.node_specn_revsn_alt_id, rev.mtrl_cd
    FROM rme_node_mtrl_revsn rev, mtrl m, mfr
    WHERE rev.mtrl_id = m.mtrl_id
    AND m.mfr_id = mfr.mfr_id
    AND m.rcrds_only_ind = 'Y') roMtl
    
  WHERE n.node_specn_id = pId
  AND n.node_specn_id = g.node_specn_id(+)
  AND n.node_specn_id = a.node_specn_id(+)
  AND n.specn_record_use_typ_id = usetyp.specn_record_use_typ_id(+)
  AND a.node_specn_revsn_alt_id = mtl.node_specn_revsn_alt_id(+)
  AND a.node_specn_revsn_alt_id = roMtl.node_specn_revsn_alt_id(+)
  ;

  EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE(' Error in procedure get_node_specn ' || SQLERRM);
    RAISE_APPLICATION_ERROR(-20001, 'get_node_specn Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END get_node_specn;

PROCEDURE get_node_specn_role_typ(pId IN NUMBER,retcsr OUT ref_cursor) AS
BEGIN
  OPEN retcsr FOR
  SELECT ct.node_role_typ_id, ct.node_role_typ, cr.node_typ_role_prty_no, DECODE(NVL(cr.node_typ_id, 0), 0, 'N', 'Y')AS is_selected
  FROM node_role_typ ct,
  (SELECT csr.node_typ_id,csr.node_role_typ_id,csr.node_typ_role_prty_no FROM NODE_TYP_ROLE csr
  WHERE csr.NODE_TYP_ID = pId) cr WHERE ct.node_role_typ_id = cr.node_role_typ_id(+);
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure get_node_specn_role_typ ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'get_node_specn_role_typ Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END get_node_specn_role_typ;

PROCEDURE get_node_specn_use_typ(pId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    select specn_record_use_typ_id, use_typ
    from specn_record_use_typ
    where specn_typ_id = pId
    order by
        case 
          when use_typ like 'mit_%' then 1
        end, use_typ;
  
    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in procedure get_node_specn_use_typ ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'get_node_specn_use_typ Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END get_node_specn_use_typ;
  
PROCEDURE update_node_specn(pId IN NUMBER,pNm IN VARCHAR2,pNdTypId IN NUMBER,pPrtsInd IN VARCHAR2,pShlvInd IN VARCHAR2,
    pQosCpblInd IN VARCHAR2,pMuxgCpbl IN VARCHAR2,pPrfmc IN VARCHAR2,pEnni IN VARCHAR2, pNewSrvc      IN VARCHAR2,
    pNodeFrmtCd   IN VARCHAR2,pNodeFrmtVal  IN NUMBER,pFrmtNclud    IN VARCHAR2,pNodeSpecDsc  IN VARCHAR2,pWallMnt      IN VARCHAR2,
    pStraightThru IN VARCHAR2,pMidPln       IN VARCHAR2,pGnrc         IN VARCHAR2,pSmpleCmplx   IN VARCHAR2,pLabelNm      IN VARCHAR2,
    pLabelPosId   IN NUMBER,pSwVrsnNo     IN VARCHAR2, 
    pUseTyp  IN VARCHAR2
    )
AS
  pUseTypId NUMBER;
BEGIN

  SELECT specn_record_use_typ_id INTO pUseTypId
    FROM specn_record_use_typ
    WHERE specn_typ_id = 1 
    AND use_typ = pUseTyp
  ;
    
  UPDATE NODE_SPECN n
  SET n.node_specn_nm        =pNm,
    n.node_typ_id            =pNdTypId,
    n.has_prts_ind           =pPrtsInd,
    n.has_shlvs_ind          =pShlvInd,
    n.qos_cpbl_ind           =pQosCpblInd,
    n.muxg_cpbl_ind          =pMuxgCpbl,
    n.prfmc_mntrg_cpbl_ind   =pPrfmc,
    n.enni_cpbl_ind          =pEnni,
    n.new_srvc_allow_ind     =pNewSrvc,
    n.node_frmt_cd           =pNodeFrmtCd,
    n.node_frmt_val_qlfr_id  =pNodeFrmtVal,
    n.node_frmt_nclud_ind    =pFrmtNclud,
    n.node_specn_dsc         =pNodeSpecDsc,
    n.wll_mnt_allow_ind      =pWallMnt,
    n.strght_thru_ind        =pStraightThru,
    n.mid_pln_ind            =pMidPln,
    n.gnrc_ind               =pGnrc,
    n.smple_cmplx_strctrd_ind=pSmpleCmplx,
    n.label_nm               =pLabelNm,
    n.label_pos_id           =pLabelPosId,
    n.sw_vrsn_no             =pSwVrsnNo,
    n.specn_record_use_typ_id=pUseTypId
  WHERE n.node_specn_id      = pId;
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure update_node_specn ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'update_node_specn Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END update_node_specn;

PROCEDURE insert_node_specn(pNm IN VARCHAR2, pNdTypId IN NUMBER, pPrtsInd IN VARCHAR2, pShlvInd IN VARCHAR2,
    pQosCpblInd IN VARCHAR2, pMuxgCpbl IN VARCHAR2, pPrfmc IN VARCHAR2, pEnni IN VARCHAR2,
    pNewSrvc IN VARCHAR2, pNodeFrmtCd IN VARCHAR2, pNodeFrmtVal IN NUMBER, pFrmtNclud IN VARCHAR2, pNodeSpecDsc IN VARCHAR2,
    pWallMnt IN VARCHAR2, pStraightThru IN VARCHAR2, pMidPln IN VARCHAR2, pGnrc IN VARCHAR2, pSmpleCmplx IN VARCHAR2,
    pLabelNm IN VARCHAR2, pLabelPosId IN NUMBER, pSwVrsnNo IN VARCHAR2, pUseTyp IN VARCHAR2, oSpecnId OUT NUMBER)
AS
  pUseTypId NUMBER;
BEGIN
  SELECT specn_record_use_typ_id INTO pUseTypId
    FROM specn_record_use_typ
    WHERE specn_typ_id = 1 
    AND use_typ = pUseTyp
  ;
    
  SELECT specn_id_seq.nextval INTO oSpecnId FROM dual;
  INSERT INTO NODE_SPECN
    (node_specn_id,node_specn_nm,node_typ_id,has_prts_ind,has_shlvs_ind,qos_cpbl_ind,muxg_cpbl_ind,prfmc_mntrg_cpbl_ind,enni_cpbl_ind,
      new_srvc_allow_ind,node_frmt_cd,node_frmt_val_qlfr_id,node_frmt_nclud_ind,node_specn_dsc,wll_mnt_allow_ind,
      strght_thru_ind,mid_pln_ind,gnrc_ind,smple_cmplx_strctrd_ind,label_nm,label_pos_id,sw_vrsn_no,specn_record_use_typ_id)
    VALUES(oSpecnId,pNm,pNdTypId,pPrtsInd,pShlvInd,pQosCpblInd,pMuxgCpbl,pPrfmc,pEnni, pNewSrvc,pNodeFrmtCd,pNodeFrmtVal,
      pFrmtNclud,pNodeSpecDsc,pWallMnt,pStraightThru,pMidPln,pGnrc,pSmpleCmplx,pLabelNm,pLabelPosId,pSwVrsnNo,pUseTypId);
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure insert_node_specn ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'insert_node_specn Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END insert_node_specn;

PROCEDURE insert_node_specn_gnrc(pId    IN NUMBER,pCmplt IN VARCHAR2,pPrpgt IN VARCHAR2,pDel   IN VARCHAR2,pDpth  IN NUMBER,
    pHght  IN NUMBER,pWdth  IN NUMBER,pUom   IN NUMBER)AS
BEGIN
  INSERT INTO node_specn_gnrc
    (node_specn_id,cmplt_ind,prpgt_ind,del_ind,dpth_no,hgt_no,wdth_no,dim_uom_id)
    VALUES(pId,pCmplt,pPrpgt,pDel,pDpth,pHght,pWdth,pUom);
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure insert_node_specn_gnrc ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'insert_node_specn_gnrc Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END insert_node_specn_gnrc;

PROCEDURE update_node_specn_gnrc(pId    IN NUMBER,pCmplt IN VARCHAR2,pPrpgt IN VARCHAR2,pDel   IN VARCHAR2,pDpth  IN NUMBER,
    pHght  IN NUMBER,pWdth  IN NUMBER,pUom   IN NUMBER)AS
BEGIN
  UPDATE node_specn_gnrc s
  SET s.cmplt_ind       = pCmplt,
    s.prpgt_ind         = pPrpgt,
    s.del_ind           = pDel,
    s.dpth_no           = pDpth,
    s.hgt_no            = pHght,
    s.wdth_no           = pWdth,
    s.dim_uom_id        = pUom
  WHERE s.node_specn_id = pId;
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure update_node_specn_gnrc ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'update_node_specn_gnrc Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END update_node_specn_gnrc;

PROCEDURE insert_node_specn_revsn_alt(pId IN NUMBER, pNm IN VARCHAR2, pRo IN VARCHAR2, pCmplt IN VARCHAR2,
  pPrpgt IN VARCHAR2, pDel IN VARCHAR2, oRevsnAltId OUT NUMBER) AS
BEGIN
  SELECT specn_id_seq.nextval INTO oRevsnAltId
    FROM dual;

  INSERT INTO node_specn_revsn_alt(node_specn_revsn_alt_id, node_specn_id, node_specn_revsn_nm, rcrds_only_ind, cmplt_ind, prpgt_ind, del_ind)
    VALUES
    (oRevsnAltId, pId, pNm, pRo, pCmplt, pPrpgt, pDel);
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure insert_node_specn_revsn_alt ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'insert_node_specn_revsn_alt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END insert_node_specn_revsn_alt;

PROCEDURE update_node_specn_revsn_alt(pAltId   IN NUMBER,pSpecnId IN NUMBER,pNm      IN VARCHAR2,pRo      IN VARCHAR2,
    pCmplt IN VARCHAR2,pPrpgt IN VARCHAR2,pDel   IN VARCHAR2)
AS
BEGIN
  UPDATE node_specn_revsn_alt a
  SET a.node_specn_revsn_nm       = pNm,
    a.rcrds_only_ind              = pRo,
    a.cmplt_ind                   = pCmplt,
    a.prpgt_ind                   = pPrpgt,
    a.del_ind                     = pDel
  WHERE a.node_specn_revsn_alt_id = pAltId
  AND a.node_specn_id             = pSpecnId;
EXCEPTION
WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE(' Error in procedure update_node_specn_revsn_alt ' || SQLERRM);
  RAISE_APPLICATION_ERROR(-20001, 'update_node_specn_revsn_alt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
END update_node_specn_revsn_alt;

PROCEDURE delete_node_specn_role(pId IN NUMBER) AS
  BEGIN
    DELETE FROM NODE_TYP_ROLE
    WHERE NODE_TYP_ID = pId;

    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in procedure delete_node_specn_role ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'delete_node_specn_role Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END delete_node_specn_role;

  PROCEDURE insert_node_specn_role(pId IN NUMBER, pRlTypId IN NUMBER, pPrtyNo IN NUMBER) AS
  BEGIN
    INSERT INTO NODE_TYP_ROLE (node_typ_id, node_role_typ_id, node_typ_role_prty_no)
    VALUES (pId, pRlTypId, pPrtyNo);

    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in procedure insert_node_specn_role ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'insert_node_specn_role Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END insert_node_specn_role;

  PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER) AS
  BEGIN
    UPDATE rme_node_mtrl_revsn a
    SET a.node_specn_revsn_alt_id = pSpecnRvsnId
    WHERE a.rme_node_mtrl_revsn_id = pMtlItmId;

    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in associate_material ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'associate_material Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END associate_material;

  PROCEDURE update_node_material(pMtlItmId IN NUMBER, pHght IN NUMBER, pDpth IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER,
    pPlndHt IN NUMBER, pPlndHtUom IN NUMBER, pElcNrmnl IN NUMBER,
    pElcNrmnlUom IN NUMBER, pElcMx IN NUMBER, pElcMxUom IN NUMBER, pWt IN NUMBER, pWtUom IN NUMBER,
    pHt IN NUMBER, pHtUom IN NUMBER, didUpdate OUT VARCHAR2) AS

    extHght NUMBER;
    extDpth NUMBER;
    extWdth NUMBER;
    extUom NUMBER;
    extPlndHt NUMBER;
    extPlndHtUom NUMBER;
    extElcNrmnl NUMBER;
    extElcNrmnlUom NUMBER;
    extElcMx NUMBER;
    extElcMxUom NUMBER;
    extWt NUMBER;
    extWtUom NUMBER;
    extHt NUMBER;
    extHtUom NUMBER;
    mtrlId NUMBER;
  BEGIN
    didUpdate := 'N';

    SELECT m.dpth_no, m.hgt_no, m.wdth_no, m.dim_uom_id, m.mtrl_id, r.plnd_het_gntn_no,
    r.plnd_het_gntn_uom_id, r.elc_curr_norm_drn_no, r.elc_curr_norm_drn_uom_id, r.elc_curr_max_drn_no,
    r.elc_curr_max_drn_uom_id, r.node_wt_no, r.node_wt_uom_id, r.het_dssptn_no, r.het_dssptn_uom_id
    INTO extDpth, extHght, extWdth, extUom, mtrlId, extPlndHt, extPlndHtUom, extElcNrmnl,
    extElcNrmnlUom, extElcMx, extElcMxUom, extWt, extWtUom, extHt, extHtUom
    FROM rme_node_mtrl m, rme_node_mtrl_revsn r
    WHERE r.mtrl_id = m.mtrl_id
    AND r.rme_node_mtrl_revsn_id = pMtlItmId;

    IF(extHght != pHght OR extDpth != pDpth OR extWdth != pWdth OR extUom != pUom) THEN
      UPDATE rme_node_mtrl m
      SET m.dpth_no = pDpth, m.hgt_no = pHght, m.wdth_no = pWdth, m.dim_uom_id = pUom
      WHERE m.mtrl_id = mtrlId;

      didUpdate := 'Y';
    END IF;

    IF(reference_pkg.are_equal(extPlndHt, pPlndHt) = 'N' OR
      reference_pkg.are_equal(extPlndHtUom, pPlndHtUom) = 'N' OR
      reference_pkg.are_equal(extElcNrmnl, pElcNrmnl) = 'N' OR
      reference_pkg.are_equal(extElcNrmnlUom, pElcNrmnlUom) = 'N' OR
      reference_pkg.are_equal(extElcMx, pElcMx) = 'N' OR
      reference_pkg.are_equal(extElcMxUom, pElcMxUom) = 'N' OR
      reference_pkg.are_equal(extWt, pWt) = 'N' OR
      reference_pkg.are_equal(extWtUom, pWtUom) = 'N' OR
      reference_pkg.are_equal(extHt, pHt) = 'N' OR
      reference_pkg.are_equal(extHtUom, pHtUom) = 'N') THEN

      UPDATE rme_node_mtrl_revsn r
      SET r.plnd_het_gntn_no = pPlndHt, r.plnd_het_gntn_uom_id = pPlndHtUom, r.elc_curr_norm_drn_no = pElcNrmnl,
      r.elc_curr_norm_drn_uom_id = pElcNrmnlUom, r.elc_curr_max_drn_no = pElcMx,
      r.elc_curr_max_drn_uom_id = pElcMxUom, r.node_wt_no = pWt, r.node_wt_uom_id = pWtUom,
      r.het_dssptn_no = pHt, r.het_dssptn_uom_id = pHtUom
      WHERE r.rme_node_mtrl_revsn_id = pMtlItmId;

      didUpdate := 'Y';
    END IF;

    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in update_node_material ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'update_node_material Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END update_node_material;
  
  PROCEDURE get_node_tirks_unit_info(pSpecnNm IN VARCHAR2, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT ns.node_specn_id, ns.node_frmt_cd, ns.node_frmt_nclud_ind, ns.node_frmt_val_qlfr_id, ns.box_cd, vq.val_qlfr_nm,
    a.node_frmt_val, a.val_opr_nm
    FROM node_specn ns, node_specn_revsn_alt ra, val_qlfr vq, (SELECT v.node_specn_id, v.node_frmt_val, vo.val_opr_nm 
    FROM node_specn_val v, val_opr vo
    WHERE v.val_opr_id = vo.val_opr_id(+)) a
    WHERE ra.node_specn_revsn_nm = pSpecnNm
    AND ra.node_specn_id = ns.node_specn_id
    AND ns.node_frmt_val_qlfr_id = vq.val_qlfr_id(+)
    AND ns.node_specn_id = a.node_specn_id(+);
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_NODE_TIRKS_UNIT_INFO', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_node_tirks_unit_info Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_node_tirks_unit_info;
  
  PROCEDURE get_port_sequences(pSpecnId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT pd.node_specn_with_prts_def_id, pd.port_seq_no, pd.port_qty, pd.has_asnbl_prts_ind, pt.port_typ, pd.port_no_ofst_val
    FROM node_specn_with_prts_def pd, port_typ pt
    WHERE pd.node_specn_id = pSpecnId
    AND pd.has_asnbl_prts_ind = 'Y'
    AND pd.port_typ_id = pt.port_typ_id
    ORDER BY pd.port_seq_no;
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_PORT_SEQUENCES', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_port_sequences Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_port_sequences;
  
  PROCEDURE get_pluggable_port_assignments(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT ppd.asnbl_nd_spcn_plg_prts_def_id, ppd.port_heci_cd, ppd.unit_frmt_cd, ppd.msdp_grpg_reqr_ind, 
    vd.val_dlmtr_cd, vq.val_qlfr_nm, pa.port_no + nswpd.port_no_ofst_val AS port_no, 
    pa.asnbl_nd_spcn_wth_prts_asmt_id, pd.node_specn_with_prts_def_id, 
    CASE WHEN dnswpa.asnbl_nd_spcn_wth_prts_asmt_id IS NOT NULL THEN 'Y' ELSE 'N' END AS dual_mode
    FROM asnbl_node_specn_with_prts_def pd, asnbl_node_specn_plg_prts_def ppd, val_dlmtr vd, val_qlfr vq, 
    asnbl_node_spcn_with_prts_asmt pa, dual_node_spcn_with_prts_asmt dnswpa, node_specn_with_prts_def nswpd
    WHERE pd.node_specn_with_prts_def_id = pPrtsDefId
    AND pd.node_specn_with_prts_def_id = nswpd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = ppd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = pa.node_specn_with_prts_def_id
    AND ppd.unit_frmt_val_qlfr_id = vq.val_qlfr_id(+)
    AND ppd.unit_frmt_val_dlmtr_id = vd.val_dlmtr_id(+)
    AND pa.asnbl_nd_spcn_wth_prts_asmt_id = dnswpa.asnbl_nd_spcn_wth_prts_asmt_id(+)
    ORDER BY port_no;
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_PLUGGABLE_PORT_ASSIGNMENTS', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_pluggable_port_assignments Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_pluggable_port_assignments;
  
  PROCEDURE get_non_plug_port_assignments(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT ppd.asnbl_nd_spcn_nplg_prts_def_id, ppd.port_heci_cd, ppd.unit_frmt_cd, 
    vd.val_dlmtr_cd, vq.val_qlfr_nm, pa.port_no + nswpd.port_no_ofst_val AS port_no, pd.node_specn_with_prts_def_id,
    CASE WHEN dnswpa.asnbl_nd_spcn_wth_prts_asmt_id IS NOT NULL THEN 'Y' ELSE 'N' END AS dual_mode,
    dnswpa.dual_nd_spcn_wth_prts_asmt_id
    FROM asnbl_node_specn_with_prts_def pd, asnbl_node_specn_nplg_prts_def ppd, val_dlmtr vd, val_qlfr vq, 
    asnbl_node_spcn_with_prts_asmt pa, node_specn_with_prts_def nswpd, dual_node_spcn_with_prts_asmt dnswpa
    WHERE pd.node_specn_with_prts_def_id = pPrtsDefId
    AND pd.node_specn_with_prts_def_id = nswpd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = ppd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = pa.node_specn_with_prts_def_id
    AND ppd.unit_frmt_val_qlfr_id = vq.val_qlfr_id(+)
    AND ppd.unit_frmt_val_dlmtr_id = vd.val_dlmtr_id(+)
    AND pa.asnbl_nd_spcn_wth_prts_asmt_id = dnswpa.asnbl_nd_spcn_wth_prts_asmt_id(+)
    ORDER BY port_no;
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_NON_PLUG_PORT_ASSIGNMENTS', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_non_plug_port_assignments Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_non_plug_port_assignments;
  
  PROCEDURE get_pluggable_msdp_ports(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT pdv.unit_frmt_val, vo.val_opr_nm
    FROM asnbl_node_specn_plg_def_val pdv, val_opr vo
    WHERE pdv.asnbl_nd_spcn_plg_prts_def_id = pPrtsDefId
    AND pdv.val_opr_id = vo.val_opr_id(+)
    ORDER BY pdv.unit_frmt_val;
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_PLUGGABLE_MSDP_PORTS', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_pluggable_msdp_ports Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_pluggable_msdp_ports;
  
  PROCEDURE get_non_plug_msdp_ports(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT npdv.unit_frmt_val, vo.val_opr_nm
    FROM asnbl_node_specn_nplg_def_val npdv, val_opr vo
    WHERE npdv.asnbl_nd_spcn_nplg_prts_def_id = pPrtsDefId
    AND npdv.val_opr_id = vo.val_opr_id(+)
    ORDER BY npdv.unit_frmt_val;
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_NON_PLUG_MSDP_PORTS', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_non_plug_msdp_ports Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_non_plug_msdp_ports;
  
  PROCEDURE get_non_plug_msdp_required(pDualNdId IN NUMBER, reqrd OUT VARCHAR2) AS
  BEGIN
    SELECT c.msdp_grpg_reqr_ind INTO reqrd
    FROM dual_node_spcn_with_prts_asmt a, asnbl_node_spcn_with_prts_asmt b, asnbl_node_specn_plg_prts_def c
    WHERE a.dual_nd_spcn_wth_prts_asmt_id = pDualNdId
    AND a.lk_asnbl_nd_spcn_w_prt_asmt_id = b.asnbl_nd_spcn_wth_prts_asmt_id
    AND b.node_specn_with_prts_def_id = c.node_specn_with_prts_def_id;
    
    EXCEPTION
    WHEN no_data_found THEN
      reqrd := 'N';
    WHEN OTHERS THEN
      reqrd := 'N';
      
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.GET_NON_PLUG_MSDP_REQUIRED', 'MTL_EDTR', 'DB', NULL);
      --RAISE_APPLICATION_ERROR(-20001, 'get_non_plug_msdp_required Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_non_plug_msdp_required;
  
  PROCEDURE get_plgble_prt_asgn_frm_term(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT ppd.asnbl_nd_spcn_plg_prts_def_id, ppd.port_heci_cd, ppd.unit_frmt_cd, ppd.msdp_grpg_reqr_ind, 
    vd.val_dlmtr_cd, vq.val_qlfr_nm, pa.port_no, pa.label_nm, pd.node_specn_with_prts_def_id
    FROM asnbl_node_specn_with_prts_def pd, asnbl_node_specn_plg_prts_def ppd, val_dlmtr vd, val_qlfr vq, asnbl_node_spcn_with_prts_asmt pa
    WHERE pd.node_specn_with_prts_def_id = pPrtsDefId
    AND pd.node_specn_with_prts_def_id = ppd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = pa.node_specn_with_prts_def_id
    AND ppd.unit_frmt_val_qlfr_id = vq.val_qlfr_id(+)
    AND ppd.unit_frmt_val_dlmtr_id = vd.val_dlmtr_id(+);
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.get_plgble_prt_asgn_frm_term', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_plgble_prt_asgn_frm_term Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_plgble_prt_asgn_frm_term;
  
  PROCEDURE get_non_plg_prt_asgn_frm_term(pPrtsDefId IN NUMBER, retcsr OUT ref_cursor) AS
  BEGIN
    OPEN retcsr FOR
    SELECT ppd.asnbl_nd_spcn_nplg_prts_def_id, ppd.port_heci_cd, ppd.unit_frmt_cd, 
    vd.val_dlmtr_cd, vq.val_qlfr_nm, pa.port_no, pa.label_nm
    FROM asnbl_node_specn_with_prts_def pd, asnbl_node_specn_nplg_prts_def ppd, val_dlmtr vd, val_qlfr vq, asnbl_node_spcn_with_prts_asmt pa
    WHERE pd.node_specn_with_prts_def_id = pPrtsDefId
    AND pd.node_specn_with_prts_def_id = ppd.node_specn_with_prts_def_id
    AND pd.node_specn_with_prts_def_id = pa.node_specn_with_prts_def_id
    AND ppd.unit_frmt_val_qlfr_id = vq.val_qlfr_id(+)
    AND ppd.unit_frmt_val_dlmtr_id = vd.val_dlmtr_id(+);
    
    EXCEPTION
    WHEN OTHERS THEN
      insert_cdmms_log(SQLCODE || ': ' || SQLERRM, 'ERROR', 'NODE_SPECN_PKG.get_non_plg_prt_asgn_frm_term', 'MTL_EDTR', 'DB', NULL);
      RAISE_APPLICATION_ERROR(-20001, 'get_non_plg_prt_asgn_frm_term Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_non_plg_prt_asgn_frm_term;
  
  PROCEDURE insert_nds_spec_id(pSpecRvsnId IN NUMBER, pNdsSpecId IN NUMBER, pGnrcInd IN VARCHAR2) AS
    aliasId NUMBER;
    cnt NUMBER;
  BEGIN
    SELECT ba.node_specn_alias_id INTO aliasId
    FROM node_specn_alias ba, appl
    WHERE ba.alias_nm = 'Shelf Specification'
    AND ba.appl_id = appl.appl_id
    AND appl.appl_nm = 'NDS';

    IF(pGnrcInd = 'Y') THEN
      SELECT COUNT(*) INTO cnt
      FROM node_specn_gnrc_alias_val gv
      WHERE gv.node_specn_id = pSpecRvsnId
      AND gv.node_specn_alias_id = aliasId;

      IF(cnt = 0) THEN
        INSERT INTO node_specn_gnrc_alias_val (node_specn_id, node_specn_alias_id, alias_val)
        VALUES (pSpecRvsnId, aliasId, pNdsSpecId);
      ELSE
        UPDATE node_specn_gnrc_alias_val av
        SET av.alias_val = pNdsSpecId
        WHERE av.node_specn_id = pSpecRvsnId
        AND av.node_specn_alias_id = aliasId;
      END IF;
    ELSE
      SELECT COUNT(*) INTO cnt
      FROM node_specn_alias_val aav
      WHERE aav.node_specn_id = pSpecRvsnId
      AND aav.node_specn_alias_id = aliasId;

      IF(cnt = 0) THEN
        INSERT INTO node_specn_alias_val (node_specn_id, node_specn_alias_id, alias_val)
        VALUES (pSpecRvsnId, aliasId, pNdsSpecId);
      ELSE
        UPDATE node_specn_alias_val aav
        SET aav.alias_val = pNdsSpecId
        WHERE aav.node_specn_id = pSpecRvsnId
        AND aav.node_specn_alias_id = aliasId;
      END IF;
    END IF;

    EXCEPTION
    WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE(' Error in insert_nds_spec_id ' || SQLERRM);
         RAISE_APPLICATION_ERROR(-20001, 'insert_nds_spec_id Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END insert_nds_spec_id;

  FUNCTION get_material_search_query(pMtlCd IN VARCHAR2, pId IN VARCHAR2, pPrtNo IN VARCHAR2, pDsc IN VARCHAR2, pClmc IN VARCHAR2, pRo IN VARCHAR2) RETURN VARCHAR2 AS
    finalQuery VARCHAR2(4000);
    idWhere VARCHAR2(100);
    mtlCdWhere VARCHAR2(1000);
    dscWhere VARCHAR2(2000);
    prtNoWhere VARCHAR2(1000);
    clmcWhere VARCHAR2(1000);
  BEGIN
    finalQuery := 'SELECT r.rme_node_mtrl_revsn_id as id, r.mtrl_cd, v.mfr_cd, v.part_no, v.item_desc, r.revsn_no
                      FROM material_id_mapping_vw v, rme_node_mtrl_revsn r, mtrl m
                      WHERE v.material_item_id = r.rme_node_mtrl_revsn_id
                      AND r.node_specn_revsn_alt_id IS NULL
                      AND r.mtrl_id = m.mtrl_id
                      AND m.rcrds_only_ind = ''' || pRo || '''';
    idWhere := ' AND r.rme_node_mtrl_revsn_id LIKE ''%' || pId || '%''';
    mtlCdWhere := ' AND r.mtrl_cd LIKE ''%' || pMtlCd || '%''';
    dscWhere := ' AND v.item_desc LIKE ''%' || pDsc || '%''';
    prtNoWhere := ' AND v.part_no LIKE ''%' || pPrtNo || '%''';
    clmcWhere := ' AND v.mfr_cd LIKE ''%' || pClmc || '%''';

    IF(pId != '%') THEN
     finalQuery := finalQuery || idWhere;
    END IF;

    IF(pMtlCd != '%') THEN
     finalQuery := finalQuery || mtlCdWhere;
    END IF;

    IF(pDsc != '%') THEN
     finalQuery := finalQuery || dscWhere;
    END IF;

    IF(pPrtNo != '%') THEN
     finalQuery := finalQuery || prtNoWhere;
    END IF;

    IF(pClmc != '%') THEN
     finalQuery := finalQuery || clmcWhere;
    END IF;

    RETURN finalQuery;
  END get_material_search_query;
END node_specn_pkg;
/
