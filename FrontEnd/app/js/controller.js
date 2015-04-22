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

  angular.module('docascode.controller', ['docascode.contentService', 'docascode.urlService', 'docascode.directives', 'docascode.constants'])
    .controller('DocsController', [
      '$scope', '$http', '$q', '$rootScope', '$location', '$window', '$cookies', '$timeout',
      'NG_PAGES', 'NG_VERSION', 'NG_ITEMTYPES', 'contentService', 'urlService', 'docConstants',
      DocsCtrl
    ]);

  function DocsCtrl($scope, $http, $q, $rootScope, $location, $window, $cookies, $timeout,
    NG_PAGES, NG_VERSION, NG_ITEMTYPES, contentService, urlService, docConstants) {

    /**********************************
     Initialize
     **********************************/

    $scope.versionNumber = angular.version.full;
    $scope.version = angular.version.full + "  " + angular.version.codeName;
    $scope.loading = 0;
    $scope.docsVersion = NG_VERSION.isSnapshot ? 'snapshot' : NG_VERSION.version;
    $scope.tocClass = tocClass;
    $scope.navClass = navClass;

    $scope.expandAll = expandAll;
    $scope.getViewSourceHref = getViewSourceHref;
    $scope.getTocHref = getTocHref;
    $scope.getBreadCrumbHref =getBreadCrumbHref;
    $scope.getNavHref = getNavHref;
    $scope.getLinkHref = getLinkHref;
    $scope.filteredItems = filteredItems;
    $scope.getNumber = function(num) { return new Array(num + 1); };

    /****************************************
     element ng-class related Implementation
     ****************************************/
    function tocClass(navItem) {
      /* jshint validthis: true */
      var current = {
        current: navItem.href && this.pathInfo.contentPath === navItem.href,
        'nav-index-section': navItem.type === 'section'
      };
      if (current.current === true) {
        $scope.navGroup = this.navGroup;
        $scope.navItem = this.navItem;
      }

      return current;
    }

    function navClass(navItem) {
      /* jshint validthis: true */
      var navPath;
      if (this.pathInfo) {
        navPath = urlService.normalizeUrl(this.pathInfo.tocPath || this.pathInfo.contentPath);
      }

      return {
        current: navPath && navPath === navItem.href,
      };
    }

    /**************************************
     element button related Implementation
     **************************************/
    function getViewSourceHref(){
      /* jshint validthis: true */
      return urlService.getRemoteUrl(this.model.source, this.model.source.startLine + 1);
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

    // Href relative to current toc file
    function getTocHref(url) {
      if (!$scope.toc) return '';
      return urlService.getHref($scope, $scope.toc.path, url);
    }

    // Href relative to current toc file
    function getBreadCrumbHref(url) {
      // For navbar url, no need to calculate relative path from toc
      if (url && url.indexOf('/#/') === 0) return url;
      if (!$scope.toc) return '';
      return urlService.getHref($scope, $scope.toc.path, url);
    }

    function getNavHref(url) {
      if (urlService.isAbsoluteUrl(url)) return url;
      if (!url) return '';
      return '#' + url;
    }

    // Href relative to current file
    function getLinkHref(url) {
      return urlService.getHref($scope, $location.path(), url);
    }

    function filterNavItem(name, text) {
      if (!text) return true;
      if (name.toLowerCase().indexOf(text.toLowerCase()) > -1) return true;
      return false;
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

    contentService.getNavBar().then(function(data) {
      $scope.navbar = data;
      contentService.getDefaultItem($scope.navbar,
          function(defaultItem) {
            if (!$location.path() && defaultItem.href) $location.url(defaultItem.href);
          });
    });

    // #a/b/c!d/e/f => a/b/c/toc.yml as toc, d/e/f as content
    $scope.$watch(function docsPathWatch() {
      return $location.path();
    }, function docsPathWatchAction(path) {
      path = path.replace(/^\/?(.+?)(\/index)?\/?$/, '$1');
      $scope.navGroup = null;
      $scope.navItem = null;
      var currentPage = $scope.currentPage = path; //NG_PAGES[path];
      // TODO: check if it is inside NG_PAGES
      // If current page exists in NG_PAGES
      if (currentPage) {
        var pathInfo = urlService.getPathInfo(currentPage);
        $scope.pathInfo = pathInfo;
        contentService.getTocContent(pathInfo.tocFilePath).then(
          function(data){
            $scope.toc = data;
          });

        path = urlService.getContentFilePath(pathInfo);
        if (path) {
          // If end with .md
          if ((/\.md$/g).test(path)) {
            $scope.contentType = 'md';

            var partialModel = {
              path: path,
              title: path,
            };

            $scope.partialModel = partialModel;
          } else if ((docConstants.YamlRegexExp).test(path)) {
            $scope.contentType = 'yaml';
            // if is yaml
            // 1. try get md.yaml from the same path as toc, or current path if toc is not there
            contentService.getMdContent(currentPage).then(function(data){
              $scope.mdIndex = data;
            });

            contentService.getContent(path).then(function(data) {
                $scope.partialModel = partialModelHandler(data);
              }).catch(
              function() {
                $scope.partialModel = {
                  path : 'template/error404.tmpl',
                };
                $scope.breadcrumb = [];
              }
            );
          } else {
            // If not md or yaml, simply try load the path
            $scope.partialPath = path;
          }
        }
      } else {
        if ($scope.navbar && $scope.navbar.length > 0) {
          $location.url($scope.navbar[0].href);
        }
      }
    });

    function partialModelHandler(data) {
      var partialModel = {
        model: undefined,
        path: undefined,
        title: undefined,
        itemtypes: undefined,
      };
      if (data instanceof Array) {
        // toc list
        partialModel.path = 'template' + '/tocpage.tmpl';
        partialModel.model = data;
      } else {
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
          partialModel.path = 'template' + '/namespace.tmpl';
        } else {
          partialModel.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.class, item.items);
          partialModel.path = 'template' + '/class.tmpl';
        }
      }
      return partialModel;
    }

    function getItemWithSameUidFunction(child){
      return function(x) {
              return x.uid === child;
            };
    }

    function getMdItemIndex(item, tocPath, mdPath, mdInitial, mdResolved) {
        var itemHref = (tocPath || '') + '/' + item.href;
        return contentService.getMarkdownContent(itemHref).then(
               function (res) {
                var snippet = res;
                if (item.referenceStartLine != null && item.referenceEndLine != null) {
                    var lines = snippet.split('\n');
                    snippet = "";
                    for (var i = item.referenceStartLine - 1; i < item.referenceEndLine; i++) {
                        snippet += lines[i];
                    }
                }
                return mdResolved.replace(mdInitial.substr(item.startLine - mdPath.startLine, item.endLine - item.startLine + 1), snippet);
            });
    }

    function makeThenFunction(item, tocPath, mdPath, mdInitial){
      return function(md){ return getMdItemIndex(item, tocPath, mdPath, mdInitial, md);};
    }
    // TODO: move to directive?
    // BUG? method's binding works? looks like not
    function mdIndexWatcher(path) {
        if ($scope.mdIndex && $scope.partialModel) {
          var partialModel = $scope.partialModel.model;
          if (partialModel){
            var mdPath = $scope.mdIndex[partialModel.id];
            if (mdPath) {
                if (mdPath.href) {
                    partialModel.mdHref = urlService.getRemoteUrl(mdPath);
                    var tocPath = urlService.getPathInfo($location.path()).tocPath;
                    var href = (tocPath || '') + '/' + mdPath.href;
                    var getMdIndex = contentService.getMarkdownContent(href).then(
                      function(result) {
                          var md = result.substr(mdPath.startLine, mdPath.endLine - mdPath.startLine + 1);
                          if (mdPath.items) {
                              var promise = contentService.valueHttpWrapper(md);
                              for (var i = 0; i < mdPath.items.length; i++) {
                                  var item = mdPath.items[i];
                                  promise = promise.then(makeThenFunction(item, tocPath, mdPath, md));
                              }
                              promise.then(function(md){
                                partialModel.mdContent = md;
                              });
                          }
                          else{
                              partialModel.mdContent = md;
                          }
                      });
                }
            }
          }
        }
    }

    function breadCrumbWatcher(currentGroup, currentPage, currentPath) {
      // breadcrumb generation logic
      var breadcrumb = $scope.breadcrumb = [];
      var pathInfo = urlService.getPathInfo(currentPath);

      var currentNav = urlService.normalizeUrl(pathInfo.tocPath || pathInfo.contentPath);
      var navbar = $scope.navbar;
      if (currentNav && navbar) {
        var navName = navbar.filter(function(x) {
          return urlService.normalizeUrl(x.href) === currentNav;
        })[0] || {};

        breadcrumb.push({
          name: navName.name,
          // use '/#/' to indicate this is a nav link...
          url: '/#/' + currentNav
        });
      }
      if (currentGroup) {
        breadcrumb.push({
          name: currentGroup.uid,
          url: currentGroup.href
        });

        // If toc does not exist, use navbar's title
        if (currentPage) {
          breadcrumb.push({
            name: currentPage.name,
            url: currentPage.href
          });
        }
      }
    }

    $scope.$watch(function modelWatch() {
      return $scope.partialModel;
    }, function() {
      mdIndexWatcher();
      breadCrumbWatcher($scope.navGroup, $scope.navItem, $location.path());
    });

    $scope.$watch(function modelWatch() {
      return $scope.navbar;
    }, function(navbar) {
      breadCrumbWatcher($scope.navGroup, $scope.navItem, $location.path());
    });

    $scope.$watch(function modelWatch() {
      return $scope.navGroup;
    }, function(navGroup) {
      breadCrumbWatcher(navGroup, $scope.navItem, $location.path());
    });

    $scope.$watch(function modelWatch() {
      return $scope.navItem;
    }, function(navItem) {
      breadCrumbWatcher($scope.navGroup, navItem, $location.path());
    });

    $scope.$watch(function modelWatch() {
      return $scope.mdIndex;
    }, mdIndexWatcher);

    // listen for toc change
    $scope.$watch(function modelWatch() {
      return $scope.navbar;
    }, function modelWatchAction(navbar) {
      if (!$location.path() && navbar && navbar.count > 0) {
        $location.url(navbar[0].href);
      }
    });

  }

})();