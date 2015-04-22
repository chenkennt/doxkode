/* 
 * https://github.com/johnpapa/angular-styleguide
 * define 1 component per file
*/
angular.module('docsApp', [
  'ngRoute',
  'ngSanitize',
  'itemTypes',

  'bootstrap',
  'ui.bootstrap',
  'ui.bootstrap.dropdown',
  
  'docascode.controller',
  'docascode.directives',
]);