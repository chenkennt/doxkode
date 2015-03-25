var player;

function createPlayer() {
  var player = csplay.play("player", "http://dotnetsandbox.azurewebsites.net" /* hardcode for now */);
  player.editor.setTheme("ace/theme/ambiance");
  player.editor.setFontSize(16);
  $("#run").click(function () {
    var that = $(this);
    that.html('<i class="fa fa-refresh fa-fw fa-spin"></i>Run');
    that.addClass("disabled");
    player.run({
      complete: function () {
        that.text("Run");
        that.removeClass("disabled");
      }
    });
  });
  $("#close").click(function () {
    tryCode(false);
  });
  return player;
}

function tryCode(enable, code) {
  if (enable) {
    if (typeof player == "undefined") {
      player = createPlayer();
    }
    player.editor.setValue(code, -1);
    player.editor.clearSelection();
    player.clearOutput();
    angular.element("#console").css("margin-left", "50%");
  }
  else {
    angular.element("#console").css("margin-left", "100%");
  }
  if (typeof player != "undefined") {
    player.editor.setReadOnly(!enable);
  }
}

function cleanArray(actual){
  var newArray = new Array();
  for(var i = 0; i<actual.length; i++){
      if (actual[i]){
        newArray.push(actual[i]);
    }
  }
  return newArray;
}

// TO BE IMPLEMENTED
function getTocFromMd(mdContent){
  var html = marked(mdContent);
}

angular.module('docsApp', [
  'ngRoute',
  'ngCookies',
  'ngSanitize',
  'ngAnimate',
  'DocsController',
  'versionsData',
  'pagesData',
  'itemTypes',
  'directives',
  'errors',
  'examples',
  'tutorials',
  'versions',
  'bootstrap',
  'ui.bootstrap.dropdown',
  'hc.marked'
]);

angular.module('directives', [])
.directive('markdown', function() {
    var md = function () {
        marked.setOptions({
            gfm:true,
            pedantic:false,
            sanitize:true
        });

        var toHtml = function (markdown) {
            if (markdown == undefined)
                return '';

            return marked(markdown);
        };
        return {
            toHtml:toHtml
        };
    }();
    return {
        restrict: 'E',
        link: function(scope, element, attrs) {
            scope.$watch(attrs.ngModel, function(value, oldValue) {
                var markdown = value;
                var html = md.toHtml(markdown);
                element.html(html);
                angular.forEach(element.find("code"), function (block) {
                    // use highlight.js to highlight code
                    hljs.highlightBlock(block);
                    // Add try button
                    block = block.parentNode;
                    var wrapper = document.createElement("div");
                    wrapper.className = "codewrapper";
                    wrapper.innerHTML = '<div class="trydiv"><span class="tryspan">Try this code</span></div>';
                    wrapper.childNodes[0].childNodes[0].onclick = function () {
                      tryCode(true, block.innerText);
                    };
                    block.parentNode.replaceChild(wrapper, block);
                    wrapper.appendChild(block);
                });
            });
        }
    };
})
.directive("declaration", function() {
    return {
        restrict: 'E',
        link: function(scope, element, attrs) {
            scope.$watch(attrs.ngModel, function(value, oldValue) {
                var language = attrs.ngLanguage;
                element.html('<code class="lang-' + 'language' +'"></code>');
                var code = element.children("code");
                code.text(value);
                hljs.highlightBlock(code[0]);
            });
        }
    };
})

/**
 * backToTop Directive
 * @param  {Function} $anchorScroll
 *
 * @description Ensure that the browser scrolls when the anchor is clicked
 */
.directive('backToTop', ['$anchorScroll', '$location', function($anchorScroll, $location) {
  return function link(scope, element) {
    element.on('click', function(event) {
      $location.hash('');
      scope.$apply($anchorScroll);
    });
  };
}])


.directive('code', function() {
  return {
    restrict: 'E',
    terminal: true,
    compile: function(element) {
      var linenums = element.hasClass('linenum');// || element.parent()[0].nodeName === 'PRE';
      var match = /lang-(\S+)/.exec(element[0].className);
      var lang = match && match[1];
      var html = element.html();
      element.html(window.prettyPrintOne(html, lang, linenums));
    }
  };
})

.directive('scrollYOffsetElement', ['$anchorScroll', function($anchorScroll) {
  return function(scope, element) {
    $anchorScroll.yOffset = element;
  };
}]);

angular.module('DocsController', [])
.service('docService', ['$q', '$http', docServiceFunction])
.factory('tocCache', ['$cacheFactory', function($cacheFactory) {
    return $cacheFactory('toc-cache');
  }])
.controller('DocsController', [
          '$scope', '$http', '$q','$rootScope', '$location', '$window', '$cookies', 'openPlunkr',
              'NG_PAGES', 'NG_VERSION', 'NG_ITEMTYPES', 'docService', 'tocCache',
  function($scope, $http, $q, $rootScope, $location, $window, $cookies, openPlunkr,
              NG_PAGES, NG_VERSION, NG_ITEMTYPES, docService, tocCache) {
  $scope.openPlunkr = openPlunkr;

  $scope.docsVersion = NG_VERSION.isSnapshot ? 'snapshot' : NG_VERSION.version;

  $scope.tocClass = docService.tocClassApi;

  $scope.navClass = docService.navClassApi;

  $scope.getNumber = function(num) {
      return new Array(num + 1);
  }

  $scope.GetDetail = function(e){
    console.log(e.target);
    var display = e.target.nextElementSibling.style.display;
    e.target.nextElementSibling.style.display = (display == 'block')? 'none':'block';
  };

  $scope.ViewSource = function(){
    return docService.getRemoteUrl(this.model.source, this.model.source.startLine + 1);
  };

  $scope.ImproveThisDoc = function(){
    return $scope.partialModel.mdContent;
  };

  $scope.GetTocHref = function(relativeUrl){
    if (!$scope.toc) return '#' + relativeUrl;
    var tocPath = $scope.toc.path;
    if (tocPath){
      return '#' + tocPath + '!' + relativeUrl;
    }

    return '#' + relativeUrl;
  };

  $scope.$on('$includeContentLoaded', function() {
    var pagePath = $scope.currentPage ? $scope.currentPage.path : $location.path();
  });

  (function getNavbar(){
    // load navigation bar, should be toc.yaml in root path
    // TODO: support toc.md => extract <h><a> from marked(toc.md)
    var navBarPath = "toc.yaml";
    docService.asyncFetchIndex(navBarPath, function(result){
      $scope.navbar = jsyaml.load(result);

      // Load first item as the default page
      docService.getDefaultItem($scope.navbar,
        function(defaultItem) {
         if (defaultItem.href) $location.url(defaultItem.href);
        });
    });
  })();

  // TODO: move path to app.config?
  (function getMdIndex(){
    docService.asyncFetchIndex('md.yaml', function(result){
      $scope.mdIndex = jsyaml.load(result);
    });
  })();

// #a/b/c!d/e/f => a/b/c/toc.yaml as toc, d/e/f as content
$scope.$watch(function docsPathWatch() {return $location.path(); }, function docsPathWatchAction(path) {
    path = path.replace(/^\/?(.+?)(\/index)?\/?$/, '$1');

    var currentPage = $scope.currentPage = path;//NG_PAGES[path];

    // TODO: check if it is inside NG_PAGES
    // If current page exists in NG_PAGES
    if (currentPage ) {
      var pathInfo = docService.getPathInfo(currentPage);
      if (pathInfo.tocPath){
        var temp = tocCache.get(pathInfo.tocPath);
        if (temp){
          if (temp.content) $scope.toc = temp;
        }else{
          docService.asyncFetchIndex(pathInfo.tocFilePath, function(result){
                var content = jsyaml.load(result);
                var toc = {path: pathInfo.tocPath, content: content};
                tocCache.put(pathInfo.tocPath, toc);
                $scope.toc = toc;
                // If the requested content path is not specified, pick the first item in toc.yaml as the default one
                if (!pathInfo.contentPath && toc.content.count > 0) {
                  $location.url(toc.path + '!' + toc.content[0].href)
                }
              }, function(){
                tocCache.put(pathInfo.tocPath, {path: pathInfo.tocPath});
              });
        }
      }else{
        // hide toc
        $scope.toc = undefined;
      }

      path = pathInfo.contentPath;
      if (path){
        if ($scope.toc) path = $scope.toc.path + '/' + path;
        // If end with .md
        if ((/\.md$/g).test(path)){
          $scope.contentType = 'md';
          $scope.partialModel = {};
          $scope.title = path;
          $scope.partialPath = path;
        }else if ((/\.yaml$/g).test(path)){
          $scope.contentType = 'yaml';
          // if is yaml
          var promise = docService.asyncFetchIndex(path, function(result){
              $scope.partialModel = jsyaml.load(result);
              $scope.title = $scope.partialModel.id;
            if ($scope.partialModel.type.toLowerCase() == 'namespace'){
              $scope.itemtypes = NG_ITEMTYPES.namespace;
              for(var i in $scope.partialModel.items){
                var itemtype = $scope.itemtypes[$scope.partialModel.items[i].type];
                if (itemtype){
                  itemtype.show = true;
                }
              }
              $scope.partialPath = 'template' + '/namespace.tmpl';
            }
            else {
              $scope.itemtypes = NG_ITEMTYPES.class;
              for(var i in $scope.itemtypes){
                $scope.itemtypes[i].show = false;
              }
              for(var i in $scope.partialModel.items){
                var itemtype = $scope.itemtypes[$scope.partialModel.items[i].type];
                if (itemtype){
                  itemtype.show = true;
                }
              }
              $scope.partialPath = 'template' + '/class.tmpl';
            }
          },
          function(){
            $scope.breadcrumb = [];
            $scope.partialPath = 'template/error404.tmpl';
          }
          );
        }else{
          // If not md or yaml, simply try load the path
          $scope.partialPath = path;
        }
      }else{
        if ($scope.navbar && $scope.navbar.count > 0){
          $location.url($scope.navbar[0].href);
        }else{
          $scope.breadcrumb = [];
          $scope.partialPath = 'template/error404.tmpl';
        }
      }

      var pathParts = path.split('/');
      var breadcrumb = $scope.breadcrumb = [];
      var breadcrumbPath = '';
      angular.forEach(pathParts, function(part) {
        breadcrumbPath += part;
        breadcrumb.push({ name: part, url: breadcrumbPath });
        breadcrumbPath += '/';
      });
    }else{
      if ($scope.navbar && $scope.navbar.length > 0){
        $location.url($scope.navbar[0].href);
      }
    }
  });

  $scope.$watch(function modelWatch() {return $scope.partialModel; }, function modelWatchAction(path) {
      if ($scope.mdIndex && $scope.partialModel){
        var mdPath = $scope.mdIndex[$scope.partialModel.id];
        if (mdPath){
          if (mdPath.href){
            $scope.partialModel.mdHref = docService.getRemoteUrl(mdPath);
            var getMdIndex = docService.asyncFetchIndex(mdPath.href,
              function(result){
                var md = result.substr(mdPath.startLine, mdPath.endLine - mdPath.startLine + 1);
                $scope.partialModel.mdContent = md;
              });
          }
        }
      }
  });

  // listen for toc change
  $scope.$watch(function modelWatch() {return $scope.toc; }, function modelWatchAction(toc) {
    if (toc && toc.content){
      docService.getDefaultItem(toc.content,
        function(defaultItem){
          if (defaultItem && defaultItem.href){
             $location.url(toc.path + '!' + defaultItem.href);
          }
        });
    }
  });

  // listen for toc change
  $scope.$watch(function modelWatch() {return $scope.navbar; }, function modelWatchAction(navbar) {
    if (!$location.path() && navbar && navbar.count > 0){
      $location.url(navbar[0].href);
    }
  });
  /**********************************
   Initialize
   ***********************************/

  $scope.versionNumber = angular.version.full;
  $scope.version = angular.version.full + "  " + angular.version.codeName;
  $scope.loading = 0;


 /* var INDEX_PATH = /^(\/|\/index[^\.]*.html)$/;
  if (!$location.path() || INDEX_PATH.test($location.path())) {
    $location.path('/api').replace();
  }*/

}]);

angular.module('errors', ['ngSanitize'])

.filter('errorLink', ['$sanitize', function ($sanitize) {
  var LINKY_URL_REGEXP = /((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}<>]/g,
      MAILTO_REGEXP = /^mailto:/,
      STACK_TRACE_REGEXP = /:\d+:\d+$/;

  var truncate = function (text, nchars) {
    if (text.length > nchars) {
      return text.substr(0, nchars - 3) + '...';
    }
    return text;
  };

  return function (text, target) {
    var targetHtml = target ? ' target="' + target + '"' : '';

    if (!text) return text;

    return $sanitize(text.replace(LINKY_URL_REGEXP, function (url) {
      if (STACK_TRACE_REGEXP.test(url)) {
        return url;
      }

      // if we did not match ftp/http/mailto then assume mailto
      if (!/^((ftp|https?):\/\/|mailto:)/.test(url)) url = 'mailto:' + url;

      return '<a' + targetHtml + ' href="' + url +'">' +
                truncate(url.replace(MAILTO_REGEXP, ''), 60) +
              '</a>';
    }));
  };
}])


.directive('errorDisplay', ['$location', 'errorLinkFilter', function ($location, errorLinkFilter) {
  var interpolate = function (formatString) {
    var formatArgs = arguments;
    return formatString.replace(/\{\d+\}/g, function (match) {
      // Drop the braces and use the unary plus to convert to an integer.
      // The index will be off by one because of the formatString.
      var index = +match.slice(1, -1);
      if (index + 1 >= formatArgs.length) {
        return match;
      }
      return formatArgs[index+1];
    });
  };

  return {
    link: function (scope, element, attrs) {
      var search = $location.search(),
        formatArgs = [attrs.errorDisplay],
        i;

      for (i = 0; angular.isDefined(search['p'+i]); i++) {
        formatArgs.push(search['p'+i]);
      }
      element.html(errorLinkFilter(interpolate.apply(null, formatArgs), '_blank'));
    }
  };
}]);

angular.module('examples', [])

.factory('formPostData', ['$document', function($document) {
  return function(url, newWindow, fields) {
    /**
     * If the form posts to target="_blank", pop-up blockers can cause it not to work.
     * If a user choses to bypass pop-up blocker one time and click the link, they will arrive at
     * a new default plnkr, not a plnkr with the desired template.  Given this undesired behavior,
     * some may still want to open the plnk in a new window by opting-in via ctrl+click.  The
     * newWindow param allows for this possibility.
     */
    var target = newWindow ? '_blank' : '_self';
    var form = angular.element('<form style="display: none;" method="post" action="' + url + '" target="' + target + '"></form>');
    angular.forEach(fields, function(value, name) {
      var input = angular.element('<input type="hidden" name="' +  name + '">');
      input.attr('value', value);
      form.append(input);
    });
    $document.find('body').append(form);
    form[0].submit();
    form.remove();
  };
}])


.factory('openPlunkr', ['formPostData', '$http', '$q', function(formPostData, $http, $q) {
  return function(exampleFolder, clickEvent) {

    var exampleName = 'AngularJS Example';
    var newWindow = clickEvent.ctrlKey || clickEvent.metaKey;

    // Load the manifest for the example
    $http.get(exampleFolder + '/manifest.json')
      .then(function(response) {
        return response.data;
      })
      .then(function(manifest) {
        var filePromises = [];

        // Build a pretty title for the Plunkr
        var exampleNameParts = manifest.name.split('-');
        exampleNameParts.unshift('AngularJS');
        angular.forEach(exampleNameParts, function(part, index) {
          exampleNameParts[index] = part.charAt(0).toUpperCase() + part.substr(1);
        });
        exampleName = exampleNameParts.join(' - ');

        angular.forEach(manifest.files, function(filename) {
          filePromises.push($http.get(exampleFolder + '/' + filename, { transformResponse: [] })
            .then(function(response) {

              // The manifests provide the production index file but Plunkr wants
              // a straight index.html
              if (filename === "index-production.html") {
                filename = "index.html"
              }

              return {
                name: filename,
                content: response.data
              };
            }));
        });
        return $q.all(filePromises);
      })
      .then(function(files) {
        var postData = {};

        angular.forEach(files, function(file) {
          postData['files[' + file.name + ']'] = file.content;
        });

        postData['tags[0]'] = "angularjs";
        postData['tags[1]'] = "example";
        postData.private = true;
        postData.description = exampleName;

        formPostData('http://plnkr.co/edit/?p=preview', newWindow, postData);
      });
  };
}]);

angular.module('tutorials', [])

.directive('docTutorialNav', function() {
  var pages = [
    '',
    'step_00', 'step_01', 'step_02', 'step_03', 'step_04',
    'step_05', 'step_06', 'step_07', 'step_08', 'step_09',
    'step_10', 'step_11', 'step_12', 'the_end'
  ];
  return {
    scope: {},
    template:
      '<a ng-href="tutorial/{{prev}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-step-backward"></i> Previous</li></a>\n' +
      '<a ng-href="http://angular.github.io/angular-phonecat/step-{{seq}}/app"><li class="btn btn-primary"><i class="glyphicon glyphicon-play"></i> Live Demo</li></a>\n' +
      '<a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{diffLo}}...step-{{diffHi}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-search"></i> Code Diff</li></a>\n' +
      '<a ng-href="tutorial/{{next}}"><li class="btn btn-primary">Next <i class="glyphicon glyphicon-step-forward"></i></li></a>',
    link: function(scope, element, attrs) {
      var seq = 1 * attrs.docTutorialNav;
      scope.seq = seq;
      scope.prev = pages[seq];
      scope.next = pages[2 + seq];
      scope.diffLo = seq ? (seq - 1): '0~1';
      scope.diffHi = seq;

      element.addClass('btn-group');
      element.addClass('tutorial-nav');
    }
  };
})


.directive('docTutorialReset', function() {
  return {
    scope: {
      'step': '@docTutorialReset'
    },
    template:
      '<p><a href="" ng-click="show=!show;$event.stopPropagation()">Workspace Reset Instructions  ➤</a></p>\n' +
      '<div class="alert alert-info" ng-show="show">\n' +
      '  <p>Reset the workspace to step {{step}}.</p>' +
      '  <p><pre>git checkout -f step-{{step}}</pre></p>\n' +
      '  <p>Refresh your browser or check out this step online: '+
          '<a href="http://angular.github.io/angular-phonecat/step-{{step}}/app">Step {{step}} Live Demo</a>.</p>\n' +
      '</div>\n' +
      '<p>The most important changes are listed below. You can see the full diff on ' +
        '<a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{step ? (step - 1): \'0~1\'}}...step-{{step}}">GitHub</a>\n' +
      '</p>'
  };
});
"use strict";

angular.module('versions', [])

.controller('DocsVersionsCtrl', ['$scope', '$location', '$window', 'NG_VERSIONS', function($scope, $location, $window, NG_VERSIONS) {
  $scope.docs_version  = NG_VERSIONS[0];
  $scope.docs_versions = NG_VERSIONS;

  for(var i=0, minor = NaN; i < NG_VERSIONS.length; i++) {
    var version = NG_VERSIONS[i];
    // NaN will give false here
    if (minor <= version.minor) {
      continue;
    }
    version.isLatest = true;
    minor = version.minor;
  }

  $scope.getGroupName = function(v) {
    return v.isLatest ? 'Latest' : ('v' + v.major + '.' + v.minor + '.x');
  };

  $scope.jumpToDocsVersion = function(version) {
    var currentPagePath = $location.path().replace(/\/$/, '');

    // TODO: We need to do some munging of the path for different versions of the API...


    $window.location = version.docsUrl + currentPagePath;
  };
}]);
