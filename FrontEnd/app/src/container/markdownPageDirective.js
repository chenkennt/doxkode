/**
 * @ngdoc directive
 * @name docascode.directive:markdownPage
 * @restrict E
 * @element ANY
 * @priority 1000
 * @scope
 * 
 * @description markdownPage directive to help render markdown pages
 **/

(function () {
  'use strict';

  angular.module('docascode.directives')
  /**
   * markdownPage Directive
   * @param  {Function} $anchorScroll
   *
   * @description Ensure that the browser scrolls when the anchor is clicked
   */
    .directive('markdownPage', ['contentService', 'markdownService', 'urlService', markdownPage]);

  function markdownPage(contentService, markdownService, urlService) {
    var template = 
    '<div>' +
      '<a ng-if="markdownPageModel.href" ng-href="{{markdownPageModel.href}}" class="btn pull-right mobile-hide">' +
      '<!--<span class="glyphicon glyphicon-edit">&nbsp;</span>-->Improve this Doc' +
      '</a>' +
      '<markdown></markdown>' +
     '</div>';
    function render(scope, element, markdownFilePath, loadMapFile) {
      if (!markdownFilePath) return;
      var mapFilePath = markdownFilePath + ".map";
      console.log("Start loading map file" + mapFilePath);
      angular.forEach(element.find("markdown"), function (block){
        block.innerHTML = '';
      });
      contentService.getMarkdownContent(markdownFilePath)
        .then(
        function (result) {
          var html = markdownService.transform(result);
          angular.forEach(element.find("markdown"), function (block) {
            block.innerHTML = html;
          });
        },
        function (result) { }
        );

      if (loadMapFile) {
        contentService.getMdContent(mapFilePath)
          .then(
          function (result) {
            var data = result.data;
            for (var key in data) {
              if (data.hasOwnProperty(key)) {
                var value = data[key];
                if (value.remote && value.remote.repo){
                  if (!scope.markdownPageModel) scope.markdownPageModel = {};
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
      link: function (scope, element, attrs) {
        if (attrs.src){
          
          var getMap = attrs.getMap === "true";
          var localScope = scope;
          scope.$watch(attrs.src, function(value, oldValue){
            if (value === undefined) return;
            render(localScope, element, value, getMap);
          });
        }
      }
    };
  }
})();