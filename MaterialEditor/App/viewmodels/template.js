// #region jQuery convenience functions
/*
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
*/
// #endregion jQuery convenience functions



// #region javascript array convenience functions
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
// #endregion javascript array convenience functions

// #region Global helper functions
function createGUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
// #endregion Global help functions


// #region PopUp Dialogs 
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
// #endregion PopUp Dialogs

EMPTY_SET = "{}";

define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system'
    , null // "system" arg
    , 'durandal/app'
    , 'datablescelledit'
    , 'bootstrapJS'
    , 'jquerydatatable'
    , 'jqueryui'
    , '../Utility/referenceDataHelper'
    , './templateViewer'
    , '../Utility/RackUnitLabel'
    ], function (composition, ko, $, http, activator, mapping, system, baseCard, app, datablescelledit, bootstrapJS, jquerydatatable, jqueryui, reference, tmpltViewer, rackUnitLabel) {
    // mwj: app is always undefined... don't know how else to get it to load...
    app = (app || require('durandal/app'));
    // same problem with reference but require won't work either...
    //reference = (reference || require('../Utility/referenceDataHelper'));

    var CAPTION = "Template";
    var ERR_NO_FORM_DATA = "no_data";
    var ERR_NO_RESULTS = "no_results";
    var MSG_NO_RESULTS_FOUND = "No results found.";
    var MSG_PROVIDE_SEARCH_CRITERIA = "Please provide search criteria.";
    var MSG_UNABLE_TO_PROCESS_REQUEST = "Unable to process your request due to an internal error. If problem persists please contact your system administrator.";

    var confirmed = { OK: "OK", CANCEL: "CANCEL" };

    var Split = require(['../../Scripts/split-1.5.11.min'], function(f) { Split = f; });

    var Template = function () {
        selfTmplt = this;

        selfTmplt.usr = require('Utility/user');
        selfTmplt.tmplt = ko.observable({});
        selfTmplt.tmpltOrigin = ko.observable({});
        selfTmplt.baseTmplt = ko.observable({});

        selfTmplt.firstLoaded = true;
        selfTmplt.pendingChanges = false;
        selfTmplt.isSearching = false;
        selfTmplt.containmentRules = [];

        // this will track the original records and also track what records/columns changed...
        selfTmplt.initAudit();

        selfTmplt.typeOptions = {};

        // common stuff
        selfTmplt.shouldDisplaySaveButtons = ko.observable(false);
        selfTmplt.currTmplt = ko.observable(null);

        //selfTmplt.selectedScreen = ko.observable(CC_EXISTING);
        selfTmplt.specTypeOptions = ko.observableArray([]);
        selfTmplt.srchStatusOptions = ko.observableArray(' ,Update In Progress,Propagated,Completed,Deactivated'.split(','));
        selfTmplt.templateItemList = ko.observableArray([]);
        selfTmplt.isNewTmplt = ko.observable(false);
        selfTmplt.isBaseTmplt = ko.observable(false);
        selfTmplt.isOvrlTmplt = ko.observable(false);

        selfTmplt.searchResults = ko.observableArray([]);

        selfTmplt.changeLogItems = ko.observableArray([]);

        selfTmplt.mtrlTypOptions = ko.observableArray([]);
        selfTmplt.featTypOptions = ko.observableArray([]);
        selfTmplt.mtrlStatusOptions = ko.observableArray([]);
        selfTmplt.drpstatus = ko.observableArray(['', 'Completed', 'Delete', 'Propagate', 'Retired', 'Update In-Progress']);

        var angles = [{ text: "0", value: 1},{ text: "-90", value: 2},{ text: "90", value: 3},{ text: "-180", value: 4},{ text: "180", value: 5}];
        selfTmplt.rotationAngleOptions = ko.observableArray(angles);

        selfTmplt.specificationTypes = ko.observableArray([]);
        selfTmplt.TemplateType = ko.observableArray(['', 'Base Template', 'Overall Template']);
        selfTmplt.RoGenerictype = ko.observableArray(['', 'Record Only', 'Generic']);
        //selfTmplt.drpIndicator = ko.observableArray(['', 'Front', 'Rear']);
        //selfTmplt.RotationAngle = ko.observableArray();
        selfTmplt.BayChildChoices = ko.observableArray([]);
        selfTmplt.BayWidth = ko.observable();
        selfTmplt.BayDepth = ko.observable();
        selfTmplt.BayHeight = ko.observable();
        selfTmplt.BayExtenderHeight = ko.observable();
        selfTmplt.BaySelectionButton = ko.observable(false);
        selfTmplt.SpecList = ko.observableArray();
        selfTmplt.HlpMnrMtlList = ko.observableArray();
        selfTmplt.rotationAnglist();
        //selfTmplt.indicator = ko.observable();
        selfTmplt.statusVal = ko.observable();
        selfTmplt.specTypeVal = ko.observable();
        //selfTmplt.RotationAngleId = ko.observable();
        selfTmplt.specTypeVal = 0;
        selfTmplt.tempTypeVal = ko.observable();
        selfTmplt.SpecStatusVal = ko.observable();
        selfTmplt.specificationList();
        selfTmplt.specTypeText = ko.observable();

        selfTmplt.btnEnableBase = ko.observable();
        selfTmplt.btnEnableBase = true;
        selfTmplt.templateViewer = ko.observable();

        selfTmplt.searchSpecType = ko.observable();
        selfTmplt.RoGenTypeVal = ko.observable();
        selfTmplt.searchSpecStatusVal = ko.observable();
        selfTmplt.searchSpecStatusVal = 0;
        selfTmplt.searchSpecType = "";
        selfTmplt.RoGenTypeVal = "";
        selfTmplt.MtrlCatId = ko.observable();
        selfTmplt.FeatTypId = ko.observable();
        selfTmplt.MtrlCatId = 0;
        selfTmplt.FeatTypId = 0;
        selfTmplt.selectedMtlCtgry = ko.observable('');
        selfTmplt.selectedNewMtlCtgry = ko.observable('');
        selfTmplt.mtlCtgryOptions = ko.observableArray([{ "value": "", "text": "" },
        { "value": 1, "text": "HIGH_LEVEL_PART" },
        { "value": 2, "text": "COMON_CONFIG" },
        { "value": 3, "text": "SPEC_PART" }]);

        $("#PANEL_DETAILS").hide();
        selfTmplt.HlpRevId = ko.observable();
        selfTmplt.BayExtndrSpecnRevsnAltId = ko.observable();

        selfTmplt.theDrawingView = null;
        selfTmplt.tmpltItemsSearchResults = ko.observableArray([]);

        var jsonres = {
            resp: { "version": "2.4.6", "objects": [{ "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 2.13, "top": 86.32, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 357.42, "top": 259, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 137.94, "top": 70.37, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 314.55, "top": 42.12, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 270.5, "top": 341.77, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 378.57, "top": 150.04, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 140.71, "top": 225.78, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 26, "top": 344, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 239.75, "top": 430.39, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 551.12, "top": 395.53, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 241.55, "top": 231.07, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 31, "top": 274.89, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 68.08, "top": 27.26, "width": 82.25, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "New Text", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 186, "top": 26, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 166.68, "top": 160.05, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 85, "top": 36, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 12.09, "top": 374.82, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 328.49, "top": -13.92, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 23, "top": 66, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 134, "top": 176, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 210, "top": 316, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 530.72, "top": 109.93, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 555.53, "top": 37.57, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 501.88, "top": 314.46, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 393.17, "top": 142.11, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 444.78, "top": 220.8, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 374.25, "top": 313.98, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 104.72, "top": 264.94, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "line", "version": "2.4.6", "originX": "left", "originY": "top", "left": 563.88, "top": 236.13, "width": 150, "height": 100, "fill": "rgb(0,0,0)", "stroke": "black", "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "x1": -75, "x2": 75, "y1": -50, "y2": 50 }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 508.66, "top": 242.35, "width": 155.66, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "TSLS-82.71030Q", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 427.19, "top": 339.71, "width": 275.66, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "ADCP-FMT-GQL070A00-A72B", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 470.87, "top": 13.17, "width": 140.1, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "TSLS-81.71060", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 141, "top": 148, "width": 141.89, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "MRTP-QW1306", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 70.28, "top": 418.32, "width": 155.66, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "TSLS-82.71030Q", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }, { "type": "i-text", "version": "2.4.6", "originX": "left", "originY": "top", "left": 546.9, "top": 95.13, "width": 154.54, "height": 22.6, "fill": "blue", "stroke": null, "strokeWidth": 1, "strokeDashArray": null, "strokeLineCap": "butt", "strokeDashOffset": 0, "strokeLineJoin": "miter", "strokeMiterLimit": 4, "scaleX": 1, "scaleY": 1, "angle": 0, "flipX": false, "flipY": false, "opacity": 1, "shadow": null, "visible": true, "clipTo": null, "backgroundColor": "yellow", "fillRule": "nonzero", "paintFirst": "fill", "globalCompositeOperation": "source-over", "transformMatrix": null, "skewX": 0, "skewY": 0, "text": "TSLS-82.71020C", "fontSize": 20, "fontWeight": "normal", "fontFamily": "Arial", "fontStyle": "normal", "lineHeight": 1.16, "underline": false, "overline": false, "linethrough": false, "textAlign": "left", "textBackgroundColor": "", "charSpacing": 0, "styles": {} }] },
            specification: true
        }

        selfTmplt.$btnSaveTmplt = null; 

        selfTmplt.attempAutoSearch();
    };

    Template.prototype.attempAutoSearch = function () {
        var showSearchForm = function() { 
            $("#navbarContainer").show();
            $("#navigareRecordHide").show();
            $(".viewContainerPadding").css('padding-top', 150);
            $("#PANEL_MAIN_SCREEN").show();

            $(".btnRESTORE_SEARCH").hide();
        };

        if (window.location.hash.indexOf('?') >= 0 ) {
            var parms = new URLSearchParams(window.location.hash.split('?')[1]);
            selfTmplt.autoSearchId = parms.get('id');
            //selfTmplt.autoSearch = Object.fromEntries(parms.entries()); // IE11 is useless...
            console.log(selfTmplt.autoSearch);
        }
        if (selfTmplt.autoSearchId > 0 ) {
            $("#navbarContainer").hide();
            $("#navigareRecordHide").hide();
            $(".viewContainerPadding").css('padding-top', 10);
            setTimeout(function () {
                $('#idTmpltID').val(selfTmplt.autoSearchId);

                $("#PANEL_MAIN_SCREEN").hide();
                $("#PANEL_SEARCH_RESULTS").hide();
    
                selfTmplt.fetchTemplate(selfTmplt.autoSearchId, function() { 
                });

                selfTmplt.$btnRESTORE_SEARCH = $(".btnRESTORE_SEARCH");
                selfTmplt.$btnRESTORE_SEARCH.unbind('click').click(showSearchForm).show();
            },1000);
        }
    }
    Template.prototype.showMessage = function (msg, title) {
        selfTmplt.showWheelOfPatience(false);
        app.showMessage(msg, (title||"Templates"));
    }
    Template.prototype.showWheelOfPatience = function (show) {

        var $wait = $("#interstitial").css("height", $("body").height());

        show ? $wait.show()
            : $wait.hide()
            ;

        show ? $("body").addClass('wait-cursor')
            : $("body").removeClass('wait-cursor')
            ;

        console.log('Template.showWheelOfPatience() %s', show);
    }
    Template.prototype.checkEnter = function (root, event) {
        if (event.keyCode === 13) {
            console.log("Enter");
            //selfTmplt.searchForMaterial(root);
        }
        else { return true; }
    };

    Template.prototype.onChngNewMtlCtgry = function () {
        selfTmplt.HlpMnrMtlList([]);
        $('#idHlpList').DataTable().clear().destroy();

        //if (selfTmplt.selectedMtlCtgry() < 3) {
        //    var ind = selfTmplt.specificationTypes().map(function (img) { return img.text; }).indexOf(selfTmplt.mtlCtgryOptions()[selfTmplt.selectedMtlCtgry()].text);
        //    if (ind > -1)
        //        selfTmplt.specTypeVal = selfTmplt.specificationTypes()[ind].value;
        //    else
        //        selfTmplt.specTypeVal = 0;
        //}
    };
    Template.prototype.checkForAnyChanges = function () {

        var prev = selfTmplt.tmpltOrigin();
        var curr = selfTmplt.tmplt();

        if (curr.BaseTemplateInd()) {
            curr = selfTmplt.baseTmplt();
        }

        var changed = false;
        for (var field in curr) {
            if (field.indexOf("_") === 0) {
                continue;
            }

            changed = (changed || (curr[field]() != prev[field]()));
            console.log('dbg: checkForAnyChanges() %s => %s | %s %s', field, prev[field](), curr[field](), prev[field]()==curr[field]()?"":"[changed]");
        }
        return changed;

        //return true; // TODO: fix this...

        //if (selfTmplt.tmplt().baseTmplt()) {
        //    if (selfTmplt.tmplt().cmpltd() == selfTmplt.tmpltOrigin().cmpltd()
        //        && selfTmplt.tmplt().prpgtd() == selfTmplt.tmpltOrigin().prpgtd()
        //        && selfTmplt.tmplt().dltd() == selfTmplt.tmpltOrigin().dltd()
        //        && selfTmplt.tmplt().rtrd() == selfTmplt.tmpltOrigin().rtrd()
        //        && selfTmplt.tmplt().updtInPrgss() == selfTmplt.tmpltOrigin().updtInPrgss()
        //        && selfTmplt.tmplt().nm().trim().toUpperCase() == selfTmplt.tmpltOrigin().nm().trim().toUpperCase()
        //        && selfTmplt.tmplt().dsc().trim().toUpperCase() == selfTmplt.tmpltOrigin().dsc().trim().toUpperCase()) {
        //        if (selfTmplt.tmplt().typ().toUpperCase() == "BAY"
        //            && selfTmplt.tmplt().frntRr.value() == selfTmplt.tmpltOrigin().frntRr.value()
        //            && selfTmplt.tmplt().rtnAngl.value() == selfTmplt.tmpltOrigin().rtnAngl.value()) {
        //            selfTmplt.showMessage("There is no modification in the Base Template  fields");
        //            return false;
        //        }
        //        else {
        //            return true;
        //        }
        //        selfTmplt.showMessage("There is no modification in the current fields");
        //        return false;
        //    }
        //    else {
        //        return true;
        //    }
        //}
        //else if (!selfTmplt.tmplt().baseTmplt()) {
        //    if (selfTmplt.tmplt().cmpltd() == selfTmplt.tmpltOrigin().cmpltd()
        //        && selfTmplt.tmplt().prpgtd() == selfTmplt.tmpltOrigin().prpgtd()
        //        && selfTmplt.tmplt().dltd() == selfTmplt.tmpltOrigin().dltd()
        //        && selfTmplt.tmplt().rtrd() == selfTmplt.tmpltOrigin().rtrd()
        //        && selfTmplt.tmplt().updtInPrgss() == selfTmplt.tmpltOrigin().updtInPrgss()
        //        && selfTmplt.tmplt().nm().trim().toUpperCase() == selfTmplt.tmpltOrigin().nm().trim().toUpperCase()
        //        && selfTmplt.tmplt().dsc().trim().toUpperCase() == selfTmplt.tmpltOrigin().dsc().trim().toUpperCase()) {
        //        selfTmplt.showMessage("There is no modification in the Overall Template fields");
        //        return false;
        //    }
        //    else {
        //        return true;
        //    }
        //}
    };

   
    Template.prototype.enableSaveButton = function (flag) {
        $("#divTmpltSave").show();
        selfTmplt.$btnSaveTmplt = (selfTmplt.$btnSaveTmplt||$("#btnSaveTmplt"));

        var disabled = 'disabled';
        flag ? selfTmplt.$btnSaveTmplt.removeProp(disabled).removeAttr(disabled)
             : selfTmplt.$btnSaveTmplt.prop(disabled, true)
        ;
        selfTmplt.$btnSaveTmplt.show();
        console.log('save button %s', (flag ? 'enabled' : 'disabled'));
    }
    Template.prototype.onSaveTemplateData = function () {
        selfTmplt.enableSaveButton(false);

        if ( selfTmplt.checkForAnyChanges() === false ) {
            //document.getElementById("btnSaveTemp").disabled = true;
            selfTmplt.showMessage("You have not made any modifications to the template attributes!");
            return 0;
        }

        selfTmplt.showWheelOfPatience(true);

        var tmpltJSON = mapping.toJSON(selfTmplt.tmplt);

        if (selfTmplt.tmplt().BaseTemplateInd()) {
            tmpltJSON = mapping.toJSON(selfTmplt.baseTmplt);
        }
        
        var tmpltObj = JSON.parse(tmpltJSON);

        tmpltObj.CUID = selfTmplt.usr.cuid;
        tmpltObj.MtrlCatId = selfTmplt.MtrlCatId;
        tmpltObj.FeatTypId = selfTmplt.FeatTypId;
        tmpltObj.TemplateType = tmpltObj.TemplateType.toUpperCase();

        if (tmpltObj.TemplateType == "BAY" && tmpltObj.TemplateType.BaseTemplateInd === false) {
            tmpltObj.BayExtndrSpecnRevsnAltId = selfTmplt.BayExtndrSpecnRevsnAltId; // mwj: I don't understand why this is here yet...
            //tmpltObj.RtnAnglId = selfTmplt.tmplt().bsTmplt.rtnAngl.value(); // mwj: we shouldn't need this since we have RotationAngleID
        }

        // 
        // TODO: save canvas/fabricjs data
        // NOTE: doing this in the templateViewer.js
        // 
        //if (selfTmplt.templateViewer && selfTmplt.templateViewer() !== undefined && selfTmplt.templateViewer().canvas) {
        //    *** this isn't the right way to do this... ***
        //    tmpltObj.canvas = selfTmplt.templateViewer().canvas
        //}
        

        tmpltJSON = JSON.stringify(tmpltObj);

        $.ajax({
            type: "POST",
            url: 'api/tmplt/update',
            data: tmpltJSON,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: updateSuccess,
            error: updateError,
            context: selfTmplt
        });

        function updateSuccess(result) {
            selfTmplt.enableSaveButton(true);  //  wonder if we should actually disable this until another change is detected...

            var reval = JSON.parse(result);
            selfTmplt.showMessage(reval.Status, "Template Updated");

            //
            // i dont think we want to hide everything as they might be making a series of changes...
            //
            // CHECK HERE:
            /*
            $("#PANEL_DETAILS").hide();
            $("#overAllTmpltDtls").hide();
            $("#divTmpltSave").hide();
            $("#PANEL_GRAPHICAL_VIEW").hide();

            
            //document.getElementById("btnSaveTemp").disabled = true;
            
            $('#idTmpltID').val(selfTmplt.tmplt().tmpltId());
            selfTmplt.showWheelOfPatience(true);
            selfTmplt.onSearchTemplate();
            //if (selfTmplt.searchResults().length > 0) {
            //    setTimeout(function () {
            //        var item = {
            //            MTL_CAT_ID: selfTmplt.searchResults()[0].MTL_CAT_ID,
            //            FEAT_TYP_ID: selfTmplt.searchResults()[0].FEAT_TYP_ID,
            //            TMPLT_ID: selfTmplt.searchResults()[0].TMPLT_ID,
            //            TMPLT_TYP: selfTmplt.searchResults()[0].TMPLT_TYP,
            //            TMPLT_CAT: selfTmplt.searchResults()[0].TMPLT_CAT
            //        };

            //        selfTmplt.onSelectTemplate(item);

            //        $("#PANEL_DETAILS").show();
            //        $("#overAllTmpltDtls").show();
            //        $("#divTmpltSave").show();
            //        $("#PANEL_GRAPHICAL_VIEW").show();
            //        $("#interstitial").hide();
            //    }, 1000);
            //}
            */
        }

        function updateError() {
            selfTmplt.showWheelOfPatience(false);
            alert("Internal Server error");
        }
    };

    Template.prototype.onChngStatus = function () {
        selfTmplt.searchResults([]);
    };

    Template.prototype.onChngTmpltTyp = function (val) {
        //
        // mwj: I don't think this is working right, if I select Bay then specTypeText is set to Card....
        //
        //if (selfTmplt.specTypeVal != "") {
        //    gCount = selfTmplt.specTypeVal;
        //    gCount++;
        //
        //    console.log("Before " + selfTmplt.specTypeVal + "   After" + gCount);
        //    selfTmplt.specTypeText = selfTmplt.specificationTypes()[gCount].text;
        //    
        //}
        //else {
        //    selfTmplt.specTypeText = "";
        //}

        //
        // mwj: rewrite....
        //
        var $c = $("#idTmpltType").find('option:selected');
        selfTmplt.specTypeText = $c.text();
        console.log('onChngTmpltType: %s %s', selfTmplt.specTypeVal, selfTmplt.specTypeText);

        selfTmplt.searchResults([]);
        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        $("#divTmpltSave").hide();
    };

    Template.prototype.onChngTmpltTyp = function () {
        selfTmplt.searchResults([]);
    };

    Template.prototype.onChngStatusNewtmplt = function () {
        //console.log(item);
    };

    Template.prototype.onChngNewTmpSpecTyp = function () {
        //console.log(item);
        selfTmplt.SpecList([]);
    };
    Template.prototype.onChngNewTmpTyp = function () {
        //console.log(item);
    };

    Template.prototype.specificationList = function () {
        $.ajax({
            type: "GET",
            url: 'api/reference/SpecType',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc,
            context: selfTmplt
        });

        function successFunc(data, status) {

            if (data == 'no_results') {
                selfTmplt.showWheelOfPatience(false);
                //selfTmplt.showMessage("No records found", 'Failed');
            }
            else {
                selfTmplt.specificationTypes(JSON.parse(data));
            }
        }

        function errorFunc() {
            selfTmplt.showWheelOfPatience(false);
        }
    };

    // This method written for binding the rotation angle details into dropdown.
    Template.prototype.rotationAnglist = function () {
        $.ajax({
            type: "GET",
            url: 'api/reference/RotationAng',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc,
            context: selfTmplt
        });
        function successFunc(data, status) {

            if (data == 'no_results') {
                selfTmplt.showWheelOfPatience(false);
                //selfTmplt.showMessage("No records found", 'Failed');
            }
            else {
                //selfTmplt.RotationAngle(JSON.parse(data));
            }
        }
        function errorFunc() {
            selfTmplt.showWheelOfPatience(false);
        }
    };
    //end of binding rotation angle details

    Template.prototype.onSearchTemplate = function () {
        selfTmplt.searchResults([]);
        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        $("#divTmpltSave").hide();
        $("#PANEL_GRAPHICAL_VIEW").hide();

        selfTmplt.showWheelOfPatience(true);
        $("#tmpltViewerArea").hide();
        var CmpltInd = '';
        var PrpgtInd = '';
        var RetTmpltInd = '';
        var UpdtInPrgsInd = '';
        var DelInd = '';
        var highLevelP = '';

        var specTypes = selfTmplt.specificationTypes().map(function (img) { return img.value; })
        var key = specTypes.indexOf(selfTmplt.specTypeVal);
        var valhigh = selfTmplt.specificationTypes()[key].text;

        if (valhigh.indexOf("High") > -1) {
            highLevelP = 'Y';
        }

        switch (selfTmplt.statusVal()) {
            case 'Completed':
                CmpltInd = 'Y';
                DelInd = '';
                PrpgtInd = '';
                RetTmpltInd = '';
                UpdtInPrgsInd = '';
                break;
            case 'Delete':
                CmpltInd = '';
                DelInd = 'Y';
                PrpgtInd = '';
                RetTmpltInd = '';
                UpdtInPrgsInd = '';
                break;
            case 'Propagate':
                CmpltInd = '';
                DelInd = '';
                PrpgtInd = 'Y';
                RetTmpltInd = '';
                UpdtInPrgsInd = '';
                break;
            case 'Retired':
                CmpltInd = '';
                DelInd = '';
                PrpgtInd = '';
                RetTmpltInd = 'Y';
                UpdtInPrgsInd = '';
                break;
            case 'Update In-Progress':
                CmpltInd = '';
                DelInd = '';
                PrpgtInd = '';
                RetTmpltInd = '';
                UpdtInPrgsInd = 'Y';
                break;
            default:
                CmpltInd = '';
                DelInd = '';
                PrpgtInd = '';
                RetTmpltInd = '';
                UpdtInPrgsInd = '';
        }

        var BaseTmpltInd;

        switch (selfTmplt.tempTypeVal()) {
            case 'Base Template':
                BaseTmpltInd = 'Y'
                break;
            case 'Overall Template':
                BaseTmpltInd = 'N'
                break;
            default:
                BaseTmpltInd = '';
        }

        var payload = {
            pTmpltNm: $('#idTmpName').val(),
            pTmpltId: $('#idTmpltID').val(),
            pTmpltDsc: $('#idTmpDesc').val(),
            pBaseTmpltInd: BaseTmpltInd,
            pHlpTmpltInd: highLevelP,
            pComnCnfgTmpltInd: '',
            pCmpltInd: CmpltInd,
            pPrpgtInd: PrpgtInd,
            pUpdtInPrgsInd: UpdtInPrgsInd,
            pRetTmpltInd: RetTmpltInd,
            pDelInd: DelInd,
            pTmpltTypId: selfTmplt.specTypeVal
        };
        selfTmplt.searchTemplateDetails(payload);
        selfTmplt.scrollToResults();
    };

    Template.prototype.scrollToResults = function () {
        setTimeout(function () {
            document.getElementById('PANEL_SEARCH_RESULTS').scrollIntoView();
            selfTmplt.showWheelOfPatience(false);
        }, 1000);
    };

    Template.prototype.searchTemplateDetails = function (payloadRes) {
        $.ajax({
            type: "GET",
            url: 'api/tmplt/search/all',
            data: payloadRes,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: searchSuccess,
            error: searchError,
            context: selfTmplt
        });

        function searchSuccess(response) {
            selfTmplt.searchResults(response);
            if (selfTmplt.searchResults().length == 0) {
                selfTmplt.showMessage("No data found", "Template");
            }
            selfTmplt.BayChildChoices([]);
            selfTmplt.showWheelOfPatience(false);

        }

        function searchError() {
            selfTmplt.showWheelOfPatience(false);
            alert("Internal Server error");
        }

    };

    Template.prototype.initAudit = function (rec) {
        // tmplt is the original template record
        // items is the original items list...
        selfTmplt.audit = { tmplt: {}, items: [], deleted: [], added: [], log: [] };
        $.extend(selfTmplt.audit.tmplt, rec);

        // *** NOTE: changes will be determined at the time of save()
    }

    Template.prototype.onNewTemplate = function () {

        // do create new template UI
        var span = document.getElementById('idNewTemplateclose');
        var modal = document.getElementById('divCreateNewTmplt');
        selfTmplt.onClearSearch();
        span.onclick = function () {
            modal.style.display = "none";
            selfTmplt.SpecList([]);
            selfTmplt.HlpMnrMtlList([]);
            selfTmplt.resetSpecSearch();
        }
        selfTmplt.searchResults([]);
        $("#divCreateBasetmplt").hide();
        $("#divCreateOveralltmplt").hide();
        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        selfTmplt.resetSpecSearch();
        $("#divCreateNewTmplt").show();
        selfTmplt.onClearSearchForm();
    };

    Template.prototype.resetSpecSearch = function () {
        $("#idSpecID").val("");
        $("#idSpecName").val("");
        $("#idSpecDesc").val("");
        selfTmplt.searchSpecStatusVal = "";
        selfTmplt.searchSpecType = "";
        selfTmplt.RoGenTypeVal = "";
        $("#idTmpModel").val("");
        selfTmplt.tmplt({});
        //selfTmplt.selectedMtlCtgry('');
    };

    Template.prototype.onCancelBaseTmp = function () {
        $("#divCreateBasetmplt").hide();
        $("#idBaseTmpNm").val("");
        $("#idBaseTmpDsc").val("");
    };

    Template.prototype.onCancelOverallTmp = function () {
        $("#divCreateOveralltmplt").hide();
        $("#idOverallTmpNm").val("");
        $("#idOverallTmpDsc").val("");
    };

    Template.prototype.onEditBaseTmp = function () {
        $("#divCreateOveralltmplt").hide();
        $("#idOverallTmpNm").val("");
        $("#idOverallTmpDsc").val("");

        selfTmplt.showWheelOfPatience(true);

        //var searchJSON = {
        //	pTmpltId: $("#hiddenCreateOverAllBaseTmpltId").text()
        //};

        //$.ajax({
        //	type: "GET",
        //	url: 'api/tmplt/getBaseOverallTmpltDtls',
        //	data: searchJSON,
        //	contentType: "application/json; charset=utf-8",
        //	dataType: "json",
        //	success: TmpSearchSuccess,
        //	error: TmpSearchError,
        //	context: selfTmplt
        //});

        //function TmpSearchSuccess(result) {
        //	$("#interstitial").hide();
        //	document.getElementById("idBaseTmpNm").value = result[0].TMPLT_NM;
        //	document.getElementById("idBaseTmpDsc").value = result[0].TMPLT_DSC;   
        //	document.getElementById("idBaseTmpNm").disabled = true;
        //}

        //function TmpSearchError() {
        //	$("#interstitial").hide();
        //	alert("Internal Server error");
        //}

        //$("#divCreateBasetmplt").show();

        var payload = {
            pTmpltNm: "",
            pTmpltId: $("#hiddenCreateOverAllBaseTmpltId").text(),
            pTmpltDsc: "",
            pBaseTmpltInd: "",
            pHlpTmpltInd: "",
            pComnCnfgTmpltInd: "",
            pCmpltInd: "",
            pPrpgtInd: "",
            pUpdtInPrgsInd: "",
            pRetTmpltInd: "",
            pDelInd: "",
            pTmpltTypId: ""
        };
        $("#divCreateBasetmplt").hide();
        $("#divCreateNewTmplt").hide();
        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        $("#divTmpltSave").hide();
        selfTmplt.resetSpecSearch();
        selfTmplt.onCancelOverallTmp();
        selfTmplt.onCancelBaseTmp();
        selfTmplt.searchTemplateDetails(payload);

    };

    Template.prototype.onChngBaseTmpltName = function () {
        selfTmplt.checkForUniqueTemplateName("Base", $("#idBaseTmpNm").val());
    };

    Template.prototype.onChngOverallTmpltName = function () {
        selfTmplt.checkForUniqueTemplateName("Overall", $("#idOverallTmpNm").val());
    };

    Template.prototype.checkForUniqueTemplateName = function (type, name) {
        if (name.length === 0) {
            $("#idBaseTmpNm").css("background-color", "salmon");
            return;
        }

        $("#idBaseTmpNm").css("background-color", "white");
        function NameSearchSuccess(result) {
            selfTmplt.showWheelOfPatience(false);
            if (result.indexOf("already exists") > 0) {
                $("#idBaseTmpNm").val("");
                if (type == "Base") {
                    document.getElementById("idCreateBaseTmp").disabled = true;
                }
                if (type == "Overall") {
                    document.getElementById("idCreateOverallTmp").disabled = false;
                }
                $("#idBaseTmpNm").css("background-color", "salmon");
                selfTmplt.showMessage(result, "Template");
            }
            else {
                document.getElementById("idCreateBaseTmp").disabled = false;
                document.getElementById("idCreateOverallTmp").disabled = false;
            }
        }

        function NameSearchError() {
            selfTmplt.showWheelOfPatience(false);
            alert("Internal Server error");
        }

        var searchJSON = {
            pTmpNm: name.toUpperCase()
        };

        $.ajax({
            type: "GET",
            url: 'api/tmplt/findTmpltByName',
            data: searchJSON,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: NameSearchSuccess,
            error: NameSearchError,
            context: selfTmplt
        });
    };

    Template.prototype.activateTemplateViewer = function () {
        if (!selfTmplt.tmplt) {
            return;
        }
        
        selfTmplt.showWheelOfPatience(true);

        var $viewer = $("#tmpltViewerArea");

        function display(data) {
            if (!data || !data.curr) {
                selfTmplt.showMessage("No Records Found for this template id", "Template Viewer");
                return;
            }

            //var obj = (typeof response === 'string' ? JSON.parse(response) : response);
            selfTmplt.showWheelOfPatience(true);

            var loaded = function (status) {
                selfTmplt.showWheelOfPatience(false);
                if (status.error) {
                    selfTmplt.showMessage(status.error);
                    return;
                }

                $viewer.show();
                if ($('.tabs').hasClass('ui-tabs') === false) {
                    $(".tabs").tabs();
                }
                if( $(".gutter ").length === 0 ) { 
                    Split(["#PANEL_GRAPHICAL_VIEW", "#PANEL_TABULAR_VIEW"], { sizes: [ 33, 67 ], gutterSize: 6 });
                }
            }
            if (selfTmplt.theDrawingView === null) {
                selfTmplt.theDrawingView = new tmpltViewer(data, loaded);
                selfTmplt.templateViewer(selfTmplt.theDrawingView);
            }
            else {
                selfTmplt.templateViewer().refreshDrawing(data, loaded);
            }

            selfTmplt.templateViewer().setParent(selfTmplt);

            $("#PANEL_GRAPHICAL_VIEW").show();
        }

        function error() {
            alert("Internal Server error");
        }


        var bay = null;
        var route = 'api/specn/284/'+selfTmplt.tmplt().TemplateType().toUpperCase();
        http.get(route)
            .then(function (response) { bay = JSON.parse(response); })
            .then(function() { 
        
            $viewer.hide();

            var tmpltId = selfTmplt.tmplt().TemplateID();
            var tmpltTyp = selfTmplt.tmplt().TemplateType().toUpperCase();

            var data = { curr: selfTmplt.tmplt, orig: selfTmplt.tmpltOrigin, bay: bay };

            display(data);
        });

        return;
        //
        // TODO: skip this for now, no sense in repulling the basic template infor again
        // this route should fetch only items for drawing, not a deep search for items themselves...
        //
        //var route = 'api/tmplt/drawing/' + tmpltId + '/' + tmpltTyp;

        //var payload = {
        //    baseTmplt: selfTmplt.tmplt().baseTmplt(),
        //    forEdit: false
        //};


        //selfTmplt.showWheelOfPatience(true);
        //$.ajax({
        //    type: "GET",
        //    url: route,
        //    data: payload,
        //    contentType: "application/json; charset=utf-8",
        //    dataType: "json",
        //    context: selfTmplt
        //})
        //    .done(display)
        //    .fail(error)
        //    .complete(function () {
        //        selfTmplt.showWheelOfPatience(false);
        //    })
        //    ;

        
    };

    Template.prototype.getAddEquipDisplayResultHandle = function () {
        // ugg, don't know a good way to do this...
        return this.tmpltItemsSearchResults;
    }
    Template.prototype.activateTemplateViewerORIG = function () {
        if (selfTmplt.tmplt && selfTmplt.tmplt().baseTmplt) {
            selfTmplt.showWheelOfPatience(true);

            var payload = {
                baseTmplt: selfTmplt.tmplt().baseTmplt(),
                forEdit: false
            };
            var tmpltId = selfTmplt.tmplt().tmpltId();
            var tmpltTyp = selfTmplt.tmplt().typ();

            var route = 'api/tmplt/drawing/' + tmpltId + '/' + tmpltTyp;

            $.ajax({
                type: "GET",
                url: route,
                data: payload,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: activateSuccess,
                error: activateError,
                context: selfTmplt
            });

            function activateSuccess(response) {
                if (response !== EMPTY_SET) {
                    var obj = JSON.parse(response);

                    if (obj)
                        $("#tmpltViewerArea").show();
                    selfTmplt.templateViewer(new tmpltViewer(obj));
                }
                else {
                    selfTmplt.showMessage("No Records Found for this template id", "Template Viewer");
                }
                selfTmplt.showWheelOfPatience(false);
            }

            function activateError() {
                selfTmplt.showWheelOfPatience(false);
                alert("Internal Server error");
            }
        }
    };
    Template.prototype.onChangeBaseTmpltDetails = function () {
        selfTmplt.enableSaveButton(true);
    };

    Template.prototype.onChangeOverAllTmpltDetails = function () {
        selfTmplt.enableSaveButton(true);
    };

    Template.prototype.onClearSearch = function () {
        $("#idSpecID").val("");
        $("#idSpecID").val("");
        $("#idSpecName").val("");
        $("#idSpecDesc").val("");
        $("#idTmpModel").val("");
        selfTmplt.searchSpecType = "";
        selfTmplt.RoGenTypeVal = "";
        selfTmplt.searchSpecStatusVal = "";
        selfTmplt.SpecList([]);
        selfTmplt.HlpMnrMtlList([]);
        selfTmplt.tmplt({});
        selfTmplt.selectedNewMtlCtgry('');

        selfTmplt.templateViewer().clear();
        selfTmplt.templateViewer().hide();
    };

    Template.prototype.onClearSearchForm = function () {
        $("#idTmpltID").val("");
        $("#idTmpName").val("");
        $("#idTmpDesc").val("");
        selfTmplt.statusVal("");
        selfTmplt.specTypeVal = 0;
        selfTmplt.tempTypeVal("");
        selfTmplt.specTypeText = "";
        selfTmplt.searchResults([]);
        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        $("#divTmpltSave").hide();
        selfTmplt.SpecList([]);
        selfTmplt.HlpMnrMtlList([]);
        selfTmplt.tmplt({});

        selfTmplt.templateViewer().clear();
        selfTmplt.templateViewer().hide();
    };

    Template.prototype.onChngIndicator = function (item) {
    };

    Template.prototype.onChngBayChoice = function (obj, event) {
        var featTypId = event.target.value;  // based on the value, populate the child choices
        var tmpltId = selfTmplt.tmplt().tmpltId();
        if (featTypId === '1') {
            if (selfTmplt.tmpltOrigin().BayExtSpecRevAltId() && typeof selfTmplt.tmpltOrigin().BayExtSpecRevAltId() !== 'undefined') {
                var bayExtSpecRevAltId = selfTmplt.tmpltOrigin().BayExtSpecRevAltId();
            }
            if (selfTmplt.tmplt().BayExtSpecRevAltId() && typeof selfTmplt.tmplt().BayExtSpecRevAltId() !== 'undefined') {
                let tempbayExtSpecRevAltId = selfTmplt.tmplt().BayExtSpecRevAltId();
                if (tempbayExtSpecRevAltId !== '0') {
                    var bayExtSpecRevAltId = selfTmplt.tmplt().BayExtSpecRevAltId();
                }
            }
        }
        var json = '';
        if (featTypId !== "0") {
            json = { feattypid: featTypId, templateid: tmpltId };
            $.ajax({
                type: "GET",
                url: 'api/tmplt/getBayChoices',
                data: json,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getBayChoiceSuccess,
                error: getBayChoiceError,
                context: selfTmplt,
                async: true
            });
            function getBayChoiceSuccess(data) {
                var results = JSON.parse(data);
                var obj = [];
                for (let i = 0; i < results.length; i++) {
                    var fields = results[i].split('~');
                    var tempobj = { 'value': fields[0], 'text': fields[1] };
                    obj.push(tempobj);
                }
                selfTmplt.BayChildChoices(obj);
                if (featTypId === '1') {
                    selfTmplt.tmplt().BayExtSpecRevAltId(bayExtSpecRevAltId);
                    selfTmplt.BaySelectionButton(false);
                }
                else {
                    selfTmplt.BaySelectionButton(true);
                }
            }
            function getBayChoiceError(response) {
                selfTmplt.showMessage("Error attempting to find suitable Bay Choices.");
            }
        }
        else {
            selfTmplt.BayChildChoices([]);
            selfTmplt.BaySelectionButton(false);
        }
    };

    Template.prototype.onChngChoice = function (obj, event) {
        if (event.originalEvent) {
            var id = event.target.value;
            var tmpltId = selfTmplt.tmplt().tmpltId();
            selfTmplt.BayExtndrSpecnRevsnAltId = id;
        }
    }

    Template.prototype.onAddBaySelection = function () {
        alert('Hi Mike');
    }

    Template.prototype.SearchSpec = function () {
        selfTmplt.HlpMnrMtlList([]);
        $('#idHlpList').DataTable().clear().destroy();

        selfTmplt.showWheelOfPatience(true);

        if (selfTmplt.selectedNewMtlCtgry() == 3) {
            var replVal = "";
            if (selfTmplt.searchSpecType != "") {
                if (selfTmplt.searchSpecType == "Plug-In") {
                    replVal = selfTmplt.searchSpecType;
                    replVal = replVal.replace(/-/g, '_');
                }
            }

            var searchJSON = {
                pTyp: selfTmplt.searchSpecType == "Plug-In" ? replVal.toUpperCase() : selfTmplt.searchSpecType.toUpperCase(),
                pClss: selfTmplt.RoGenTypeVal,
                pId: $("#idSpecID").val(),
                pNm: $("#idSpecName").val(),
                pDsc: $("#idSpecDesc").val(),
                pStts: selfTmplt.searchSpecStatusVal,
                pmodelNm: $("#idTmpModel").val(),
                pMtlCd: ""
            };

            $.ajax({
                type: "GET",
                url: 'api/tmplt/searchTmpltSpec',
                data: searchJSON, contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: searchSpecSuccess,
                error: searchSpecError,
                context: selfTmplt
            });

            function searchSpecSuccess(response) {
                selfTmplt.SpecList(response);
                if (selfTmplt.SpecList().length == 0) {
                    selfTmplt.showMessage("No data found", "Template");
                }
                selfTmplt.showWheelOfPatience(false);

            }

            function searchSpecError() {
                selfTmplt.showWheelOfPatience(false);
                alert("Internal Server error");
            }
        }
        if (selfTmplt.selectedNewMtlCtgry() < 3) {


            //pPrdId,pPrtNo,pMtlDesc,pMFG,pcdmmsid,pItmSts,pStatus,pFtrTyp,pSpecNm,pMtlCtgry,pCblTyp,pLstDt,pLstCuid,pHeciClei,pHasHeciClei,pStandaloneClei

            var standaloneCleiSearch = 'N';
            var productId = "";
            var partNo = "";
            var mtlDesc = "";
            var mfg = "";
            var stts = "";
            var featureType = "";
            var cableType = "";
            var materialCategory = selfTmplt.selectedNewMtlCtgry();
            var itemStatus = "";
            var specName = "";
            var mtlItemId = "";
            var lastupdt = "";
            var userid = "";
            var heciclei = "N";



            if (productId == '' && partNo == '' && mtlDesc == '' && mfg == '' && stts == '' && featureType == ''
                && cableType == '' && materialCategory == '' && itemStatus == '' && specName == '' && mtlItemId == ''
                && lastupdt == '' && userid == '') {
                standaloneCleiSearch = 'Y';
            }

            var showMessage = false;
            var searchJSON = {
                pPrdId: productId,
                pPrtNo: partNo,
                pMtlDesc: mtlDesc,
                pMFG: mfg,
                pcdmmsid: mtlItemId,
                pStatus: stts,
                pMtlCtgry: materialCategory,
                pCblTyp: cableType,
                pFtrTyp: featureType,
                pItmSts: itemStatus,
                pSpecNm: specName,
                pLstDt: lastupdt,
                pLstCuid: userid,
                pHasHeciClei: heciclei,
                pStandaloneClei: standaloneCleiSearch
            };

            //if (standaloneCleiSearch == 'Y' && heciclei == '') {
            //    showMessage = true;
            //}

            //if (productId !== "") {
            //    if (partNo === "" && mtlDesc === "" && mfg === "" && (stts === "" || stts === undefined) && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && itemStatus === "" && specName === "" && mtlItemId === "" && lastupdt === "" && userid === "") {
            //        if (productId.length <= 2)
            //            showMessage = true;
            //    }
            //}
            //else if (itemStatus === 'x' || itemStatus === 'X') {
            //    if (partNo === "" && mtlDesc === "" && mfg === "" && (stts === "" || stts === undefined) && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && productId === "" && specName === "" && mtlItemId === "" && lastupdt === "" && userid === "") {
            //        showMessage = true;
            //    }
            //} else if (partNo === "" && mtlDesc === "" && mfg === "" && stts === "3" && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && itemStatus === "" && specName === "" && mtlItemId === "" && lastupdt === "") {
            //    showMessage = true;
            //}

            //if (showMessage) {
            //    selfTmplt.showMessage("Please enter additional search criteria.").then(function () {
            //        return;
            //    });
            //}

            //var searchJSON = {
            //    pTyp: selfTmplt.searchSpecType == "Plug-In" ? replVal.toUpperCase() : selfTmplt.searchSpecType.toUpperCase(),
            //    pClss: selfTmplt.RoGenTypeVal,
            //    pId: $("#idSpecID").val(),
            //    pNm: $("#idSpecName").val(),
            //    pDsc: $("#idSpecDesc").val(),
            //    pStts: selfTmplt.searchSpecStatusVal,
            //    pmodelNm: $("#idTmpModel").val(),
            //    pMtlCd: ""
            //};

            $.ajax({
                type: "GET",
                url: 'api/tmplt/searchHlpMnrMtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: searchHlpSuccess,
                error: searchHlpError,
                context: selfTmplt
            });

            function searchHlpSuccess(response) {
                selfTmplt.HlpMnrMtlList(response);
                $(document).ready(function () {
                    $('#idHlpList').DataTable({
                        "pagingType": "simple"// false to disable pagination (or any other option)
                    });
                });

                if (selfTmplt.HlpMnrMtlList().length == 0) {
                    selfTmplt.showMessage("No data found", "HLP");
                }
                selfTmplt.showWheelOfPatience(false);

            }

            function searchHlpError(status) {
                selfTmplt.showMessage(status, "Internal Server error");
            }
        }

    };
    Template.prototype.resetPageForDisplay = function () {
        var viewer = selfTmplt.templateViewer();
        if (viewer) {
            viewer.clear();
            viewer.hide();
        }

        $("#PANEL_DETAILS").hide();
        $("#overAllTmpltDtls").hide();
        $("#divTmpltSave").hide();
        $("#PANEL_GRAPHICAL_VIEW").hide();
        $("#tmpltViewerArea").hide();

        selfTmplt.tmplt({});
    }
    Template.prototype.fetchTemplate = function ( tmpltId, next ) {
        var $this = selfTmplt;

        next = (next||function() { });

        $this.showWheelOfPatience(true);
        $this.resetPageForDisplay();

        var href = window.location.href;

        var route = 'api/tmplt/' + tmpltId + '/ANY'; // mwj: can a base template and overall template have the same id #?

        
        //
        // !!! CHECK HERE: MOCKING BASE TMPLT FETCH
        //
        var mocking = false; //(tmpltId == 43 || tmpltId == 45);

        var display = function (response) {
            $this.showWheelOfPatience(false);
            $this.displayTemplate(response, mocking);
            next();
        }
        var error = function (status) {
            $this.showWheelOfPatience(false);
            if (window.location.hash.indexOf('tmplt') == -1) {
                return;
            }
            // make this better... internal server error doesn't tell us much.
            alert("Failed to fetch template #"+tmpltId+"\n"+JSON.stringify(status));
        }

        var payload = { baseTmplt: -1, forEdit: true }; // mwj: why are we doing this?

        route = (mocking ? './test-data/'+tmpltId+'.json' : route);
        $.ajax({
            type: "GET",
            url: route,
            data: payload,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: display,
            error: error,
            context: selfTmplt
        });
    }
    Template.prototype.displayTemplate = function (response, mocking) {
        if (!response || response === EMPTY_SET) {
            selfTmplt.showMessage("No records found", "Template");
            return;
        }
        if (response.length == 0) {
            selfTmplt.showMessage("No records found", "Template");
            return;
        }

        var baseTmplt = JSON.parse(response[0]);
        var cmplxTmplt = (response.length > 1 ? JSON.parse(response[1]) : null);
        if( cmplxTmplt && response.length > 2 ) {
            $.extend(cmplxTmplt, JSON.parse(response[2]));
        }

        var tmplt = {};
        $.extend(tmplt, (cmplxTmplt||baseTmplt));

        var status = [ "BaseTemplateInd", "CompletedInd", "PropagatedInd", "UpdateInProgressInd", "RetiredTemplateInd", "DeletedInd", "HlpTmpltInd", "CommonConfigTemplateInd" ];
        status.forEach(function (field) {
            tmplt[field] = (tmplt[field]=='Y');
            baseTmplt[field] = (baseTmplt[field]=='Y');
        });

        selfTmplt.tmplt( mapping.fromJS(tmplt) );
        selfTmplt.tmpltOrigin( mapping.fromJS(tmplt) );
        selfTmplt.baseTmplt( mapping.fromJS(baseTmplt));

        // clear screen
        $("#overAllTmpltDtls").hide();
        //$("#bayTmpltHidden").hide();

        //switch( baseTmplt.TemplateType.toUpperCase() ) {
        //    case "BAY":  $("#bayTmpltHidden").show(); break;
        //    // put others here
        //}

        if (cmplxTmplt) {
            $("#overAllTmpltDtls").show();
        }
        
        $("#PANEL_DETAILS").show();

        selfTmplt.scrollToDetails();
        selfTmplt.BayExtndrSpecnRevsnAltId = (cmplxTmplt ? (cmplxTmplt.BayExtndrSpecnRevsnAltId||0) : 0);

        selfTmplt.enableSaveButton(true); // FIX THIS; should be false ultimately, I think....
        //$("#divTmpltSave").hide();

        selfTmplt.showWheelOfPatience(true);
        selfTmplt.activateTemplateViewer();
    }
    Template.prototype.displayTemplateOld = function (response, mocking) {
         if (response === EMPTY_SET) {
            selfTmplt.showMessage("No records found", "Template");
            return;
         }

         var rsp = (mocking ? response : JSON.parse(response));

         $("#divTmpltSave").show();
         $("#btnSaveTmplt").show();

         selfTmplt.tmplt(mapping.fromJS(rsp));
         selfTmplt.tmpltOrigin(mapping.fromJS(rsp));

         if (selfTmplt.tmplt().baseTmplt()) {
             if (selfTmplt.tmplt().typ().toUpperCase() == "BAY") {
                 //selfTmplt.indicator(selfTmplt.tmplt().frntRr() == "F" ? "Front" : "Rear");
                 //selfTmplt.RotationAngleId(selfTmplt.tmplt().rtnAngl.value());
                 //$("#bayTmpltHidden").show();
             }
             else {
                 //$("#bayTmpltHidden").hide();
             }

             $("#PANEL_DETAILS").show();
         }
         else {
             if (selfTmplt.tmplt().typ().toUpperCase() == "BAY") {
                 //selfTmplt.indicator(selfTmplt.tmplt().bsTmplt.frntRr() == "F" ? "Front" : "Rear");
                 //selfTmplt.RotationAngleId(selfTmplt.tmplt().bsTmplt.rtnAngl.value());
                 //$("#bayTmpltHidden").show();
                 // $("#PANEL_GRAPHICAL_VIEW").show();
             }
             else {
                 //$("#bayTmpltHidden").hide();
                 //$("#PANEL_GRAPHICAL_VIEW").hide();
             }

             $("#PANEL_DETAILS").show();
             $("#overAllTmpltDtls").show();
             //$("#PANEL_GRAPHICAL_VIEW").show();
             selfTmplt.scrollToDetails();
             selfTmplt.BayExtndrSpecnRevsnAltId = selfTmplt.tmplt().BayExtndrSpecnRevsnAltId != undefined ? selfTmplt.tmplt().BayExtndrSpecnRevsnAltId() : 0;
         }
         //document.getElementById("btnSaveTemp").disabled = true;
         selfTmplt.enableSaveButton(true); // FIX THIS; should be false ultimately, I think....
         $("#divTmpltSave").hide();

         selfTmplt.activateTemplateViewer();
    }
    Template.prototype.onSelectTemplate = function (item) {
        selfTmplt.fetchTemplate(item.TMPLT_ID);
        return;
/*
        selfTmplt.showWheelOfPatience(true);
        selfTmplt.resetPageForDisplay();

        selfTmplt.MtrlCatId = item.MTL_CAT_ID;
        selfTmplt.FeatTypId = item.FEAT_TYP_ID;

        var tmpltTyp = item.TMPLT_TYP.toUpperCase();
        tmpltTyp = tmpltTyp.replace(/-/g, '_');

        if (tmpltTyp.toUpperCase().indexOf("HIGH") > -1) {
            tmpltTyp = replVal.replace(/ /g, '_');
        }
        var route = 'api/tmplt/' + item.TMPLT_ID + '/' + tmpltTyp;
        var isBaseTmplt = (item.TMPLT_CAT.indexOf('Base') >= 0);

        var payload = {
            baseTmplt: isBaseTmplt,
            forEdit: true
        };

        //
        // !!! CHECK HERE: MOCKING BASE TMPLT FETCH
        //
        var mocking = (item.TMPLT_ID == 43 || item.TMPLT_ID == 45);
        route = (mocking ? './test-data/'+item.TMPLT_ID+'.json' : route);
        $.ajax({
            type: "GET",
            url: route,
            data: payload,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(response) { selfTmplt.displayTemplate(mocking, response); },
            error: searchError,
            context: selfTmplt
        });
*/       
/*        
        function searchSuccess(response) {
            selfTmplt.showWheelOfPatience(false);

            if (response === EMPTY_SET) {
                selfTmplt.showMessage("No records found", "Template");
                return;
            }

            var rsp = (mocking ? response : JSON.parse(response));

            $("#divTmpltSave").show();
            $("#btnSaveTmplt").show();

            selfTmplt.tmplt(mapping.fromJS(rsp));
            selfTmplt.tmpltOrigin(mapping.fromJS(rsp));

            if (selfTmplt.tmplt().baseTmplt()) {
                if (selfTmplt.tmplt().typ().toUpperCase() == "BAY") {
                    //selfTmplt.indicator(selfTmplt.tmplt().frntRr() == "F" ? "Front" : "Rear");
                    //selfTmplt.RotationAngleId(selfTmplt.tmplt().rtnAngl.value());
                    //$("#bayTmpltHidden").show();
                }
                else {
                    //$("#bayTmpltHidden").hide();
                }

                $("#PANEL_DETAILS").show();
            }
            else {
                if (selfTmplt.tmplt().typ().toUpperCase() == "BAY") {
                    //selfTmplt.indicator(selfTmplt.tmplt().bsTmplt.frntRr() == "F" ? "Front" : "Rear");
                    //selfTmplt.RotationAngleId(selfTmplt.tmplt().bsTmplt.rtnAngl.value());
                    //$("#bayTmpltHidden").show();
                    // $("#PANEL_GRAPHICAL_VIEW").show();
                }
                else {
                    //$("#bayTmpltHidden").hide();
                    //$("#PANEL_GRAPHICAL_VIEW").hide();
                }

                $("#PANEL_DETAILS").show();
                $("#overAllTmpltDtls").show();
                //$("#PANEL_GRAPHICAL_VIEW").show();
                selfTmplt.scrollToDetails();
                selfTmplt.BayExtndrSpecnRevsnAltId = selfTmplt.tmplt().BayExtSpecRevAltId != undefined ? selfTmplt.tmplt().BayExtSpecRevAltId() : 0;
            }
            //document.getElementById("btnSaveTemp").disabled = true;
            selfTmplt.enableSaveButton(true); // FIX THIS; should be false ultimately, I think....
            $("#divTmpltSave").hide();

            selfTmplt.activateTemplateViewer();
        }
        */
        function searchError() {
            selfTmplt.showWheelOfPatience(false);
            // make this better... internal server error doesn't tell us much.
            alert("Internal Server error");
        }
    };

    Template.prototype.scrollToDetails = function () {
        setTimeout(function () {
            selfTmplt.showWheelOfPatience(false);
            document.getElementById('PANEL_DETAILS').scrollIntoView();
        }, 1000);
    };

    Template.prototype.specificationSelectCall = function (item) {
        selfTmplt.showWheelOfPatience(true);

        var mtlType = ""
        var mtlId = "";
        var tempname = "";
        if (selfTmplt.selectedNewMtlCtgry() == 1) {      // This id for High Level Part 
            mtlType = "HIGH_LEVEL_PART";
            mtlId = item.MATERIAL_ITEM_ID;
            tempname = item.MFG_PART_NO;
            selfTmplt.HlpRevId = item.MATERIAL_ITEM_ID;
        }
        else if (selfTmplt.selectedNewMtlCtgry() == 2) {   // This id for Common Config 
            mtlType = "COMMON_CONFIG";
            mtlId = item.MATERIAL_ITEM_ID;
            tempname = "COMMON_CONFIG";
        }
        else if (selfTmplt.selectedNewMtlCtgry() == 3) {    // This id for Spec Part 
            mtlType = item.SPECTYP;
            mtlId = item.SPECN_ID;
            tempname = item.SPECN_NM;
        }

        var searchJSON = {
            pSpecTyp: mtlType,
            pSpecId: mtlId
        };

        $.ajax({
            type: "GET",
            url: 'api/tmplt/searchBaseTmplt',
            data: searchJSON,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: specSelectSuccess,
            error: specSelectError,
            context: selfTmplt
        });

        function specSelectSuccess(baseTmpId) {
            selfTmplt.showWheelOfPatience(false);

            var baseHeader = "Create New " + mtlType + " Template for <b>" + tempname + "</b>";

            $("#idBaseTmpHeader").html(baseHeader);
            $("#idOverallTmpHeader").html(baseHeader);
            document.getElementById("idBaseTmpNm").disabled = false;
            if (baseTmpId == 0) {
                $("#hiddenCreateBaseSpecId").text(mtlId);
                var replVal = mtlType;
                replVal = replVal.replace(/-/g, '_');
                $("#hiddenCreateBaseSpecTyp").text(replVal);
                $("#divCreateBasetmplt").show();
            }
            else {
                $("#hiddenCreateOverAllBaseTmpltId").text(baseTmpId);
                var replVal = mtlType;
                replVal = replVal.replace(/-/g, '_');
                $("#hiddenCreateOverAllSpecTyp").text(replVal);
                if (replVal != "PLUG_IN") {
                    $("#divCreateOveralltmplt").show();
                }
                else {
                    selfTmplt.showMessage("This template already have base template with id : " + baseTmpId + ".  Plug in template do not have the over all template.", "PlugIn template");
                }
            }
        }
        function specSelectError() {
            selfTmplt.showWheelOfPatience(false);
            alert("Internal Server error");
        }
    };

    Template.prototype.btnCreateBaseTmp = function () {
        if ($("#idBaseTmpNm").val() == "") {
            selfTmplt.showMessage("Please enter Base template name.", "Template Validations");
            return false;
        }
        else if ($("#idBaseTmpDsc").val() == "") {
            selfTmplt.showMessage("Please enter Base template description.", "Template Validations");
            return false;
        }

        selfTmplt.showWheelOfPatience(true);

        var searchJSON = {
            pTmpType: "Base",
            pTmpName: $("#idBaseTmpNm").val(),
            pTmpDesc: $("#idBaseTmpDsc").val(),
            pTmpTyp: $("#hiddenCreateBaseSpecTyp").text(),
            pSpecId: $("#hiddenCreateBaseSpecId").text()
        };

        selfTmplt.createBaseOverallTmplt(searchJSON);
    };

    Template.prototype.onSelectTemplateItem = function () {
        selfTmplt.showMessage('TODO: Select Template Item');
    }
    Template.prototype.onCreateOverallTmplt = function () {
        if ($("#idOverallTmpNm").val() == "") {
            selfTmplt.showMessage("Please enter Overall template name.", "Template Validations");
            return false;
        }
        else if ($("#idOverallTmpDsc").val() == "") {
            selfTmplt.showMessage("Please enter Overall template description.", "Template Validations");
            return false;
        }

        selfTmplt.showWheelOfPatience(true);

        var searchJSON = {
            pTmpType: "Overall",
            pTmpName: $("#idOverallTmpNm").val(),
            pTmpDesc: $("#idOverallTmpDsc").val(),
            pTmpTyp: $("#hiddenCreateOverAllSpecTyp").text(),
            pBaseTmpltId: $("#hiddenCreateOverAllBaseTmpltId").text(),
            pHlpRevId: selfTmplt.selectedNewMtlCtgry() == 1 ? selfTmplt.HlpRevId : 0,
            cuid: selfTmplt.usr.cuid
        };

        //var searchJSON = mapping.toJSON(selfTmplt.tmplt);
        //searchJSON = JSON.parse(searchJSON);	

        selfTmplt.createBaseOverallTmplt(searchJSON);
    };

    Template.prototype.createBaseOverallTmplt = function (reqData) {
        var payload = JSON.stringify(reqData);
        $.ajax({
            type: "POST",
            url: 'api/tmplt/createTmplt',
            data: payload,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: createBaseTmpltSuccess,
            error: createBaseTmpltError,
            context: selfTmplt
        });

        function createBaseTmpltSuccess(baseTmpId) {
            selfTmplt.showWheelOfPatience(false);

            if (baseTmpId > 0) {
                selfTmplt.showMessage("Template created successfully", "Template");

                $("#divCreateNewTmplt").hide();
                selfTmplt.onCancelBaseTmp();
                selfTmplt.onCancelOverallTmp();


                var payload = {
                    pTmpltNm: "",
                    pTmpltId: baseTmpId,
                    pTmpltDsc: "",
                    pBaseTmpltInd: "",
                    pHlpTmpltInd: "",
                    pComnCnfgTmpltInd: "",
                    pCmpltInd: "",
                    pPrpgtInd: "",
                    pUpdtInPrgsInd: "",
                    pRetTmpltInd: "",
                    pDelInd: "",
                    pTmpltTypId: ""
                };
                selfTmplt.searchTemplateDetails(payload);
                $("#PANEL_DETAILS").hide();
                $("#overAllTmpltDtls").hide();
                $("#divTmpltSave").hide();
            }
            else {
                selfTmplt.showMessage("Template did not create, internal error", "Template");
            }
        }

        function createBaseTmpltError(status) {
            selfTmplt.showWheelOfPatience(false);
            selfTmplt.showMessage(status, "Internal Server Error Creating Base Template");
        }
    };


    return Template;
});
