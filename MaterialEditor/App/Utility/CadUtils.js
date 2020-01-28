define(['jquery', 'fabricjs'], function ($, fabricjs) {
    var RackUnitUtils = function (rackUnits) {
        this._rackUnits = rackUnits.filter(function(i) { 
            return i.rackUnit > 0 
        });
    }
    RackUnitUtils.prototype.rackUnits = function (list) {
        if (list) {
            this._rackUnits = list;
        }
        return this._rackUnits;
    }
    RackUnitUtils.prototype.findRU = function(unit) {
        var ru = this._rackUnits.filter(function(o) { return o.rackUnit == unit; });
        return (ru.length > 0 ? ru[0] : null);
    }
    RackUnitUtils.prototype.getRackUnitAtPoint = function (pt) {
        var ru = this._rackUnits.filter(function(u) {
            return u.containsPointEx(pt);
        });
        return (ru.length > 0 ? ru[0] : null);
        //var left = this._rackUnits.reduce(function (acc, o) {
        //    return (acc.left < o.left ? acc.left : o.left);
        //});
        //var right = this._rackUnits.reduce(function (acc, o) {
        //    return (acc.left+acc.width < o.left+acc.width ? acc.left : o.left)+o.width;
        //});
        //var top = this._rackUnits[len-1].top;
        //var bottom = this._rackUnits[0].bottom;

        //if( pt.x < left || pt.x > right || pt.y < top || pt.y > bottom ) {
        //    return null; 
        //}

        //var units = this._rackUnits;
        //var len = units.length;
        //for (var i = 0; i < len - 1; i++) {
        //    var curr = units[i];
        //    var next = units[i+1];

        //    var max = curr.top;
        //    var min = next.top+next.height; 

        //    console.log('RackUnitUtils.prototype.getRackUnitAtPoint() ru %s - %s > %s < %s', curr.rackUnit, max, min);
        //    if( max > pt.y && min < pt.y ) {
        //        return curr;
        //    }
        //}
        
        return null;
    }
    RackUnitUtils.prototype.getRackUnitLoc = function(o) {
        var bot = { x: o.left, y: o.top+o.height };
        var ru = this.findRackUnitByPoint(bot);
        return ru||{ name: '???', rackUnit: '?'};
    }
    RackUnitUtils.prototype.findRackUnitRange = function(o) {
        var top = { x: o.left, y: o.top };
        var bot = { x: o.left, y: o.top+o.height };
        var end = this.findRackUnitByPoint(top);
        var start = this.findRackUnitByPoint(bot);
        var range = [];
        if (!start && !end ) {
            return [];
        }
        if (!start && end) {
            return [end];
        }
        if (!end && start) {
            return [start];
        }

        for (var i = start.rackUnit; i <= end.rackUnit; i++) {
            range.push( this.findRU(i) );
        }
        return range;
    }
    RackUnitUtils.prototype.findRackUnitByPoint = function(pt) {
        var ru = this._rackUnits.filter(function(o) { 
            var lhs = o.labels.lhs;
            var rhs = o.labels.rhs;

            var tl = { x: lhs.left, y: lhs.top };
            var bl = { x: lhs.left, y: lhs.top+lhs.height };
            var tr = { x: rhs.left, y: rhs.top };
            var br = { x: rhs.left, y: rhs.top+rhs.height };
    
            var x = (pt.x >= tl.x && pt.x <= tr.x)
            var y = (pt.y >= tl.y && pt.y <= br.y);

            var rv = (x && y);
            return rv;
        });
        if (ru.length === 0) {
            return null;
        }
        return ru[0];
    }
    
    var CadUtils = function () {
        this._canvas = null;
        this._rackUnitMgr = null;
    }

    CadUtils.prototype.ACTIVE_SELECTION = 'activeSelection';
    CadUtils.prototype.UITYP_TRAILPOINT = 'TRAIL_POINT';

    CadUtils.prototype.EQTYP_RU_POS = 'RU_POS';
    CadUtils.prototype.EQTYP_LABEL_RU= 'LABEL_RU';
    CadUtils.prototype.EQTYP_LABEL_BAYNAME = 'LABEL_BAYNAME';
    CadUtils.prototype.EQTYP_LABEL = 'LABEL';
    CadUtils.prototype.EQTYP_SHELF = 'SHELF';
    CadUtils.prototype.EQTYP_NODE = 'NODE';
    CadUtils.prototype.EQTYP_SLOT = 'SLOT';
    CadUtils.prototype.EQTYP_PORT = 'PORT';
    CadUtils.prototype.EQTYP_BAY_INTERNAL = 'BAY_ITNL';
    CadUtils.prototype.EQTYP_BAY_EXTERNAL = 'BAY_XTNL';

    CadUtils.prototype.COLOR_TRANSPARENT = 'rgba(0,0,0,0)';
    CadUtils.prototype.BODY = 'body';
    CadUtils.prototype.DEFAULT_SCALE = .1;
    CadUtils.prototype.DEFAULT_TOP_POSITION_FOR_BAY_ON_CANVAS = 100; // arbitrary...
    CadUtils.prototype.DEFAULT_LEFT_POSITION_FOR_BAY_ON_CANVAS = 100;
    CadUtils.prototype.LABEL_OFFSET_RHS_BAY = 0; // right hand side offset when setting labels around bay...arbitrary 
    CadUtils.prototype.LABEL_OFFSET_LHS_BAY = 0; // left hand side offset when setting labels around bay..."

    CadUtils.prototype.CtrlKeyActive = { CTRL: false, SHIFT: false, ALT: false };

    CadUtils.prototype.Canvas = function (canvas) {
        if (canvas) {
            this._canvas = canvas;
        }
        return this._canvas;
    }
    CadUtils.prototype.toInches = function (sz) {
        sz = (sz||"0").toString();
        if (sz.indexOf('in') == -1) {
            sz += "in";
        }
        return fabric.util.parseUnit(sz);
    }
     CadUtils.prototype.toDefaultScaledInches = function (sz) {
        return this.toInches(sz) * this.DEFAULT_SCALE;
    }
    CadUtils.prototype.setRackUnits = function (ruList) {
        this._rackUnitMgr = new RackUnitUtils(ruList);
    }
    CadUtils.prototype.findRU = function (ru) {
        var ru = this._rackUnitMgr.findRU(ru);
        return ru;
    }
    CadUtils.prototype.getRackUnitAtPoint = function (pt) {
        var pos = { x: pt.x||pt.left+(pt.width/2)
                  , y: pt.y||pt.top+(pt.height/2) 
                  };

        return this._rackUnitMgr.getRackUnitAtPoint(pos);
    }
    CadUtils.prototype.snapToNearestRU = function (equip) {
        var pos = {x: equip.left, y: equip.top+equip.height};
        var ru = this.getRackUnitAtPoint(pos);
        if (!ru) {
            if( equip.RU ) { 
                ru = this.findRU(equip.RU);
                if (!ru) {
                    return null;
                }
            }
            else {
                return null;
            }
        }
        equip.set({ top: ru.top-equip.height}).setCoords();
        return ru;
    }
    CadUtils.prototype.alignLeft = function(o, container) {
        container = (container||o.container);

        var calcPos = { 0: function(o, container) { return container.left; }
                      , 90: function(o, container) { return container.left+(o.height*o.scaleY); }
                      , 180:  function(o, container) { return container.left+(o.width*o.scaleX); }
                      , 270:  function(o, container) { return container.left; }

        }[o.angle];
    
        var padding = function (o, container) {
            return +(container.strokeWidth||1)+(o.strokeWidth||1);
        }
        this.alignTo({ o: o, align: 'left', calcPos: calcPos, padding: padding });
    }
    CadUtils.prototype.alignRight = function(o, container) {
         var calcPos = { 0: function(o, container) { return container.left+container.width-(o.width*o.scaleX); }
                      , 90: function(o, container) { return container.left+container.width; }
                      , 180:  function(o, container) { return container.left+container.width; }
                      , 270:  function(o, container) { return container.left+container.width-(o.height*o.scaleX); }

        }[o.angle];
    
        var padding = function (o, container) {
            return -(container.strokeWidth||1)+(o.strokeWidth||1);
        }
        this.alignTo({ o: o, align: 'right', calcPos: calcPos, padding: padding });
    }
    CadUtils.prototype.alignCenter = function(o, container) {
        container = (container||o.container);

        var calcPos =  {   0: function(o, container) { return container.left+(container.width/2)-((o.width*o.scaleX)/2); }
                         ,  90: function(o, container) { return container.left+(container.width/2)+((o.height*o.scaleY) /2); }
                         ,  901: function(o, container) { 
                             var contMidLeft = container.left+(container.width/2);
                             var itemWidth = (o.height*o.scaleY)/2;
                             var itemLeft = contMidLeft+itemWidth;
                             //return container.left+(container.height/2)-((o.height*o.scaleY) /2); 
                             return itemLeft;
                         }
                         , 180: function(o, container) { return container.left+(container.width/2)+((o.width*o.scaleX)/2); }
                         //, 270: function(o, container) { return container.left+(container.height/2)-((o.height*o.scaleY) /2); }
                         , 270: function(o, container) { 
                             var contMidLeft = container.left+(container.width/2);
                             var itemWidth = ((o.height*o.scaleY)/2);
                             var itemLeft = contMidLeft-itemWidth;
                             return itemLeft;
                         }
        }[o.angle];

        var padding = function (o, container) {
            return +(container.strokeWidth||1)+(o.strokeWidth||1);
        }
        this.alignTo({ o: o, align: 'center', calcPos: calcPos, padding: padding });
    }
    CadUtils.prototype._createDataStructureForAlignment = function(o) {
        var set = [];
        var container = o.container;

        if ( o.get('type') === this.ACTIVE_SELECTION ) {
            if (this.CtrlKeyActive.SHIFT === false) {
                container = o.getObjects()[0].container;
                set.push(o);
            }
            else { 
                o.forEachObject(function (i) {
                    set.push(i);
                });
                container = { left: o.left, top: o.top, width: o.width, height: o.height, strokeWidth: (o.strokeWidth||1) };
            }
        }
        else {
            set.push(o);
        }

        return { set: set, container: container };
    }
    CadUtils.prototype.getMainContainer = function() {
        var main = this._canvas.getObjects().filter(function(o) { return o.isMainContainer === true; });
        if (main.length > 0) {
            return main[0];
        }
        throw '"main" container not found';
        return null;
    }
    CadUtils.prototype.updateBinding = function() {
        var main = this.getMainContainer();
        if (!main) {
            return;
        }
        var children = this._canvas.getObjects().filter(function(o) { return o !== main; });
        main.children = children;

        var mainTransform = main.calcTransformMatrix();
        var invertedMainTransform = fabric.util.invertTransform(mainTransform);
        children.forEach( function(o) {
            var desiredTransform = fabric.util.multiplyTransformMatrices(
                invertedMainTransform,
                o.calcTransformMatrix()
            );
            // save the desired relation here.
            o.relationship = desiredTransform;
            o.container = main;
        });
    }
    CadUtils.prototype.unbindObjects = function(e) {
        var main = this.getMainContainer();
        var children = this._canvas.getObjects().filter(function(o) { return o !== main; });
        children.forEach(function(o) {
            delete o.relationship;
        });
        main.children = [];
    }
    CadUtils.prototype.getCoords = function(o) {
        if (!o) {
            return null;
        }
        var top = o.top;
        var bottom = top + o.height;
        var left = o.left;
        var right = left + o.width;

        o.bottom = o.top+o.height;
        o.right = o.left+o.width;

        return { top: top, left: left, bottom: bottom, right: right, angle: o.angle };
    }
    CadUtils.prototype.constrainTo = function( o, container ) {
        var angle = o.get('angle');
        var oMoved= this.getCoords(o);
        if (!oMoved) {
            return;
        }
        var oBounding = this.getCoords(container);
        if (!oBounding) {
            return;
        }

        var padding = 0;//(container.strokeWidth+(o.strokeWidth+1));

        var left, top;
        switch( angle ) {
            case 0: 
                left = Math.min(Math.max(oBounding.left+padding, oMoved.left), (oBounding.right-padding)-o.width);
                top =  Math.min(Math.max(oBounding.top+padding, oMoved.top), (oBounding.bottom-padding)-o.height);
                break;
            case 90:
                left = Math.min(Math.max(oBounding.left+padding+o.height, oMoved.left), oBounding.right);
                top =  Math.min(Math.max(oBounding.top+padding, oMoved.top), oBounding.bottom-padding - o.width);
                break;
            case 180:
                left = Math.min(Math.max(oBounding.left+padding+o.width, oMoved.left), oBounding.right-(padding/2));
                top =  Math.min(Math.max(oBounding.top+padding+o.height, oMoved.top), oBounding.bottom-(padding/2));
                break;
            case 270:
                left = Math.min(Math.max(oBounding.left+padding, oMoved.left), oBounding.right-padding-o.height);
                top =  Math.min(Math.max(oBounding.top+padding+o.width, oMoved.top), oBounding.bottom-padding);
                break;
        }

        this.unbindObjects();
        o.set('left',left);
        o.set('top',top);
        o.setCoords();
        this.updateBinding();
    }
    CadUtils.prototype.alignTo = function( args ) {
        var align = args.align;
        align = ( align === 'bottom' ? 'top'
                : align === 'right' ? 'left'
                : align === 'middle' ? 'top'
                : align === 'center' ? 'left'
                : align
        );

        var canvas = (args.o.canvas);
        var data = this._createDataStructureForAlignment(args.o);
        if (data.set.length > 1) {
            // we have to "let go" of the group so that it 
            // will align properly within to the specified "container";
            // we'll reselect it after we are done.
            canvas.discardActiveObject();
        }

        var calc = (args.calcPos||function() { return null; });
        var padding = (args.padding||function() { return 0; });
        var container = data.container;
        var self = this;
        console.log('alignTo: %s', container.name||'no-name');
        data.set.forEach(function(o) { 
            var pos = calc(o, container);
            pos += padding(o,container);

            self.unbindObjects({ target: o });
            o.set(align, pos).setCoords().canvas.renderAll();
            self.constrainTo(o, container); // what if they put a label outside of the container...
            o.LastAuditMsg = 'aligned '+align;
        });
        this.updateBinding();

        if (data.set.length > 1) {
            canvas.setActiveObject(new fabric.ActiveSelection(data.set, { canvas: canvas }));
        }
        canvas.requestRenderAll();
    }

    CadUtils.prototype.createLabelFromCadAttr = function (cadAttr) {
        var data = JSON.copy(cadAttr);
        var fabjs = JSON.parse(data.JSON_DATA);
        delete data.JSON_DATA;
        $.extend(fabjs, data); // add CAD_ATTR columns;

        var o = new fabricjs.IText(fabjs.text, fabjs);
        o.snapAngle = 45;

        return o;
    }
     CadUtils.prototype.createPlainLabel = function(props) {
        var ruUtils = this._rackUnitMgr;
        if( !ruUtils && props.rackUnits ) {
            ruUtils = new RackUnitUtils(props.rackUnits);
        }

        var pos = (props.ru !== undefined ? ruUtils.findRU(props.ru) : props.pos);
        
        var ref = (props.container||pos);

        var fontSize = (props.fontSize||22);
        var h = (props.height||pos.height);
        var t = (props.top||pos.top);
        var l = (props.left||ref.left);

        var opts = { top: t, left: l };

        var offset = (props.offset||{ left: 0, top: 0 });
        opts.top += (offset.top||0);
        opts.left += (offset.left||0);

        opts.fontFamily='Consolas';
        opts.fontSize=(props.fontSize||22);
        opts.fontWeight='normal';
        opts.fill = 'green';
        opts.backgroundColor = (props.backgroundColor||this.COLOR_TRANSPARENT);
        
        var label = new fabric.IText(props.text, opts);
        label.container = props.container;
        label.equipType = this.EQTYP_LABEL;
        
        if (props.rotate) {
            label.rotate(props.rotate);
        }
        
        label.snapAngle = 45;

        label.ru = props.ru; // will either be 'undefined' or an 'ru'
        label.name = props.text;

        var equip = this._canvas.getObjects().filter(function (sh) {
            return sh.equipType 
                    && sh.equipType === this.EQTYP_SHELF 
                    && sh.name 
                    && sh.name === props.text
            ;
        });
        if (equip.length > 0) {
            equip = equip[0];
            console.log('label found: %s', equip.name);
            label.equip = equip;
            equip.label = label;
        }
        return label;
    }
    CadUtils.prototype.createLabel = function(props) {
        var ruUtils = this._rackUnitMgr;
        if( !ruUtils && props.rackUnits ) {
            ruUtils = new RackUnitUtils(props.rackUnits);
        }

        var pos = (props.ru !== undefined ? ruUtils.findRU(props.ru) : props.pos);
        
        var ref = (props.container||pos);

        var fontSize = (props.fontSize||22);
        var h = (props.height||pos.height);
        var t = (props.top||(pos.top-(h/2)-(fontSize/2)));
        var l = (props.left||ref.left+ref.width);

        var opts = { top: t, left: l };

        var offset = (props.offset||{ left: 0, top: 0 });
        opts.top += (offset.top||0);
        opts.left += (offset.left||0);

        opts.fontFamily='Consolas';
        opts.fontSize=(props.fontSize||22);
        opts.fontWeight='normal';
        opts.fill = 'green';
        opts.backgroundColor = (props.backgroundColor||this.COLOR_TRANSPARENT);
        
        var label = new fabric.IText(props.text, opts);
        label.container = props.container;
        label.equipType = this.EQTYP_LABEL;
        
        if (props.rotate) {
            label.rotate(props.rotate);
        }
        
        label.snapAngle = 45;

        label.ru = props.ru; // will either be 'undefined' or an 'ru'
        label.name = props.text;

        var equip = this._canvas.getObjects().filter(function (sh) {
            return sh.equipType 
                    && sh.equipType === this.EQTYP_SHELF 
                    && sh.name 
                    && sh.name === props.text
            ;
        });
        if (equip.length > 0) {
            equip = equip[0];
            console.log('label found: %s', equip.name);
            label.equip = equip;
            equip.label = label;
        }
        return label;
    }
   
    CadUtils.prototype.setEventHandler = function(eventName, eventProc, objects) {
        objects.forEach(function (o) {
            if (!o) {
                return;
            }

            switch( eventName )
            {
                case 'selected':
                    o.on(eventName, function (e) {
                        e.target = o;
                        eventProc(e);
                    })
                    break;
                case 'deselected':
                    o.on(eventName, function (e) {
                        e.target = o;
                        eventProc(e);
                    })
                    break;
                default:
                    o.on(eventName, eventProc);
                    break;
            }
        
        })
    }
    CadUtils.prototype.setupDefaultEventHandlersForObject = function(obj, events) {
        var defHandler = function() { };
        this.setEventHandler('rotated'      , events.onItemRotated   || defHandler    , [obj]);
        this.setEventHandler('moving'       , events.onItemMoving    || defHandler    , [obj]);
        this.setEventHandler('modified'     , events.onItemModified  || defHandler    , [obj]);
        this.setEventHandler('scaling'      , events.onItemScaling   || defHandler    , [obj]);
        this.setEventHandler('selected'     , events.onItemSelected  || defHandler    , [obj]);
        this.setEventHandler('deselected'   , events.onItemDeselected|| defHandler    , [obj]);
    }
   
    CadUtils.prototype.applyContainerBindings = function(container, items) {
        if (!this._canvas) {
            return;
        }
        if (!container) {
            return;
        }
        items = (items||this._canvas.getObjects());

        var children = items.filter(function(o) { return o !== container});
        container.children = children;

        var containerTransform = container.calcTransformMatrix();
        var invertedMainTransform = fabric.util.invertTransform(containerTransform);
        children.forEach(function(o) {
            var desiredTransform = fabric.util.multiplyTransformMatrices(invertedMainTransform, o.calcTransformMatrix());
            // save the desired relation here.
            o.relationship = desiredTransform;
            o.container = container;
            o.parent = container;
        });
        return container;
    }
    CadUtils.prototype.releaseContainerBindings = function(container) {
        if (!this._canvas) {
            return;
        }
        if (!container) {
            return;
        }

        var children = (container.children||[]);
        children.forEach(function(o) {
            delete o.relationship;
        });
        
        container.children.length = 0;
        container.children = [];

        return container;
    }
    CadUtils.prototype.refreshContainerItems = function(container) {
        var canvas = this._canvas;
        var children = (container.children||canvas.getObjects().filter(function(o) { return o !== container }));
        children.forEach( function(o) {
            if (!o.relationship) {
                return;
            }
            var relationship = o.relationship;
            var newTransform = fabric.util.multiplyTransformMatrices(container.calcTransformMatrix(),relationship);
            var opt = fabric.util.qrDecompose(newTransform);
    
            o.set({ flipX: false, flipY: false });
            o.setPositionByOrigin({ x: opt.translateX, y: opt.translateY }, 'center', 'center' );
            o.set(opt);
            o.setCoords();
        });
    }
    CadUtils.prototype.refreshTaggedItems = function() {
        if (this.CtrlKeyActive.CTRL === true) {
            console.log('CadUtils.prototype.refreshTaggedItems() - ignored due to CTRL pressed (easy to forget)');
            return;
        }
        var canvas = this._canvas;
        var tagged = canvas.getObjects().filter(function(o) { 
            return o.taggedItems != null && o.isMoving === true;
        });
        tagged.forEach(function (main) {
            main.taggedItems.forEach(function (i) {
                var relationship = i.taggedRelationship;
                if (!relationship) {
                    return;
                }
                var transform = fabric.util.multiplyTransformMatrices(main.calcTransformMatrix(),relationship);
                var opt = fabric.util.qrDecompose(transform);
                opt.scaleX = i.scaleX;
                opt.scaleY = i.scaleY;
                i.set({ flipX: false, flipY: false });
                i.setPositionByOrigin({ x: opt.translateX, y: opt.translateY }, 'center', 'center' );
                i.set(opt);
                i.setCoords(); 
            });
        });
    }
    
    return new CadUtils();
});
