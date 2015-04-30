/*
 * Define directives used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function() {
  'use strict';

  angular.module('docascode.directives', ['docascode.urlService', 'docascode.csplayService'])
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
    .directive('markdown', ['csplayService', 'urlService', '$location', function(csplayService, urlService, $location) {
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
      return {
        restrict: 'AE',
        link: function(scope, element, attrs) {
          function set(val) {
            var html = md.toHtml(val);
            element.html(html);
            angular.forEach(element.find("code"), function(block) {
              // use highlight.js to highlight code
              hljs.highlightBlock(block);
              // Add try button
              block = block.parentNode;
              if ($(block).is("pre")) { //make sure the parent is pre and not inline code
                var wrapper = document.createElement("div");
                wrapper.className = "codewrapper";
                wrapper.innerHTML = '<div class="trydiv"><span class="tryspan">Try this code</span></div>';
                wrapper.childNodes[0].childNodes[0].onclick = function() {
                  csplayService.tryCode(true, block.innerText);
                };
                block.parentNode.replaceChild(wrapper, block);
                wrapper.appendChild(block);
              }
            });
            angular.forEach(element.find("a"), function(block) {
              var url = block.attributes['href'] && block.attributes['href'].value;
              if (!url) return;
              if (!urlService.isAbsoluteUrl(url))
                block.attributes['href'].value = urlService.getHref(scope, $location.path(), url);
            });
            angular.forEach(element.find("img"), function(block) {
              var url = block.attributes['src'] && block.attributes['src'].value;
              if (!url) return;
              if (!urlService.isAbsoluteUrl(url))
                block.attributes['src'].value = urlService.getAbsolutePath($location.path(), url);
            });
            angular.forEach(element.find("table"), function(block) {
              $(block).addClass('table table-bordered table-striped table-condensed');
            });
          }

          set(element.text() || '');
          if (attrs.ngModel) {
            var unwatch = scope.$watch(attrs.ngModel, function(value, oldValue) {
              if (value === undefined) return;
              set(value);
              unwatch();
            });
          }
        }
      };
    }])
    .directive('code', function() {
      return {
        restrict: 'E',
        terminal: true,
        link: function(scope, element, attrs) {
          var unwatch = scope.$watch(attrs.ngModel, function(value, oldValue) {
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
            unwatch();
          });
        }
      };
    });
})();