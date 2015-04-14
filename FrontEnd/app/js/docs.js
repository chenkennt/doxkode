var player;


angular.module('docsApp', [
  'ngRoute',
  'ngCookies',
  'ngSanitize',
  'docConstants',
  'docUtility',
  'docCtrl',
  'versionsData',
  'pagesData',
  'itemTypes',
  'directives',
  'errors',
  'versions',
  'bootstrap',
  'ui.bootstrap',
  'ui.bootstrap.dropdown',
]);

angular.module('docCtrl', ['docInitService', 'docUtility'])
  .factory('tocCache', ['$cacheFactory', function($cacheFactory) {
    return $cacheFactory('toc-cache');
  }])
  .factory('mdIndexCache', ['$cacheFactory', function($cacheFactory) {
    return $cacheFactory('mdIndex-cache');
  }])
  .controller('DocsController', [
    '$scope', '$http', '$q', '$rootScope', '$location', '$window', '$cookies', '$timeout',
    'NG_PAGES', 'NG_VERSION', 'NG_ITEMTYPES', 'docService', 'tocCache', 'mdIndexCache', 'docUtility', 'docsMarkdownService',
    function ($scope, $http, $q, $rootScope, $location, $window, $cookies, $timeout,
      NG_PAGES, NG_VERSION, NG_ITEMTYPES, docService, tocCache, mdIndexCache, docUtility, docsMarkdownService) {
      'use strict';

      $scope.docsVersion = NG_VERSION.isSnapshot ? 'snapshot' : NG_VERSION.version;

      $scope.tocClass = docService.tocClassApi;
      $scope.gridClass = docService.gridClassApi;

      $scope.navClass = docService.navClassApi;

      var md = (function () {
          marked.setOptions({
              gfm: true,
              pedantic: false,
              sanitize: true
          });

          var toHtml = function (markdown) {
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
      $scope.finishLoading = function () {
          $timeout(function () {
              //declaration
              var declarationElement = $("div.declaration-content").html(function () {
                  var language = $(this).attr("ng-language");
                  return '<pre><code class="lang-' + language + '">' + $(this).context.innerText + '</code></pre>';
              }).each(function (i, block) {
                  hljs.highlightBlock(block);
              });
              //markdown
              var markdownElement = $("div.markdown").html(function () {
                  var html = md.toHtml($(this).text());
                  return html;
              });
              markdownElement.find("code").each(function (i, block) {
                  // use highlight.js to highlight code
                  hljs.highlightBlock(block);
                  // Add try button
                  block = block.parentNode;
                  var wrapper = document.createElement("div");
                  wrapper.className = "codewrapper";
                  wrapper.innerHTML = '<div class="trydiv"><span class="tryspan">Try this code</span></div>';
                  wrapper.childNodes[0].childNodes[0].onclick = function () {
                      docsMarkdownService.tryCode(true, block.innerText);
                  };
                  block.parentNode.replaceChild(wrapper, block);
                  wrapper.appendChild(block);
              });
              markdownElement.find("code").each(function (i, block) {
                  var url = block.attributes['href'] && block.attributes['href'].value;
                  if (!url) return;
                  if (!docService.isAbsoluteUrl(url))
                      block.attributes['href'].value = docService.getHref($scope, $location.path(), url);
              });
              markdownElement.find("code").each(function (i, block) {
                  var url = block.attributes['src'] && block.attributes['src'].value;
                  if (!url) return;
                  if (!docService.isAbsoluteUrl(url))
                      block.attributes['src'].value = docService.getAbsolutePath($location.path(), url);
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
        return docService.getRemoteUrl(this.model.source, this.model.source.startLine + 1);
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
        return docService.getHref($scope, $scope.toc.path, url);
      };

      $scope.GetNavHref = function(url) {
        if (docService.isAbsoluteUrl(url)) return url;
        if (!url) return '';
        return '#' + url;
      };

      // Href relative to current file
      $scope.GetLinkHref = function(url) {
        return docService.getHref($scope, $location.path(), url);
      };

      $scope.$on('$includeContentLoaded', function() {
        // Add post actions when ng-include updated
      });

      (function getNavbar() {
        // load navigation bar, should be toc.yaml in root path
        // TODO: support toc.md => extract <h><a> from marked(toc.md)
        var navBarPath = "toc.yaml";
        docService.asyncFetchIndex(navBarPath, function(result) {
          $scope.navbar = jsyaml.load(result);

          // Load first item as the default page
          docService.getDefaultItem($scope.navbar,
            function(defaultItem) {
              if (!$location.path() && defaultItem.href) $location.url(defaultItem.href);
            });
        });
      })();

      // #a/b/c!d/e/f => a/b/c/toc.yaml as toc, d/e/f as content
      $scope.$watch(function docsPathWatch() {
        return $location.path();
      }, function docsPathWatchAction(path) {
        path = path.replace(/^\/?(.+?)(\/index)?\/?$/, '$1');

        var currentPage = $scope.currentPage = path; //NG_PAGES[path];

        // TODO: check if it is inside NG_PAGES
        // If current page exists in NG_PAGES
        if (currentPage) {
          var pathInfo = docService.getPathInfo(currentPage);
          $scope.pathInfo = pathInfo;
          docService.getTocContent($scope, pathInfo.tocFilePath, tocCache);

          path = docService.getContentFilePath(pathInfo);
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
              docService.getMdContent($scope, currentPage, mdIndexCache);

              docService.asyncFetchIndex(path, function(result) {
                  var model = $scope.partialModel = jsyaml.load(result);
                  if (model instanceof Array) {
                    // toc list
                    $scope.partialPath = 'template' + '/tocpage.tmpl';
                  } else {
                  $scope.title = model.id;
                    if (model.type.toLowerCase() === 'namespace') {
                      $scope.itemtypes = docService.setItemTypeVisiblity(NG_ITEMTYPES.namespace, model.items);
                      $scope.partialPath = 'template' + '/namespace.tmpl';
                    } else {
                      $scope.itemtypes = docService.setItemTypeVisiblity(NG_ITEMTYPES.class, model.items);
                      $scope.partialPath = 'template' + '/class.tmpl';
                    }
                  }
                },
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

          // breadcrumb generation logic
          var breadcrumb = $scope.breadcrumb = [];
          var currElem = document.getElementsByClassName("current");
          if (currElem[0]) {
            breadcrumb.push({
              name: currElem[0].firstElementChild.innerText,
              url: currElem[0].firstElementChild.href
            });
          }
          if (currElem[1]) {
            if (currElem[1].parentNode.parentNode.nodeName === 'LI') {
              breadcrumb.push({
                name: currElem[1].parentNode.parentNode.firstElementChild.innerText,
                url: currElem[1].parentNode.parentNode.firstElementChild.href
              });
            }
            breadcrumb.push({
              name: currElem[1].firstElementChild.innerText,
              url: currElem[1].firstElementChild.href
            });
          }

          /*var pathParts = pathInfo.tocPath ? pathInfo.tocPath.split('/') : [];
          if (pathInfo.contentPath) pathParts.push('!' + pathInfo.contentPath);
          var breadcrumb = $scope.breadcrumb = [];
          var breadcrumbPath = '';
          angular.forEach(pathParts, function(part) {
            if (part) {
              breadcrumbPath += part;
              breadcrumb.push({
                name: part,
                url: breadcrumbPath
              });
              breadcrumbPath += '/';
            }
          });*/

        } else {
          if ($scope.navbar && $scope.navbar.length > 0) {
            $location.url($scope.navbar[0].href);
          }
        }
      });

      function mdIndexWatcher(path){
        if ($scope.mdIndex && $scope.partialModel) {
          var mdPath = $scope.mdIndex[$scope.partialModel.id];
          if (mdPath) {
            if (mdPath.href) {
              $scope.partialModel.mdHref = docService.getRemoteUrl(mdPath);
              var tocPath = docService.getPathInfo($location.path()).tocPath;
              var href = (tocPath || '') + '/' + mdPath.href;
              var getMdIndex = docService.asyncFetchIndex(href,
                function(result) {
                  var md = result.substr(mdPath.startLine, mdPath.endLine - mdPath.startLine + 1);
                  $scope.partialModel.mdContent = md;
                });
            }
          }
        }
      }

      $scope.$watch(function modelWatch() {
        return $scope.partialModel ;
      }, mdIndexWatcher);

      $scope.$watch(function modelWatch() {
        return $scope.mdIndex ;
      }, mdIndexWatcher);

      // listen for toc change
      // $scope.$watch(function modelWatch() {
      //   return $scope.toc;
      // }, function modelWatchAction(toc) {
      //   if (toc && toc.content) {
      //     var info = docService.getPathInfo($location.path());
      //     if (!info.contentPath) {
      //       docService.getDefaultItem(toc.content,
      //         function(defaultItem) {
      //           if (defaultItem && defaultItem.href) {
      //             $location.url(docService.getContentUrlWithTocAndContentUrl(toc.path, defaultItem.href));
      //           }
      //         });
      //     }
      //   }
      // });

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
  ]);