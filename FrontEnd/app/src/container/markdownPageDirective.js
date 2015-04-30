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
    .directive('markdownPage', ['contentService', 'markdownService', markdownPage]);

  function markdownPage(contentService, markdownService) {
    var template = 
    '<div>' +
      '<a ng-if="model.href" ng-href="{{model.href}}" class="btn pull-right mobile-hide">' +
      '<!--<span class="glyphicon glyphicon-edit">&nbsp;</span>-->Improve this Doc' +
      '</a>' +
      '<markdown></markdown>' +
     '</div>';
    function render(element, markdownFilePath, loadMapFile) {
      if (!markdownFilePath) return;
      var mapFilePath = markdownFilePath + ".map";
      console.log("Start loading map file" + mapFilePath);
      element.html(template);
      var wrapper = document.createElement("div");
      contentService.getMarkdownContent(markdownFilePath)
        .then(
        function (result) {
          var html = markdownService.transform(result);
          wrapper.innerHTML = html;
          angular.forEach(element.find("markdown"), function (block) {
            block.parentNode.replaceChild(wrapper, block);
          });
        },
        function (result) { }
        );

      if (loadMapFile) {
        contentService.getMdContent(mapFilePath)
          .then(
          function (result) {

          },
          function (result) { 
            
          }
          );
      }
    }
    return {
      restrict: 'E',
      replace: true,
      priority: 200,
      link: function (scope, element, attrs) {
        if (attrs.pageUrl){
          scope.$watch(attrs.pageUrl, function(value, oldValue){
            if (value === undefined) return;
            render(element, value, scope.getMap);
          });
        }
      }
    };
  }
})();