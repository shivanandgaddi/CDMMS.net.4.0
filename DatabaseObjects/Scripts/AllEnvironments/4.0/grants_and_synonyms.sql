create public synonym base_tmplt_pkg for cdmms_owner.base_tmplt_pkg;
create public synonym tmplt_pkg for cdmms_owner.tmplt_pkg;
create public synonym overl_bay_pkg for cdmms_owner.overl_bay_pkg;
 
grant execute on cdmms_owner.base_tmplt_pkg to cdmms_app;
grant execute on cdmms_owner.tmplt_pkg to cdmms_app;
grant execute on cdmms_owner.overl_bay_pkg to cdmms_app;

drop table shelf_specn_with_slts cascade constraints;
drop public synonym shelf_specn_with_slts;