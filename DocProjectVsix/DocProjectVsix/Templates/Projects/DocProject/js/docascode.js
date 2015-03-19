/*! Generated by doc-as-code: docascode 2015-03-19 */
"use strict";function createPlayer(){var a=csplay.play("player","http://dotnetsandbox.azurewebsites.net");return a.editor.setTheme("ace/theme/ambiance"),a.editor.setFontSize(16),$("#run").click(function(){var b=$(this);b.html('<i class="fa fa-refresh fa-fw fa-spin"></i>Run'),b.addClass("disabled"),a.run({complete:function(){b.text("Run"),b.removeClass("disabled")}})}),$("#close").click(function(){tryCode(!1)}),a}function tryCode(a,b){a?("undefined"==typeof player&&(player=createPlayer()),player.editor.setValue(b,-1),player.editor.clearSelection(),player.clearOutput(),angular.element("#console").css("margin-left","50%")):angular.element("#console").css("margin-left","100%"),"undefined"!=typeof player&&player.editor.setReadOnly(!a)}var directive={};directive.runnableExample=["$templateCache","$document",function(a,b){var c=".runnable-example-file",d=(b[0],'<nav class="runnable-example-tabs" ng-if="tabs">  <a ng-class="{active:$index==activeTabIndex}"ng-repeat="tab in tabs track by $index" href="" class="btn"ng-click="setTab($index)">    {{ tab }}  </a></nav>');return{restrict:"C",scope:!0,controller:["$scope",function(a){a.setTab=function(b){var c=a.tabs[b];a.activeTabIndex=b,a.$broadcast("tabChange",b,c)}}],compile:function(a){return a.html(d+a.html()),function(a,b){{var d=b[0],e=d.querySelectorAll(c),f=[];Date.now()}angular.forEach(e,function(a){f.push(a.getAttribute("name"))}),f.length>0&&(a.tabs=f,a.$on("tabChange",function(a,b){angular.forEach(e,function(a){a.style.display="none"});var c=e[b];c.style.display="block"}),a.setTab(0))}}}}],directive.dropdownToggle=["$document","$location","$window",function(a,b){var c,d=null;return{restrict:"C",link:function(e,f){e.$watch(function(){return b.path()},function(){c&&c()}),f.parent().on("click",function(){c&&c()}),f.on("click",function(b){b.preventDefault(),b.stopPropagation();var e=!1;d&&(e=d===f,c()),e||(f.parent().addClass("open"),d=f,c=function(b){b&&b.preventDefault(),b&&b.stopPropagation(),a.off("click",c),f.parent().removeClass("open"),c=null,d=null},a.on("click",c))})}}}],directive.syntax=function(){return{restrict:"A",link:function(a,b,c){function d(a,b,c,d){return'<a href="'+c+'" class="btn syntax-'+a+'" target="_blank" rel="nofollow"><span class="'+d+'"></span> '+b+"</a>"}var e="",f={github:{text:"View on Github",key:"syntaxGithub",icon:"icon-github"},plunkr:{text:"View on Plunkr",key:"syntaxPlunkr",icon:"icon-arrow-down"},jsfiddle:{text:"View on JSFiddle",key:"syntaxFiddle",icon:"icon-cloud"}};for(var g in f){var h=f[g],i=c[h.key];i&&(e+=d(g,h.text,i,h.icon))}var j=document.createElement("nav");j.className="syntax-links",j.innerHTML=e;var k=b[0],l=k.parentNode;l.insertBefore(j,k)}}},directive.tabbable=function(){return{restrict:"C",compile:function(a){var b=angular.element('<ul class="nav nav-tabs"></ul>'),c=angular.element('<div class="tab-content"></div>');c.append(a.contents()),a.append(b).append(c)},controller:["$scope","$element",function(a,b){var c,d=b.contents().eq(0),e=b.controller("ngModel")||{},f=[];e.$render=function(){var a=this.$viewValue;if((c?c.value!=a:a)&&(c&&(c.paneElement.removeClass("active"),c.tabElement.removeClass("active"),c=null),a)){for(var b=0,d=f.length;d>b;b++)if(a==f[b].value){c=f[b];break}c&&(c.paneElement.addClass("active"),c.tabElement.addClass("active"))}},this.addPane=function(b,g){function h(){k.title=g.title,k.value=g.value||g.title,e.$setViewValue||e.$viewValue&&k!=c||(e.$viewValue=k.value),e.$render()}var i=angular.element("<li><a href></a></li>"),j=i.find("a"),k={paneElement:b,paneAttrs:g,tabElement:i};return f.push(k),g.$observe("value",h)(),g.$observe("title",function(){h(),j.text(k.title)})(),d.append(i),i.on("click",function(b){b.preventDefault(),b.stopPropagation(),e.$setViewValue?a.$apply(function(){e.$setViewValue(k.value),e.$render()}):(e.$viewValue=k.value,e.$render())}),function(){k.tabElement.remove();for(var a=0,b=f.length;b>a;a++)k==f[a]&&f.splice(a,1)}}}]}},directive.table=function(){return{restrict:"E",link:function(a,b,c){c["class"]||b.addClass("table table-bordered table-striped code-table")}}};var popoverElement=function(){var a={init:function(){this.element=angular.element('<div class="popover popover-incode top"><div class="arrow"></div><div class="popover-inner"><div class="popover-title"><code></code></div><div class="popover-content"></div></div></div>'),this.node=this.element[0],this.element.css({display:"block",position:"absolute"}),angular.element(document.body).append(this.element);var a=this.element.children()[1];this.titleElement=angular.element(a.childNodes[0].firstChild),this.contentElement=angular.element(a.childNodes[1]),this.element.on("click",function(a){a.preventDefault(),a.stopPropagation()});var b=this;angular.element(document.body).on("click",function(){b.visible()&&b.hide()})},show:function(a,b){this.element.addClass("visible"),this.position(a||0,b||0)},hide:function(){this.element.removeClass("visible"),this.position(-9999,-9999)},visible:function(){return this.position().y>=0},isSituatedAt:function(a){return this.besideElement?a[0]==this.besideElement[0]:!1},title:function(a){return this.titleElement.html(a)},content:function(a){return a&&a.length>0&&(a=marked(a)),this.contentElement.html(a)},positionArrow:function(a){this.node.className="popover "+a},positionAway:function(){this.besideElement=null,this.hide()},positionBeside:function(a){this.besideElement=a;var b=a[0],c=b.offsetLeft,d=b.offsetTop;c-=30,d-=this.node.offsetHeight+10,this.show(c,d)},position:function(a,b){return null==a||null==b?{x:this.node.offsetLeft,y:this.node.offsetTop}:(this.element.css("left",a+"px"),void this.element.css("top",b+"px"))}};return a.init(),a.hide(),a};directive.popover=["popoverElement",function(a){return{restrict:"A",priority:500,link:function(b,c,d){c.on("click",function(b){b.preventDefault(),b.stopPropagation(),a.isSituatedAt(c)&&a.visible()?(a.title(""),a.content(""),a.positionAway()):(a.title(d.title),a.content(d.content),a.positionBeside(c))})}}}],directive.tabPane=function(){return{require:"^tabbable",restrict:"C",link:function(a,b,c,d){b.on("$remove",d.addPane(b,c))}}},directive.foldout=["$http","$animate","$window",function(a,b,c){return{restrict:"A",priority:500,link:function(d,e,f){var g,h,i=f.url;/\/build\//.test(c.location.href)&&(i="/build/docs"+i),e.on("click",function(){d.$apply(function(){if(g)g.hasClass("ng-hide")?b.removeClass(g,"ng-hide"):b.addClass(g,"ng-hide");else{if(h)return;h=!0;var c=e.parent();g=angular.element('<div class="foldout">loading...</div>'),b.enter(g,null,c),a.get(i,{cache:!0}).success(function(a){h=!1,a='<div class="foldout-inner"><div calss="foldout-arrow"></div>'+a+"</div>",g.html(a),"block"==g.css("display")&&(g.css("display","none"),b.addClass(g,"ng-hide"))})}})})}}}],angular.module("bootstrap",[]).directive(directive).factory("popoverElement",popoverElement).run(function(){marked.setOptions({gfm:!0,tables:!0})}),angular.module("pagesData",[]).value("NG_PAGES",{}),angular.module("itemTypes",[]).value("NG_ITEMTYPES",{"class":{Property:{name:"Property",description:"Properties",show:!1},Method:{name:"Method",description:"Methods",show:!1},Constructor:{name:"Constructor",description:"Constructors",show:!1},Field:{name:"Field",description:"Fields",show:!1}},namespace:{Class:{name:"Class",description:"Classes",show:!1},Enum:{name:"Enum",description:"Enums",show:!1},Delegate:{name:"Delegate",description:"Delegates",show:!1},Interface:{name:"Interface",description:"Interfaces",show:!1},Struct:{name:"Struct",description:"Structs",show:!1}}}),angular.module("versionsData",[]).value("NG_VERSION",{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}).value("NG_VERSIONS",[{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}]),window.csplay={play:function(a,b){"undefined"==typeof b&&(b=""),"/"!=b.substr(-1,1)&&(b+="/"),"string"==typeof a&&(a=document.getElementById(a));var c=document.createElement("div"),d=document.createElement("div"),e=document.createElement("div");c.innerHTML=a.innerHTML,c.className="csplay_editor",d.className="csplay_splitbar",e.className="csplay_output";var f=a.cloneNode(!1);a.parentNode.replaceChild(f,a),a=f,a.appendChild(c),a.appendChild(d),a.appendChild(e);var g=ace.edit(c);g.getSession().setMode("ace/mode/csharp"),a=$(a),c=$(c),d=$(d),e=$(e);var h=!1;return d.mousedown(function(){h=!0}),a.mouseup(function(){h=!1}).mousemove(function(a){if(h){var b=a.pageY-c.offset().top,f=c.outerHeight(!0)+e.outerHeight(!0)-b,i=d.outerHeight(!0);b>0&&f>0&&(c.css("height",b),e.css("height","calc(100% - "+(b+i)+"px)"),g.resize()),a.preventDefault()}}),{run:function(a){$.ajax({url:b+"run",type:"POST",data:g.getValue(),contentType:"text/plain",success:function(b,c,d){e.html(b).removeClass("error"),"function"==typeof a.success&&a.success(b,c,d)},error:function(b,c,d){"string"==typeof b.responseJSON.error_message?e.text(b.responseJSON.error_message).addClass("error"):e.text(b.responseText).addClass("error"),"function"==typeof a.error&&a.error(b,c,d)},complete:function(b,c){"function"==typeof a.complete&&a.complete(b,c)}})},clearOutput:function(){e.html("")},editor:g}}};var player;angular.module("docsApp",["ngRoute","ngCookies","ngSanitize","ngAnimate","DocsController","versionsData","pagesData","itemTypes","directives","errors","examples","tutorials","versions","bootstrap","ui.bootstrap.dropdown"]),angular.module("directives",[]).directive("markdown",function(){var a=function(){marked.setOptions({gfm:!0,pedantic:!1,sanitize:!0});var a=function(a){return void 0==a?"":marked(a)};return{toHtml:a}}();return{restrict:"E",link:function(b,c,d){b.$watch(d.ngModel,function(b){var d=b,e=a.toHtml(d);c.html(e),angular.forEach(c.find("code"),function(a){hljs.highlightBlock(a),a=a.parentNode;var b=document.createElement("div");b.className="codewrapper",b.innerHTML='<div class="trydiv"><span class="tryspan">Try this code</span></div>',b.childNodes[0].childNodes[0].onclick=function(){tryCode(!0,a.innerText)},a.parentNode.replaceChild(b,a),b.appendChild(a)})})}}}).directive("declaration",function(){return{restrict:"E",link:function(a,b,c){a.$watch(c.ngModel,function(a){b.html('<code class="lang-csharp"></code>');var c=b.children("code");c.text(a),hljs.highlightBlock(c[0])})}}}).directive("backToTop",["$anchorScroll","$location",function(a,b){return function(c,d){d.on("click",function(){b.hash(""),c.$apply(a)})}}]).directive("code",function(){return{restrict:"E",terminal:!0,compile:function(a){var b=a.hasClass("linenum"),c=/lang-(\S+)/.exec(a[0].className),d=c&&c[1],e=a.html();a.html(window.prettyPrintOne(e,d,b))}}}).directive("scrollYOffsetElement",["$anchorScroll",function(a){return function(b,c){a.yOffset=c}}]),angular.module("DocsController",[]).controller("DocsController",["$scope","$http","$q","$rootScope","$location","$window","$cookies","openPlunkr","NG_PAGES","NG_VERSION","NG_ITEMTYPES",function(a,b,c,d,e,f,g,h,i,j,k){function l(a,b){if(!(a&&a.remote&&a.remote.repo))return"#";var c=a.remote.repo;".git"==c.substr(-4)&&(c=c.substr(0,c.length-4));var d=b?b:a.startLine;return c.match(/https:\/\/.*\.visualstudio\.com\/.*/g)?c+"#path=/"+a.path+"&line="+d:c.match(/https:\/\/.*github\.com\/.*/g)?c+"/blob/"+a.remote.branch+"/"+a.path+"/#L"+d:void 0}function m(a,d,e){var f=c.defer(),g={method:"GET",url:a,headers:{"Content-Type":"text/plain"}};return b.get(g.url,g).success(function(a){d&&d(a),f.resolve()}).error(function(a){e&&e(a),f.reject()}),f.promise}a.openPlunkr=h,a.docsVersion=j.isSnapshot?"snapshot":j.version,a.navClass=function(a){return{active:a.href&&this.currentPage,current:this.currentPage===a.href,"nav-index-section":"section"===a.type}},a.getNumber=function(a){return new Array(a+1)},a.GetDetail=function(a){console.log(a.target);var b=a.target.nextElementSibling.style.display;a.target.nextElementSibling.style.display="block"==b?"none":"block"},a.ViewSource=function(){return l(this.model.source,this.model.source.startLine+1)},a.ImproveThisDoc=function(){return a.partialModel.mdContent},a.$on("$includeContentLoaded",function(){a.currentPage?a.currentPage.path:e.path()});m("toc.yaml",function(b){a.currentArea=jsyaml.load(b)}),m("md.yaml",function(b){a.mdIndex=jsyaml.load(b)});a.$watch(function(){return e.path()},function(b){b=b.replace(/^\/?(.+?)(\/index)?\/?$/,"$1");var c=a.currentPage=b;if(c){var d=(m(b+".yaml",function(b){if(a.partialModel=jsyaml.load(b),a.title=a.partialModel.id,"namespace"==a.partialModel.type.toLowerCase()){a.itemtypes=k.namespace;for(var c in a.partialModel.items){var d=a.itemtypes[a.partialModel.items[c].type];d&&(d.show=!0)}a.partialPath="template/namespace.tmpl"}else{a.itemtypes=k["class"];for(var c in a.itemtypes)a.itemtypes[c].show=!1;for(var c in a.partialModel.items){var d=a.itemtypes[a.partialModel.items[c].type];d&&(d.show=!0)}a.partialPath="template/class.tmpl"}}),c.split("/")),e=a.breadcrumb=[],f="";angular.forEach(d,function(a){f+=a,e.push({name:a,url:f}),f+="/"})}else a.currentArea="api",a.breadcrumb=[],a.partialPath="Error404.html"}),a.$watch(function(){return a.partialModel},function(){if(a.mdIndex&&a.partialModel){var b=a.mdIndex[a.partialModel.id];if(b&&b.href){a.partialModel.mdHref=l(b);{m(b.href,function(c){var d=c.substr(b.startLine,b.endLine-b.startLine+1);a.partialModel.mdContent=d})}}}}),a.versionNumber=angular.version.full,a.version=angular.version.full+"  "+angular.version.codeName,a.loading=0;var n=/^(\/|\/index[^\.]*.html)$/;(!e.path()||n.test(e.path()))&&e.path("/api").replace()}]),angular.module("errors",["ngSanitize"]).filter("errorLink",["$sanitize",function(a){var b=/((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}<>]/g,c=/^mailto:/,d=/:\d+:\d+$/,e=function(a,b){return a.length>b?a.substr(0,b-3)+"...":a};return function(f,g){var h=g?' target="'+g+'"':"";return f?a(f.replace(b,function(a){return d.test(a)?a:(/^((ftp|https?):\/\/|mailto:)/.test(a)||(a="mailto:"+a),"<a"+h+' href="'+a+'">'+e(a.replace(c,""),60)+"</a>")})):f}}]).directive("errorDisplay",["$location","errorLinkFilter",function(a,b){var c=function(a){var b=arguments;return a.replace(/\{\d+\}/g,function(a){var c=+a.slice(1,-1);return c+1>=b.length?a:b[c+1]})};return{link:function(d,e,f){var g,h=a.search(),i=[f.errorDisplay];for(g=0;angular.isDefined(h["p"+g]);g++)i.push(h["p"+g]);e.html(b(c.apply(null,i),"_blank"))}}}]),angular.module("examples",[]).factory("formPostData",["$document",function(a){return function(b,c,d){var e=c?"_blank":"_self",f=angular.element('<form style="display: none;" method="post" action="'+b+'" target="'+e+'"></form>');angular.forEach(d,function(a,b){var c=angular.element('<input type="hidden" name="'+b+'">');c.attr("value",a),f.append(c)}),a.find("body").append(f),f[0].submit(),f.remove()}}]).factory("openPlunkr",["formPostData","$http","$q",function(a,b,c){return function(d,e){var f="AngularJS Example",g=e.ctrlKey||e.metaKey;b.get(d+"/manifest.json").then(function(a){return a.data}).then(function(a){var e=[],g=a.name.split("-");return g.unshift("AngularJS"),angular.forEach(g,function(a,b){g[b]=a.charAt(0).toUpperCase()+a.substr(1)}),f=g.join(" - "),angular.forEach(a.files,function(a){e.push(b.get(d+"/"+a,{transformResponse:[]}).then(function(b){return"index-production.html"===a&&(a="index.html"),{name:a,content:b.data}}))}),c.all(e)}).then(function(b){var c={};angular.forEach(b,function(a){c["files["+a.name+"]"]=a.content}),c["tags[0]"]="angularjs",c["tags[1]"]="example",c["private"]=!0,c.description=f,a("http://plnkr.co/edit/?p=preview",g,c)})}}]),angular.module("tutorials",[]).directive("docTutorialNav",function(){var a=["","step_00","step_01","step_02","step_03","step_04","step_05","step_06","step_07","step_08","step_09","step_10","step_11","step_12","the_end"];return{scope:{},template:'<a ng-href="tutorial/{{prev}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-step-backward"></i> Previous</li></a>\n<a ng-href="http://angular.github.io/angular-phonecat/step-{{seq}}/app"><li class="btn btn-primary"><i class="glyphicon glyphicon-play"></i> Live Demo</li></a>\n<a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{diffLo}}...step-{{diffHi}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-search"></i> Code Diff</li></a>\n<a ng-href="tutorial/{{next}}"><li class="btn btn-primary">Next <i class="glyphicon glyphicon-step-forward"></i></li></a>',link:function(b,c,d){var e=1*d.docTutorialNav;b.seq=e,b.prev=a[e],b.next=a[2+e],b.diffLo=e?e-1:"0~1",b.diffHi=e,c.addClass("btn-group"),c.addClass("tutorial-nav")}}}).directive("docTutorialReset",function(){return{scope:{step:"@docTutorialReset"},template:'<p><a href="" ng-click="show=!show;$event.stopPropagation()">Workspace Reset Instructions  ➤</a></p>\n<div class="alert alert-info" ng-show="show">\n  <p>Reset the workspace to step {{step}}.</p>  <p><pre>git checkout -f step-{{step}}</pre></p>\n  <p>Refresh your browser or check out this step online: <a href="http://angular.github.io/angular-phonecat/step-{{step}}/app">Step {{step}} Live Demo</a>.</p>\n</div>\n<p>The most important changes are listed below. You can see the full diff on <a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{step ? (step - 1): \'0~1\'}}...step-{{step}}">GitHub</a>\n</p>'}}),angular.module("versions",[]).controller("DocsVersionsCtrl",["$scope","$location","$window","NG_VERSIONS",function(a,b,c,d){a.docs_version=d[0],a.docs_versions=d;for(var e=0,f=0/0;e<d.length;e++){var g=d[e];f<=g.minor||(g.isLatest=!0,f=g.minor)}a.getGroupName=function(a){return a.isLatest?"Latest":"v"+a.major+"."+a.minor+".x"},a.jumpToDocsVersion=function(a){var d=b.path().replace(/\/$/,"");c.location=a.docsUrl+d}}]);