requirejs.config({
    paths: {
        'text': '../Scripts/text',
        'durandal': '../Scripts/durandal',
        'plugins': '../Scripts/durandal/plugins',
        'transitions': '../Scripts/durandal/transitions',
        'jquery': '../Scripts/jquery-2.0.3.min',        
        'knockout': '../Scripts/knockout-3.4.0',
        'knockout.mapping': '../Scripts/knockout.mapping',
        'jquerydatatable': '../Scripts/jquery.dataTables.min',
        'jqueryui': '../Scripts/jquery-ui.min',
        'datablescelledit': '../Scripts/dataTables.cellEdit',
        'bootstrapJS': '../Scripts/bootstrap.min',
        'jszip': '../Scripts/jszip',
        'fileSaver': '../Scripts/FileSaver',       
        'myexcel': '../Scripts/myexcel',
        'jqueryValidator': '../Scripts/jquery.validate',
		'fabricjs': '../Scripts/fabric.js/fabric.require', 
    },
    shim: {
        'datablescelledit': {
            deps: ['jquery', 'jqueryui', 'jquerydatatable']
        },
        'jquerydatatable': {
                deps: ['jquery', 'jqueryui']
        },
        'myexcel': {
            deps: ['jszip', 'fileSaver']
        },
    }

});

//define('jquery', function () { return jQuery; });
//define('knockout', ko);

define(['durandal/system', 'durandal/app', 'durandal/viewLocator','jszip'], function (system, app, viewLocator,jszip) {
    //>>excludeStart("build", true);
    system.debug(true);
    //>>excludeEnd("build");

    //Uncomment this line before doing a Release build
    //system.debug(false);
    window.JSZip = jszip;
    app.title = 'Material Editor';

    app.configurePlugins({
        router: true,
        dialog: true,
        widget: {
            kinds: ['searchBar']
        },
        observable: true
    });

    app.start().then(function() {
        //Replace 'viewmodels' in the moduleId with 'views' to locate the view.
        //Look for partial views in a 'views' folder in the root.
        viewLocator.useConvention();

        //Show the app by setting the root view model for our application with a transition.
        app.setRoot('viewmodels/shell', 'entrance');  
    });
});