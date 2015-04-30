/*
 * Define content load related functions used in docsController
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */

(function () {
  'use strict';
  /*jshint validthis:true */
  function provider($q, $http, $location, urlService, csplayService) {
    this.transform = transform;

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
    } ());

    function transform(markdown) {
      var html = md.toHtml(markdown);
      var element = angular.element(document.createElement("div"));
      element.html(html);
      angular.forEach(element.find("code"), function (block) {
        // use highlight.js to highlight code
        hljs.highlightBlock(block);
        // Add try button
        block = block.parentNode;
        if ($(block).is("pre")) { //make sure the parent is pre and not inline code
          var wrapper = document.createElement("div");
          wrapper.className = "codewrapper";
          wrapper.innerHTML = '<div class="trydiv"><span class="tryspan">Try this code</span></div>';
          wrapper.childNodes[0].childNodes[0].onclick = function () {
            csplayService.tryCode(true, block.innerText);
          };
          block.parentNode.replaceChild(wrapper, block);
          wrapper.appendChild(block);
        }
      });
      angular.forEach(element.find("a"), function (block) {
        var url = block.attributes['href'] && block.attributes['href'].value;
        if (!url) return;
        if (!urlService.isAbsoluteUrl(url))
          block.attributes['href'].value = urlService.getPageHref($location.path(), url);
      });
      angular.forEach(element.find("img"), function (block) {
        var url = block.attributes['src'] && block.attributes['src'].value;
        if (!url) return;
        if (!urlService.isAbsoluteUrl(url))
          block.attributes['src'].value = urlService.getAbsolutePath($location.path(), url);
      });
      angular.forEach(element.find("table"), function (block) {
        $(block).addClass('table table-bordered table-striped table-condensed');
      });
      return html;
    }
  }

  angular.module('docascode.markdownService', ['docascode.urlService', 'docascode.csplayService'])
    .service('markdownService', ['$q', '$http', '$location', 'urlService', 'csplayService', provider]);

})();