/*
 * Define the main functionality used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 * Controller is *class*
 * Use controllers to
 *   * Set up the initial state of the $scope object
 *   * Add behavior to the $scope object
 * DONOT use controllers to
 *   * Manipulate DOM -- Controllers should contain only business logic
 *   * Format input -- Use *form* controls instead
 *   * Filter output -- Use *filter* instad
 *   * Share code or state across controllers -- Use *service* instead
 *   * Manage the life-cycle of other components
 */

(function () {
  'use strict';

  angular.module('docascode.controller', ['docascode.contentService', 'docascode.urlService', 'docascode.directives', 'docascode.util', 'docascode.constants'])
    .controller('DocsController', [
    '$rootScope', '$scope', '$location', 'NG_ITEMTYPES', 'contentService', 'urlService', 'docUtility', 'docConstants',
    DocsController
  ]);

  function DocsController($rootScope, $scope, $location, NG_ITEMTYPES, contentService, urlService, docUtility, docConstants) {
    $rootScope.$on("activeNavItemChanged", function (event, args) {
      var selectedTocItem = args.active;
      var parent = args.parent;

    });
    $rootScope.$on("activeTocItemChanged", function (event, args) {
      var selectedTocItem = args.active;
      var parent = args.parent;
    });

    // watch for resize and reset height of side section
    /*$(window).resize(function () {
      $scope.$apply(function () {
        bodyOffset();
      });
    });*/

    function getMdItemIndex(item, tocPath, mdPath, mdInitial, mdResolved) {
      var itemHref = (tocPath || '') + '/' + item.href;
      return contentService.getMarkdownContent(itemHref).then(
        function (res) {
          var snippet = docUtility.substringLine(res, item.startLine, item.endLine);
          return mdResolved.replace(mdInitial.substr(item.startLine - mdPath.startLine, item.endLine - item.startLine + 1), snippet);
        });
    }

    function makeThenFunction(item, tocPath, mdPath, mdInitial) {
      return function (md) { return getMdItemIndex(item, tocPath, mdPath, mdInitial, md); };
    }
      
      
    // TODO: move to directive?
    // BUG? method's binding works? looks like not
    function mdIndexWatcher(path) {
      if ($scope.mdIndex && $scope.partialModel) {
        var partialModel = $scope.partialModel.model;
        if (partialModel) {
          var mapItem = $scope.mdIndex[partialModel.id];
          if (mapItem) {
            if (mapItem.href) {
              partialModel.mdHref = urlService.getRemoteUrl(mapItem);
              var tocPath = urlService.getPathInfo($location.path()).tocPath;
              var href = (tocPath || '') + '/' + mapItem.href;
              // Get markdown content
              contentService.getMarkdownContent(href).then(
                function (result) {
                  var md = docUtility.substringLine(result, mapItem.startLine, mapItem.endLine);
                  if (mapItem.references) {
                    var promise = contentService.valueHttpWrapper(md);
                    for (var i = 0; i < mapItem.items.length; i++) {
                      var item = mapItem.references[i];

                      promise = promise.then(makeThenFunction(item, tocPath, mapItem, md));
                    }
                    promise.then(function (md) {
                      partialModel.mdContent = md;
                    });
                  }
                  else {
                    partialModel.mdContent = md;
                  }
                });
            }
          }
        }
      }
    }

    /*function bodyOffset() {
      var navHeight = $('.topnav').height() + $('.subnav').height();
      $('.sidefilter').css('top', navHeight + 'px');
      $('.sidetoc').css('top', navHeight + 60 + 'px');
      $('.article').css('margin-top', navHeight + 30 + 'px');
    }*/
  }

})();