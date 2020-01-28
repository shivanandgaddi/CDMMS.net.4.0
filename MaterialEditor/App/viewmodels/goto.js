define(['plugins/router', '../Utility/user', 'plugins/http', 'durandal/system'], function (router, user, http, system) {
    var GoTo = function () {
        var self = this;

        self.usr = require('Utility/user');
    };

    GoTo.prototype.canActivate = function (page, usr) {
        var pageIsValid = false;

        if (system.debug()){
            user.cuid = 'dev';
            user.sessionId = 'VwuNdnOzrnvtK34H1BQ7VklFJ9z3fcqt';
        }

        if (page == 'roNew') {
            pageIsValid = true;           
            document.getElementById("navigareRecordHide").style.display = "none";          
            if (document.getElementById("viewContainerTransition").classList.contains('viewContainerPadding')) {
                document.getElementById("viewContainerTransition").classList.add('viewContainerPaddingRO');
                document.getElementById("viewContainerTransition").classList.remove('viewContainerPadding');
            }
        }

        //Validate the page is valid, if valid set pageIsValid = true
        //TODO
        if (pageIsValid) {
            return system.defer(function (p) {
                p.resolve(true);
            }).promise();
        } else
            return { redirect: '#' };
    };

    GoTo.prototype.activate = function (page, usr) {
        router.navigate('#' + page);
    };

    return GoTo;
});