/**
 * @ngdoc directive
 * @name docascode.directive:markdownContent
 * @restrict E
 * @element ANY
 * @priority 1000
 * @scope
 * 
 * @description markdownContent directive to help render markdown pages
 **/

(function () {
  'use strict';

  angular.module('docascode.directives')
  /**
   * markdownContent Directive
   * @param  {Function} contentService
   * @param  {Function} markdownService
   * @param  {Function} urlService
   *
   * @description Render a page with .md file, supporting try...code
   */
    .directive('markdown', ['contentService', 'markdownService', 'urlService', markdown])
    .directive('markdownContent', ['contentService', 'markdownService', 'urlService', markdownContent]);
  function markdown(contentService, markdownService, urlService){
    function render(element, content) {
      var html = markdownService.transform(content);
      element.html(html);
    }
    return {
      restrict: 'AE',
      link: function (scope, element, attrs) {
        if (attrs.src) {
          scope.$watch(attrs.src, function (value, oldValue) {
            if (!value) return;
            element.html('');
            
            contentService.getMarkdownContent(value)
              .then(
              function (result) {
                render(element, result);
              },
              function (result) { }
              );
          });
        }
        if (attrs.data) {
          scope.$watch(attrs.data, function (value, oldValue) {
            if (value === undefined) return;
            render(element, value);
          });
        }
      }
    };
  }
  
  function markdownContent(contentService, markdownService, urlService) {
    var template =
      '<div>' +
      '<a ng-if="markdownPageModel.href" ng-href="{{markdownPageModel.href}}" class="btn pull-right mobile-hide">' +
      '<!--<span class="glyphicon glyphicon-edit">&nbsp;</span>-->Improve this Doc' +
      '</a>' +
      '<markdown src="markdownPageModel.src"></markdown>' +
      '</div>';
    function render(scope, element, markdownFilePath, loadMapFile) {
      if (!markdownFilePath) return;
      
      scope.markdownPageModel = {
        src: markdownFilePath
      };
      var mapFilePath = markdownFilePath + ".map";
      // console.log("Start loading map file" + mapFilePath);
      if (loadMapFile) {
        contentService.getMdContent(mapFilePath)
          .then(
          function (result) {
            if (!result) return;
            var data = result.data;
            // TODO: change md.map's key to "default" to make it much easier
            for (var key in data) {
              if (data.hasOwnProperty(key)) {
                var value = data[key];
                if (value.remote && value.remote.repo) {
                  scope.markdownPageModel.href = urlService.getRemoteUrl(value, value.startLine);
                }
              }
            }
          },
          function (result) {

          }
          );
      }
    }
    return {
      restrict: 'E',
      replace: true,
      template: template,
      priority: 100,
      link: function (scope, element, attrs) {
        if (attrs.src) {

          var getMap = attrs.getMap === "true";
          var localScope = scope;
          scope.$watch(attrs.src, function (value, oldValue) {
            if (value === undefined) return;
            render(localScope, element, value, getMap);
          });
        }
      }
    };
  }
})();