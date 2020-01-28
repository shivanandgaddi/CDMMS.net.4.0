define(['jquery', 'fabricjs', '../Utility/CadUtils', '../Utility/CadComponentBay', '../Utility/CadEquipPropertiesDialog']
    , function ($, fabricjs, utils, bay, CadEquipPropertiesDialog) {
    var CadComponentShelf = function() {
        this._data = null;
        this._cad = null;
        this._canvas = null;
        this._shelf = null;
    }
    CadComponentShelf.prototype.dbg = function (msg, data) {
        if( data && console.table ) {
            console.log('+++ %s:', msg);
            console.table(data)
            console.log('--- %s:', msg);

            return;
        }
        console.log('%s', msg);
    }
    CadComponentShelf.prototype.render = function (props) {
        var canvas = this._canvas;
        //var shelf = new fabric.Rect({ width: utils.toDefaultScaledInches("12in")
        //                            , height: utils.toDefaultScaledInches(1.75*3)
        //                            , stroke: 'darkgreen'
        //                            , strokeWidth: .5
        //                            , fill: 'lightgreen'
        //                            , top: 0
        //                            , left: 0
        //                            , selectable: true
        //                            , lockScalingX: true
        //                            , lockScalingY: true
        //                            , lockUniScaling: true
        //});
        
        //shelf.on('rotated', function (e) {
        //    shelf.straighten();
        //});
        //canvas.add(shelf);
        canvas.requestRenderAll();
    }
    CadComponentShelf.prototype.Create = function(props, bayExternal, bayInternal) {
        var uom = props.UOM.toLowerCase();
        if (uom !== 'in') {
            throw 'CadComponentShelf.Create: UOM "'+uom+'" not implemented!';
        }
        
        var ru = utils.findRU(props.RU);
        if (!ru) {
            throw 'CadComponentShelf.Create: could not locate mounting poisition (RU) #'+props.RU;
        }

        var canvas = bayInternal.canvas;

        var opts = { height: utils.toDefaultScaledInches(props.HEIGHT)
                   , width: utils.toDefaultScaledInches(props.WIDTH)
                   , top: 0
                   , left: props.container.left+1
                   , fill: 'white'
                   , stroke: 'green'
                   , strokeWidth: .5
        };

        opts.top = ru.top-opts.height;
        
        props.LABEL = (props.LABEL||props.NAME);

        var sh = new fabric.Rect(opts);
        sh.container = props.container;
        sh.name = props.LABEL;
        sh.equipType = props.FEAT_TYP;
        sh.selectable = true;
        sh.lockMovementX = false;
        sh.lockMovementY = false;
        sh.lockScalingX = true;
        sh.lockScalingY = true;
        sh.lockUniScaling = true;
        sh.lockRotation = false;
        //sh.backgroundColor = utils.COLOR_TRANSPARENT;

        sh.setControlsVisibility({tl: false, tr: false, mt: false, mb: false,mr: false, ml: false,bl: false,br: false});
    
        var offset = { top: -(sh.top+(sh.height/2)) };

        var descr = utils.createLabel({ text: props.MTRL_DESCR, ru: props.RU, container: props.container, offset: offset });
        descr.equip = sh;
        descr.equipType = 'SHELF_DESCR';

        var name = utils.createLabel({ text: props.NAME, ru: props.RU, container: props.container, offset: offset });
        name.equipType = 'SHELF_NAME';
        name.selectable = false;
        name.equip = sh;
        name.set({left: sh.left+(sh.width/2)-(name.width/2)}).setCoords();
        
        sh.label  = { name: name, descr: descr };
       
        sh.lockOn = 'center';

        sh.adjustLabels = function () {
            var top = (sh.height <= ru.rackUnitHeight ? sh.top : sh.top+(sh.height/2));

            if( this.label.name ) {
                this.label.name.set({left: this.left+(this.width/2)-(this.label.name.width/2), top: top, fontSize: 12}).setCoords();
                if (this.label.name.left <= this.left) {
                    this.label.name.set({ left: this.left+10 }).setCoords();
                }
                canvas.bringToFront(this.label.name);
            }
            if( this.label.descr ) {
                this.label.descr.set({left: bayExternal.left+bayExternal.width+10, top: top, fontSize: 12}).setCoords();
            }
        }
        
        sh.isMoving = false;
        sh.on('moving', function(e) { 
            this.isMoving = true;
            utils.constrainTo (this.container);
            this.adjustLabels();
        });
        sh.on('modified', function (e) {
            if (!this.isMoving) {
                return;
            }
            var ru = utils.snapToNearestRU(this);
            if( ru ) { 
                this.rackUnit = ru.rackUnit;
                this.rec.MNTNG_POS = ru.rackUnit;
                this.rec.RU = ru.rackUnit
            }
            sh.adjustAlignment();
            this.adjustLabels();
            this.isMoving = false;
            canvas.discardActiveObject();
        });

        sh.onSelected = function () {
            var config = JSON.copy(this.rec);
            config.ru = utils.findRU(this.RU);

            config.onOK = function () {
                sh.canvas.discardActiveObject();
                sh.canvas.requestRenderAll();
                return true;
            }
            config.onCANCEL = function () {
                sh.canvas.discardActiveObject();
            }
            CadEquipPropertiesDialog.Display(config);
        }
        sh.adjustAlignment = function() { 
            switch( this.lockOn ) 
            {   case 'center': utils.alignCenter(this); break;
                case 'left': utils.alignLeft(this); break;
                case 'right': utils.alignRight(this); break;
                default: 
                    this.lockOn = 'center';
                    utils.alignCenter(this); 
                    break;
            }
        };
        return { shelf: sh };
    }
    CadComponentShelf.prototype.Display = function (props) {
        this._data = props;
        this._cad = props.cad;
        this._canvas = props.canvas;
        this._shelf = props.obj;

        this.render();
    }
    return new CadComponentShelf();
});    