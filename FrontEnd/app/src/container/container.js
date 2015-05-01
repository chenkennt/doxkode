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

(function() {
  'use strict';

  angular.module('docascode.controller')
    .controller('ContainerController', [
      '$rootScope', '$scope', '$location', 'NG_ITEMTYPES', 'contentService', 'urlService', 'docConstants',
      ContainerController
    ]);

  function ContainerController($rootScope, $scope, $location, NG_ITEMTYPES, contentService, urlService, docConstants) {

    // TODO: merge TOC&content loading to an animation/directive
    $scope.loading = 0;
    $scope.contentLoading = 0;
    $scope.tocLoading = 0;

    $scope.expandAll = expandAll;
    $scope.filteredItems = filteredItems;
    $scope.tocClass = tocClass;
    $scope.getTocHref = getTocHref;
    $scope.getViewSourceHref = getViewSourceHref;
    $scope.getLinkHref = getLinkHref;
    $scope.getNumber = function(num) { return new Array(num + 1); };

    $scope.toc = null;
    $scope.content = null;
    $rootScope.$on("navbarLoaded", function(event, args){

    });
    $scope.$watch(function(){return $location.path();}, function(path){
      loadContent(path);
    });

    $rootScope.$on("activeNavItemChanged", function(event, args){
      $scope.toc = null;
      var currentNavItem = args.active;
      $scope.currentHomepage = currentNavItem.homepage;
      var currentNavbar = args.active;
      var tocPath = currentNavbar.href + '/' + docConstants.TocFile;

      contentService.getTocContent(tocPath).then(
        function(data){
          if (data.content){
            $scope.toc = data;
          }
        });
    });
    
    $scope.$watchGroup(['tocPage', 'currentHomepage'], function(newValues, oldValues){
      if (!newValues) return;
      var tocPage = newValues[0];
      var currentHomepage = newValues[1];
      if (!tocPage) return;
      var partialModel = $scope.partialModel;
      if (!partialModel) partialModel = $scope.partialModel = {};
      // If current homepage exists, use homepage
      if (currentHomepage) {
        partialModel.path = currentHomepage;
        partialModel.contentType = 'md';
      } else {
        contentService.getContent(tocPage).then(function (data) {
          // If path is already set to homepage, return;
          if (partialModel.path) return;
          
          // toc list
          partialModel.path = 'template/tocpage.html';
          partialModel.model = data;
          partialModel.contentType = 'yaml';
        }).catch(
          function () {
            $scope.partialModel = {
              path: 'template/error404.html',
            };
          }
          );
      }
    });

    function loadContent(url){
        if (!url) return;
        var pathInfo = urlService.getPathInfo(url);
        var path = urlService.getContentFilePath(pathInfo);
        if (path) {
          // If is toc.yml and home page exists, set to $scope and return
          // TODO: refactor using ngRoute
          $scope.tocPage = (docConstants.TocYamlRegexExp).test(path) ? path : '';
          if ($scope.tocPage) return;
          
          // If end with .md
          if ((docConstants.MdRegexExp).test(path)) {
            var partialModel = {
              contentType: 'md',
              path: path,
              title: path,
            };

            $scope.partialModel = partialModel;
          } else if ((docConstants.YamlRegexExp).test(path)) {
            // if is yaml
            // 1. try get md.yaml from the same path as toc, or current path if toc is not there
            contentService.getMdContent(path).then(function(data){
              $scope.mdIndex = data;
            });

            contentService.getContent(path).then(function(data) {
                $scope.partialModel = partialModelHandler(data);
              }).catch(
              function() {
                $scope.partialModel = {
                  path : 'template/error404.html',
                };
                $scope.breadcrumb = [];
              }
            );
          } else {
            // If not md or yaml, simply try load the path
            $scope.partialPath = path;
          }
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

    // Href relative to current file
    function getLinkHref(url) {
      var currentPath = $location.path();
      var pathInfo = urlService.getPathInfo(currentPath);
      return urlService.getHref(pathInfo.tocPath, pathInfo.contentPath, url);
    }

    function filteredItems(f) {
      /* jshint validthis: true */
      var globalVisible = !f;
      this.model.forEach(function(a, i, o) {
        // show namespace if any of its child is visible
        // show all the children if the namespace is visible
        var firstLevelTocName = a.uid || a.name;
        var hide = !globalVisible && !filterNavItem(firstLevelTocName, f);
        var tempHide = hide;
        if (a.items){
          a.items.forEach(function(a1, i1, o1){
            // support firstLevel.lastLevel format seach
            var lastLevelFullName = firstLevelTocName + '.' + a1.name;
            a1.hide = tempHide && !filterNavItem(lastLevelFullName, f);
            if (!a1.hide){
              // show firstLevel if any of its children is visible
              hide = false;
            }
          });
        }

        a.hide = hide;
      });
    }

    function filterNavItem(name, text) {
      if (!text) return true;
      if (name.toLowerCase().indexOf(text.toLowerCase()) > -1) return true;
      return false;
    }

    /****************************************
     element ng-class related Implementation
     ****************************************/
    function tocClass(selectedItem) {
      /* jshint validthis: true */
      var currentPath = $location.path();
      var pathInfo = urlService.getPathInfo(currentPath);

      var current = {
        active: selectedItem.href && pathInfo.contentPath === selectedItem.href,
        'nav-index-section': selectedItem.type === 'section'
      };

      if (current.active === true) {
        var currentTocSelectedItem = $scope.currentTocSelectedItem;
        if (selectedItem && currentTocSelectedItem !== selectedItem){
          $scope.currentTocSelectedItem = currentTocSelectedItem = selectedItem;
          var selectedItemInfo = {
            active: selectedItem,
          };

          /* Use this.navGroup and this.navItem to get current selected item and its parent if has */
          if (selectedItem === this.navItem){
            selectedItemInfo.parent = this.navGroup;
          }

          $rootScope.$broadcast("activeTocItemChanged", selectedItemInfo);
        }
      }
      return current;
    }

    function getViewSourceHref(){
      /* jshint validthis: true */
      return urlService.getRemoteUrl(this.model.source, this.model.source.startLine + 1);
    }

    function partialModelHandler(data) {
      var partialModel = {
        model: undefined,
        path: undefined,
        title: undefined,
        itemtypes: undefined,
        contentType: 'yaml'
      };
      {
        var items = data.items;
        var references = data.references || [];

        // TODO: what if items are not in order? what if items are not arranged as expected, e.g. multiple namespaces in one yml?
        var item = items[0];
        references = items.slice(1).concat(references || []);
        if (item.children){
          var children = [];
          for(var i = 0, l = item.children.length; i < l; i++) {
            var matched = references.filter(getItemWithSameUidFunction(item.children[i]))[0] || {};
            if (matched.uid){
              children.push(matched);
            }
          }
          item.items = children;
        }

        partialModel.model = item;
        partialModel.title = item.name;
        if (item.type.toLowerCase() === 'namespace') {
          partialModel.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.namespace, item.items);
          partialModel.path = 'template' + '/namespace.html';
        } else {
          partialModel.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.class, item.items);
          partialModel.path = 'template' + '/class.html';
        }
      }
      return partialModel;
    }

    function getItemWithSameUidFunction(child){
      return function(x) {
              return x.uid === child;
            };
    }
    
    // expand / collapse all logic for model items
    function expandAll(state) {
      var partialModel = $scope.partialModel? $scope.partialModel.model : null;
      if (partialModel && partialModel.items) {
        partialModel.items.forEach(function(e) {
          e.showDetail = state;
        });
      }
    }
  }
})();