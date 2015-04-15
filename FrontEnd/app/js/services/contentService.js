/*
 * Define doc related functions used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function() {
  'use strict';
   /*jshint validthis:true */
  function provider($q, $http, constants, urlService) {
    function asyncFetchIndex(path, success, fail) {
      var deferred = $q.defer();

      var req = {
        method: 'GET',
        url: path,
        headers: {
          'Content-Type': 'text/plain'
        }
      };
      $http.get(req.url, req)
        .success(
          function(result) {
            if (success) success(result);
            deferred.resolve();
          }).error(
          function(result) {
            if (fail) fail(result);
            deferred.reject();
          }
        );

      return deferred.promise;
    }

    function getYamlResponse(response){
      return jsyaml.load(response.data);
    }

    this.getNavBar = function(){
      function getNavbarFailed(error) {
        console.log(error.data);
      }

      return $http.get(constants.TocFile)
      .then(getYamlResponse)
      .catch(getNavbarFailed);
    };

    this.getContent = function(path){

      function getContentFailed(error) {
        console.log(error.data);
      }

      return $http.get(path)
      .then(getYamlResponse)
      .catch(getContentFailed);
    };

    this.getMarkdownContent = function(path){

      function getContentComplete(response){
        return response.data;
      }

      function getContentFailed(error) {
        console.log(error.data);
      }

      return $http.get(path)
      .then(getContentComplete)
      .catch(getContentFailed);
    };

    this.getTocContent = function($scope, path, tocCache) {
      if (path) {
        path = urlService.normalizeUrl(path);
        var temp = tocCache.get(path);
        if (temp) {
          $scope.toc = temp;
        } else {
          $scope.toc = {
            path: path
          };
          asyncFetchIndex(path, function(result) {
            var content = jsyaml.load(result);
            var toc = {
              path: path,
              content: content
            };
            tocCache.put(path, toc);
            $scope.toc = toc;
          }, function() {
            var toc = {
              path: path,
            };
            tocCache.put(path, toc);
            $scope.toc = toc;
          });
        }
      } else {
        $scope.toc = undefined;
      }
    };

    this.getMdContent = function($scope, path, mdIndexCache) {
      if (!path) return;
      var pathInfo = urlService.getPathInfo(path);
      var mdPath = urlService.normalizeUrl((pathInfo.tocPath || '') + '/' + 'md.yaml');

      if (mdPath) {
        var tempMdIndex = mdIndexCache.get(mdPath);
        if (tempMdIndex) {
          if (tempMdIndex) $scope.mdIndex = tempMdIndex;
        } else {
          asyncFetchIndex(mdPath, function(result) {
            tempMdIndex = jsyaml.load(result);
            // This is the md file path
            mdIndexCache.put(mdPath, tempMdIndex);
            $scope.mdIndex = tempMdIndex;
          });
        }
      } else {
        $scope.toc = undefined;
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
    .service('contentService', ['$q', '$http', 'docConstants', 'urlService', provider]);

})();