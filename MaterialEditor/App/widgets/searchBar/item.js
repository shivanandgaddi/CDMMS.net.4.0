define([], function () {
    var Item = function (value, displayValue) {
        this.itemValue = value;
        this.displayValue = displayValue;
    };

    return Item;
});