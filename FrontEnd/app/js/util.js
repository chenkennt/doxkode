function utilityProvider() {
  this.cleanArray = function(actual) {
    'use strict';
    var newArray = [];
    for (var i = 0; i < actual.length; i++) {
      if (actual[i]) {
        newArray.push(actual[i]);
      }
    }
    return newArray;
  };
}

angular.module('docUtility', [])
  .service('docUtility', utilityProvider);