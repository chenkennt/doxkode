/*! Generated by doc-as-code: docascode 2015-04-13 */
function docsConstantsProvider(){this.TocFile="toc.yaml",this.TocAndFileUrlSeperator="!"}function utilityProvider(){this.cleanArray=function(a){"use strict";for(var b=[],c=0;c<a.length;c++)a[c]&&b.push(a[c]);return b}}function docServiceFunction(a,b,c,d){function e(a){if(!a)return"";var b=a.split(/[/|\\]/),c=d.cleanArray(b);return c.join("/")}this.isAbsoluteUrl=function(a){if(!a)return!1;var b=new RegExp("^(?:[a-z]+:)?//","i");return b.test(a)},this.tocClassApi=function(a){return{active:a.href&&this.pathInfo&&this.pathInfo.contentPath,current:this.pathInfo.contentPath===a.href,"nav-index-section":"section"===a.type}},this.gridClassApi=function(a){return{"grid-right":a,grid:!a}},this.navClassApi=function(a){var b;return this.pathInfo&&(b=e(this.pathInfo.tocPath||this.pathInfo.contentPath)),{active:a.href&&b,current:b===a.href}},this.getRemoteUrl=function(a,b){if(!(a&&a.remote&&a.remote.repo))return"#";var c=a.remote.repo;".git"===c.substr(-4)&&(c=c.substr(0,c.length-4));var d=b?b:a.startLine;return c.match(/https:\/\/.*\.visualstudio\.com\/.*/g)?c+"#path=/"+a.path+"&line="+d:c.match(/https:\/\/.*github\.com\/.*/g)?c+"/blob/"+a.remote.branch+"/"+a.path+"/#L"+d:void 0},this.setItemTypeVisiblity=function(a,b){for(var c in a)a[c].show=!1;return b?(b.forEach(function(b){a[b.type]&&!a[b.type].show&&(a[b.type].show=!0)}),a):a},this.getPathInfo=function(a){if(!a)return"";a=e(a);var b=a.indexOf(c.TocAndFileUrlSeperator);return 0>b?/(\.yaml$)|(\.md$)/g.test(a)?{contentPath:a}:{tocPath:a,tocFilePath:a+"/"+c.TocFile}:{tocPath:a.substring(0,b),tocFilePath:a.substring(0,b)+"/"+c.TocFile,contentPath:a.substring(b+1,a.length)}},this.getContentFilePath=function(a){if(!a)return"";var b=a.tocPath?a.tocPath+"/":"";return b+=a.contentPath?a.contentPath:c.TocFile},this.getContentUrl=function(a){if(!a)return a;var b=a.tocPath?a.tocPath+c.TocAndFileUrlSeperator:"";return b+=a.contentPath?a.contentPath:c.TocFile},this.getContentUrlWithTocAndContentUrl=function(a,b){var d=a?a+c.TocAndFileUrlSeperator:"";return d+=b?b:c.TocFile},this.getPathInfoFromContentPath=function(a,b){if(b=e(b),!a||0===a.length)return{contentPath:b};for(var d=0;d<a.length;d++){var f=a[d].href;if(f=e(f)+"/",0===b.indexOf(f))return{tocPath:f,tocFilePath:f+c.TocFile,contentPath:b.replace(f,"")}}return{contentPath:b}},this.getAbsolutePath=function(a,b){var c=this.getPathInfo(a);if(!c)return"";for(var e=this.getContentFilePath(c),f="/",g=d.cleanArray(e.split(f)),h=d.cleanArray(b.split(f)),i=(g.pop(),g);h.length>0;){var j=h.shift();if(".."===j){if(!(i.length>0))throw"invalid path: "+b;i.pop()}else i.push(j)}return i.join(f)},this.asyncFetchIndex=function(c,d,e){var f=a.defer(),g={method:"GET",url:c,headers:{"Content-Type":"text/plain"}};return b.get(g.url,g).success(function(a){d&&d(a),f.resolve()}).error(function(a){e&&e(a),f.reject()}),f.promise},this.getTocContent=function(a,b,c){if(b){b=e(b);var d=c.get(b);d?a.toc=d:(a.toc={path:b},this.asyncFetchIndex(b,function(d){var e=jsyaml.load(d),f={path:b,content:e};c.put(b,f),a.toc=f},function(){var d={path:b};c.put(b,d),a.toc=d}))}else a.toc=void 0},this.getMdContent=function(a,b,c){if(b){var d=this.getPathInfo(b),f=e((d.tocPath||"")+"/md.yaml");if(f){var g=c.get(f);g?g&&(a.mdIndex=g):this.asyncFetchIndex(f,function(b){g=jsyaml.load(b),c.put(f,g),a.mdIndex=g})}else a.toc=void 0}},this.getDefaultItem=function(a,b){return b&&a&&a.length>0?b(a[0]):void 0},this.getHref=function(a,b,c){if(this.isAbsoluteUrl(c))return c;if(!c)return"";var d=this.getAbsolutePath(b,c),e=this.getPathInfoFromContentPath(a.navbar,d);return"#"+this.getContentUrl(e)}}function markdownServiceFunction(){function a(a,b){"use strict";"undefined"==typeof b&&(b=""),"/"!==b.substr(-1,1)&&(b+="/"),"string"==typeof a&&(a=document.getElementById(a));var c=document.createElement("div"),d=document.createElement("div"),e=document.createElement("div");c.innerHTML=a.innerHTML,c.className="csplay_editor",d.className="csplay_splitbar",e.className="csplay_output";var f=a.cloneNode(!1);a.parentNode.replaceChild(f,a),a=f,a.appendChild(c),a.appendChild(d),a.appendChild(e);var g=ace.edit(c);g.getSession().setMode("ace/mode/csharp"),a=$(a),c=$(c),d=$(d),e=$(e);var h=!1;return d.mousedown(function(){h=!0}),a.mouseup(function(){h=!1}).mousemove(function(a){if(h){var b=a.pageY-c.offset().top,f=c.outerHeight(!0)+e.outerHeight(!0)-b,i=d.outerHeight(!0);b>0&&f>0&&(c.css("height",b),e.css("height","calc(100% - "+(b+i)+"px)"),g.resize()),a.preventDefault()}}),{run:function(a){$.ajax({url:b+"run",type:"POST",data:g.getValue(),contentType:"text/plain",success:function(b,c,d){e.html(b).removeClass("error"),"function"==typeof a.success&&a.success(b,c,d)},error:function(b,c,d){"string"==typeof b.responseJSON.error_message?e.text(b.responseJSON.error_message).addClass("error"):e.text(b.responseText).addClass("error"),"function"==typeof a.error&&a.error(b,c,d)},complete:function(b,c){"function"==typeof a.complete&&a.complete(b,c)}})},clearOutput:function(){e.html("")},editor:g}}function b(){"use strict";var b=a("player","http://dotnetsandbox.azurewebsites.net");return b.editor.setTheme("ace/theme/ambiance"),b.editor.setFontSize(16),$("#run").click(function(){var a=$(this);a.html('<i class="fa fa-refresh fa-fw fa-spin"></i>Run'),a.addClass("disabled"),b.run({complete:function(){a.text("Run"),a.removeClass("disabled")}})}),$("#close").click(function(){angular.element("#console").css("margin-left","100%"),b&&b.editor.setReadOnly(!0)}),b}var c;this.tryCode=function(a,d){"use strict";a?("undefined"==typeof c&&(c=b()),c.editor.setValue(d,-1),c.editor.clearSelection(),c.clearOutput(),angular.element("#console").css("margin-left","50%")):angular.element("#console").css("margin-left","100%"),"undefined"!=typeof c&&c.editor.setReadOnly(!a)}}angular.module("docConstants",[]).service("docConstants",docsConstantsProvider),angular.module("docUtility",[]).service("docUtility",utilityProvider),angular.module("directives",[]).directive("backToTop",["$anchorScroll","$location",function(a,b){return function(c,d){d.on("click",function(){b.hash(""),c.$apply(a)})}}]).directive("scrollYOffsetElement",["$anchorScroll",function(a){return function(b,c){a.yOffset=c}}]);var directive={};directive.runnableExample=["$templateCache","$document",function(a,b){"use strict";var c=".runnable-example-file",d=(b[0],'<nav class="runnable-example-tabs" ng-if="tabs">  <a ng-class="{active:$index==activeTabIndex}"ng-repeat="tab in tabs track by $index" href="" class="btn"ng-click="setTab($index)">    {{ tab }}  </a></nav>');return{restrict:"C",scope:!0,controller:["$scope",function(a){a.setTab=function(b){var c=a.tabs[b];a.activeTabIndex=b,a.$broadcast("tabChange",b,c)}}],compile:function(a){return a.html(d+a.html()),function(a,b){{var d=b[0],e=d.querySelectorAll(c),f=[];Date.now()}angular.forEach(e,function(a){f.push(a.getAttribute("name"))}),f.length>0&&(a.tabs=f,a.$on("tabChange",function(a,b){angular.forEach(e,function(a){a.style.display="none"});var c=e[b];c.style.display="block"}),a.setTab(0))}}}}],directive.dropdownToggle=["$document","$location","$window",function(a,b){"use strict";var c,d=null;return{restrict:"C",link:function(e,f){e.$watch(function(){return b.path()},function(){c&&c()}),f.parent().on("click",function(){c&&c()}),f.on("click",function(b){b.preventDefault(),b.stopPropagation();var e=!1;d&&(e=d===f,c()),e||(f.parent().addClass("open"),d=f,c=function(b){b&&b.preventDefault(),b&&b.stopPropagation(),a.off("click",c),f.parent().removeClass("open"),c=null,d=null},a.on("click",c))})}}}],directive.syntax=function(){"use strict";return{restrict:"A",link:function(a,b,c){function d(a,b,c,d){return'<a href="'+c+'" class="btn syntax-'+a+'" target="_blank" rel="nofollow"><span class="'+d+'"></span> '+b+"</a>"}var e="",f={github:{text:"View on Github",key:"syntaxGithub",icon:"icon-github"},plunkr:{text:"View on Plunkr",key:"syntaxPlunkr",icon:"icon-arrow-down"},jsfiddle:{text:"View on JSFiddle",key:"syntaxFiddle",icon:"icon-cloud"}};for(var g in f){var h=f[g],i=c[h.key];i&&(e+=d(g,h.text,i,h.icon))}var j=document.createElement("nav");j.className="syntax-links",j.innerHTML=e;var k=b[0],l=k.parentNode;l.insertBefore(j,k)}}},directive.tabbable=function(){"use strict";return{restrict:"C",compile:function(a){var b=angular.element('<ul class="nav nav-tabs"></ul>'),c=angular.element('<div class="tab-content"></div>');c.append(a.contents()),a.append(b).append(c)},controller:["$scope","$element",function(a,b){var c,d=b.contents().eq(0),e=b.controller("ngModel")||{},f=[];e.$render=function(){var a=this.$viewValue;if((c?c.value!==a:a)&&(c&&(c.paneElement.removeClass("active"),c.tabElement.removeClass("active"),c=null),a)){for(var b=0,d=f.length;d>b;b++)if(a===f[b].value){c=f[b];break}c&&(c.paneElement.addClass("active"),c.tabElement.addClass("active"))}},this.addPane=function(b,g){function h(){k.title=g.title,k.value=g.value||g.title,e.$setViewValue||e.$viewValue&&k!==c||(e.$viewValue=k.value),e.$render()}var i=angular.element("<li><a href></a></li>"),j=i.find("a"),k={paneElement:b,paneAttrs:g,tabElement:i};return f.push(k),g.$observe("value",h)(),g.$observe("title",function(){h(),j.text(k.title)})(),d.append(i),i.on("click",function(b){b.preventDefault(),b.stopPropagation(),e.$setViewValue?a.$apply(function(){e.$setViewValue(k.value),e.$render()}):(e.$viewValue=k.value,e.$render())}),function(){k.tabElement.remove();for(var a=0,b=f.length;b>a;a++)k===f[a]&&f.splice(a,1)}}}]}},directive.table=function(){"use strict";return{restrict:"E",link:function(a,b,c){c["class"]||b.addClass("table table-bordered table-striped code-table")}}};var popoverElement=function(){"use strict";var a={init:function(){this.element=angular.element('<div class="popover popover-incode top"><div class="arrow"></div><div class="popover-inner"><div class="popover-title"><code></code></div><div class="popover-content"></div></div></div>'),this.node=this.element[0],this.element.css({display:"block",position:"absolute"}),angular.element(document.body).append(this.element);var a=this.element.children()[1];this.titleElement=angular.element(a.childNodes[0].firstChild),this.contentElement=angular.element(a.childNodes[1]),this.element.on("click",function(a){a.preventDefault(),a.stopPropagation()});var b=this;angular.element(document.body).on("click",function(){b.visible()&&b.hide()})},show:function(a,b){this.element.addClass("visible"),this.position(a||0,b||0)},hide:function(){this.element.removeClass("visible"),this.position(-9999,-9999)},visible:function(){return this.position().y>=0},isSituatedAt:function(a){return this.besideElement?a[0]===this.besideElement[0]:!1},title:function(a){return this.titleElement.html(a)},content:function(a){return a&&a.length>0&&(a=marked(a)),this.contentElement.html(a)},positionArrow:function(a){this.node.className="popover "+a},positionAway:function(){this.besideElement=null,this.hide()},positionBeside:function(a){this.besideElement=a;var b=a[0],c=b.offsetLeft,d=b.offsetTop;c-=30,d-=this.node.offsetHeight+10,this.show(c,d)},position:function(a,b){return null==a||null==b?{x:this.node.offsetLeft,y:this.node.offsetTop}:(this.element.css("left",a+"px"),void this.element.css("top",b+"px"))}};return a.init(),a.hide(),a};directive.popover=["popoverElement",function(a){"use strict";return{restrict:"A",priority:500,link:function(b,c,d){c.on("click",function(b){b.preventDefault(),b.stopPropagation(),a.isSituatedAt(c)&&a.visible()?(a.title(""),a.content(""),a.positionAway()):(a.title(d.title),a.content(d.content),a.positionBeside(c))})}}}],directive.tabPane=function(){"use strict";return{require:"^tabbable",restrict:"C",link:function(a,b,c,d){b.on("$remove",d.addPane(b,c))}}},directive.foldout=["$http","$animate","$window",function(a,b,c){"use strict";return{restrict:"A",priority:500,link:function(d,e,f){var g,h,i=f.url;/\/build\//.test(c.location.href)&&(i="/build/docs"+i),e.on("click",function(){d.$apply(function(){if(g)g.hasClass("ng-hide")?b.removeClass(g,"ng-hide"):b.addClass(g,"ng-hide");else{if(h)return;h=!0;var c=e.parent();g=angular.element('<div class="foldout">loading...</div>'),b.enter(g,null,c),a.get(i,{cache:!0}).success(function(a){h=!1,a='<div class="foldout-inner"><div calss="foldout-arrow"></div>'+a+"</div>",g.html(a),"block"===g.css("display")&&(g.css("display","none"),b.addClass(g,"ng-hide"))})}})})}}}],angular.module("bootstrap",[]).directive(directive).factory("popoverElement",popoverElement).run(function(){marked.setOptions({gfm:!0,tables:!0})}),angular.module("pagesData",[]).value("NG_PAGES",{}),angular.module("itemTypes",[]).value("NG_ITEMTYPES",{"class":{Property:{name:"Property",description:"Properties",show:!1},Method:{name:"Method",description:"Methods",show:!1},Constructor:{name:"Constructor",description:"Constructors",show:!1},Field:{name:"Field",description:"Fields",show:!1}},namespace:{Class:{name:"Class",description:"Classes",show:!1},Enum:{name:"Enum",description:"Enums",show:!1},Delegate:{name:"Delegate",description:"Delegates",show:!1},Interface:{name:"Interface",description:"Interfaces",show:!1},Struct:{name:"Struct",description:"Structs",show:!1}}}),angular.module("versionsData",[]).value("NG_VERSION",{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}).value("NG_VERSIONS",[{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}]),angular.module("docInitService",["docConstants","docUtility"]).service("docService",["$q","$http","docConstants","docUtility",docServiceFunction]),angular.module("directives",["docInitService"]).service("docsMarkdownService",markdownServiceFunction).directive("markdown",["docsMarkdownService","docService","$location",function(a,b,c){"use strict";var d=function(){marked.setOptions({gfm:!0,pedantic:!1,sanitize:!0});var a=function(a){return a?marked(a):""};return{toHtml:a}}();return{restrict:"AE",link:function(e,f,g){function h(g){var h=d.toHtml(g);f.html(h),angular.forEach(f.find("code"),function(b){hljs.highlightBlock(b),b=b.parentNode;var c=document.createElement("div");c.className="codewrapper",c.innerHTML='<div class="trydiv"><span class="tryspan">Try this code</span></div>',c.childNodes[0].childNodes[0].onclick=function(){a.tryCode(!0,b.innerText)},b.parentNode.replaceChild(c,b),c.appendChild(b)}),angular.forEach(f.find("a"),function(a){var d=a.attributes.href&&a.attributes.href.value;d&&(b.isAbsoluteUrl(d)||(a.attributes.href.value=b.getHref(e,c.path(),d)))}),angular.forEach(f.find("img"),function(a){var d=a.attributes.src&&a.attributes.src.value;d&&(b.isAbsoluteUrl(d)||(a.attributes.src.value=b.getAbsolutePath(c.path(),d)))})}h(f.text()||""),g.ngModel&&e.$watch(g.ngModel,function(a){h(a)}),g.markdown&&e.$watch("markdown",function(a){h(a)})}}}]).directive("declaration",function(){return{restrict:"E",link:function(a,b,c){a.$watch(c.ngModel,function(a){c.ngLanguage;b.html('<pre><code class="lang-language"></code></pre>');var d=b.children("pre").children("code");d.text(a),hljs.highlightBlock(d[0])})}}}).directive("code",function(){return{restrict:"E",terminal:!0,compile:function(a){var b=a.hasClass("linenum"),c=/lang-(\S+)/.exec(a[0].className),d=c&&c[1],e=a.html();a.html(window.prettyPrintOne(e,d,b))}}}),angular.module("errors",["ngSanitize"]).filter("errorLink",["$sanitize",function(a){"use strict";var b=/((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}<>]/g,c=/^mailto:/,d=/:\d+:\d+$/,e=function(a,b){return a.length>b?a.substr(0,b-3)+"...":a};return function(f,g){var h=g?' target="'+g+'"':"";return f?a(f.replace(b,function(a){return d.test(a)?a:(/^((ftp|https?):\/\/|mailto:)/.test(a)||(a="mailto:"+a),"<a"+h+' href="'+a+'">'+e(a.replace(c,""),60)+"</a>")})):f}}]).directive("errorDisplay",["$location","errorLinkFilter",function(a,b){"use strict";var c=function(a){var b=arguments;return a.replace(/\{\d+\}/g,function(a){var c=+a.slice(1,-1);return c+1>=b.length?a:b[c+1]})};return{link:function(d,e,f){var g,h=a.search(),i=[f.errorDisplay];for(g=0;angular.isDefined(h["p"+g]);g++)i.push(h["p"+g]);e.html(b(c.apply(null,i),"_blank"))}}}]),angular.module("versions",[]).controller("DocsVersionsCtrl",["$scope","$location","$window","NG_VERSIONS",function(a,b,c,d){"use strict";a.docs_version=d[0],a.docs_versions=d;for(var e=0,f=0/0;e<d.length;e++){var g=d[e];f<=g.minor||(g.isLatest=!0,f=g.minor)}a.getGroupName=function(a){return a.isLatest?"Latest":"v"+a.major+"."+a.minor+".x"},a.jumpToDocsVersion=function(a){var d=b.path().replace(/\/$/,"");c.location=a.docsUrl+d}}]);var player;angular.module("docsApp",["ngRoute","ngCookies","ngSanitize","docConstants","docUtility","docCtrl","versionsData","pagesData","itemTypes","directives","errors","versions","bootstrap","ui.bootstrap","ui.bootstrap.dropdown"]),angular.module("docCtrl",["docInitService","docUtility"]).factory("tocCache",["$cacheFactory",function(a){return a("toc-cache")}]).factory("mdIndexCache",["$cacheFactory",function(a){return a("mdIndex-cache")}]).controller("DocsController",["$scope","$http","$q","$rootScope","$location","$window","$cookies","$timeout","NG_PAGES","NG_VERSION","NG_ITEMTYPES","docService","tocCache","mdIndexCache","docUtility","docsMarkdownService",function(a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p){"use strict";function q(){if(a.mdIndex&&a.partialModel){var b=a.mdIndex[a.partialModel.id];if(b&&b.href){a.partialModel.mdHref=l.getRemoteUrl(b);{var c=l.getPathInfo(e.path()).tocPath,d=(c||"")+"/"+b.href;l.asyncFetchIndex(d,function(c){var d=c.substr(b.startLine,b.endLine-b.startLine+1);a.partialModel.mdContent=d})}}}}a.docsVersion=j.isSnapshot?"snapshot":j.version,a.tocClass=l.tocClassApi,a.gridClass=l.gridClassApi,a.navClass=l.navClassApi;var r=function(){marked.setOptions({gfm:!0,pedantic:!1,sanitize:!0});var a=function(a){return a?marked(a):""};return{toHtml:a}}();a.finishLoading=function(){h(function(){var b=($("div.declaration-content").html(function(){var a=$(this).attr("ng-language");return'<pre><code class="lang-'+a+'">'+$(this).context.innerText+"</code></pre>"}).each(function(a,b){hljs.highlightBlock(b)}),$("div.markdown").html(function(){var a=r.toHtml($(this).text());return a}));b.find("code").each(function(a,b){hljs.highlightBlock(b),b=b.parentNode;var c=document.createElement("div");c.className="codewrapper",c.innerHTML='<div class="trydiv"><span class="tryspan">Try this code</span></div>',c.childNodes[0].childNodes[0].onclick=function(){p.tryCode(!0,b.innerText)},b.parentNode.replaceChild(c,b),c.appendChild(b)}),b.find("code").each(function(b,c){var d=c.attributes.href&&c.attributes.href.value;d&&(l.isAbsoluteUrl(d)||(c.attributes.href.value=l.getHref(a,e.path(),d)))}),b.find("code").each(function(a,b){var c=b.attributes.src&&b.attributes.src.value;c&&(l.isAbsoluteUrl(c)||(b.attributes.src.value=l.getAbsolutePath(e.path(),c)))})},1,!1)},a.getNumber=function(a){return new Array(a+1)},a.GetDetail=function(a){var b=a.target.nextElementSibling.style.display;a.target.nextElementSibling.style.display="block"===b?"none":"block"},a.ViewSource=function(){return l.getRemoteUrl(this.model.source,this.model.source.startLine+1)},a.ImproveThisDoc=function(){return a.partialModel.mdContent},a.GetTocHref=function(b){return a.toc?l.getHref(a,a.toc.path,b):""},a.GetNavHref=function(a){return l.isAbsoluteUrl(a)?a:a?"#"+a:""},a.GetLinkHref=function(b){return l.getHref(a,e.path(),b)},a.$on("$includeContentLoaded",function(){}),function(){var b="toc.yaml";l.asyncFetchIndex(b,function(b){a.navbar=jsyaml.load(b),l.getDefaultItem(a.navbar,function(a){!e.path()&&a.href&&e.url(a.href)})})}(),a.$watch(function(){return e.path()},function(b){b=b.replace(/^\/?(.+?)(\/index)?\/?$/,"$1");var c=a.currentPage=b;if(c){var d=l.getPathInfo(c);a.pathInfo=d,l.getTocContent(a,d.tocFilePath,m),b=l.getContentFilePath(d),b&&(/\.md$/g.test(b)?(a.contentType="md",a.partialModel={},a.title=b,a.partialPath=b):/\.yaml$/g.test(b)?(a.contentType="yaml",l.getMdContent(a,c,n),l.asyncFetchIndex(b,function(b){var c=a.partialModel=jsyaml.load(b);c instanceof Array?a.partialPath="template/tocpage.tmpl":(a.title=c.id,"namespace"===c.type.toLowerCase()?(a.itemtypes=l.setItemTypeVisiblity(k.namespace,c.items),a.partialPath="template/namespace.tmpl"):(a.itemtypes=l.setItemTypeVisiblity(k["class"],c.items),a.partialPath="template/class.tmpl"))},function(){a.breadcrumb=[],a.partialPath="template/error404.tmpl"})):a.partialPath=b);var f=d.tocPath?d.tocPath.split("/"):[];d.contentPath&&f.push("!"+d.contentPath);var g=a.breadcrumb=[],h="";angular.forEach(f,function(a){a&&(h+=a,g.push({name:a,url:h}),h+="/")})}else a.navbar&&a.navbar.length>0&&e.url(a.navbar[0].href)}),a.$watch(function(){return a.partialModel},q),a.$watch(function(){return a.mdIndex},q),a.$watch(function(){return a.navbar},function(a){!e.path()&&a&&a.count>0&&e.url(a[0].href)}),a.versionNumber=angular.version.full,a.version=angular.version.full+"  "+angular.version.codeName,a.loading=0}]);