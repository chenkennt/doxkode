angular.module('tocCache', ['angular-cache'])
    .factory('tocCache', ['$cacheFactory', function($cacheFactory) {
        return $cacheFactory('toc-cache');
    }]);