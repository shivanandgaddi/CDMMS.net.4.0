define(['durandal/composition', 'plugins/router', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jqueryui'
    , '../Utility/referenceDataHelper', 'durandal/app', 'bootstrapJS', 'fabricjs', '../Utility/cadHelper'
    , '../Utility/CadAddEquipDialog'
    , '../../Scripts/AppPopoutWindow'
    ],
    function (composition, router, ko, $, http, activator, mapping, system, jqueryui,
        reference, app, bootstrapJS, fabricjs, cad
        , cadAddEquipDialog
        , popoutWindow
        ) 
    {
        var TemplateViewer = function (drawing, next) {
            selftv = this;

            selftv.canvas = null;
            selftv.usr = require('Utility/user');
            selftv.cad = null;
            selftv.drawing = drawing;
            selftv.clipboard = ko.observable();
            selftv.$container = null;
            selftv.waitTillDocumentReady(next);
        };

        TemplateViewer.prototype.waitTillDocumentReady = function (next) {
            next = (next || function () { });
            var _canvas = document.getElementById('tmpltCanvas');

            var checkIfReady = function () {
                if (!_canvas) {
                    selftv.waitTillDocumentReady(next);
                    return;
                }

                selftv.$container = $('#tmpltViewerArea');

                selftv.canvas = new fabricjs.Canvas(_canvas);
                selftv.cad = new cad(selftv.canvas);

                if (selftv.drawing) {
                    selftv.refreshDrawing(selftv.drawing, next);
                }

                //var freeDrawing = true;

                //var offset = $('#tmpltCanvas').offset();
                //$(document).mousemove(function (e) {
                //	divPos = {
                //		left: e.pageX - offset.left,
                //		top: e.pageY - offset.top
                //	};
                //});
            }

            setTimeout(checkIfReady, 1000);
        };

        TemplateViewer.prototype.refreshDrawing = function (data, next) {
            next = (next || function () { });

            selftv.drawing = data; // curr & orig

            selftv.setupButtonControls();
            selftv.initEquipOptionsFilter();

            var error = null;
            //try
            //{
                var display = function (attrs) {
                    selftv.loadGraphicalView(attrs);
                    selftv.loadTabularView(attrs);
                }
                
                var route = 'api/cad/attrs/list/'+selftv.drawing.curr().TemplateID()+'/0'; // 0 tmplDefId = N/A
                $.ajax({
                    type: 'GET',
                    url: route,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    context: selftv,
                    async: true
                })
                .done(display)
                .fail(function(err) { 
                    selftv.showMessage(err, "Failed To Retrieve Drawing"); 
                })
                .complete(function () {
                    selftv.showWheelOfPatience(false);
                });
            //}
            //catch (err) {
            //    error = err;
            //}
            next({ error: error });
        }
        TemplateViewer.prototype.loadGraphicalView = function (attrs) {
            selftv.cad.generateDrawing(selftv.drawing, attrs, function() { 
                        document.getElementById('tmpltViewerArea').scrollIntoView();
                    });
        }
        TemplateViewer.prototype.loadTabularView = function (data) {
            var $container = $("#tabINFO_LOC table tbody");
            $container.empty();

            var pnl = 1;
            var list = [];
            // mock up data items...
            data.forEach(function (item) {
                if (!item.JSON_DATA) {
                    return;
                }

                var d = JSON.parse(item.JSON_DATA);
                if (!d.rec) {
                    return;
                }

                var i = d.rec;
                var rec = { }
                rec.CDMMS_ID    = i.CDMMS_ID||i.SPEC_ID;
                rec.MTRL_ID     = i.MTRL_ID||0;
                rec.SPECN_NAME  = i.SPECN_NAME||i.NAME;
                rec.PART_NO     = i.PART_NO||'-';
                rec.MTRL_DESC   = i.MTRL_DESCR||i.MTRL_DESC;
                rec.FEAT_TYP    = i.FEAT_TYP||'-';
                rec.LABEL       = i.SPECN_NAME||i.NAME; //'PNL-'+(pnl++);
                rec.MNTNG_POS   = i.MNTNG_POS||i.RU||'-';
                rec.EQDES       = i.EQDES||d.top;
                rec.HORZ_DISP   = i.HORZ_DISP||d.left;
                rec.TMPLT_NM  = i.TMPLT_NM||rec.SPECN_NAME;
                rec.DEPT        = 'Local';
                rec.OWNER       = 'CTL';
                list.push(rec);
            });
            var $tmplt = $("#TMPLT_TAB_INFO_LOC");
            $tmplt.tmpl(list).appendTo($container);

            var view = function () {
                var cat = $(this).data('cat');
                var id = $(this).parent().data(cat.toLowerCase());
                var title = { mtlInv: 'Material Inventory', spec: 'Specification', comnCnfg: 'Common Config' }[cat];
                
                popoutWindow.Open(title, cat, id);
            }
            $container.find('.btnOPEN').click(view);
        }
   
        TemplateViewer.prototype.showMessage = function (msg,title) {
            var text = "";
            if (msg instanceof Array ) {
                text = msg.join("<br/>");
            }
            else if( msg instanceof Object ) {
                text = [msg.MSG,msg.OV].join("<br/>");
            }
            else { 
                text = msg.split('\n').join("<br/>");
            }
            app.showMessage(text, title, null, null, { style: { width: '600px' } });
            selftv.showWheelOfPatience(false);
        }
        TemplateViewer.prototype.showWheelOfPatience = function (show) {
            var $wait = $('#interstitial').css('height', $('body').height());

            show ? $wait.show()
                : $wait.hide()
                ;

            show ? $('body').addClass('wait-cursor')
                : $('body').removeClass('wait-cursor')
                ;
        }
        TemplateViewer.prototype.clear = function () {
            var canvas = selftv.canvas;
            if (!canvas) {
                return;
            }
            canvas.discardActiveObject();

            var list = canvas.getObjects();
            while (list.length) {
                canvas.remove(list.shift());
            }
        }
        TemplateViewer.prototype.show = function (status) {
            if (status == false) {
                this.hide();
                return;
            }
            selftv.$container.show();
        }
        TemplateViewer.prototype.hide = function () {
            selftv.$container.hide();
        }
        TemplateViewer.prototype.setParent = function (parent) {
            selftv.parent = parent;
        }
        TemplateViewer.prototype.onSave = function () {
            selftv.showWheelOfPatience(true);
            //// mock up
            //var data = { cuid: selftv.usr.cuid 
            //           , updates: [ { CAD_ATTR_ID: 0, CAD_ATTR_TYP: "LABEL", TMPLT_ID: 1, TMPLT_DEF_ID: 101, JSON_DATA: '{ "one": "1", "two": "2" }'} // add 
            //                      , { CAD_ATTR_ID: 101, CAD_ATTR_TYP: "LABEL", TMPLT_ID: 1, TMPLT_DEF_ID: 101, JSON_DATA: '{ "three": "3", "four": "4" }'} // update
            //                      ] 
            //           , deletes: []
            //};

            // fix this...use the constants in CadUtils....
            var types = 'LABEL,SHELF,NODE,SLOT,CARD,PORT,LABEL_BAYNAME'.split(',');
            var data = { cuid: selftv.usr.cuid, updates: [] , deletes: [] };
            var attrs = selftv.canvas.getObjects().filter(function(item) { 
                var save =(item.equipType != undefined && types.indexOf(item.equipType) >= 0);
                console.log('templateViewer.onSave() %s %s %s', item.equipType, item.name||'?', save ? 'save' : 'skip');
                return save;
            });
            attrs.forEach(function (item) {
                var rec = { CAD_ATTR_TYP: item.equipType };
                rec.CAD_ATTR_ID = (item.CAD_ATTR_ID||0);
                rec.TMPLT_ID = (item.TMPLT_ID||selftv.drawing.curr().TemplateID());
                rec.TMPLT_DEF_ID = (item.TMPLT_DEF_ID||0);
                rec.JSON_DATA = JSON.parse(JSON.stringify(item));
                if (item.rec) {
                    rec.JSON_DATA.rec = item.rec;
                }
                rec.JSON_DATA = JSON.stringify(rec.JSON_DATA);

                data.updates.push(rec);
            });

            if (console.table) {
                console.table(data);
            }
            var errors = [];
            var updated = function (status) {
                selftv.showWheelOfPatience(false);
                
                // OV (output value) { errors: [], updates: [] }
                var ov = JSON.parse(status.OV);
                ov.updates.forEach(function (item, ndx) {
                    attrs[ndx].CAD_ATTR_ID = parseInt(item.CAD_ATTR_ID);
                });

                status.EC = (ov.errors.length > 0 ? -1 : status.EC);
                status.OV = (ov.errors.length > 0 ? ov.errros.join("<br/>") : status.OV);
                if (status.EC != 0) {
                    selftv.showMessage(status, "Problem Updating Attributes");
                }
            }
            var failed = function (err) {
                var status = { EC: -1, MSG: JSON.stringify(err.responseJSON), OV: '' };

                selftv.showMessage(status, "Process Failure Updating Attributes");
                console.log("+++ failed - saveCadAttributes");
                console.log(err.responseJSON);
                console.log("--- failed - saveCadAttributes");
            }

            $.ajax({
                type: "POST",
                url: 'api/cad/attrs/update',
                data: JSON.stringify(data),
                contentType: "application/json; charset=utf-8",
                dataType: "json", 
                contenxt: selfTmplt

            })
            .then( updated )
            .fail( failed )
            .done()
            ;
        }
        
        TemplateViewer.prototype.createCadButtonEvents = function () {
            var defHandler = function () {
                var $btn = $(this);
                selftv.showMessage($btn.attr('id')+': to be implemented');
            }
            var events = {
                OnResizeContainer: defHandler
                , OnSetContainer: defHandler
                , OnToggleGrid: defHandler

                , OnAlignLeft: function() { selftv.onAlignLeft(); }
                , OnAlignCenter: function() { selftv.onAlignCenter(); }
                , OnAlignRight: function() { selftv.onAlignRight(); }

                , OnAlignTop: defHandler
                , OnAlignMiddle: defHandler
                , OnAlignBottom: defHandler

                , OnDistributeHorizontal: defHandler
                , OnDistributeVertical: defHandler

                , OnRotateLeft: function() { selftv.onRotateActiveObject(-90); }
                , OnRotateRight: function() { selftv.onRotateActiveObject(90); }

                , OnCanvasCut: defHandler
                , OnCanvasCopy: defHandler
                , OnCanvasPaste: defHandler

                , OnAutoNumber: defHandler
                , OnToggleTrailPoints: function() { selftv.onToggleTrailPoints(); }
                , OnAddLabel: function() { selftv.onAddLabel(); }
                , OnAddEquip: function() { selftv.onAddEquip(); }

                , OnFontLarger: defHandler
                , OnFontSmaller: defHandler
                , OnFontSettings: defHandler

                , OnViewAuditLog: defHandler
                , OnSave: function() { selftv.onSave(); }
            };
            return events;
        }
        TemplateViewer.prototype.onRotateActiveObject = function (degrees) {
            selftv.cad.rotateActiveObject(degrees);
        }

        TemplateViewer.prototype.onAlignLeft = function () {
            var o = selftv.cad._canvas.getActiveObject();
            o.lockOn= 'left';
            if( o.adjustAlignment ) o.adjustAlignment();
            if( o.adjustLabels ) o.adjustLabels();
        }
        TemplateViewer.prototype.onAlignCenter= function () {
            var o = selftv.cad._canvas.getActiveObject();
            o.lockOn= 'center';
            if( o.adjustAlignment ) o.adjustAlignment();
            if( o.adjustLabels ) o.adjustLabels();
        }
        TemplateViewer.prototype.onAlignRight= function () {
            var o = selftv.cad._canvas.getActiveObject();
            o.lockOn= 'right';
            if( o.adjustAlignment ) o.adjustAlignment();
            if( o.adjustLabels ) o.adjustLabels();
        }
        TemplateViewer.prototype.onAddEquip = function () {
            var $this = this;
            var caption = 'Add Equipment';
            if ($this.cad.getTrailPoints().length === 0) {
                $this.onToggleTrailPoints(function() { 
                    $this.onAddEquip();
                });
                return;
            }

            var add = function (rec) {
                if (rec.CAN_FIT == 0) {
                    var msg = [rec.NAME+" is too big to fit:"
                    , "* equipment size: "+rec.DIMS
                    , "* container size: "+rec.CNTNR_W+rec.CNTNR_UOM+" x "+rec.CNTNR_H+rec.CNTNR_UOM
                    ];
                    $this.showMessage(msg)
                    return false;
                }
                $this.cad.addEquipAtTrailPoint(rec);
                $this.clearTrailPoints(); 
                return true;
            }
            var cancel = function () {
            }
            var props = { title: "Add Equipment", onOK: add, onCANCEL: cancel };
            props.typ = $this.drawing.curr().TemplateType();
            props.tmpltId = $this.drawing.curr().TemplateID();
            
            cadAddEquipDialog.Display(props);
        }
        TemplateViewer.prototype.onAddLabel = function () {
            var caption = 'Add New Label';
            if (selftv.cad.getTrailPoints().length === 0) {
                selftv.showMessage('Please set at least one trail point to add a label to the drawing.', caption)
                return;
            }
            var label = prompt('New label:', '');
            label = $.trim(label);
            if (label.length === 0) {
                return;
            }
            try {
                selftv.cad.addLabelAtTrailPoint(label);
            }
            catch (err) {
                selftv.showMessage((err.message||err), caption);
            }
        }
        TemplateViewer.prototype.onToggleTrailPoints = function( callWhenSet ) {
            var $btn = $("#btnSET_TRAIL_POINT");
            $btn.toggleClass('btn-enabled');

            callWhenSet = (callWhenSet||function() { });

            var enabled = $btn.hasClass('btn-enabled');
            if( enabled ) { 
                var onTrailPointSet = function (points) {
                    selftv.onTrailPointSet(points);
                    callWhenSet(points);
                }
                var onTrailPointsDisabled = function () {
                    $btn.removeClass('btn-enabled');
                }
                selftv.cad.enableTrailPointing(onTrailPointSet, onTrailPointsDisabled);
            }
            else {
                selftv.clearTrailPoints(); // user turned off trailpoint, so discard info...
            }
        }
        TemplateViewer.prototype.clearTrailPoints = function () {
            selftv.cad.disableTrailPointing();
            selftv.cad.clearTrailPoints(); 
             $("#btnSET_TRAIL_POINT").removeClass('btn-enabled');
        }
        TemplateViewer.prototype.onTrailPointSet = function (trailPoints) {
            if (trailPoints.length == 2) {
                var $btn = $("#btnSET_TRAIL_POINT");
                $btn.toggleClass('btn-enabled');
                selftv.cad.disableTrailPointing();
                selftv.cad.resetCursor();
                selftv.cad.drawTrailPointTracer(trailPoints);
                //
                // !!! NOTE: we aren't clearing the trailpoints because now
                // they are going to place something on the drawing when 
                // they place something, then we'll clear the trail points
                // from the canvas.
                //
            }
        }
        TemplateViewer.prototype.setupButtonControls = function () {

            console.log('TemplateViewer.prototype.setupButtonControls()');
            var events = this.createCadButtonEvents();
            
            var $view = selftv.$container;
            $view.find('.zoom-control').hide();

            var $btns = $view.find('.cad-button-bank');
            $btns.find('#btnRESIZE_CONTAINER').button().unbind('click').click(events.OnResizeContainer).hide();
            $btns.find('#btnSET_CONTAINER').button().unbind('click').click(events.OnSetContainer).hide();
            $btns.find('#btnTOGGLE_GRID').button().unbind('click').click(events.OnToggleGrid).hide();

            $btns.find('#btnALIGN_LEFT').button().unbind('click').click(events.OnAlignLeft);
            $btns.find('#btnALIGN_CENTER').button().unbind('click').click(events.OnAlignCenter);
            $btns.find('#btnALIGN_RIGHT').button().unbind('click').click(events.OnAlignRight);

            $btns.find('#btnALIGN_TOP').button().unbind('click').click(events.OnAlignTop).hide();
            $btns.find('#btnALIGN_MIDDLE').button().unbind('click').click(events.OnAlignMiddle).hide();
            $btns.find('#btnALIGN_BOTTOM').button().unbind('click').click(events.OnAlignBottom).hide();

            $btns.find('#btnDIST_HORZ').button().unbind('click').click(events.OnDistributeHorizontal).hide();
            $btns.find('#btnDIST_VERT').button().unbind('click').click(events.OnDistributeVertical).hide();

            $btns.find('#btnROTATE_LEFT').button().unbind('click').click(events.OnRotateLeft);
            $btns.find('#btnROTATE_RIGHT').button().unbind('click').click(events.OnRotateRight);

            $btns.find('#btnCUT').button().unbind('click').click(events.OnCanvasCut).hide();
            $btns.find('#btnCOPY').button().unbind('click').click(events.OnCanvasCopy).hide();
            $btns.find('#btnPASTE').button().unbind('click').click(events.OnCanvasPaste).hide();

            $btns.find('#btnAUTO_NUMBER').button().unbind('click').click(events.OnAutoNumber).hide();
            $btns.find('#btnSET_TRAIL_POINT').button().unbind('click').click(events.OnToggleTrailPoints);
            $btns.find('#btnADD_LABEL').button().unbind('click').click(events.OnAddLabel);
            $btns.find('#btnADD_EQUIP').button().unbind('click').click(events.OnAddEquip);

            $btns.find('#btnFONT_LARGER').button().unbind('click').click(events.OnFontLarger).hide();
            $btns.find('#btnFONT_SMALLER').button().unbind('click').click(events.OnFontSmaller).hide();
            $btns.find('#btnFONT_SETTINGS').button().unbind('click').click(events.OnFontSettings).hide();

            $btns.find('#btnAUDIT_LOG').button().unbind('click').click(events.OnViewAuditLog);

            $btns.find("#btnSAVE").unbind('click').click(events.OnSave);

            var what = selftv.drawing.curr().TemplateType().toLowerCase();
            var typ = what.toUpperCase().replace(/[ -]/g, '');
            placeholder = 'Search for equipment to add to ' + what + '...';
            $('.cad-equip-search').attr('placeholder', placeholder);

            $btns.find('.btn-enabled').removeClass('btn-enabled');

            selftv.clearTrailPoints(); 
        }

        TemplateViewer.prototype.onAddSelectedEquip = function () {
            var $featTyp = selftv.$container.find('#idBayChoices').find('option:selected');
            var $equipChoice = selftv.$container.find('#idChildChoice').find('option:selected');

            if ($featTyp.length === 0 || $featTyp.val() === '0') {
                selftv.showMessage('Please select an equipment type choice!');
                return;
            }
            if ($equipChoice.length === 0 || $equipChoice.val() === '0') {
                selftv.showMessage('Please select an equipment from the choice list!');
                return;
            }


            var tmpltId = $equipChoice.val();
            var ft = $featTyp.text().replace('/[ -]/g', '').toUpperCase();
            try {
                var load = function (rec) {
                    $.extend(rec, { FEAT_TYP_ID: $featTyp.val(), FEAT_TYP: ft, ID: $equipChoice.val(), LABEL: $equipChoice.text() });
                    //selftv.cad.addEquipToContainer({ FEAT_TYP_ID: $featTyp.val(), FEAT_TYP: ft, ID: $equipChoice.val(), TEXT: $equipChoice.text() });
                    selftv.cad.addEquipToContainer(rec);
                }
                var showError = function (msg) {
                    app.showMessage(msg);
                }
                var finished = function () {
                    selftv.showWheelOfPatience(false);
                }

                selftv.showWheelOfPatience(true);
                http.get('api/tmplt/get' + ft + 'Tmplt/' + tmpltId).done(load).fail(showError).done(finished);
            }
            catch (err) {
                console.error(err);
                selftv.showMessage(err);
            }
        }
        TemplateViewer.prototype.initEquipOptionsFilter = function () {
            var data = selftv.drawing.curr();
            var typ = data.TemplateType().toUpperCase().replace(/[ -]/g, '');

            var defList = { options: [], route: 'api/tmplt/getEquipChoices' };
            var map = {
                BAY: { options: (data.BayChoices ? data.BayChoices.optionVals() : []), route: 'api/tmplt/getBayChoices' }
                //  , NODE   : { options: data.NodeChoices.optionVals()    , route: 'api/tmplt/getNodeChoices'     }
                //  , SHELF  : { options: data.ShelfChoices.optionVals()   , route: 'api/tmplt/getShelfChoices'    }
                //  , CARD   : { options: data.CardChoices.optionVals()    , route: 'api/tmplt/getCardChoices'     }
                //  , PLUGIN : { options: data.PlugInChoices.optionVals()  , route: 'api/tmplt/getPlugInChoices'   }
            };

            var choices = (map[typ] || defList);
            var $c = selftv.$container.find('#idBayChoices').empty();

            choices.options.forEach(function (item) {
                $('<option>').val(item.value()).text(item.text()).appendTo($c);
            })
            $c.unbind('change').change(function () {
                var $opt = $(this).find('option:selected');
                selftv.onFetchEquipChoices($opt, choices.route);
            });
        }
        TemplateViewer.prototype.onFetchEquipChoices = function ($opt, route) {
            var curr = selftv.drawing.curr();
            var orig = selftv.drawing.orig();

            var featTypId = $opt.val();  // based on the value, populate the child choices

            var $equipChoice = selftv.$container.find('#idChildChoice').empty();
            if (featTypId === '0') {
                // disable add selection button
                // old code: selftvTmplt.BayChildChoices([]);
                // old code: selftvTmplt.BaySelectionButton(false);
                return;
            }

            var tmpltId = curr.tmpltId();

            var BAY_EXTENDER_TYP_ID = '1';
            var bayExtSpecRevAltId = 0;
            if (featTypId === BAY_EXTENDER_TYP_ID) {
                if (orig.BayExtSpecRevAltId() && typeof orig.BayExtSpecRevAltId() !== 'undefined') {
                    bayExtSpecRevAltId = orig.BayExtSpecRevAltId();
                }
                if (curr.BayExtSpecRevAltId() && typeof curr.BayExtSpecRevAltId() !== 'undefined') {
                    var tempId = curr.BayExtSpecRevAltId();
                    if (tempId !== '0') {
                        bayExtSpecRevAltId = tempId;
                    }
                }
            }

            var showError = function (response) {
                selftv.showMessage('Error attempting to find suitable Bay Choices:\n' + JSON.stringify(response));
            }

            var loadChoices = function (data) {
                if (data === selftv.EMTPY_SET) {
                    return;
                }

                var list = JSON.parse(data);
                list.forEach(function (item) {
                    var av = item.split('~');
                    $('<option>').val(av[0]).text(av[1]).appendTo($equipChoice);
                });
                $equipChoice.unbind('change').change(function () {
                    selftv.onEquipChoiceChanged($(this));
                })

                if (featTypId === BAY_EXTENDER_TYP_ID) {
                    curr.BayExtSpecRevAltId(bayExtSpecRevAltId);
                }

                $('#btnADD_SELECTED_EQUIP').unbind('click').click(function () {
                    selftv.onAddSelectedEquip();
                })
            }

            selftv.showWheelOfPatience(true);
            var json = { feattypid: featTypId, templateid: tmpltId };
            $.ajax({
                type: 'GET',
                url: route,
                data: json,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                //success: getBayChoiceSuccess,
                //error: getBayChoiceError,
                context: selftv,
                async: true
            })
                .done(loadChoices)
                .fail(showError)
                .complete(function () {
                    selftv.showWheelOfPatience(false);
                });
        }
        TemplateViewer.prototype.onEquipChoiceChanged = function ($choices) {
        }
       
        TemplateViewer.prototype.onClickAddRectangle = function () {
            var left = 10;
            var top = 15;
            var w = 30;
            var h = 50;
            var ru = { id: 99, name: 'testRect', left: left + 300 + 1, top: top, width: w, height: h, fill: 'red', strokeWidth: .5, stroke: 'rgba(0,0,0,1)' };

            var rect = selftv.cad.addRectangle(ru, true);

            rect.canDrag(true);
        };

        TemplateViewer.prototype.onClickCopy = function () {
            selftv.canvas.getActiveObject().clone(function (cloned) {
                selftv.clipboard = cloned;
            });
        };

        TemplateViewer.prototype.onClickPaste = function () {
            selftv.clipboard.clone(function (clonedObj) {
                selftv.canvas.discardActiveObject();
                clonedObj.set({
                    left: clonedObj.left + 10,
                    top: clonedObj.top + 10,
                    evented: true,
                });
                if (clonedObj.type === 'activeSelection') {
                    // active selection needs a reference to the canvas.
                    clonedObj.canvas = selftv.canvas;
                    clonedObj.forEachObject(function (obj) {
                        selftv.canvas.add(obj);
                    });
                    // this should solve the unselectability
                    clonedObj.setCoords();
                } else {
                    selftv.canvas.add(clonedObj);
                }
                selftv.clipboard.top += 10;
                selftv.clipboard.left += 10;
                selftv.canvas.setActiveObject(clonedObj);
                selftv.canvas.requestRenderAll();
            });
        };

        TemplateViewer.prototype.onClickAddCircle = function () {
            var ellipse = {
                top: 30,
                left: 30,
                rx: 50,
                ry: 50,
                fill: '',
                radius: 30,
                transparentCorners: true,
                hasBorders: true, strokeWidth: .5, stroke: 'rgba(0,0,0,1)',
                hasControls: true
            };

            var circleVal = selftv.cad.addCircle(ellipse, true);

            circleVal.canDrag(true);

        };


        TemplateViewer.prototype.onClickSave = function () {
            console.log(JSON.stringify(selftv.canvas));

            //
            // find the label attribues, in the real world we would have tagged them with the equip's TMPLT_DEF_ID 
            // in this example, the label has an 'equip' attribute that links them and the 'equip' has a 'label' attribute as well...
            // 
            var labels = selftv.canvas.getObjects().filter(function (o) {
                return o.equip != undefined;
            });

            // if( console.table ) { console.table(labels); } // debug raw data....

            var data = [];
            labels.forEach(function (l) {
                var attrs = {
                    top: l.top, left: l.left
                    , height: l.height, width: l.width
                    , fontFamily: l.fontFamily, fontSize: l.fontSize
                    , fill: l.fill, backgroundColor: l.backgroundColor
                };
                var item = {
                    TMPLT_DEF_ID: 0 // would know this in real life or would be able to get one for a new one
                    , CAD_ATTR_ID: 0 // would have it or generate one on save
                    , CAD_ATTR_TYP: l.equipType
                    , JSON: JSON.stringify(attrs)
                };
                data.push(item);
            });

            if( console.table ) { console.table(data);  } // debug 

            searchJSON = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: 'api/cad/InsertCadAttributes',
                data: searchJSON, contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: cadAttrDataSuccess,
                error: cadAttrDataError,
                context: document.body
            });
            function cadAttrDataSuccess(response) {
                if (response == "") {
                    //selftv.showMessage("No data found", "Template Viewer");
                    //$("#interstitial").hide();
                    alert(response,"Response ")
                    return;
                }
                alert(response, "Response ")
                console.log("response" + response);
                //$("#interstitial").hide();
                //return selftv.showMessage(response, 'Template Viewer');
            }
            function cadAttrDataError(err) {
                //$("#interstitial").hide();
                //return selftv.showMessage('Unable cad attributes. If problem persists please contact your system administrator.', 'Template Viewer');
                alert("error" + JSON.stringify(err))                
                console.log("error  : " + JSON.stringify(err));
            }

        };

        TemplateViewer.prototype.onClickAddtext = function () {
            selftv.resetAll();
            var textAttr = {
                id: 100, top: Math.floor(Math.random() * 350 + 1),
                left: Math.floor(Math.random() * 250 + 1), width: 100, fontSize: 20, fontFamily: 'Arial',
                backgroundColor: '', editingBorderColor: 'blue', borderColor: 'black', textboxBorderColor: 'black',
                fill: 'blue', showTextBoxBorder: true
            };
            var textVal = selftv.cad.addText('New text value', textAttr, true);
            textVal.canDrag(true);
        };

        TemplateViewer.prototype.onClickAddline = function () {
            selftv.resetAll();

            var line = new fabricjs.Line([50, 90, 200, 200], {
                left: Math.floor(Math.random() * 250 + 1),
                top: Math.floor(Math.random() * 350 + 1),
                stroke: 'black'
            });

            line.triangle = new fabricjs.Triangle({
                'left': 170, 'top': 150,
                'angle': 45
            });

            selftv.canvas.add(line);
            selftv.canvas.setActiveObject(line);
        };

        TemplateViewer.prototype.resetAll = function () {
            selftv.canvas.off('mouse:down');
            selftv.canvas.off('mouse:move');
            selftv.canvas.off('mouse:up');
            selftv.canvas.off('object:moving');
        };

        TemplateViewer.prototype.onClickDelete = function () {
        };

        return TemplateViewer;
    });