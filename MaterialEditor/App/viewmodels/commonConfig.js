// #region jQuery convenience functions
$.fn.escape = function (callback) {
    return this.each(function () {
        $(document).on('keydown', this, function (e) {
            var keycode = ((typeof e.keyCode != 'undefined' && e.keyCode) ? e.keyCode : e.which);
            if (keycode === 27) {
                callback.call(this, e);
            }
        });
    });
}
$.fn.selectOption = function (val, ignoreSetDefault) {
    return this.each(function () {
        var $c = $(this);
        if ($c.prop('tagName') !== 'SELECT') {
            return $c;
        }

        if (typeof val === 'object') {
            val = (val.val || val.value || val.txt || val.text);
        }

        if (!ignoreSetDefault && (!val || val === "" || val <= 0)) {
            $opt = $($c.find("option")[0]);
            $opt.prop('selected', true);
            return;
        }

        var $opt = $c.find("option[value='" + val + "']");
        if ($opt.length === 0) {
            $opt = $c.find('option:contains("' + val + '")');
        }
        if ($opt.length > 0) {
            $opt.prop('selected', true);
        }
        else {
            $c.val(val).trigger('change');
        }

        return $c;
    });
}
$.fn.clearSelectedOption = function () {
    return this.each(function () {
        var $c = $(this);
        if ($c.prop('tagName') !== 'SELECT') {
            return $c;
        }

        $c.find("option:selected").prop('selected', false);
        return $c;
    });
}
$.fn.getSelectedOption = function () {
    var $c = $(this);
    var $sel = $c.find("option:selected");
    if ($sel.length === 0) {
        return null;
    }
    return $sel;
}
// #endregion jQuery convenience functions


// #region javascript array convenience functions
Array.prototype.insertArrayAt = function (pos, arr) {
    var myself = this;
    var args = [pos, 0].concat(arr);
    myself.splice.apply(myself, args);
    return this;
}
Array.prototype.createNewArray = function () {
    var myself = this;
    //var arr = myself.map(a=>Object.assign({}, a));  won't work in IE
    var json = JSON.stringify(myself); // sledgehammer...
    var arr = JSON.parse(json);
    return arr;
}
Array.prototype.removeItem = function (comparer) {
    var self = this;
    var ndx = self.findIndex(comparer);
    if (ndx >= 0) {
        self.splice(ndx, 1);
    }
    return self;
}
Array.prototype.findItem = function (comparer) {
    var self = this;
    var ndx = self.findIndex(comparer);
    if (ndx < 0)
        return null;
    return self[ndx];
}
if (!Array.findIndex) { // IE doesn't have Array#findIndex
    Array.prototype.findIndex = function (comparer) {
        var myself = this;
        var ndx = -1;
        myself.some(function (e, i) {
            if (comparer(e)) {
                ndx = i;
                return true;
            }
        });
        return ndx;
    }
}
Array.prototype.contains = function (comparer) {
    var myself = this;
    if (myself.length === 0)
        return false;

    if (typeof comparer === "function") {
        return myself.findIndex(comparer) >= 0;
    }
    return myself.findIndex(function (item) { return item == comparer; }) >= 0; // double equals 
}

if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (search, this_len) {
        if (this_len === undefined || this_len > this.length) {
            this_len = this.length;
        }
        return this.substring(this_len - search.length, this_len) === search;
    };
}
// #endregion javascript array convenience functions

// #region Global helper functions
function createGUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
// #endregion Global help functions

// #region UI helper module
UiUtils = (function () {
    var _ui = null;

    function setupExpanders($fields) {
        $fields.each(function (ndx, item) {
            var $i = $(item);
            var $container = $i.find(".expander");
            var txt = $container.text();
            $container.text('').attr('title', 'Click to toggle child item list...');
            $("<a>")
                .click(function () {
                    var $a = $(this);
                    _ui.toggleChildItems($a, function (state) {
                        //var css = "alert-info";
                        //$a.toggleClass(css);
                        //$a.data('expanded', state);
                        var $p = $a.parents("tr:first");
                        //console.log("setupExpanders: toggleChildItems " + $p.data("expanded")+"/"+state);
                    });
                })
                .text(txt)
                .appendTo($container);
        });
    }
    function setupCoordInput($fields) {
        var CAT_MINOR = "Minor Material";
        var CAT_HLP = "High Level Part";
        $fields.each(function (ndx, item) {
            var $i = $(item);
            var cat = $i.data("cat");
            if (cat === CAT_MINOR)// cat === CAT_HLP)
                return;

            var $parent = $i.parents("tr:first");
            var $container = $i.siblings(".xy-coord-input");

            var coords = $i.data("coords").toString().split('/');
            var qty = parseInt($parent.find(".qty").val());

            var onEditCoords = function () {
                var $p = $i.parents("tr:first");
                var recId = $p.data("id");
                var seqNo = $p.data("seqno");
                var $qty = $p.find(".qty");
                var config = { ui: _ui, $row: $i, recId: recId, seqNo: seqNo, qty: $qty.val() };
                config.onUpdate = function (rec, coords) {
                    makeDisplayBox($container, coords);
                }
                EditCoordinatesDialog.Display(config);
            }

            // more than one, gets complicated...
            var makeDisplayBox = function ($ui, list) {
                $ui.empty();

                var len = list.length;
                if (len === 1) { // for demo purposes, we need to revamp when have more time...
                    var c = list[0];
                    $("<input type='text' class='form-control X_COORD_NO'>").val(c.y).appendTo($ui);
                    $("<span>&nbsp;</span>").appendTo($ui);
                    $("<input type='text' class='form-control Y_COORD_NO'>").val(c.x).appendTo($ui);
                }
            }

            var len = coords.length;
            for (var i = 0; i < len; i++) {
                var xy = coords[i].split(',');
                coords[i] = { x: xy[0], y: xy[1] };
            }
            makeDisplayBox($container, coords);
        });
    }

    function setupDropDowns($fields, opts) {
        var defVal = (opts ? opts.defVal || "" : "");
        // defaults...
        var filter = {
            hasNoValue: function ($o, val) {
                $o.selectOption(val);
                //$o.hide();
            }
            , hasValue: function ($o, val) {
                $o.selectOption(val);
                $o.show();
            }
        };

        $.extend(filter, opts);

        $fields.each(function (ndx, item) {
            var $i = $(item);
            if ($i.is(":visible") === false)
                return;

            var v = ($i.data("val") || "").toString();
            if (filter.print) {
                console.log("setupDropDowns: " + $i.attr("class") + "=" + v);
            }

            if (v && v.length > 0) {
                filter.hasValue($i, v);
            }
            else {
                filter.hasNoValue($i, v);
            }
        });

        if (opts.onChange) {
            $fields.unbind('change').change(opts.onChange);
        }
    }

    function fromCamelCaseToUpperCase(obj, prefix) {
        var pfx = { from: "", to: "" };
        $.extend(pfx, prefix);

        var rv = {};
        for (var k in obj) {
            var name = k.replace(pfx.from, "");
            name = pfx.to + name.replace(/\.?([A-Z]+)/g, function (x, y) { return "_" + y.toLowerCase() }).replace(/^_/, "").toUpperCase();
            rv[name] = obj[k];
        }
        return rv;
    }

    function toCamelCase(obj, prefix) {
        var pfx = { from: "", to: "" };
        $.extend(pfx, prefix);

        var rv = {};
        for (var k in obj) {
            var name = k.replace(pfx.from, pfx.to).toLowerCase();
            name = name.replace(/_([a-z])/g, function (m, w) { return w.toUpperCase(); });
            rv[name] = obj[k];
        }
        return rv;
    }

    function convertToDbProcParams(params, pfx) {
        return toCamelCase(params, { from: pfx, to: "p_" });
    }

    function getFormData($form, opts) {
        var data = {};
        var pfx = $form.data('pfx');
        var handler = { ignore: function () { return false; }, postProcess: function () { return 0; }, unCheckedValue: '' };
        $.extend(handler, opts);

        //var ignore = (opts && opts.ignore ? opts.ignore : function () { return false; });
        //var postProcess = (opts && opts.postProcess ? opts.postProcess : function () { return 0; });

        var $fields = $form.find("input,select,textarea");

        $fields.each(function (ndx, item) {
            var $i = $(item);

            var id = $i.attr("id");
            var val = $.trim($i.val());
            var type = $i.attr("type");

            if (handler.ignore(id))
                return;

            if (handler.removePrefix && pfx) {
                id = id.replace(pfx, "");
            }
            data[id] = (type === "radio" || type === "checkbox"
                ? ($i.is(":checked") ? 'Y' : (handler.unCheckedValue || ''))
                : val);
        });

        return data;
    }

    function isFormNotEmpty(form) {
        return getFormDataLength(form) > 0;
    }
    function isFormEmpty(form) {
        return getFormDataLength(form) === 0;
    }
    function getFormDataLength(form) {
        var len = 0;
        for (var i in form) {
            var val = $.trim(form[i]);
            len += (val === "-1" ? 0 : val.length);
        }
        return len;
    }

    function createRecord(baseRec, $ui) {
        var rec = {};
        $.extend(rec, baseRec);
        for (var col in rec) {
            var $c = $ui.find("." + col);
            if ($c.length && $c.is(":visible")) {
                rec[col] = $c.val();
            }
        }
        return rec;
    }
    function setUI(ui) {
        _ui = ui;

    }
    return {
        SetUI: setUI
        , GetFormData: getFormData
        , ConvertToDbProcParams: convertToDbProcParams
        , CreateRecord: createRecord
        , SetupDropDowns: setupDropDowns
        , SetupCoordInput: setupCoordInput
        , SetupExpanders: setupExpanders
        , IsFormNotEmpty: isFormNotEmpty
        , IsFormEmpty: isFormEmpty
        , GetFormDataLength: getFormDataLength
    };
})();
// #endregion UI helper module

// #region PopUp Dialogs for SelectConfigItemDialog
AffectedCommonConfigsDialog = (function () {
    var _$handle = null;
    var _config = null;

    function setCursor(cusor) {
        _$handle.css("cursor", (cusor || "default").toLowerCase());
    }
    function onBeforeShow() {
        // anything special do here before dialog is displayed...
        _$handle.find(".modal-title").html(_config.action + " Common Config: <b>" + _config.COMN_CNFG_NM + " (#" + _config.COMN_CNFG_ID + ")</b>");
        // anything special do here before dialog is displayed...
        var opts = {
            pageLength: 10
            , lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]]
            , scrollY: "360px"
            , scrollCollapse: true
            , retrieve: true
            , order: [[0, 'desc']]
        };

        $koPanel = _$handle.find('.ko-panel');
        $dtPanel = _$handle.find('.dt-panel');
        $dtPanel.empty();
        $dtPanel.html($koPanel.html());

        _dataTable = $dtPanel.find(".table").DataTable(opts);
    }
    function onCheckEnter() {
    }
    function onOK() {
        close();
    }
    function onClose() {
        _dataTable.clear();
        close();
    }
    function close() {
        _$handle.hide();
    }
    function isActive() {
        return _$handle && _$handle.is(":visible");
    }
    function init() {
        _$handle = $("#AffectedCommonConfigsDialog");
        _$handle.find(".btnOK").unbind('click').click(onOK);
        _$handle.find(".btnCLOSE,.btnX").unbind('click').click(onClose);
        _$handle.escape(onClose);
    }

    function display(config) {
        _config = config;
        init();
        _$handle.show(0, onBeforeShow);
    }

    return {
        Display: display
        , Close: onClose
        , IsActive: isActive
        , OnCheckEnter: onCheckEnter
    };
})();

CloneConfigDialog = (function () {
    var _$handle = null;
    var _config = null;

    function setCursor(cusor) {
        _$handle.css("cursor", (cusor || "default").toLowerCase());
    }
    function onBeforeShow() {
        // anything special do here before dialog is displayed...
        _$handle.find(".modal-title").html("Clone Common Config: <b>" + _config.COMN_CNFG_NM + " (#" + _config.COMN_CNFG_ID + ")</b>");
    }
    function onCheckEnter() {
    }
    function onOK() {
        var form = UiUtils.GetFormData(_$handle, { removePrefix: true });
        close();
        _config.ui.cloneConfig(form);
    }
    function onClose() {
        close();
    }
    function close() {
        _$handle.hide();
    }
    function isActive() {
        return _$handle && _$handle.is(":visible");
    }
    function init() {
        _$handle = $("#CloneConfigDialog");
        _$handle.find(".btnOK").unbind('click').click(onOK);
        _$handle.find(".btnCLOSE,.btnX").unbind('click').click(onClose);
        _$handle.escape(onClose);
    }

    function display(config) {
        _config = config;
        init();
        _$handle.show(0, onBeforeShow);
    }

    return {
        Display: display
        , Close: onClose
        , IsActive: isActive
        , OnCheckEnter: onCheckEnter
    };
})();
ChangeLogDialog = (function () {
    var _$handle = null;
    var _dataTable = null;
    var _config = null;

    function setCursor(cusor) {
        _$handle.css("cursor", (cusor || "default").toLowerCase());
    }
    function onBeforeShow() {
        // anything special do here before dialog is displayed...
        var opts = {
            pageLength: 10
            , lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]]
            , scrollY: "450px"
            , scrollCollapse: true
            , retrieve: true
            , order: [[0, 'desc']]
        };

        $koPanel = _$handle.find('.ko-panel');
        $dtPanel = _$handle.find('.dt-panel');
        $dtPanel.empty();
        $dtPanel.html($koPanel.html());

        _dataTable = $dtPanel.find(".table").DataTable(opts);

        _$handle.find(".modal-title").html("Change Log for <b>" + _config.COMN_CNFG_NM + " (#" + _config.COMN_CNFG_ID + ")</b>");
    }
    function onCheckEnter() {
    }
    function onClose() {
        _dataTable.clear();
        close();
    }
    function close() {
        _$handle.hide();
    }
    function isActive() {
        return _$handle && _$handle.is(":visible");
    }
    function init() {
        _$handle = $("#ChangeLogDialog");
        _$handle.find(".btnCLOSE,.btnX").unbind('click').click(onClose);
        _$handle.escape(onClose);
    }

    function display(config) {
        _config = config;
        init();
        _$handle.show(0, onBeforeShow);
    }

    return {
        Display: display
        , Close: onClose
        , IsActive: isActive
        , OnCheckEnter: onCheckEnter
    };
})();
SelectConfigItemDialog = (function () {
    var _$handle = null;
    var _$form = null;
    var _$dtPanel = null;
    var _config = null;
    var _pfx = null;
    var _selection = {};
    var _isSearching = false;
    var _koList = null;
    var _dataTable = null;

    function setCursor(cusor) {
        _$handle.css("cursor", (cusor || "default").toLowerCase());
    }
    function onBeforeShow() {
        // anything special do here before dialog is displayed...
        _$form = _$handle.find(".search-form");

        var ref = _$form.data("results");
        _koList = _config.ui[ref];

        onClear();
    }
    function onCheckEnter() {
        onSearch();
    }
    function onAddSelection() {
        if (_isSearching) {
            _config.ui.showMessage("Search in progress...");
            return;
        }
        var list = [];

        for (var i in _selection) {
            var rec = _selection[i];
            var $dt = _$handle.find(".dataTable")
            var $qty = $dt.find('#item' + rec.CDMMS_ID).find(".qty");
            var total = $qty.val();
            total = (isNaN(total) ? 1 : parseInt(total));

            rec.CNTND_IN_MTRL_QTY = ((total === 0 ? 1 : total) || 1);

            list.push(_selection[i]);
        }
        if (list.length === 0) {
            _config.ui.showMessage("You haven't selected any items!", "Add Child Items To Common Config");
            return;
        }
        _config.ui.onAddChildItems(_config.partType.name, list);

        onClose();
    }
    function onClear() {
        if (_isSearching) {
            _config.ui.showMessage("Search in progress...");
            return;
        }

        if (hasSelections() === false) {
            doClear();
            return;
        }

        _config.ui.confirm("Click OK to clear the current selection.", "Clear Current Selections", function (ans) {
            if (ans === "OK") {
                doClear();
            }
        });
    }

    function doClear() {
        setCursor("default");

        _$handle.find("input").val('');
        _$handle.find("select :nth-child(1)").prop('selected', true);
        _$handle.find('input[type=checkbox]').prop('checked', false);

        _selection = {};
        _isSearching = false;

        _koList([]); // clear..

        if (_dataTable) {
            _dataTable.clear().destroy();
            _dataTable = null;
            _$dtPanel.empty();
        }

    }
    function onToggleSelectChildCommonConfigItem(rec, e) {
        if (!rec) {
            _config.ui.showMessage("No record found for selection....");
        }
        var $e = (!e.length ? $(e) : e);

        var id = (rec.CDMMS_ID || rec.COMN_CNFG_ID);
        rec.CDMMS_ID = id; // normalize pk

        var key = "i" + id;
        _selection[key] = ($e.is(':checked') ? rec : null);
    }
    function setupCheckBoxClickEvents() {
        var self = SelectConfigItemDialog;
        var items = self.GetCurrentItemList();

        _$dtPanel = _$handle.find(".dt-table-panel");

        var monitor = function () {
            var $items = _$dtPanel.find("input[type=checkbox]");
            $items.click(function (e) {

                var $ckbx = $(e.target);
                var id = $ckbx.data('id');
                var rec = items.findItem(function (e) { return e.CDMMS_ID == id || e.COMN_CNFG_ID == id; });

                self.OnToggleSelectChildCommonConfigItem(rec, $ckbx);
            });
        }

        // we have to re-setup the click events when the user does anything to cause the table to redraw..
        var $tbl = _$dtPanel.find(".dataTable");
        $tbl = ($tbl.length > 1 ? $($tbl[1]) : $tbl); // dont know if we have to do this, but it works...
        $tbl.on('draw.dt', monitor).DataTable();

        monitor(); // kick it off!
    }
    function hasSelections() {
        var count = 0;
        for (var i in _selection) {
            count++;
        }
        return count > 0;
    }
    function onSearch() {
        if (_isSearching)
            return;

        if (!hasSelections()) {
            runSearch();
            return;
        }

        _config.ui.confirm("Click OK to clear the current selection and perform a new search.", "Clear Current Selections", function (ans) {
            if (ans === "OK") {
                runSearch();
            }
        });
    }
    function runSearch() {
        _koList([]) // clear...
        if (_dataTable) {
            _dataTable.clear().destroy();
            _dataTable = null;
        }

        var $ko_panel = _$handle.find(".ko-table-panel");
        var $dt_panel = _$handle.find(".dt-table-panel");

        var display = function (data) {
            _isSearching = false;

            setCursor("default");

            if (_$handle.is(":visible") === false) {
                return;
            }

            _config.ui.showWheelOfPatience(false);

            if (!data || data.length === 0) {
                _config.ui.showMessage("No material records found!");
                return;
            }
            if (data.ERR) {
                _config.ui.showMessage(data.ERR);
                return;
            }

            _koList(data); // fill

            $dt_panel.empty();
            $dt_panel.html($ko_panel.html());

            var $tbl = $dt_panel.find("table");

            _dataTable = $tbl.DataTable({
                pageLength: 5
                , lengthMenu: [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]]
                , scrollY: "195px"
                , scrollCollapse: true
                , retrieve: true
            });
            setupCheckBoxClickEvents();
        }
        var fatal = function (err) {
            _isSearching = false;

            setCursor("default");

            if (_$handle.is(":visible") === false) {
                return;
            }
            _config.ui.showWheelOfPatience(false);
            _config.ui.showMessage(err, "Search");

        }

        var pfx = _$form.data("pfx");
        var args = UiUtils.GetFormData(_$form);
        args = UiUtils.ConvertToDbProcParams(args, pfx);

        if (UiUtils.IsFormEmpty(args)) {
            _config.ui.showMessage("Please provide some search data...");
            return;
        }

        setCursor("wait");
        _isSearching = true;

        var search = _$form.data("search");
        _config.http.get('api/comncnfg' + search, args).then(display, fatal);
    }
    function onClose() {
        _isSearching = false;
        close();
    }
    function close() {
        doClear();
        _$handle.hide();
    }
    function isActive() {
        return _$handle && _$handle.is(":visible");
    }
    function init() {
        var ref = "SelectConfigItemDialog_" + _config.partType.name;
        if (_$handle && _$handle.attr("id") === ref) {
            return;
        }

        _$handle = $("#" + ref);
        _$handle.find(".btnCLOSE,.btnX").unbind('click').click(onClose);
        _$handle.find(".btnSEARCH").unbind('click').click(onSearch);
        _$handle.find(".btnCLEAR").unbind('click').click(onClear);
        _$handle.find(".btnADD").unbind('click').click(onAddSelection)
        _$handle.escape(onClose);

        _pfx = _$handle.data("pfx");
    }

    function display(config) {
        _config = config;
        init();
        _$handle.show(0, onBeforeShow);
    }
    return {
        Display: display
        , Close: onClose
        , IsActive: isActive
        , OnToggleSelectChildCommonConfigItem: onToggleSelectChildCommonConfigItem
        , OnCheckEnter: onCheckEnter
        , GetCurrentItemList: function () { return _koList(); }
    };
})();
// #endregion PopUp Dialogs for SelectConfigItemDialog

define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system'
    , null // "system" arg
    , 'durandal/app'
    , 'datablescelledit'
    , 'bootstrapJS'
    , 'jquerydatatable'
    , 'jqueryui'
    , '../Utility/referenceDataHelper'
]
    , function (composition, ko, $, http, activator, mapping, system, baseCard, app, datablescelledit, bootstrapJS, jquerydatatable, jqueryui, reference) {
        // mwj: app is always undefined... don't know how else to get it to load...
        app = (app || require('durandal/app'));
        // same problem with reference but require won't work either...
        //reference = (reference || require('../Utility/referenceDataHelper'));

        var CAPTION = "Common Configuration";
        var ERR_NO_FORM_DATA = "no_data";
        var ERR_NO_RESULTS = "no_results";
        var MSG_NO_RESULTS_FOUND = "No results found.";
        var MSG_NO_CHILD_ITEMS_FOUND_FOR = "No common config items found for ";
        var MSG_PROVIDE_SEARCH_CRITERIA = "Please provide search criteria.";
        var MSG_UNABLE_TO_PROCESS_REQUEST = "Unable to process your request due to an internal error. If problem persists please contact your system administrator.";
        var LOCATABLE_TYPES = ["Bay Extender", "Shelf", "Node", "Card"];
        var confirmed = { OK: "OK", CANCEL: "CANCEL" };

        var ComnCnfg = function () {
            self = this;

            self.firstLoaded = true;
            self.pendingChanges = false;
            console.log("ComnCnfg() - pendingChanges %s", self.pendingChanges);

            self.cachedChanges = [];
            self.isSearching = false;
            self.childItemCache = []; // because some CNTND_IN_ID items might repeat, no sense in going back to server for them...
            self.expandedRows = [];
            self.containmentRules = [];
            self.parentCandidates = null; // per common config, we'll collect a list of parents...
            self.suppressRelatedToTracking = false;
            self.cachedFetchRelatedToData = {};

            // this will track the original records and also track what records/columns changed...
            self.initAudit();


            UiUtils.SetUI(self);

            self.usr = require('Utility/user');

            self.typeOptions = {};

            self.debugging = ko.observable(false);

            // common stuff
            self.shouldDisplaySaveButtons = ko.observable(false);
            self.currCommonConfig = ko.observable(null);
            self.configRefItems = ko.observable([]);

            //self.selectedScreen = ko.observable(CC_EXISTING);
            self.configTypeOptions = ko.observableArray([]);
            self.srchStatusOptions = ko.observableArray(' ,Update In Progress,Propagated,Completed,Deactivated'.split(','));
            self.comnCnfgItemList = ko.observableArray([]);
            self.showConfigChildItems = ko.observable(false);
            self.isNewConfig = ko.observable(false);

            // Search Existing
            self.$btnSEARCH_FOR_CC = $("#btnSEARCH_FOR_CC");
            self.$btnSEARCH_FOR_CC_CLEAR = $("#btnSEARCH_FOR_CC_CLEAR");
            self.$btnNEW_CC = $("#btnNEW_CC");

            self.existingConfigTypeOptions = ko.observableArray([]);
            self.ccSearchResults = ko.observableArray([]);

            // "btnADD_ITEMS" dialog lists...(see SelectConfigItemDialog module);
            self.ccAddChildConfigItemResults = ko.observableArray([]);
            self.ccAddChildPartItemResults = ko.observableArray([]);

            self.changeLogItems = ko.observableArray([]);

            // Create New
            self.selectedNewConfigType = ko.observable(0);
            self.isEditing = ko.observable(false);
            self.hasRecord = ko.observable(false);
            self.selectedMtrlItems = ko.observable({});
            self.searchResultsForAssociatedPart = ko.observableArray([]);

            self.mtrlTypeOptions = ko.observableArray([]);
            self.srchFeatTypeOptions = ko.observableArray([]);
            self.mtrlStatusOptions = ko.observableArray([]);

            self.loadOptions("MaterialStatusTypes", self.mtrlStatusOptions);
            self.loadOptions("TemplateTypes", self.configTypeOptions);
            self.loadOptions("MaterialTypes", function (opts) {
                self.mtrlTypeOptions(opts);
                $.each(opts, function (ndx, item) {
                    self.typeOptions["m" + item.value] = item.text;
                });
            });

            self.loadOptions("FeatureTypes", function (opts) {
                self.srchFeatTypeOptions(opts);
                $.each(opts, function (ndx, item) {
                    self.typeOptions["f" + item.value] = item.text;
                });
            });

            http.get('api/comncnfg/containmentrules').then(function (list) {
                self.containmentRules = list;
            });

            $(document).ready(function () {
                $(window).scroll(function () {
                    if ($(this).scrollTop() > 100) {
                        $('.scrollup').fadeIn();
                    } else {
                        $('.scrollup').fadeOut();
                    }
                });
                $('.scrollup').click(function () {
                    $("html, body").animate({
                        scrollTop: 0
                    }, 600);
                    return false;
                });

                self.attemptAutoSearch();
            });
        };

        ComnCnfg.prototype.attemptAutoSearch = function () {
            self.autoSearchId = null;

            var showSearchForm = function () {
                $("#navbarContainer").show();
                $("#navigareRecordHide").show();
                $(".viewContainerPadding").css('padding-top', 150);

                // 
                // show search form and results form here...
                // 
                $("#PANEL_MAIN_SCREEN").show();
                $("#PANEL_CC_SEARCH_RESULTS").show();
                $("html, body").animate({ scrollTop: 0 }, 0);

                window.document.title = "Specifications | Material Editor";
                $(this).hide();
            };

            if (window.location.hash.indexOf('?') >= 0) {
                var parms = new URLSearchParams(window.location.hash.split('?')[1]);
                self.autoSearchId = parms.get('id');
                console.log(self.autoSearchId);
            }

            if (!self.autoSearchId) {
                return;
            }

            $("#navbarContainer").hide();
            $("#navigareRecordHide").hide();
            $(".viewContainerPadding").css('padding-top', 10);

            var timer = null;
            var setupRestoreSearchButton = function () {
                self.$btnRESTORE_SEARCH = $(".btnRESTORE_SEARCH");
                if (self.$btnRESTORE_SEARCH.length === 0) {
                    return;
                }
                clearInterval(timer);
                timer = null;
                self.$btnRESTORE_SEARCH.unbind('click').click(showSearchForm).show();
            }
            setTimeout(function () {
                $('#srchCOMN_CNFG_ID').val(self.autoSearchId);

                // do search.. hopefully it'll find just one!, replace with exact search later...
//                self.onSearchForCommonConfig(function () {
                self.doExactSearchForCommonConfig(function() { 
                    $("#PANEL_MAIN_SCREEN").hide();
                    $("#PANEL_CC_SEARCH_RESULTS").hide();
                    $(".btnEXPORT_SEARCH_RESULTS").hide();
                    var cnfgName = self.ccSearchResults()[0].COMN_CNFG_NM;
                    window.document.title = 'CDMMS | Config Name: ' + cnfgName + ' (#' + self.autoSearchId + ')';
                });

                timer = setInterval(setupRestoreSearchButton, 250);
            }, 1000);
        }

        ComnCnfg.prototype.dbg = function (banner, msg) {
            if (!system.debug()) {
                return;
            }

            if (self.debugging() === false) {
                return;
            }
            7
            if (!msg) {
                console.log(banner);
                return;
            }

            console.log("+++ " + banner);
            if (typeof msg === "object" && console.table) {
                console.table(msg);
            }
            else {
                console.log(msg);
            }
            console.log("--- " + banner);
        }

        ComnCnfg.prototype.reset = function () {
            self.onClearSearchForCommonConfig();
            self.onClearEditCommonConfigDialog();

            self.displayConfigEntry(null);
            self.shouldDisplaySaveButtons(false);
        }
        ComnCnfg.prototype.getFormData = function ($form) {
            var data = UiUtils.GetFormData($form, { removePrefix: true });
            return data;
        }
        ComnCnfg.prototype.setupConfigTypeDropDown = function (cnfgTyp) {
            var $sel = $("#EditCommonConfigDialog").find("#edtTMPLT_TYP_ID");
            if ($sel.find("option").length === 0) {
                $.each(self.configTypeOptions(), function (ndx, e) {
                    if (e.text.length > 1) {
                        $("<option>").val(e.value).text(e.text).appendTo($sel);
                    }
                });
            }
            //$sel.find("option:contains('" + txt + "')").prop('selected', true);
            $sel.selectOption(cnfgTyp);
            $sel.unbind('change').change(self.sanityCheckConfigType);
        }
        ComnCnfg.prototype.checkForPendingChanges = function (next) {
            next = (next || function () { });

            if (!self.pendingChanges) {
                next();
                return;
            }

            var msg = "WARNING: There are unsave changes to this Common Config, click Save to save your changes before continuing.";
            self.confirm(msg, "Discard Pending Changes?", ["Discard Changes", "Save Changes"], function (ans) {
                if (ans === "DISCARD CHANGES") {
                    self.pendingChanges = false;
                    console.log("checkForPendingChanges - pendingChanges %s (discard changes)", self.pendingChanges);
                    next();
                }
                else {
                    self.saveChanges(next);
                }
            });
        }

        ComnCnfg.prototype.doSearch = function ($form, next) {
            var pfx = $form.data("pfx");
            var args = UiUtils.GetFormData($form);
            args = UiUtils.ConvertToDbProcParams(args, pfx);

            var display = function (results) {
                next({ err: null, results: results });
            }

            var fatal = function (err) {
                self.showMessage(err);
                next({ err: err, results: null });
            }

            var route = $form.data("search");
            http.get('api/comncnfg/' + route, args).then(display, fatal);

            //$.ajax({
            //    type: "GET",
            //    url: 'api/comncnfg/' + route,
            //    data: args,
            //    contentType: "application/json; charset=utf-8",
            //    dataType: "json",
            //    success: display,
            //    error: fatal,
            //    context: self
            //});
        }
        ComnCnfg.prototype.displayConfigEntry = function (rec) {
            self.currCommonConfig(rec);

            self.isEditing(rec != null);
            self.isNewConfig(rec != null && !rec.COMN_CNFG_ID);

            if (rec === null) {
                self.comnCnfgItemList([]);
                self.showConfigChildItems(false);
                self.refreshInfoStatusBar();
                return;
            }

            var $form = $("#EditCommonConfigDialog");
            var pfx = $form.data("pfx");
            var $fields = $form.find('input,select');
            $fields.each(function (ndx, field) {

                var $f = $(field);
                var id = $f.attr('id');
                var col = id.replace(pfx, '');
                var v = rec[col];

                var type = ($f.attr('type') || 'select').toLowerCase();
                switch (type) {
                    case 'text': $f.val(v);
                        break;
                    case 'checkbox':
                        $f.prop('checked', v === "Y");
                        break;
                    case 'select':
                        $f.selectOption(v, true);
                        break
                }
            })
            for (var col in rec) {
                $form.find(pfx + col).val(rec[col]);
            }

        }

        ComnCnfg.prototype.addPartNumberChildItems = function (list) {
            var prev = self.comnCnfgItemList();

            var ndxSeq = 0;
            prev.forEach(function (e) {
                if (!e._isChild) {
                    ndxSeq = parseInt(e.CNTND_IN_SEQ_NO);
                }
            });
            ndxSeq++;

            var addChildItems = ["High Level Part", "Bay", "Shelf", "Node", "Card"];

            var toBeAdded = [];
            list.forEach(function (rec) {
                var qty = (rec.CNTND_IN_MTRL_QTY || 1);
                var addLineItems = addChildItems.contains(function (e) { return e == rec.MTRL_CAT_TYP || e === rec.FEAT_TYP; });

                var children = (addLineItems ? qty : 1);
                for (var i = 0; i < children; i++) {

                    rec.COMN_CNFG_DEF_ID = -1;

                    rec.CNTND_IN_FEAT_TYP_ID = rec.FEAT_TYP_ID;
                    rec.CNTND_IN_MTRL_CAT_ID = rec.MTRL_CAT_ID;
                    rec.CNTND_IN_ID = rec.CDMMS_ID;

                    rec.CNTND_IN_MTRL_QTY = (children > 1 ? 1 : qty);
                    rec.CNTND_IN_MTRL_TOTAL_QTY = (children > 1 ? 1 : qty);

                    rec.CNTND_IN_MTRL_SPR_QTY = 0;

                    rec.PRNT_COMN_CNFG_DEF_ID = -1;
                    rec.PRNT_CI_COMN_CNFG_DEF_ID = -1;
                    rec.PRNT_CI_HLP_MTRL_REVSN_DEF_ID = -1;

                    rec.FRNT_RER_IND = "";
                    rec.X_COORD_NO = "0";
                    rec.Y_COORD_NO = "0";
                    rec.LABEL_NM = "";
                    rec.RACK_POS = "";
                    rec.SPECN_NM = "";

                    rec.CNTND_IN_REVSN_LVL_IND = (rec.CNTND_IN_REVSN_LVL_IND || "N");

                    rec.CNTND_IN_SEQ_NO = ndxSeq;

                    var item = JSON.parse(JSON.stringify(rec));
                    item._isNew = true;
                    item._guid = createGUID();

                    toBeAdded.push(item);

                    ndxSeq++;
                }
            });

            var $ui = $("#uiComnConfigItemList");
            var attrs = [];
            $ui.find("tr:first").find("td").children().each(function (n, i) {
                var a = ($(i).data("bind") || "").split('$data');
                if (a.length > 0) {
                    attrs = attrs.concat(a);
                }
            });

            attrs = attrs.filter(function (v, i, arr) {
                return v.indexOf(".") === 0;
            });

            attrs = attrs.map(function (v, i, arr) {
                var match = (/[^A-Za-z_]/.exec(v.substring(1)));
                v = (match && match.index > 0) ? v.substring(1, match.index + 1) : v.substring(1);
                return v;
            });

            attrs = attrs.filter(function (v, i, arr) {
                return arr.indexOf(v) === i;
            });

            attrs.forEach(function (v) {
                list.forEach(function (rec) {
                    rec[v] = (rec[v] || "");
                })
            });

            toBeAdded.forEach(function (item) {
                self.audit.added.push(item);
            });

            curr = prev.concat(toBeAdded);
            self.refreshCommonConfigItemList(curr);
            self.pendingChanges = true;
            console.log("addPartNumberChildItems - pendingChanges %s", self.pendingChanges);
        }
        ComnCnfg.prototype.addCommonConfigChildItems = function (list) {
            var prev = self.comnCnfgItemList();

            var ndxSeq = 0;
            prev.forEach(function (e) {
                if (!e._isChild) {
                    ndxSeq = parseInt(e.CNTND_IN_SEQ_NO);
                }
            });
            ndxSeq++;

            var toBeAdded = [];
            list.forEach(function (rec) {
                var qty = (rec.CNTND_IN_MTRL_QTY || 1);
                for (var i = 0; i < qty; i++) {
                    rec.COMN_CNFG_DEF_ID = -1;

                    // note: feat and mtrl are zero...
                    rec.CNTND_IN_FEAT_TYP_ID = 0;
                    rec.CNTND_IN_MTRL_CAT_ID = 0;

                    rec.CNTND_IN_ID = rec.COMN_CNFG_ID;
                    rec.CNTND_IN_MTRL_QTY = 1;
                    rec.CNTND_IN_MTRL_SPR_QTY = "0";
                    rec.CNTND_IN_MTRL_TOTAL_QTY = 1;

                    rec.CNTND_IN_SEQ_NO = ndxSeq;


                    rec.FEAT_TYP = "Generic";
                    rec.MTRL_CAT_TYP = "Common Config";

                    rec.RT_PART_NO = rec.COMN_CNFG_NM;
                    rec.X_COORD_NO = "";
                    rec.Y_COORD_NO = "";
                    rec.FRNT_RER_IND = "";

                    var item = JSON.parse(JSON.stringify(rec));
                    item._isNew = true;
                    item._guid = createGUID();

                    toBeAdded.push(item);

                    ndxSeq++;
                }
            });

            var $ui = $("#uiComnConfigItemList");
            var attrs = [];
            $ui.find("tr:first").find("td").children().each(function (n, i) {
                var a = ($(i).data("bind") || "").split('$data');
                if (a.length > 0) {
                    attrs = attrs.concat(a);
                }
            });

            attrs = attrs.filter(function (v, i, arr) {
                return v.indexOf(".") === 0;
            });

            attrs = attrs.map(function (v, i, arr) {
                var match = (/[^A-Za-z_]/.exec(v.substring(1)));
                v = (match && match.index > 0) ? v.substring(1, match.index + 1) : v.substring(1);
                return v;
            });

            attrs = attrs.filter(function (v, i, arr) {
                return arr.indexOf(v) === i;
            });

            attrs.forEach(function (v) {
                list.forEach(function (rec) {
                    rec[v] = (rec[v] || "");
                })
            });

            toBeAdded.forEach(function (item) {
                self.audit.added.push(item);
            });

            curr = prev.concat(toBeAdded);
            self.refreshCommonConfigItemList(curr);
            self.pendingChanges = true;
            console.log("addCommonConfigChildItems - pendingChanges %s", self.pendingChanges);
        }
        ComnCnfg.prototype.insertChildItemsIntoResults = function (ndx, parent, childItems) {

            if (ndx === -1 || !parent || !childItems || childItems.length === 0) {
                return;
            }

            var seqNo = parent.CNTND_IN_SEQ_NO;

            // we need to maintain original for this parent so we know what changed...
            // we'll match on _guid later...
            var key = parent._guid;
            self.childItemCache[key] = childItems.createNewArray();

            self.expandedRows.push(parent);

            var items = childItems.createNewArray(); // make a copy because we are going to change it...
            items.forEach(function (e) {
                e._isChild = true;
                e._parentSeqNo = seqNo;
                e._parentComnCnfgDefId = parent.COMN_CNFG_DEF_ID;
                e._parentCntndInId = parent.CNTND_IN_ID;
                e._depth = (seqNo.toString().split('-').length + 1);
                e._css = "child-item depth" + e._depth.toString();

                e.SEQ_NO = (e.SEQ_NO || e.CNTND_IN_SEQ_NO); // retain original seq# of child..
                e.CNTND_IN_SEQ_NO = seqNo + '-' + e.SEQ_NO;


                // we need this information for determining what child items can be potential parent options
                // for the "RELATED TO" drop down.
                //
                e.CNTND_IN_FEAT_TYP_ID = e.FEAT_TYP_ID;
                e.CNTND_IN_MTRL_CAT_ID = e.MTRL_CAT_ID;
                e.CNTMT_TYP_ID = e.FEAT_TYP_ID;
                e.CNTMT_TYP = e.FEAT_TYP;

                if (e._depth == 2) {
                    // add child items so we can compare changes...
                    // we have to track related-to and location information
                    // but beyond level 2 (#-#) we don't care...
                    //self.audit.items.push(e); 
                }
            });

            var curr = self.comnCnfgItemList();
            curr[ndx]._childCount = items.length;
            curr.insertArrayAt(ndx + 1, items);
            self.refreshCommonConfigItemList(curr);
        }

        ComnCnfg.prototype.findResidualParentEntry = function (childTyp) {
            var list = self.comnCnfgItemList();
            var rules = self.containmentRules
                .filter(function (e) { return e.CHILD_FEAT_TYP == childTyp; })
                .map(function (i, n, a) { return i.PRNT_FEAT_TYP_ID; })
                ;

            var rule = -1;
            var ndx = list.findIndex(function (e) {
                if (e._isDeleted) {
                    return false;
                }
                rule = rules.findIndex(function (id) { return id == e.CNTND_IN_FEAT_TYP_ID; });
                return rule >= 0;
            });

            return (ndx >= 0 ? list[ndx] : null);
        }
        ComnCnfg.prototype.sanityCheckConfigType = function () {
            var list = self.comnCnfgItemList();

            var $form = $("#EditCommonConfigDialog");
            var cc = self.getFormData($form);

            var $c = $form.find("#edtTMPLT_TYP_ID option:selected");

            var configType = $c.text();
            var configTypeId = $c.val();

            var ndx = list.findIndex(function (e) { return e.FEAT_TYP == configType || e.CNTMT_TYP == configType; });
            var foundConfigType = (ndx >= 0);
            var item = (foundConfigType ? list[ndx] : null);

            var parent = self.findResidualParentEntry(configType);

            var msgs = [];
            if (cc.DEL_IND !== "Y" && cc.RET_COMN_CNFG_IND !== "Y") { // if we are deleting or retiring don't worry about it...
                if (!foundConfigType && configType !== 'Generic') {
                    msgs.push("WARNING: " + configType + " entry not found in item list below...");
                }
                if (foundConfigType && item._isDeleted) {
                    msgs.push("WARNING: " + configType + " found but has been marked for deletion (item #" + item.CNTND_IN_SEQ_NO + ") ...")
                }
                if (parent) {
                    msgs.push("WARNING: " + configType + " parent entry found in list below"
                        + "; please remove  <b>#" + parent.CNTND_IN_SEQ_NO + ": " + parent.RT_PART_NO + " (" + parent.FEAT_TYP + ")</b>"
                        + " - config is designated as a <b>" + configType + "</b> type."
                    );
                }
            }
            self.updateConfigTypeWarnings(msgs);
        }
        ComnCnfg.prototype.refreshInfoStatusBar = function () {
            var $form = $("#EditCommonConfigDialog");
            var cc = self.getFormData($form);
            var list = self.comnCnfgItemList();

            var itemCount = list.length;
            list.forEach(function (item) {
                if (item._isDeleted) {
                    itemCount--;
                }
            });

            var msg = (cc.DEL_IND !== "Y" && cc.RET_COMN_CNFG_IND !== "Y" && itemCount <= 0
                ? MSG_NO_CHILD_ITEMS_FOUND_FOR + cc.COMN_CNFG_NM
                : "");

            self.updateStatusBar(msg);
            self.sanityCheckConfigType();
        }
        ComnCnfg.prototype.updateStatusBar = function (msg) {
            var $c = $("#EditCommonConfigDialog").find(".msg-info");
            if (!msg || msg.length === 0) {
                $c.text("").parents(":first").hide();
                return;
            }
            var descr = "<li>" + msg + "</li>";
            $c.html(descr).addClass("alert-info").css("font-size", "larger");
            $c.parents(":first").show();
        }
        ComnCnfg.prototype.updateConfigTypeWarnings = function (msgs) {
            var $c = $("#EditCommonConfigDialog").find(".msg-warn");
            if (!msgs || msgs.length === 0) {
                $c.text("").parents(":first").hide();
                return;
            }
            var descr = "<li>" + msgs.join("<li>");
            $c.html(descr).addClass("alert-danger").css("font-size", "larger");
            $c.parents(":first").show();
        }
        ComnCnfg.prototype.initAudit = function (rec) {
            // cc is the original comn cnfg record
            // items is the original items list...
            self.audit = { cc: {}, items: [], deleted: [], added: [], log: [] };
            $.extend(self.audit.cc, rec);

            // *** NOTE: changes will be determined at the time of saveConfig()
        }

        ComnCnfg.prototype.fetchChildItems = function () {
            var rec = self.currCommonConfig();

            self.initAudit(rec);

            // clear a bunch of stuff...
            self.pendingChanges = false;
            console.log("fetchChildItems - pendingChanges %s", self.pendingChanges);
            self.comnCnfgItemList([]);
            self.updateConfigTypeWarnings(null);
            self.childItemCache = [];
            self.expandedRows = [];

            $("#ComnCnfgItemsScreen .caption").text("# " + rec.COMN_CNFG_ID + " / " + rec.TMPLT_TYP + " / " + rec.COMN_CNFG_NM);

            var display = function (list) {

                list = (list || []);

                self.showWheelOfPatience(false);
                self.showConfigChildItems(true);

                var $btnADD_ITEM = $(".btnADD_ITEM");
                $btnADD_ITEM.unbind('click').click(function () {
                    var $b = $(this);
                    var $menu = $(".menu-add-items");
                    if ($menu.is(":visible")) {
                        $menu.hide();
                        return;
                    }
                    $menu.find("a").unbind('click').click(function () {
                        var $a = $(this);
                        var cat = $a.data("cat");
                        $menu.hide();
                        SelectConfigItemDialog.Display({ ui: self, partType: { id: 0, name: cat }, http: http });
                    });
                    //$menu.mouseout(function () { $menu.hide(); });
                    $menu.css({ position: "absolute", top: $b.offset().top + 31, left: $b.offset().left }).show();
                });
                $btnADD_ITEM.show();
                var $btn = $(".btnADD_ITEM.footer").hide();

                // clear existing list...
                self.audit.items = [];

                list.forEach(function (e) { e._guid = createGUID(); }); // internally unique id...

                // current list before any changes
                self.audit.items = list.createNewArray();
                self.refreshCommonConfigItemList(list);

                if (list.length > 10) {
                    $btn.show();
                }
            }
            var loadParentCandidates = function (list) {
                self.parentCandidates = list;
            }
            var fatal = function (err) {
                self.showWheelOfPatience(false);
                self.showMessage(err);
            }

            self.showWheelOfPatience(true);

            // just give it time for the drop down to update...
            var fetch = function () {
                http.get('api/comncnfg/cc/items/' + rec.COMN_CNFG_ID).then(display, fatal);
                http.get('api/comncnfg/cc/parentCandidates/' + rec.COMN_CNFG_ID).then(loadParentCandidates, fatal);
            }
            setTimeout(fetch, 500); // fetchChildItems()
        }

        ComnCnfg.prototype.getExpandedRowsParentGuidList = function () {
            // this function is just used to get a list of rows that are expanded
            // usually to then close them, do some UI operations, then open them again
            var $ui = $("#uiComnConfigItemList");
            var list = [];
            $ui.find("tr").each(function (ndx, e) {
                var $e = $(e);
                var keep = (($e.hasClass('HighLevelPart') || $e.hasClass('CommonConfig')) && $e.data('expanded') === true);
                if (keep) {
                    list.push($e.data('guid'));
                }
            });
            return list;
        }
        ComnCnfg.prototype.reExpandRows = function (list) {
            var $ui = $("#uiComnConfigItemList");

            var allItems = self.comnCnfgItemList();
            list.forEach(function (guid) {
                var hlpn = "High Level Part";
                var cc = "Common Config";

                var rec = allItems.findItem(function (e) { return e._guid === guid; });
                if (rec.MTRL_CAT_TYP != hlpn && rec.FEAT_TYP !== cc)
                    return;
                $ui.find("." + guid).show();
                $ui.find("tr#rec" + guid).data('expanded', true);
                rec._isExpanded = true;
            })
        }

        ComnCnfg.prototype.closeAllExpandedRows = function () {
            var list = self.comnCnfgItemList();
            var $ui = $("#uiComnConfigItemList");
            var parents = list.filter(function (e) { return e.MTRL_CAT_TYP === "High Level Part" || e.FEAT_TYP === "Common Config"; });
            parents.forEach(function (e) {
                var guid = e._guid;
                var $tr = $ui.find("tr#" + guid);
                if ($tr.data('expanded')) {
                    $ui.find("." + guid).hide();
                    $tr.data("expanded", false);
                    e._isExpanded = false;
                }
            });
        }
        ComnCnfg.prototype.closeExpandedRow = function (ndx, rec) {
            if (!rec) {
                return;
            }

            var $ui = $("#uiComnConfigItemList");

            var guidList = {}

            var collapseChildren = function ($list) {
                if ($list || $list.length === 0) {
                    return; // all done...
                }

                var guid = $i.data("guid");
                guildList[guid] = "." + guid;

                $list.each(function (ndx, i) {
                    var $next = $ui.find("." + guid);
                    collapseChildren($next);
                });

                $list.hide();
                $ui.find("tr#rec" + guid).data('expanded', false);
            }

            var $list = $ui.find("." + rec._guid);
            collapseChildren($list); // recursively close child items...

            for (guid in guidList) {
                var parent = self.comnCnfgItemList().findItem(function (e) { return e._guid === guid; });
                parent._isExpanded = false;
            }
        }
        ComnCnfg.prototype.collapseChildRows = function (parentGUID) {
            if (!parentGUID || parentGUID.length === 0) {
                return;
            }

            var $ui = $("#uiComnConfigItemList");

            var guidList = {}

            var collapse = function ($list) {
                if (!$list || $list.length === 0) {
                    return; // all done...
                }

                var guid = "";
                $list.each(function (ndx, i) {
                    guid = $(i).data("guid");
                    var $next = $ui.find("." + guid);
                    collapse($next);
                });

                $list.hide();
                $ui.find("tr#rec" + guid).data('expanded', false);
                //console.log('collapseChildRows: ' + false);
            }
            var $childList = $ui.find("." + parentGUID);
            collapse($childList); // recursively close child items...

            for (guid in guidList) {
                var parent = self.comnCnfgItemList().findItem(function (e) { return e._guid === guid; });
                parent._isExpanded = false;
            }
        }
        ComnCnfg.prototype.toggleChildItems = function ($btn, next) {
            next = (next || function () { });

            var $p = $btn.parents('tr:first');

            var parentGUID = $p.data("guid");
            var parent = self.comnCnfgItemList().findItem(function (e) { return e._guid === parentGUID; });

            var $childItems = $("#uiComnConfigItemList").find("." + parentGUID);
            var expanded = ($childItems.length === 0 ? false : $($childItems[0]).is(":visible"));

            $p.data("expanded", !expanded); // toggle
            parent._isExpanded = !expanded;

            if (expanded) {
                self.collapseChildRows(parentGUID);
                return;
            }

            if ($childItems.length > 0) {
                $childItems.show(); // just show top level...
                return;
            }

            // go get the child items...
            var ccDefId = $p.data('cc-def-id');
            var seqNo = $p.data('seq-no').toString();
            var lvl = seqNo.split('-').length;
            var hlp = $p.hasClass('HighLevelPart');
            var cc = $p.hasClass('CommonConfig');

            var query = (cc ? (ccDefId > 0 ? "cntndin-cc/items/" + ccDefId + "/" + lvl : "cc/items")
                : hlp ? (ccDefId > 0 ? "cntndin-hlp/items/" + ccDefId + "/" + lvl + "/" + seqNo : "hlp/items")
                    : null
            );

            if (!query) {
                return;
            }

            self.cacheDropDownChanges(); // save any changes the user made since we are going to redraw the screen...

            var cntndInId = $p.data("cntnd-in-id");
            query = "api/comncnfg/" + query + "/" + cntndInId;

            var display = function (list) {
                self.dbg("toggleChildItems", list);

                var curr = self.comnCnfgItemList();
                var ndx = curr.findIndex(function (e) {
                    return e._guid === parentGUID;
                });

                // set guids...
                list.forEach(function (e) {
                    e._guid = createGUID();
                    e._parentGUID = parentGUID;

                    if (!e.COMN_CNFG_DEF_ID && parent.COMN_CNFG_DEF_ID) {
                        e.COMN_CNFG_DEF_ID = parent.COMN_CNFG_DEF_ID;
                    }
                });

                // check to make sure that any parent references that are marked for delete are removed...
                self.removeAnyDeleteParentReferences(list);

                self.showWheelOfPatience(false);

                self.suppressRelatedToTracking = true; // while we draw the list, don't check on deleted parents...
                self.insertChildItemsIntoResults(ndx, curr[ndx], list);
                self.applyCachedDropDownChanges(); // re-apply the changes...
                self.suppressRelatedToTracking = false; // now you can sanity check on deleted parents as use makes changes...
            }

            var fatal = function (err) {
                self.showWheelOfPatience(false);
                self.showMessage(err);
            }


            self.showWheelOfPatience(true);
            http.get(query)
                .then(display, fatal)
                .then(next)
                ;
        }
        ComnCnfg.prototype.removeAnyDeleteParentReferences = function (list) {
            if (self.audit.deleted.length === 0) {
                return;
            }
            list.forEach(function (chld) {
                if (chld.PRNT_COMN_CNFG_DEF_ID == -1) {
                    return; // no parent reference...
                }

                var prnt = self.audit.deleted.filter(function (e) { return e.COMN_CNFG_DEF_ID == chld.PRNT_COMN_CNFG_DEF_ID || e.COMN_CNFG_DEF_ID == chld.PRNT_CI_COMN_CNFG_DEF_ID; });
                if (prnt.length > 0) {
                    chld.PRNT_COMN_CNFG_DEF_ID = -1;
                    chld.PRNT_CI_COMN_CNFG_DEF_ID = -1;
                    chld.PRNT_CI_HLP_MTRL_REVSN_DEF_ID = -1;
                    chld._related_to = "-1.-1.-1";
                }
            });
        }
        ComnCnfg.prototype.showHideChildItems = function () {
            var $ui = $("#uiComnConfigItemList");
            self.comnCnfgItemList().forEach(function (e) {
                var guid = e._guid;
                if (e._isExpanded === undefined || (e.MTRL_CAT_TYP !== "High Level Part" && e.FEAT_TYP !== "Common Config"))
                    return;

                var $children = $ui.find("." + guid);

                e._isExpanded
                    ? $children.show()
                    : $children.hide()
                    ;
            });
        }
        ComnCnfg.prototype.makeChildItemsReadOnly = function ($ui) {
            // 
            // if the location information for a child item was set outside within another HLPN or CC 
            // then the location information should be locked in and not changegable
            //
            var $items = $ui.find(".child-item");

            var list = self.comnCnfgItemList();

            $items.find("input[type='checkbox']").hide();

            // get all the locatable fields except for "relatedto" 
            var $fields = $items.find("input,select").filter(":visible").filter(":not(.relatedto)");
            $fields.each(function (ndx, f) {
                var $f = $(f);
                if ($f.hasClass('seq-no') || $f.hasClass('qty') || $f.hasClass('spr-qty')) {
                    $f.removeClass('form-control').css('border', 'none').css('background-color', 'inherit').prop('readonly', true);
                    return;
                }
                if ($f.hasClass('revisions')) {
                    $f.remove();
                    return;
                }

                var $tr = $f.parents("tr:first");
                var $p = $f.parents(":first");

                var guid = $tr.data('guid');

                var rec = list.findItem(function (i) {
                    return i._guid == guid;
                });

                var lvl = rec._depth;

                var isReadOnly = (lvl > 2);
                if (isReadOnly === false) {
                    if (rec.FEAT_TYP !== "Bay") {
                        return;
                    }
                }

                // special case for label....
                if ($f.hasClass(".LABEL_NM") || $f.hasClass(".RACK_POS")) {

                    // if we are a 1st level child HLPN item then the label IS editable
                    if (rec._depth <= 2 && rec.MTRL_CAT_TYP === "High Level Part") {
                        return;
                    }

                    // if we are a 1st level common config and the parent is not setup the label IS editable
                    if (rec._depth <= 2 && (rec.PRNT_COMN_CNFG_DEF_ID || -1) <= 0) {
                        return;
                    }

                    // anything else, remove the label...
                }
                $f.removeClass('form-control').css('border', 'none').css('background-color', 'transparent').prop('readonly', true);

                if ($f.prop('tagName').toUpperCase() === 'SELECT') {
                    var val = $f.find("option:selected").text();
                    $("<span>").text(val).appendTo($p);
                    $f.remove();
                }
            });

            // for any child items beyond a depth of 1 level don't provide 
            // this drop down and instead just show the parent info via text
            var $relatedto = $items.find("select").filter(".relatedto");
            $relatedto.each(function (ndx, e) {
                var $e = $(e);
                var $p = $e.parent();
                var $tr = $p.parents("tr:first");

                var guid = $tr.data("guid");
                var rec = list.findItem(function (i) {
                    return i._guid == guid;
                });
                var lvl = rec._depth;

                var isReadOnly = (lvl > 2);

                if (isReadOnly === false) {
                    return; // ok, let the user set the parent...
                }

                // readonly...   
                //if (rec.PRNT_COMN_CNFG_DEF_ID > 0 || rec.PRNT_HLP_MTRL_REVSN_DEF_ID > 0) {
                if (rec.CNTND_IN_PRNT_DSC && rec.CNTND_IN_PRNT_DSC.length > 0) {
                    var seq = null;
                    var startingSeqNo = rec.CNTND_IN_SEQ_NO.split('-')[0];
                    var hier = list.filter(function (i) { return i.CNTND_IN_SEQ_NO.toString().indexOf(startingSeqNo) === 0; });
                    hier = JSON.parse(JSON.stringify(hier));

                    var prnt = hier.filter(function (i) { return i.CNTND_IN_ID == rec._rltd_to_cntnr_id; });
                    if (prnt.length > 0) {
                        prnt = prnt[0];
                        if (prnt.CNTND_IN_COMN_CNFG_DEF_ID == rec._rltd_to_prnt_comn_cnfg_def_id) {
                            seq = prnt.CNTND_IN_SEQ_NO + "-" + rec._rltd_to_item_seq_no;
                        }

                    }

                    if (!seq && rec._parentSeqNo) { // kludge...
                        // #1: MRTP QW7R108231B
                        var tmp = rec.CNTND_IN_PRNT_DSC.split(":");
                        if (tmp.length >= 2) {
                            var itemNo = tmp[0].replace(/#/g, "");
                            rec.CNTND_IN_PRNT_DSC = $.trim(tmp[1]);
                            seq = rec._parentSeqNo + "-" + itemNo;
                        }

                    }

                    //
                    // CHECKHERE: TODO we need to set the parent based on the combo of columns PRNT_COMN_CNFG_DEF_ID/PRNT_CI_cc/hlp_DEF_ID
                    //
                    var dsc = "#" + seq + ": " + (rec.CNTND_IN_PRNT_DSC || rec.CNTND_IN_PRNT_COMN_CNFG_ID + "(???)");

                    $("<span>").text(dsc).appendTo($p);
                }
                $e.remove();

            });

            $items.find(".clear-txt").html('&nbsp;'); // hack for demo; have not implemented related to functionality yet.
            $items.find("input.seq-no,input.qty,input.spr-qty").css("background-color", "transparent");

        }
        ComnCnfg.prototype.setDefaultRelatedToOption = function () {
            // if there is only one parent item in a drop down and it hasn't already 
            // been selected, set it as the default parent for the user...
            var $relatedTo = $(".relatedto").filter(":visible");
            $relatedTo.each(function (ndx, e) {
                var $e = $(e);
                var $opts = $e.children("option");
                if ($opts.length != 2) {
                    return; // more than one option available, user has to decide...
                }

                var $sel = $opts.filter(":selected");
                if ($sel.text().length > 0) {
                    return; // already selected...
                }

                $sel = $($opts[1]);

                $e.val($sel.val()).trigger('change');
                var frntRerInd = $sel.data("frntRerInd");
                var $tr = $e.parents("tr:first");
                var $frntRerInd = $tr.find(".FRNT_RER_IND").filter(":visible");
                if ($frntRerInd.length > 0) {
                    $frntRerInd.val(frntRerInd).trigger('change');
                }
                self._pendingChanges = true;
                console.log("setDefaultRelatedToOption - _pendingChanges %s", self._pendingChanges);
            });
        }
        ComnCnfg.prototype.setUserSelectedRelatedToOptions = function () {
            // re-hilight user selections...
            self.suppressRelatedToTracking = false;

            // get a list of any records where the user has changed the related to parent...
            var list = self.comnCnfgItemList().filter(function (e) { return (!e._tempRelatedToParent === false); });

            list.forEach(function (rec) {
                var guid = rec._guid;
                var prnt = rec._tempRelatedToParent;
                $("#rec" + guid).find(".relatedto").val(prnt).trigger('change');
                console.log("setUserSelectedRelatedToOptions: %s %s", rec.CNTND_IN_SEQ_NO, prnt);
            });
        }
        ComnCnfg.prototype.selectRelatedToOptions = function () {

            //
            // this is miserable....
            //

            var prnts = self.parentCandidates;
            if (!prnts || prnts.length === 0) {
                return; // nothing to do, no related-to parent candidates
            }

            //
            // NOTE: this is kind of complicated -- the related-to parent
            // is either set in "context" of this config, or it might have
            // a "default" related-to parent based on how the c/i HLPN or CC
            // was configured outside of the current config.
            //
            // we have to check if the related-to parent is set by "context" or by "default"
            //
            // so, we have 3 conditions:
            // * 1) related-to parent is setup in context of this config - PRNT_COMN_CNFG_DEF_ID is set and one of PRNT_CI_XXXX_DEF_ID is set
            // * 2) related-to parent is setup based via HLPN only - PRNT_COMN_CNFG_DEF_ID is not set only PRNT_HLP_MTRL_REVSN_DEF_ID is set
            // * 3) related-to parent is availabe by default - PRNT_COMN_CNFG_DEF_ID is set and one of PRNT_CI_XXXX_DEF_ID is set but outside of this config (i.e. not in "context")
            //

            //
            // NOTE ON THE TERM "CONTEXT":
            //      Say you have CC A and CC B; both contain HLPN A;
            //      the related-to parent for HLPN A is in "context" of CC A 
            //      and consequently can be different from the relate-to parent 
            //      setup in for HLPN A in "context" of CC B
            //
            // Not only that, but HLPN A might contain child items that have their
            // own default internal related-to parent items that can be changed to a new
            // external related-to parent in "context" of CC A or CC B (see #3 condition above)
            //

            var state = self.pendingChanges; // save pending state; don't want trigger() below to change it...

            var rules = JSON.parse(JSON.stringify(self.containmentRules));
            var unique = function (v, i, arr) { return arr.indexOf(v) === i; }
            var prntTyps = rules.map(function (i) { return i.PRNT_FEAT_TYP; }).filter(unique);
            var chldTyps = rules.map(function (i) { return i.CHLD_FEAT_TYP; }).filter(unique);

            // get all the possible children
            var chlds = self.comnCnfgItemList().filter(function (i) {
                var ft = (i.FEAT_TYP || i.CNTND_IN_FEAT_TYP);
                var rv = chldTyps.indexOf(ft);
                return rv != -1;
            });


            chlds.forEach(function (ch) {
                var guid = ch._guid;
                var $relatedTo = $("#rec" + guid).find(".relatedto");
                var rec = self.comnCnfgItemList().filter(function (e) { return e._guid == guid; })[0];

                //if ($relatedTo.is(":visible") === false) {
                //    return; // readonly, drop down isn't visible...  REMOVE THIS
                //}

                // default related-to parent info preconfigured on HLPN or CC
                var defPrntRefKeyId = [ch._PRNT_DEF_CC_DF_ID || -1, ch._PRNT_DEF_CI_HLP_DF_ID || -1, ch._PRNT_DEF_CI_CC_DF_ID || -1].join(".");
                var rltdToPrntKeyId = (ch._rltd_to_prnt_ref_key_id || "-1.-1.-1");

                var pCcDefId = (ch.PRNT_COMN_CNFG_DEF_ID || ch._PRNT_DEF_CC_DF_ID || -1);
                var pHlpId = (ch.PRNT_CI_HLP_MTRL_REVSN_DEF_ID || -1);
                var pCcId = (ch.PRNT_CI_COMN_CNFG_DEF_ID || -1);

                var prntRefKeyId = [pCcDefId, pHlpId, pCcId].join(".");

                var pHlpPrntDefId = (ch.PRNT_HLP_MTRL_REVSN_DEF_ID || -1);

                if (pCcDefId == pCcId) {
                    pCcId = -1;
                    ch.PRNT_CI_COMN_CNFG_DEF_ID = -1;
                }

                if (prntRefKeyId == "-1.-1.-1" && (defPrntRefKeyId != "-1.-1.-1" || rltdToPrntKeyId != "-1.-1.-1")) {
                    // HLPN or CC has setup a default parent, this info hasn't been
                    // copied into our instance, so setup the relationship 
                    // in case the user clicks save...
                    if (defPrntRefKeyId != "-1.-1.-1") {
                        prntRefKeyId = defPrntRefKeyId;
                        pCcDefId = ch.PRNT_COMN_CNFG_DEF_ID;
                    }
                    else {
                        var parts = rltdToPrntKeyId.split(".");
                        parts[0] = (parts[0] == "-1" ? (ch._rltd_to_prnt_comn_cnfg_def_id || -1) : parts[0]);
                        //parts[1] = (parts[1]);
                        //parts[2] = (parts[2]);
                        prntRefKeyId = parts.join(".");
                        pCcDefId = parts[0];
                    }
                }

                var prnt = prnts.findItem(function (p) {
                    return p.PRNT_REF_KEY_ID == prntRefKeyId;
                });

                if (!prnt && prntRefKeyId === "-1.-1.-1" && pHlpPrntDefId > 0) {
                    // this means that the parent was preconfigured on the HLPN
                    // so maybe we can find the parent based on the HLPN def id
                    var prnt = prnts.findItem(function (p) {
                        return p.CAT === "COMN_CNFG_CI_HLPN"
                            && p.COMN_CNFG_DEF_ID == pHlpPrntDefId
                            ;
                    });
                    if (prnt) {
                        pCcDefId = prnt.PRNT_COMN_CNFG_DEF_ID;
                        pHlpId = pHlpPrntDefId;
                        pCcId = -1;

                        prntRefKeyId = [pCcDefId, pHlpId, pCcId].join(".");
                    }
                }


                ch._related_to = prntRefKeyId; // default, no parent...

                if (pCcDefId == -1) {
                    $relatedTo.val("-1.-1.-1").trigger('change');
                    //console.log("selectRelatedToOptions: %s prnt=%s curr=%s (CLEARED)"
                    //    , ch.CNTND_IN_SEQ_NO + ": " + ch.SPECN_NM
                    //    , prntRefKeyId
                    //    , (prnt == null ? "[no parent]" : prnt.PRNT_REF_KEY_ID + " / " + prnt.PRNT_SEQ_NO + "-" + prnt.ITEM_SEQ_NO + ": " + prnt.SPECN_NM)
                    //);

                    if (rec._tempRelatedToParent) {
                        console.log("selectRelatedToOptions: %s prnt=%s curr=%s (CLEARED) user selection = %s"
                            , ch.CNTND_IN_SEQ_NO + ": " + ch.SPECN_NM
                            , prntRefKeyId
                            , (prnt == null ? "[no parent]" : prnt.PRNT_REF_KEY_ID + " / " + prnt.PRNT_SEQ_NO + "-" + prnt.ITEM_SEQ_NO + ": " + prnt.SPECN_NM)
                            , rec._tempRelatedToParent
                        );

                        //$relatedTo
                        //    .data("orig", "-1.-1.-1")
                        //    .val(rec._tempRelatedToParent)
                        //    .trigger('change'); // user selected changed...
                    }
                    return;
                }

                var setSelection = function () {
                    $relatedTo.val(prntRefKeyId); // note, we aren't calling trigger('change') here...
                    var descr = $relatedTo.children("option").filter(":selected").text();
                    $relatedTo.data('orig', descr);

                    //if (rec._tempRelatedToParent) {
                    //    $relatedTo
                    //        .data("orig", prntRefKeyId)
                    //        .val(rec._tempRelatedToParent)
                    //        .trigger('change'); // user selected changed...
                    //}
                }

                var $opt = $relatedTo.find("option[value='" + prntRefKeyId + "']");
                if ($opt.length === 0) {
                    // special case; go fetch the parent info and add it to the drop down list....
                    self.fetchRelatedToParentInfo(prntRefKeyId, function (prnt) {
                        if (!prnt) {
                            return;
                        }

                        var parts = prnt.PRNT_SEQ_NO.split('-');
                        if (parts.length == 2) {
                            var $p = $relatedTo.parents(":first");
                            var dsc = "# " + prnt.PRNT_SEQ_NO + "-" + prnt.ITEM_SEQ_NO + ": ";
                            dsc += (prnt.FRNT_RER_IND ? prnt.FRNT_RER_IND + "|" : "") + (prnt.SPECN_NM || prnt.FEAT_TYP);
                            $("<span>").text(dsc).appendTo($p);
                            $relatedTo.hide();
                            return;
                        }

                        var seqNo = ch.CNTND_IN_SEQ_NO.split('-')[0];
                        var containers = self.comnCnfgItemList().filter(function (e) {
                            if (prnt != null && e.CNTND_IN_SEQ_NO.toString().indexOf(seqNo) === 0 && e.COMN_CNFG_DEF_ID == prnt.PRNT_COMN_CNFG_DEF_ID) {
                                return e;
                            }
                        });
                        if (containers.length > 0) {
                            seqNo = containers[0].CNTND_IN_SEQ_NO + "-" + prnt.ITEM_SEQ_NO;
                        }
                        var prefix = ((prnt.FRNT_RER_IND || "").length > 0 ? prnt.FRNT_RER_IND + "|" : "");

                        var dsc = seqNo + ": " + prefix + prnt.SPECN_NM;

                        $("<option>").val(prntRefKeyId)
                            .text(dsc)
                            .data('frnt-rer-ind', prnt.FRNT_RER_IND)
                            .appendTo($relatedTo)
                            ;
                        setSelection();
                    });
                    return;
                }

                setSelection();
            });

            self.pendingChanges = state; // restore state...
            console.log("selectRelatedToOptions - pendingChanges %s", self.pendingChanges);

        }
        ComnCnfg.prototype.fetchRelatedToParentInfo = function (prntRefKeyId, next) {
            // can't pass a variable in the format #.#.# so reformatting to #x#x#x
            var delim = "x";
            var id = prntRefKeyId.replace(/\./g, delim);
            var url = "api/comncnfg/fetchRelatedToParentInfo/" + id;

            var cached = self.cachedFetchRelatedToData[id];
            if (cached) {
                next(cached);
                return;
            }

            http.get(url).then(function (prnt) {
                self.cachedFetchRelatedToData[id] = prnt;
                next(prnt);
            });
        }
        ComnCnfg.prototype.fillRelatedTo = function ($ctrl) {

            //if ($ctrl.is(":visible") === false) {
            //    return; // not a locatable record...
            //}

            var $opts = $ctrl.find("option");
            if ($opts.length > 0) {
                return; // if already filled don't re-fill it/change it...
            }

            var candidates = self.parentCandidates;

            if (!candidates || candidates.length === 0) {
                //console.log("fillRelatedTo: not parent candidates found");
                return;
            }

            var $p = $ctrl.parents("tr:first");
            var id = $p.data("ccDefId");
            var guid = $p.data("guid");
            var specnm = $p.find(".specnm").text();
            var feattyp = $p.find(".feattyp").text();

            if (!id && id != 0 && !guid) {
                return;
            }


            $ctrl.empty(); // clear it...
            var itemList = self.comnCnfgItemList();
            var ndx = itemList.findIndex(function (e) {
                return e._guid === guid;
            });

            if (ndx === -1) {
                //console.log("fillRelatedTo: could not find item in list for %s (%s %s)", guid, feattyp, specnm);
                return;
            }

            var rec = itemList[ndx];
            var childCntmtTypId = rec.CNTMT_TYP_ID || rec.FEAT_TYP_ID; // i.e. child type of 1,5,7,8 can have parents...

            //PRNT_FEAT_TYP_ID	CHILD_FEAT_TYP_ID	PRNT_FEAT_TYP	CHILD_FEAT_TYP
            //               2	                1	Bay	            Bay Extender
            //               2	                5	Bay	            Node
            //               2	                6	Bay	            Shelf
            //               5	                6	Node	        Shelf
            //               6	                7	Shelf	        Card
            //               5	                7	Node	        Card
            //               7	                8	Card	        Plug - In
            //               5	                8	Node	        Plug - In

            var rules = self.containmentRules
                .filter(function (e) { return e.CHILD_FEAT_TYP_ID == childCntmtTypId; })
                .map(function (i, n, a) { return i.PRNT_FEAT_TYP_ID }) // these are my parent types...
                ;

            if (rules.length === 0) {
                //console.log("fillRelatedTo: no rules found for cntmt_typ_id %s for %s (%s %s)", childCntmtTypId, guid, feattyp, specnm);
                return; // no parents for this feature type
            }


            // look for any items in the list that are potential parents (FEAT_TYP_ID of 2, 5, 6, 7)
            var parents = candidates.filter(function (e) {
                return rules.indexOf(e.FEAT_TYP_ID) != -1;
            });

            $("<option>").val("-1.-1.-1").text("").appendTo($ctrl);
            parents.forEach(function (e) {
                // it's possible that the user will move the parent, so really we need to get the current "PRNT_SEQ_NO" 
                // the list; but I don't know how to do that yet...
                var descr = (e.PRNT_SEQ_NO == 0 ? e.ITEM_SEQ_NO : e.PRNT_SEQ_NO + "-" + e.ITEM_SEQ_NO) + ": " + (e.FRNT_RER_IND ? e.FRNT_RER_IND + "|" : "") + e.SPECN_NM;
                var $opt = $("<option>").val(e.PRNT_REF_KEY_ID).text(descr).data("frntRerInd", e.FRNT_RER_IND).data('rec', e);
                $opt.appendTo($ctrl);
            });
        }
        ComnCnfg.prototype.hilightNewRows = function () {
            $(".ccDefId-1").addClass("txt-hilite");
        }
        ComnCnfg.prototype.showBreadCrumb = function ($ctrl) {
            var orig = $ctrl.data("orig");

            var sel = $ctrl.children("option").filter(":selected").text();

            if (orig == sel) {
                $ctrl.removeClass("has-changed").attr("title", "");
                return;
            }
            $ctrl.addClass("has-changed").attr("title", "Originally was set to \"" + orig + "\"");
        }
        ComnCnfg.prototype.cacheDropDownChanges = function () {
            //
            // NOTE: I don't know how to get knockjs to sync the drop down optoins and selections... so this is my work-around
            //

            if (self.pendingChanges === false) {
                return;
            }
            self.cachedChanges = [];

            var list = JSON.parse(JSON.stringify(self.comnCnfgItemList()));

            var $ui = $("#uiComnConfigItemList");
            var $selects = $ui.find("select").filter(":visible");
            $selects.each(function (ndx, ctrl) {
                var $ctrl = $(ctrl);


                var $tr = $ctrl.parents("tr:first");
                var guid = $tr.data("guid");

                var col = $ctrl.data('col');
                var val = $ctrl.find("option:selected").val();

                var rec = list.findItem(function (e) { return e._guid === guid; });

                if (rec[col] != val) {
                    var data = { $ctrl: $ctrl, guid: guid, rec: rec, col: col, val: val };
                    self.cachedChanges.push(data);
                    console.log("cacheDropDownChanges: %s - %s %s (orig: %s)", rec.CNTND_IN_SEQ_NO, col, val, rec[col]);
                }
            });
        }
        ComnCnfg.prototype.applyCachedDropDownChanges = function () {
            if (self.cachedChanges.length === 0) {
                return;
            }
            self.cachedChanges.forEach(function (item) {
                var $ctrl = item.$ctrl;

                var col = item.col;
                var val = item.val;

                $ctrl.val(item.val).trigger('change');
                console.log('applyCachedDropDownChanges - %s set %s from %s to %s', item.rec.CNTND_IN_SEQ_NO, col, item.rec[col], val);
            });
            self.cachedChanges.length = 0;
            self.suppressRelatedToTracking = false;
        }
        ComnCnfg.prototype.trackRelatedToChanges = function ($ctrl) {
            self.showBreadCrumb($ctrl);

            var $tr = $ctrl.closest('tr');
            var guid = $tr.data('guid');
            var rec = self.comnCnfgItemList().filter(function (e) { return e._guid == guid })[0];

            //if (!$ctrl.data('is-init')) {
            //    $ctrl.data('is-init', true); // first time, we are only interested in changes
            //    $ctrl.data('changes', 0);
            //    //console.log("trackRelatedToChanges: init %s %s", rec.CNTND_IN_SEQ_NO, self.suppressRelatedToTracking);
            //    return;
            //}

            if (self.suppressRelatedToTracking === true) {
                //console.log("trackRelatedToChanges: %s suppressRelatedToTracking=%s", rec.CNTND_IN_SEQ_NO, self.suppressRelatedToTracking);
                return;
            }

            var $opt = $ctrl.find('option:selected');
            var selRefKey = $opt.val();
            if (!selRefKey) {
                //console.log("trackRelatedToChanges: %s selRefKey=%s", rec.CNTND_IN_SEQ_NO, selRefKey);
                return;
            }


            // we need to make sure the user isn't selecting a parent that has been marked for deletion...
            var prnt = self.audit.deleted.findItem(function (e) { if (e.CNTND_IN_COMN_CNFG_DEF_ID == rec.COMN_CNFG_DEF_ID || e.CNTND_IN_COMN_CNFG_DEF_ID == rec.PRNT_COMN_CNFG_DEF_ID) return e; });
            if (prnt) {
                var dsc = $opt.text();
                self.showMessage(dsc + " has been marked for deletion so it cannot be used as a parent.");

                selRefKey = '-1.-1.-1';
                $ctrl.val(selRefKey).trigger('change');
            }

            rec._tempRelatedToParent = selRefKey;
            $ctrl.data('rec', rec);

            console.log("trackRelatedToChanges: %s=%s %s", rec.CNTND_IN_SEQ_NO, selRefKey, guid);

            self.pendingChanges = true;
            console.log("trackRelatedToChanges - pendingChanges %s", self.pendingChanges);
        }
        ComnCnfg.prototype.refreshCommonConfigItemList = function (list) {
            self.comnCnfgItemList(list);

            var cc = self.currCommonConfig();

            if (!list || list.length === 0) {
                self.refreshInfoStatusBar();
                return;
            }

            var $pane = $("#uiComnConfigItemList");

            // load possible parent options...
            var $relatedTo = $pane.find(".relatedto");
            $relatedTo.each(function (ndx, ctrl) {
                var $c = $(ctrl).empty();
                self.fillRelatedTo($c);
            });

            UiUtils.SetupDropDowns($pane.find(".placement"), { defVal: "", onChange: function () { self.pendingChanges = true; console.log("SetupDropDowns(placement) - pendingChanges %s", self.pendingChanges); self.showBreadCrumb($(this)); } });
            UiUtils.SetupDropDowns($pane.find(".revisions"), { defVal: "", onChange: function () { self.pendingChanges = true; console.log("SetupDropDowns(revisions) - pendingChanges %s", self.pendingChanges); self.showBreadCrumb($(this)); } });
            UiUtils.SetupDropDowns($relatedTo, {
                onChange: function () {
                    self.trackRelatedToChanges($(this));
                }
            });

            UiUtils.SetupExpanders($pane.find(".HighLevelPart,.CommonConfig"));

            // remove edit boxes from child items...
            self.makeChildItemsReadOnly($pane);
            //self.showHideChildItems(); // depending on their last state...

            $pane.find("tr").find(".LABEL_NM,.RACK_POS").addClass("tbd").hide();
            $pane.find(".placement,.xy-coord-input").addClass("tbd").hide();
            $pane.find(".relatedto").addClass("tbd").hide();

            $pane.find("tr.BayExtender,tr.Shelf,tr.Node,tr.Card,tr.PlugIn").find(".relatedto").removeClass("tbd").show();
            $pane.find("tr.Node,tr.Shelf").find(".placement,.xy-coord-input,.LABEL_NM,.RACK_POS").removeClass("tbd").show();
            $pane.find("tr.Bay,tr.PlugIn").find(".placement").removeClass("tbd").show();

            // newly added items....
            self.hilightNewRows();

            // now remove any fields that we have hidden as they are N/A 
            $pane.find(".placement:hidden").filter(".tbd").remove();
            $pane.find(".xy-coord-input:hidden").filter(".tbd").remove();
            $pane.find(".LABEL_NM:hidden").filter(".tbd").remove();
            $pane.find(".RACK_POS:hidden").filter(".tbd").remove();
            $pane.find(".relatedto:hidden").filter(".tbd").remove();

            self.selectRelatedToOptions();

            self.resetBreadCrumbs();
            self.setDefaultRelatedToOption(); // if there is only one prnt item available, set it...

            console.log("refreshCommonConfigItemList - pendingChanges %s (before setUserSelectedRelatedToOptions)", self.pendingChanges);
            var pendingChangeState = self.pendingChanges;
            self.setUserSelectedRelatedToOptions(); // user might have changed the default selection, restore that due to screen refresh
            self.pendingChanges = pendingChangeState;
            console.log("refreshCommonConfigItemList - pendingChanges %s (after setUserSelectedRelatedToOptions)", self.pendingChanges);

            self.shouldDisplaySaveButtons(true);
            self.refreshInfoStatusBar();

            setTimeout(self.enableDragging, 250);
        }
        ComnCnfg.prototype.resetBreadCrumbs = function () {
            $(".has-changed").removeClass("has-changed").attr("title", "");
            $("select").each(function (ndx, dd) {
                var $dd = $(dd);
                var sel = $dd.children("option").filter(":selected");
                $dd.data('orig', sel.text());
            });
        }
        ComnCnfg.prototype.enableDragging = function () {
            var $pane = $("#uiComnConfigItemList");

            // disable the ability to drag child items, don't want user to accidently try it....
            $pane.find("tr").filter(".is-child").find(".dragger").css("cursor", "default");

            // can only drag top level rows...
            var $rows = $pane.find("tr").not(".is-child");

            var cloned = {};

            $rows.draggable({
                scroll: true
                , cusor: "move"
                , helper: "clone"
                , handle: ".dragger"
                , start: function (event, ui) {
                    cloned.$tr = $(this);
                    cloned.$helper = $(ui.helper);
                    cloned.$tr.addClass("disabled");
                    cloned.$helper.addClass("dragging");
                    cloned.$helper.find("td:gt(6)").hide();
                }
                , drag: function (event, ui) {
                    var $d = $(this);
                    if (cloned.$tr.data("expanded")) {
                        self.showMessage("Cannot re-sequence expanded HLPN or Common Config line item; please collapse before changing the sequence of the item.");
                        return false;
                    }
                }

                , stop: function (event, ui) {
                    cloned.$tr.removeClass("disabled");
                    cloned.$helper.removeClass("dragging").remove();
                }
                , cancel: function (event, uit) {
                    cloned.$helper.remove();
                }
            });

            $rows.droppable({
                over: function (event, ui) {
                    var $e = $(this);
                    //console.log("over: %s %s %s", $e.data("cc-def-id"), $e.data("cntnd-in-id"), $e.data("expaned"));
                    $e.addClass("success");
                }
                , out: function (event, ui) {
                    var $e = $(this);
                    //console.log("out: %s %s %s", $e.data("cc-def-id"), $e.data("cntnd-in-id"), $e.data("expaned"));
                    $e.removeClass("success");
                }
                , drop: function (event, ui) {
                    var $t = $(this);
                    $t.removeClass("success");

                    if ($t.data("expanded")) {
                        self.showMessage("Cannot re-order item while expanded");
                        return false;
                    }

                    var currSeqNo = cloned.$tr.data("seq-no");
                    var newSeqNo = $t.data("seq-no");

                    if (isNaN(newSeqNo) && newSeqNo.indexOf('-') > 0) {
                        self.showMessage("Cannot re-order item onto child item");
                        return false;
                    }
                    //self.dbg("move "+currSeqNo+" to "+newSeqNo);

                    var $edit = cloned.$tr.find("input.seq-no");
                    $edit.val(newSeqNo);
                    self.reOrderList($edit);
                }
            });
        }

        ComnCnfg.prototype.showMessage = function (msg, caption, css) {
            var disp = msg;
            if (Array.isArray(msg)) {
                disp = msg.join("<br/>");
            }

            if (typeof msg === "object") {
                disp = JSON.stringify(msg);
            }

            disp = disp.replace(/\n/g, "<br/>");

            return app.showMessage(disp, (caption || CAPTION), ["OK"], false, css);
        }
        ComnCnfg.prototype.confirm = function (msg, caption, buttons, onClick, css) {
            var opts = { msg: msg, caption: caption, buttons: buttons, style: {}, onClick: onClick, css: css };
            if (typeof msg === "object" && Array.isArray(msg) === false) {
                opts = msg;
            }

            opts.caption = (opts.caption || "Please Confirm");
            opts.msg = (Array.isArray(opts.msg) ? opts.msg.join("<br/>") : opts.msg);
            opts.onClick = (opts.onClick || function () { });

            var _buttons = (typeof opts.buttons === 'object' ? opts.buttons : ['OK', 'Cancel']);
            var _cb = (typeof opts.buttons === 'function' ? opts.buttons : opts.onClick);

            app.showMessage(opts.msg, opts.caption, _buttons, false, opts.css)
                .then(function (choice) { opts.onClick(choice.toUpperCase()); })
                ;
        }

        ComnCnfg.prototype.groomAuditLog = function (auditLog) {
            if (!auditLog || auditLog.length === 0) {
                return auditLog;
            }
            //
            // this is a kluge because we don't know where the changes are coming from 
            // since the column values can come from multiple tables
            // personally, I think audit tracking should be done within the SPROC that 
            // actually makes the change because it knows better...
            //

            var log = JSON.parse(JSON.stringify(auditLog)); // make a copy..
            log.forEach(function (e) {
                if (!e._depth || e._depth < 2 || !e.CNTND_IN_PRNT_TBL_NM) {
                    return;
                }

                if (e.col === "PRNT_COMN_CNFG_DEF_ID") {
                    e.tbl = e.CNTND_IN_PRNT_TBL_NM;
                }
                if (e.col === "PRNT_CI_HLP_MTRL_REVSN_DEF_ID") {
                    e.tbl = e.CNTND_IN_PRNT_TBL_NM;
                }
                if (e.col === "PRNT_CI_COMN_CNFG_DEF_ID") {
                    e.tbl = e.CNTND_IN_PRNT_TBL_NM;
                }
            });
            return log;
        }
        ComnCnfg.prototype.determineChanges = function (curr_cc) {
            //
            // this function generates what will become the "audit log"
            // that gets posted to the controller and ultimately recorded...
            //
            var changes = [];       // all the records that have changes will go here, the local functions will fill this array
            var hasChanged = false;    // indicator flag while looking for a change...
            var orig = null;     // handle to original records

            var comnCnfgId = self.audit.cc.COMN_CNFG_ID;

            //
            // this is the callback procedure used to deterimine 
            // if there is any differences between the two records
            // (src=orig vs dst=curr) and if detected adds it to 
            // the 'changes' list and markes the 'hasChanged' flag
            // so the entire record can be marked as having a change
            //
            var collectChanges = function (oldRec, newRec, colsToIgnore, colsToAdd, print) {
                var guid = newRec["_guid"];
                var comnCnfgDefId = oldRec.COMN_CNFG_DEF_ID;
                var recId = oldRec.CDMMS_ID;
                var tbl = oldRec.CDMMS_COMN_CNFG_CNTD_IN_TBL_NM.toLowerCase();

                var cols = Object.keys(newRec).filter(function (key) {
                    //console.log("%s %s %s: curr=%s orig=%s", oldRec._depth, oldRec.CNTND_IN_SEQ_NO, key, newRec[key], oldRec[key]);
                    return key.indexOf("_") !== 0
                        && colsToIgnore.contains(key) === false
                        && (oldRec[key] || "") != (newRec[key] || "")
                        ;
                });

                if (cols.length === 0)
                    return; // no changes...

                for (var i = 0; i < cols.length; i++) {
                    var col = cols[i];
                    var info = {
                        id: recId
                        , guid: guid
                        , comnCnfgId: comnCnfgId
                        , comnCnfgDefId: comnCnfgDefId
                        , tbl: (col === "CNTND_IN_SEQ_NO" ? "COMN_CNFG_DEF" : tbl)
                        , dsc: dsc
                        , col: col
                        , oldVal: oldRec[col], newVal: newRec[col]
                    };
                    colsToAdd.forEach(function (c) { if (newRec[c]) info[c] = newRec[c]; });
                    changes.push(info);
                    hasChanged = true;
                }
                if (print) {
                    self.dbg("checkAndCollectChanges", info);
                }
            }

            // helper function...
            var checkAndCollectChanges = function (guid, comnCnfgDefId, recId, dsc, tbl, col, oldRec, newRec, print) {
                if (col.indexOf("_") === 0)
                    return; // ignore these fields..

                var ov = (oldRec[col] || "");
                var nv = (newRec[col] || "");
                if (ov == nv || (ov == "-1" && nv === ""))
                    return;

                var info = {
                    id: recId
                    , guid: guid
                    , comnCnfgId: comnCnfgId
                    , comnCnfgDefId: comnCnfgDefId
                    , tbl: (col === "CNTND_IN_SEQ_NO" ? "COMN_CNFG_DEF" : tbl)
                    , dsc: dsc
                    , col: col
                    , oldVal: ov, newVal: nv
                };
                changes.push(info);
                hasChanged = true;

                if (print) {
                    self.dbg("checkAndCollectChanges", info);
                }
            }

            //
            // this is where the work starts...
            //

            //
            // STEP 1: HAS ANYTHING CHANGED ON THE 'COMN_CNFG' record...
            //
            if (self.isNewConfig() === false) {

                orig = self.audit.cc;

                // iterate through the column set of the COMN_CNFG record
                // and look for changes, if this isn't a new record...
                var tbl = "COMN_CNFG"
                for (var col in curr_cc) {
                    checkAndCollectChanges("", 0, curr_cc.COMN_CNFG_ID, curr_cc.COMN_CNFG_NM, tbl, col, orig, curr_cc);
                }
                curr_cc._hasChanged = (curr_cc._hasChanged || hasChanged);
            }


            // 
            // STEP 2: HAS ANYTHING CHANGED ON THE CHILD 'COMN_CNFG_DEF/COMN_CNFG_CNTND_IN_xxxx' RECORDS
            // 

            var origItems = self.audit.items;
            var currItems = self.comnCnfgItemList();

            var curr = null;

            // iterate through the columns in the child item list and look for changes...
            var len = origItems.length;
            for (var i = 0; i < len; i++) {
                hasChanged = false;

                orig = origItems[i];
                tbl = orig.CDMMS_COMN_CNFG_CNTD_IN_TBL_NM;

                var cid = (orig.CNTND_IN_ID || orig.CDMMS_ID);
                var comnCnfgDefId = orig.COMN_CNFG_DEF_ID;

                // we have to find the item in the list because it's sequence number may have changed
                var ndx = currItems.findIndex(function (e) { return e.COMN_CNFG_DEF_ID == comnCnfgDefId; });
                if (ndx === -1) {
                    continue; // not found...
                }

                curr = currItems[ndx];

                var dsc = (curr.RT_PART_NO || curr.PART_NO || curr.MTRL_DSC);
                var cols = Object.keys(orig).filter(function (key) { return orig[key] != curr[key]; }); // what cols have changed...
                for (var ndx = 0; ndx < cols.length; ndx++) {
                    var col = cols[ndx];
                    checkAndCollectChanges(curr._guid, comnCnfgDefId, cid, dsc, tbl, col, orig, curr);  // fill 'changes[]' array if there are any....
                }
                curr._hasChanged = (curr._hasChanged || hasChanged);
            }

            // 
            // STEP 3: HAS ANYTHING CHANGE ON THE CHILD RECORDS INSIDE A CONTAINED-IN COMMON CONFIG OR HLPN
            // 

            // we are ony concerned about pre-existing records (ie: COMN_CNFG_DEF_ID > 0)
            // and, at this point, only interested in 2nd level locatable items: Bay Extender, Shelf, Node, Card
            var locTypes = ["Bay Extender", "Shelf", "Node", "Card"]
            var $childItems = $("#uiComnConfigItemList").find(".depth2"); // ie: one level down, parent is .depth1
            $childItems.each(function (ndx, item) {
                var $i = $(item);
                var guid = $i.data("guid");

                var rec = currItems.findItem(function (e) {
                    return (e.COMN_CNFG_DEF_ID || -1) > 0       // existing record
                        //        && e._depth === 2                   // i.e. one level down, parent is 1
                        && locTypes.contains(e.FEAT_TYP)
                        && e._guid === guid
                        ;
                });
                if (!rec) {
                    return; // probably a non-locatable item, new item, or grandchild item...
                }

                // found a locatable item, check for any changes...
                //
                var currRec = JSON.parse(JSON.stringify(rec)); // make a copy because we are going to change some things...
                var parent = currItems.findItem(function (e) { return e._guid === currRec._parentGUID; });

                // since this is an item inside a CC or HLPN it won't have it's own COMN_CNFG_DEF_ID so borrow the parent's for now...
                currRec._cntndInDefTyp = (parent.FEAT_TYP || parent.MTRL_CAT_TYP);
                currRec.COMN_CNFG_DEF_ID = parent.COMN_CNFG_DEF_ID;

                var key = currRec._parentGUID;
                var subList = (self.childItemCache[key] || []); // original records...
                rec = subList.findItem(function (e) { return e._guid === guid; });
                if (!rec) {
                    return; // shouldn't happen
                }
                origRec = JSON.parse(JSON.stringify(rec)); // make a copy
                origRec.COMN_CNFG_DEF_ID = parent.COMN_CNFG_DEF_ID;

                var colsToIgnore = ["CNTND_IN_SEQ_NO", "SEQ_NO", "CNTMT_TYP"]; // this can change but it doesn't matter so ignore it...
                var colsToAdd = ["COMN_CNFG_CNTND_HLP_MTRL_DF_ID"  // HLPN columns...
                    , "CNTND_IN_HLP_MTRL_REVSN_DEF_ID"
                    , "HLP_MTRL_REVSN_DEF_ID"

                    , "COMN_CNFG_CNTD_COMN_CNFG_DF_ID" // CC columns...
                    , "CNTND_IN_COMN_CNFG_DEF_ID"
                    , "CNTND_IN_PRNT_TBL_NM"

                    , "_cntndInDefTyp"                  // backend will need this info...
                    , "_depth"

                    , "SRC"
                ]; // we need this info for the audit log...
                collectChanges(origRec, currRec, colsToIgnore, colsToAdd, true); // fill 'changes[]' array if there are any....
            });


            self.dbg("determineChanges", changes);

            return changes;
        }
        ComnCnfg.prototype.syncChildItemUpdates = function () {
            // don't think we should have to do this because that's what knockout if for, right?
            // but regardless, we need to sync any 'added' items as well that might have changed before saving...
            var hasChanged = function (col, prev, curr) {
                if (prev == curr) {
                    return false;
                }

                if (col.endsWith("_ID")) {
                    prev = (prev || "-1");
                    curr = (curr || "-1");

                    if (isNaN(prev) && curr == "") {
                        return false;
                    }
                    if (prev == "" && isNaN(curr)) {
                        return false;
                    }
                }
                return true;
            }

            var whatChanged = function (col, prev, curr) { return col + " changed from " + prev + " to " + curr; }

            var list = self.comnCnfgItemList();
            list.forEach(function (e) {
                e._changes = (e._changes || []);

                var audit = self.audit.added.findItem(function (a) { return a._guid === e._guid; });

                var $tr = $("#rec" + e._guid);
                var $fields = {
                    FRNT_RER_IND: $tr.find(".placement")
                    , PRNT_COMN_CNFG_DEF_ID: $tr.find(".relatedto")
                    , CNTND_IN_REVSN_LVL_IND: $tr.find(".revisions")
                    , X_COORD_NO: $tr.find(".X_COORD_NO")
                    , Y_COORD_NO: $tr.find(".Y_COORD_NO")
                    , LABEL_NM: $tr.find(".LABEL_NM")
                    , RACK_POS: $tr.find(".RACK_POS")
                };
                for (var col in $fields) {
                    var $f = $fields[col];
                    if ($f.length === 0) {
                        continue;
                    }

                    var curr = $f.val();
                    var prev = (e[col] || "");

                    console.log("syncChildItemUpdates: %s %s %s", e.CNTND_IN_SEQ_NO, col, curr);
                    e[col] = curr;

                    // CHECKHERE: mwj I don't think this is correct... only for newly added items that we don't have an audit for...
                    //if (audit && !audit[col] ) {
                    //    audit[col] = prev;
                    //}

                    if (hasChanged(col, curr, prev) === false) {
                        continue;
                    }

                    e._changes.push(whatChanged(col, prev, curr));

                    if (col !== "PRNT_COMN_CNFG_DEF_ID") {
                        e[col] = curr; // regular cases....
                        continue;
                    }

                    //
                    // special case col is PRNT_COMN_CNFG_DEF_ID which contais a guid 
                    // so we have to find the actual parent in the list...
                    //
                    if (!curr || curr.length === 0 || curr === "-1") { // user cleared the parent info...
                        e.PRNT_COMN_CNFG_DEF_ID = -1;
                        e.PRNT_CI_HLP_MTRL_REVSN_DEF_ID = -1;
                        e.PRNT_CI_COMN_CNFG_DEF_ID = -1;
                        continue;
                    }

                    // parent info is set, find the parent and sync the "related to" info...
                    // 'curr' is in the format of 'COMN_CNFG_DEF_ID.PRNT_CI_HLP_MTRL_DEF_ID.PRNT_CI_COMN_CNFG_DEF_ID' 
                    // (ie. 1230. - 1. - 1 for top level, 1230.9870.-1 for c / i hlp, 1230.-1.5670 for c / i cc)
                    if (curr && curr.indexOf(".") > 0) {
                        var ids = curr.split('.');
                        var prnt = { COMN_CNFG_DEF_ID: ids[0], CNTND_IN_HLP_MTRL_REVSN_DEF_ID: ids[1], CNTND_IN_COMN_CNFG_DEF_ID: ids[2] };

                        // debugging stuff...
                        prev = e.PRNT_COMN_CNFG_DEF_ID + '.' + e.PRNT_CI_HLP_MTRL_REVSN_DEF_ID + '.' + e.PRNT_CI_COMN_CNFG_DEF_ID;
                        console.log("syncChildItemUpdates: %s %s %s=prev %s / curr %s (%s)", e.FEAT_TYP, e.RT_PART_NO, col, prev, curr, (e._isNew ? "NEW" : ""));// REMOVETHIS

                        e.PRNT_COMN_CNFG_DEF_ID = prnt.COMN_CNFG_DEF_ID;
                        e.PRNT_CI_HLP_MTRL_REVSN_DEF_ID = prnt.CNTND_IN_HLP_MTRL_REVSN_DEF_ID
                        e.PRNT_CI_COMN_CNFG_DEF_ID = prnt.CNTND_IN_COMN_CNFG_DEF_ID;
                    }
                }
            });
        }

        ComnCnfg.prototype.saveConfig = function (next) {
            next = (next || function () { });

            var $form = $("#EditCommonConfigDialog");

            // remove any warnings, we'll put them back later if needed...
            var cssAlertWarn = "alert-warning"
            var $alerts = $form.find("." + cssAlertWarn);
            $alerts.removeClass(cssAlertWarn)

            // evidently, I'm not using knockout here correctly, so 
            // just getting a copy and then sync'ing the observable
            var cc = UiUtils.GetFormData($form, { removePrefix: true, unCheckedValue: 'N' });

            // brief sanity checks; make sure they have a name and a template type
            if (cc.COMN_CNFG_NM.length === 0) {
                self.showMessage("Common Config must at least have a name!");
                return;
            }
            if (!cc.TMPLT_TYP_ID || cc.TMPLT_TYP_ID.length === 0) {
                $form.find("#edtTMPLT_TYP_ID").addClass(cssAlertWarn);
                self.showMessage("Please select a config type from the drop down.");
                return;
            }

            var continueWithSave = function () {

                cc.TMPLT_TYP = $form.find("#edtTMPLT_TYP_ID option:selected").text();
                cc._isNew = self.isNewConfig();

                // get original config record so we can see what changed, if new/none create a default one...
                var curr_cc = self.currCommonConfig();
                curr_cc = (curr_cc || { COMN_CNFG_ID: 0, _hasChanged: true, _isNew: true });
                $.extend(curr_cc, cc);
                curr_cc._hasChanged = false; // to be determined...

                self.showWheelOfPatience(true);

                // if any items have been deleted, we need to generate updated sequence #s
                if (self.audit.deleted.length > 0) {
                    self.refreshSequenceNumbers();
                }

                // mwj: i don't know how to get knockout to give me these changes...
                self.syncChildItemUpdates();

                var auditLog = self.determineChanges(curr_cc);
                cc._hasChanged = (cc._isNew ? true : curr_cc._hasChanged);

                // ensure the comn_cnfg_id is on any new records...
                self.audit.added.forEach(function (e) {
                    e.COMN_CNFG_ID = cc.COMN_CNFG_ID;
                });

                // these are existing items that have changed...
                var changed = self.comnCnfgItemList().filter(function (e) {
                    return e._hasChanged === true && !e._isNew && !e._isDeleted;
                });

                // make sure rack positions don't overlap....
                var errors = self.ensureRackPositionsDontOverlap(self.comnCnfgItemList);
                if (errors.length > 0) { // can't save...
                    self.showMessage(errors, "Found Invalid or Overlapping Rack Positions");
                    return;
                }

                // these are any changes to PRNT_COMN_CNFG_DEF_ID, FRNT_RER_IND, X/Y, LABEL_NM, & RACK_POS for 
                // those HLP and CC contained in child items for existing records...
                var currCntndInChildItems = self.collectCntndInChildItemRecordChanges(auditLog);
                var newCntdInChildItems = self.collectNewCntndInChildItemRecords();

                // complete list of contained-in child items...
                var cntndInChildItems = currCntndInChildItems.concat(newCntdInChildItems);

                auditLog = self.groomAuditLog(auditLog); // any last minute changes that we need

                var success = function (status) {
                    self.showWheelOfPatience(false);

                    if (status.ERR) {
                        self.showMessage(status.ERR);
                        return;
                    }
                    if (status.EC == 0) {
                        cc.COMN_CNFG_ID = status.OV;
                    }

                    cc._hasChanged = false;
                    cc._isNew = false;

                    next(status, cc);
                }
                var fatal = function (err) {
                    self.showWheelOfPatience(false);
                    self.showMessage(err, "Problem Saving Common Config")
                }

                var payload = {
                    cuid: self.usr.cuid
                    , comnCnfg: cc
                    , items: self.audit.items
                    , changed: changed
                    , added: self.audit.added
                    , deleted: self.audit.deleted
                    , cntndInChildItems: cntndInChildItems
                    , auditlog: auditLog
                };

                $.ajax({
                    type: "POST",
                    url: 'api/comncnfg/update',
                    data: JSON.stringify(payload),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: success,
                    error: fatal,
                    context: self
                });

                //self.dbg("payload", JSON.parse(JSON.stringify(self.audit.added)));
                self.dbg("payload", JSON.parse(JSON.stringify(payload)).changed);
                //self.dbg("payload", JSON.parse(JSON.stringify(payload)).cntndInChildItems);
                self.showWheelOfPatience(false);
            }

            var isDel = (cc.DEL_IND === 'Y' && self.audit.cc.DEL_IND === 'N');
            var isRet = (cc.RET_COMN_CNFG_IND === 'Y' && self.audit.cc.RET_COMN_CNFG_IND === 'N');
            if (isDel || isRet) {
                var opts = {};
                opts.msg = (isDel ? "Are you sure you want to delete this config?" : "Are you sure you want to deactivate this config?");
                opts.caption = (isDel ? "Delete Common Config?" : "Deactivate Common Config?");
                opts.buttons = [(isDel ? "Delete Config" : "Deactivate Config"), "Cancel"];
                opts.css = { style: { width: "400px" } }
                opts.onClick = function (ans) {
                    if (ans === "CANCEL") {
                        return;
                    }
                    continueWithSave();
                };
                self.confirm(opts);
                return;
            }

            continueWithSave();
        }

        ComnCnfg.prototype.ensureRackPositionsDontOverlap = function () {
            $(".RACK_POS").css("background-color", "white"); // clear any errors...

            var list = self.comnCnfgItemList();

            var rackPos = [];

            $(".RACK_POS").each(function (i, e) {

                var $e = $(e);

                var pos = parseInt($e.val());
                if (!rackPos[pos]) {
                    rackPos[pos] = [];
                }

                var $tr = $e.parents("tr:first");
                var guid = $tr.data("guid");
                var rec = list.findItem(function (item) {
                    return item._guid === guid;
                });

                if (rec) {
                    rec.RACK_POS = pos;
                }

                rackPos[pos].push(rec);

            });

            var overlaps = rackPos.filter(function (e) { return e.length > 1; }) || [];
            return overlaps;
        }
        ComnCnfg.prototype.hiliteOverlappingRackPositions = function (list) {
            list = (list || self.ensureRackPositionsDontOverlap());
            var errors = [];
            list.forEach(function (items) {
                items.forEach(function (e) {
                    $("#rec" + e._guid).find(".RACK_POS").css("background-color", "salmon");
                    errors.push("#" + e.CNTND_IN_SEQ_NO + ": " + e.CLMC + " " + e.RT_PART_NO + " at rack position " + e.RACK_POS);
                });
            });

            return errors;
        }


        ComnCnfg.prototype.collectNewCntndInChildItemRecords = function () {
            // this will collect locatable information for NEWLY added HLPN/CC child items that don't have a DB reference yet...
            var origItems = self.audit.items;
            var locTypes = ["Bay Extender", "Shelf", "Node", "Card"];
            var candidates = self.comnCnfgItemList().filter(function (e) {

                e._newItem = (e._newItem || []);
                e._newItem.push("isChild: " + e._isChild);
                e._newItem.push("COMN_CNFG_DEF_ID: " + e.COMN_CNFG_DEF_ID);
                e._newItem.push("COMN_CNFG_CNTD_COMN_CNFG_DF_ID: " + e.COMN_CNFG_CNTD_COMN_CNFG_DF_ID);
                e._newItem.push("FEAT_TYP: " + locTypes.findIndex(function (i) { return e.FEAT_TYP == i; }));

                var rv = (e._isChild || false) === true
                    && (e.COMN_CNFG_DEF_ID || -1) == -1
                    && (e.COMN_CNFG_CNTD_COMN_CNFG_DF_ID || -1) == -1 // CC
                    && (e.COMN_CNFG_CNTND_HLP_MTRL_DF_ID || -1) == -1 // HLP
                    && locTypes.findIndex(function (i) { return e.FEAT_TYP == i; }) != -1
                    ;

                if (rv) {
                    console.log(e.CNTND_IN_SEQ_NO + ": " + e._newItem.join(" / "));
                }
                return rv;
            });

            var children = candidates.filter(function (e) {
                // if any one of these are set, we need to save it...
                var rv = e.PRNT_COMN_CNFG_DEF_ID > 0
                    || (e.X_COORD_NO || "0") > 0
                    || (e.Y_COORD_NO || "0") > 0
                    || (e.FRNT_RER_IND || "") != ""
                    || (e.LABEL_NM || "") != ""
                    || (e.RACK_POS || "") != ""
                    ;

                return rv;
            });
            return children;
        }
        ComnCnfg.prototype.collectCntndInChildItemRecordChanges = function (changes) {
            var allItems = self.comnCnfgItemList();

            var cntndInChildItems = [];
            changes.forEach(function (colInfo) {
                var id = colInfo.id;
                var guid = colInfo.guid;
                var tbl = colInfo.tbl;
                if (tbl !== "comn_cnfg_cntnd_hlp_mtrl_def") {
                    var item = allItems.findItem(function (e) { return e._guid === guid; });
                    if (!item || !item.CNTND_IN_PRNT_TBL_NM) {
                        return;
                    }
                    tbl = item.CNTND_IN_PRNT_TBL_NM;
                }

                // merge all the changes into a single record...
                var ndx = cntndInChildItems.findIndex(function (e) { return e._guid === guid; });
                var rec = (ndx === -1 ? null : cntndInChildItems[ndx]);
                if (rec) {
                    rec[colInfo.col] = colInfo.newVal;
                    cntndInChildItems[ndx] = rec;
                }
                else {
                    var item = allItems.findItem(function (e) { return e._guid === guid; });
                    if (!item) { // shouldn't happen unless there is a real problem...
                        var msg = "Could not find child item info for " + guid;
                        self.showMessage(msg, "Save Contained-In Child Item Changes");
                        throw msg;
                    }

                    var rec = { _tbl: tbl };
                    $.extend(rec, item);
                    rec[colInfo.col] = colInfo.newVal;

                    cntndInChildItems.push(rec);
                }
            });
            return cntndInChildItems;
        }
        ComnCnfg.prototype.loadOptions = function (cat, next) {
            next = (next || function () { });

            var args = { CAT: cat || "TemplateTypes" };
            http.get('api/comncnfg/getOptionList', args)
                .then(function (opts) {
                    opts.unshift({ text: " ", value: "-1" });
                    next(opts);
                }
                    , function (err) {
                        self.showMessage("Failed to load template options");
                    }
                );
        }
        ComnCnfg.prototype.showWheelOfPatience = function (show) {
            var $i = $("#interstitial");
            var $panel = $("#PANEL_MAIN_SCREEN");

            if (show) {
                $i.css("height", "100%").show();
                $("body").css("cursor", "wait");

                return;
            }

            $i.hide();
            $("body").css("cursor", "default");
        }
        ComnCnfg.prototype.refreshSequenceNumbers = function () {
            // usually this is called before saving when there
            // have been some deleted items and we need to get
            // a refreshed list of seq #s

            // this is tricky because of the possiblity of expanded cntnd-in child items
            var seqNo = 0;
            var list = self.comnCnfgItemList();
            var len = list.length;
            var total = 0;
            list.forEach(function (e) {
                total++;
                if (e._isDeleted)
                    return; // ignore deleted items...

                if (e.CNTND_IN_SEQ_NO.toString().indexOf('-') !== -1)
                    return; // ignore cntnd-in child items...

                seqNo++;
                if (e.CNTND_IN_SEQ_NO == seqNo)
                    return; // ignore no changes to seq #

                e.CNTND_IN_SEQ_NO = seqNo;
                e._hasChanged = true;
            });
            console.log("refreshSequenceNumbers: %s/%s", total, len);
        }

        //ComnCnfg.prototype.moveToNearestNeighbor = function ($curr) {
        //    // if we are here, that means the user changed the seq #
        //    // to a # that isn't in our list so we just need to move it 
        //    // to the vacinity but everybody else seq # stays the same...

        //    var currSeqNo = parseInt($curr.data('curr-seq-no'));
        //    var newSeqNo = parseInt($curr.val());
        //    var list = self.comnCnfgItemList().createNewArray();
        //    list.forEach(function (i) {
        //        if (!i._isChild) {
        //            i.CNTND_IN_SEQ_NO = parseInt(i.CNTND_IN_SEQ_NO); // make sure they are all integers...
        //        }
        //    });

        //    var ndx = list.findIndex(function (e) { return e.CNTND_IN_SEQ_NO == currSeqNo; });
        //    var item = list[ndx];
        //    item.CNTND_IN_SEQ_NO = newSeqNo;

        //    list.splice(ndx, 1); // remove item from list...

        //    var len = list.length;
        //    if (newSeqNo == 1) {
        //        list.splice(0, 0, item); // add to front
        //    }
        //    else {
        //        var i = 0;
        //        for(; i< len; i++) {
        //            if ( list[i]._isChild ) 
        //                continue;

        //            if (list[i].CNTND_IN_SEQ_NO < newSeqNo)
        //                continue;

        //            break;
        //        }
        //        i = (i == len ? i - 1 : i);
        //        list.splice(i, 0, item);
        //    }

        //    self.refreshCommonConfigItemList(list);
        //    self.hilightNewRows();
        //}
        ComnCnfg.prototype.moveToNearestNeighbor = function ($curr) {
            // if we are here, that means the user changed the seq #
            // to a # that isn't in our list so we just need to move it 
            // to the vacinity but everybody else seq # stays the same...

            var currSeqNo = parseInt($curr.data('curr-seq-no'));
            var newSeqNo = parseInt($curr.val());
            var list = self.comnCnfgItemList().createNewArray();
            list.forEach(function (i) {
                if (!i._isChild) {
                    i.CNTND_IN_SEQ_NO = parseInt(i.CNTND_IN_SEQ_NO); // make sure they are all integers...
                }
            });

            var ndx = list.findIndex(function (e) { return e.CNTND_IN_SEQ_NO == currSeqNo; });
            var item = list[ndx];
            list.splice(ndx, 1); // remove item from list...

            var aboveMe = list.filter(function (e) { return !e._isChild && e.CNTND_IN_SEQ_NO < newSeqNo; });
            var belowMe = list.filter(function (e) { return !e._isChild && e.CNTND_IN_SEQ_NO > newSeqNo; });
            var sameAsMe = list.filter(function (e) { return !e._isChild && e.CNTND_IN_SEQ_NO == newSeqNo; });

            item.CNTND_IN_SEQ_NO = newSeqNo;

            if (aboveMe.length === 0) {
                list.splice(0, 0, item); // put me at the top...
            }
            else if (belowMe.length === 0) {
                list.push(item); // put me at the bottom...
            }
            else {
                // I'm somewhere in the middle...
                var pos = (aboveMe.length > 0 ? aboveMe[aboveMe.length - 1] : belowMe[0]);
                ndx = list.findIndex(function (e) { if (e._guid === pos._guid) return true; });
                if (ndx != -1) {
                    list.splice(ndx, 0, item);
                }
            }

            //var len = list.length;
            //if (newSeqNo == 1) {
            //    list.splice(0, 0, item); // add to front
            //}
            //else {
            //    var i = 0;
            //    for (; i < len; i++) {
            //        if (list[i]._isChild)
            //            continue;

            //        if (list[i].CNTND_IN_SEQ_NO < newSeqNo)
            //            continue;

            //        break;
            //    }
            //    i = (i == len ? i - 1 : i);
            //    list.splice(i, 0, item);
            //}

            self.refreshCommonConfigItemList(list);
            self.hilightNewRows();
        }
        ComnCnfg.prototype.resequenceParentCandidates = function () {
            console.log("resequenceParentCandidates: todo");
            var list = self.comnCnfgItemList();
            self.parentCandidates.forEach(function (i) {
                var ccDefId = i.PRNT_COMN_CNFG_DEF_ID;
                var ndx = list.findIndex(function (p) {
                    return p.COMN_CNFG_DEF_ID == ccDefId;
                });
                if (ndx === -1) {
                    return;
                }
                var prnt = list[ndx];
                if (i.PRNT_SEQ_NO == 0) {
                    i.ITEM_SEQ_NO = prnt.CNTND_IN_SEQ_NO;
                }
                else {
                    i.PRNT_SEQ_NO = prnt.CNTND_IN_SEQ_NO;
                }
            });
        }
        ComnCnfg.prototype.reOrderList = function ($curr) {
            if (self.isNewConfig()) {
                $curr.val("1");
                self.showMessage("Please save this Config before trying to re-order the list");
                return;
            }

            var list = self.comnCnfgItemList();
            var listLength = list.length;
            if (listLength === 0) {
                return;
            }

            self.pendingChanges = true;
            console.log("reOrderList - pendingChanges %s", self.pendingChanges);
            var $tr = $curr.parents("tr:first");

            var caption = "Re-Order List";

            var oldSeqNo = parseInt($curr.data("curr-seq-no"));
            var newSeqNo = parseInt($curr.val());

            if (newSeqNo == oldSeqNo) {
                return; // nothing to do
            }


            if (isNaN(oldSeqNo)) {
                self.showMessage("Current Sequence # is not valid", caption);
                return;
            }
            if (isNaN(newSeqNo)) {
                self.showMessage("New Sequence # is not valid", caption);
                return;
            }
            var v = $curr.val();
            if (!(/^[0-9]+$/.test(v))) {
                self.showMessage("New Sequence # is not valid - must be an integer", caption);
                return;
            }

            if (newSeqNo < 1) {
                newSeqNo = 1;
            }

            var $ui = $("#uiComnConfigItemList");
            var ndxCurr = -1, ndxTarget = -1;
            var currId = $curr.parents("tr:first").data("seq-no");
            var $target = $ui.find(".seq-no" + newSeqNo);
            if ($target.length === 0) { //&& newSeqNo < listLength ) {
                //self.showMessage("Could not find item to position in list to move to.", caption);
                //return;

                // this means that the user entered a number that doesn't already exist in the list
                // so that means that we don't have to renumber anything and we just have to take 
                // the seq # as is...however, we should shift it up or down... so find the nearest neighbor
                self.moveToNearestNeighbor($curr);
                return;
            }
            var targetId = $target.parents("tr:first").data("seq-no");
            var len = list.length;
            for (var i = 0; i < len && (ndxCurr === -1 || ndxTarget === -1); i++) {
                var rec = list[i];
                if (rec.CNTND_IN_SEQ_NO == currId) {
                    ndxCurr = i;
                }
                if (rec.CNTND_IN_SEQ_NO == targetId) {
                    ndxTarget = i;
                }
            }

            var refresh = function ($obj, seqNo) {
                var oldNo = $obj.data("curr-seq-no");
                var oldVal = $obj.val();
                $obj.removeClass("seq-no" + oldNo)
                    .addClass("seq-no" + seqNo)
                    .data("curr-seq-no", seqNo)
                    .val(seqNo)
                    ;

                var css = 'alert-success';
                var $p = $obj.parents("tr:first").data("seq-no", seqNo);
                $p.addClass(css).fadeOut('slow', function () { $(this).removeClass(css).show(); });
            }

            var recToMove = (ndxCurr === -1 ? null : list.splice(ndxCurr, 1)[0]); // remove item from the list...

            var moveItemDown = function () {
                if (ndxTarget === -1) {
                    return;
                }

                for (var ndx = ndxCurr; ndx < ndxTarget; ndx++) {
                    var rec = list[ndx];
                    var $i = $ui.find(".seq-no" + rec.CNTND_IN_SEQ_NO);
                    rec.CNTND_IN_SEQ_NO = parseInt(rec.CNTND_IN_SEQ_NO) - 1;
                    refresh($i, rec.CNTND_IN_SEQ_NO);
                }
            }

            var moveItemUp = function () {
                if (ndxTarget === -1) {
                    return;
                }
                for (var ndx = ndxCurr - 1; ndx >= ndxTarget; ndx--) {
                    var rec = list[ndx];
                    var $i = $ui.find(".seq-no" + rec.CNTND_IN_SEQ_NO);
                    rec.CNTND_IN_SEQ_NO = parseInt(rec.CNTND_IN_SEQ_NO) + 1;
                    refresh($i, rec.CNTND_IN_SEQ_NO);
                }
            }

            if (oldSeqNo > newSeqNo) {
                moveItemUp();
            }
            else {
                moveItemDown();
            }

            if (recToMove != null) {
                ndxTarget = (ndxTarget === -1 ? ndxCurr : ndxTarget);
                list.splice(ndxTarget, 0, recToMove);
                recToMove.CNTND_IN_SEQ_NO = newSeqNo;
            }

            refresh($curr, newSeqNo);

            list = self.resyncChildItemSequenceNumbers(list);
            self.resequenceParentCandidates();
            self.refreshCommonConfigItemList(list);
            self.hilightNewRows();

            // close any non-expanded child items...
            var $tr = $ui.find("tr").filter(function (ndx, e) { return $(e).data("expanded") === false });
            $tr.each(function (ndx, e) {
                var $e = $(e);
                var guid = $e.data("guid");
                $ui.find("." + guid).hide();
            })

        }
        ComnCnfg.prototype.resyncChildItemSequenceNumbers = function (itemList) {
            var list = itemList.createNewArray();
            var children = list.filter(function (e) { return e._isChild === true; });
            if (children.length === 0) {
                return list;
            }


            var parents = list.filter(function (e) { return !e._isChild; });
            var tmp = [];
            parents.forEach(function (p) {
                tmp.push(p);

                if (children.length === 0) {
                    return;
                }

                var guid = p._guid;
                var fromNdx = children.findIndex(function (c) { return c._parentGUID === guid; });
                if (fromNdx === -1) {
                    return;
                }

                var toNdx = 0;
                for (var i = fromNdx; i < children.length; i++) {
                    ch = children[i];
                    if (children[i]._parentGUID === guid) {
                        toNdx = i;
                        ch._parentSeqNo = p.CNTND_IN_SEQ_NO;
                        ch.CNTND_IN_SEQ_NO = p.CNTND_IN_SEQ_NO + "-" + ch.SEQ_NO;
                        tmp.push(ch);
                    }
                }
                if (fromNdx >= 0 && toNdx < children.length) {
                    children.splice(fromNdx, (toNdx - fromNdx) + 1);
                }
            })

            list.length = 0;
            list = tmp.createNewArray();
            return list;
        }
        ComnCnfg.prototype.refreshRelatedToOptions = function () {
            // if the user expands a contained in CC or HLPN then we need
            // to update our list of potential parents...
            var $pane = $("#uiComnConfigItemList");
            var $relatedTo = $pane.find(".relatedto");
            $relatedTo.each(function (ndx, ctrl) {
                self.fillRelatedTo($(ctrl));
            });
            self.selectRelatedToOptions();
            self.setDefaultRelatedToOption();
            self.setUserSelectedRelatedToOptions();
        }
        ComnCnfg.prototype.resyncRelatedToOptions = function () {
            // if the user moves a locatable item to a new sequence #
            // we need to update the label in the drop down of the rill
            // "related to" control so it tracks...
            var list = self.comnCnfgItemList();
            var $ctrls = $(".relatedto").filter(":visible");
            $ctrls.each(function (ndx, item) {
                var $i = $(item);
                $i.find("option").each(function (n, o) {
                    var $o = $(o);
                    var val = $o.val();
                    var txt = $o.text();

                    var ndx = list.findIndex(function (rec) { return rec._guid == val; });
                    if (ndx === -1)
                        return;

                    var rec = list[ndx];
                    var label = rec.CNTND_IN_SEQ_NO + ": " + rec.RT_PART_NO + " (" + rec.FEAT_TYP + ")";
                    $o.text(label);
                });
            });
        }
        ComnCnfg.prototype.toggleSearchState = function (state) {
            self.isSearching = state;
            // just in case we need to toggle anything on the UI going at some point in the future...
        }

        ComnCnfg.prototype.findRelatedToChildren = function (rec) {
            var children = [];
            var $relatedTo = $("#uiComnConfigItemList").find(".relatedto").filter(":visible");
            var pId = rec.COMN_CNFG_DEF_ID;

            var hlpOrCc = ["High Level Part", "Generic"]; // generic = common config
            var list = self.comnCnfgItemList();
            $relatedTo.each(function (ndx, c) {
                var val = $(c).val();
                if (val && val.indexOf(pId) >= 0) {
                    var $p = $(c).parents("tr:first");
                    var guid = $p.data("guid");
                    var item = list.findItem(function (e) { return e._guid === guid; });
                    if (!item) {
                        return;
                    }

                    if (hlpOrCc.contains(rec.MTRL_CAT_TYP) && item.CNTND_IN_SEQ_NO.toString().indexOf(rec.CNTND_IN_SEQ_NO) === 0) {
                        return; // we're removing the HLPN or CC, don't report out on contained-in items; they aren't actually touched
                    }

                    children.push(item);
                }
            });

            return children;
        }
        ComnCnfg.prototype.findRelatedToChildrenInOtherConfigs = function (prnt, next) {
            var query = 'api/comncnfg/findRelatedToChildrenInOtherConfigs/' + prnt.COMN_CNFG_ID + '/' + prnt.COMN_CNFG_DEF_ID;
            http.get(query).then(next);
        }
        ComnCnfg.prototype.findReferencesInOtherConfigs = function (ref, next) {
            var query = 'api/comncnfg/findReferencesInOtherConfigs/' + ref.COMN_CNFG_DEF_ID;
            http.get(query).then(next);
        }
        ComnCnfg.prototype.checkForAnyReferences = function (action) {
            var cc = self.currCommonConfig();
            var id = cc.COMN_CNFG_ID;
            if (id === 0) {
                return;
            }

            var display = function (list) {
                self.showWheelOfPatience(false);

                if (!list || list.length === 0) {
                    return;
                }
                self.configRefItems(list);

                var $form = $("#EditCommonConfigDialog");
                var opts = UiUtils.GetFormData($form, { removePrefix: true, unCheckedValue: 'N' });
                opts.action = action;

                AffectedCommonConfigsDialog.Display(opts);
            }

            var fatal = function (err) {
                self.showWheelOfPatience(false);
                self.showMessage(err, "Find Parent/Child Config References");
            }
            self.showWheelOfPatience(true);
            http.get('api/comncnfg/refs/' + id).then(display, fatal);
        }
        ComnCnfg.prototype.scrollTo = function (id) {
            setTimeout(function () {
                var e = document.getElementById(id);
                e.scrollIntoView(true);
            }, 1000);
        };
        // #region UI Events (button clicks, keyboard presses, etc...)
        ComnCnfg.prototype.onCloneConfig = function () {
            if (self.isNewConfig()) {
                self.showMessage("You cannot clone this config yet until you create it.", "Cannot Clone Config");
                return;
            }

            if (!self.pendingChanges) {
                self.showCloneDialog();
                return;
            }

            var msg = ["You have <span style='color: red'>pending changes</span> that have not been saved yet!"
                , ""
                , "Click <b>SAVE CHANGES AND CLONE</b> if you want to save"
                , "these changes first so that they propogate to the cloned config."
            ];
            var opts = {
                msg: msg
                , caption: "Continue to Clone Config?"
                , buttons: ["Save Changes and Clone", "Clone without Changes", "Cancel"]
                , css: { style: { width: "500px" } }
                , onClick: function (ans) {
                    if (ans === "CANCEL") {
                        return;
                    }

                    if (ans === "CLONE WITHOUT CHANGES") {
                        self.showCloneDialog();
                        return;
                    }

                    self.saveChanges(function () { self.showCloneDialog(); });
                }
            };
            self.confirm(opts);
        }
        ComnCnfg.prototype.showCloneDialog = function () {
            var $form = $("#EditCommonConfigDialog");
            var config = UiUtils.GetFormData($form, { removePrefix: true, unCheckedValue: 'N' });

            var success = function (status) {
                self.showWheelOfPatience(false);

                if (status.EC != 0) {
                    self.showMessage(status.MSG, "Problem Cloning Config");
                    return;
                }

                var rec = (!status.REC || status.REC.length === 0 ? null : JSON.parse(status.REC));

                if (rec === null) {
                    return;
                }

                var msg = ["Do you want to load the new config?", "", "<li> #" + rec.COMN_CNFG_ID + ": " + rec.COMN_CNFG_NM];
                self.confirm(msg, "Switch To New Config?", ["Yes", "No"], function (ans) {
                    if (ans === "YES") {
                        self.onSelectCommonConfigEntry(rec);
                    }
                });
            }
            var fatal = function (status) {
                self.showWheelOfPatience(false);
                self.showMessage(status, "Failed to Clone Config");
            }
            var doClone = function (data) {
                self.showWheelOfPatience(true);

                // save user changes...
                config = JSON.parse(JSON.stringify(data));

                // clone payload...
                data.SRC_COMN_CNFG_ID = self.currCommonConfig().COMN_CNFG_ID;
                data.CUID = self.usr.cuid;

                $.ajax({
                    type: "POST",
                    url: 'api/comncnfg/clone',
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: success,
                    error: fatal,
                    context: self
                });
            }

            config.ui = { cloneConfig: doClone };
            CloneConfigDialog.Display(config);
        }
        ComnCnfg.prototype.onViewChangeLog = function () {
            var id = self.currCommonConfig().COMN_CNFG_ID;
            var display = function (list) {
                self.showWheelOfPatience(false);
                self.changeLogItems(list);
                ChangeLogDialog.Display(self.currCommonConfig());
            }
            var fatal = function (err) {
                self.showWheelOfPatience(false);
                self.showMessage(err, "View Change Log");
            }
            self.showWheelOfPatience(true);
            http.get('api/comncnfg/changelog/' + id).then(display, fatal);
        }
        ComnCnfg.prototype.disableIndicators = function ($fields, state) {
            $fields.prop('disabled', state);
        }
        ComnCnfg.prototype.onToggleComnCnfgIndicator = function (rec, event) {
            var $e = $(event.target);
            var checked = $e.is(":checked");
            var id = $e.attr("id");
            var isDltd = (id.indexOf("DEL_IND") >= 0);
            var isRtrd = (id.indexOf("RET_COMN_CNFG_IND") >= 0);
            var isInPrgs = (id.indexOf("UPDT_IN_PRGS_IND") >= 0);

            if (isInPrgs) {
                var $form = $("#EditCommonConfigDialog");
                self.disableIndicators($form.find("#edtDEF_PRPGT_IND,#edtDEF_CMPLT_IND"), checked);
            }
            // if DEL_IND then check if this common config is a parent or child or any other configs....
            if (checked && (isDltd || isRtrd)) {
                self.checkForAnyReferences((isDltd ? "Delete" : "Deactivate"));
            }
            return true;
        }
        ComnCnfg.prototype.onClearSearchForCommonConfig = function () {

            var clear = function () {
                $("#FormSearchForCommonConfig").find("input").val("");
                $("#FormSearchForCommonConfig").find("select :nth-child(1)").prop('selected', true);

                $("#FormSearchForCommonConfigViaPartNumber").find("input").val("");
                $("#FormSearchForCommonConfigViaPartNumber").find("input[type='checkbox']").prop('checked', false);
                $("#FormSearchForCommonConfigViaPartNumber").find("select :nth-child(1)").prop('selected', true);

                self.pendingChanges = false;
                console.log("onClearSearchForCommonConfig - pendingChanges %s", self.pendingChanges);
                self.cachedFetchRelatedToData = {};
                self.toggleSearchState(false);

                self.currCommonConfig(null);
                self.initAudit(null);
                self.ccSearchResults([]);
                self.comnCnfgItemList([]);
                self.shouldDisplaySaveButtons(false);

                self.isNewConfig(false);
                self.isEditing(false);

                self.showConfigChildItems(false);
                self.shouldDisplaySaveButtons(false);

                self.updateConfigTypeWarnings(null);
                self.updateStatusBar(null);
                self.onClearEditCommonConfigDialog();
            }

            self.checkForPendingChanges(clear);
        }

        ComnCnfg.prototype.doExactSearchForCommonConfig = function ( next ) {
            next = (next||function(){});
            var id =  $('#srchCOMN_CNFG_ID');

            self.ccSearchResults([]);
            self.currCommonConfig(null);
            self.isEditing(false);
            self.showConfigChildItems(false);
            self.shouldDisplaySaveButtons(false);
            self.toggleSearchState(true);
            self.showWheelOfPatience(true);

            var $form = $("#FormSearchForCommonConfig");
            
            var pfx = $form.data("pfx");
            var args = UiUtils.GetFormData($form);
            args = UiUtils.ConvertToDbProcParams(args, pfx);

            var display = function (data) {
                data = (data||[]);
                self.ccSearchResults(data);
                if (data.length === 0) {
                    self.showMessage("Could not find Common Config entry #"+self.autoSearchId, "Common Config Not Found");
                    return;
                }
                self.onSelectCommonConfigEntry(data[0])
                self.showWheelOfPatience(false);
                self.toggleSearchState(false);
                next();
            }

            var fatal = function (err) {
                self.showMessage(err);
                next({ err: err, results: null });
            }

            if (window.location.hash.indexOf('comnCnfg') === -1) {
                return;
            }
            http.get('api/comncnfg/search/exact', args).then(display, fatal);
        }

        ComnCnfg.prototype.onSearchForCommonConfig = function () {
            if (self.isSearching) {
                self.showMessage("Search in progresss, click CLEAR to cancel...");
                return;
            }

            var search = function () {
                var searchCount = 0;
                var hasData = 0;
                var lists = [];
                function resultsFromPartNumber(data) {
                    data.results = data.results || [];
                    hasData += data.results.length;
                    lists.push(data);
                }

                function resultsFromCommonConfig(data) {
                    data.results = data.results || [];
                    hasData += data.results.length;
                    lists.push(data);
                }

                var timer = null;
                function isDataReady() {
                    if (self.isSearching === false && timer) {
                        clearTimeout(timer)
                        timer = null;
                        return;
                    }

                    if (lists.length != searchCount) {
                        return;
                    }
                    clearTimeout(timer);
                    timer = null;

                    self.toggleSearchState(false);
                    self.showWheelOfPatience(false);
                    setTimeout(function () { self.showConfigChildItems(false); }, 250); // kludge!


                    if (hasData === 0) {
                        self.showMessage(MSG_NO_RESULTS_FOUND);
                        return;
                    }

                    var comparer = function (a, b) {
                        a.COMN_CNFG_ID = parseInt(a.COMN_CNFG_ID);
                        b.COMN_CNFG_ID = parseInt(b.COMN_CNFG_ID);

                        return (a.COMN_CNFG_ID < b.COMN_CNFG_ID ? -1
                            : a.COMN_CNFG_ID > b.COMN_CNFG_ID ? 1
                                : 0);
                    }


                    if (lists.length === 1) {
                        lists.push({ results: [] });
                    }

                    var results = lists[0].results.concat(lists[1].results)
                    results = results.sort(comparer);

                    // remove dups...
                    var filtered = results.filter(function (item, pos, array) {
                        return array.map(function (todo) {
                            return todo.COMN_CNFG_ID
                        }).indexOf(item.COMN_CNFG_ID) === pos;
                    });

                    var final = filtered.sort(comparer);
                    self.ccSearchResults(final);

                    var $panel = $("#PANEL_CC_SEARCH_RESULTS").show();
                    $panel.find(".total").text(": records found = " + results.length);


                }

                self.ccSearchResults([]);
                self.currCommonConfig(null);
                self.isEditing(false);
                self.showConfigChildItems(false);
                self.shouldDisplaySaveButtons(false);
                self.toggleSearchState(true);
                self.showWheelOfPatience(true);

                // have to figure out which search type we are doing, maybe both
                var $cc = $("#FormSearchForCommonConfig");
                var $pn = $("#FormSearchForCommonConfigViaPartNumber");

                var ccSearch = UiUtils.GetFormDataLength(UiUtils.GetFormData($cc));
                var pnSearch = UiUtils.GetFormDataLength(UiUtils.GetFormData($pn));

                if ((ccSearch === 0 && pnSearch === 0) || ccSearch > 0) {
                    searchCount++;
                    self.doSearch($cc, resultsFromCommonConfig);
                }

                if (pnSearch > 0) {
                    searchCount++;
                    self.doSearch($pn, resultsFromPartNumber);
                }

                timer = setInterval(isDataReady, 250);
            }

            self.checkForPendingChanges(search);
        }
        ComnCnfg.prototype.onUpdateSequence = function (root, event) {
            var $item = $(event.target);
            self.reOrderList($item);
        }
        ComnCnfg.prototype.onCheckEnter = function (root, event) {
            if (event.keyCode != 13) {
                return true;
            }

            var $item = $(event.target);
            if ($item.hasClass('seq-no')) {
                self.reOrderList($item);
                return;
            }
            if ($item.hasClass('qty')) {
                return;
            }
            if (SelectConfigItemDialog.IsActive()) {
                SelectConfigItemDialog.OnCheckEnter();
                return;
            }
            self.onSearchForCommonConfig();
        };
        ComnCnfg.prototype.onSave = function () {
            self.showWheelOfPatience(true);
            self.saveChanges();
        }
        ComnCnfg.prototype.saveChanges = function (next) {
            next = (next || function () { });

            var overlaps = self.hiliteOverlappingRackPositions();
            if (overlaps.length > 0) {
                self.showWheelOfPatience(false);
                // can't save until user fixes...
                app.showMessage(overlaps.join("<br/>"), "Overlapping Rack Mount Positions Found");
                return;
            }

            self.saveConfig(function (status, comnCnfg) {
                self.showWheelOfPatience(false);
                if (status.EC != 0) {
                    self.showMessage(status.MSG);
                    return;
                }

                if (status.WARNINGS) {
                    self.showMessage(status.WARNINGS.split('|').join("<br/>"), "!!! WARNING !!!");
                }

                self.pendingChanges = false;
                console.log("saveConfig - pendingChanges %s", self.pendingChanges);
                if (self.isNewConfig()) {
                    self.isNewConfig(false);

                    self.displayConfigEntry(comnCnfg);
                }
                if (comnCnfg.DEL_IND === 'Y') {
                    self.reset();
                }
                else {
                    self.fetchChildItems(); // refresh list...
                }
                next();
            });
        }
        ComnCnfg.prototype.onRemoveConfigItem = function (rec, event) {
            //
            // just fyi, have to "return true;" if you want 
            // the checkbox to toggle...
            //

            if (self.isNewConfig()) {
                self.showMessage("Please save the new Config first if you want to change the initial item.");
                return false;
            }
            var $i = $(event.target);
            var shouldRemove = ($i.is(":checked") === false);

            var cssToBeRemoved = "to-be-removed";
            var $ui = $("#uiComnConfigItemList");
            var $tr = $ui.find("tr#rec" + rec._guid);

            var reCheckBox = function () {
                // kludge!
                setTimeout(function () { $ui.find("tr#rec" + rec._guid).find(".ckDELETE").prop('checked', true); }, 250);
            }

            self.pendingChanges = true;
            console.log("onRemoveConfigItem - pendingChanges %s", self.pendingChanges);

            if (shouldRemove === false) {
                rec._isDeleted = false;
                $tr.removeClass(cssToBeRemoved);
                self.audit.deleted.removeItem(function (e) { return e._guid === rec._guid; });
                if (rec._children) {
                    rec._children.forEach(function (e) {
                        $ui.find("tr#rec" + e._guid).find(".relatedto").selectOption(rec.COMN_CNFG_DEF_ID);
                    });
                    delete rec._children;
                }
                self.refreshInfoStatusBar();
                return true;
            }

            var markForDelete = function (item) {
                item._isDeleted = shouldRemove;
                $tr.addClass(cssToBeRemoved);

                if (!item.CNTND_IN_ID && item.CDMMS_ID) {
                    item.CNTND_IN_ID = item.CDMMS_ID;
                }

                if (self.audit.deleted.contains(function (e) { return e._guid === item._guid; }) === false) {
                    self.audit.deleted.push(item);
                }


                self.refreshInfoStatusBar();

                return true;
            }

            // this are for existing items...
            if (rec.COMN_CNFG_DEF_ID > 0) {
                self.attemptToMarkExistingItemForRemoval($ui, rec, markForDelete, reCheckBox);
                return true; // un-check it but we might have to recheck it depening on user's response
            }


            //
            // these are newly added items that haven't been saved yet...
            //

            // close because we have to reorder the list...
            self.closeAllExpandedRows();

            var findIt = function (e) { return (e._guid ? rec._guid === e._guid : false); }

            var list = self.comnCnfgItemList();

            // if it was a HLP or CC, just remove the child rows...
            var $children = $("tr." + rec._guid);
            $children.each(function (ndx, item) {
                var guid = $(item).data("guid")
                list.removeItem(function (e) { return e._guid === guid; });
                //console.log("guid: " + guid + " = " + list.length);
            });
            $children.empty().remove();


            list.removeItem(findIt);
            self.audit.added.removeItem(findIt);

            // re-order items...
            for (var i = 0; i < list.length; i++) {
                //console.log("remove item: re-order " + list[i].COMN_CNFG_DEF_ID + " from " + list[i].CNTND_IN_SEQ_NO + " to " + (i + 1));
                list[i].CNTND_IN_SEQ_NO = (i + 1);
            }

            self.refreshCommonConfigItemList([]);
            self.refreshCommonConfigItemList(list);

            return true;
        }

        ComnCnfg.prototype.attemptToMarkExistingItemForRemoval = function ($ui, rec, markForDelete, reCheckBox) {

            var typ = self.coalesce(rec.FEAT_TYP, rec.MTRL_CAT_TYP);
            var dsc = '#' + rec.CNTND_IN_SEQ_NO + ": " + rec.CLMC + " " + rec.RT_PART_NO + " (" + typ + ")";

            var timer = null;
            var localChildren = null; // children in this config
            var otherChildren = null; // children in other configs
            var otherConfigs = null; // other configs that we are used in...

            var continueWithMarkForDelete = function () {
                //
                // holy crap this is complicated!
                //

                // once we have all the children, we can continue...
                if (!localChildren || !otherChildren || !otherConfigs) {
                    console.log("markForDelete: waiting on child list...");
                    return;
                }

                clearInterval(timer);
                timer = null;

                if (localChildren.length === 0 && otherChildren.length === 0 && otherConfigs.length === 0) {
                    // no children, we can remove this item....
                    markForDelete(rec);
                    return;
                }

                //
                // there are children, display a list to the user so they know the consequences.... ugg
                //
                localChildren = JSON.parse(JSON.stringify(localChildren)); // make copy cos we're going to modify the items..

                var items = []; // these are the items we need to clear drop downs for...

                var expandedChildren = localChildren.filter(function (e) { return e.CNTND_IN_SEQ_NO.toString().indexOf("-") > 0; });
                expandedChildren.forEach(function (exp) {
                    items.push(exp); // we need to clear these drop down boxes...
                    localChildren.removeItem(function (e) { return e._guid === exp._guid; });
                });

                // pull out any child items that are contained by ourselves; note: REF_COMN_CNFG_ID is currently selected config
                var ci = otherChildren.filter(function (e) { return e.REF_COMN_CNFG_ID === e.AFFECTED_COMN_CNFG_ID; });
                if (ci.length > 0) {

                    // reduce it down one more level, because maybe the user expanded something so it's already in our "local" list...
                    var remove = [];
                    for (var i = 0; i < ci.length; i++) {
                        var item = ci[i];
                        var found = localChildren.filter(function (e) { return e.CNTND_IN_COMN_CNFG_DEF_ID === item.AFFECTED_COMN_CNFG_DEF_ID; });
                        if (found.length > 0) {
                            remove.push(i); // save the index
                        }
                    }
                    remove.forEach(function (ndx) { ci.splice(ndx, 1) }); // user has expanded these items already, take them ...

                    // add these unexpanded c/i children to our "local" list
                    localChildren = localChildren.concat(ci);

                    // get the final "other" list; i.e. skip all the "local" c/i children we just moved...
                    otherChildren = otherChildren.filter(function (e) { return e.REF_COMN_CNFG_ID !== e.AFFECTED_COMN_CNFG_ID; });
                }

                //
                // almost there!
                //

                // create the display lines...
                var local = [];
                localChildren.forEach(function (e) {
                    if (e.CLMC) {
                        local.push("<li>#" + e.CNTND_IN_SEQ_NO + ") <b>" + e.CLMC + " " + e.RT_PART_NO + "</b> (" + self.coalesce(e.FEAT_TYP, e.MTRL_CAT_TYP) + ")</li>");
                    }
                    else {
                        // these are the "unexpanded" c/i children
                        local.push("<li>#" + e.SEQ_NO + ") <b>" + e.FEAT_TYP + "</b> in <b>" + e.CAT + " " + e.CNTND_IN_ID + "</b> </li>");
                    }
                    items.push(e);
                })


                // too much code, wrapped it up...
                var others = self.createSummaryDependencyInfo(otherChildren);

                //
                // build the complete message
                //
                var total = (localChildren.length + otherChildren.length);
                var markup = ["Click <b> Proceed</b> to automatically remove any related-to parent relationships..."
                    + "<br/><br/>"
                    + "The items listed below are dependent on <b>" + dsc + "</b>"
                    + "<br/>"
                ];
                if (local.length > 0) {
                    markup.push("<hr/>");
                    markup.push(local.join(""));
                }
                if (others.length > 0) {
                    markup.push("<hr/>");
                    markup.push("<div style='font-weight: bold; text-decoration: underline; padding: 4px;'>Affected Related-To Parent Assignments:</div>");
                    markup.push("<div class='scrollable'>");
                    markup.push(others);
                    markup.push("</div>");
                }

                if (otherConfigs.length > 0) {
                    markup.push("<hr/>");
                    markup.push("<div style='font-weight: bold; text-decoration: underline; padding: 4px;'>Other Common Configs:</div>");
                    markup.push("<div class='scrollable'>");
                    otherConfigs.forEach(function (e) {
                        markup.push("<li>CC #" + e.COMN_CNFG_ID + " " + e.COMN_CNFG_NM + "</li>");
                    });

                    markup.push("</div>");
                }

                // prompt the user if we should continue....
                self.confirm(markup.join(""), "Auto Remove Related-To Parent?", ["Proceed", "Cancel"], function (ans) {
                    if (ans !== "PROCEED") {
                        reCheckBox();
                        return;
                    }

                    items.forEach(function (e) {
                        $ui.find("tr#rec" + e._guid).find(".relatedto").val("-1.-1.-1").trigger('change');
                    });

                    rec._children = items; // save these in case use "undeletes"
                    markForDelete(rec);
                    self.showMessage("Please remember to save your changes!", "Reminder!");
                }
                    , { 'class': 'modal-content messageBox wide-msg-box' });
            }

            var findChildrenInThisConfig = function () {
                localChildren = self.findRelatedToChildren(rec);
                localChildren = (localChildren || []);
            }
            var findChildrenInOtherConfigs = function () {
                self.findRelatedToChildrenInOtherConfigs(rec, function (list) {
                    otherChildren = (list || []);
                });
            }

            self.findReferencesInOtherConfigs(rec, function (list) {
                otherConfigs = (list || []);
            });

            timer = setInterval(continueWithMarkForDelete, 500);

            findChildrenInOtherConfigs();
            findChildrenInThisConfig();
        }
        ComnCnfg.prototype.createSummaryDependencyInfo = function (info) {
            if (!info || info.length === 0) {
                return "";
            }

            var rv = [];

            var dsc = "";
            var summary = {};
            info.forEach(function (e) {
                if (!summary[e.AFFECTED_COMN_CNFG_ID]) {
                    summary[e.AFFECTED_COMN_CNFG_ID] = { info: e };
                }

                summary[e.AFFECTED_COMN_CNFG_ID][e.FEAT_TYP] = (summary[e.AFFECTED_COMN_CNFG_ID][e.FEAT_TYP] || 0) + 1;
            });

            var items = [];
            for (var key in summary) {
                var e = summary[key];
                var i = e.info;
                dsc = "CC #" + i.AFFECTED_COMN_CNFG_ID + " " + i.AFFECTED_COMN_CNFG_NM
                    + (i.CAT === "HLPN" ? " (HLP #" + i.CNTND_IN_ID + ")" : "")
                    + " / "
                    ;
                //dsc = i.CAT + " #" + i.AFFECTED_COMN_CNFG_ID + " " + i.AFFECTED_COMN_CNFG_NM + " / ";

                delete e.info; // don't need this anymore...
                items.length = 0;
                for (var ft in e) {
                    var total = e[ft];
                    items.push(total + " " + ft + (total != 1 ? "s" : ""));
                }
                dsc += items.join(", ");

                rv.push("<li>" + dsc + "</li>");
            };


            return rv.join("");
        }
        ComnCnfg.prototype.generateReport = function (fname, sheetName, headers, cols, list) {
            if (headers.length != cols.length) {
                self.showMessage("Cannot generate export for " + fname + "; header/column mismtach!", "Export Problem");
                return;
            }

            self.showWheelOfPatience(true);

            var excel = $JExcel.new("Calibri light 10 #333333");

            // groom sheet name
            sheetName = sheetName.replace(/[\[\]\\\/\*:\?]/g, "").substring(0, 30);
            excel.set({ sheet: 0, value: sheetName });
            var formatHeader = excel.addStyle({
                border: "none,none,none,thin #333333",
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < headers.length; i++) {
                excel.set(0, i, 0, headers[i], formatHeader);
                excel.set(0, i, undefined, "auto");
            }

            var len = list.length;
            for (var i = 0; i < len; i++) {
                rec = list[i];
                for (var c = 0; c < cols.length; c++) {
                    col = cols[c];
                    excel.set(0, c, i + 1, (rec[col] || ""));
                }
            }

            fname += (fname.toLowerCase().indexOf(".xlsx") === -1 ? ".xlsx" : "");

            // groom filename, remove any illegal characters...
            fname = fname.replace(/[<>:"\/\\\|\?\*]/g, "");
            excel.generate(fname);

            self.showWheelOfPatience(false);
        }
        ComnCnfg.prototype.onExportComnCnfg = function () {
            var cc = self.currCommonConfig();
            var display = function (list) {
                self.showWheelOfPatience(false);
                if (!list || list.length === 0) {
                    self.showMessage("Failed to generate report; no data received from server", "Export Common Config");
                    return;
                }
                var meta = list.shift();
                var headers = meta.HDRS.split(',');
                var cols = meta.COLS.split(',');

                var tbl = [];
                list.forEach(function (e) {
                    var rec = {};
                    cols.forEach(function (c) { rec[c] = e[c]; });
                    tbl.push(rec);
                });

                var fname = cc.COMN_CNFG_NM + "-" + cc.COMN_CNFG_ID + ".xlsx";
                try {
                    self.generateReport(fname, cc.COMN_CNFG_NM, headers, cols, list);
                }
                catch (err) {
                    self.showMessage(err, "Export Common Config");
                }
                self.showWheelOfPatience(false);
            }
            var fatal = function (err) {
                self.showWheelOfPatience(false);
                self.showMessage(err);
            }
            self.showWheelOfPatience(true);
            http.get('api/comncnfg/export/' + cc.COMN_CNFG_ID).then(display, fatal);
        }
        ComnCnfg.prototype.onExportComnCnfgSearchResults = function () {
            var list = self.ccSearchResults();
            if (list.length === 0) {
                self.showMessage("There are no items in the list", "Export Search Results");
                return;
            }

            var cols = [];
            var headers = [];

            var $ui = $("#PANEL_CC_SEARCH_RESULTS");
            // collect the column headers...
            $ui.find("thead th:gt(0)").each(function (ndx, hdr) {
                headers.push($(hdr).text().toUpperCase());
            });

            // collect the cell attribute (ie: db col names)
            $ui.find("tbody tr:first td:gt(0) span").each(function (ndx, col) {
                cols.push($(col).attr('data-bind').split('.')[1]);
            });

            if (headers.length != cols.length) {
                self.showMessage("Cannot generate export; header/column mismtach!", "Export Problem");
                return;
            }

            var fname = "cdmms_config_search_results.xlsx";
            self.generateReport(fname, "Search Results", headers, cols, list);
        }
        ComnCnfg.prototype.onSelectCommonConfigEntry = function (rec) {
            var select = function () {
                self.displayConfigEntry(rec);
                self.fetchChildItems();
                self.shouldDisplaySaveButtons(true);
                //self.ccSearchResults([]);

                var $form = $("#EditCommonConfigDialog");
                self.disableIndicators($form.find("#edtDEF_PRPGT_IND,#edtDEF_CMPLT_IND"), rec.UPDT_IN_PRGS_IND === "Y");

                self.scrollTo("PANEL_EDIT_COMMON_CONFIG");
            }
            self.checkForPendingChanges(select);
        }
        ComnCnfg.prototype.onNewCommonConfig = function () {
            self.onClearSearchForCommonConfig();

            var cnfg = { COMN_CNFG_ID: 0, TMPLT_TYP_ID: 0, COMN_CNFG_NM: "", TMPLT_NM: "", }
            self.displayConfigEntry(cnfg);
            self.isNewConfig(true);
            self.isEditing(true);
            self.showConfigChildItems(false);
            self.shouldDisplaySaveButtons(true);
        }
        ComnCnfg.prototype.onSaveCancel = function () {
            if (self.pendingChanges === false) {
                self.reset();
                return;
            }

            self.confirm("Click OK to cancel any changes and return to search form?", "Continue?", ["OK", "Cancel"], function (choice) {
                if (choice === "CANCEL") {
                    return;
                }
                self.pendingChanges = false;
                console.log("onSaveCancel - pendingChanges %s", self.pendingChanges);
                self.reset();
            });
        }
        ComnCnfg.prototype.onUpdateRackMountPos = function (rec, event) {
            var $e = $(event.target);
            $e.css("background-color", "white"); // clear any errors...

            var pos = $e.val();
            if (pos.length === 0) {
                return;
            }

            var isWholeNum = new RegExp(/^\d+$/);
            if (isNaN(pos) || pos < 0 || pos > 200 || isWholeNum.test(pos) === false) {
                $("#rec" + rec._guid).find(".RACK_POS").css("background-color", "salmon");
                return self.showMessage(pos + " is not a valid rack mount position; must be a whole number between 0 and 200.", "Invalid Rack Mount Position: " + pos);
            }

            self.hiliteOverlappingRackPositions();
        }
        ComnCnfg.prototype.onUpdateLabelHint = function (rec, event) {
            self.pendingChanges = true;
            console.log("onUpdateLabelHint - pendingChanges %s", self.pendingChanges);
            var $i = $(event.target);
            var txt = $i.val().toUpperCase();
            $i.attr('title', txt).val(txt);
        }
        ComnCnfg.prototype.onUpdateItemQuantity = function (rec, event) {

            var $e = $(event.target);
            var $p = $e.parents("tr:first");
            var $q = $p.find(".qty");
            var $s = $p.find(".spr-qty");
            var $t = $p.find(".total-qty");

            var qOrig = parseInt($q.data("val"));
            var sOrig = parseInt($s.data("val"));


            var qty = 0, spr = 0, total = 0;
            try {
                qty = parseInt(rec.CNTND_IN_MTRL_QTY);
                spr = parseInt(rec.CNTND_IN_MTRL_SPR_QTY);

                if (self.isNewConfig() && qOrig != qty) {
                    qty = qOrig;
                    throw { message: "Please save this Config before updating the quantity." };
                }

                if (isNaN(qty) || qty < 0) {
                    throw { message: "Quantity must be greater than zero." };
                }
                if (isNaN(spr) || spr < 0) {
                    throw { message: "Spare Quantity must be zero or greater." };
                }

                if (qty > 1) {
                    self.pendingChanges = true;
                    console.log("onUpdateItemQuantity - pendingChanges %s", self.pendingChanges);

                    var addNewLines = ($p.hasClass("Bay") || $p.hasClass("Shelf") || $p.hasClass("Node") || $p.hasClass("Card") || $p.hasClass("CommonConfig") || $p.hasClass("HighLevelPart"));
                    if (addNewLines) {
                        rec.CNTND_IN_MTRL_QTY = 1;

                        // steps: 
                        // 1) get a list of all expanded rows, save the guids...
                        // 2) close all the expanded rows
                        // 3) get a copy of the current 'comnCnfgItemList'
                        // 4) remove all the child items from the copied list, we have to reorder the parents
                        // 5) reorder the parents
                        // 6) add back in all the children
                        // 7) refresh the 'comnCnfgItemList'
                        // 8) reopen all the expanded rows...
                        // 9) :)

                        var listOfParentRowsToReOpenLater = self.getExpandedRowsParentGuidList();
                        self.closeAllExpandedRows();
                        var list = self.comnCnfgItemList().createNewArray();
                        var children = {};
                        list.forEach(function (e) {
                            if (!e._isChild)
                                return;
                            var guid = e._parentGUID;
                            var subList = (children[guid] || []);
                            subList.push(e);
                            children[guid] = subList;
                        });
                        var tmp = list.filter(function (e) { return !e._isChild; });
                        list.length = 0; // clear...
                        list = tmp; // parents only...

                        var diff = qty - 1;


                        var ndx = list.findIndex(function (i) { return i.CNTND_IN_SEQ_NO == rec.CNTND_IN_SEQ_NO; });
                        var currSeqNo = parseInt(rec.CNTND_IN_SEQ_NO);
                        for (var i = 0; i < diff; i++) {
                            var entry = {};
                            $.extend(entry, rec);

                            entry._isNew = true;
                            entry._guid = createGUID();

                            currSeqNo++;
                            entry.CNTND_IN_SEQ_NO = currSeqNo;

                            entry.COMN_CNFG_DEF_ID = -1;
                            entry.CNTND_IN_COMN_CNFG_DEF_ID = -1;

                            entry.CNTND_IN_MTRL_QTY = 1;
                            entry.CNTND_IN_MTRL_SPR_QTY = 0;
                            entry.CNTND_IN_MTRL_TOTAL_QTY = 1;

                            // locatable info...
                            entry.PRNT_COMN_CNFG_DEF_ID = rec.PRNT_COMN_CNFG_DEF_ID;
                            entry.PRNT_CI_HLP_MTRL_REVSN_DEF_ID = rec.PRNT_CI_HLP_MTRL_REVSN_DEF_ID;
                            entry.PRNT_CI_COMN_CNFG_DEF_ID = rec.PRNT_CI_COMN_CNFG_DEF_ID;

                            entry.CNTND_IN_PRNT_COMN_CNFG_ID = -1;

                            entry.X_COORD_NO = rec.X_COORD_NO; // clone as per Jesse...
                            entry.Y_COORD_NO = "0";
                            entry.LABEL_NM = "";
                            entry.RACK_POS = "";
                            entry.SPECN_NM = "";
                            entry.FRNT_RER_IND = "";
                            entry.CNTND_IN_REVSN_LVL_IND = "N";

                            ndx++;
                            list.splice(ndx, 0, entry);

                            self.audit.added.push(entry);
                        }

                        // reorder list for everyone after the add...
                        for (var i = ndx + 1; i < list.length; i++) {
                            // update CNTND_IN_SEQ_NO for all the other items...
                            if (currSeqNo == list[i].CNTND_IN_SEQ_NO) {
                                currSeqNo++;
                                list[i].CNTND_IN_SEQ_NO = currSeqNo; //(i + 1);
                            }
                        }

                        for (var guid in children) {
                            var subList = children[guid];
                            var ndx = list.findIndex(function (e) { return e._guid === guid; });
                            if (ndx != -1) {
                                list.insertArrayAt(ndx + 1, subList);
                            }
                        }

                        self.refreshCommonConfigItemList(list);
                        self.reExpandRows(listOfParentRowsToReOpenLater);

                        qty = 1;
                    }
                }
                total = qty + spr;

                rec.CNTND_IN_MTRL_TOTAL_QTY = total;
            }
            catch (err) {
                // reset....
                rec.CNTND_IN_MTRL_QTY = (qty = qOrig); // yes, setting both at the same time.
                rec.CNTND_IN_MTRL_SPR_QTY = (spr = sOrig);

                total = qty + spr;

                self.showMessage(err.message);
            }

            // make sure the values are reflected becuase if they put in 2.5 update it to show 2
            $q.val(qty);
            $s.val(spr);
            $t.text(total);
        }
        ComnCnfg.prototype.onClearEditCommonConfigDialog = function () {
            var $ui = $("#EditCommonConfigDialog");
            $ui.find("input").val('');
            $ui.find("select :nth-child(1)").prop('selected', true);
            $ui.find("input[type=checkbox]").prop('checked', false);
        }
        ComnCnfg.prototype.onToggleSelectChildCommonConfigItem = function (rec, event, isChecked) {
            SelectConfigItemDialog.OnToggleSelectChildCommonConfigItem(rec, event, isChecked);
            return true;
        }
        ComnCnfg.prototype.onAddChildItems = function (cat, list) {
            if (list.length === 0) {
                return;
            }

            if (cat === "CONFIGS") {
                self.addCommonConfigChildItems(list);
                return;
            }

            self.addPartNumberChildItems(list);
        }

        ComnCnfg.prototype.coalesce = function (items) {
            var len = arguments.length;
            for (var i = 0; i < len; i++) {
                var e = arguments[i];
                if (e !== null && e !== undefined) {
                    if (typeof e === "string" && e != "") {
                        return e;
                    }
                    else if (typeof e === "number" && isNaN(e) === false) {
                        return e;
                    }
                    else if (typeof e === "object") {
                        return e;
                    }
                }
            }
            return "";
        }
        // #endregion
        return ComnCnfg;
    });
