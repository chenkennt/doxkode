/*
 * Define general utility functions used in docsApp
 * Wrap Angular components in an Immediately Invoked Function Expression (IIFE)
 * to avoid variable collisions
 */
(function() {
  'use strict';
  /*jshint validthis:true */
  function utilityProvider() {
    this.cleanArray = function(actual) {
      var newArray = [];
      for (var i = 0; i < actual.length; i++) {
        if (actual[i]) {
          newArray.push(actual[i]);
        }
      }
      return newArray;
    };
  }

  angular.module('docascode.util', [])
    .service('docUtility', utilityProvider);
})();