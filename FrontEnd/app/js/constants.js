/*
 * Define constants used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
*/

(function() {
    'use strict';
     /*jshint validthis:true */
    function provider() {
        this.TocFile = 'toc.yaml'; // docConstants.TocFile
        this.TocAndFileUrlSeperator = '!'; // docConstants.TocAndFileUrlSeperator
    }

    angular.module('docascode.constants', [])
        .service('docConstants', provider);

})();