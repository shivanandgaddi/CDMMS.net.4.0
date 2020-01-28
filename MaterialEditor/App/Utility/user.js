define(['knockout'], function (ko) {
    var cuid = ko.observable('unset');
    var sessionId = ko.observable('');

    return {
        cuid: cuid,
        sessionId: sessionId
    };
});