/*! Generated by doc-as-code: docascode 2015-03-26 */
"use strict";function docServiceFunction(a,b){this.tocClassApi=function(a){return{active:a.href&&this.currentPage,current:this.currentPage===a.href,"nav-index-section":"section"===a.type}},this.navClassApi=function(a){return{active:a.href&&this.currentPage,current:this.currentPage.indexOf(a.href)>-1}},this.getRemoteUrl=function(a,b){if(!(a&&a.remote&&a.remote.repo))return"#";var c=a.remote.repo;".git"==c.substr(-4)&&(c=c.substr(0,c.length-4));var d=b?b:a.startLine;return c.match(/https:\/\/.*\.visualstudio\.com\/.*/g)?c+"#path=/"+a.path+"&line="+d:c.match(/https:\/\/.*github\.com\/.*/g)?c+"/blob/"+a.remote.branch+"/"+a.path+"/#L"+d:void 0},this.getPathInfo=function(a){if(!a)return void 0;var b=a.indexOf("!");return 0>b?/(\.yaml$)|(\.md$)/g.test(a)?{contentPath:a}:{tocPath:a,tocFilePath:a+"/toc.yaml"}:{tocPath:a.substring(0,b),tocFilePath:a.substring(0,b)+"/toc.yaml",contentPath:a.substring(b+1,a.length)}},this.asyncFetchIndex=function(c,d,e){var f=a.defer(),g={method:"GET",url:c,headers:{"Content-Type":"text/plain"}};return b.get(g.url,g).success(function(a){d&&d(a),f.resolve()}).error(function(a){e&&e(a),f.reject()}),f.promise},this.getDefaultItem=function(a,b){return b&&a&&a.length>0?b(a[0]):void 0}}function createPlayer(){var a=csplay.play("player","http://dotnetsandbox.azurewebsites.net");return a.editor.setTheme("ace/theme/ambiance"),a.editor.setFontSize(16),$("#run").click(function(){var b=$(this);b.html('<i class="fa fa-refresh fa-fw fa-spin"></i>Run'),b.addClass("disabled"),a.run({complete:function(){b.text("Run"),b.removeClass("disabled")}})}),$("#close").click(function(){tryCode(!1)}),a}function tryCode(a,b){a?("undefined"==typeof player&&(player=createPlayer()),player.editor.setValue(b,-1),player.editor.clearSelection(),player.clearOutput(),angular.element("#console").css("margin-left","50%")):angular.element("#console").css("margin-left","100%"),"undefined"!=typeof player&&player.editor.setReadOnly(!a)}function cleanArray(a){for(var b=new Array,c=0;c<a.length;c++)a[c]&&b.push(a[c]);return b}function getTocFromMd(a){marked(a)}(function(){function a(a){this.tokens=[],this.tokens.links={},this.options=a||j.defaults,this.rules=k.normal,this.options.gfm&&(this.rules=this.options.tables?k.tables:k.gfm)}function b(a,b){if(this.options=b||j.defaults,this.links=a,this.rules=l.normal,this.renderer=this.options.renderer||new c,this.renderer.options=this.options,!this.links)throw new Error("Tokens array requires a `links` property.");this.options.gfm?this.rules=this.options.breaks?l.breaks:l.gfm:this.options.pedantic&&(this.rules=l.pedantic)}function c(a){this.options=a||{}}function d(a){this.tokens=[],this.token=null,this.options=a||j.defaults,this.options.renderer=this.options.renderer||new c,this.renderer=this.options.renderer,this.renderer.options=this.options}function e(a,b){return a.replace(b?/&/g:/&(?!#?\w+;)/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;").replace(/'/g,"&#39;")}function f(a){return a.replace(/&([#\w]+);/g,function(a,b){return b=b.toLowerCase(),"colon"===b?":":"#"===b.charAt(0)?String.fromCharCode("x"===b.charAt(1)?parseInt(b.substring(2),16):+b.substring(1)):""})}function g(a,b){return a=a.source,b=b||"",function c(d,e){return d?(e=e.source||e,e=e.replace(/(^|[^\[])\^/g,"$1"),a=a.replace(d,e),c):new RegExp(a,b)}}function h(){}function i(a){for(var b,c,d=1;d<arguments.length;d++){b=arguments[d];for(c in b)Object.prototype.hasOwnProperty.call(b,c)&&(a[c]=b[c])}return a}function j(b,c,f){if(f||"function"==typeof c){f||(f=c,c=null),c=i({},j.defaults,c||{});var g,h,k=c.highlight,l=0;try{g=a.lex(b,c)}catch(m){return f(m)}h=g.length;var n=function(a){if(a)return c.highlight=k,f(a);var b;try{b=d.parse(g,c)}catch(e){a=e}return c.highlight=k,a?f(a):f(null,b)};if(!k||k.length<3)return n();if(delete c.highlight,!h)return n();for(;l<g.length;l++)!function(a){return"code"!==a.type?--h||n():k(a.text,a.lang,function(b,c){return b?n(b):null==c||c===a.text?--h||n():(a.text=c,a.escaped=!0,void(--h||n()))})}(g[l])}else try{return c&&(c=i({},j.defaults,c)),d.parse(a.lex(b,c),c)}catch(m){if(m.message+="\nPlease report this to https://github.com/chjj/marked.",(c||j.defaults).silent)return"<p>An error occured:</p><pre>"+e(m.message+"",!0)+"</pre>";throw m}}var k={newline:/^\n+/,code:/^( {4}[^\n]+\n*)+/,fences:h,hr:/^( *[-*_]){3,} *(?:\n+|$)/,heading:/^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)/,nptable:h,lheading:/^([^\n]+)\n *(=|-){2,} *(?:\n+|$)/,blockquote:/^( *>[^\n]+(\n(?!def)[^\n]+)*\n*)+/,list:/^( *)(bull) [\s\S]+?(?:hr|def|\n{2,}(?! )(?!\1bull )\n*|\s*$)/,html:/^ *(?:comment *(?:\n|\s*$)|closed *(?:\n{2,}|\s*$)|closing *(?:\n{2,}|\s*$))/,def:/^ *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +["(]([^\n]+)[")])? *(?:\n+|$)/,table:h,paragraph:/^((?:[^\n]+\n?(?!hr|heading|lheading|blockquote|tag|def))+)\n*/,text:/^[^\n]+/};k.bullet=/(?:[*+-]|\d+\.)/,k.item=/^( *)(bull) [^\n]*(?:\n(?!\1bull )[^\n]*)*/,k.item=g(k.item,"gm")(/bull/g,k.bullet)(),k.list=g(k.list)(/bull/g,k.bullet)("hr","\\n+(?=\\1?(?:[-*_] *){3,}(?:\\n+|$))")("def","\\n+(?="+k.def.source+")")(),k.blockquote=g(k.blockquote)("def",k.def)(),k._tag="(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\\b)\\w+(?!:/|[^\\w\\s@]*@)\\b",k.html=g(k.html)("comment",/<!--[\s\S]*?-->/)("closed",/<(tag)[\s\S]+?<\/\1>/)("closing",/<tag(?:"[^"]*"|'[^']*'|[^'">])*?>/)(/tag/g,k._tag)(),k.paragraph=g(k.paragraph)("hr",k.hr)("heading",k.heading)("lheading",k.lheading)("blockquote",k.blockquote)("tag","<"+k._tag)("def",k.def)(),k.normal=i({},k),k.gfm=i({},k.normal,{fences:/^ *(`{3,}|~{3,}) *(\S+)? *\n([\s\S]+?)\s*\1 *(?:\n+|$)/,paragraph:/^/}),k.gfm.paragraph=g(k.paragraph)("(?!","(?!"+k.gfm.fences.source.replace("\\1","\\2")+"|"+k.list.source.replace("\\1","\\3")+"|")(),k.tables=i({},k.gfm,{nptable:/^ *(\S.*\|.*)\n *([-:]+ *\|[-| :]*)\n((?:.*\|.*(?:\n|$))*)\n*/,table:/^ *\|(.+)\n *\|( *[-:]+[-| :]*)\n((?: *\|.*(?:\n|$))*)\n*/}),a.rules=k,a.lex=function(b,c){var d=new a(c);return d.lex(b)},a.prototype.lex=function(a){return a=a.replace(/\r\n|\r/g,"\n").replace(/\t/g,"    ").replace(/\u00a0/g," ").replace(/\u2424/g,"\n"),this.token(a,!0)},a.prototype.token=function(a,b,c){for(var d,e,f,g,h,i,j,l,m,a=a.replace(/^ +$/gm,"");a;)if((f=this.rules.newline.exec(a))&&(a=a.substring(f[0].length),f[0].length>1&&this.tokens.push({type:"space"})),f=this.rules.code.exec(a))a=a.substring(f[0].length),f=f[0].replace(/^ {4}/gm,""),this.tokens.push({type:"code",text:this.options.pedantic?f:f.replace(/\n+$/,"")});else if(f=this.rules.fences.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"code",lang:f[2],text:f[3]});else if(f=this.rules.heading.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"heading",depth:f[1].length,text:f[2]});else if(b&&(f=this.rules.nptable.exec(a))){for(a=a.substring(f[0].length),i={type:"table",header:f[1].replace(/^ *| *\| *$/g,"").split(/ *\| */),align:f[2].replace(/^ *|\| *$/g,"").split(/ *\| */),cells:f[3].replace(/\n$/,"").split("\n")},l=0;l<i.align.length;l++)i.align[l]=/^ *-+: *$/.test(i.align[l])?"right":/^ *:-+: *$/.test(i.align[l])?"center":/^ *:-+ *$/.test(i.align[l])?"left":null;for(l=0;l<i.cells.length;l++)i.cells[l]=i.cells[l].split(/ *\| */);this.tokens.push(i)}else if(f=this.rules.lheading.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"heading",depth:"="===f[2]?1:2,text:f[1]});else if(f=this.rules.hr.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"hr"});else if(f=this.rules.blockquote.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"blockquote_start"}),f=f[0].replace(/^ *> ?/gm,""),this.token(f,b,!0),this.tokens.push({type:"blockquote_end"});else if(f=this.rules.list.exec(a)){for(a=a.substring(f[0].length),g=f[2],this.tokens.push({type:"list_start",ordered:g.length>1}),f=f[0].match(this.rules.item),d=!1,m=f.length,l=0;m>l;l++)i=f[l],j=i.length,i=i.replace(/^ *([*+-]|\d+\.) +/,""),~i.indexOf("\n ")&&(j-=i.length,i=this.options.pedantic?i.replace(/^ {1,4}/gm,""):i.replace(new RegExp("^ {1,"+j+"}","gm"),"")),this.options.smartLists&&l!==m-1&&(h=k.bullet.exec(f[l+1])[0],g===h||g.length>1&&h.length>1||(a=f.slice(l+1).join("\n")+a,l=m-1)),e=d||/\n\n(?!\s*$)/.test(i),l!==m-1&&(d="\n"===i.charAt(i.length-1),e||(e=d)),this.tokens.push({type:e?"loose_item_start":"list_item_start"}),this.token(i,!1,c),this.tokens.push({type:"list_item_end"});this.tokens.push({type:"list_end"})}else if(f=this.rules.html.exec(a))a=a.substring(f[0].length),this.tokens.push({type:this.options.sanitize?"paragraph":"html",pre:"pre"===f[1]||"script"===f[1]||"style"===f[1],text:f[0]});else if(!c&&b&&(f=this.rules.def.exec(a)))a=a.substring(f[0].length),this.tokens.links[f[1].toLowerCase()]={href:f[2],title:f[3]};else if(b&&(f=this.rules.table.exec(a))){for(a=a.substring(f[0].length),i={type:"table",header:f[1].replace(/^ *| *\| *$/g,"").split(/ *\| */),align:f[2].replace(/^ *|\| *$/g,"").split(/ *\| */),cells:f[3].replace(/(?: *\| *)?\n$/,"").split("\n")},l=0;l<i.align.length;l++)i.align[l]=/^ *-+: *$/.test(i.align[l])?"right":/^ *:-+: *$/.test(i.align[l])?"center":/^ *:-+ *$/.test(i.align[l])?"left":null;for(l=0;l<i.cells.length;l++)i.cells[l]=i.cells[l].replace(/^ *\| *| *\| *$/g,"").split(/ *\| */);this.tokens.push(i)}else if(b&&(f=this.rules.paragraph.exec(a)))a=a.substring(f[0].length),this.tokens.push({type:"paragraph",text:"\n"===f[1].charAt(f[1].length-1)?f[1].slice(0,-1):f[1]});else if(f=this.rules.text.exec(a))a=a.substring(f[0].length),this.tokens.push({type:"text",text:f[0]});else if(a)throw new Error("Infinite loop on byte: "+a.charCodeAt(0));return this.tokens};var l={escape:/^\\([\\`*{}\[\]()#+\-.!_>])/,autolink:/^<([^ >]+(@|:\/)[^ >]+)>/,url:h,tag:/^<!--[\s\S]*?-->|^<\/?\w+(?:"[^"]*"|'[^']*'|[^'">])*?>/,link:/^!?\[(inside)\]\(href\)/,reflink:/^!?\[(inside)\]\s*\[([^\]]*)\]/,nolink:/^!?\[((?:\[[^\]]*\]|[^\[\]])*)\]/,strong:/^__([\s\S]+?)__(?!_)|^\*\*([\s\S]+?)\*\*(?!\*)/,em:/^\b_((?:__|[\s\S])+?)_\b|^\*((?:\*\*|[\s\S])+?)\*(?!\*)/,code:/^(`+)\s*([\s\S]*?[^`])\s*\1(?!`)/,br:/^ {2,}\n(?!\s*$)/,del:h,text:/^[\s\S]+?(?=[\\<!\[_*`]| {2,}\n|$)/};l._inside=/(?:\[[^\]]*\]|[^\[\]]|\](?=[^\[]*\]))*/,l._href=/\s*<?([\s\S]*?)>?(?:\s+['"]([\s\S]*?)['"])?\s*/,l.link=g(l.link)("inside",l._inside)("href",l._href)(),l.reflink=g(l.reflink)("inside",l._inside)(),l.normal=i({},l),l.pedantic=i({},l.normal,{strong:/^__(?=\S)([\s\S]*?\S)__(?!_)|^\*\*(?=\S)([\s\S]*?\S)\*\*(?!\*)/,em:/^_(?=\S)([\s\S]*?\S)_(?!_)|^\*(?=\S)([\s\S]*?\S)\*(?!\*)/}),l.gfm=i({},l.normal,{escape:g(l.escape)("])","~|])")(),url:/^(https?:\/\/[^\s<]+[^<.,:;"')\]\s])/,del:/^~~(?=\S)([\s\S]*?\S)~~/,text:g(l.text)("]|","~]|")("|","|https?://|")()}),l.breaks=i({},l.gfm,{br:g(l.br)("{2,}","*")(),text:g(l.gfm.text)("{2,}","*")()}),b.rules=l,b.output=function(a,c,d){var e=new b(c,d);return e.output(a)},b.prototype.output=function(a){for(var b,c,d,f,g="";a;)if(f=this.rules.escape.exec(a))a=a.substring(f[0].length),g+=f[1];else if(f=this.rules.autolink.exec(a))a=a.substring(f[0].length),"@"===f[2]?(c=this.mangle(":"===f[1].charAt(6)?f[1].substring(7):f[1]),d=this.mangle("mailto:")+c):(c=e(f[1]),d=c),g+=this.renderer.link(d,null,c);else if(this.inLink||!(f=this.rules.url.exec(a))){if(f=this.rules.tag.exec(a))!this.inLink&&/^<a /i.test(f[0])?this.inLink=!0:this.inLink&&/^<\/a>/i.test(f[0])&&(this.inLink=!1),a=a.substring(f[0].length),g+=this.options.sanitize?e(f[0]):f[0];else if(f=this.rules.link.exec(a))a=a.substring(f[0].length),this.inLink=!0,g+=this.outputLink(f,{href:f[2],title:f[3]}),this.inLink=!1;else if((f=this.rules.reflink.exec(a))||(f=this.rules.nolink.exec(a))){if(a=a.substring(f[0].length),b=(f[2]||f[1]).replace(/\s+/g," "),b=this.links[b.toLowerCase()],!b||!b.href){g+=f[0].charAt(0),a=f[0].substring(1)+a;continue}this.inLink=!0,g+=this.outputLink(f,b),this.inLink=!1}else if(f=this.rules.strong.exec(a))a=a.substring(f[0].length),g+=this.renderer.strong(this.output(f[2]||f[1]));else if(f=this.rules.em.exec(a))a=a.substring(f[0].length),g+=this.renderer.em(this.output(f[2]||f[1]));else if(f=this.rules.code.exec(a))a=a.substring(f[0].length),g+=this.renderer.codespan(e(f[2],!0));else if(f=this.rules.br.exec(a))a=a.substring(f[0].length),g+=this.renderer.br();else if(f=this.rules.del.exec(a))a=a.substring(f[0].length),g+=this.renderer.del(this.output(f[1]));else if(f=this.rules.text.exec(a))a=a.substring(f[0].length),g+=e(this.smartypants(f[0]));else if(a)throw new Error("Infinite loop on byte: "+a.charCodeAt(0))}else a=a.substring(f[0].length),c=e(f[1]),d=c,g+=this.renderer.link(d,null,c);return g},b.prototype.outputLink=function(a,b){var c=e(b.href),d=b.title?e(b.title):null;return"!"!==a[0].charAt(0)?this.renderer.link(c,d,this.output(a[1])):this.renderer.image(c,d,e(a[1]))},b.prototype.smartypants=function(a){return this.options.smartypants?a.replace(/--/g,"—").replace(/(^|[-\u2014/(\[{"\s])'/g,"$1‘").replace(/'/g,"’").replace(/(^|[-\u2014/(\[{\u2018\s])"/g,"$1“").replace(/"/g,"”").replace(/\.{3}/g,"…"):a},b.prototype.mangle=function(a){for(var b,c="",d=a.length,e=0;d>e;e++)b=a.charCodeAt(e),Math.random()>.5&&(b="x"+b.toString(16)),c+="&#"+b+";";return c},c.prototype.code=function(a,b,c){if(this.options.highlight){var d=this.options.highlight(a,b);null!=d&&d!==a&&(c=!0,a=d)}return b?'<pre><code class="'+this.options.langPrefix+e(b,!0)+'">'+(c?a:e(a,!0))+"\n</code></pre>\n":"<pre><code>"+(c?a:e(a,!0))+"\n</code></pre>"},c.prototype.blockquote=function(a){return"<blockquote>\n"+a+"</blockquote>\n"},c.prototype.html=function(a){return a},c.prototype.heading=function(a,b,c){return"<h"+b+' id="'+this.options.headerPrefix+c.toLowerCase().replace(/[^\w]+/g,"-")+'">'+a+"</h"+b+">\n"},c.prototype.hr=function(){return this.options.xhtml?"<hr/>\n":"<hr>\n"},c.prototype.list=function(a,b){var c=b?"ol":"ul";return"<"+c+">\n"+a+"</"+c+">\n"},c.prototype.listitem=function(a){return"<li>"+a+"</li>\n"},c.prototype.paragraph=function(a){return"<p>"+a+"</p>\n"},c.prototype.table=function(a,b){return"<table>\n<thead>\n"+a+"</thead>\n<tbody>\n"+b+"</tbody>\n</table>\n"},c.prototype.tablerow=function(a){return"<tr>\n"+a+"</tr>\n"},c.prototype.tablecell=function(a,b){var c=b.header?"th":"td",d=b.align?"<"+c+' style="text-align:'+b.align+'">':"<"+c+">";return d+a+"</"+c+">\n"},c.prototype.strong=function(a){return"<strong>"+a+"</strong>"},c.prototype.em=function(a){return"<em>"+a+"</em>"},c.prototype.codespan=function(a){return"<code>"+a+"</code>"},c.prototype.br=function(){return this.options.xhtml?"<br/>":"<br>"},c.prototype.del=function(a){return"<del>"+a+"</del>"},c.prototype.link=function(a,b,c){if(this.options.sanitize){try{var d=decodeURIComponent(f(a)).replace(/[^\w:]/g,"").toLowerCase()}catch(e){return""}if(0===d.indexOf("javascript:")||0===d.indexOf("vbscript:"))return""}var g='<a href="'+a+'"';return b&&(g+=' title="'+b+'"'),g+=">"+c+"</a>"},c.prototype.image=function(a,b,c){var d='<img src="'+a+'" alt="'+c+'"';return b&&(d+=' title="'+b+'"'),d+=this.options.xhtml?"/>":">"},d.parse=function(a,b,c){var e=new d(b,c);return e.parse(a)},d.prototype.parse=function(a){this.inline=new b(a.links,this.options,this.renderer),this.tokens=a.reverse();for(var c="";this.next();)c+=this.tok();return c},d.prototype.next=function(){return this.token=this.tokens.pop()},d.prototype.peek=function(){return this.tokens[this.tokens.length-1]||0},d.prototype.parseText=function(){for(var a=this.token.text;"text"===this.peek().type;)a+="\n"+this.next().text;return this.inline.output(a)},d.prototype.tok=function(){switch(this.token.type){case"space":return"";case"hr":return this.renderer.hr();case"heading":return this.renderer.heading(this.inline.output(this.token.text),this.token.depth,this.token.text);case"code":return this.renderer.code(this.token.text,this.token.lang,this.token.escaped);case"table":var a,b,c,d,e,f="",g="";for(c="",a=0;a<this.token.header.length;a++)d={header:!0,align:this.token.align[a]},c+=this.renderer.tablecell(this.inline.output(this.token.header[a]),{header:!0,align:this.token.align[a]});for(f+=this.renderer.tablerow(c),a=0;a<this.token.cells.length;a++){for(b=this.token.cells[a],c="",e=0;e<b.length;e++)c+=this.renderer.tablecell(this.inline.output(b[e]),{header:!1,align:this.token.align[e]});g+=this.renderer.tablerow(c)}return this.renderer.table(f,g);case"blockquote_start":for(var g="";"blockquote_end"!==this.next().type;)g+=this.tok();return this.renderer.blockquote(g);case"list_start":for(var g="",h=this.token.ordered;"list_end"!==this.next().type;)g+=this.tok();return this.renderer.list(g,h);case"list_item_start":for(var g="";"list_item_end"!==this.next().type;)g+="text"===this.token.type?this.parseText():this.tok();return this.renderer.listitem(g);case"loose_item_start":for(var g="";"list_item_end"!==this.next().type;)g+=this.tok();return this.renderer.listitem(g);case"html":var i=this.token.pre||this.options.pedantic?this.token.text:this.inline.output(this.token.text);return this.renderer.html(i);case"paragraph":return this.renderer.paragraph(this.inline.output(this.token.text));case"text":return this.renderer.paragraph(this.parseText())}},h.exec=h,j.options=j.setOptions=function(a){return i(j.defaults,a),j},j.defaults={gfm:!0,tables:!0,breaks:!1,pedantic:!1,sanitize:!1,smartLists:!1,silent:!1,highlight:null,langPrefix:"lang-",smartypants:!1,headerPrefix:"",renderer:new c,xhtml:!1},j.Parser=d,j.parser=d.parse,j.Renderer=c,j.Lexer=a,j.lexer=a.lex,j.InlineLexer=b,j.inlineLexer=b.output,j.parse=j,"undefined"!=typeof module&&"object"==typeof exports?module.exports=j:"function"==typeof define&&define.amd?define(function(){return j}):this.marked=j}).call(function(){return this||("undefined"!=typeof window?window:global)}()),function(){var a=angular.module("hc.marked",[]);a.provider("marked",function(){var a=this;a.setOptions=function(a){this.defaults=a},a.$get=["$window",function(b){var c=b.marked;return a.setOptions=c.setOptions,c.setOptions(a.defaults),c}]}),a.directive("marked",["marked",function(a){return{restrict:"AE",replace:!0,scope:{opts:"=",marked:"="},link:function(b,c,d){function e(d){c.html(a(d||"",b.opts||null))}e(b.marked||c.text()||""),d.marked&&b.$watch("marked",e)}}}])}();var directive={};directive.runnableExample=["$templateCache","$document",function(a,b){var c=".runnable-example-file",d=(b[0],'<nav class="runnable-example-tabs" ng-if="tabs">  <a ng-class="{active:$index==activeTabIndex}"ng-repeat="tab in tabs track by $index" href="" class="btn"ng-click="setTab($index)">    {{ tab }}  </a></nav>');return{restrict:"C",scope:!0,controller:["$scope",function(a){a.setTab=function(b){var c=a.tabs[b];a.activeTabIndex=b,a.$broadcast("tabChange",b,c)}}],compile:function(a){return a.html(d+a.html()),function(a,b){{var d=b[0],e=d.querySelectorAll(c),f=[];Date.now()}angular.forEach(e,function(a){f.push(a.getAttribute("name"))}),f.length>0&&(a.tabs=f,a.$on("tabChange",function(a,b){angular.forEach(e,function(a){a.style.display="none"});var c=e[b];c.style.display="block"}),a.setTab(0))}}}}],directive.dropdownToggle=["$document","$location","$window",function(a,b){var c,d=null;return{restrict:"C",link:function(e,f){e.$watch(function(){return b.path()},function(){c&&c()}),f.parent().on("click",function(){c&&c()}),f.on("click",function(b){b.preventDefault(),b.stopPropagation();var e=!1;d&&(e=d===f,c()),e||(f.parent().addClass("open"),d=f,c=function(b){b&&b.preventDefault(),b&&b.stopPropagation(),a.off("click",c),f.parent().removeClass("open"),c=null,d=null},a.on("click",c))})}}}],directive.syntax=function(){return{restrict:"A",link:function(a,b,c){function d(a,b,c,d){return'<a href="'+c+'" class="btn syntax-'+a+'" target="_blank" rel="nofollow"><span class="'+d+'"></span> '+b+"</a>"}var e="",f={github:{text:"View on Github",key:"syntaxGithub",icon:"icon-github"},plunkr:{text:"View on Plunkr",key:"syntaxPlunkr",icon:"icon-arrow-down"},jsfiddle:{text:"View on JSFiddle",key:"syntaxFiddle",icon:"icon-cloud"}};for(var g in f){var h=f[g],i=c[h.key];i&&(e+=d(g,h.text,i,h.icon))}var j=document.createElement("nav");j.className="syntax-links",j.innerHTML=e;var k=b[0],l=k.parentNode;l.insertBefore(j,k)}}},directive.tabbable=function(){return{restrict:"C",compile:function(a){var b=angular.element('<ul class="nav nav-tabs"></ul>'),c=angular.element('<div class="tab-content"></div>');c.append(a.contents()),a.append(b).append(c)},controller:["$scope","$element",function(a,b){var c,d=b.contents().eq(0),e=b.controller("ngModel")||{},f=[];e.$render=function(){var a=this.$viewValue;if((c?c.value!=a:a)&&(c&&(c.paneElement.removeClass("active"),c.tabElement.removeClass("active"),c=null),a)){for(var b=0,d=f.length;d>b;b++)if(a==f[b].value){c=f[b];break}c&&(c.paneElement.addClass("active"),c.tabElement.addClass("active"))}},this.addPane=function(b,g){function h(){k.title=g.title,k.value=g.value||g.title,e.$setViewValue||e.$viewValue&&k!=c||(e.$viewValue=k.value),e.$render()}var i=angular.element("<li><a href></a></li>"),j=i.find("a"),k={paneElement:b,paneAttrs:g,tabElement:i};return f.push(k),g.$observe("value",h)(),g.$observe("title",function(){h(),j.text(k.title)})(),d.append(i),i.on("click",function(b){b.preventDefault(),b.stopPropagation(),e.$setViewValue?a.$apply(function(){e.$setViewValue(k.value),e.$render()}):(e.$viewValue=k.value,e.$render())}),function(){k.tabElement.remove();for(var a=0,b=f.length;b>a;a++)k==f[a]&&f.splice(a,1)}}}]}},directive.table=function(){return{restrict:"E",link:function(a,b,c){c["class"]||b.addClass("table table-bordered table-striped code-table")}}};var popoverElement=function(){var a={init:function(){this.element=angular.element('<div class="popover popover-incode top"><div class="arrow"></div><div class="popover-inner"><div class="popover-title"><code></code></div><div class="popover-content"></div></div></div>'),this.node=this.element[0],this.element.css({display:"block",position:"absolute"}),angular.element(document.body).append(this.element);var a=this.element.children()[1];this.titleElement=angular.element(a.childNodes[0].firstChild),this.contentElement=angular.element(a.childNodes[1]),this.element.on("click",function(a){a.preventDefault(),a.stopPropagation()});var b=this;angular.element(document.body).on("click",function(){b.visible()&&b.hide()})},show:function(a,b){this.element.addClass("visible"),this.position(a||0,b||0)},hide:function(){this.element.removeClass("visible"),this.position(-9999,-9999)},visible:function(){return this.position().y>=0},isSituatedAt:function(a){return this.besideElement?a[0]==this.besideElement[0]:!1},title:function(a){return this.titleElement.html(a)},content:function(a){return a&&a.length>0&&(a=marked(a)),this.contentElement.html(a)},positionArrow:function(a){this.node.className="popover "+a},positionAway:function(){this.besideElement=null,this.hide()},positionBeside:function(a){this.besideElement=a;var b=a[0],c=b.offsetLeft,d=b.offsetTop;c-=30,d-=this.node.offsetHeight+10,this.show(c,d)},position:function(a,b){return null==a||null==b?{x:this.node.offsetLeft,y:this.node.offsetTop}:(this.element.css("left",a+"px"),void this.element.css("top",b+"px"))}};return a.init(),a.hide(),a};directive.popover=["popoverElement",function(a){return{restrict:"A",priority:500,link:function(b,c,d){c.on("click",function(b){b.preventDefault(),b.stopPropagation(),a.isSituatedAt(c)&&a.visible()?(a.title(""),a.content(""),a.positionAway()):(a.title(d.title),a.content(d.content),a.positionBeside(c))})}}}],directive.tabPane=function(){return{require:"^tabbable",restrict:"C",link:function(a,b,c,d){b.on("$remove",d.addPane(b,c))}}},directive.foldout=["$http","$animate","$window",function(a,b,c){return{restrict:"A",priority:500,link:function(d,e,f){var g,h,i=f.url;/\/build\//.test(c.location.href)&&(i="/build/docs"+i),e.on("click",function(){d.$apply(function(){if(g)g.hasClass("ng-hide")?b.removeClass(g,"ng-hide"):b.addClass(g,"ng-hide");else{if(h)return;h=!0;var c=e.parent();g=angular.element('<div class="foldout">loading...</div>'),b.enter(g,null,c),a.get(i,{cache:!0}).success(function(a){h=!1,a='<div class="foldout-inner"><div calss="foldout-arrow"></div>'+a+"</div>",g.html(a),"block"==g.css("display")&&(g.css("display","none"),b.addClass(g,"ng-hide"))})}})})}}}],angular.module("bootstrap",[]).directive(directive).factory("popoverElement",popoverElement).run(function(){marked.setOptions({gfm:!0,tables:!0})}),angular.module("pagesData",[]).value("NG_PAGES",{}),angular.module("itemTypes",[]).value("NG_ITEMTYPES",{"class":{Property:{name:"Property",description:"Properties",show:!1},Method:{name:"Method",description:"Methods",show:!1},Constructor:{name:"Constructor",description:"Constructors",show:!1},Field:{name:"Field",description:"Fields",show:!1}},namespace:{Class:{name:"Class",description:"Classes",show:!1},Enum:{name:"Enum",description:"Enums",show:!1},Delegate:{name:"Delegate",description:"Delegates",show:!1},Interface:{name:"Interface",description:"Interfaces",show:!1},Struct:{name:"Struct",description:"Structs",show:!1}}}),angular.module("versionsData",[]).value("NG_VERSION",{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}).value("NG_VERSIONS",[{raw:"1.0.0",major:1,minor:0,patch:0,prerelease:["local"],build:"sha.8200011",version:"1.0.0-master",branch:"master"}]),window.csplay={play:function(a,b){"undefined"==typeof b&&(b=""),"/"!=b.substr(-1,1)&&(b+="/"),"string"==typeof a&&(a=document.getElementById(a));var c=document.createElement("div"),d=document.createElement("div"),e=document.createElement("div");c.innerHTML=a.innerHTML,c.className="csplay_editor",d.className="csplay_splitbar",e.className="csplay_output";var f=a.cloneNode(!1);a.parentNode.replaceChild(f,a),a=f,a.appendChild(c),a.appendChild(d),a.appendChild(e);var g=ace.edit(c);g.getSession().setMode("ace/mode/csharp"),a=$(a),c=$(c),d=$(d),e=$(e);var h=!1;return d.mousedown(function(){h=!0}),a.mouseup(function(){h=!1}).mousemove(function(a){if(h){var b=a.pageY-c.offset().top,f=c.outerHeight(!0)+e.outerHeight(!0)-b,i=d.outerHeight(!0);b>0&&f>0&&(c.css("height",b),e.css("height","calc(100% - "+(b+i)+"px)"),g.resize()),a.preventDefault()}}),{run:function(a){$.ajax({url:b+"run",type:"POST",data:g.getValue(),contentType:"text/plain",success:function(b,c,d){e.html(b).removeClass("error"),"function"==typeof a.success&&a.success(b,c,d)},error:function(b,c,d){"string"==typeof b.responseJSON.error_message?e.text(b.responseJSON.error_message).addClass("error"):e.text(b.responseText).addClass("error"),"function"==typeof a.error&&a.error(b,c,d)},complete:function(b,c){"function"==typeof a.complete&&a.complete(b,c)}})},clearOutput:function(){e.html("")},editor:g}}};var player;angular.module("docsApp",["ngRoute","ngCookies","ngSanitize","ngAnimate","DocsController","versionsData","pagesData","itemTypes","directives","errors","examples","tutorials","versions","bootstrap","ui.bootstrap.dropdown","hc.marked"]),angular.module("directives",[]).directive("markdown",function(){var a=function(){marked.setOptions({gfm:!0,pedantic:!1,sanitize:!0});var a=function(a){return void 0==a?"":marked(a)};return{toHtml:a}}();return{restrict:"E",link:function(b,c,d){b.$watch(d.ngModel,function(b){var d=b,e=a.toHtml(d);c.html(e),angular.forEach(c.find("code"),function(a){hljs.highlightBlock(a),a=a.parentNode;var b=document.createElement("div");b.className="codewrapper",b.innerHTML='<div class="trydiv"><span class="tryspan">Try this code</span></div>',b.childNodes[0].childNodes[0].onclick=function(){tryCode(!0,a.innerText)},a.parentNode.replaceChild(b,a),b.appendChild(a)})})}}}).directive("declaration",function(){return{restrict:"E",link:function(a,b,c){a.$watch(c.ngModel,function(a){c.ngLanguage;b.html('<code class="lang-language"></code>');var d=b.children("code");d.text(a),hljs.highlightBlock(d[0])})}}}).directive("backToTop",["$anchorScroll","$location",function(a,b){return function(c,d){d.on("click",function(){b.hash(""),c.$apply(a)})}}]).directive("code",function(){return{restrict:"E",terminal:!0,compile:function(a){var b=a.hasClass("linenum"),c=/lang-(\S+)/.exec(a[0].className),d=c&&c[1],e=a.html();a.html(window.prettyPrintOne(e,d,b))}}}).directive("scrollYOffsetElement",["$anchorScroll",function(a){return function(b,c){a.yOffset=c}}]),angular.module("DocsController",[]).service("docService",["$q","$http",docServiceFunction]).factory("tocCache",["$cacheFactory",function(a){return a("toc-cache")}]).controller("DocsController",["$scope","$http","$q","$rootScope","$location","$window","$cookies","openPlunkr","NG_PAGES","NG_VERSION","NG_ITEMTYPES","docService","tocCache",function(a,b,c,d,e,f,g,h,i,j,k,l,m){a.openPlunkr=h,a.docsVersion=j.isSnapshot?"snapshot":j.version,a.tocClass=l.tocClassApi,a.navClass=l.navClassApi,a.getNumber=function(a){return new Array(a+1)},a.GetDetail=function(a){console.log(a.target);var b=a.target.nextElementSibling.style.display;a.target.nextElementSibling.style.display="block"==b?"none":"block"},a.ViewSource=function(){return l.getRemoteUrl(this.model.source,this.model.source.startLine+1)},a.ImproveThisDoc=function(){return a.partialModel.mdContent},a.GetTocHref=function(b){if(!a.toc)return"#"+b;var c=a.toc.path;return c?"#"+c+"!"+b:"#"+b},a.$on("$includeContentLoaded",function(){a.currentPage?a.currentPage.path:e.path()}),function(){var b="toc.yaml";l.asyncFetchIndex(b,function(b){a.navbar=jsyaml.load(b),l.getDefaultItem(a.navbar,function(a){a.href&&e.url(a.href)})})}(),function(){l.asyncFetchIndex("md.yaml",function(b){a.mdIndex=jsyaml.load(b)})}(),a.$watch(function(){return e.path()},function(b){b=b.replace(/^\/?(.+?)(\/index)?\/?$/,"$1");var c=a.currentPage=b;if(c){var d=l.getPathInfo(c);if(d.tocPath){var f=m.get(d.tocPath);f?f.content&&(a.toc=f):l.asyncFetchIndex(d.tocFilePath,function(b){var c=jsyaml.load(b),f={path:d.tocPath,content:c};m.put(d.tocPath,f),a.toc=f,!d.contentPath&&f.content.count>0&&e.url(f.path+"!"+f.content[0].href)},function(){m.put(d.tocPath,{path:d.tocPath})})}else a.toc=void 0;if(b=d.contentPath)if(a.toc&&(b=a.toc.path+"/"+b),/\.md$/g.test(b))a.contentType="md",a.partialModel={},a.title=b,a.partialPath=b;else if(/\.yaml$/g.test(b)){a.contentType="yaml";{l.asyncFetchIndex(b,function(b){if(a.partialModel=jsyaml.load(b),a.title=a.partialModel.id,"namespace"==a.partialModel.type.toLowerCase()){a.itemtypes=k.namespace;for(var c in a.partialModel.items){var d=a.itemtypes[a.partialModel.items[c].type];d&&(d.show=!0)}a.partialPath="template/namespace.tmpl"}else{a.itemtypes=k["class"];for(var c in a.itemtypes)a.itemtypes[c].show=!1;for(var c in a.partialModel.items){var d=a.itemtypes[a.partialModel.items[c].type];d&&(d.show=!0)}a.partialPath="template/class.tmpl"}},function(){a.breadcrumb=[],a.partialPath="template/error404.tmpl"})}}else a.partialPath=b;else a.navbar&&a.navbar.count>0?e.url(a.navbar[0].href):(a.breadcrumb=[],a.partialPath="template/error404.tmpl");var g=b.split("/"),h=a.breadcrumb=[],i="";angular.forEach(g,function(a){i+=a,h.push({name:a,url:i}),i+="/"})}else a.navbar&&a.navbar.length>0&&e.url(a.navbar[0].href)}),a.$watch(function(){return a.partialModel},function(){if(a.mdIndex&&a.partialModel){var b=a.mdIndex[a.partialModel.id];if(b&&b.href){a.partialModel.mdHref=l.getRemoteUrl(b);{l.asyncFetchIndex(b.href,function(c){var d=c.substr(b.startLine,b.endLine-b.startLine+1);a.partialModel.mdContent=d})}}}}),a.$watch(function(){return a.toc},function(a){a&&a.content&&l.getDefaultItem(a.content,function(b){b&&b.href&&e.url(a.path+"!"+b.href)})}),a.$watch(function(){return a.navbar},function(a){!e.path()&&a&&a.count>0&&e.url(a[0].href)}),a.versionNumber=angular.version.full,a.version=angular.version.full+"  "+angular.version.codeName,a.loading=0}]),angular.module("errors",["ngSanitize"]).filter("errorLink",["$sanitize",function(a){var b=/((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}<>]/g,c=/^mailto:/,d=/:\d+:\d+$/,e=function(a,b){return a.length>b?a.substr(0,b-3)+"...":a};return function(f,g){var h=g?' target="'+g+'"':"";return f?a(f.replace(b,function(a){return d.test(a)?a:(/^((ftp|https?):\/\/|mailto:)/.test(a)||(a="mailto:"+a),"<a"+h+' href="'+a+'">'+e(a.replace(c,""),60)+"</a>")
})):f}}]).directive("errorDisplay",["$location","errorLinkFilter",function(a,b){var c=function(a){var b=arguments;return a.replace(/\{\d+\}/g,function(a){var c=+a.slice(1,-1);return c+1>=b.length?a:b[c+1]})};return{link:function(d,e,f){var g,h=a.search(),i=[f.errorDisplay];for(g=0;angular.isDefined(h["p"+g]);g++)i.push(h["p"+g]);e.html(b(c.apply(null,i),"_blank"))}}}]),angular.module("examples",[]).factory("formPostData",["$document",function(a){return function(b,c,d){var e=c?"_blank":"_self",f=angular.element('<form style="display: none;" method="post" action="'+b+'" target="'+e+'"></form>');angular.forEach(d,function(a,b){var c=angular.element('<input type="hidden" name="'+b+'">');c.attr("value",a),f.append(c)}),a.find("body").append(f),f[0].submit(),f.remove()}}]).factory("openPlunkr",["formPostData","$http","$q",function(a,b,c){return function(d,e){var f="AngularJS Example",g=e.ctrlKey||e.metaKey;b.get(d+"/manifest.json").then(function(a){return a.data}).then(function(a){var e=[],g=a.name.split("-");return g.unshift("AngularJS"),angular.forEach(g,function(a,b){g[b]=a.charAt(0).toUpperCase()+a.substr(1)}),f=g.join(" - "),angular.forEach(a.files,function(a){e.push(b.get(d+"/"+a,{transformResponse:[]}).then(function(b){return"index-production.html"===a&&(a="index.html"),{name:a,content:b.data}}))}),c.all(e)}).then(function(b){var c={};angular.forEach(b,function(a){c["files["+a.name+"]"]=a.content}),c["tags[0]"]="angularjs",c["tags[1]"]="example",c["private"]=!0,c.description=f,a("http://plnkr.co/edit/?p=preview",g,c)})}}]),angular.module("tutorials",[]).directive("docTutorialNav",function(){var a=["","step_00","step_01","step_02","step_03","step_04","step_05","step_06","step_07","step_08","step_09","step_10","step_11","step_12","the_end"];return{scope:{},template:'<a ng-href="tutorial/{{prev}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-step-backward"></i> Previous</li></a>\n<a ng-href="http://angular.github.io/angular-phonecat/step-{{seq}}/app"><li class="btn btn-primary"><i class="glyphicon glyphicon-play"></i> Live Demo</li></a>\n<a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{diffLo}}...step-{{diffHi}}"><li class="btn btn-primary"><i class="glyphicon glyphicon-search"></i> Code Diff</li></a>\n<a ng-href="tutorial/{{next}}"><li class="btn btn-primary">Next <i class="glyphicon glyphicon-step-forward"></i></li></a>',link:function(b,c,d){var e=1*d.docTutorialNav;b.seq=e,b.prev=a[e],b.next=a[2+e],b.diffLo=e?e-1:"0~1",b.diffHi=e,c.addClass("btn-group"),c.addClass("tutorial-nav")}}}).directive("docTutorialReset",function(){return{scope:{step:"@docTutorialReset"},template:'<p><a href="" ng-click="show=!show;$event.stopPropagation()">Workspace Reset Instructions  ➤</a></p>\n<div class="alert alert-info" ng-show="show">\n  <p>Reset the workspace to step {{step}}.</p>  <p><pre>git checkout -f step-{{step}}</pre></p>\n  <p>Refresh your browser or check out this step online: <a href="http://angular.github.io/angular-phonecat/step-{{step}}/app">Step {{step}} Live Demo</a>.</p>\n</div>\n<p>The most important changes are listed below. You can see the full diff on <a ng-href="https://github.com/angular/angular-phonecat/compare/step-{{step ? (step - 1): \'0~1\'}}...step-{{step}}">GitHub</a>\n</p>'}}),angular.module("versions",[]).controller("DocsVersionsCtrl",["$scope","$location","$window","NG_VERSIONS",function(a,b,c,d){a.docs_version=d[0],a.docs_versions=d;for(var e=0,f=0/0;e<d.length;e++){var g=d[e];f<=g.minor||(g.isLatest=!0,f=g.minor)}a.getGroupName=function(a){return a.isLatest?"Latest":"v"+a.major+"."+a.minor+".x"},a.jumpToDocsVersion=function(a){var d=b.path().replace(/\/$/,"");c.location=a.docsUrl+d}}]);