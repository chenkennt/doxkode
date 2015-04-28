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

    // StartLine starts @1 to be consistent with IDE
    this.substringLine = function(input, startline, endline){
      if (!input || endline < 1 || startline > endline) return '';
      var lines = input.split('\n');
      var maxLine = lines.length;
      startline = startline <= 1 ? 1 : startline;
      endline = endline >= maxLine ? maxLine: endline;
      var snippet = '';
      for(var i = startline -1; i < endline; i++){
        snippet += lines[i] + '\n';
      }
      return snippet;
    };
  }

  angular.module('docascode.util', [])
    .service('docUtility', utilityProvider);
})();