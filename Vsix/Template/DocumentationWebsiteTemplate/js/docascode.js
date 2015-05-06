/*! Generated by doc-as-code: docascode 2015-05-06 */
!function(){"use strict";function a(a,b,c,d,e,f){function g(a){return jsyaml.load(a.data)}function h(b){var c=a.defer();return c.resolve(b),c.promise}this.valueHttpWrapper=h,this.getNavBar=function(){return b.get(c.TocFile).then(g)},this.getContent=function(a){return b.get(a).then(g)},this.getMarkdownContent=function(a){function c(a){return a.data}return b.get(a).then(c)},this.getTocContent=function(a){if(!a)return h(null);var c;a=d.normalizeUrl(a);var f=e.get(a);return f?h(f):(c={path:a},b.get(a).then(function(b){var d=g(b);return c.content=d,e.put(a,c),c})["catch"](function(b){return e.put(a,c),c}))},this.getMdContent=function(a){if(!a)return h(null);var c;return a?(c=f.get(a),void 0!==c?h(c):b.get(a).then(function(b){var c=b;return f.put(a,c),c})["catch"](function(b){return f.put(a,null),h(null)})):h(null)},this.getDefaultItem=function(a,b){return b&&a&&a.length>0?b(a[0]):void 0}}angular.module("docascode.contentService",["docascode.constants","docascode.urlService"]).factory("myInterceptor",["$q",function(a){return{response:function(b){return 404===b.status?a.resolve():a.reject(b)},responseError:function(b){return 404===b.status?a.resolve():a.reject(b)}}}]).config(["$httpProvider",function(a){}]).factory("tocCache",["$cacheFactory",function(a){return a("toc-cache")}]).factory("mdIndexCache",["$cacheFactory",function(a){return a("mdIndex-cache")}]).service("contentService",["$q","$http","docConstants","urlService","tocCache","mdIndexCache",a])}(),function(){"use strict";function a(){function a(a,b){"undefined"==typeof b&&(b=""),"/"!==b.substr(-1,1)&&(b+="/"),"string"==typeof a&&(a=document.getElementById(a));var c=document.createElement("div"),d=document.createElement("div"),e=document.createElement("div");c.innerHTML=a.innerHTML,c.className="csplay_editor",d.className="csplay_splitbar",e.className="csplay_output";var f=a.cloneNode(!1);a.parentNode.replaceChild(f,a),a=f,a.appendChild(c),a.appendChild(d),a.appendChild(e);var g=ace.edit(c);g.getSession().setMode("ace/mode/csharp"),a=$(a),c=$(c),d=$(d),e=$(e);var h=!1;return d.mousedown(function(a){h=!0}),a.mouseup(function(){h=!1}).mousemove(function(a){if(h){var b=a.pageY-c.offset().top,f=c.outerHeight(!0)+e.outerHeight(!0)-b,i=d.outerHeight(!0);b>0&&f>0&&(c.css("height",b),e.css("height","calc(100% - "+(b+i)+"px)"),g.resize()),a.preventDefault()}}),{run:function(a){$.ajax({url:b+"run",type:"POST",data:g.getValue(),contentType:"text/plain",success:function(b,c,d){e.html(b).removeClass("error"),"function"==typeof a.success&&a.success(b,c,d)},error:function(b,c,d){"string"==typeof b.responseJSON.error_message?e.text(b.responseJSON.error_message).addClass("error"):e.text(b.responseText).addClass("error"),"function"==typeof a.error&&a.error(b,c,d)},complete:function(b,c){"function"==typeof a.complete&&a.complete(b,c)}})},clearOutput:function(){e.html("")},editor:g}}function b(){var b=a("player","http://dotnetsandbox.azurewebsites.net");return b.editor.setTheme("ace/theme/ambiance"),b.editor.setFontSize(16),$("#run").click(function(){var a=$(this);a.html('<i class="fa fa-refresh fa-fw fa-spin"></i>Run'),a.addClass("disabled"),b.run({complete:function(){a.text("Run"),a.removeClass("disabled")}})}),$("#close").click(function(){angular.element("#console").css("margin-left","100%"),b&&b.editor.setReadOnly(!0)}),b}var c;this.tryCode=function(a,d){a?("undefined"==typeof c&&(c=b()),c.editor.setValue(d,-1),c.editor.clearSelection(),c.clearOutput(),angular.element("#console").css("margin-left","50%")):angular.element("#console").css("margin-left","100%"),"undefined"!=typeof c&&c.editor.setReadOnly(!a)}}angular.module("docascode.csplayService",["docascode.constants","docascode.util"]).service("csplayService",["$q","$http","docConstants","docUtility",a])}(),function(){"use strict";function a(a,b,c,d){function e(a){if(!a)return"";var b=a.split(/[/|\\]/),c=d.cleanArray(b);return c.join("/")}function f(a,b){return e(a)===e(b)}this.isAbsoluteUrl=function(a){if(!a)return!1;var b=new RegExp("^(?:[a-z]+:)?//","i");return b.test(a)},this.normalizeUrl=e,this.urlAreEqual=f,this.getRemoteUrl=function(a,b){if(!a||!a.repo)return"";var c=a.repo;".git"===c.substr(-4)&&(c=c.substr(0,c.length-4));var d=b?b:0;if(c.match(/https:\/\/.*\.visualstudio\.com\/.*/g))return c+"#path=/"+a.path;if(c.match(/https:\/\/.*github\.com\/.*/g)){var e=c+"/blob/"+a.branch+"/"+a.path;return d>0&&(e+="/#L"+d),e}},this.setItemTypeVisiblity=function(a,b){for(var c in a)a[c].show=!1;if(!b)return a;if(angular.isArray(b))b.forEach(function(b){a[b.type]&&!a[b.type].show&&(a[b.type].show=!0)});else for(var d in b)if(b.hasOwnProperty(d)){var e=b[d];a[e.type]&&!a[e.type].show&&(a[e.type].show=!0)}return a},this.getPathInfo=function(a){if(!a)return"";a=e(a);var b=a.indexOf(c.TocAndFileUrlSeperator);return 0>b?c.MdOrYamlRegexExp.test(a)?{contentPath:a}:{tocPath:a,tocFilePath:a+"/"+c.TocFile}:{tocPath:a.substring(0,b),tocFilePath:a.substring(0,b)+"/"+c.TocFile,contentPath:a.substring(b+1,a.length)}},this.getContentFilePath=function(a){if(!a)return"";var b=a.tocPath?a.tocPath+"/":"";return b+=a.contentPath?a.contentPath:c.TocFile},this.getContentUrl=function(a){if(!a)return a;var b=a.tocPath?a.tocPath+c.TocAndFileUrlSeperator:"";return b+=a.contentPath?a.contentPath:c.TocFile},this.getContentUrlWithTocAndContentUrl=function(a,b){var d=a?a+c.TocAndFileUrlSeperator:"";return d+=b?b:c.TocFile},this.getPathInfoFromContentPath=function(a,b){if(b=e(b),!a||0===a.length)return{contentPath:b};for(var d=0;d<a.length;d++){var f=a[d].href;if(f=e(f)+"/",0===b.indexOf(f))return{tocPath:f,tocFilePath:f+c.TocFile,contentPath:b.replace(f,"")}}return{contentPath:b}},this.getAbsolutePath=function(a,b){if(!a)return b;var c=this.getPathInfo(a);if(!c)return"";for(var e=this.getContentFilePath(c),f="/",g=d.cleanArray(e.split(f)),h=d.cleanArray(b.split(f)),i=(g.pop(),g);h.length>0;){var j=h.shift();".."===j?i.length>0?i.pop():i.push(".."):i.push(j)}return i.join(f)},this.asyncFetchIndex=function(c,d,e){var f=a.defer(),g={method:"GET",url:c,headers:{"Content-Type":"text/plain"}};return b.get(g.url,g).success(function(a){d&&d(a),f.resolve()}).error(function(a){e&&e(a),f.reject()}),f.promise},this.getTocContent=function(a,b,c){if(b){b=e(b);var d=c.get(b);d?a.toc=d:(a.toc={path:b},this.asyncFetchIndex(b,function(d){var e=jsyaml.load(d),f={path:b,content:e};c.put(b,f),a.toc=f},function(){var d={path:b};c.put(b,d),a.toc=d}))}else a.toc=void 0},this.getMdContent=function(a,b,d){if(b){var f=this.getPathInfo(b),g=e((f.tocPath||"")+"/"+c.MdIndexFile);if(g){var h=d.get(g);h?h&&(a.mdIndex=h):this.asyncFetchIndex(g,function(b){h=jsyaml.load(b),d.put(g,h),a.mdIndex=h})}else a.toc=void 0}},this.getDefaultItem=function(a,b){return b&&a&&a.length>0?b(a[0]):void 0},this.getHref=function(a,b,c){if(this.isAbsoluteUrl(c))return c;if(!c)return"";var d=this.getAbsolutePath(b,c);return"#"+this.getContentUrl({tocPath:a,contentPath:d})},this.getPageHref=function(a,b){var c=this.getPathInfo(a);return this.getHref(c.tocPath,c.contentPath,b)}}angular.module("docascode.urlService",["docascode.constants","docascode.util"]).service("urlService",["$q","$http","docConstants","docUtility",a])}(),function(){"use strict";function a(a,b,c,d,e,f,g){function h(a,b){var h=i.toHtml(b);a.html(h),angular.forEach(a.find("code"),function(a){if(hljs.highlightBlock(a),a=a.parentNode,angular.element(a).is("pre")){var b=document.createElement("div");b.className="codewrapper",b.innerHTML='<div class="trydiv"><span class="tryspan">Try this code</span></div>',b.childNodes[0].childNodes[0].onclick=function(){e.tryCode(!0,a.innerText)},a.parentNode.replaceChild(b,a),b.appendChild(a)}}),angular.forEach(a.find("a"),function(a){var b=a.attributes.href&&a.attributes.href.value;b&&(d.isAbsoluteUrl(b)||(a.attributes.href.value=d.getPageHref(c.path(),b)))}),angular.forEach(a.find("img"),function(a){var b=a.attributes.src&&a.attributes.src.value;b&&(d.isAbsoluteUrl(b)||(a.attributes.src.value=d.getAbsolutePath(c.path(),b)))}),angular.forEach(a.find("table"),function(a){angular.element(a).addClass("table table-bordered table-striped table-condensed")}),angular.forEach(a.find("code-snippet",function(a){var b=a.attributes.src,c=a.attributes.sl,d=a.attributes.el;f.getMdContent(b).then(function(b){var e=b.data,f=g.substringLine(e,c,d),h=document.createElement("div");h.innerHTML=f,a.parentNode.replaceChild(h,a)})}))}this.transform=h;var i=function(){marked.setOptions({gfm:!0,pedantic:!1,sanitize:!1});var a=function(a){return a?marked(a):""};return{toHtml:a}}()}angular.module("docascode.markdownService",["docascode.urlService","docascode.csplayService","docascode.contentService","docascode.util"]).service("markdownService",["$q","$http","$location","urlService","csplayService","contentService","docUtility",a])}(),angular.module("docsApp",["ngRoute","ngSanitize","itemTypes","bootstrap","ui.bootstrap","ui.bootstrap.dropdown","docascode.controller","docascode.directives"]);var directive={};directive.runnableExample=["$templateCache","$document",function(a,b){"use strict";var c=".runnable-example-file",d=(b[0],'<nav class="runnable-example-tabs" ng-if="tabs">  <a ng-class="{active:$index==activeTabIndex}"ng-repeat="tab in tabs track by $index" href="" class="btn"ng-click="setTab($index)">    {{ tab }}  </a></nav>');return{restrict:"C",scope:!0,controller:["$scope",function(a){a.setTab=function(b){var c=a.tabs[b];a.activeTabIndex=b,a.$broadcast("tabChange",b,c)}}],compile:function(a){return a.html(d+a.html()),function(a,b){{var d=b[0],e=d.querySelectorAll(c),f=[];Date.now()}angular.forEach(e,function(a,b){f.push(a.getAttribute("name"))}),f.length>0&&(a.tabs=f,a.$on("tabChange",function(a,b,c){angular.forEach(e,function(a){a.style.display="none"});var d=e[b];d.style.display="block"}),a.setTab(0))}}}}],directive.dropdownToggle=["$document","$location","$window",function(a,b,c){"use strict";var d,e=null;return{restrict:"C",link:function(c,f,g){c.$watch(function(){return b.path()},function(){d&&d()}),f.parent().on("click",function(a){d&&d()}),f.on("click",function(b){b.preventDefault(),b.stopPropagation();var c=!1;e&&(c=e===f,d()),c||(f.parent().addClass("open"),e=f,d=function(b){b&&b.preventDefault(),b&&b.stopPropagation(),a.off("click",d),f.parent().removeClass("open"),d=null,e=null},a.on("click",d))})}}}],directive.syntax=function(){"use strict";return{restrict:"A",link:function(a,b,c){function d(a,b,c,d){return'<a href="'+c+'" class="btn syntax-'+a+'" target="_blank" rel="nofollow"><span class="'+d+'"></span> '+b+"</a>"}var e="",f={github:{text:"View on Github",key:"syntaxGithub",icon:"icon-github"},plunkr:{text:"View on Plunkr",key:"syntaxPlunkr",icon:"icon-arrow-down"},jsfiddle:{text:"View on JSFiddle",key:"syntaxFiddle",icon:"icon-cloud"}};for(var g in f){var h=f[g],i=c[h.key];i&&(e+=d(g,h.text,i,h.icon))}var j=document.createElement("nav");j.className="syntax-links",j.innerHTML=e;var k=b[0],l=k.parentNode;l.insertBefore(j,k)}}},directive.tabbable=function(){"use strict";return{restrict:"C",compile:function(a){var b=angular.element('<ul class="nav nav-tabs"></ul>'),c=angular.element('<div class="tab-content"></div>');c.append(a.contents()),a.append(b).append(c)},controller:["$scope","$element",function(a,b){var c,d=b.contents().eq(0),e=b.controller("ngModel")||{},f=[];e.$render=function(){var a=this.$viewValue;if((c?c.value!==a:a)&&(c&&(c.paneElement.removeClass("active"),c.tabElement.removeClass("active"),c=null),a)){for(var b=0,d=f.length;d>b;b++)if(a===f[b].value){c=f[b];break}c&&(c.paneElement.addClass("active"),c.tabElement.addClass("active"))}},this.addPane=function(b,g){function h(){k.title=g.title,k.value=g.value||g.title,e.$setViewValue||e.$viewValue&&k!==c||(e.$viewValue=k.value),e.$render()}var i=angular.element("<li><a href></a></li>"),j=i.find("a"),k={paneElement:b,paneAttrs:g,tabElement:i};return f.push(k),g.$observe("value",h)(),g.$observe("title",function(){h(),j.text(k.title)})(),d.append(i),i.on("click",function(b){b.preventDefault(),b.stopPropagation(),e.$setViewValue?a.$apply(function(){e.$setViewValue(k.value),e.$render()}):(e.$viewValue=k.value,e.$render())}),function(){k.tabElement.remove();for(var a=0,b=f.length;b>a;a++)k===f[a]&&f.splice(a,1)}}}]}},directive.table=function(){"use strict";return{restrict:"E",link:function(a,b,c){c["class"]||b.addClass("table table-bordered table-striped table-condensed")}}};var popoverElement=function(){"use strict";var a={init:function(){this.element=angular.element('<div class="popover popover-incode top"><div class="arrow"></div><div class="popover-inner"><div class="popover-title"><code></code></div><div class="popover-content"></div></div></div>'),this.node=this.element[0],this.element.css({display:"block",position:"absolute"}),angular.element(document.body).append(this.element);var a=this.element.children()[1];this.titleElement=angular.element(a.childNodes[0].firstChild),this.contentElement=angular.element(a.childNodes[1]),this.element.on("click",function(a){a.preventDefault(),a.stopPropagation()});var b=this;angular.element(document.body).on("click",function(a){b.visible()&&b.hide()})},show:function(a,b){this.element.addClass("visible"),this.position(a||0,b||0)},hide:function(){this.element.removeClass("visible"),this.position(-9999,-9999)},visible:function(){return this.position().y>=0},isSituatedAt:function(a){return this.besideElement?a[0]===this.besideElement[0]:!1},title:function(a){return this.titleElement.html(a)},content:function(a){return a&&a.length>0&&(a=marked(a)),this.contentElement.html(a)},positionArrow:function(a){this.node.className="popover "+a},positionAway:function(){this.besideElement=null,this.hide()},positionBeside:function(a){this.besideElement=a;var b=a[0],c=b.offsetLeft,d=b.offsetTop;c-=30,d-=this.node.offsetHeight+10,this.show(c,d)},position:function(a,b){return null==a||null==b?{x:this.node.offsetLeft,y:this.node.offsetTop}:(this.element.css("left",a+"px"),void this.element.css("top",b+"px"))}};return a.init(),a.hide(),a};directive.popover=["popoverElement",function(a){"use strict";return{restrict:"A",priority:500,link:function(b,c,d){c.on("click",function(b){b.preventDefault(),b.stopPropagation(),a.isSituatedAt(c)&&a.visible()?(a.title(""),a.content(""),a.positionAway()):(a.title(d.title),a.content(d.content),a.positionBeside(c))})}}}],directive.tabPane=function(){"use strict";return{require:"^tabbable",restrict:"C",link:function(a,b,c,d){b.on("$remove",d.addPane(b,c))}}},directive.foldout=["$http","$animate","$window",function(a,b,c){"use strict";return{restrict:"A",priority:500,link:function(d,e,f){var g,h,i=f.url;/\/build\//.test(c.location.href)&&(i="/build/docs"+i),e.on("click",function(){d.$apply(function(){if(g)g.hasClass("ng-hide")?b.removeClass(g,"ng-hide"):b.addClass(g,"ng-hide");else{if(h)return;h=!0;var c=e.parent();g=angular.element('<div class="foldout">loading...</div>'),b.enter(g,null,c),a.get(i,{cache:!0}).success(function(a){h=!1,a='<div class="foldout-inner"><div calss="foldout-arrow"></div>'+a+"</div>",g.html(a),"block"===g.css("display")&&(g.css("display","none"),b.addClass(g,"ng-hide"))})}})})}}}],angular.module("bootstrap",[]).directive(directive).factory("popoverElement",popoverElement).run(function(){marked.setOptions({gfm:!0,tables:!0})}),function(){"use strict";function a(){this.YamlExtension=".yml",this.MdExtension=".md",this.TocYamlRegexExp=/toc\.yml$/,this.YamlRegexExp=/\.yml$/,this.MdRegexExp=/\.md$/,this.MdOrYamlRegexExp=/(\.yml$)|(\.md$)/,this.MdIndexFile=".map",this.TocFile="toc"+this.YamlExtension,this.TocAndFileUrlSeperator="!"}angular.module("docascode.constants",[]).service("docConstants",a)}(),function(){"use strict";function a(a,b,c,d,e,f,g,h){a.$on("activeNavItemChanged",function(a,b){b.active,b.parent}),a.$on("activeTocItemChanged",function(a,b){b.active,b.parent})}angular.module("docascode.controller",["docascode.contentService","docascode.urlService","docascode.directives","docascode.util","docascode.constants"]).controller("DocsController",["$rootScope","$scope","$location","NG_ITEMTYPES","contentService","urlService","docUtility","docConstants",a])}(),function(){"use strict";function a(a,b,c,d,e,f,g){function h(a){if(a){var c=f.getPathInfo(a),d=f.getContentFilePath(c),e=b;if(d){if(e.tocPage=g.TocYamlRegexExp.test(d)?d:"",e.tocPage)return;e.contentPath=d,g.MdRegexExp.test(d)?(e.title=d,e.contentType="markdown"):g.YamlRegexExp.test(d)?e.contentType="yaml":e.contentType="error"}}}function i(a){var b=c.path(),d=f.getPathInfo(b);return d.contentPath="",f.getHref(d.tocPath,"",a)}function j(a){var b=!a;this.toc.content.forEach(function(c,d,e){var f=c.uid||c.name,g=!b&&!k(f,a),h=g;c.items&&c.items.forEach(function(b,c,d){var e=f+"."+b.name;b.hide=h&&!k(e,a),b.hide||(g=!1)}),c.hide=g})}function k(a,b){return b?a.toLowerCase().indexOf(b.toLowerCase())>-1?!0:!1:!0}function l(d){var e=c.path(),g=f.getPathInfo(e),h={active:d.href&&g.contentPath===d.href,"nav-index-section":"section"===d.type};if(h.active===!0){var i=b.currentTocSelectedItem;if(d&&i!==d){b.currentTocSelectedItem=i=d;var j={active:d};d===this.navItem&&(j.parent=this.navGroup),a.$broadcast("activeTocItemChanged",j)}}return h}b.loading=0,b.contentLoading=0,b.tocLoading=0,b.filteredItems=j,b.tocClass=l,b.getTocHref=i,b.getNumber=function(a){return new Array(a+1)},b.toc=null,b.content=null,a.$on("navbarLoaded",function(a,b){}),b.$watch(function(){return c.path()},function(a){h(a)}),a.$on("activeNavItemChanged",function(a,c){b.toc=null,"error"===b.contentType&&(b.contentType="success");var d=c.active;b.currentHomepage=d.homepage;var f=c.active,h=f.href+"/"+g.TocFile;e.getTocContent(h).then(function(a){a.content&&(b.toc=a)})}),a.$on("activeNavItemError",function(a,c){b.contentType="error",b.toc=null}),b.$watchGroup(["tocPage","currentHomepage"],function(a,c){if(a){var d=a[0],e=a[1];if(d){var f=b;"error"!==f.contentType&&(e?(f.contentPath=e,f.contentType="markdown"):(f.contentPath=d,f.contentType="toc"))}}})}angular.module("docascode.controller").controller("ContainerController",["$rootScope","$scope","$location","NG_ITEMTYPES","contentService","urlService","docConstants",a])}(),function(){"use strict";function a(a,b,c,d,e,f,g){function h(a,c,d){var e=b.breadcrumb=[];a&&e.push({name:a.name,url:"/#/"+a.href}),c&&e.push({name:c.uid||c.name,url:c.href}),d&&e.push({name:d.name})}function i(a){if(a&&0===a.indexOf("/#/"))return a.substring(1);var b=c.path(),d=f.getPathInfo(b);return f.getHref(d.tocPath,"",a)}function j(a){var b,d=c.path(),e=f.getPathInfo(d);e&&(b=f.normalizeUrl(e.tocPath||e.contentPath));var g={active:b&&b===a.href};return g}function k(a){return f.isAbsoluteUrl(a)?a:a?"#"+a:""}b.navClass=j,b.getNavHref=k,b.getBreadCrumbHref=i,a.$on("activeTocItemChanged",function(a,c){var d=c.active,e=c.parent;h(b.currentNavItem,e,d)}),e.getNavBar().then(function(d){var g=d;e.getDefaultItem(g,function(a){!c.path()&&a.href&&c.url(a.href)}),b.model=g,a.$broadcast("navbarLoaded",{navbar:d}),b.$watch(function(){return c.path()},function(c){var d=f.getPathInfo(c);if(d){var e=d.tocPath||d.contentPath,g=b.model.filter(function(a){return f.urlAreEqual(a.href,e)})[0];if(h(g),g){var i=b.currentNavItem;g&&i!==g&&(b.currentNavItem=i=g,a.$broadcast("activeNavItemChanged",{active:g}))}else b.currentNavItem=null,a.$broadcast("activeNavItemError")}})},function(b){a.$broadcast("navbarError",{})})}angular.module("docascode.controller").controller("NavbarController",["$rootScope","$scope","$location","NG_ITEMTYPES","contentService","urlService","docConstants",a])}(),angular.module("pagesData",[]).value("NG_PAGES",{}),angular.module("itemTypes",[]).value("NG_ITEMTYPES",{"class":{Property:{name:"Property",description:"Properties",show:!1},Method:{name:"Method",description:"Methods",show:!1},Constructor:{name:"Constructor",description:"Constructors",show:!1},Field:{name:"Field",description:"Fields",show:!1}},namespace:{Class:{name:"Class",description:"Classes",show:!1},Enum:{name:"Enum",description:"Enums",show:!1},Delegate:{name:"Delegate",description:"Delegates",show:!1},Interface:{name:"Interface",description:"Interfaces",show:!1},Struct:{name:"Struct",description:"Structs",show:!1}}}),function(){"use strict";angular.module("docascode.directives",["itemTypes","docascode.urlService","docascode.util","docascode.markdownService","docascode.csplayService","docascode.contentService"]).directive("backToTop",["$anchorScroll","$location",function(a,b){return function(c,d){d.on("click",function(d){b.hash(""),c.$apply(a)})}}]).directive("scrollYOffsetElement",["$anchorScroll",function(a){return function(b,c){a.yOffset=c}}]).directive("code",function(){return{restrict:"E",require:"ngModel",scope:{bindonce:"@"},terminal:!0,link:function(a,b,c,d){var e=a.$watch(function(){return d.$modelValue},function(c,d){if(void 0!==c){var f,g;c.CSharp?(f="csharp",g=c.CSharp):c.VB&&(f="vb",g=c.VB),b.text(g),angular.forEach(b,function(a){hljs.highlightBlock(a,f)}),a.bindonce&&e()}})}}})}(),function(){"use strict";function a(a,b,c,d,e,f){function g(a){return function(b){return b.uid===a}}function h(a){return a?e.getRemoteUrl(a.remote,a.startLine+1):""}function i(a){return a&&a.source&&a.source.remote?e.getRemoteUrl(a.source.remote,a.source.startLine+1):""}function j(a){return new Array(a+1)}function k(a){var c=b.path(),d=e.getPathInfo(c);return e.getHref(d.tocPath,d.contentPath,a)}function l(a,b){if(a&&a.items){var c=a.items;for(var d in c)if(c.hasOwnProperty(d)){var e=c[d];e.showDetail=b}}}function m(b,d,f,h){f&&(b.contentType="",b.model={},b.title="",c.getContent(f).then(function(c){var d=c.items,i=c.references||[],j=d[0];if(i=d.slice(1).concat(i||[]),j.children){for(var k={},l=0,m=j.children.length;m>l;l++){var o=i.filter(g(j.children[l]))[0]||{};o.uid&&(k[o.uid]=o)}j.items=k}"namespace"===j.type.toLowerCase()?(b.contentType="namespace",b.itemtypes=e.setItemTypeVisiblity(a.namespace,j.items)):(b.contentType="class",b.itemtypes=e.setItemTypeVisiblity(a["class"],j.items)),b.model=j,b.title=j.name,h&&n(f+".map",b.model)})["catch"](function(){b.contentType="error"}))}function n(a,b){c.getMdContent(a).then(function(a){if(a){var c=a.data;for(var d in c)if(c.hasOwnProperty(d)){var e=c[d];if(d===b.uid)b.map=e,o(b.map);else{var f=b.items[d];f&&(f.map=e,o(f.map))}}}},function(a){})}function o(a){var d=a.href,g=a.startLine,h=a.endLine,i=(a.override,a.references),j=e.getAbsolutePath(b.path(),d);c.getMdContent(j).then(function(b){if(b){var d=b.data,k=f.substringLine(d,g,h),l=k;if(i){for(var m in i)if(i.hasOwnProperty(m)){var n=i[m],o="";if("codeSnippet"===n.type)if(n.href){{var r=e.getAbsolutePath(j,n.href);n.startLine,n.endLine}c.getMdContent(r).then(p(a,n.Keys))}else{var s="Warning: Unable to resolve "+n.id+": "+n.message;l=q(n.Keys,l,s)}else{var t=n.id,u=n.href;o="<a href='"+u+"'>"+t+"</a>",l=q(n.Keys,l,o)}}a.content=l}}})}function p(a,b){return function(c){if(c){var d=c.data,e=a.content;a.content=q(b,e,d)}}}function q(a,b,c){for(var d=0;d<a.length;d++){var e=new RegExp(f.escapeRegExp(a[d]),"g");b=b.replace(e,c)}return b}function r(a){var c=b.path(),d=e.getPathInfo(c);return d.contentPath="",e.getHref(d.tocPath,"",a)}function s(a){a.getViewSourceHref=i,a.getImproveTheDocHref=h,a.getLinkHref=k,a.expandAll=l,a.getNumber=j,a.getTocHref=r}return s.$inject=["$scope"],{restrict:"E",replace:!0,templateUrl:"template/yamlContent.html",priority:100,require:"ngModel",scope:{getMap:"="},controller:s,link:function(a,b,c,d){var e=a;a.$watch(function(){return d.$modelValue},function(a,c){void 0!==a&&m(e,b,a,e.getMap)})}}}angular.module("docascode.directives").directive("yamlContent",["NG_ITEMTYPES","$location","contentService","markdownService","urlService","docUtility",a])}(),function(){"use strict";function a(a,b,c){function d(a,c){b.transform(a,c)}function e(b,c){c&&(b.html(""),a.getMarkdownContent(c).then(function(a){d(b,a)},function(a){b.html(a.data)}))}return{restrict:"AE",link:function(a,b,c){c.src&&a.$watch(c.src,function(a,c){e(b,a)}),c.data&&a.$watch(c.data,function(a,c){void 0!==a&&d(b,a)})}}}function b(a,b,c){function d(b,d,e,f){if(e){b.markdownPageModel={src:e};var g=e+".map";f&&a.getMdContent(g).then(function(a){if(a){var d=a.data;for(var e in d)if(d.hasOwnProperty(e)){var f=d[e];b.markdownPageModel.href=c.getRemoteUrl(f.remote,f.startLine)}}},function(a){})}}var e='<div><a ng-if="markdownPageModel.href" ng-href="{{markdownPageModel.href}}" class="btn pull-right mobile-hide"><!--<span class="glyphicon glyphicon-edit">&nbsp;</span>-->Improve this Doc</a><markdown src="markdownPageModel.src"></markdown></div>';return{restrict:"E",replace:!0,template:e,priority:100,link:function(a,b,c){if(c.src){var e="true"===c.getMap,f=a;a.$watch(c.src,function(a,c){void 0!==a&&d(f,b,a,e)})}}}}angular.module("docascode.directives").directive("markdown",["contentService","markdownService","urlService",a]).directive("markdownContent",["contentService","markdownService","urlService",b])}(),function(){"use strict";function a(){this.cleanArray=function(a){for(var b=[],c=0;c<a.length;c++)a[c]&&b.push(a[c]);return b},this.substringLine=function(a,b,c){if(!a||1>c||b>c)return"";var d=a.split("\n"),e=d.length;b=1>=b?1:b,c=c>=e?e:c;for(var f="",g=b-1;c>g;g++)f+=d[g]+"\n";return f},this.escapeRegExp=function(a){return a.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g,"\\$&")}}angular.module("docascode.util",[]).service("docUtility",a)}(),angular.module("versionsData",[]).value("NG_VERSION",{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}).value("NG_VERSIONS",[{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}]),angular.module("versions",[]).controller("DocsVersionsCtrl",["$scope","$location","$window","NG_VERSIONS",function(a,b,c,d){"use strict";a.docs_version=d[0],a.docs_versions=d;for(var e=0,f=0/0;e<d.length;e++){var g=d[e];f<=g.minor||(g.isLatest=!0,f=g.minor)}a.getGroupName=function(a){return a.isLatest?"Latest":"v"+a.major+"."+a.minor+".x"},a.jumpToDocsVersion=function(a){var d=b.path().replace(/\/$/,"");c.location=a.docsUrl+d}}]);