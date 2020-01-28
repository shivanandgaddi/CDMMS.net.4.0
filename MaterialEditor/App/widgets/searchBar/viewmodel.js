define(['durandal/system', 'durandal/app', 'durandal/composition', 'knockout', 'jquery', './dropdown'], function (system, app, composition, ko, $, Dropdown) {
    /*
    When placed in a view, the SearchBar should be given the following settings.
        searchFunction: The function called to perform the search. Should return a json array of SearchResult.cs. 
        selectFunction: The function to call when an item in the dropdown list is selected.
        isVisible: A boolean to indicate if the search bar should be visible when the view is initially shown.
        bindingObject: The observable to bind to the main view.
        showView: An observable to be used to show or hide the main view.}
    */
    var SearchBar = function () {
        this.isVisible = ko.observable(false);
        this.optionVisible = ko.observable(false);
        this.resultsDropdown = ko.observable('');
        this.dropdown = {};
    };

    SearchBar.prototype.activate = function (settings) {
        this.settings = settings;
        this.optionVisible(settings.optionVisible);
        this.isVisible(settings.isVisible);
    };

    SearchBar.prototype.handleSearch = function () {
        var self = this;

        if (self.settings.searchFunction) {
            var searchValue = document.getElementById('searchBarInput').value;

            console.log('searchValue = ' + searchValue);

            self.settings.searchFunction(searchValue, self.resultsDropdown).then(function (response) {
                if ('no_results' === response) {
                    app.showMessage("No results found for '" + document.getElementById('searchBarInput').value + "'.").then(function () {
                        $("#interstitial").hide();
                        return;
                    });
                } else {
                    var results = JSON.parse(response);

                    if (results.length > 1) {
                        dropdown = new Dropdown(results);

                        dropdown.selectedItem.subscribe(self.dropdownItemSelected, self);

                        self.resultsDropdown(dropdown);
                    }
                    else if (results.length == 1)
                    {
                        self.dropdownItemSelected(results[0].itemValue);
                    }

                }

                $("#interstitial").hide();

                /*system.acquire("viewmodels/statusBar").then(function (sb) {
                    sb.sendStatus('.status-info', 'Search Successful');
                });*/
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        }
    };

    SearchBar.prototype.dropdownItemSelected = function (selectedValue) {
        console.log('selectedValue = ' + selectedValue);
        var self = this;

        if (self.settings.selectFunction) {
            self.settings.selectFunction(selectedValue, self.settings.bindingContext.$root)
            self.resultsDropdown(false);
            //self.settings.selectFunction(selectedValue, self.settings.bindingObject, self.settings.showView)
        }
    };

    SearchBar.prototype.handleSearchEvent = function (data, event) {
        if (this.settings.searchFunction) {
            console.log('key pressed.');

            if (event.keyCode == 13) {
                this.handleSearch();
            }
            else
                return true;
        }
    };

    return SearchBar;
});