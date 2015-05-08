/* global angular */
/*
 * Define directives used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function () {
  'use strict';

  angular.module('docascode.directives', ['itemTypes', 'docascode.urlService', 'docascode.util', 'docascode.markdownService', 'docascode.csplayService', 'docascode.contentService'])
  /**
   * backToTop Directive
   * @param  {Function} $anchorScroll
   *
   * @description Ensure that the browser scrolls when the anchor is clicked
   */
    .directive('backToTop', ['$anchorScroll', '$location', function ($anchorScroll, $location) {
    return function link(scope, element) {
      element.on('click', function (event) {
        $location.hash('');
        scope.$apply($anchorScroll);
      });
    };
  }])
    .directive('scrollYOffsetElement', ['$anchorScroll', function ($anchorScroll) {
    return function (scope, element) {
      $anchorScroll.yOffset = element;
    };
  }])
    .directive('affixBar', function () {
    var template =
      '<nav class="affix">' +
      '<h3>{{title}}</h3>' +
      '<ul class="nav">' +
      '<li scroll-link="{{child.htmlId}}" ng-attr-last="{{$last}}" ng-repeat="child in model">' +
      '<a>{{child.title}}</a>' +
      '<ul class="nav">' +
      '<li scroll-link="{{item.htmlId}}" ng-attr-last="{{$last}}" ng-repeat="item in child.items">' +
      '<a>{{item.title}}</a>' +
      '</li>' +
      '</ul>' +
      '</li>' +
      '</ul>' +
      '</nav>';

    return {
      restrict: 'E',
      replace: true,
      template: template,
      priority: 10, 
      scope:{
        title:"@",
        model:"=data"
      },
    };
  })
    .directive('scrollLink', ['$timeout', '$window', '$location', '$document', '$anchorScroll', function ($timeout, $window, $location, $document, $anchorScroll) {
    // get offset from anchorScroll's yOffset
    function getYOffset() {
      var offset = $anchorScroll.yOffset;
      if (angular.isFunction(offset)) {
        offset = offset();
      } else if (angular.isElement(offset)) {
        var elem = offset[0];
        var style = $window.getComputedStyle(elem);
        if (style.position !== 'fixed') {
          offset = 0;
        } else {
          offset = elem.getBoundingClientRect().bottom;
        }
      } else if (!angular.isNumber(offset)) {
        offset = 0;
      }

      return offset;
    }

    function getActiveScroll(id, isLast) {
      var element = angular.element("#" + id);
      if (element && element.length > 0) {
        if (isLast && ($window.scrollY + $window.innerHeight === $document.height())) {
          return true;
        }

        var top = element.offset().top;
        var bottom = top + element.height();
        var yOffset = getYOffset();
        var pageYOffset = $window.pageYOffset + yOffset;

        if (top <= pageYOffset && bottom > pageYOffset)
          return true;
      }

      return false;
    }

    function setActive(liElement) {
      if (!liElement) return;
      var ulElement = liElement.parent();
      // for all the descendant and sibling <li>'s remove active
      angular.forEach(ulElement.children("li"), function (block) {
        if (block === liElement[0]) {
          angular.element(block).addClass("active");
        }
        else {
          var ele = angular.element(block);
          ele.removeClass("active");
          angular.forEach(ele.find("li"), function (block) {
            angular.element(block).removeClass("active");
          });
        }
      });
    }

    function scrollTo(element, id) {
      if ($location.hash() !== id) {
        // trick provided by http://stackoverflow.com/questions/11784656/angularjs-location-not-changing-the-path
        $timeout(function () {
          $location.hash(id);
        }, 1);
      } else {
        $anchorScroll();
      }
      setActive(element);
    }

    return {
      restrict: 'A',
      link: function (scope, element, attrs, ngModel) {
        var id = attrs.scrollLink;
        var last = attrs.last === "true";
        var active = getActiveScroll(id, last);
        if (active) {
          setActive(element);
        }
          
        // get <a> children if exists
        // children() travels a single level down the DOM tree
        // while find() search through the descendants of the DOM
        var linkElement = element.children('a')[0];
        if (linkElement) {
          linkElement.onclick = function () {
            scrollTo(element, id);
          };
        }

        angular.element($window).bind('scroll', function () {
          var active = getActiveScroll(id, last);
          if (active) {
            setActive(element);
          }
        });
      }
    };
  }])
    .directive('code', function () {
    return {
      restrict: 'E',
      require: 'ngModel',
      scope: {
        bindonce: "@",
      },
      terminal: true,
      link: function (scope, element, attrs, ngModel) {
        var unwatch = scope.$watch(function () { return ngModel.$modelValue; }, function (value, oldValue) {
          if (value === undefined) return;
          var language;
          var content;
          if (value.CSharp) {
            language = "csharp";
            content = value.CSharp;
          } else if (value.VB) {
            language = "vb";
            content = value.VB;
          }

          element.text(content);
          angular.forEach(element, function (block) {
            hljs.highlightBlock(block, language);
          });
          if (scope.bindonce) {
            unwatch();
          }
        });
      }
    };
  });
})();