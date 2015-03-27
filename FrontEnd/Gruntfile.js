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
        'app/bower_components/marked/lib/marked.js',
        'app/bower_components/angular-marked/angular-marked.js',
        'app/js/bootstrap.js',
        'app/js/pages-data.js',
        'app/js/versions-data.js',
        'app/js/csplay.js',
        'app/js/service.js',
        'app/js/docs.js',
    ],
    ownCssFiles: [
        'app/bower_components/highlightjs/styles/vs.css',
        // 'app/css/default.css',
        // 'app/css/docs.css',
        // 'app/css/prettify-theme.css',
        // 'app/css/csplay.css',
        // 'app/css/animations.css',
        'tmp/main.css',
        // 'app/css/open-sans.css'
    ],
    // REMEMBER:
    // * ORDER OF FILES IS IMPORTANT
    // * ALWAYS ADD EACH FILE TO BOTH minified/unminified SECTIONS!
    cssFiles: [
       /* move to use CDN
        'app/bower_components/bootstrap/dist/css/bootstrap.min.css',*/
        // 'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        // 'app/bower_components/highlightjs/styles/vs.css',
    ],
    jsFiles: [
    /*  move to use CDN
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
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
        'app/js/angular-bootstrap/bootstrap.min.js',*/
    ],
    unminifiedCssFiles: [
       /* move to use CDN
         'app/bower_components/bootstrap/dist/css/bootstrap.min.css',*/
        // 'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        // 'app/bower_components/highlightjs/styles/vs.css',
    ],
    unminifiedJsFiles: [
      /* move to use CDN
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
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
        'app/js/angular-bootstrap/bootstrap.min.js',*/
    ],
    cdnCssFiles: [
        "//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css",
        "//cdnjs.cloudflare.com/ajax/libs/bootswatch/3.3.2/custom/bootstrap.min.css",
    ],
    cdnJsFiles: [
        "//cdnjs.cloudflare.com/ajax/libs/highlight.js/8.4/highlight.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/highlight.js/8.4/languages/cs.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/ace/1.1.8/ace.js",
        "//cdnjs.cloudflare.com/ajax/libs/jquery/2.1.3/jquery.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/js-yaml/3.2.7/js-yaml.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/lunr.js/0.5.7/lunr.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-resource.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-route.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-cookies.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-sanitize.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-touch.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-animate.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/angular-ui-bootstrap/0.12.1/ui-bootstrap-tpls.min.js",
        "//cdnjs.cloudflare.com/ajax/libs/marked/0.3.2/marked.min.js",
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
                ace: true,
                $: true,
                csplay: true,
                angular: true,
                jsyaml: true,
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
    watch: {
        files: [
            'Gruntfile.js',
            'app/js/*.js',
            'app/css/*.css',
            'app/css/**/*.css',
            'app/css/*.css',
            'app/css/*.less',
            'app/css/**/*.less',
            'app/template/*.tmpl',
            'app/index.tmpl',
            'sample/data/**/*.*',
        ],
        tasks: ['jshint']
    },
    connect: {
      test: {
        options: {
          port: 8000,
          hostname: '0.0.0.0',
          base: './sample/host/debug',
          keepalive: true
        }
      }
    },
    less: {
        dev: {
            options: {
                compress: false,
            },
            files: {
                'tmp/main.css': 'app/css/*.less',
            },
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
        dest: 'tmp/<%= pkg.name %>.js'
      }
    },
    cssmin: {
      target: {
          src:  ['<%= ownCssFiles %>'],
          dest: 'tmp/<%= pkg.name %>.css'
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
        test: ['sample/host/**']
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
            {expand: true,flatten: true, src: ['<%= uglify.js.dest %>'], dest: 'release/<%= copydest.js %>', filter: 'isFile'},
            {expand: true,flatten: true, src: ['<%= cssmin.target.dest %>'], dest: 'release/<%= copydest.css %>', filter: 'isFile'},

          ]
        },
        test: {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: 'sample/host/debug' },
            {expand: true, src: ['**'], cwd: 'sample/data/', dest: 'sample/host/debug' },
            {expand: true, src: ['**'], cwd: 'release/', dest: 'sample/host/release' },
            {expand: true, src: ['**'], cwd: 'sample/data/', dest: 'sample/host/release' },
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

    grunt.registerTask('debug', [ 'less:dev', 'concat', 'index_debug_ref', 'clean:debug', 'copy:debug_ref']);
    grunt.registerTask('debuginline', [ 'less:dev', 'concat', 'index_debug_inline','clean:debug', 'copy:debug']);
    grunt.registerTask('test', [ 'debug', 'release', 'clean:test', 'copy:test']);
    grunt.registerTask('testinline', [ 'debuginline', 'releaseinline', 'clean:test', 'copy:test']);
    grunt.registerTask('release', [ 'less:dev', 'concat', 'cssmin', 'uglify', 'index_release_ref', 'clean:release','copy:release_ref']);
    grunt.registerTask('releaseinline', [ 'less:dev','concat', 'cssmin', 'uglify', 'index_release_inline','clean:release', 'copy:release']);
    grunt.registerTask('vsix', [ 'release', 'copy:vsix']);
    grunt.registerTask('vsixdebug', [ 'debug', 'copy:vsix_debug']);
    grunt.registerTask('default', ['debug']);
    grunt.registerTask('server', [ 'debug', 'copy:test', 'connect:test' ]);
};