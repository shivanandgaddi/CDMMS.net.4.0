define(['plugins/router', 'durandal/app', 'durandal/system', '../Utility/user', '../Utility/helper', 'knockout'], function (router, app, system, user, helper, ko) {
    console.log('Inside shell.js');
    return {
        router: router,
        appUser: ko.observable(user),
        activate: function () {
            var routeMap = [];

            if (system.debug()) {
                routeMap = [
                    { route: 'mtlInv(/:mtlItmId)', title: 'Material Inventory', moduleId: 'viewmodels/materialItemEdit', hash: '#mtlInv', nav: true },
                    { route: 'nuP', title: 'New and Updated Parts', moduleId: 'viewmodels/newUpdatedParts', nav: true },
                    { route: 'report', title: 'Reports', moduleId: 'viewmodels/generateReports', nav: true },
                    { route: 'roNew(/:mtlItmId)', title: 'Record Only Parts', moduleId: 'viewmodels/recordOnly', nav: true, hash: '#roNew', nav: true },
                    { route: 'spec(/:specTyp)(/:specId)', title: 'Specifications', moduleId: 'viewmodels/specification', hash: '#spec', nav: true },
                    { route: 'tmplt', title: 'Templates', moduleId: 'viewmodels/template', nav: true },
                    //{ route: 'tmpltvwr', title: 'Template Viewer', moduleId: 'viewmodels/templateViewer', nav: true },
                    { route: 'mtlGrpMng', title: 'Material Group Management', moduleId: 'viewmodels/categoryManagement', nav: true },
                    { route: 'assembly', title: 'Assembly Units', moduleId: 'viewmodels/assemblyUnits', nav: true },
                    { route: 'drpdwn', title: 'Reference Tables', moduleId: 'viewmodels/DDdropDownTable', nav: true },
                    //{ route: 'shlfassgn', title: 'Shelf Assignment', moduleId: 'viewmodels/shelfModel', nav: true },
                    //{ route: 'cardassgn', title: 'Card Assignment', moduleId: 'viewmodels/cardAssignment', nav: true },
                    //{ route: 'portassgn', title: 'Port Assignment', moduleId: 'viewmodels/portAssignment', nav: true },
                    //{ route: 'plgnassgn', title: 'Plug-In Association', moduleId: 'viewmodels/PlugInAssociations', nav: true },
                    { route: 'comnCnfg', title: 'Common Config', moduleId: 'viewmodels/commonConfig', nav: true },                
                    { route: 'goto/:page', moduleId: 'viewmodels/goto', nav: false },
                    { route: '', moduleId: 'viewmodels/authorize', nav: false }];
            } else {
                routeMap = [
                    { route: 'mtlInv(/:mtlItmId)', title: 'Material Inventory', moduleId: 'viewmodels/materialItemEdit', hash: '#mtlInv', nav: true },
                    { route: 'nuP', title: 'New and Updated Parts', moduleId: 'viewmodels/newUpdatedParts', nav: true },
                    { route: 'report', title: 'Reports', moduleId: 'viewmodels/generateReports', nav: true },
                    { route: 'roNew(/:mtlItmId)', title: 'Record Only Parts', moduleId: 'viewmodels/recordOnly', nav: true, hash: '#roNew', nav: true },
                    { route: 'spec(/:specTyp)(/:specId)', title: 'Specifications', moduleId: 'viewmodels/specification', hash: '#spec', nav: true },
                    { route: 'tmplt(/:tmpltTyp)(/:tmpltId)', title: 'Templates', moduleId: 'viewmodels/template', hash: '#tmplt', nav: true },
                    { route: 'mtlGrpMng', title: 'Material Group Management', moduleId: 'viewmodels/categoryManagement', nav: true },
                    { route: 'assembly', title: 'Assembly Units', moduleId: 'viewmodels/assemblyUnits', nav: true },
                    { route: 'drpdwn', title: 'Reference Tables', moduleId: 'viewmodels/DDdropDownTable', nav: true },
                    //{ route: 'shlfassgn', title: 'Shelf Assignment', moduleId: 'viewmodels/shelfModel', nav: true },
                    //{ route: 'cardassgn', title: 'Card Assignment', moduleId: 'viewmodels/cardAssignment', nav: true },
                    //{ route: 'portassgn', title: 'Port Assignment', moduleId: 'viewmodels/portAssignment', nav: true },
                    //{ route: 'plgnassgn', title: 'Plug-In Association', moduleId: 'viewmodels/PlugInAssociations', nav: true },
                    { route: 'comnCnfg', title: 'Common Config', moduleId: 'viewmodels/commonConfig', nav: true },
                    { route: 'goto/:page', moduleId: 'viewmodels/goto', nav: false },
                    { route: 'auth/:id', moduleId: 'viewmodels/authorize', nav: false },
                    { route: '', moduleId: 'viewmodels/notAuthorized', nav: false }];
            }

            router.map(routeMap).buildNavigationModel();

            router.guardRoute = function (instance, instruction) {
                //console.log("router.guardRoute");
                if (instruction.config.moduleId === 'viewmodels/authorize' || instruction.config.moduleId === 'viewmodels/notAuthorized' || instruction.config.moduleId === 'viewmodels/goto') {
                    return true;
                } else {
                    if (instance && instance.usr) {
                        var cuid = '';

                        try {
                            cuid = instance.usr.cuid();
                        }
                        catch (error) {
                            cuid = instance.usr.cuid;
                        }

                        if (cuid !== 'unset') {
                            return true;
                        } else {
                            return '#';
                        }
                    } else {
                        return '#';
                    }
                }
            };

            return router.activate();
        }
    };
});

// IE missing URLSearchParams api
(function (w) {

    w.URLSearchParams = w.URLSearchParams || function (searchString) {
        var self = this;
        self.searchString = searchString;
        self.get = function (name) {
            var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(self.searchString);
            if (results == null) {
                return null;
            }
            else {
                return decodeURI(results[1]) || 0;
            }
        };
    }

})(window);


$(document).ready(function () {
    $(document).on('click', '.navbar-collapse.in', function (e) {
        if ($(e.target).is('a:not(".dropdown-toggle")')) {
            $(this).collapse('hide');
        }
    });
});