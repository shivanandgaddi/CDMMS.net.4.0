define([], function() { 
    var AppPopoutWindow = (function() { 
	    this.win = null;
        this.page = null;
        this.url = null;
	    this.title = null;
        this.user = null;
        this.isDebugMode = false;

	    var changeTitleWhenLoaded = function( $this ) { 
            if( $this.win.document.title.indexOf('Switching') >= 0 ) {
                setTimeout(function() { changeTitleWhenLoaded($this); }, 250);
                return;
            }
            $this.win.document.title = $this.title;
	    }
	    var checkLoadStatus = function( $this ) { 
            if ($this.win === null || $this.win.location === null || $this.win.location.hash === null) {
                console.log('AppPopoutWindow.checkLoadStatus(%s) - no window reference or hash', $this.title);
                return;
            }
            var loc = $this.win.location.hash;
            var landing = ( $this.isDebugMode ? 'tmplt' : 'mtlInv');
	        if( !loc || loc.indexOf(landing) === -1 ) { 
	             setTimeout(function() { checkLoadStatus($this); }, 250);
                 return;
            }
	    
            var refreshToNewURL = function() { 
                console.log('AppPopoutWindow.checkLoadStatus() switching to %s', $this.url);
	            $this.win.document.title = 'Switching to '+$this.url;
                $this.win.location = $this.url;
   	            setTimeout(function() { changeTitleWhenLoaded($this); }, 250);
            }

            if (landing === 'tmplt') {
                setTimeout(refreshToNewURL,1000);
                return;
            }

            refreshToNewURL();
	    }

	    var open = function(title, page, id) { 
            this.user = require('Utility/user');
            this.isDebugMode = (this.user.cuid === 'dev');
            this.page = page;                
            this.url = window.location.origin+'/#'+page+'?id='+id;
            this.title = 'CDMMS '+title+' (#'+id+')';
            
            //
            // example: 
            // http://cdmmsdotnetprod.corp.intranet/#auth/c405a8ab124ce1a07f7a8300a6ce0893?cuid=mxjoh18
            //
            var auth = window.location.origin+'/#auth/'+this.user.sess+'?cuid='+this.user.cuid;
            if (this.isDebugMode ) {
                auth = this.url; // dev env -- we cant/dont authorize...
            }

            var opts = 'width=1024px,height=1024px'.split(',');
            this.win = window.open(auth, 'CDMMS-'+page+id, opts);
            if (this.win === null) {
                alert('Failed to open popout windows for '+title);
                return;
            }
    	    this.win.focus();

            var $this = this;
            setTimeout(function() { checkLoadStatus($this); }, 250);
	    }
	    var close = function() { 
		    this.win.close();
	    }
	    return { Open: open, Close: close }
    })();

    return AppPopoutWindow;
});