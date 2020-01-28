define(['jquery','../../Scripts/jquery.tmpl.min','../../Scripts/AppPopOutWindow'], function ($, jqueryTmpl,popOut) {
    CadEquipPropertiesDialogHandle = null;
    var CadEquipPropertiesDialog = function () {
        CadEquipPropertiesDialogHandle = this;
        this.$handle = null;
        this.config = {};
        this.isLoaded = false;
        this.selected = 0;
    }
    CadEquipPropertiesDialog.prototype.showStatus = function (msg) {
    }
    CadEquipPropertiesDialog.prototype.showError = function (err) {
        var msg = "";
        if (Array.isArray(err)) {
            msg = err.join("<br/>");
        }
        else if (err instanceof Object) {
            if (err.EC) {
                msg = err.MSG+"<br/>"+err.OV;
            }
        }
        else if( typeof err === 'string' ) {
            msg = err;
        }
    }
    CadEquipPropertiesDialog.prototype.clearAll = function () {
    }
    
    CadEquipPropertiesDialog.prototype.onBeforeOpen = function () {
        var $this = this;
        
        this.isLoaded = (this.$handle != null);
        this.$handle = (this.$handle||$("#CadEquipPropertiesDialog"));
    }

    CadEquipPropertiesDialog.prototype.loadProperties = function() { 
        if( this.$handle.find('.ui-tabs').length === 0 ) {
            this.$handle.find(".tabs-props").tabs();
        }

        var $tmplt = $("#TMPLT_EQUIP_PROPS");
        var $container = this.$handle.find('#tabPROPS_INFO_LOC .container');
        $container.empty();
        $tmplt.tmpl(this.config).appendTo($container);

        var openPopOutWindow = function($btn) {
                var id = $btn.data('id');
                var page = $btn.data('page');
                popOut.Open(page+'_'+id, page, id);
        }
        $container.find(".btnOPEN_MTRLINV").click(function () {
                openPopOutWindow($(this));
        });
        $container.find(".btnOPEN_SPEC").click(function () {
                openPopOutWindow($(this));
        });

        this.clearAll();

    }
    CadEquipPropertiesDialog.prototype.adjustPosition = function() { 
        var $p =  this.$handle.parents(":first");
        //$p.css({ left: 1093.5, top: 1530.5 });
    }
    CadEquipPropertiesDialog.prototype.onOK = function () {
        if (!this.config.onOK(this.selected)) {
            return;
        }
        
        this.close();
    }
    CadEquipPropertiesDialog.prototype.onCANCEL = function () {
        this.close()
    }
    CadEquipPropertiesDialog.prototype.close = function () {
        this.$handle.dialog('close');
    }

    CadEquipPropertiesDialog.prototype.Display = function (props) {
        
        CadEquipPropertiesDialogHandle = this;

        $.extend(this.config, props)
        this.$handle = (this.$handle||$("#CadEquipPropertiesDialog"));

        this.config.title = 'Equipment Properties';
        this.config.autoOpen = false;
        this.config.modal = false;
        this.config.width = 560;
        this.config.height = 720;
        this.config.open = function() { CadEquipPropertiesDialogHandle.onBeforeOpen(); }
        this.config.buttons = [];
        this.config.buttons.push({ text: 'Apply', click: function() { CadEquipPropertiesDialogHandle.onOK(); } });
        //this.config.buttons.push({ text: 'Cancel', click: function() { CadEquipPropertiesDialogHandle.onCANCEL(); }});
        this.config.onOK = (this.config.onOK || function() { return true; });
        this.config.onCANCEL = (this.config.onCANCEL || function() { });
        this.$handle.dialog(this.config);
        this.$handle.dialog('open');

        this.adjustPosition();
        this.loadProperties();
    }
    return new CadEquipPropertiesDialog();
});
