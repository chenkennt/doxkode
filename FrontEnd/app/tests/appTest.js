describe("Test for DocsController", function(){
  var mockScope, controller, backend;

  beforeEach(angular.mock.module("docsApp"));
  beforeEach(angular.mock.inject(function($httpBackend){
    backend = $httpBackend;
    backend.expect("GET", "toc.yml").respond("-id: 'a'");
  }));

  beforeEach(angular.mock.inject(function($controller, $rootScope, $http){
    mockScope = $rootScope.$new();
    mockScope.toc = [
      {name: 'Home', href: 'index.md' },
      {name: 'About', href: 'about.md' },
    ];

    controller = $controller("DocsController", {
      $scope: mockScope,
      $http: $http
    });
    backend.flush();
  }))

  it("Test getTocHref", function(){
    expect(mockScope.getTocHref('a.yml')).toEqual('#toc.yml');
  })

  it("Makes an Ajax request", function(){
    backend.verifyNoOutstandingExpectation();
  })

});