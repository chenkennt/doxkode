var createIndex = function (grunt, taskname) {
    'use strict';
    var conf = grunt.config('index')[taskname],
    tmpl = grunt.file.read(conf.template);
    var templatesString = grunt.file.read('tmp/templates.html');
    grunt.config.set('templatesString', templatesString);
    // register the task name in global scope so we can access it in the .tmpl file
    grunt.config.set('currentTask', {name: taskname});
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
        'app/js/docs.js'
    ],
    // REMEMBER:
    // * ORDER OF FILES IS IMPORTANT
    // * ALWAYS ADD EACH FILE TO BOTH minified/unminified SECTIONS!
    cssFiles: [
        'app/bower_components/bootstrap/dist/css/bootstrap.min.css',
        'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        'app/css/prettify-theme.css',
        'app/css/docs.css',
        'app/css/animations.css'
    ],
    jsFiles: [
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
        'app/bower_components/google-code-prettify/bin/prettify.min.js',
        'app/bower_components/google-code-prettify/src/lang-css.js',
        'app/js/angular-bootstrap/bootstrap.min.js',
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
    ],
    unminifiedCssFiles: [
        'app/bower_components/bootstrap/dist/css/bootstrap.css',
        'app/bower_components/google-code-prettify/styles/sons-of-obsidian.css',
        'app/css/prettify-theme.css',
        'app/css/docs.css',
        'app/css/animations.css'
    ],
    unminifiedJsFiles: [
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
        'app/bower_components/google-code-prettify/bin/prettify.min.js',
        'app/bower_components/google-code-prettify/src/lang-css.js',
        'app/js/angular-bootstrap/bootstrap.min.js',
        'app/js/angular-bootstrap/dropdown-toggle.min.js'
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
        },
        default: {
            template: 'app/index.tmpl',
            dest: 'dist/docascode.html'
        }
    },
    copy: {
        release: {
            expand: false,
            flatten: true,
            src: [ 'dist/docascode.html' ],
            dest: 'release/docascode-<%= grunt.config("pkg").version %>/docascode.html'
        },
        release_debug: {
            expand: false,
            flatten: true,
            src: [ 'dist/docascode-debug.html' ],
            dest: 'release/docascode-<%= grunt.config("pkg").version %>/docascode-debug.html'
        },
        release_templates: {
            expand: true,
            flatten: true,
            src: [ 'release_templates/*' ],
            dest: 'release/docascode-<%= grunt.config("pkg").version %>/'
        }
     }
  });
  grunt.fs = require('fs');

  grunt.loadNpmTasks('grunt-contrib-uglify');

    /*** CUSTOM CODED TASKS ***/
    grunt.registerTask('index', 'Generate docascode.html, inline all scripts', function() {
        createIndex(grunt, 'release');
    });

    /*  is basically the releaes version but without any minifing */
    grunt.registerTask('index_debug', 'Generate docascode-debug.html, inline all scripts unminified', function() {
        createIndex(grunt, 'debug');
    });

    /* Debug is basically the releaes version but without any minifing */
    grunt.registerTask('index_default', 'Generate docascode-debug.html, inline all scripts unminified', function() {
        createIndex(grunt, 'default');
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

    grunt.registerTask('debug', [ 'assembleTemplates', 'concat', 'uglify', 'index_debug' ]);
    grunt.registerTask('release', [ 'assembleTemplates', 'concat', 'uglify', 'index' ]);
    grunt.registerTask('default', ['uglify']);
};