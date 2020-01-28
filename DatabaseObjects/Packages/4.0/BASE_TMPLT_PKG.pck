CREATE OR REPLACE PACKAGE BASE_TMPLT_PKG AS

  TYPE REF_CURSOR IS REF CURSOR;

  PROCEDURE insert_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id%TYPE, pFrntRerInd IN base_bay_tmplt.frnt_rer_ind%TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id%TYPE);
  PROCEDURE update_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id%TYPE, pFrntRerInd IN base_bay_tmplt.frnt_rer_ind%TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id%TYPE);
  PROCEDURE get_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor);
  PROCEDURE get_base_shelf_tmplt (pTmpltId IN NUMBER, retCsr OUT ref_cursor);
  PROCEDURE insert_base_card_tmplt (pTmpltId IN base_card_tmplt.tmplt_id%TYPE, pCardSpecnRevsnAltId IN base_card_tmplt.card_specn_revsn_alt_id%TYPE);
  PROCEDURE insert_base_node_tmplt (pTmpltId IN base_node_tmplt.tmplt_id%TYPE, pNodeSpecnRevsnAltId IN base_node_tmplt.node_specn_revsn_alt_id%TYPE);
  PROCEDURE insert_base_shelf_tmplt (pTmpltId IN base_shelf_tmplt.tmplt_id%TYPE, pShelfSpecnRevsnAltId IN base_shelf_tmplt.shelf_specn_revsn_alt_id%TYPE);
  PROCEDURE insert_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, pPlgInRoleTypId IN base_plg_in_tmplt.plg_in_role_typ_id%TYPE, pCnctrTypId IN base_plg_in_tmplt.cnctr_typ_id%TYPE, pBiDrctnlInd IN base_plg_in_tmplt.bi_drctnl_ind%TYPE);
  PROCEDURE update_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, pPlgInRoleTypId IN base_plg_in_tmplt.plg_in_role_typ_id%TYPE, pCnctrTypId IN base_plg_in_tmplt.cnctr_typ_id%TYPE, pBiDrctnlInd IN base_plg_in_tmplt.bi_drctnl_ind%TYPE);
  PROCEDURE get_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor);
  PROCEDURE insert_base_plg_in_tmplt_prts (pTmpltId IN base_plg_in_tmplt_prts.tmplt_id%TYPE, pXmtRecvInd IN base_plg_in_tmplt_prts.xmt_recv_ind%TYPE, pXCoordNo IN base_plg_in_tmplt_prts.x_coord_no%TYPE, pYCoordNo IN base_plg_in_tmplt_prts.y_coord_no%TYPE, pTmpltDefId OUT base_plg_in_tmplt_prts.tmplt_def_id%TYPE);
  PROCEDURE update_base_plg_in_tmplt_prts (pTmpltDefId IN base_plg_in_tmplt_prts.tmplt_def_id%TYPE, pXmtRecvInd IN base_plg_in_tmplt_prts.xmt_recv_ind%TYPE, pXCoordNo IN base_plg_in_tmplt_prts.x_coord_no%TYPE, pYCoordNo IN base_plg_in_tmplt_prts.y_coord_no%TYPE);
  PROCEDURE get_base_plg_in_tmplt_prts (pTmpltDefId IN base_plg_in_tmplt_prts.tmplt_def_id%TYPE, retCsr OUT ref_cursor);
  PROCEDURE get_shelf_slot_specn(pShlfSpcnId IN NUMBER, retCsr OUT ref_cursor);

END BASE_TMPLT_PKG;
/
CREATE OR REPLACE PACKAGE BODY BASE_TMPLT_PKG AS

  PROCEDURE insert_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id%TYPE,
    pFrntRerInd IN base_bay_tmplt.frnt_rer_ind%TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id%TYPE) AS
  
    p_FrntRerInd char(1);
    p_RottnAnglId Number;
    
    BEGIN
      p_FrntRerInd := nvl(pFrntRerInd,'F');
      p_RottnAnglId := nvl(pRottnAnglId,1);
        
      INSERT INTO base_bay_tmplt (tmplt_id, bay_specn_revsn_alt_id, frnt_rer_ind, rottn_angl_id)
      (SELECT pTmpltId, bsra.bay_specn_revsn_alt_id, p_FrntRerInd, p_RottnAnglId
      FROM bay_specn_revsn_alt bsra
      WHERE bsra.bay_specn_id = pBaySpecnRevsnAltId);
    
      --commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_bay_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_bay_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_bay_tmplt;
    
  PROCEDURE update_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, pBaySpecnRevsnAltId IN base_bay_tmplt.bay_specn_revsn_alt_id%TYPE,
      pFrntRerInd IN base_bay_tmplt.frnt_rer_ind%TYPE, pRottnAnglId IN base_bay_tmplt.rottn_angl_id%TYPE) AS
  
      p_RottnAnglId Number;
      
    BEGIN
  
      p_RottnAnglId := nvl(pRottnAnglId,1);
      
      update base_bay_tmplt 
      set frnt_rer_ind = pFrntRerInd, rottn_angl_id = p_RottnAnglId
      where tmplt_id = pTmpltId 
      and bay_specn_revsn_alt_id = pBaySpecnRevsnAltId;
  
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.update_base_bay_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'update_base_bay_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));

  END update_base_bay_tmplt;
  
  PROCEDURE get_base_bay_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor) AS
    BEGIN
    OPEN retCsr FOR
    
    SELECT bbt.tmplt_id, bbt.bay_specn_revsn_alt_id, bbt.frnt_rer_ind, bbt.rottn_angl_id, t.tmplt_nm, t.tmplt_dsc,
    t.cmplt_ind, t.prpgt_ind, t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind, bsra.bay_specn_id
    FROM base_bay_tmplt bbt, tmplt t, bay_specn_revsn_alt bsra
    WHERE bbt.tmplt_id = pTmpltId
    AND bbt.tmplt_id = t.tmplt_id
    AND bbt.bay_specn_revsn_alt_id = bsra.bay_specn_revsn_alt_id
    AND t.base_tmplt_ind = 'Y';
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.get_base_bay_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_base_bay_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END get_base_bay_tmplt;
  
  PROCEDURE get_base_shelf_tmplt (pTmpltId IN NUMBER, retCsr OUT ref_cursor) AS
    BEGIN
    OPEN retCsr FOR
    
    SELECT bst.tmplt_id, bst.shelf_specn_revsn_alt_id, t.tmplt_nm, t.tmplt_dsc, t.cmplt_ind, t.prpgt_ind,
    t.updt_in_prgs_ind, t.ret_tmplt_ind, t.del_ind, ssra.shelf_specn_id
    FROM base_shelf_tmplt bst, tmplt t, shelf_specn_revsn_alt ssra
    WHERE bst.tmplt_id = t.tmplt_id
    AND bst.tmplt_id = pTmpltId
    AND bst.shelf_specn_revsn_alt_id = ssra.shelf_specn_revsn_alt_id
    AND t.base_tmplt_ind = 'Y';
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.get_base_shelf_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_base_shelf_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END get_base_shelf_tmplt;
  
  PROCEDURE insert_base_card_tmplt (pTmpltId IN base_card_tmplt.tmplt_id%TYPE, pCardSpecnRevsnAltId IN base_card_tmplt.card_specn_revsn_alt_id%TYPE) AS

    
    BEGIN
        
      insert into base_card_tmplt (tmplt_id, card_specn_revsn_alt_id)
      values (pTmpltId, pCardSpecnRevsnAltId);
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_card_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_card_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_card_tmplt;
  
  PROCEDURE insert_base_node_tmplt (pTmpltId IN base_node_tmplt.tmplt_id%TYPE, pNodeSpecnRevsnAltId IN base_node_tmplt.node_specn_revsn_alt_id%TYPE) AS

    
    BEGIN
        
      insert into base_node_tmplt (tmplt_id, node_specn_revsn_alt_id)
      values (pTmpltId, pNodeSpecnRevsnAltId);
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_node_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_node_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_node_tmplt;
  
  PROCEDURE insert_base_shelf_tmplt (pTmpltId IN base_shelf_tmplt.tmplt_id%TYPE, pShelfSpecnRevsnAltId IN base_shelf_tmplt.shelf_specn_revsn_alt_id%TYPE) AS

    
    BEGIN
        
      INSERT INTO base_shelf_tmplt (tmplt_id, shelf_specn_revsn_alt_id)
      (SELECT pTmpltId, ssra.shelf_specn_revsn_alt_id
      FROM shelf_specn_revsn_alt ssra
      WHERE ssra.shelf_specn_id = pShelfSpecnRevsnAltId);
    
      --commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_shelf_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_shelf_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_shelf_tmplt;
  
  PROCEDURE insert_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, pPlgInRoleTypId IN base_plg_in_tmplt.plg_in_role_typ_id%TYPE,
    pCnctrTypId IN base_plg_in_tmplt.cnctr_typ_id%TYPE, pBiDrctnlInd IN base_plg_in_tmplt.bi_drctnl_ind%TYPE) AS
  
    p_CnctrTypId Number;
    p_BiDrctnlInd char(1);
    
    BEGIN
      
      p_CnctrTypId := nvl(pCnctrTypId,1);
      p_BiDrctnlInd := nvl(pBiDrctnlInd,'N');
        
      insert into base_plg_in_tmplt (tmplt_id, plg_in_role_typ_id, cnctr_typ_id, bi_drctnl_ind)
      values (pTmpltId, pPlgInRoleTypId, p_CnctrTypId, p_BiDrctnlInd);
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_plg_in_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_plg_in_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_plg_in_tmplt;
  
  PROCEDURE update_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, pPlgInRoleTypId IN base_plg_in_tmplt.plg_in_role_typ_id%TYPE,
    pCnctrTypId IN base_plg_in_tmplt.cnctr_typ_id%TYPE, pBiDrctnlInd IN base_plg_in_tmplt.bi_drctnl_ind%TYPE) AS
   
    BEGIN
      
      update base_plg_in_tmplt set cnctr_typ_id = pCnctrTypId, bi_drctnl_ind = pBiDrctnlInd
      where tmplt_id = pTmpltid and plg_in_role_typ_id = pPlgInRoleTypId;
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.update_base_plg_in_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'update_base_plg_in_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END update_base_plg_in_tmplt;
  
  PROCEDURE get_base_plg_in_tmplt (pTmpltId IN base_plg_in_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor) AS
    BEGIN
    OPEN retCsr FOR
    
    select tmplt_id, plg_in_role_typ_id, cnctr_typ_id, bi_drctnl_ind
    from base_plg_in_tmplt
    where tmplt_id = pTmpltId;
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.get_base_plg_in_tmplt', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_base_plg_in_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END get_base_plg_in_tmplt;
  
  PROCEDURE insert_base_plg_in_tmplt_prts (pTmpltId IN base_plg_in_tmplt_prts.tmplt_id%TYPE, pXmtRecvInd IN base_plg_in_tmplt_prts.xmt_recv_ind%TYPE,
    pXCoordNo IN base_plg_in_tmplt_prts.x_coord_no%TYPE, pYCoordNo IN base_plg_in_tmplt_prts.y_coord_no%TYPE,
    pTmpltDefId OUT base_plg_in_tmplt_prts.tmplt_def_id%TYPE) AS
    
    BEGIN
      
      insert into base_plg_in_tmplt_prts (tmplt_id, xmt_recv_ind, x_coord_no, y_coord_no)
      values (pTmpltId, pXmtRecvInd, pXCoordNo, pYCoordNo);
      
      select max(tmplt_def_id) into pTmpltDefId from base_plg_in_tmplt_prts;
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.insert_base_plg_in_tmplt_prts', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_base_plg_in_tmplt_prts Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_base_plg_in_tmplt_prts;
  
  PROCEDURE update_base_plg_in_tmplt_prts (pTmpltDefId IN base_plg_in_tmplt_prts.tmplt_def_id%TYPE, pXmtRecvInd IN base_plg_in_tmplt_prts.xmt_recv_ind%TYPE,
    pXCoordNo IN base_plg_in_tmplt_prts.x_coord_no%TYPE, pYCoordNo IN base_plg_in_tmplt_prts.y_coord_no%TYPE) AS
   
    BEGIN
      
      update base_plg_in_tmplt_prts set xmt_recv_ind = pXmtRecvInd, x_coord_no = pXCoordNo, y_coord_no = pYCoordNo
      where tmplt_def_id = pTmpltDefId;
    
      commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.update_base_plg_in_tmplt_prts', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'update_base_plg_in_tmplt_prts Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END update_base_plg_in_tmplt_prts;
  
  PROCEDURE get_base_plg_in_tmplt_prts (pTmpltDefId IN base_plg_in_tmplt_prts.tmplt_def_id%TYPE, retCsr OUT ref_cursor) AS
    BEGIN
    OPEN retCsr FOR
    
    select tmplt_def_id, tmplt_id, xmt_recv_ind, x_coord_no, y_coord_no
    from base_plg_in_tmplt_prts
    where tmplt_def_id = pTmpltDefId;
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.get_base_plg_in_tmplt_prts', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_base_plg_in_tmplt_prts Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END get_base_plg_in_tmplt_prts;
  
  PROCEDURE get_shelf_slot_specn(pShlfSpcnId IN NUMBER, retCsr OUT ref_cursor) AS
  BEGIN
    OPEN retCsr FOR
    SELECT sswsd.slot_specn_id, ss.hgt_no, ss.wdth_no, sswss.slot_no, sswss.x_coord_no, sswss.y_coord_no, sswss.label_nm, 
    du.dim_uom_cd, sswss.shelf_specn_with_slts_slts_id
    FROM shelf_specn_with_slts_def sswsd, shelf_specn_with_slts_slts sswss, slot_specn ss, dim_uom du
    WHERE sswsd.shelf_specn_id = pShlfSpcnId
    AND sswsd.shelf_specn_with_slts_def_id = sswss.shelf_specn_with_slts_def_id
    AND sswsd.slot_specn_id = ss.slot_specn_id
    AND ss.dim_uom_id = du.dim_uom_id
    ORDER BY sswsd.slot_seq_no, sswss.slot_no;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log(SQLERRM, 'ERROR', 'BASE_TMPLT_PKG.get_shelf_slot_specn', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_shelf_slot_specn Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  END get_shelf_slot_specn;

END BASE_TMPLT_PKG;
/
