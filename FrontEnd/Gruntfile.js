var createIndex = function (grunt, config, mode) {
    'use strict';
    var conf = grunt.config('index')[config],
    tmpl = grunt.file.read(conf.template);
    // Doesn't seem to be useful... comment for now to unblock build
    // var templatesString = grunt.file.read('tmp/templates.html');
    // grunt.config.set('templatesString', templatesString);
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
        'app/js/services/contentService.js',
        'app/js/services/csplayService.js',
        'app/js/services/urlService.js',
        'app/js/app.js',
        'app/js/bootstrap.js',
        'app/js/constants.js',
        'app/js/controller.js',
        'app/js/directives.js',
        'app/js/errors.js',
        'app/js/pages-data.js',
        'app/js/util.js',
        'app/js/versions-data.js',
        'app/js/versions.js',
    ],
    ownCssFiles: [
        'app/bower_components/highlightjs/styles/vs.css',
        'tmp/main.css',
    ],
    // REMEMBER:
    // * ORDER OF FILES IS IMPORTANT
    // * ALWAYS ADD EACH FILE TO BOTH minified/unminified SECTIONS!
    // Move to use CDN instead
    cssFiles: [
    ],
    jsFiles: [
    ],
    unminifiedCssFiles: [
    ],
    unminifiedJsFiles: [
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
    jshint: {
        options: {
            jshintrc: '.jshintrc'
        },
        js: {
            src: ['app/js/*.js', 'app/js/**/*.js', '!app/js/marked.js']
        }
    },
    watch: {
        test: {
            files: [
                'Gruntfile.js',
                'app/js/*.js',
                'app/js/services/*.js',
                'app/css/*.css',
                'app/css/**/*.css',
                'app/css/*.css',
                'app/css/*.less',
                'app/css/**/*.less',
                'app/template/*.tmpl',
                'app/index.tmpl',
                'sample/data/**/*.*',
            ],
            tasks: ['jshint', 'test']
        },
        'test.min': {
            files: [
                'Gruntfile.js',
                'app/js/*.js',
                'app/js/services/*.js',
                'app/css/*.css',
                'app/css/**/*.css',
                'app/css/*.css',
                'app/css/*.less',
                'app/css/**/*.less',
                'app/template/*.tmpl',
                'app/index.tmpl',
                'sample/data.min/**/*.*',
            ],
            tasks: ['jshint', 'test.min']
        }
    },
    connect: {
      debug: {
        options: {
          port: 8000,
          hostname: 'localhost',
          base: './sample/host/debug',
          keepalive: true
        }
      },
      release: {
        options: {
          port: 8001,
          hostname: 'localhost',
          base: './sample/host/release',
          keepalive: true
        }
      },
      'debug.min': {
        options: {
          port: 8002,
          hostname: 'localhost',
          base: './sample/host.min/debug',
          keepalive: true
        }
      },
      'release.min': {
        options: {
          port: 8003,
          hostname: 'localhost',
          base: './sample/host.min/release',
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
    exists: {
        src: ['<%= ownJsFiles %>', '<%= ownCssFiles %>']
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
        test: ['sample/host/**'],
        'test.min': ['sample/host.min']
    },
    copy: {
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
        'test.min': {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: 'sample/host.min/debug' },
            {expand: true, src: ['**'], cwd: 'sample/data.min/', dest: 'sample/host.min/debug' },
            {expand: true, src: ['**'], cwd: 'release/', dest: 'sample/host.min/release' },
            {expand: true, src: ['**'], cwd: 'sample/data.min/', dest: 'sample/host.min/release' },
          ]
        },
        vsix: {
          files: [
            {expand: true, src: ['**'], cwd: 'release/', dest: '../Vsix/Template/DocumentationWebsiteTemplate/' },
          ]
        },
        vsix_debug: {
          files: [
            {expand: true, src: ['**'], cwd: 'debug/', dest: '../Vsix/Template/DocumentationWebsiteTemplate/' },
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

    var fs = require('fs');
    var path = require('path');
    grunt.registerMultiTask('exists', 'File Existence', function() {
        grunt.util._.each(this.data, function(files) {
            files.forEach(function(file){
                if (!fs.existsSync(file)) {
                    grunt.fatal("Required file '" + file + "' doesn't exist.");
                } else {
                    grunt.log.ok("Verified that required file [" + file + "] exists.");
                }
            })
        });
    });

    grunt.registerTask('build', ['less:dev', 'exists:src', 'jshint']);
    grunt.registerTask('debug', [ 'build', 'concat', 'index_debug_ref', 'clean:debug', 'copy:debug_ref']);
    grunt.registerTask('debuginline', [ 'build', 'concat', 'index_debug_inline','clean:debug', 'copy:debug']);
    grunt.registerTask('release', [ 'build', 'concat', 'cssmin', 'uglify', 'index_release_ref', 'clean:release','copy:release_ref']);
    grunt.registerTask('releaseinline', [ 'build','concat', 'cssmin', 'uglify', 'index_release_inline','clean:release', 'copy:release']);
    grunt.registerTask('test', [ 'debug', 'release', 'clean:test', 'copy:test']);
    grunt.registerTask('testinline', [ 'debuginline', 'releaseinline', 'clean:test', 'copy:test']);
    grunt.registerTask('vsix', [ 'release', 'copy:vsix']);
    grunt.registerTask('vsixdebug', [ 'debug', 'copy:vsix_debug']);
    grunt.registerTask('server', [ 'debug', 'copy:test', 'connect:debug' ]);
    grunt.registerTask('serverrelease', [ 'release', 'copy:test', 'connect:release' ]);
    grunt.registerTask('test.min', [ 'debug', 'release', 'clean:test.min', 'copy:test.min']);
    grunt.registerTask('test.release.min', [ 'debug', 'release', 'clean:test.min', 'copy:test.min']);
    grunt.registerTask('default', ['test.min']);
};