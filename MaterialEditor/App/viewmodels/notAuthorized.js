define(['durandal/app'], function (app) {
    var NotAuthorized = function () {
        var self = this;

        app.setRoot('viewmodels/notAuthorized');
    };

    return NotAuthorized;
});