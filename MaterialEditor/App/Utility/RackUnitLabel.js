define(['jquery', 'fabricjs'], function ($, fabricjs) {
    var RackUnitLabel = fabric.util.createClass(fabric.Rect, {
        type: 'RackUnitLabel',

        initialize: function(options) {
            options || (options = { });

            this.callSuper('initialize', options);
            this.set('label', options.label || '');
        },

        toObject: function() {
            return fabric.util.object.extend(this.callSuper('toObject'), {
            label: this.get('label')
            });
        },

        _render: function(ctx) {
            this.callSuper('_render', ctx);

            ctx.font = '10px Courier New';
            ctx.fillStyle = 'grey';
            ctx.fillText(this.label, -(this.width/2)+2, -(this.height/2)+12);
        },
    });
    return RackUnitLabel;
});