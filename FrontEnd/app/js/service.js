function docServiceFunction($q, $http) {
  this.tocClassApi =  function(navItem) {
    return {
      active: navItem.href && this.currentPage,
      current: this.currentPage === navItem.href,
      'nav-index-section': navItem.type === 'section'
    };
  };

  this.navClassApi = function(navItem) {
    return {
      active: navItem.href && this.currentPage,
      current: this.currentPage.indexOf(navItem.href) > -1,
    };
  };

  this.getRemoteUrl = function(item, startLine){
    if (item && item.remote && item.remote.repo){
      var repo = item.remote.repo;
      if (repo.substr(-4) == '.git') {
        repo = repo.substr(0, repo.length-4);
      }
      var linenum = startLine? startLine:item.startLine;
      if (repo.match(/https:\/\/.*\.visualstudio\.com\/.*/g)){
        // TODO: line not working for vso
        return repo + '#path=/' + item.path+'&line='+linenum;
      }
      if (repo.match(/https:\/\/.*github\.com\/.*/g)){
        return repo + '/blob'+'/'+ item.remote.branch+'/'+ item.path+'/#L'+linenum;
      }
    }else{
      return "#";
    }
  };

  this.getPathInfo = function(currentPath){
    if (!currentPath) return undefined;
    // seperate toc and content with !
    var index = currentPath.indexOf('!');
    if (index < 0){
      // If it ends with .md/.yaml, render it without toc
      if ((/(\.yaml$)|(\.md$)/g).test(currentPath)){
        return {contentPath: currentPath};
      }else{
        return {tocPath: currentPath, tocFilePath: currentPath + '/toc.yaml'};
      }
    }

    return {
      tocPath: currentPath.substring(0, index),
      tocFilePath: currentPath.substring(0, index) + '/toc.yaml',
      contentPath: currentPath.substring(index+1, currentPath.length)
    };
  }

  this.asyncFetchIndex = function(path, success, fail) {
    var deferred = $q.defer();

    //deferred.notify();
    var req = {
            method: 'GET',
            url: path,
            headers: {
            'Content-Type': 'text/plain'
            }
        }
    $http.get(req.url, req)
        .success(
            function(result){
            if (success) success(result);
            deferred.resolve();

            }).error(
            function(result){
                if (fail) fail(result);
                deferred.reject();
            }
            );

    return deferred.promise;
  };

  this.getDefaultItem = function(array, action){
    if (!action) return;
    if (array && array.length > 0){
        return action(array[0]);
    }
  };
}