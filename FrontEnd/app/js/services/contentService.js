/*
 * Define content load related functions used in docsController
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function() {
  'use strict';
   /*jshint validthis:true */
  function provider($q, $http, constants, urlService, tocCache, mdIndexCache) {
    function getYamlResponse(response){
      return jsyaml.load(response.data);
    }

    function valueHttpWrapper(value){
      var deferred = $q.defer();
      deferred.resolve(value);
      return deferred.promise;
    }

    this.getNavBar = function(){
      return $http.get(constants.TocFile)
      .then(getYamlResponse);
    };

    this.getContent = function(path){
      return $http.get(path)
      .then(getYamlResponse);
    };

    this.getMarkdownContent = function(path){
      function getContentComplete(response){
        return response.data;
      }
      return $http.get(path)
      .then(getContentComplete);
    };

    this.getTocContent = function(path) {
      if (!path) return valueHttpWrapper(null);
      var toc;
      path = urlService.normalizeUrl(path);
      var temp = tocCache.get(path);
      if (temp) {
        return valueHttpWrapper(temp);
      } else {
        toc = {
          path: path
        };
        return $http.get(path)
          .then(function(result) {
            var content = getYamlResponse(result);
            toc.content = content;
            tocCache.put(path, toc);
            return toc;
          }).catch(function(result) {
            tocCache.put(path, toc);
            return toc;
          });
      }
    };

    this.getMdContent = function(path) {
      if (!path) return valueHttpWrapper(null);
      var tempMdIndex;
      var pathInfo = urlService.getPathInfo(path);
      var mdPath = urlService.normalizeUrl((pathInfo.tocPath || '') + '/' + 'md.yaml');

      if (mdPath) {
        tempMdIndex = mdIndexCache.get(mdPath);
        if (tempMdIndex !== undefined) {
          if (tempMdIndex) return valueHttpWrapper(tempMdIndex);
        } else {
          return $http.get(mdPath)
            .then(function(result) {
              var content = getYamlResponse(result);
              mdIndexCache.put(path, content);
              return content;
            }).catch(function(result) {
              mdIndexCache.put(path, null);
              return null;
            });
        }
      } else {
        return valueHttpWrapper(null);
      }
    };

    this.getDefaultItem = function(array, action) {
      if (!action) return;
      if (array && array.length > 0) {
        return action(array[0]);
      }
    };
  }

  angular.module('docascode.contentService', ['docascode.constants', 'docascode.urlService'])
    .factory('tocCache', ['$cacheFactory', function($cacheFactory) {
      return $cacheFactory('toc-cache');
    }])
    .factory('mdIndexCache', ['$cacheFactory', function($cacheFactory) {
      return $cacheFactory('mdIndex-cache');
    }])
    .service('contentService', ['$q', '$http', 'docConstants', 'urlService', 'tocCache', 'mdIndexCache', provider]);

})();