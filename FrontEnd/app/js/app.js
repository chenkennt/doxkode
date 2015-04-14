/* 
 * https://github.com/johnpapa/angular-styleguide
 * define 1 component per file
*/
angular.module('docsApp', [
  'ngRoute',
  'ngCookies',
  'ngSanitize',
  'docascode.controller',
  'versionsData',
  'pagesData',
  'itemTypes',
  'docascode.directives',
  'errors',
  'versions',
  'bootstrap',
  'ui.bootstrap',
  'ui.bootstrap.dropdown',
]);