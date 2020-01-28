CREATE OR REPLACE PACKAGE TMPLT_PKG AS

  TYPE REF_CURSOR IS REF CURSOR;

  PROCEDURE get_status_options(retCsr out ref_cursor);
  
  PROCEDURE insert_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltTyp IN VARCHAR2, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
    pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
    pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
    pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId OUT tmplt.tmplt_id%TYPE);
  
  PROCEDURE update_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
    pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
    pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
    pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId IN tmplt.tmplt_id%TYPE);
  
  PROCEDURE get_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor);
  
  PROCEDURE search_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltTypId IN tmplt.tmplt_typ_id%TYPE, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
    pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
    pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
    pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId IN tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor);

  PROCEDURE search_base_tmplt (pSpecId NUMBER,pSpecTyp VARCHAR2 ,pBaseTmpId OUT NUMBER);
  PROCEDURE search_tmplt_name (pTmpNm VARCHAR2,resID OUT NUMBER);
  
END TMPLT_PKG;
/
CREATE OR REPLACE PACKAGE BODY TMPLT_PKG AS

  PROCEDURE insert_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltTyp IN VARCHAR2, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
  pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
  pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
  pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId OUT tmplt.tmplt_id%TYPE) AS
  
      p_HlpTmpltInd char(1);
      p_ComnCnfgTmpltInd char(1);
      p_CmpltInd char(1);
      p_PrpgtInd char(1);
      p_UpdtInPrgsInd char(1);
      p_RetTmpltInd char(1);
      p_DelInd char(1);
      tmpltTypId NUMBER;
    
    BEGIN
      p_HlpTmpltInd := nvl(pHlpTmpltInd,'N');
      p_ComnCnfgTmpltInd := nvl(pComnCnfgTmpltInd,'N');
      p_CmpltInd := nvl(pCmpltInd,'N');
      p_PrpgtInd := nvl(pPrpgtInd,'N');
      p_UpdtInPrgsInd := nvl(pUpdtInPrgsInd,'N');
      p_RetTmpltInd := nvl(pRetTmpltInd,'N');
      p_DelInd := nvl(pDelInd,'N');
      
      SELECT t.tmplt_typ_id INTO tmpltTypId
      FROM tmplt_typ t
      WHERE t.tmplt_typ = pTmpltTyp;
        
      insert into tmplt(tmplt_nm, tmplt_typ_id, tmplt_dsc, base_tmplt_ind, hlp_tmplt_ind, 
      comn_cnfg_tmplt_ind, cmplt_ind, prpgt_ind, updt_in_prgs_ind, ret_tmplt_ind, del_ind)
      values (pTmpltNm, tmpltTypId, pTmpltDsc, pBaseTmpltInd, p_HlpTmpltInd, p_ComnCnfgTmpltInd,
      p_CmpltInd, p_PrpgtInd, p_UpdtInPrgsInd, p_RetTmpltInd, p_DelInd);
      
      select max(tmplt_id) into pTmpltId from tmplt;
    
      --commit;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.INSERT_TMPLT', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END insert_tmplt;
  
  PROCEDURE update_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
    pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
    pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
    pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId IN tmplt.tmplt_id%TYPE) AS
  
    BEGIN
        
      update tmplt set tmplt_nm = pTmpltNm, tmplt_dsc = pTmpltDsc, base_tmplt_ind = pBaseTmpltInd,
      hlp_tmplt_ind = pHlpTmpltInd, comn_cnfg_tmplt_ind = pComnCnfgTmpltInd, cmplt_ind = pCmpltInd, prpgt_ind = pPrpgtInd,
      updt_in_prgs_ind = pUpdtInPrgsInd, ret_tmplt_ind = pRetTmpltInd, del_ind = pDelInd
      where tmplt_id = pTmpltId;
    
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.UPDATE_TMPLT', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'update_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END update_tmplt;
  
  PROCEDURE get_tmplt (pTmpltId IN base_bay_tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor) AS
    BEGIN
    OPEN retCsr FOR
    
    select tmplt_id, tmplt_nm, tmplt_typ_id, tmplt_dsc, base_tmplt_ind, hlp_tmplt_ind, comn_cnfg_tmplt_ind,
    cmplt_ind, prpgt_ind, updt_in_prgs_ind, ret_tmplt_ind, del_ind
    from tmplt
    where tmplt_id = pTmpltId;
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.GET_TMPLT', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END get_tmplt;
  
  PROCEDURE search_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltTypId IN tmplt.tmplt_typ_id%TYPE, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
      pBaseTmpltInd IN tmplt.base_tmplt_ind%TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind%TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind%TYPE,
      pCmpltInd IN tmplt.cmplt_ind%TYPE, pPrpgtInd IN tmplt.prpgt_ind%TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind%TYPE,
      pRetTmpltInd IN tmplt.ret_tmplt_ind%TYPE, pDelInd tmplt.del_ind%TYPE, pTmpltId IN tmplt.tmplt_id%TYPE, retCsr OUT ref_cursor) AS
      
      andclause varchar2(2000);
      myquery varchar2(2000);
    
    BEGIN
    
      andclause := '';
      andclause := andclause || (CASE WHEN pTmpltId Is null THEN '' ELSE ' and tm.tmplt_id like ''%' || pTmpltId || '%''' END);
      andclause := andclause || (CASE WHEN '%' = pTmpltNm THEN '' ELSE ' and upper(tm.tmplt_nm) like ''%'' || upper(replace(trim(''' || pTmpltNm || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN pTmpltTypId Is null THEN '' ELSE ' and tm.tmplt_typ_id = ' || pTmpltTypId || ' ' END);
      andclause := andclause || (CASE WHEN '%' = pTmpltDsc THEN '' ELSE ' and upper(tm.tmplt_dsc) like ''%'' || upper(replace(trim(''' || pTmpltDsc || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pBaseTmpltInd THEN '' ELSE ' and upper(tm.base_tmplt_ind) like ''%'' || upper(replace(trim(''' || pBaseTmpltInd || '''),'' '',''%'')) || ''%''' END);
     -- andclause := andclause || (CASE WHEN '%' = pHlpTmpltInd THEN '' ELSE ' and upper(tm.hlp_tmplt_ind) like ''%'' || upper(replace(trim(''' || pHlpTmpltInd || '''),'' '',''%'')) || ''%''' END);
     -- andclause := andclause || (CASE WHEN '%' = pComnCnfgTmpltInd THEN '' ELSE ' and upper(tm.comn_cnfg_tmplt_ind) like ''%'' || upper(replace(trim(''' || pComnCnfgTmpltInd || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pCmpltInd THEN '' ELSE ' and upper(tm.cmplt_ind) like ''%'' || upper(replace(trim(''' || pCmpltInd || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pPrpgtInd THEN '' ELSE ' and upper(tm.prpgt_ind) like ''%'' || upper(replace(trim(''' || pPrpgtInd || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pUpdtInPrgsInd THEN '' ELSE ' and upper(tm.updt_in_prgs_ind) like ''%'' || upper(replace(trim(''' || pUpdtInPrgsInd || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pRetTmpltInd THEN '' ELSE ' and upper(tm.ret_tmplt_ind) like ''%'' || upper(replace(trim(''' || pRetTmpltInd || '''),'' '',''%'')) || ''%''' END);
      andclause := andclause || (CASE WHEN '%' = pDelInd THEN '' ELSE ' and upper(tm.del_ind) like ''%'' || upper(replace(trim(''' || pDelInd || '''),'' '',''%'')) || ''%''' END);
      
      myquery := 'select tm.tmplt_id, tm.tmplt_nm, tm.tmplt_typ_id,tp.tmplt_typ, tm.tmplt_dsc, 
                  Case tm.base_tmplt_ind
                      When ''Y'' Then ''Base Template''
                      When ''N'' Then ''Overall Template''
                  End base_tmp, 
                  tm.hlp_tmplt_ind, tm.comn_cnfg_tmplt_ind, 
                  Case 
                    When (tm.cmplt_ind = ''Y'' AND tm.prpgt_ind = ''N'' AND tm.updt_in_prgs_ind = ''N'' 
                         AND tm.ret_tmplt_ind = ''N'' AND tm.del_ind = ''N'')  Then ''Completed'' 
                    When (tm.cmplt_ind = ''N'' AND tm.prpgt_ind = ''Y'' AND tm.updt_in_prgs_ind = ''N'' 
                         AND tm.ret_tmplt_ind = ''N'' AND tm.del_ind = ''N'') Then ''Propagate''
                    When (tm.cmplt_ind = ''N'' AND tm.prpgt_ind = ''N'' AND tm.updt_in_prgs_ind = ''Y'' 
                         AND tm.ret_tmplt_ind = ''N'' AND tm.del_ind = ''N'') Then ''Update In-Progress'' 
                    When (tm.cmplt_ind = ''N'' AND tm.prpgt_ind = ''N'' AND tm.updt_in_prgs_ind = ''N'' 
                         AND tm.ret_tmplt_ind = ''Y'' AND tm.del_ind = ''N'') Then ''Retired''
                    When (tm.cmplt_ind = ''N'' AND tm.prpgt_ind = ''N'' AND tm.updt_in_prgs_ind = ''N'' 
                         AND tm.ret_tmplt_ind = ''N'' AND tm.del_ind = ''Y'') Then ''Deleted''                             
                    Else ''''     
                  End tmp_Status   
                  from tmplt tm ,tmplt_typ tp
                    where 1=1 AND tp.tmplt_typ_id = tm.TMPLT_TYP_ID ' || andclause || ' order by tm.tmplt_id';
    
    OPEN retCsr FOR    
      myquery;
  
    EXCEPTION
        WHEN OTHERS THEN
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.SEARCH_TMPLT', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'search_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
  
  END search_tmplt;

PROCEDURE get_status_options(retCsr out ref_cursor)
  AS
  
  BEGIN 
  
    open retCsr for 
      select ' ' as STATUS from dual
      union select 'Completed' as STATUS from dual
      union select 'Propagated' as STATUS from dual
      union select 'In-Progress' as STATUS from dual
      union select 'Retired' as STATUS from dual
    ;
    exception
      when others then
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.GET_STATUS_OPTIONS', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'get_status_options Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
             
  END get_status_options;

PROCEDURE search_base_tmplt (pSpecId NUMBER,pSpecTyp VARCHAR2 ,pBaseTmpId OUT NUMBER)
AS 
  
  BEGIN 
   
    pBaseTmpId := 0;  
  
   IF(pSpecTyp = 'BAY') THEN
        SELECT bbt.tmplt_id INTO pBaseTmpId
        FROM base_bay_tmplt bbt, bay_specn_revsn_alt bsra
        WHERE bbt.bay_specn_revsn_alt_id = bsra.bay_specn_revsn_alt_id
        AND bsra.bay_specn_id = pSpecId;
  
   ELSIF(pSpecTyp = 'PLUG-IN') THEN
       Select 0 Into pBaseTmpId from dual;
         
   ELSIF(pSpecTyp = 'SHELF') THEN
        SELECT bst.tmplt_id INTO pBaseTmpId
        FROM base_shelf_tmplt bst, shelf_specn_revsn_alt ssra
        WHERE bst.shelf_specn_revsn_alt_id = ssra.shelf_specn_revsn_alt_id
        AND ssra.shelf_specn_id = pSpecId;
         
   ELSIF(pSpecTyp = 'NODE') THEN
        Select TMPLT_ID Into pBaseTmpId From BASE_NODE_TMPLT Where NODE_SPECN_REVSN_ALT_ID = pSpecId;
         
   ELSIF(pSpecTyp = 'SLOT') THEN
       Select 0 Into pBaseTmpId from dual;
         
   ELSIF(pSpecTyp = 'CARD') THEN
        Select TMPLT_ID Into pBaseTmpId From BASE_CARD_TMPLT Where CARD_SPECN_REVSN_ALT_ID = pSpecId;  
           
   End If;   
   
   
   EXCEPTION        
      when NO_DATA_FOUND then
          pBaseTmpId := 0;
              
      when others then
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.SEARCH_BASE_TMPLT', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'search_base_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
             
  END search_base_tmplt;

PROCEDURE search_tmplt_name (pTmpNm VARCHAR2,resID OUT NUMBER)
AS 
     pTmpName varchar2(200); 
     
  BEGIN  
    
    SELECT TMPLT_NM INTO pTmpName
      FROM TMPLT Where trim(TMPLT_NM) = trim(pTmpNm);  

   If pTmpName = '' OR pTmpName Is Null Then
        resID := 0;
   Else
       resID := 1;
   End If;
   
   
   EXCEPTION        
      when NO_DATA_FOUND then
           resID := 0;
              
      when others then
             insert_cdmms_log('insert_tmplt Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200), 'ERROR', 'TMPLT_PKG.search_tmplt_name', 'MTL_EDTR', 'DB', NULL);
             raise_application_error(-20001, 'search_tmplt_name Failed. ' || SQLCODE || ' : ' || SUBSTR(SQLERRM, 1, 200));
             
  END search_tmplt_name;
  
END TMPLT_PKG;
/
