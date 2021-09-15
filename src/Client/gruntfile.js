/// <binding BeforeBuild='default' Clean='clean' ProjectOpened='watch' />
const sass = require("node-sass"),

    scriptsFolder = "wwwroot/scripts/",
    stylesFolder = "wwwroot/styles/",

    sourceJs = scriptsFolder + "app.js",
    outputJs = scriptsFolder + "app.min.js",
    outputJsMap = outputJs + ".map",

    sourceScss = stylesFolder + "app.scss",
    outputCss = stylesFolder + "app.min.css",
    outputCssMap = outputCss + ".map";

module.exports = function (grunt) {
    grunt.initConfig({
        clean: [outputCss, outputCssMap, outputJs, outputJsMap],
        uglify: {
            options: {
                mangle: true,
                sourceMap: true
            },
            app: {
                src: [sourceJs],
                dest: outputJs,
            }
        },
        sass: {
            options: {
                implementation: sass,
                outFile: outputCss,
                outputStyle: "compressed",
                sourceMap: outputCssMap
            },
            app: {
                files: { [outputCss]: sourceScss }
            }
        },
        watch: {
            scipts: {
                files: [sourceJs],
                tasks: ["uglify"]
            },
            styles: {
                files: [stylesFolder + "*.scss"],
                tasks: ["sass"]
            }
        }
    });
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.registerTask("default", ["uglify", "sass"]);
}