﻿#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static Json5.JSON5;

public class NpmPackageJson {
    [Fact] void Json5MatchesEquivalentJsonReference() => Parse(PackageJson5).Should().Be(JsonNode.Parse(PackageJson));

    [Fact] void JsonMatchesJsonReference() => Parse(PackageJson).Should().Be(JsonNode.Parse(PackageJson));

    private const string PackageJson5 =
        """
        {
          name: 'npm',
          publishConfig: {
            'proprietary-attribs': false,
          },
          description: 'A package manager for node',
          keywords: [
            'package manager',
            'modules',
            'install',
            'package.json',
          ],
          version: '1.1.22',
          preferGlobal: true,
          config: {
            publishtest: false,
          },
          homepage: 'http://npmjs.org/',
          author: 'Isaac Z. Schlueter <i@izs.me> (http://blog.izs.me)',
          repository: {
            type: 'git',
            url: 'https://github.com/isaacs/npm',
          },
          bugs: {
            email: 'npm-@googlegroups.com',
            url: 'http://github.com/isaacs/npm/issues',
          },
          directories: {
            doc: './doc',
            man: './man',
            lib: './lib',
            bin: './bin',
          },
          main: './lib/npm.js',
          bin: './bin/npm-cli.js',
          dependencies: {
            semver: '~1.0.14',
            ini: '1',
            slide: '1',
            abbrev: '1',
            'graceful-fs': '~1.1.1',
            minimatch: '~0.2',
            nopt: '1',
            'node-uuid': '~1.3',
            'proto-list': '1',
            rimraf: '2',
            request: '~2.9',
            which: '1',
            tar: '~0.1.12',
            fstream: '~0.1.17',
            'block-stream': '*',
            inherits: '1',
            mkdirp: '0.3',
            read: '0',
            'lru-cache': '1',
            'node-gyp': '~0.4.1',
            'fstream-npm': '0 >=0.0.5',
            'uid-number': '0',
            archy: '0',
            chownr: '0',
          },
          bundleDependencies: [
            'slide',
            'ini',
            'semver',
            'abbrev',
            'graceful-fs',
            'minimatch',
            'nopt',
            'node-uuid',
            'rimraf',
            'request',
            'proto-list',
            'which',
            'tar',
            'fstream',
            'block-stream',
            'inherits',
            'mkdirp',
            'read',
            'lru-cache',
            'node-gyp',
            'fstream-npm',
            'uid-number',
            'archy',
            'chownr',
          ],
          devDependencies: {
            ronn: 'https://github.com/isaacs/ronnjs/tarball/master',
          },
          engines: {
            node: '0.6 || 0.7 || 0.8',
            npm: '1',
          },
          scripts: {
            test: 'node ./test/run.js',
            prepublish: 'npm prune; rm -rf node_modules/*/{test,example,bench}*; make -j4 doc',
            dumpconf: 'env | grep npm | sort | uniq',
          },
          licenses: [
            {
              type: 'MIT +no-false-attribs',
              url: 'http://github.com/isaacs/npm/raw/master/LICENSE',
            },
          ],
        }
        """;

    private const string PackageJson =
        """
        {
          "name": "npm",
          "publishConfig": {
            "proprietary-attribs": false
          },
          "description": "A package manager for node",
          "keywords": [
            "package manager",
            "modules",
            "install",
            "package.json"
          ],
          "version": "1.1.22",
          "preferGlobal": true,
          "config": {
            "publishtest": false
          },
          "homepage": "http://npmjs.org/",
          "author": "Isaac Z. Schlueter <i@izs.me> (http://blog.izs.me)",
          "repository": {
            "type": "git",
            "url": "https://github.com/isaacs/npm"
          },
          "bugs": {
            "email": "npm-@googlegroups.com",
            "url": "http://github.com/isaacs/npm/issues"
          },
          "directories": {
            "doc": "./doc",
            "man": "./man",
            "lib": "./lib",
            "bin": "./bin"
          },
          "main": "./lib/npm.js",
          "bin": "./bin/npm-cli.js",
          "dependencies": {
            "semver": "~1.0.14",
            "ini": "1",
            "slide": "1",
            "abbrev": "1",
            "graceful-fs": "~1.1.1",
            "minimatch": "~0.2",
            "nopt": "1",
            "node-uuid": "~1.3",
            "proto-list": "1",
            "rimraf": "2",
            "request": "~2.9",
            "which": "1",
            "tar": "~0.1.12",
            "fstream": "~0.1.17",
            "block-stream": "*",
            "inherits": "1",
            "mkdirp": "0.3",
            "read": "0",
            "lru-cache": "1",
            "node-gyp": "~0.4.1",
            "fstream-npm": "0 >=0.0.5",
            "uid-number": "0",
            "archy": "0",
            "chownr": "0"
          },
          "bundleDependencies": [
            "slide",
            "ini",
            "semver",
            "abbrev",
            "graceful-fs",
            "minimatch",
            "nopt",
            "node-uuid",
            "rimraf",
            "request",
            "proto-list",
            "which",
            "tar",
            "fstream",
            "block-stream",
            "inherits",
            "mkdirp",
            "read",
            "lru-cache",
            "node-gyp",
            "fstream-npm",
            "uid-number",
            "archy",
            "chownr"
          ],
          "devDependencies": {
            "ronn": "https://github.com/isaacs/ronnjs/tarball/master"
          },
          "engines": {
            "node": "0.6 || 0.7 || 0.8",
            "npm": "1"
          },
          "scripts": {
            "test": "node ./test/run.js",
            "prepublish": "npm prune; rm -rf node_modules/*/{test,example,bench}*; make -j4 doc",
            "dumpconf": "env | grep npm | sort | uniq"
          },
          "licenses": [
            {
              "type": "MIT +no-false-attribs",
              "url": "http://github.com/isaacs/npm/raw/master/LICENSE"
            }
          ]
        }
        """;
}
