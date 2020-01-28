define(['jquery', 'fabricjs'], function ($, fabricjs, utils) {
    var CadComponentNotImplemented = function() {
        
        this.DEFAULT_SCALE = .1;

        this._data = null;
        this._cad = null;
        this._bay = null;
        this._canvas = null;
        this._rackUnits = null;
    }
    CadComponentNotImplemented.prototype.Display = function(props) {
        throw props.obj.typ.toUpperCase()+': CadComponent not implemented yet.';
    }

    return new CadComponentNotImplemented();
});