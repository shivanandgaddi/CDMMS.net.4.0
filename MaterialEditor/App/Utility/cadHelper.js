if (!JSON.copy) {
    JSON.copy = function (json) {
        return JSON.parse(JSON.stringify(json));
    }
}


define(['jquery', 'fabricjs', '../Utility/CadUtils', '../Utility/CadComponentBay', '../Utility/CadComponentShelf', '../Utility/CadComponentNotImplemented']
, function ($, fabricjs
    , CadUtils
    , bayComponent
    , shelfComponent
    , notImplmentedComponent
) {
    //var CADHelper = function (drawingCanvas) {
    //    cad = this;

    //    cad.canvas = drawingCanvas;
    //    cad.itemCount = 0;
    //    //cad.additionalAttributes = "id,name".split(',');

    //    cad.canvas.on('mouse:over', function (e) { cad.onHover(e, true); });
    //    cad.canvas.on('mouse:out', function (e) { cad.onHover(e, false); });
    //    cad.canvas.on('mouse:down', cad.onMouseDown);
    //    cad.canvas.on('mouse:up', cad.onMouseUp);
    //    //_canvas.on('object:moving', checkForIntersection);
    //};
    
    //CADHelper.prototype.generateDrawingOld = function (drawing) {
    //    if (drawing.Groupings && drawing.Groupings.length > 0) {
    //        for (var i = 0; i < drawing.Groupings.length; i++) {
    //            var objects = drawing.Groupings[i].objcts;
    //            var group = [];
    //            var fabricGroup;

    //            if (objects && objects.length > 0) {
    //                for (var j = 0; j < objects.length; j++) {
    //                    var object = objects[j];
    //                    var obj;

    //                    if (object.attributes) {
    //                        object.attributes.tmpltId = object.tmpltId;
    //                        object.attributes.uId = object.uId;

    //                        if (object.typ === "RECTANGLE") {
    //                            obj = cad.addRectangle(object.attributes, false);
    //                        } else if (object.typ === "TEXT") {
    //                            obj = cad.addText(object.txt, object.attributes, false);
    //                        }

    //                        group.push(obj);
    //                    }
    //                }
    //            }

    //            //TODO: fix this so it doesn't wipe out all attributes
    //            if (drawing.Groupings[i].attributes) {
    //                drawing.Groupings[i].attributes.tmpltId = drawing.Groupings[i].tmpltId;
    //                drawing.Groupings[i].attributes.uId = drawing.Groupings[i].uId;
    //                drawing.Groupings[i].attributes.strokeWidth = .5;
    //                drawing.Groupings[i].attributes.originX = 'left';
    //                drawing.Groupings[i].attributes.originY = 'top';
    //            }

    //            fabricGroup = cad.addGroup(group, drawing.Groupings[i].attributes, true);
    //        }

    //        cad.canvas.renderAll();
    //    }
    //}

    //CADHelper.prototype.addObject = function (object, addDirectToCanvas) {
    //    object.toObject = (function (toObject) {
    //        return function () {
    //            return fabricjs.util.object.extend(toObject.call(this), {
    //                tmpltId: this.tmpltId, uId: this.uId
    //            });
    //        };
    //    })(object.toObject);

    //    console.log("addObject: " + JSON.stringify(object));

    //    object.canDrag = function (canBeMoved) {
    //        if (canBeMoved === true) {
    //            this.lockMovementX = false;
    //            this.lockMovementY = false;
    //            this.selectable = true;
    //        } else {
    //            this.lockMovementX = true;
    //            this.lockMovementY = true;
    //            this.selectable = false;
    //        }

    //        if (canBeMoved === false) {
    //            this.hoverCursor = "default";
    //        }
    //    }

    //    object.lockUniScaling = true;

    //    if (addDirectToCanvas === false) {
    //        return object;
    //    }

    //    cad.canvas.add(object);
    //    cad.itemCount++;

    //    return object;
    //};

    //CADHelper.prototype.addRectangle = function (attributes, addDirectToCanvas) {
    //    //alert("New Rectangle!");
    //    var rectangle = cad.addObject(new fabricjs.Rect(attributes), addDirectToCanvas);

    //    rectangle.canDrag(attributes.canBeMoved);

    //    return rectangle;
    //};

    //CADHelper.prototype.addCircle = function (attributes, addDirectToCanvas) {
    //    var circle = cad.addObject(new fabricjs.Circle(attributes), addDirectToCanvas);

    //    circle.canDrag(attributes.canBeMoved);

    //    return circle;
    //};

    //CADHelper.prototype.addText = function (txt, attributes, addDirectToCanvas) {
    //    var text = cad.addObject(new fabricjs.Text(txt, attributes), addDirectToCanvas);

    //    text.canDrag(attributes.canBeMoved);

    //    return text;
    //};

    //CADHelper.prototype.addGroup = function (itemsArray, attributes, addDirectToCanvas) {
    //    var group = cad.addObject(new fabricjs.Group(itemsArray, attributes), addDirectToCanvas);

    //    //group.canDrag(false);
    //    group.canDrag(attributes.canBeMoved);

    //    return group;
    //};

    //CADHelper.prototype.onHover = function (e, flag) {
    //    if (!e || !e.target) {
    //        return;
    //    }

    //    var o = e.target;

    //    if (!o.name || !o.onhover) {
    //        return;
    //    }

    //    o.onhover(e, flag);

    //    cad.canvas.renderAll();
    //};

    //CADHelper.prototype.onMouseDown = function (event) {
    //    var o = event.target;
    //};

    //CADHelper.prototype.onMouseUp = function(event) {
    //    if (!event || !event.target) {
    //        return;
    //    }

    //    var o = event.target;

    //    if (cad.isTrailPointingEnabled) {
    //        cad.triggerTrailPointSet();
    //    }

    //    if (!o.name || !o.onclick) {
    //        return;
    //    }

    //    //if (o.objectType && o.objectType === "Shelf")
    //    //    checkShelfPlacement(o);

    //    o.onclick(e);
    //    cad.canvas.renderAll();
    //}

    //var COLOR_TRANSPARENT = 'rgba(0,0,0,0)';
    //function getDefaultZoomFactor() {
    //    return .1; // 10%
    //}
    //function toInches(sz) {
    //    var z = getDefaultZoomFactor();
    //    return fabric.util.parseUnit(sz+'in')*z;
    //}

    //return CADHelper;
    var cad = null;

    var CADUI = function (canvas) {
        cad = this;

        this._canvas = canvas;
        this._drawing = null;

        this.trailPoints = [];
        this.trailPointTracer = null;
        this.onTrailPointSet = null; // callback function for notifying client when tail point has been set
        this.onTrailPointsDisabled = null; // callback function for notifying client when "trail pointing" is disabled internally
    }
    
    CADUI.prototype.isCanvasActive = function () {
        var $active = $(document.activeElement);
        if ($active.attr('id') !== "body") {
            return false;
        }
        return cad._canvas.getActiveObject() != null;
    }
    CADUI.prototype.canvas = function() { return this._canvas; }

    CADUI.prototype.log = function () {
        var msg = [];
        for (var i = 0; i < arguments.length; i++) {
            msg.push(arguments[i]);
        }
        console.log(msg.join(" "));
    }

    CADUI.prototype.addEquipToContainer = function (data) {
        this._drawing.addEquipToContainer(data);
    }

    CADUI.prototype.generateDrawing = function (data, attrs, next) {
        console.log('CADUI: generate drawing');
        this.loadCanvasWorkspace();
        this.refreshDrawing(data, attrs);

        (next||function() {})();
    }

    CADUI.prototype.refreshDrawing = function( data, attrs, next ) {
        
        var curr = data.curr();
        var cat = curr.TemplateType().toUpperCase().replace(/[- ]/g,"");
        var containerTyp = { BAY: bayComponent
            , NODE: shelfComponent
            , SHELF: shelfComponent
            , CARD: notImplmentedComponent
            , PLUGIN: notImplmentedComponent
            , HIGHLEVELPART: notImplmentedComponent
            , GENERIC: notImplmentedComponent
        }[cat]

        if (!containerTyp) {
            if( console.warn ) {
                console.warn("CADUI: drawing type '%s' not defined", data.TemplateType());
            }
            return;
        }

        this._drawing = containerTyp;
        var obj = this._drawing.Display({ cad: this, canvas: this._canvas, obj: data, attrs: attrs, next: next });
        obj.isMainContainer = true;
    }
    CADUI.prototype.loadCanvasWorkspace = function() {
        var _self = this;

        if ((this.hasBeenLoaded || false) === true ) {
            return;
        }

        this._canvas.fireRightClick = true; 
        this._canvas.fireMiddleClick = true;

        
        this._canvas.on('object:modified', function(evt) { _self.onObjectModified(evt); });
        this._canvas.on('object:rotating', function(evt) { _self.onObjectRotating(evt); });
        this._canvas.on('object:selected', function(evt) { _self.onObjectSelected(evt); });
        
        this._canvas.on('selected', function(evt) { _self.onSelected(evt); });

        this._canvas.on('selection:created', function(evt) { _self.onSelectionCreated(evt); });
        this._canvas.on('selection:updated', function(evt) { _self.onSelectionUpdated(evt); });
        this._canvas.on('selection:cleared', function(evt) { _self.onSelectionCleared(evt); });

        this._canvas.on('mouse:up', function(evt) { _self.onMouseUp(evt); });
        this._canvas.on('mouse:down', function(evt) { _self.onMouseDown(evt); });
        this._canvas.on('mouse:move', function(evt) { _self.onMouseMove(evt); });
        this._canvas.on('mouse:wheel', function(evt) { return _self.onMouseWheel(evt); });

        this.hasBeenLoaded = true;
    }
    
    CADUI.prototype.onObjectModified = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onObjectModified', o.type)
    }
    CADUI.prototype.onObjectRotating = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onObjectRotating', o.type)
    }
    CADUI.prototype.onObjectSelected = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onObjectSelected', o.type, (o.equipType||'?'), (o.NAME||o.LABEL||'?'));
        if (o.onSelected) {
            o.onSelected(evt);
        }
    }
        
    CADUI.prototype.onSelected = function (evt) {
        //https://stackoverflow.com/questions/23418055/fabricjs-double-click-on-objects
        var o = evt.target||{type: 'no target'};
        this.log('onSelected', o.type)
    }

    CADUI.prototype.onSelectionCreated = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onSelectionCreated', o.type);
    }
    CADUI.prototype.onSelectionUpdated = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onSelectionUpdated', o.type);
    }
    CADUI.prototype.onSelectionCleared = function(evt) { 
        var o = evt.target||{type: 'no target'};
        this.log('onSelectionCleared', o.type);
    }

    CADUI.prototype.onMouseUp   = function(evt) { 
        var o = evt.target||{ type: 'no target' };
        cad.log('onMouseUp', o.type);
        if (cad.isTrailPointingEnabled) { // TODO: check for ESC key, drop trail point on ESC key
            var pt = cad._canvas.getPointer(event.e);
            cad.notifyNearbyObjects({ point: pt, event: event, aka: 'onMouseUp', data: { type: CadUtils.UITYP_TRAILPOINT, items: cad.trailPoints } }); // what the hell am I doing...
            cad.triggerTrailPointSet();
        }
    }
    CADUI.prototype.onMouseDown = function(event) { 
        var o = event.target||{ type: 'no target' };
        this.log('onMouseDown', o.type);
        if (cad.isTrailPointingEnabled) {
            var pt = cad._canvas.getPointer(event.e);
            cad.drawTrailPointAnchor(pt);
            cad.notifyNearbyObjects({ point: pt, event: event, aka: 'onMouseDown', data: { type: CadUtils.UITYP_TRAILPOINT, items: cad.trailPoints } }); // what the hell am I doing...
        }
    }
    CADUI.prototype.onMouseMove = function(evt) { 
        var o = evt.target||{ type: 'no target' };
        this.log('onMouseMove', o.type);
    }
    CADUI.prototype.onMouseWheel = function(evt) { 
        var e = evt.e;
        var o = evt.target||{ type: 'no target' };
        this.log('onMouseWheel', o.type);

        e.stopPropagation();
        e.preventDefault();

        return false;
    }
    CADUI.prototype.notifyNearbyObjects = function (props) {
        var objs = cad._canvas.getObjects();
        var target = props.event.target;
        if (!target || target.intersectsWithObject === undefined) {
            return;
        }
        objs.forEach(function (o) {
           if( o.onMouseDownNotify && target.intersectsWithObject(o) )  { 
               o.onMouseDownNotify(props);
           }
        });
    }
    CADUI.prototype.rotateActiveObject = function(direction) {
        var o = cad._canvas.getActiveObject();
        var type = o.get('type');
        if (!o) {
            return;
        }

        this.rotateObject(o, direction);
    }
    CADUI.prototype.rotateObject = function(o, direction) {
        var angle = o.get('angle');

        //console.log('rotate: %s / %s -> %s', direction, angle, angle + direction);
        angle += direction
        switch( angle ) {
            case -90: angle = 270; break;
            case 360: angle = 0; break;
            default: 
                break;
        }
        
        o.rotate(angle).canvas.requestRenderAll();
        // var container = (o.container||cad._canvass.getObjects()[0]);
        // constrainTo(o,container); // CHECK HERE: implement constrainTo
        console.log('!!!! DONT FORGET TO IMPLEMENT CONSTRAINTO() FOR CADUI.ROTATEACTIVEOBJECT !!!');

        o.LastAuditMsg = 'rotated '+angle+'°';
    }
    CADUI.prototype.resetCursor = function (cursor) {
       this.cursorList = (this.cursorList||[]);
       if (this.cursorList.length == 0) {
           return;
       }

       this._canvas.defaultCursor = this.cursorList.pop();
    }

    CADUI.prototype.setCursor = function (cursor) {
       this.cursorList = (this.cursorList||[]);
       this.cursorList.push(this._canvas.defaultCursor);
       this._canvas.defaultCursor = cursor;
       this._canvas.hoverCursor = cursor;
    }
    CADUI.prototype.setHoverCursor = function (cursor) {
       this._canvas.hoverCursor = cursor;
    }
    CADUI.prototype.triggerTrailPointSet = function () {
       var notify = (this.onTrailPointSet||function(){});
       notify(this.trailPoints);
    }
    CADUI.prototype.enableTrailPointing = function( onTrailPointSet, onTrailPointsDisabled ) {
        // clear anything that has been set but not used...
        this.disableTrailPointing();
        this.clearTrailPoints();
        this.resetCursor();

        this.isTrailPointingEnabled = true;
        this.trailPoints.length = 0;
        this.trailPointTracer = null;
        this.onTrailPointSet = onTrailPointSet;
        this.onTrailPointsDisabled = onTrailPointsDisabled;

        this.setCursor('copy');
    }
    CADUI.prototype.disableTrailPointing = function () {
       this.isTrailPointingEnabled = false;
       this.onTrailPointSet = null;
    }
    CADUI.prototype.clearTrailPoints = function () {
        var canvas = cad._canvas;
        var points = cad.trailPoints;
        while( points.length > 0 ) {
            var pt = points.pop();
            canvas.remove(pt);
        }
        cad.trailPoints.length = 0;
    
        if( cad.trailPointTracer ) {
            canvas.remove(cad.trailPointTracer);
        }
        cad.trailPointTracer = null;

        this.resetCursor();

        canvas.requestRenderAll();
    }
    
    CADUI.prototype.getTrailPoints = function () {
        return this.trailPoints;
    }
    CADUI.prototype.addEquipAtTrailPoint = function (equip) {
        if (this.trailPoints.length === 0 ) {
            throw "No trailpoints placed to position label";
        }

        if (!equip) {
            return;
        }
        if (equip && (!equip.WIDTH || !equip.HEIGHT)) {
            throw "Equipment does not specify width/height dimensions";
        }

        var pt = cad.trailPoints[0];
        var ru = CadUtils.getRackUnitAtPoint(pt);
        if (!ru) {
            throw "Could not locate bay position to place shelf";
        }

        ru.set({backgroundColor: 'orange'});
        
        // FIX THIS: don't hard code to shelf, check equip.FEAT_TYP
        bayComponent.AddShelf(equip, ru);

        cad._canvas.requestRenderAll();

        setTimeout(function () {
            ru.set({backgroundColor: 'white'});
            cad._canvas.requestRenderAll();
        }, 3000);
    }
    CADUI.prototype.addLabelAtTrailPoint = function (text) {
        if (this.trailPoints.length === 0 ) {
            throw "No trailpoints placed to position label";
        }
        text = $.trim(text);
        if (text.length === 0) {
            return;
        }
        var pt = cad.trailPoints[0];
        
        var pos = { left: pt.left+(pt.width/2), top: pt.top+(pt.height/2), width: pt.width, height: pt.height };
        
        console.log('addLabelAtTrailPoint() pt=%s', JSON.stringify(pos));
        var label = CadUtils.createPlainLabel({ text: text, pos: pos, backgroundColor: CadUtils.COLOR_TRANSPARENT  });
        console.log('addLabelAtTrailPoint() label=%s', JSON.stringify({ left: label.left, top: label.top }));

        if( pt.lockOn === 'top') { 
            label.set({ top: label.top-(label.height/2) }).setCoords();
        }
        else { 
            label.rotate(90)
                 .set({ left: pos.left+(label.height/2), top: pos.top })
                 .setCoords()
            ;
        }
        cad._canvas.add(label).renderAll();
        
        cad.clearTrailPoints();
        cad.disableTrailPointing();

        // let client know we've disabled the trail points...
        (cad.onTrailPointsDisabled||function(){})();
    }
    CADUI.prototype.getRelativeLocation = function (pt, ref) {
        var p_left = (pt.left||pt.x);
        var p_top = (pt.top||pt.y);

        var r_left = (ref.left||ref.x);
        var r_top = (ref.top||ref.y);

        var diffLeft  = Math.abs(p_left-r_left);
        var diffTop = Math.abs(p_top-r_top);

        var diffBottom = null
        var diffRight = null;

        if (pt.width && ref.width) { //  CHECK HERE, we need to ajust for rotation
            diffRight = Math.abs( (pt.left+pt.width)-(ref.left+ref.width) );
        }
        if( pt.height && ref.height ) {
            diffBottom = Math.abs( (pt.top+pt.height)-(ref.top+ref.height) );
        }

        //console.log('getRelativeLocation: left=%s|%s (%s) / top=%s|%s (%s)', p_left, r_left, diffLeft, p_top, r_top, diffTop );
        
        var rv = { left: false, top: false, bottom: false, right: false, inline: { left: false, top: false } };

        rv.left = (p_left < r_left)
        rv.right = (p_left > r_left);
        rv.top = (p_top < r_top);
        rv.bottom = (p_top > r_top);
        rv.inline.left = (p_left === r_left);
        rv.inline.top = (p_top === r_top);

        rv.diffLeft = diffLeft;
        rv.diffTop = diffTop;
        rv.diffRight = diffRight;
        rv.diffBottom = diffBottom;

        return rv;
    }
    CADUI.prototype.drawTrailPointAnchor = function (pt, next) {
        next = (next||function() { });

        var canvas = cad._canvas;
        var url = '/Content/icons/trail-point-anchor.png'; // CHECK HERE; don't hard code...
        fabric.Image.fromURL(url, function(img) {
                img.set({ left: pt.x-10, top: pt.y-10 });
                img.trailPointLoc = pt;
                //img.container = container; TODO: set container intelligently
                img.lockMovementX = false;
                img.lockMovementY = false;
                img.lockScalingX = true;
                img.lockScalingY = true;
                img.lockUniScaling = true;
                img.lockRotation = true;
                img.backgroundColor = CadUtils.COLOR_TRANSPARENT;

                img.setControlsVisibility({tl: false, tr: false, mt: false, mb: false,mr: false, ml: false,bl: false,br: false});
                img.hoverCursor = 'default'; // this doesn't work...

                cad.trailPoints.push(img);

                canvas.add(img); 
                canvas.bringToFront(img);

                next(img);
        });
    }
    CADUI.prototype.drawTrailPointTracer = function ( points ) {
        var canvas = cad._canvas;
        if (points.length != 2) {
            return;
        }
        
        cad.alignTrailPoints( points );
        var tracer = cad.drawTracer( points );
        cad.setupTrailPointEvents(points, tracer);
    }
    CADUI.prototype.alignTrailPoints = function (points) {
        var from = points[0];
        var to = points[1];

        var loc = cad.getRelativeLocation(from, to);
        
        if( loc.diffTop < loc.diffLeft ) { 
            to.set({ top: from.top });
            from.lockOn = 'top';
            to.lockOn = 'top';
        }
        else { 
            to.set({ left: from.left });
            from.lockOn = 'left';
            to.lockOn = 'left';
        }

        from.peer = to;
        to.peer = from;

//      _canvas.bringToFront(from);
//      _canvas.bringToFront(to);
    }
    CADUI.prototype.drawTracer = function(points) {
        var pt1 = points[0];
        var pt2 = points[1];

        var padding = 10;
        var loc = cad.getRelativeLocation(pt1, pt2);
        
        var points = [ pt1.left+(loc.inline.top ? pt1.width : pt1.width/2)
                     , pt1.top+(loc.inline.top ? pt1.height/2 : pt1.height)
                     , pt2.left+(loc.inline.top ? 0 : pt2.width/2)
                     , pt2.top+(loc.inline.top ? pt2.height/2 : 0)
        ];

        line = new fabric.Line(points, {
           strokeWidth: 1,
           stroke: 'grey',
           strokeDashArray: [2,2],
           originX: 'center',
           originY: 'center'
        });

        line.selectable = true;
        line.lockMovementX = false;
        line.lockMovementY = false;
        line.lockScalingX = true;
        line.lockScalingY = true;
        line.lockUniScaling = true;
        line.lockRotation = true;
        line.setControlsVisibility({tl: false, tr: false, mt: false, mb: false,mr: false, ml: false,bl: false,br: false});
        line.padding = padding;

        cad._canvas.add(line);

        cad.trailPointTracer = line;
        
        return line;
        //_canvas.sendToBack(line);
    }
    CADUI.prototype.setupTrailPointEvents = function (points, tracer) {
        var canvas = cad._canvas;
        //var main = getMainContainer();

        var pt1 = points[0];
        var pt2 = points[1];

        var alignTracer = function () {
            var padding = tracer.padding;
            var loc = cad.getRelativeLocation(tracer.pt1, tracer.pt2);
            var props = { x1: pt1.left+(pt1.width/2)+(loc.inline.top ? +padding : 0)
                     , y1: pt1.top+(pt1.height/2)+(loc.inline.left ? +padding : 0)
                     , x2: pt2.left+(pt2.width/2)+(loc.inline.top ? -padding : 0)
                     , y2: pt2.top+(pt2.height/2)+(loc.inline.left ? -padding : 0)
            };
            tracer.set(props).setCoords();
        }

        var alignTracerPoints = function () {
           var pos = { left: 0, top: 0 };
            if( pt1.lockOn === 'left' )
            {
                pos.left = tracer.left-(pt1.width/2)+.5;
                pos.top = tracer.top-(tracer.height/2)-(pt1.height);
            }
            else 
            { 
                pos.left = tracer.left-(tracer.width/2)-(pt1.width/2)-tracer.padding;
                pos.top = tracer.top-tracer.padding;
            }
            pt1.set(pos).setCoords();

                
            if( pt1.lockOn === 'left') {
                pos.top = pt1.top + tracer.padding + tracer.height + tracer.padding;                
            }
            else { 
                pos.left = pt1.left+tracer.padding+tracer.width+tracer.padding;    
            }
            pt2.set(pos).setCoords();
        }
        pt1.on('moving', function (e) {
            pt1.peer.set(pt1.lockOn, (pt1.lockOn === 'left' ? pt1.left: pt1.top)).setCoords();
            alignTracer();
            //constrainTo(pt1, main); // TODO: make this work
            canvas.requestRenderAll();
        });
        pt1.on('modified', function (e) {
            pt1.peer.set(pt1.lockOn, (pt1.lockOn === 'left' ? pt1.left: pt1.top)).setCoords();
            alignTracer();
            // constrainTo(pt1, main);  // TODO: make this work
            canvas.requestRenderAll();
        });
        pt2.on('moving', function (e) {
            pt2.peer.set(pt2.lockOn, (pt2.lockOn === 'left' ? pt2.left: pt2.top)).setCoords();
            alignTracer();
            //constrainTo(pt2, main);  // TODO: make this work
            canvas.requestRenderAll();
        });
        pt2.on('modified', function (e) {
            pt2.peer.set(pt2.lockOn, (pt2.lockOn === 'left' ? pt2.left: pt2.top)).setCoords();
            alignTracer();
            //constrainTo(pt2, main); // TODO: make this work
            canvas.requestRenderAll();
        }); 
        tracer.on('moving', function (e) {
            alignTracerPoints();
            canvas.requestRenderAll
        });
        tracer.on('modified', function (e) {
            alignTracerPoints();
            canvas.requestRenderAll();
        });
        tracer.pt1 = pt1;
        tracer.pt2 = pt2;

        //var alignTrailPointsToTracer = function(o) { // CHECK HERE: NOT SURE WE NEED THIS
        //    var lockOn = pt1.lockOn;

        //    var props1 = { left: o.aCoords.tl.x, top: o.aCoords.tl.y };
        //    var props2 = { left: o.aCoords.tr.x, top: o.aCoords.tr.y };

        //    switch (lockOn) {
        //        case 'top': 
        //            props1.left -= pt1.width;
        //            props1.top -= pt1.height/2;
        //            props2.top -= pt2.height/2;
        //            break;
        //        case 'left': 
        //            props1.left -= pt1.width/2;
        //            props1.top -= pt1.height;
        //            props2.left -= pt2.width/2;
        //            break;
        //    }
        //    pt1.set(props1).setCoords();
        //    pt2.set(props2).setCoords();

        //    canvas.renderAll();
        //}
        canvas.requestRenderAll();
    }
    return CADUI;
});