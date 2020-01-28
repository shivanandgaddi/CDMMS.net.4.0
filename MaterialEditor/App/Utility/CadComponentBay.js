
define(['jquery', 'fabricjs', '../Utility/CadUtils', '../Utility/RackUnitLabel', '../Utility/CadComponentShelf']
    , function ($, fabricjs, CadUtils, RackUnitLabel, ShelfMgr) {
        console.log('CadComponentBay: created');
    var CadComponentBay = function() {
        this._data = null;
        this._cad = null;
        this._bay = null;
        this._canvas = null;
        this._attrs = null;
        this._rackUnits = null;
        this._bayExternal = null;
        this._items = [];
        this._typ = 'BAY';
    }
    
    CadComponentBay.prototype.createRackUnits = function(props) {
        
        var rackUnits = [];
        var items = [];

        var zFactor = .1;
        
        var rackUnitHeight = CadUtils.toDefaultScaledInches(props.height); 
        var h = rackUnitHeight;
        var w = props.ref.width;
        
        var odd = 'black';
        var even = 'black';
        var color = null;
        
        var opts = null;
        var padding= h+1.5;

        var t = props.ref.top; //fabric.util.parseUnit('2in')*zFactor;
        var l = props.ref.left-padding;
        
        
        var lz = function(x) { return x < 10 ? '0'+x : x; }

        var ru = (props.count||40);
        var peer = null;
        while( ru > 0 ) {
            color = ((ru%2)==0 ? even : odd);

            opts = { label: lz(ru)+':', top: t, left: l-2, height: h, width: padding, strokeWidth: .25, stroke: 'white', fill: 'white' };
            var lhs = new RackUnitLabel(opts);
            lhs.selectable = false;
            lhs.opts = JSON.copy(opts);
            lhs.hilite = function (e) { this.set({ fill: 'whitesmoke' }); }
            lhs.unhilite = function() { this.set({ fill: 'white' }); }
            lhs.equipType = CadUtils.EQTYP_RU_LABEL;
            lhs.side = 'LHS';
            lhs.ru = ru;
            items.push(lhs);

            opts = { label: ':'+lz(ru), top: t, left: l+w+padding, height: h, width: padding, strokeWidth: .25, stroke: 'white', fill: 'white' };
            var rhs = new RackUnitLabel(opts);
            rhs.selectable = false;
            rhs.opts = JSON.copy(opts);
            rhs.hilite = function (e) { this.set({ fill: 'whitesmoke' }); }
            rhs.unhilite = function() { this.set({ fill: 'white' }); }
            rhs.equipType = CadUtils.EQTYP_RU_LABEL;
            rhs.side = 'RHS';
            rhs.ru = ru;
            items.push(rhs);

            var r = new fabric.Line([(l+padding)+1, t+padding, l+w+padding,  t+h], { strokeWidth: .25, stroke: 'grey', fill: 'white' })
            r.selectable = false;
            r.name = 'ru'+lz(ru);
            r.rackUnit = ru;
            r.rackUnitHeight = h;
            r.actualHeight = props.height;
            r.equipType = CadUtils.EQTYP_RU_POS;
            
            r.labels = { lhs: lhs, rhs: rhs };
            r.physLoc = { left: (l+padding)+1, top: t-rackUnitHeight, right: l+w+padding,  bottom: t };
            r.containsPointEx = function (pt) {
                var lhs = this.left;
                var rhs = this.left+this.width;
                var top = this.top-(h*this.scaleY);
                var bot = this.top-this.height;

                var withinX = (pt.x >= lhs && pt.x <= rhs);
                var withinY = (pt.y >= top && pt.y <= bot);

                return withinX && withinY;
            }
            r.peer = peer;
            rackUnits.push(r);
            items.push(r);

            peer = r;
            t += h;
            ru--;
        }
        return items;
    }
    CadComponentBay.prototype.applyCustomAttributes = function (obj, attrs) {
        // 
        // this is a bit naive, will adjust as I understand more how it should work...
        //
        if (!attrs) {
            return obj;
        }

        var fabjs = JSON.parse(attrs.JSON_DATA);
        $.extend(obj, fabjs ); // overlay fabricjs attributes

        var cadAttr = JSON.copy(attrs);
        delete cadAttr.JSON_DATA;
            
        $.extend(obj, cadAttr ); // apply CAD_ATTR columns....

        return obj;
    }
    CadComponentBay.prototype.renderOtherItems = function ( items ) {
        var $this = this;
        var canvas = this._canvas;
        
        var notImplemented = function () {
            alert('CadComponentBay.prototype.renderOtherItems: '+item.CAD_ATTR_TYP+' not implemented yet for Bay');
        }
        var drawLabel = function (item) {
            var o = CadUtils.createLabelFromCadAttr(item);
            o.equipType = (item.CAD_ATTR_TYP||item.equipType||CadUtils.EQTYP_LABEL);
            canvas.add(o);
            console.log('CadComponentBay.prototype.renderOtherItems: canvas.add('+item.CAD_ATTR_TYP+')');
        }
        var drawShelf = function (item) {
            var data = JSON.parse(item.JSON_DATA);

            var ru = CadUtils.findRU(data.rec.MNTNG_POS||data.rec.RU);
            if (!ru) {
                console.log('WARNING: could not found mounting position for shelf %s', item.NAME);
                return;
            }
            var o = $this.AddShelf(data.rec, ru);
            $.extend(o.shelf, item);

             if (item.IS_EDITABLE === "0") {
                var sh = o.shelf;
                sh.selectable = true;
                sh.lockMovementX = true;
                sh.lockMovementY = true;
                sh.lockScalingX = true;
                sh.lockScalingY = true;
                sh.lockUniScaling = true;
                sh.lockRotation = true;
            }
            console.log('CadComponentBay.prototype.renderOtherItems: canvas.add('+item.CAD_ATTR_TYP+')');
        }
        var cat = { LABEL: drawLabel 
            , SHELF: drawShelf
        };
        items.forEach(function (i) {
            var proc = (cat[i.CAD_ATTR_TYP]||notImplemented);
            proc(i);
        });
        canvas.requestRenderAll();
    }
    CadComponentBay.prototype.setupNotificationReceivers = function () {
        var comp = this;

        if (comp.areNotificationReceiversSetup) {
            return;
        }

        comp._bayInternal.onMouseUpNotify = function (props) {
            var pt = props.point;
            console.log('CadComponentBay.prototype.onMouseUpNotify() %s,%s', pt.left, pt.top)
        }
        comp._bayInternal.onMouseDownNotify = function (props) {
            //
            // just an example as to how to find the RUs at a given point...
            //
            //var pt = props.point;
            //var ru = CadUtils.getRackUnitAtPoint(pt);
            //if (!ru) {
            //    return;
            //}
            //ru.set({backgroundColor: 'orange'});
            //ru.peer.set({backgroundColor: 'orange'});

            //console.log('CadComponentBay.prototype.onMouseDownNotify() - %s,%s', pt.x, pt.y);
        }
        comp._bayInternal.onMouseOverNotify = function (props) {
            var pt = props.point;
            console.log('CadComponentBay.prototype.onMouseOverNotify() %s,%s', pt.left, pt.top)
        }

        comp.areNotificationReceiversSetup = true;
    }
    
    CadComponentBay.prototype.condenseObject = function (obj) {
        for (var field in obj) {
            var f = obj[field];
            if (f.value !== undefined ) {
                obj[field] = f.value;
            }
        }
        return obj;
    }
        
    CadComponentBay.prototype.render = function() {
        var canvas = this._canvas;
        CadUtils.Canvas(canvas);

        this._items.length = 0;
        
        var bay = this._bay;
        var name = bay.Nm.value;
        var itnlId = bay.BayItnlId.value;
        var bayInternalOptions = bay.BayItnlLst.list;
        var internal = bayInternalOptions.find(function(i) { return i.id.value == itnlId; });

        var uom = (internal.BayItnlHghtUom.value||'??').toLowerCase() // assuming width is same uom
        if( uom !== 'in' ) {
            throw 'CadComponentBay.render: UOM "'+uom+'" not implmenented; cannot draw bay';
        }

        var ru = { count: parseInt(internal.MntngPosQty.value), height: internal.MntngPosDist.value };
        var dims =  { height: internal.BayItnlHght.value
                    , width: internal.BayItnlWdth.value
        };

        if (uom === "in") {
            dims.height = CadUtils.toDefaultScaledInches(dims.height);
            dims.width = CadUtils.toDefaultScaledInches(dims.width);
        }
        var top = CadUtils.DEFAULT_TOP_POSITION_FOR_BAY_ON_CANVAS;
        var left = CadUtils.DEFAULT_LEFT_POSITION_FOR_BAY_ON_CANVAS;

        var bayInternal = new fabric.Rect({ width: dims.width
                                   , height: dims.height
                                   , stroke: 'red'
                                   , strokeWidth: 1
                                   , fill: CadUtils.COLOR_TRANSPARENT
                                   , top: top
                                   , left: left
                                   
                                   , selectable: true 

                                   , lockMovementX: true
                                   , lockMovementY: true
                                   , lockScalingX: true
                                   , lockScalingY: true
                                   , lockUniScaling: true
                                   , lockRotation: true

                                   , actualHeight: dims.height
                                   , actualWidth: dims.width

                                   , equipType: CadUtils.EQTYP_BAY_INTERNAL
        });
        
        var offset = 50;
        var defOffset = CadUtils.toDefaultScaledInches("1.75in");
        var hExt = CadUtils.toDefaultScaledInches(bay.MntngPosOfst.value);
        if (hExt == 0) {
            hExt = defOffset;
        }

        var bayExternal = new fabric.Rect({ width: dims.width+(offset*2)
                                   , height: dims.height+(hExt*2)
                                   , stroke: 'blue'
                                   , strokeWidth: 1
                                   , fill: CadUtils.COLOR_TRANSPARENT
                                   , top: top-hExt // offset
                                   , left: left-offset
                                   
                                   , selectable: true
                                   
                                   , lockMovementX: true
                                   , lockMovementY: true
                                   , lockScalingX: true
                                   , lockScalingY: true
                                   , lockUniScaling: true
                                   , lockRotation: true

                                   , actualHeight: dims.height
                                   , actualWidth: dims.width
                                   , actualHeightWidthOffset: (offset*2)
                                   , equipType: CadUtils.EQTYP_BAY_EXTERNAL
                                   
        });

        canvas.add(bayExternal);
        
        var self = this;
        var ruList = this.createRackUnits({ ref: bayInternal, height: ru.height, count: ru.count });
        ruList.forEach(function (ru) {
            canvas.add(ru);
            var cat = (ru.rackUnit ? CadUtils.EQTYP_RU_POS : CadUtils.EQTYP_RU_LABEL);
            self._items.push({ cat: cat, o: ru});
            
        });
        CadUtils.setRackUnits(ruList);

        canvas.add(bayInternal);

        var bayName = CadUtils.createLabel({ text: name, ru: 1, rackUnits: ruList, container: bayExternal, backgroundColor: CadUtils.COLOR_TRANSPARENT  });
        bayName.equipType = CadUtils.EQTYP_LABEL_BAYNAME;
        bayName.rotate(-90);
        bayName.selectable = true;
        bayName.constrainToContainer = false;
        bayName.top  = bayExternal.top + bayExternal.height/2;
        bayName.left = bayExternal.left - bayName.height - 2;

        var bayNameProps = this._attrs.find(function(a) { return a.CAD_ATTR_TYP === CadUtils.EQTYP_LABEL_BAYNAME; });
        self.applyCustomAttributes(bayName, bayNameProps);

        canvas.add(bayName);
        canvas.renderAll();

        // mwj: i dont know if we need this yet, might throw this out...
        this._items.push({ cat: bayExternal.equipType, o: bayExternal });
        this._items.push({ cat: bayInternal.equipType, o: bayInternal });
        this._items.push({ cat: bayName.equipType, o: bayName });

        var children = this._items.map(function(o) { return o.o; });
        CadUtils.applyContainerBindings(bayExternal, children);

        this._bayExternal = bayExternal;
        this._bayInternal = bayInternal;

        this.setupDefaultEventHandlers();
        this.setupNotificationReceivers();

        
        this._bayInternal.isMainContainer = true;

        var labels = this._attrs.filter(function(a) { return a.CAD_ATTR_TYP === CadUtils.EQTYP_LABEL; });
        self.renderOtherItems(labels)

        var equip = this._attrs.filter(function(a) { return a.CAD_ATTR_TYP === CadUtils.EQTYP_SHELF; });
        self.renderOtherItems(equip)

        canvas.requestRenderAll();

        return this._bayInternal;
    }
    CadComponentBay.prototype.setupDefaultEventHandlers = function () {
        var self = this;
        var events = { onItemMoving: self.onItemMoving 
                     , onItemRotated: self.onDefaultEvent
                     , onItemModified: self.onDefaultEvent
                     , onItemScaling: self.onDefaultEvent
                     , onItemSelected: self.onDefaultEvent
                     , onItemDeselected: self.onDefaultEvent
        };
        CadUtils.setupDefaultEventHandlersForObject(this._bayExternal, events);
    }
    CadComponentBay.prototype.onDefaultEvent = function(e) {
    }
    CadComponentBay.prototype.onItemMoving = function(e) {
        console.log('CadComponentBay: onItemMoving %s', e.target.type);
        //DisplayLogMessage("");

        var o = e.target;
        var container = (o.container||o);
        o.isMoving = true;
        container.isMoving = true;

        //var type = o.get('type');
        //var ignore = (type === 'text' || type === 'i-text');
        //if( !ignore )  {
        //    constrainTo(o, container);
        //}

        //switch (o.equipType) {
        //    case EQTYP_NODE: 
        //    case EQTYP_SHELF:
        //        var pt = { x: o.left, y: o.top+o.height };
        //        var snapToRu = FindRackUnitByPoint(pt);
        //        if (snapToRu) {
        //            o.set({ top: snapToRu.top-o.height }).setCoords();
        //        }
        //        break;
        //}

        //CadUtils.refreshContainerBindings(container); 

        CadUtils.refreshContainerItems(container); //  move child items in sync with parent (container)
    }
    CadComponentBay.prototype.GetItems = function () {
        return this._items;
    }

    CadComponentBay.prototype.addBayExtender = function (data) {
        data.proc = 'CadComponentBay.prototype.addBayExtender';
        console.log(data)
    }
    
    CadComponentBay.prototype.addNode = function (data) {
        data.proc = 'CadComponentBay.prototype.addNode';
        console.log(data)
    }
    
    CadComponentBay.prototype.dbg = function (msg, data) {
        if( data && console.table ) {
            console.log('+++ %s:', msg);
            console.table(data)
            console.log('--- %s:', msg);

            return;
        }
        console.log('%s', msg);
    }
    CadComponentBay.prototype.addShelf = function (data) {
        this.dbg('CadComponentBay.prototype.addShelf', data);

        var container = this._bayInternal;

        data.MNTNG_POS = 1;
        data.RU = data.MNTNG_POS;

        data.container = container;
        
        //var obj = ShelfMgr.Create(data, this._bayExternal, this._bayInternal);

        var canvas = container.canvas;

        //if( obj.shelf ) {
        //    canvas.add(obj.shelf);
        //    CadUtils.alignCenter(obj.shelf);
        //}
        //if (obj.label && obj.label.name) {
        //    canvas.add(obj.label.name);
        //}
        //if( obj.label && obj.label.descr ){
        //    canvas.add(obj.label.descr);
        //}
        
        canvas.requestRenderAll();
    }

    CadComponentBay.prototype.addEquipToContainer = function (data) {
        var self = this;
        var notImplemented = function () {
            throw (data.FEAT_TYP+' not a valid equipment type for '+self._typ);
        }
        var proc = { BAYEXTENDER: this.addBayExtender
                   , NODE: this.addNode
                   , SHELF: this.addShelf
        }[data.FEAT_TYP];

        proc = (proc||notImplemented);

        return proc.call(self, data);
    }
    CadComponentBay.prototype.AddShelf = function (data, ru) {
        
        this.dbg('CadComponentBay.prototype.addShelf', data);

        var container = this._bayInternal;

        data.MNTNG_POS = ru.rackUnit;
        data.RU = ru.rackUnit

        data.container = container;
        
        var obj = ShelfMgr.Create(data, this._bayExternal, this._bayInternal);
        var canvas = container.canvas;
        if( obj.shelf ) {
            if (data.container) {
                delete data.container;
            }
            obj.shelf.rec = (data.rec ? JSON.copy(data.rec) : JSON.copy(data));
            canvas.add(obj.shelf);
            CadUtils.alignCenter(obj.shelf);
        }
        if (obj.shelf.label && obj.shelf.label.name) {
            canvas.add(obj.shelf.label.name);
        }
        if( obj.shelf.label && obj.shelf.label.descr ){
            canvas.add(obj.shelf.label.descr);
        }
        if (obj.shelf.adjustLabels) {
            setTimeout(function() { obj.shelf.adjustLabels(); }, 500);
        }
        canvas.requestRenderAll();
        return obj;
    }
    CadComponentBay.prototype.Display = function(props) {
        this._data = props;
        this._cad = props.cad;
        this._canvas = props.canvas;
        this._bay = props.obj.bay;
        this._attrs = props.attrs;
        
        var rv = this.render();

        return rv;
    }

    return new CadComponentBay();
});