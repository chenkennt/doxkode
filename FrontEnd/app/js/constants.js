function docsConstantsProvider(){
  this.TocFile = 'toc.yaml'; // docConstants.TocFile
  this.TocAndFileUrlSeperator = '!'; // docConstants.TocAndFileUrlSeperator
}

angular.module('docConstants', [])
.service('docConstants', docsConstantsProvider);