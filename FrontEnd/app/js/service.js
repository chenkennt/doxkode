function docServiceFunction($q, $http, docConstants) {
  this.tocClassApi = function(navItem) {
    return {
      active: navItem.href && this.pathInfo && this.pathInfo.contentPath,
      current: this.pathInfo.contentPath === navItem.href,
      'nav-index-section': navItem.type === 'section'
    };
  };

  this.gridClassApi = function(toc) {
    return {
      'grid-right': toc,
      grid: !toc
    };
  };

  this.navClassApi = function(navItem) {
    var navPath;
    if (this.pathInfo) {
      navPath = this.pathInfo.tocPath || this.pathInfo.contentPath;
    }

    return {
      active: navItem.href && navPath,
      current: navPath === navItem.href,
    };
  };

  this.getRemoteUrl = function(item, startLine) {
    if (item && item.remote && item.remote.repo) {
      var repo = item.remote.repo;
      if (repo.substr(-4) === '.git') {
        repo = repo.substr(0, repo.length - 4);
      }
      var linenum = startLine ? startLine : item.startLine;
      if (repo.match(/https:\/\/.*\.visualstudio\.com\/.*/g)) {
        // TODO: line not working for vso
        return repo + '#path=/' + item.path + '&line=' + linenum;
      }
      if (repo.match(/https:\/\/.*github\.com\/.*/g)) {
        return repo + '/blob' + '/' + item.remote.branch + '/' + item.path + '/#L' + linenum;
      }
    } else {
      return "#";
    }
  };

  this.setItemTypeVisiblity = function(itemTypes, items) {
    for (var prop in itemTypes) {
      itemTypes[prop].show = false;
    }
    if (!items) return itemTypes;
    items.forEach(function(e) {
      if (itemTypes[e.type] && !itemTypes[e.type].show) itemTypes[e.type].show = true;
    });
    return itemTypes;
  };

  this.getPathInfo = function(currentPath) {
    if (!currentPath) return currentPath;
    // seperate toc and content with !
    var index = currentPath.indexOf(docConstants.TocAndFileUrlSeperator);
    if (index < 0) {
      // If it ends with .md/.yaml, render it without toc
      if ((/(\.yaml$)|(\.md$)/g).test(currentPath)) {
        return {
          contentPath: currentPath
        };
      } else {
        return {
          tocPath: currentPath,
          tocFilePath: currentPath + '/' + docConstants.TocFile
        };
      }
    }

    return {
      tocPath: currentPath.substring(0, index),
      tocFilePath: currentPath.substring(0, index) + '/' + docConstants.TocFile,
      contentPath: currentPath.substring(index + 1, currentPath.length)
    };
  };

  this.getContentFilePath = function(pathInfo) {
    if (!pathInfo) return pathInfo;
    var path = pathInfo.tocPath ? pathInfo.tocPath + '/' : '';
    path += pathInfo.contentPath ? pathInfo.contentPath : docConstants.TocFile;
    return path;
  };

  this.getContentUrl = function(pathInfo) {
    if (!pathInfo) return pathInfo;
    var path = pathInfo.tocPath ? pathInfo.tocPath + docConstants.TocAndFileUrlSeperator : '';
    path += pathInfo.contentPath ? pathInfo.contentPath : docConstants.TocFile;
    return path;
  };

  this.getContentUrlWithTocAndContentUrl = function(tocPath, contentPath) {
    var path = tocPath ? tocPath + docConstants.TocAndFileUrlSeperator : '';
    path += contentPath ? contentPath : docConstants.TocFile;
    return path;
  };

  this.asyncFetchIndex = function(path, success, fail) {
    var deferred = $q.defer();

    //deferred.notify();
    var req = {
      method: 'GET',
      url: path,
      headers: {
        'Content-Type': 'text/plain'
      }
    };
    $http.get(req.url, req)
      .success(
        function(result) {
          if (success) success(result);
          deferred.resolve();

        }).error(
        function(result) {
          if (fail) fail(result);
          deferred.reject();
        }
      );

    return deferred.promise;
  };

  this.getDefaultItem = function(array, action) {
    if (!action) return;
    if (array && array.length > 0) {
      return action(array[0]);
    }
  };
}

angular.module('docInitService', ['docConstants'])
  .service('docService', ['$q', '$http', 'docConstants', docServiceFunction]);