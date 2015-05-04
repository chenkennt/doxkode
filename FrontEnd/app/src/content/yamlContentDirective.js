/**
 * @ngdoc directive
 * @name docascode.directive:yamlContent
 * @restrict E
 * @element ANY
 * @priority 1000
 * @scope
 * 
 * @description yamlContent directive to help render markdown pages
 **/

(function () {
  'use strict';

  angular.module('docascode.directives')
  /**
   * yamlContent Directive
   * @param  {Function} contentService
   * @param  {Function} markdownService
   * @param  {Function} urlService
   *
   * @description Render a page with .md file, supporting try...code
   */
    .directive('yamlContent', ['NG_ITEMTYPES', '$location', 'contentService', 'markdownService', 'urlService', yamlContent]);
 
  function yamlContent(NG_ITEMTYPES, $location, contentService, markdownService, urlService) {

    function getItemWithSameUidFunction(child) {
      return function (x) {
        return x.uid === child;
      };
    }

    function getViewSourceHref(model) {
      /* jshint validthis: true */
      if (!model || !model.source) return '';
      return urlService.getRemoteUrl(model.source, model.source.startLine + 1);
    }

    function getNumber(num) {
      return new Array(num + 1);
    }
  
    // Href relative to current file
    function getLinkHref(url) {
      var currentPath = $location.path();
      var pathInfo = urlService.getPathInfo(currentPath);
      return urlService.getHref(pathInfo.tocPath, pathInfo.contentPath, url);
    }

    // expand / collapse all logic for model items
    function expandAll(model, state) {
      if (model && model.items) {
        model.items.forEach(function(e) {
          e.showDetail = state;
        });
      }
    }
    
    /**************************************/
 
    function render(scope, element, yamlFilePath, loadMapFile) {
      if (!yamlFilePath) return;

      contentService.getContent(yamlFilePath).then(function (data) {
        // If data is array, current page is toc page;
        if (angular.isArray(data)){
          scope.contentType = 'toc';
          scope.model = data;
          return;
        }
        var items = data.items;
        var references = data.references || [];

        // TODO: what if items are not in order? what if items are not arranged as expected, e.g. multiple namespaces in one yml?
        var item = items[0];
        references = items.slice(1).concat(references || []);
        if (item.children) {
          var children = [];
          for (var i = 0, l = item.children.length; i < l; i++) {
            var matched = references.filter(getItemWithSameUidFunction(item.children[i]))[0] || {};
            if (matched.uid) {
              children.push(matched);
            }
          }
          item.items = children;
        }
        
        if (item.type.toLowerCase() === 'namespace') {
          scope.contentType = 'namespace';
          scope.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.namespace, item.items);
        } else {
          scope.contentType = 'class';
          scope.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.class, item.items);
        }
        
        scope.model = item;
        scope.title = item.name;
      }).catch(
      // Set to error messages in the page
        );

      if (loadMapFile) {
        var mapFilePath = yamlFilePath + ".map";
//        console.log("Start loading map file" + mapFilePath);
        contentService.getMdContent(mapFilePath)
          .then(
          function (result) {
            if (!result) return;
            var data = result.data;
            // TODO: change md.map's key to "default" to make it much easier
            for (var key in data) {
              if (data.hasOwnProperty(key)) {
                var value = data[key];
                if (value.remote && value.remote.repo) {
                }
              }
            }
          },
          function (result) {

          }
          );
      }

    }
    
    // Href relative to current toc file
    function getTocHref(url) {
      // if (!$scope.model) return '';
      var currentPath = $location.path();
      var pathInfo = urlService.getPathInfo(currentPath);
      pathInfo.contentPath = '';
      return urlService.getHref(pathInfo.tocPath, '', url);
    }
    
    function YamlContentController($scope){
      $scope.getViewSourceHref = getViewSourceHref;
      $scope.getLinkHref = getLinkHref;
      $scope.expandAll = expandAll;
      $scope.getNumber = getNumber;
      $scope.getTocHref = getTocHref;
    }
    
    YamlContentController.$inject = ['$scope'];
    return {
      restrict: 'E',
      replace: true,
      templateUrl: 'template/yamlContent.html',
      priority: 100,
      require: 'ngModel',
      scope: {
        getMap: "=",
        contentType: "@",
      },
      controller: YamlContentController,
      link: function (scope, element, attrs, ngModel) {
        var localScope = scope;
        scope.$watch(function () { return ngModel.$modelValue; }, function (value, oldValue) {
          if (value === undefined) return;
          render(localScope, element, value, localScope.getMap);
        });
      }
    };
  }
})();