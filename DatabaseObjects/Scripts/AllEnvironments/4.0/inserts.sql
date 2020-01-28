INSERT INTO options_ref (json_name, sql)
VALUES ('rtnAngl', 'SELECT rottn_angl_id AS option_value, rottn_angl_dgr_no AS option_text, dflt_ind FROM rottn_angl WHERE vsbl_ind = ''Y'' ORDER BY 2');

INSERT INTO system_configuration (application_config_id, application_nm, config_key, config_value, last_updtd_userid, last_updtd_tmstmp)
VALUES (1, 'CDMMS_DRAWING', 'canvasWidthInPixels', '1024', 'tvitry', SYSDATE);

INSERT INTO system_configuration (application_config_id, application_nm, config_key, config_value, last_updtd_userid, last_updtd_tmstmp)
VALUES (2, 'CDMMS_DRAWING', 'canvasHeightInPixels', '1024', 'tvitry', SYSDATE);

INSERT INTO system_configuration (application_config_id, application_nm, config_key, config_value, last_updtd_userid, last_updtd_tmstmp)
VALUES (3, 'CDMMS_DRAWING', 'percentOfCanvasToUse', '98', 'tvitry', SYSDATE);