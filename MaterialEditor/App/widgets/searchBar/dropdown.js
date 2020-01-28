define(['./item', 'durandal/activator', 'knockout'], function (Item, activator, ko) {
    var DropDown = function (initialItems) {
        this.items = ko.observable(initialItems);
        this.selectedItem = ko.observable();
    };

    DropDown.prototype.activate = function (settings) {
        this.settings = settings;
    };

    DropDown.prototype.clickFunction = function (x) {
        var y = x;
    };

    return DropDown;
});