if (!String.prototype.toTitleCase) {
    String.prototype.toTitleCase = function(str) {
        str = (str||this);
        return str.toLowerCase().split(' ').map(function(s) { return s.charAt(0).toUpperCase()+s.substring(1); }).join(' ');
    }
}
if (!String.prototype.toCamelCase) {
    String.prototype.toCamelCase = function(str) {
        str = (str||this);
        var space = str.indexOf(' ') > 0;
        var under = str.indexOf('_') > 0;
        var dash = str.indexOf('-') > 0;
        var delim = (dash ? '-' : under ? '_' : space ? ' ' : null);
        if (!delim) {
            return str;
        }

        return str.toLowerCase().split(delim).map(function(s) { return s.charAt(0).toUpperCase()+s.substring(1); }).join('');
    }
}
define(['jquery','../../Scripts/jquery.tmpl.min'], function ($, jqueryTmpl) {
    CadAddEquipDialogHandle = null;
    var CadAddEquipDialog = function () {
        CadAddEquipDialogHandle = this;

        this.$handle = null;
        this.config = {};

        this.$searchBox = null;
        this.$errors = null;
        this.$results = null;
        this.$status = null;
        this.$spinner = null;
        this.$context  = null;

        this.isLoaded = false;
        this.selected = 0;
    }
    CadAddEquipDialog.prototype.onAutoSearch = function () {
        this.isUserTyping = true;

        var match = this.$searchBox.val();
        match = $.trim(match);
        if (match.length < 3) {
            return;
        }

        if (this.timer) {
            clearTimeout(this.timer);
        }
        
        var $this = this;
        var waitForUserToStopTyping = function () {
            $this.isUserTyping = false;
            $this.doSearch();
        }
        this.timer = setTimeout(waitForUserToStopTyping, 250);
    }
    CadAddEquipDialog.prototype.showStatus = function (msg) {
        this.$spinner.hide();
        this.$status.text(msg);
    }
    CadAddEquipDialog.prototype.showError = function (err) {
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

        this.$errors.html(msg);
    }
    CadAddEquipDialog.prototype.clearResults = function () {
        this.$results.empty();
    }
    CadAddEquipDialog.prototype.clearErrors = function () {
        this.$errors.empty();
    }
    CadAddEquipDialog.prototype.clearStatus = function () {
        this.$status.text('');
        this.$spinner.hide();
        this.selected = null;
    }
    CadAddEquipDialog.prototype.clearAll = function () {
        this.clearResults();
        this.clearErrors();
        this.clearStatus();
    }
    CadAddEquipDialog.prototype.doSearch = function () {
        var $this = this;

        var match = this.$searchBox.val();
        match = $.trim(match);
        if (match.length < 3) {
            this.clearAll();
            return;
        }

        var show = function (data) {
            if (this.isUserTyping ) {
                return;// user started typing again... ignore any previous results...
            }

            $this.clearAll();

            if (data.length === 1) { // might be an error
                var status = data[0]; 
                if( status.EC ) {
                    $this.showError(status);
                    return;
                }
            }
            //var $container = $this.$handle.find('.results');
            //data.forEach(function (rec) {
            //    $("<li>").text(JSON.stringify(rec)).appendTo($container);
            //});
            
            var $tmplt = $("#TMPLT_TEMPLATE_ITEMS_SEARCH_RESULTS");
            $tmplt.tmpl(data).appendTo($this.$results);

            var $btns = $this.$results.find('.btn-success').click(function () {
                $this.selected = $(this).data('rec');
                $this.onOK();
            })

            $this.showStatus('records found: '+data.length);
        }
        var error = function (err) {
            if (Array.isArray(err)) {
                err = err.join("<br/>");
            }
            else if( err instanceof Object ) {
                err = JSON.stringify(err);
            }
            else if( typeof err === 'string' ) {
                err = err.split('\n').join('<br/>');
            }
            
            $this.$errors.html(err);
        }
        this.$spinner.show();
        this.$status.text('searching...');

        this.isSearchInProgress = true;
        var cat = this.config.typ.toLowerCase();
        var context = this.$context.val().toLowerCase();

        // dont really need 'cat' as the tmpltId will tellyou the 'cat'; remove this later...
        var route = 'api/tmplt/search/equip/'+this.config.tmpltId+'/'+cat+'/'+context+'/'+encodeURI(match);
        $.ajax({ type: 'GET',
                url: route,
                //data: JSON.stringify([]),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                context: $this,
                async: true
        })
        .done(show)
        .fail(error)
        .complete(function () {
        });
    }
    
    CadAddEquipDialog.prototype.onBeforeOpen = function () {
        var $this = this;
        
        var isLoaded = (this.$searchBox != null);

        this.$searchBox = (this.$searchBox||this.$handle.find('#txtSEARCH_BOX'));
        this.$context = (this.$context||this.$handle.find('#ddCONTEXT'));
        this.$errors = (this.$errors||this.$handle.find('.errors'));
        this.$results = (this.$restults||this.$handle.find('.results'));
        this.$status = (this.$status||this.$handle.find('.status'));
        this.$spinner = (this.$spinner||this.$handle.find('.fa-spinner'));

        var typ = ((this.config.typ||'').toTitleCase()+' Equipment Choices').toTitleCase();
        this.$handle.find('#lblTYP').text(typ);

        var placeholder = 'Search for '+this.config.typ.toLowerCase()+' choices...';
        this.$searchBox.attr('placeholder', placeholder).val('');

        if( !isLoaded ) {
            this.$searchBox.keyup(function () { $this.onAutoSearch(); })
            
            "Any,Bay Internal,Node,Shelf".split(',').forEach(function (choice) { // CHECK HERE; replace with fetch
                $("<option>").val(choice).text(choice).appendTo($this.$context);
            });

            this.$context.change(function() { $this.doSearch(); });
        }

        this.clearAll();
    }
    CadAddEquipDialog.prototype.onOK = function () {

        if (!this.config.onOK(this.selected)) {
            return;
        }
        
        this.close();
    }
    CadAddEquipDialog.prototype.onCANCEL = function () {
        this.close()
    }
    CadAddEquipDialog.prototype.close = function () {
        this.$handle.dialog('close');
        this.clearAll();
    }

    CadAddEquipDialog.prototype.Display = function (props) {
        $.extend(this.config, props)
        this.$handle = (this.$handle||$("#CadAddEquipDialog"));

        this.config.autoOpen = false;
        this.config.modal = true;
        this.config.width = 850;
        this.config.height = 850;
        this.config.open = function() { CadAddEquipDialogHandle.onBeforeOpen(); }
        this.config.buttons = [];
        //this.config.buttons.push({ text: 'Add', click: function() { CadAddEquipDialogHandle.onOK(); }});
        this.config.buttons.push({ text: 'Cancel', click: function() { CadAddEquipDialogHandle.onCANCEL(); }});
        this.config.onOK = (this.config.onOK || function() { return true; });
        this.config.onCANCEL = (this.config.onCANCEL || function() { });
        this.$handle.dialog(this.config);
        this.$handle.dialog('open');
    }
    return new CadAddEquipDialog();
});
