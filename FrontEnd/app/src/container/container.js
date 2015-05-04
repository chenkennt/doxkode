/* global angular */
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

    $scope.filteredItems = filteredItems;
    $scope.tocClass = tocClass;
    $scope.getTocHref = getTocHref;
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
      var scope = $scope;
      
      // If current homepage exists, use homepage
      if (currentHomepage) {
        scope.contentPath = currentHomepage;
        scope.contentType = 'markdown';
      } else {
        scope.contentPath = tocPage;
        scope.contentType = 'toc';
      }
    });

    function loadContent(url){
        if (!url) return;
        var pathInfo = urlService.getPathInfo(url);
        var path = urlService.getContentFilePath(pathInfo);
        var scope = $scope;
        if (path) {
          // If is toc.yml and home page exists, set to $scope and return
          // TODO: refactor using ngRoute
          scope.tocPage = (docConstants.TocYamlRegexExp).test(path) ? path : '';
          if (scope.tocPage) return;
          scope.contentPath = path;
          
          // If end with .md
          if ((docConstants.MdRegexExp).test(path)) {
            // TODO: Improve TITLE
            scope.title = path;
            scope.contentType = 'markdown';
          } else if ((docConstants.YamlRegexExp).test(path)) {
            scope.contentType = 'yaml';
          }
          // If not md or yaml, simply try load the path
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
  }
})();