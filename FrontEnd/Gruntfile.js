var createIndex = function(grunt, config) {
  'use strict';
  var conf = grunt.config('index')[config],
  tmpl = grunt.file.read(conf.template);
  // Doesn't seem to be useful... comment for now to unblock build
  // register the task name in global scope so we can access it in the template file
  grunt.config.set('compileConfig', {
    config: config
  });
  var gruntTemplate = grunt.template;

  //google-code-prettify/src/prettify.js contains <% %> pair, so use customized delimiters instead
  gruntTemplate.addDelimiters('dollar', '<<%', '%>>');
  var result = gruntTemplate.process(tmpl
//  , {
//    delimiters: 'dollar'
//  }
  );

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
      'app/src/services/contentService.js',
      'app/src/services/csplayService.js',
      'app/src/services/urlService.js',
      'app/src/services/markdownService.js',
      'app/src/app.js',
      'app/src/util/bootstrap.js',
      'app/src/util/constants.js',

      'app/src/controller.js',
      'app/src/container/container.js',
      'app/src/navbar/navbar.js',

      'app/src/util/pages-data.js',
      'app/src/util/directives.js',
      'app/src/content/yamlContentDirective.js',
      'app/src/content/markdownContentDirective.js',
      'app/src/util/util.js',
      'app/src/util/versions-data.js',
      'app/src/util/versions.js',
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
//      'app/bower_components/highlightjs/highlight.pack.js',
//      'app/bower_components/jquery/dist/jquery.js',
//      'app/bower_components/angular/angular.min.js',
    ],
    unminifiedCssFiles: [],
    unminifiedJsFiles: [
//      'app/bower_components/highlightjs/highlight.pack.js',
//      'app/bower_components/jquery/dist/jquery.js',
//      'app/bower_components/angular/angular.js',
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
        src: ['app/src/*.js', 'app/src/**/*.js', '!app/src/marked.js']
      }
    },
    watch: {
      test: {
        files: [
          'Gruntfile.js',
          'app/src/*.*',
          'app/src/**/*.*',
          'app/content/*.*',
          'app/content/**/*.*',
          'app/*.*',
          'sample/data/**/*.*',
        ],
        tasks: ['jshint', 'test']
      },
      'test.min': {
        files: [
          'Gruntfile.js',
          'app/src/*.*',
          'app/src/**/*.*',
          'app/content/*.*',
          'app/content/**/*.*',
          'app/*.*',
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
          'tmp/main.css': 'app/content/css/*.less',
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
        src: ['<%= jsFiles %>', '<%= ownJsFiles %>'],
        dest: 'tmp/<%= pkg.name %>.js'
      }
    },
    exists: {
      src: ['<%= ownCssFiles %>', '<%= ownJsFiles %>']
    },
    cssmin: {
      target: {
        src: ['<%= cssFiles %>', '<%= ownCssFiles %>'],
        dest: 'tmp/<%= pkg.name %>.css'
      }
    },
    index: {
      release: {
        template: 'app/src/app.html',
        dest: 'tmp/docascode.html',
      },
      debug: {
        template: 'app/src/app.html',
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
        files: [{
          expand: true,
          flatten: false,
          src: ['web.config', 'favicon.ico', 'logo.png'],
          cwd: 'app/content',
          dest: 'debug/',
          filter: 'isFile'
        }, {
          expand: true,
          flatten: true,
          src: ['src/**/*.html', '!src/app.html'],
          cwd: 'app',
          dest: 'debug/template/',
          filter: 'isFile'
        }, {
          expand: false,
          flatten: true,
          src: ['<%= index.debug.dest %>'],
          dest: 'debug/index.html'
        }, {
          expand: true,
          flatten: true,
          src: ['<%= unminifiedJsFiles %>', '<%= unminifiedCssFiles %>'],
          dest: 'debug/<%= copydest.ext %>',
          filter: 'isFile'
        }, {
          expand: true,
          flatten: true,
          src: ['<%= concat.js.dest %>'],
          dest: 'debug/<%= copydest.js %>',
          filter: 'isFile'
        }, {
          expand: true,
          flatten: true,
          src: ['<%= concat.css.dest %>'],
          dest: 'debug/<%= copydest.css %>',
          filter: 'isFile'
        }, ]
      },
      release: {
        files: [{
          expand: true,
          flatten: false,
          src: ['web.config', 'favicon.ico', 'logo.png'],
          cwd: 'app/content',
          dest: 'release/',
          filter: 'isFile'
        }, {
          expand: true,
          flatten: true,
          src: ['src/**/*.html', '!src/app.html'],
          cwd: 'app',
          dest: 'release/template/',
          filter: 'isFile'
        }, {
            expand: false,
            flatten: true,
            src: ['<%= index.release.dest %>'],
            dest: 'release/index.html'
          }, {
            expand: true,
            flatten: true,
            src: ['<%= uglify.js.dest %>'],
            dest: 'release/<%= copydest.js %>',
            filter: 'isFile'
          }, {
            expand: true,
            flatten: true,
            src: ['<%= cssmin.target.dest %>'],
            dest: 'release/<%= copydest.css %>',
            filter: 'isFile'
          },

        ]
      },
      test: {
        files: [{
          expand: true,
          src: ['**'],
          cwd: 'debug/',
          dest: 'sample/host/debug'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'sample/data/',
          dest: 'sample/host/debug'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'release/',
          dest: 'sample/host/release'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'sample/data/',
          dest: 'sample/host/release'
        }, ]
      },
      'test.min': {
        files: [{
          expand: true,
          src: ['**'],
          cwd: 'debug/',
          dest: 'sample/host.min/debug'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'sample/data.min/',
          dest: 'sample/host.min/debug'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'release/',
          dest: 'sample/host.min/release'
        }, {
          expand: true,
          src: ['**'],
          cwd: 'sample/data.min/',
          dest: 'sample/host.min/release'
        }, ]
      },
      vsix: {
        files: [{
          expand: true,
          src: ['**'],
          cwd: 'release/',
          dest: '../Vsix/Template/DocumentationWebsiteTemplate/'
        }, ]
      },
      vsix_debug: {
        files: [{
          expand: true,
          src: ['**'],
          cwd: 'debug/',
          dest: '../Vsix/Template/DocumentationWebsiteTemplate/'
        }, ]
      }
    },
    karma: {
      ut: {
        configFile: 'karma.config.js'
      }
    },
    exec: {
      buildmeta: {
        cmd: function(projectList, target) {
          projectList = projectList.replace(/\//g, "\\");
          target = target.replace(/\//g, "\\");
          return "..\\drop\\release\\buildmeta\\buildmeta.exe " + projectList + " /o:" + target + " && del " + projectList + " " + target + "\\index.yml";
        },
        maxBuffer: 1024 * 1024 // NOTE: Make sure this is big enough, child_process has a bug that if output exceeds max buffer exec() will return early
      }
    },
    updatetest: {
      mvc: {
        source: ["src/System.Web.Mvc/System.Web.Mvc.csproj"],
        target: "sample/data/api_system_web_mvc"
      },
      roslyn: {
        source: [
          "src/Compilers/Core/Desktop/CodeAnalysis.csproj",
          "src/Compilers/Core/Portable/CodeAnalysis.Desktop.csproj",
          "src/Compilers/CSharp/Desktop/CSharpCodeAnalysis.Desktop.csproj",
          "src/Compilers/CSharp/Portable/CSharpCodeAnalysis.csproj",
          "src/EditorFeatures/Core/EditorFeatures.csproj",
          "src/EditorFeatures/CSharp/CSharpEditorFeatures.csproj",
          "src/EditorFeatures/Text/TextEditorFeatures.csproj",
          "src/Features/Core/Features.csproj",
          "src/Features/CSharp/CSharpFeatures.csproj",
          "src/VisualStudio/Core/Def/ServicesVisualStudio.csproj",
          "src/Workspaces/Core/Desktop/Workspaces.Desktop.csproj",
          "src/Workspaces/Core/Portable/Workspaces.csproj",
          "src/Workspaces/CSharp/Desktop/CSharpWorkspace.Desktop.csproj",
          "src/Workspaces/CSharp/Portable/CSharpWorkspace.csproj"
        ],
        target: "sample/data/api_roslyn"
      },
      corefx: {
        source: ["src/**/src/**/*.csproj"],
        target: "sample/data/api_corefx"
      }
    },
    "testspec": {
      "default": {
        "spec": "../doc/metadata_dotnet_spec.md"
      }
    }
  });
  grunt.fs = require('fs');

  grunt.loadNpmTasks('grunt-contrib-uglify');

  /*** CUSTOM CODED TASKS ***/
  grunt.registerTask('index_release', 'Generate docascode.html, with all the referenced local js/css files minified into single files', function() {
    createIndex(grunt, 'release');
  });

  /*  is basically the releaes version but without any concating and minifing */
  grunt.registerTask('index_debug', 'Generate docascode-debug.html, will all referenced local js/css files copied to ext folder', function() {
    createIndex(grunt, 'debug');
  });

  grunt.registerMultiTask('updatetest', "Update test data", function() {
    // Delete existing data
    grunt.file.delete(this.data.target);

    // Generate project list
    var root = grunt.option("gitroot");
    var files = grunt.file
      .expand(this.data.source.map(function(item) {
        return root === undefined ? item : root + "/" + item;
      }))
      .reduce(function(prev, cur) {
        return prev + "\n" + cur;
      });

    // Run buildmeta
    var list = this.data.target + "/project.list";
    grunt.file.write(list, files);
    grunt.task.run('exec:buildmeta:' + list + ':' + this.data.target);
  });

  var fs = require('fs');
  grunt.registerMultiTask('exists', 'File Existence', function() {
    grunt.util._.each(this.data, function(files) {
      files.forEach(function(file) {
        if (!fs.existsSync(file)) {
          grunt.fatal("Required file '" + file + "' doesn't exist.");
        } else {
          grunt.log.ok("Verified that required file [" + file + "] exists.");
        }
      });
    });
  });

  grunt.registerMultiTask("testspec", function() {
    // Delete existing data
    grunt.file.delete("obj/testspec");

    // Generate test cases
    var marked = require("marked");
    var source = grunt.file.read(this.data.spec);
    var tokens = marked.lexer(source);
    var samples = [];
    var sample = null;
    for (var i = 0; i < tokens.length; i++) {
      if (tokens[i].type === "blockquote_start") {
        if (sample !== null) {
          grunt.fail.fatal("Blockquote inside blockquote is not supported.");
        }

        sample = {};
        samples.push(sample);
      } else if (tokens[i].type === "blockquote_end") {
        sample = null;
      }

      if (sample === null) continue;

      if (tokens[i].type === "paragraph" && typeof sample.name === "undefined") {
        sample.name = tokens[i].text;
      } else if (tokens[i].type === "code" && typeof sample[tokens[i].lang] === "undefined") {
        sample[tokens[i].lang] = tokens[i].text;
      }
    }

    samples.forEach(function(item, index) {
      var folder = "obj/testspec/" + (index + 1) + "/";
      grunt.file.write(folder + "project.json", '{"frameworks":{"dnx451":{}}}');
      for (var key in item) {
        if (item.hasOwnProperty(key)) {
          switch (key) {
            case "name":
              grunt.file.write(folder + "name", item[key]);
              break;
            case "yaml":
            case "yml":
              grunt.file.write(folder + "baseline.yml", item[key]);
              break;
            case "cs":
            case "csharp":
              grunt.file.write(folder + "source.cs", item[key]);
              break;
            default:
              grunt.log.writeln("Language " + key + " is not supported.");
              break;
          }
        }
      }

      // Run buildmeta
      grunt.task.run("exec:buildmeta:" + folder + "project.json" + ':' + folder + "output");
    });

    // Verify test result
    grunt.task.run("verify_spec_case:obj/testspec/");
  });

  grunt.registerTask("verify_spec_case", function(testpath) {
    var yaml = require("js-yaml");
    var report = "# Test Case Report\n";
    var failedCount = 0;
    var files = grunt.file.expand(testpath + "*");
    files.forEach(function(path, index, list) {
      var name = grunt.file.read(path + "/name");
      grunt.log.write("Verifying test case (" + (index + 1) + "/" + list.length + "): " + name + "...");

      // Load baseline
      var baseline = yaml.safeLoad(grunt.file.read(path + "/baseline.yml"));

      // Load test result
      var result = [];
      grunt.file.expand(path + "/output/api/*.yml").forEach(function(file) {
        yaml.safeLoad(grunt.file.read(file)).items.forEach(function(item) {
          result.push(item);
        });
      });

      // Currently supports two baseline data
      // 1. A single item:
      //   uid: System.String
      //   id: String
      // 2. A list of items:
      //   - uid: System.String
      //   - uid: System.Int32
      var same = true;
      var diffObject = function(expected, actual) {
        var simpleActual = {};
        for (var key in expected) {
          if (expected.hasOwnProperty(key)) {
            simpleActual[key] = actual[key];
            if (JSON.stringify(expected[key]) !== JSON.stringify(actual[key])) {
              same = false;
            }
          }
        }

        return simpleActual;
      };

      var actualList = [];
      var checkObject = function(expected) {
        var actual = result.filter(function(item) {
          return item.uid === expected.uid;
        });

        if (actual.length == 1) {
          actualList.push(diffObject(expected, actual[0]));
        } else {
          same = false;
          actual.forEach(function(item) {
            actualList.push(diffObject(expected, item));
          });
        }
      };

      if (Array.isArray(baseline)) {
        baseline.forEach(checkObject);
      } else {
        checkObject(baseline);
      }

      if (same) {
        grunt.log.ok();
      } else {
        grunt.log.error();
        failedCount++;
      }

      report += "## " + (!same ? "**[FAILED]** " : "") + name + "\n";
      report += "C#:\n```csharp\n" + grunt.file.read(path + "/source.cs") + "```\n";
      report += "Expected YAML:\n```yaml\n" + yaml.safeDump(baseline) + "```\n";
      report += "Actual YAML:\n```yaml\n" + yaml.safeDump(actualList.length == 1 ? actualList[0] : actualList) + "```\n";
    });

    if (failedCount == 0) {
      grunt.log.ok("All test cases passed.");
    }
    else {
      grunt.log.error(failedCount + " out of " + files.length + " cases failed.");
      grunt.log.error("Check " + testpath + "report.html for details.");
      var marked = require("marked");
      var template = grunt.file.read("report.tmpl");
      var htmlReport = grunt.template.process(template, { data: { body: marked(report) } });
      grunt.file.write(testpath + "report.html", htmlReport);
    }
  });

  grunt.registerTask('build', ['less:dev', 'exists:src', 'jshint']);
  
  // debug: reference to external js/css 
  grunt.registerTask('debug', ['build', 'concat', 'index_debug', 'clean:debug', 'copy:debug']);
  
  // release: merge referenced js/css to single file and minify
  grunt.registerTask('release', ['build', 'cssmin', 'uglify', 'index_release', 'clean:release', 'copy:release']);
  grunt.registerTask('test', ['debug', 'release', 'clean:test', 'copy:test']);
  
  grunt.registerTask('vsix', ['release', 'copy:vsix']);
  
  grunt.registerTask('server', ['debug', 'copy:test', 'connect:debug']);
  grunt.registerTask('serverrelease', ['release', 'copy:test', 'connect:release']);
  grunt.registerTask('test.min', ['debug', 'release', 'clean:test.min', 'copy:test.min']);
  grunt.registerTask('test.release.min', ['debug', 'release', 'clean:test.min', 'copy:test.min']);
  grunt.registerTask('default', ['test.min']);
};
