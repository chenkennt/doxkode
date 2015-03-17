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
    ownCssFiles: [
        'app/css/default.css',
        'app/css/docs.css',
        'app/css/prettify-theme.css',
        'app/css/csplay.css',
        'app/css/animations.css',
        'app/css/open-sans.css'
    ],
    // REMEMBER:
    // * ORDER OF FILES IS IMPORTANT
    // * ALWAYS ADD EACH FILE TO BOTH minified/unminified SECTIONS!
    cssFiles: [
        'app/bower_components/bootstrap/dist/css/bootstrap.min.css',
        'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        'app/bower_components/highlightjs/styles/vs.css',
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
        js: {
            src: '<%= ownJsFiles %>',
            dest: 'tmp/<%= pkg.name %>.js'
        },
        css: {
            src: '<%= ownCssFiles %>',
            dest: 'tmp/<%= pkg.name %>.css'
        }
    },
    uglify: {
      options: {
        banner: '/*! Generated by doc-as-code: <%= pkg.name %> <%= grunt.template.today("yyyy-mm-dd") %> */\n'
      },
      js: {
        src: '<%= ownJsFiles %>',
        dest: 'tmp/<%= pkg.name %>.min.js'
      }
    },
    cssmin: {
      target: {
          src:  ['<%= ownCssFiles %>'],
          dest: 'tmp/<%= pkg.name %>.min.css'
      }
    },
    index: {
        release: {
            template: 'app/index.tmpl',
            dest: 'tmp/docascode.html',
        },
        debug: {
            template: 'app/index.tmpl',
            dest: 'tmp/docascode-debug.html'
        }
    },
    copydest: {
        ext: 'ext',
        js: 'js',
        css: 'css',
        template: 'template'
    },
    clean: {
        debug: ['debug/'],
        release: ['release/'],
       // Unable to clean folder outside current directory:
       // vsix: ['../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/'],
        test: ['test1/', 'test2/']
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
            {expand: true,flatten: false, src: ['template/*', 'web.config', 'favicon.ico'], cwd: 'app', dest: 'debug/', filter: 'isFile'},
            {expand: false,flatten: true, src: ['<%= index.debug.dest %>'], dest: 'debug/index.html'},
          ]
        },
        debug_ref: {
          files: [
            {expand: true,flatten: false, src: ['template/*', 'web.config', 'favicon.ico'], cwd: 'app', dest: 'debug/', filter: 'isFile'},
            {expand: false,flatten: true, src: ['<%= index.debug.dest %>'], dest: 'debug/index.html'},
            {expand: true,flatten: true, src: ['<%= unminifiedJsFiles %>', '<%= unminifiedCssFiles %>'], dest: 'debug/<%= copydest.ext %>', filter: 'isFile'},
            {expand: true,flatten: true, src: ['<%= concat.js.dest %>'], dest: 'debug/<%= copydest.js %>', filter: 'isFile'},
            {expand: true,flatten: true, src: ['<%= concat.css.dest %>'], dest: 'debug/<%= copydest.css %>', filter: 'isFile'},
          ]
        },
        release: {
          files: [
            {expand: true,flatten: false, src: ['template/*', 'web.config', 'favicon.ico'], cwd: 'app', dest: 'release/', filter: 'isFile'},
            {expand: false,flatten: true, src: ['<%= index.release.dest %>'], dest: 'release/index.html'},
          ]
        },
        release_ref: {
          files: [
            {expand: true,flatten: false, src: ['template/*', 'web.config', 'favicon.ico'], cwd: 'app', dest: 'release/', filter: 'isFile'},
            {expand: false,flatten: true, src: ['<%= index.release.dest %>'], dest: 'release/index.html'},
            {expand: true,flatten: true, src: ['<%= jsFiles %>', '<%= cssFiles %>'], dest: 'release/<%= copydest.ext %>', filter: 'isFile'},
            {expand: true,flatten: true, src: ['<%= concat.js.dest %>'], dest: 'release/<%= copydest.js %>', filter: 'isFile'},
            {expand: true,flatten: true, src: ['<%= concat.css.dest %>'], dest: 'release/<%= copydest.css %>', filter: 'isFile'},
          
          ]
        },
        test_roslyn: {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: 'test1/debug' },
            {expand: true, src: ['**'], cwd: 'testdata/test1/', dest: 'test1/debug' },
            {expand: true, src: ['**'], cwd: 'release/', dest: 'test1/release' },
            {expand: true, src: ['**'], cwd: 'testdata/test1/', dest: 'test1/release' },
          ]
        },
        test_simple: {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: 'test2/debug' },
            {expand: true, src: ['**'], cwd: 'testdata/test2/', dest: 'test2/debug' },
            {expand: true, src: ['**'], cwd: 'release/', dest: 'test2/release' },
            {expand: true, src: ['**'], cwd: 'testdata/test2/', dest: 'test2/release' },
          ]
        },
        vsix: {
          files: [
            {expand: true, src: ['**'], cwd: 'release/', dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/' },
          ]
        },
        vsix_debug: {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: '../DocProjectVsix/DocProjectVsix/Templates/Projects/DocProject/' },
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

    grunt.registerTask('debug', [ 'assembleTemplates', 'concat', 'index_debug_inline', 'clean:debug', 'copy:debug_ref']);
    grunt.registerTask('debuginline', [ 'assembleTemplates', 'concat', 'index_debug_ref','clean:debug', 'copy:debug']);
    grunt.registerTask('test', [ 'debug', 'clean:test', 'copy:test_roslyn', 'copy:test_simple']);
    grunt.registerTask('testinline', [ 'debuginline','clean:test', 'copy:test_roslyn', 'copy:test_simple']);
    grunt.registerTask('release', [ 'assembleTemplates', 'concat', 'cssmin', 'uglify', 'index_release_ref', 'clean:release','copy:release_ref']);
    grunt.registerTask('releaseinline', [ 'assembleTemplates', 'concat', 'cssmin', 'uglify', 'index_release_inline','clean:release', 'copy:release']);
    // TODO: change to ref 1. Change exts to CDN; 2. Add js/css to project
    grunt.registerTask('vsix', [ 'releaseinline', 'copy:vsix']);
    grunt.registerTask('vsix-debug', [ 'debuginline', 'copy:vsix_debug']);
    grunt.registerTask('default', ['debug']);
};