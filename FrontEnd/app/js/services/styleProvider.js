/*
 * Define html ng-class related functions used in docsController
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function() {
  'use strict';
   /*jshint validthis:true */
  function provider($q, $http, urlService) {
    this.navClass = function(navItem) {
      var navPath;
      if (this.pathInfo) {
        navPath = urlService.normalizeUrl(this.pathInfo.tocPath || this.pathInfo.contentPath);
      }

      return {
        current: navPath && navPath === navItem.href,
      };
    };

    this.gridClass = function(toc) {
      return {
        'grid-right': toc,
        grid: !toc
      };
    };
  }

  angular.module('docascode.styleProvider', ['docascode.urlService'])
    .service('styleProvider', ['$q', '$http', 'urlService', provider]);

})();