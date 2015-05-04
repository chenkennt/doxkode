/*
 * Define directives used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function() {
  'use strict';

  angular.module('docascode.directives', ['itemTypes', 'docascode.urlService', 'docascode.markdownService', 'docascode.csplayService','docascode.contentService'])
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
    .directive('scrollYOffsetElement', ['$anchorScroll', function($anchorScroll) {
      return function(scope, element) {
        $anchorScroll.yOffset = element;
      };
    }])
    .directive('code', function() {
      return {
        restrict: 'E',
        require: 'ngModel',
        scope: {
          bindonce: "@",
        },
        terminal: true,
        link: function(scope, element, attrs, ngModel) {
          var unwatch = scope.$watch(function () { return ngModel.$modelValue; }, function(value, oldValue) {
            if (value === undefined) return;
            var language;
            var content;
            if (value.CSharp) {
              language = "csharp";
              content = value.CSharp;
            }else if (value.VB) {
              language = "vb";
              content = value.VB;
            }

            element.text(content);
            angular.forEach(element, function(block) {
              hljs.highlightBlock(block, language);
            });
            if (scope.bindonce){
              unwatch();
            }
          });
        }
      };
    });
})();