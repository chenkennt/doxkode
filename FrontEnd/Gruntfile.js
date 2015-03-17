var createIndex = function (grunt, config, mode) {
    'use strict';
    var conf = grunt.config('index')[config],
    tmpl = grunt.file.read(conf.template);
    var templatesString = grunt.file.read('tmp/templates.html');
    grunt.config.set('templatesString', templatesString);
    // register the task name in global scope so we can access it in the .tmpl file
    grunt.config.set('compileConfig', {mode: mode, config: config});
    var gruntTemplate = grunt.template;

    //google-code-prettify/src/prettify.js contains <% %> pair, so use customized delimiters instead
    gruntTemplate.addDelimiters('dollar', '<<%', '%>>');
    var result = gruntTemplate.process(tmpl, {delimiters: 'dollar'});

    grunt.file.write(conf.dest, result.replace(/# sourceMappingURL/g, ""));
    grunt.log.writeln('Generated \'' + conf.dest + '\' from \'' + conf.template + '\'');
};


module.exports = function(grunt) {
  'use strict';
  // Project configuration.

  // load all grunt tasks matching the `grunt-*` pattern
  require('load-grunt-tasks')(grunt);

  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    ownJsFiles: [
        //'app/js/search-worker.js',
        'app/js/pages-data.js',
        'app/js/versions-data.js',
        'app/js/csplay.js',
        'app/js/docs.js'
    ],
    // REMEMBER:
    // * ORDER OF FILES IS IMPORTANT
    // * ALWAYS ADD EACH FILE TO BOTH minified/unminified SECTIONS!
    cssFiles: [
        'app/bower_components/bootstrap/dist/css/bootstrap.min.css',
        'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        'app/bower_components/highlightjs/styles/vs.css',
        'app/css/prettify-theme.css',
        'app/css/docs.css',
        'app/css/csplay.css',
        'app/css/animations.css',
        'app/css/default.css',
        'app/css/open-sans.css'
    ],
    jsFiles: [
        'app/bower_components/jquery/dist/jquery.min.js',
        'app/bower_components/js-yaml/dist/js-yaml.min.js',
        'app/bower_components/angular/angular.min.js',
        'app/bower_components/angular-resource/angular-resource.min.js',
        'app/bower_components/angular-route/angular-route.min.js',
        'app/bower_components/angular-cookies/angular-cookies.min.js',
        'app/bower_components/angular-sanitize/angular-sanitize.min.js',
        'app/bower_components/angular-touch/angular-touch.min.js',
        'app/bower_components/angular-animate/angular-animate.min.js',
        'app/bower_components/marked/lib/marked.js',
        'app/bower_components/lunr.js/lunr.min.js',
        // 'app/bower_components/google-code-prettify/src/prettify.js',
        // 'app/bower_components/google-code-prettify/src/lang-css.js',
        // 'app/bower_components/highlightjs/highlight.pack.js',
        // 'app/bower_components/highlight/src/highlight.js',
        // 'app/bower_components/highlight/src/languages/cs.js',
        'app/js/angular-bootstrap/bootstrap.min.js',
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
    ],
    unminifiedCssFiles: [
        'app/bower_components/bootstrap/dist/css/bootstrap.min.css',
        'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        'app/bower_components/highlightjs/styles/vs.css',
        'app/css/prettify-theme.css',
        'app/css/docs.css',
        'app/css/csplay.css',
        'app/css/animations.css',
        'app/css/default.css',
        'app/css/open-sans.css'
    ],
    unminifiedJsFiles: [
        'app/bower_components/jquery/dist/jquery.min.js',
        'app/bower_components/js-yaml/dist/js-yaml.min.js',
        'app/bower_components/angular/angular.min.js',
        'app/bower_components/angular-resource/angular-resource.min.js',
        'app/bower_components/angular-route/angular-route.min.js',
        'app/bower_components/angular-cookies/angular-cookies.min.js',
        'app/bower_components/angular-sanitize/angular-sanitize.min.js',
        'app/bower_components/angular-touch/angular-touch.min.js',
        'app/bower_components/angular-animate/angular-animate.min.js',
        'app/bower_components/marked/lib/marked.js',
        'app/bower_components/lunr.js/lunr.min.js',
        // 'app/bower_components/google-code-prettify/src/prettify.js',
        // 'app/bower_components/google-code-prettify/src/lang-css.js',
        // 'app/bower_components/highlight/src/highlight.js',
        // 'app/bower_components/highlight/src/languages/cs.js',
        // 'app/bower_components/highlightjs/highlight.pack.js',
        'app/js/angular-bootstrap/bootstrap.min.js',
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
    ],
    cdnCssFiles: [
        "//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css",
    ],
    cdnJsFiles: [
        "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/8.4/highlight.min.js",
        "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/8.4/languages/cs.min.js",
        "https://cdnjs.cloudflare.com/ajax/libs/ace/1.1.8/ace.js",
    ],
    /* make it use .jshintrc */
    jshint: {
        options: {
            curly: false,
            eqeqeq: true,
            immed: true,
            latedef: true,
            newcap: true,
            noarg: true,
            sub: true,
            undef: true,
            unused: false,
            boss: true,
            eqnull: true,
            browser: true,
            globals: {
                jQuery: true,
                marked: true,
                google: true,
                hljs: true,
                /* leaflet.js*/
                L: true,
                console: true,
                MDwiki: true,
                Prism: true,
                alert: true,
                Hogan: true
            }
        },
        /*gruntfile: {
            src: 'Gruntfile.js'
        },*/
        js: {
            src: ['app/js/*.js', 'app/js/**/*.js', '!app/js/marked.js']
        }
    },
    concat: {
        options: {
            //banner: '<%= banner %>',
            stripBanners: true
        },
        dev: {
            src: '<%= ownJsFiles %>',
            dest: 'tmp/<%= pkg.name %>.js'
        }
    },
    uglify: {
      options: {
        banner: '/*! Generated by doc-as-code: <%= pkg.name %> <%= grunt.template.today("yyyy-mm-dd") %> */\n'
      },
      build: {
        src: '<%= ownJsFiles %>',
        dest: 'tmp/<%= pkg.name %>.min.js'
      }
    },
    index: {
        release: {
            template: 'app/index.tmpl',
            dest: 'dist/docascode.html'
        },
        debug: {
            template: 'app/index.tmpl',
            dest: 'dist/docascode-debug.html'
        }
    },
    copy: {
        /*main: {
          files: [
            // includes files within path
            {expand: true, src: ['path/*'], dest: 'dest/', filter: 'isFile'},

            // includes files within path and its sub-directories
            {expand: true, src: ['path/**'], dest: 'dest/'},

            // makes all src relative to cwd
            {expand: true, cwd: 'path/', src: ['**'], dest: 'dest/'},

            // flattens results to a single level
            {expand: true, flatten: true, src: ['path/**'], dest: 'dest/', filter: 'isFile'},
          ],
        },*/
        debug: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: 'debug/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: 'debug/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode-debug.html'], dest: 'debug/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: 'debug/template/', filter: 'isFile'},
          ]
        },
        test_roslyn: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: 'test1/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: 'test1/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode-debug.html'], dest: 'test1/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: 'test1/template/', filter: 'isFile'},
            {expand: true, src: ['**'], cwd: 'testdata/test1/', dest: 'test1' },
          ]
        },
        test_simple: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: 'test2/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: 'test2/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode-debug.html'], dest: 'test2/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: 'test2/template/', filter: 'isFile'},
            {expand: true, src: ['**'], cwd: 'testdata/test2/', dest: 'test2' },
          ]
        },
        release: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: 'release/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: 'release/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode.html'], dest: 'release/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: 'release/template/', filter: 'isFile'},
          ]
        },
        vsix: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode.html'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/template/', filter: 'isFile'},
          ]
        },
        vsix_debug: {
          files: [
            {expand: false,flatten: true, src: ['app/favicon.ico'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/favicon.ico'},
            {expand: false,flatten: true, src: ['app/web.config'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/web.config'},
            {expand: false,flatten: true, src: ['dist/docascode-debug.html'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/index.html'},
            {expand: true,flatten: true, src: ['app/template/*'], dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/template/', filter: 'isFile'},
          ]
        }
     }
  });
  grunt.fs = require('fs');

  grunt.loadNpmTasks('grunt-contrib-uglify');

    /*** CUSTOM CODED TASKS ***/
    grunt.registerTask('index_release_inline', 'Generate docascode.html, inline all scripts', function() {
        createIndex(grunt, 'release', 'inline');
    });

    grunt.registerTask('index_release_ref', 'Generate docascode.html, ref all scripts minified', function() {
        createIndex(grunt, 'release', 'ref');
    });

    /*  is basically the releaes version but without any minifing */
    grunt.registerTask('index_debug_inline', 'Generate docascode-debug.html, inline all scripts unminified', function() {
        createIndex(grunt, 'debug', 'inline');
    });

    /* Debug is basically the releaes version but without any minifing */
    grunt.registerTask('index_debug_ref', 'Generate docascode-debug.html, ref all scripts unminified', function() {
        createIndex(grunt, 'debug', 'ref');
    });

    grunt.registerTask('assembleTemplates', 'Adds a script tag with id to each template', function() {
        var templateString = '';
        grunt.file.recurse('templates/', function(abspath, rootdir, subdir, filename){
            var intro = '<script type="text/html" id="/' + rootdir.replace('/','') + '/' + subdir.replace('/','') + '/' + filename.replace('.html','') + '">\n';
            var content = grunt.file.read(abspath);
            var outro = '</script>\n';
            templateString += intro + content + outro;
        });
        grunt.file.write('tmp/templates.html', templateString);
    });

    grunt.registerTask('debug', [ 'assembleTemplates', 'concat', 'uglify', 'index_debug_inline', 'copy:debug']);
    grunt.registerTask('test', [ 'assembleTemplates', 'concat', 'uglify', 'index_debug_inline', 'copy:test_roslyn', 'copy:test_simple']);
    grunt.registerTask('release', [ 'assembleTemplates', 'concat', 'uglify', 'index_release_inline', 'copy:release']);
    grunt.registerTask('vsix', [ 'assembleTemplates', 'concat', 'uglify', 'index_release_inline', 'copy:vsix']);
    grunt.registerTask('vsix-debug', [ 'assembleTemplates', 'concat', 'uglify', 'index_debug_inline', 'copy:vsix_debug']);
    grunt.registerTask('default', ['uglify']);
};