function docServiceFunction($q, $http, docConstants, docUtility) {

  function normalizeUrl(url){
    if (!url) return '';
    var arr = url.split(/[/|\\]/);
    var newArray = docUtility.cleanArray(arr);
    return newArray.join('/');
  }

  this.isAbsoluteUrl = function(url){
    if (!url) return false;
    var r = new RegExp('^(?:[a-z]+:)?//', 'i');
    return r.test(url);
  };

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
      navPath = normalizeUrl(this.pathInfo.tocPath || this.pathInfo.contentPath);
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
    if (!currentPath) return '';
    currentPath = normalizeUrl(currentPath);

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
    if (!pathInfo) return '';
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

  this.getPathInfoFromContentPath = function(navList, path){
    // normalize path
    path = normalizeUrl(path);
    if (!navList || navList.length === 0) return {contentPath: path};

    for (var i = 0; i < navList.length; i++){
      var href = navList[i].href;
      href = normalizeUrl(href) + '/'; // Append '/'' so that it must be a full path
      // return the first matched nav
      if (path.startsWith(href)){
        return {
              tocPath: href,
              tocFilePath: href + docConstants.TocFile,
              contentPath: path.replace(href, ''),
            };
      }
    }

    return {contentPath: path};
  };

  this. getAbsolutePath = function(currentUrl, relative){
    var pathInfo = this.getPathInfo(currentUrl);
    if (!pathInfo) return '';
    var current = this.getContentFilePath(pathInfo);
    var sep = '/',
      currentList = current.split(sep),
      relList = relative.split(sep),
      fileName = currentList.pop();

    var relPath = currentList;
    while (relList.length > 0){
      var pathPart = relList.shift();
      if (pathPart === '..'){
        if (relPath.length > 0){
          relPath.pop();
        }else{
          throw "invalid path: " + relative;
        }
      }else{
        relPath.push(pathPart);
      }
    }

    return relPath.join(sep);
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

  this.getTocContent = function($scope, path, tocCache){
    if (path) {
      path = normalizeUrl(path);
      var temp = tocCache.get(path);
      if (temp) {
        $scope.toc = temp;
      } else {
        $scope.toc = {path: path};
        this.asyncFetchIndex(path, function(result) {
          var content = jsyaml.load(result);
          var toc = {
            path: path,
            content: content
          };
          tocCache.put(path, toc);
          $scope.toc = toc;
        }, function() {
          var toc = {
            path: path,
          };
          tocCache.put(path, toc);
          $scope.toc = toc;
        });
      }
    }else{
      $scope.toc = undefined;
    }
  };

  this.getMdContent = function($scope, path, mdIndexCache){
    if (!path) return;
    var pathInfo = this.getPathInfo(path);
    var mdPath = normalizeUrl((pathInfo.tocPath || '') + '/' + 'md.yaml');

    if (mdPath) {
      var tempMdIndex = mdIndexCache.get(mdPath);
      if (tempMdIndex) {
        if (tempMdIndex) $scope.mdIndex = tempMdIndex;
      } else {
        this.asyncFetchIndex(mdPath, function(result) {
          tempMdIndex = jsyaml.load(result);
          // This is the md file path
          mdIndexCache.put(mdPath, tempMdIndex);
          $scope.mdIndex = tempMdIndex;
        });
      }
    }else{
      $scope.toc = undefined;
    }
  };

  this.getDefaultItem = function(array, action) {
    if (!action) return;
    if (array && array.length > 0) {
      return action(array[0]);
    }
  };

  this.getHref = function($scope, current, url){
    if (this.isAbsoluteUrl(url)) return url;
    if (!url) return '';

    var path = this.getAbsolutePath(current, url);
    var pathInfo = this.getPathInfoFromContentPath($scope.navbar, path);

    return '#' + this.getContentUrl(pathInfo);
  };
}

angular.module('docInitService', ['docConstants', 'docUtility'])
  .service('docService', ['$q', '$http', 'docConstants', 'docUtility', docServiceFunction]);