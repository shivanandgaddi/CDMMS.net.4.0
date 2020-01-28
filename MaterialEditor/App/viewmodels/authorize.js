define(['plugins/router', '../Utility/user', 'plugins/http', 'durandal/system'], function (router, user, http, system) {
    var Authorize = function () {
        var self = this;
    };

    Authorize.prototype.canActivate = function (sessionId, usr) {
        var self = this;
        var url = 'api/user/';
        
        if (system.debug() && user && user.cuid() === 'unset') {
            user.cuid = 'dev';
            user.sessionId = 'VwuNdnOzrnvtK34H1BQ7VklFJ9z3fcqt';
            return true;
        }

        if (sessionId && usr && usr.cuid) {
            user.cuid = usr.cuid;
            user.sessionId = sessionId;

            return system.defer(function (p) {
                http.get(url + sessionId).then(function (response) {
                    if ('Valid' === response) {
                        p.resolve(true);
                    }
                    else
                        p.resolve({ redirect: '#' });
                });
            }).promise();
        }
        else
            return { redirect: '#' };
    };

    Authorize.prototype.activate = function (sessionId, usr) {
        //router.navigate('#mtlInv');
        router.navigate('#tmplt');
    };

    return Authorize;
});