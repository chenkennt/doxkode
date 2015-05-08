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
      markdownService.transform(element, content);
    }
    
    function loadSrc(element, value) {
      if (!value) return;
      element.html('');

      contentService.getMarkdownContent(value)
        .then(
        function (result) {
          render(element, result);
        },
        function (result) {
          element.html(result.data);
        }
        );
    }
    
    return {
      restrict: 'AE',
      link: function (scope, element, attrs) {
        if (attrs.src) {
          // loadSrc(scope, element, attrs.src);
          scope.$watch(attrs.src, function (value, oldValue) {
            loadSrc(element, value);
          });
        }
        
        if (attrs.data) {
          // render(element, attrs.data);
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
      '<div ng-class="{\'col-sm-9\':affixModel, \'col-md-10\':affixModel}">' +
      '<a ng-if="href" ng-href="{{href}}" class="btn btn-primary pull-right mobile-hide">' +
      '<!--<span class="glyphicon glyphicon-edit">&nbsp;</span>-->Improve this Doc' +
      '</a>' +
      '<article></article>' +
      '</div>' +
      '<div class="hidden-xs col-sm-3 col-md-2" ng-if="affixModel">' +
      '<affix-bar title="In this article" id="docs-subnavbar" data="affixModel">' +
      '</affix-bar>' +
      '</div>' +
      '</div>'
      ;
      
    function loadSrc(scope, element, value) {
      if (!value) return;
      element.html('');

      contentService.getMarkdownContent(value)
        .then(
        function (result) {
          markdownService.transform(element, result);
          // Build Affix Bar:
          // Ignore h1, h2 as top level, h3 as leaf level
          var affixModel = [];
          var currentH2Item;
          angular.forEach(element.find('h2,h3'), function(header){
            var ele = angular.element(header);
            if (ele.is('h2')){
              // If is top level
              currentH2Item = {htmlId: header.id, title:header.innerText, items:[]};
              affixModel.push(currentH2Item);
            } else {
              // If is leaf level
              if (!currentH2Item){
                // If does not have h2 in previous section, use a default one
                currentH2Item = {title:"Summary", items:[]};
                affixModel.push(currentH2Item);
              }
              var h3Item = {htmlId: header.id, title: header.innerText};
              currentH2Item.items.push(h3Item);
            }
          });
          angular.forEach(affixModel, function(item){
            if (item.items.length === 0) item.items = null;
          });          
          if (affixModel.length === 0) affixModel = null;
          scope.affixModel = affixModel;
        },
        function (result) {
          element.html(result.data);
        }
        );
    }
    
    function render(scope, element, markdownFilePath, loadMapFile) {
      if (!markdownFilePath) return;
      var article = angular.element(element.find('article')[0]);
      loadSrc(scope, article, markdownFilePath);
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
                scope.href = urlService.getRemoteUrl(value.remote, value.startLine);
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
      require: 'ngModel',
      scope:{
        getMap: "=",
      },
      link: function (scope, element, attrs, ngModel) {
        var localScope = scope;
        // render(localScope, element, attrs.src, getMap);
        scope.$watch(function () { return ngModel.$modelValue; }, function (value, oldValue) {
          if (value === undefined) return;
          render(localScope, element, value, localScope.getMap);
        });
      }
    };
  }
})();