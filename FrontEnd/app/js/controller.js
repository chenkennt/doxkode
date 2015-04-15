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

  function DocsCtrl($scope, $http, $q, $rootScope, $location, $window, $cookies, $timeout,
    NG_PAGES, NG_VERSION, NG_ITEMTYPES, styleProvider, contentService, urlService, docsMarkdownService, tocCache, mdIndexCache) {
    $scope.docsVersion = NG_VERSION.isSnapshot ? 'snapshot' : NG_VERSION.version;

    // TODO: Hacky, need change, how to bind this if move it to styleProvider?
    $scope.tocClass = function(navItem) {
      var tocClass = {
        current: navItem.href && this.pathInfo.contentPath === navItem.href,
        'nav-index-section': navItem.type === 'section'
      };
      if (tocClass.current === true) {
        $scope.navGroup = this.navGroup;
        $scope.navItem = this.navItem;
      }

      return tocClass;
    };

    $scope.gridClass = styleProvider.gridClass;

    $scope.navClass = styleProvider.navClass;

    var md = (function() {
      marked.setOptions({
        gfm: true,
        pedantic: false,
        sanitize: true
      });

      var toHtml = function(markdown) {
        if (!markdown)
          return '';

        return marked(markdown);
      };
      return {
        toHtml: toHtml
      };
    }());
    //This is used to convert all the markdown content into html when the page is loaded
    //But I just finished it in jquery, maybe someone can change it into angularjs style
    $scope.finishLoading = function() {
      $timeout(function() {
        //declaration
        var declarationElement = $("div.declaration-content").html(function() {
          var language = $(this).attr("ng-language");
          return '<pre><code class="lang-' + language + '">' + $(this).context.innerText + '</code></pre>';
        }).each(function(i, block) {
          hljs.highlightBlock(block);
        });
        //markdown
        var markdownElement = $("div.markdown").html(function() {
          var html = md.toHtml($(this).text());
          return html;
        });
        markdownElement.find("code").each(function(i, block) {
          // use highlight.js to highlight code
          hljs.highlightBlock(block);
          // Add try button
          block = block.parentNode;
          var wrapper = document.createElement("div");
          wrapper.className = "codewrapper";
          wrapper.innerHTML = '<div class="trydiv"><span class="tryspan">Try this code</span></div>';
          wrapper.childNodes[0].childNodes[0].onclick = function() {
            docsMarkdownService.tryCode(true, block.innerText);
          };
          block.parentNode.replaceChild(wrapper, block);
          wrapper.appendChild(block);
        });
        markdownElement.find("code").each(function(i, block) {
          var url = block.attributes['href'] && block.attributes['href'].value;
          if (!url) return;
          if (!urlService.isAbsoluteUrl(url))
            block.attributes['href'].value = urlService.getHref($scope, $location.path(), url);
        });
        markdownElement.find("code").each(function(i, block) {
          var url = block.attributes['src'] && block.attributes['src'].value;
          if (!url) return;
          if (!urlService.isAbsoluteUrl(url))
            block.attributes['src'].value = urlService.getAbsolutePath($location.path(), url);
        });
      }, 1, false);
    };

    $scope.getNumber = function(num) {
      return new Array(num + 1);
    };

    /*$scope.GetDetail = function(e) {
      var display = e.target.nextElementSibling.style.display;
      e.target.nextElementSibling.style.display = (display === 'block') ? 'none' : 'block';
    };*/

    $scope.ViewSource = function() {
      return urlService.getRemoteUrl(this.model.source, this.model.source.startLine + 1);
    };

    $scope.ImproveThisDoc = function() {
      return $scope.partialModel.mdContent;
    };

    // expand / collapse all logic for model items
    $scope.expandAll = function(state) {
      if ($scope.partialModel.items) {
        $scope.partialModel.items.forEach(function(e) {
          e.isCollapsed = state;
        });
      }
    };

    // Href relative to current toc file
    $scope.GetTocHref = function(url) {
      if (!$scope.toc) return '';
      return urlService.getHref($scope, $scope.toc.path, url);
    };

    // Href relative to current toc file
    $scope.GetBreadCrumbHref = function(url) {
      // For navbar url, no need to calculate relative path from toc
      if (url && url.indexOf('/#/') === 0) return url;
      if (!$scope.toc) return '';
      return urlService.getHref($scope, $scope.toc.path, url);
    };

    $scope.GetNavHref = function(url) {
      if (urlService.isAbsoluteUrl(url)) return url;
      if (!url) return '';
      return '#' + url;
    };

    // Href relative to current file
    $scope.GetLinkHref = function(url) {
      return urlService.getHref($scope, $location.path(), url);
    };

    $scope.$on('$includeContentLoaded', function() {
      // Add post actions when ng-include updated
    });

    contentService.getNavBar().then(function(data) {
      $scope.navbar = data;
      contentService.getDefaultItem($scope.navbar,
          function(defaultItem) {
            if (!$location.path() && defaultItem.href) $location.url(defaultItem.href);
          });
    });

    // #a/b/c!d/e/f => a/b/c/toc.yaml as toc, d/e/f as content
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
        contentService.getTocContent($scope, pathInfo.tocFilePath, tocCache);

        path = urlService.getContentFilePath(pathInfo);
        if (path) {
          // If end with .md
          if ((/\.md$/g).test(path)) {
            $scope.contentType = 'md';
            $scope.partialModel = {};
            $scope.title = path;
            $scope.partialPath = path;
          } else if ((/\.yaml$/g).test(path)) {
            $scope.contentType = 'yaml';
            // if is yaml
            // 1. try get md.yaml from the same path as toc, or current path if toc is not there
            contentService.getMdContent($scope, currentPage, mdIndexCache);

            contentService.getContent(path).then(function(data) {
                var model = $scope.partialModel = data;
                if (model instanceof Array) {
                  // toc list
                  $scope.partialPath = 'template' + '/tocpage.tmpl';
                } else {
                  $scope.title = model.id;
                  if (model.type.toLowerCase() === 'namespace') {
                    $scope.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.namespace, model.items);
                    $scope.partialPath = 'template' + '/namespace.tmpl';
                  } else {
                    $scope.itemtypes = urlService.setItemTypeVisiblity(NG_ITEMTYPES.class, model.items);
                    $scope.partialPath = 'template' + '/class.tmpl';
                  }
                }
              }).catch(
              function() {
                $scope.breadcrumb = [];
                $scope.partialPath = 'template/error404.tmpl';
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

    function mdIndexWatcher(path) {
      if ($scope.mdIndex && $scope.partialModel) {
        var mdPath = $scope.mdIndex[$scope.partialModel.id];
        if (mdPath) {
          if (mdPath.href) {
            $scope.partialModel.mdHref = urlService.getRemoteUrl(mdPath);
            var tocPath = urlService.getPathInfo($location.path()).tocPath;
            var href = (tocPath || '') + '/' + mdPath.href;
            var getMdIndex = contentService.getMarkdownContent(href).then(
              function(result) {
                var md = result.substr(mdPath.startLine, mdPath.endLine - mdPath.startLine + 1);
                $scope.partialModel.mdContent = md;
              });
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
          name: navName.id,
          // use '/#/' to indicate this is a nav link...
          url: '/#/' + currentNav
        });
      }
      if (currentGroup) {
        breadcrumb.push({
          name: currentGroup.id,
          url: currentGroup.href
        });

        // If toc does not exist, use navbar's title
        if (currentPage) {
          breadcrumb.push({
            name: currentPage.id,
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

    /**********************************
     Initialize
     ***********************************/

    $scope.versionNumber = angular.version.full;
    $scope.version = angular.version.full + "  " + angular.version.codeName;
    $scope.loading = 0;
  }

  angular.module('docascode.controller', ['docascode.contentService', 'docascode.urlService', 'docascode.styleProvider', 'docascode.directives'])
    .factory('tocCache', ['$cacheFactory', function($cacheFactory) {
      return $cacheFactory('toc-cache');
    }])
    .factory('mdIndexCache', ['$cacheFactory', function($cacheFactory) {
      return $cacheFactory('mdIndex-cache');
    }])
    .controller('DocsController', [
      '$scope', '$http', '$q', '$rootScope', '$location', '$window', '$cookies', '$timeout',
      'NG_PAGES', 'NG_VERSION', 'NG_ITEMTYPES','styleProvider', 'contentService', 'urlService', 'docsMarkdownService', 'tocCache', 'mdIndexCache',
      DocsCtrl
    ]);

})();