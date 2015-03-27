angular.module('directives', [])
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
}]);