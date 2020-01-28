define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system'], function (composition, ko, $, http, activator, mapping, system) {
    var SAPMaterialItem = function (data) {
        var self = this;
        var results = JSON.parse(data);
        
        self.mtl = mapping.fromJS(results);

        if (self.mtl.id.value() > 0)
            self.exists = true;
        else
            self.exists = false;
    };

    SAPMaterialItem.prototype.save = function () {
        var self = this;

        console.log('Save');
    };

    return SAPMaterialItem;
});