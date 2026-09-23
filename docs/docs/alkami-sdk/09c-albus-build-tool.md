# Albus Front-End Build Tool

**What this covers.** Albus is Alkami's componentized, npm-based build system for front-end assets (TypeScript, SCSS, Vue, webpack bundles) in ORB widgets and modules. This document consolidates the Albus Confluence pages: what Albus is, its anatomy (CLI, plugins, presets), the CLI commands, the `albus.config.js` configuration model, getting started and converting a legacy `gulpfile.js` project, each preset (copy, legacy, SCSS, Vue, Vue 3, webpack) with its default configuration, release notes and the upgrade guides that accompany them (CSSNano to CSSO, TSLint to ESLint, Vue deep selector), tips for combining presets, and Jest unit testing. All source pages were last updated 2023-10-11; the newest release documented is Albus 1.21.0 (2023-08-31).

## 1. What Albus is

Albus is an ecosystem of npm packages (scoped `@alkami`) that work together to produce front-end assets for experiences and interfaces on the Alkami platform. The CLI package exposes commands (build, deploy, watch, test, and so on) that execute actions from those packages through a task runner (currently Gulp). Each functional package is called a preset and is highly configurable.

In one sentence: Albus is Alkami's componentized build system for front-end assets that encapsulates Alkami's best practices and opinions for producing those assets.

There is an Albus training module on YouTube: https://youtu.be/3W14SNX-WJc (the "Albus Training Video" Confluence page itself is empty; it only embeds this video).

### Anatomy: terms unique to Albus

- **Albus-CLI** (`@alkami/albus-cli`): the command-line interface to Albus. Provides the core commands (`init`, `build`, `deploy`, `watch`, `test`) plus options and modifiers. Required in virtually every project that uses Albus.
- **Plugins**: encapsulated pieces of functionality that perform one specific action. Example: the sass plugin used by `albus-preset-sass` does the actual SCSS compilation.
- **Presets**: groups of plugins tailored to a use case. Each preset exposes methods the CLI can invoke, each running one or more tasks. Example: `albus-preset-legacy` transpiles TypeScript, compiles SCSS, and minifies JavaScript and CSS.

Sources: Albus (https://confluence.alkami.com/spaces/SDKC/pages/303531402); Anatomy (https://confluence.alkami.com/spaces/SDKC/pages/303531403); Albus Training Video (https://confluence.alkami.com/spaces/SDKC/pages/303531408)

## 2. CLI commands

All commands must be prefixed with `npx` (for example `npx albus transmute`). Every command has a "magic" name and conventional aliases; either works.

```
Usage: albus <command> [options] or
       albus <presetName> <scriptName>

Commands:
  albus scry            Information about your project        [aliases: info, v]
  albus catalyst [orbWidgetName] [clientFolderName] [areaFolderName] [projectNamespace]
                        Initialize your project.              [aliases: init, i]
  albus transmute       Build your project.                   [aliases: build, b]
  albus saturate        Deploy your project                   [aliases: deploy, d]
  albus substantiate    Test your project                     [aliases: test, t]
  albus evaporate       Clean your project                    [aliases: clean, c]
  albus panopticon      Watch changes in your project         [aliases: watch, w]

Options:
  -c, --configPath            Path to configuration file.  [string] [default: albus.config.js]
  -h, -?, --help              Show help                                     [boolean]
  -v, --version
  -C, --chdir                 Path to working directory.                     [string]
  -D, --debug                 Turns on (the much slower) debug mode.        [boolean]
  -S, --silent                Turns off all output except errors.           [boolean]
  -V, --verbose               Turns on all logging.                         [boolean]
  -t, --token                 specify token.<key> <value> to create key value pairs.
  -p, --preset                list of presets to use.                         [array]
  -P, --prod, --production    use production mode                           [boolean]
```

| Command | Aliases | Purpose |
|---|---|---|
| `catalyst` | `init`, `i` | Initializes an Albus project with a default setup and configuration based on the installed presets. Run without parameters and it tries to intuit them, prompting interactively for anything it cannot infer. Each preset ships its own init that scaffolds required files (see Additional Files). Re-running catalyst is also the automatic upgrade path for CSSO and ESLint migrations. |
| `transmute` | `build`, `b` | Builds front-end assets based on the enabled presets and plugins. Extra option: `-f, --force, --rebuild` forces a build of all files instead of checking for newer. |
| `saturate` | `deploy`, `d` | Deploys all files as configured in the Albus and preset configurations. Extra options: `-d, --deployPath` overrides the configured deploy path; `-f, --force, --redeploy` forces deploy of all files. |
| `panopticon` | `watch`, `w` | Sets up a file watcher for all configured files that recompiles and deploys when one changes. |
| `substantiate` | `test`, `t` | Runs the test frameworks bundled in presets (Jest). Extra options: `-w, --watch` (watch tests), `-W, --watchAll, --watch-all` (run all tests on change), `-g, --coverage` (code coverage statistics), `--e2e` (starts the miragejs dev-server and runs end-to-end tests; requires `albus-module-cypress`). |
| `evaporate` | `clean`, `c` | Cleans the project of any generated files the enabled presets created. |
| `scry` | `info`, `v` | Detailed information about Albus and its presets, including the fully hydrated configuration. This is the diagnostic tool for understanding issues. Options: `-c/--config`, `-C/--chdir`. |

### Catalyst arguments

```
$ albus catalyst [orbWidgetName] [clientFolderName] [areaFolderName] [projectNamespace]
```

- `orbWidgetName`: the widget name. Corresponds to the Area folder the widget deploys to, e.g. `C:/orb/WebClient/Areas/WidgetName/`.
- `clientFolderName`: most likely `WebClient` or `WebClientAdmin`.
- `areaFolderName`: usually `Areas`; modules get installed into `Modules` instead.
- `projectNamespace`: usually something like `Alkami.Apps.WidgetName`. Corresponds to the directory the project code lives in within the solution. Only used when you want an entire solution to reference one project (the old way).

The `-C` option is available on every command to point at a project folder, so you can stay in the solution directory (`npx albus panopticon -C your_project_folder`) rather than `cd` into the project.

Sources: Albus CLI Commands (https://confluence.alkami.com/spaces/SDKC/pages/303531404); Getting Started (https://confluence.alkami.com/spaces/SDKC/pages/303531406)

## 3. Configuration (`albus.config.js`)

Albus is opinionated but flexible: sane defaults exist in global and preset-specific scopes, and every value can be overridden per project. Create `albus.config.js` at the root of the project folder. Whenever an Albus command runs against the project, the file is read, merged with global and preset defaults, and executed.

Annotated full configuration:

```js
module.exports = {
  global: { // Options that are shared across presets
    base: '.',                       // Base path for writing build files (defaults to cwd)
    configPath: 'albus.config.js',   // Path to Albus configuration (exclusively used by the cli)
    debug: false,                    // Runs tasks in series and enables Debug flag in logs
    deployFiles: [],                 // Glob of static files to deploy
    deployIgnore: [],                // Glob of static files to ignore during a deploy
    deployPath: '/orb/<clientFolderName>/<areaFolderName>/<orbWidgetName>', // Path to dist folder
    projectPath: './<projectNamespace>', // Path to project folder
    rebuild: false,                  // Force rebuild of all assets when transmute is called
    redeploy: false,                 // Force redeploy of all assets when deploy is called
    verbosity: 'normal'              // Logging level: 'verbose', 'normal', 'silent'
  },
  tokens: { // Tokens are values that can be interpolated in other configuration values
    orbWidgetName: "TransferV2",     // Widget Name to be used for deployment
    clientFolderName: "WebClient",   // 'WebClient' or 'WebClientAdmin'
    areaFolderName: "Areas"          // 'Areas' or 'Modules' are most common
    // Additional custom tokens can be added for plugin configuration
  },
  presets: [], // Specify presets via string or require statement (empty array will search for albus-preset packages to include)
  presetConfiguration: { // All preset specific configuration goes here
    'presetName': {
      plugins: [],                   // Plugins to enable that are available in this preset
      pluginConfiguration: {         // Each plugin in a preset gets a section keyed by its name
        'pluginName': {
          pluginProp1: 'pluginProp1Value'
        }
      }
    }
  }
}
```

Tokens are interpolated into other configuration values using `<tokenName>` syntax (for example `<projectNamespace>/Scripts/**/*.js`). The `global.runType` property can be set to `'series'` to run presets in order instead of in parallel (see Copy preset).

Minimal config, often sufficient:

```js
module.exports = {
  tokens: {
    orbWidgetName: 'WidgetName'
  }
}
```

### Precedence and merge strategy

- Command-line options take priority over `albus.config.js`, which overrides the defaults.
- `global` and `tokens` use a simple merge (like `Object.assign`).
- `presets` overrides the default if specified.
- `presetConfiguration` uses a deep merge.
- `plugins` overrides the default if specified.
- Each named `pluginConfiguration` item is shallow-merged with your configuration. A plugin with a deep object configuration should dictate its own merge strategy.

Run `npx albus scry` to see the fully hydrated configuration.

Sources: Albus Configuration (https://confluence.alkami.com/spaces/SDKC/pages/303531405); Copy Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531413)

## 4. Getting started

### 4.1 Converting an existing gulpfile project: checklist

If the project already uses the legacy `gulpfile.js`, complete this checklist before the installation steps.

1. **Clear dependencies from `package.json`** at the solution root. Delete every entry in `dependencies` and `devDependencies` except packages beginning with `@types` (still needed for TypeScript support). Result looks like:

```json
{
  ...
  "dependencies": {},
  "devDependencies": {
    "@types/knockout": "3.4.1"
  },
  ...
}
```

2. **Delete legacy files/folders** from the solution directory if they exist: `gulpfile.js`, `package-lock.json`, `node_modules/`, `prebuild.ps1`.

3. **Add `@types` packages (optional).** If the solution has a `typings` folder (e.g. `typings/jquery/index.d.ts`, `typings/knockout/index.d.ts`), install the corresponding npm packages and then delete the `typings` folder entirely:

```
$ npm install @types/jquery --save-dev
$ npm install @types/knockout --save-dev
```

Search available definitions at https://microsoft.github.io/TypeSearch/.

4. Continue with "Install Albus" below.

### 4.2 Automated conversion script

Alternatively, with a committed, clean branch, run this in the repository root:

```
$ npm install @alkami/albus-convert-widget --no-save --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/
$ npx albus-convert-widget <your_project_folder>
```

`<your_project_folder>` is the project folder name in the solution. Review the output and check that Albus builds.

### 4.3 Common conversion errors

The page is a community list with one entry: make sure the references to JavaScript and CSS files in your views match the generated file names after the upgrade.

### 4.4 Fresh setup steps

1. **Check node and npm.** Major versions must be equal to or higher than:

```
$ node -v // => 14.x.x
$ npm -v  // => 6.x.x
```

Recommended installs: the `Alkami.DeveloperKit.Node` Chocolatey package for Windows (https://packagerepo.orb.alkamitech.com/feeds/choco.dev/Alkami.DeveloperKit.Employee.Node/1.1.0), or the Homebrew `node` formula on macOS.

2. **Create `package.json`** at the repository root if missing:

```
npm init private
```

Expected contents:

```json
{
  "private": true,
  "version": "0.0.0",
  "dependencies": {},
  "devDependencies": {},
  "scripts": {}
}
```

3. **Install the Albus CLI** from the private registry as a dev dependency:

```
$ npm install @alkami/albus-cli --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/ --save-dev
```

4. **Install the preset(s) you need** (one or more):

```
$ npm install @alkami/albus-preset-legacy --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/ --save-dev
$ npm install @alkami/albus-preset-sass --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/ --save-dev
$ npm install @alkami/albus-preset-vue --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/ --save-dev
$ npm install @alkami/albus-preset-webpack --@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/ --save-dev
```

Legacy replaces a gulpfile; SASS compiles SCSS alone or alongside another preset (e.g. webpack); Vue wraps a webpack flow for `.vue` files; webpack gives a modern bundling pipeline without Vue. (`@alkami/albus-preset-vue3` and `@alkami/albus-preset-copy` are also available; see section 5.)

5. **Initialize the project** from the solution directory:

```
$ npx albus catalyst -C your_project_folder
```

Catalyst scaffolds the files each installed preset needs and a base configuration. Answer the interactive prompts if it cannot infer values. The additional files it creates must be committed to the repository.

6. **Configure** `albus.config.js` as needed (section 3 and section 5).

7. **Test it out**:

```
$ npx albus transmute -C project_folder   # => Builds front end assets.
```

### 4.5 Manual project setup (what catalyst does)

1. Create `.npmrc` at the solution root (a peer of `package.json`):

```
# Specify the registry for Alkami scoped packages
@alkami:registry=https://packagerepo.orb.alkamitech.com/npm/npm.dev/
```

2. Install Albus and the CLI from the solution directory:

```
$ npm install @alkami/albus-cli @alkami/albus --save-dev
```

3. Install at least one preset (section 5).

4. Create `albus.config.js` inside the project folder. Without preset customization it can be just:

```js
module.exports = {
  tokens: {
    orbWidgetName: "Widget"
  },
  presetConfiguration: {}
}
```

5. TypeScript support (optional): create `tsconfig.json` in the project folder extending the correct preset base. The three variants differ only in the preset name in `extends`:

```json
{
  "extends": "../node_modules/@alkami/albus-preset-legacy/configs/tsconfig.json",
  "include": [
    "**/*.ts"
  ],
  "exclude": [
    "node_modules",
    "**/*.spec.ts",
    "**/__tests__",
    "**/__mocks__",
    "packages/**"
  ]
}
```

Use `@alkami/albus-preset-vue/configs/tsconfig.json` for the Vue preset and `@alkami/albus-preset-webpack/configs/tsconfig.json` for the webpack preset.

### 4.6 Additional files scaffolded by presets

These ship with the presets and are added to the project by `albus catalyst`. All go in the solution's root. Contents for manual creation:

**`tslint.json`** (linting for Vue and webpack projects; note this is the pre-ESLint file, see section 6.2):

```json
{
  "defaultSeverity": "warning",
  "extends": [
    "tslint:recommended"
  ],
  "linterOptions": {
    "exclude": [
      "node_modules/**"
    ]
  },
  "rules": {
    "quotemark": [true, "single"],
    "indent": [true, "spaces", 4],
    "interface-name": false,
    "ordered-imports": false,
    "object-literal-sort-keys": false,
    "no-consecutive-blank-lines": false,
    "no-console": false
  }
}
```

**`tsconfig.json`** (transpile TypeScript, inheriting from the preset base):

```json
{
  // Modify the line below with the correct preset you have installed
  "extends": "./node_modules/@alkami/albus-preset-{your-preset}/configs/tsconfig.json",
  "include": [
    "**/*.ts",
    "**/*.vue" // Only include this line if you are using the Vue preset
  ],
  "exclude": [
    "node_modules",
    "**/*.spec.ts",
    "**/__tests__",
    "**/__mocks__",
    "packages/**"
  ]
}
```

**`.browserslistrc`** (CSS autoprefixing for SCSS; the Additional Files page spells it `.browserlistrc`, the preset pages spell it `.browserslistrc`, which is the standard name):

```
extends @alkami/browserslist-config-albus
```

**`postcss.config.js`** (autoprefix and minify CSS). This is the original cssnano form; since Albus 1.9.0 the Vue preset uses CSSO instead (see section 6.1):

```js
module.exports = {
  plugins: {
    autoprefixer: {},
    cssnano: {
      preset: 'default',
    }
  }
};
```

### 4.7 Albus and Visual Studio (optional)

To run Albus on every Visual Studio build, edit the project's `.csproj` `<PreBuildEvent>` (or Project > Properties > Build Events > PreBuild) and prepend:

```
powershell.exe -noninteractive -command "npx --userconfig '$(SolutionDir).npmrc' --package @alkami/albus-vs-run@~1 albus-vs-run" '$(SolutionDir)' '$(ProjectDir)' '$(ConfigurationName)'
if %errorlevel% NEQ 0 exit %errorlevel%
```

The Getting Started page shows the same command without the trailing `'$(ConfigurationName)'` argument; the Manual Project Setup page includes it.

Sources: Getting Started (https://confluence.alkami.com/spaces/SDKC/pages/303531406); Existing Project Checklist (https://confluence.alkami.com/spaces/SDKC/pages/303531410); Common Conversion Errors (https://confluence.alkami.com/spaces/SDKC/pages/303531409); Manual Project Setup (https://confluence.alkami.com/spaces/SDKC/pages/303531411); Additional Files (https://confluence.alkami.com/spaces/SDKC/pages/303531407)

## 5. Presets

| Preset | Package | Use when |
|---|---|---|
| Legacy | `@alkami/albus-preset-legacy` | Converting a project that uses the ORB `gulpfile.js`; contains all its tasks. |
| SCSS | `@alkami/albus-preset-sass` | SCSS compilation on its own or with another preset (e.g. webpack). |
| Vue | `@alkami/albus-preset-vue` | Vue 2 applications; wraps a webpack flow compiling `.vue` files to JS and CSS. |
| Vue 3 | `@alkami/albus-preset-vue3` | Brand new front-end experiences using Vue 3. |
| Webpack | `@alkami/albus-preset-webpack` | Modern JS bundling with imports and tree shaking, without Vue. |
| Copy | `@alkami/albus-preset-copy` | Copying files (typically from `node_modules`) to their destination. |

Preset names used in `presets` and `presetConfiguration` are the short names: `legacy`, `sass`, `vue`, `webpack`, `copy`.

### 5.1 Copy preset

Install: `npm install @alkami/albus-preset-copy --save-dev`

Typical configuration:

```js
module.exports = {
  global: {
    runType: 'series'
  },
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['copy', /*other preset*/],
  presetConfiguration: {
    copy: {
      pluginConfiguration: {
        copy: {
          patterns: [
            {
              from: '../../node_modules/jquery',
              to: './lib/jquery',
              clean: [
                './lib/jquery/**/*.*',
              ]
            }
          ]
        }
      }
    }
  }
};
```

`runType: 'series'` makes presets run in the order listed in `presets` instead of in parallel; use it when a copy must happen before everything else. A real example lives in the ORB repo at `Alkami.Client/Alkami.Client.WebClient/albus.config.js` (Bitbucket APPDEV/orb).

### 5.2 Legacy preset

The Albus equivalent of the gulpfile. Tasks: minify JavaScript, minify CSS, bundle using the `bundle.json` convention, compile SCSS, transpile TypeScript.

Install: `npm install @alkami/albus-preset-legacy --save-dev`

Additional files (solution root): `tsconfig.json` and `tslint.json` if using TypeScript; `postcss.config.js` and `.browserslistrc` if using SCSS.

Plugins and their default configuration:

**`bundle-js`**: bundles all JS following the ORB `bundle.json` pattern.

```js
'bundle-js': {
  destination: '.',
  files: [
    '<projectNamespace>/Scripts/**/*.bundle/bundle.json'
  ],
  watch: [
    '<projectNamespace>/Scripts/**/*.bundle/bundle.json',
    '<projectNamespace>/Scripts/**/*.bundle/*.{js,min.js}',
    '!<projectNamespace>/Scripts/**/*.bundle/*.bundle.{js,min.js}'
  ],
  clean: [
    '<projectNamespace>/Scripts/**/*.bundle/*.bundle.{js,min.js}{,.map}'
  ],
  deploy: [
    '<projectNamespace>/Scripts/**/*.bundle/*.bundle.{js,min.js}{,.map}'
  ]
}
```

**`minify-js`**: minifies standard JS files using uglify-es.

```js
'minify-js': {
  destination: '.',
  files: [
    '<projectNamespace>/Scripts/**/*.js',
    '!<projectNamespace>/Scripts/**/*.bundle/*.js',
    '!<projectNamespace>/Scripts/**/*{.min,-ts}.js'
  ],
  clean: [
    '<projectNamespace>/Scripts/**/*.min.js{,.map}',
    '!<projectNamespace>/Scripts/**/*.bundle/*.min.js{,.map}',
    '!<projectNamespace>/Scripts/**/*-ts.js{,.map}'
  ],
  deploy: [
    '<projectNamespace>/Scripts/**/*.min.js{,.map}',
    '!<projectNamespace>/Scripts/**/*.bundle/*.min.js{,.map}',
    '!<projectNamespace>/Scripts/**/*-ts.js{,.map}'
  ],
  watch: [
    '<projectNamespace>/Scripts/**/*.js',
    '!<projectNamespace>/Scripts/**/*.bundle/*.js',
    '!<projectNamespace>/Scripts/**/*{.min,-ts}.js'
  ]
}
```

**`compile-sass`**: uses `albus-preset-sass` to compile SCSS to CSS.

```js
'compile-sass': {
  sassOptions: {              // options to pass to the sass compiler
    outputStyle: 'compressed'
  },
  destination: '.',
  files: [
    '<projectNamespace>/Styles/**/*.scss'
  ],
  clean: [
    '<projectNamespace>/Styles/**/*.min.css{,.map}'
  ],
  deploy: [
    '<projectNamespace>/Styles/**/*.min.css{,.map}'
  ],
  watch: [
    '<projectNamespace>/Styles/**/*.scss'
  ]
}
```

The page refers to node-sass options (https://github.com/sass/node-sass#options); note that since Albus 1.5.0 all SCSS implementations use dart-sass instead of node-sass.

**`compile-ts`**: transpiles TypeScript and minifies the output.

```js
'compile-ts': {
  tsconfigPath: 'tsconfig.json', // provide location of tsconfig.json (v1.7.0+)
  minify: true,                  // minify output using uglify
  suffix: null,                  // add a suffix. To support a codebase that uses something like '-ts'
  files: [],                     // manually specify files to build instead of inferring from tsconfig
  destination: '.'               // where the files end up on build
}
```

Reference configuration (remove plugins you do not need; change globs if JS is not under `Scripts`):

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['legacy'],
  presetConfiguration: {
    'legacy': {
      plugins: [
        'bundle-js',
        'compile-sass',
        'compile-ts',
        'minify-js'
      ],
      pluginConfiguration: {
        'bundle-js': {},
        'compile-sass': {},
        'compile-ts': {},
        'minify-js': {},
      }
    }
  }
};
```

### 5.3 SCSS preset

For compiling SCSS separately from the legacy preset; works alongside the webpack preset.

Install: `npm install @alkami/albus-preset-sass --save-dev`

Additional files (solution root): `postcss.config.js`, `.browserslistrc`.

Single plugin, `compile-sass`, with these defaults (identical globs to the legacy `compile-sass` plugin):

```js
presetConfiguration: {
  'sass': {
    pluginConfiguration: {
      'compile-sass': {
        sassOptions: {
          outputStyle: 'compressed'
        },
        destination: '.',
        files: [
          '<projectNamespace>/Styles/**/*.scss'
        ],
        clean: [
          '<projectNamespace>/Styles/**/*.min.css{,.map}'
        ],
        deploy: [
          '<projectNamespace>/Styles/**/*.min.css{,.map}'
        ],
        watch: [
          '<projectNamespace>/Styles/**/*.scss'
        ]
      }
    }
  }
}
```

### 5.4 Vue preset (Vue 2)

Handles compiling and optimizing the JavaScript produced from Vue single file components: precompiles templates, splits code into shared chunks, extracts component CSS into a separate file, minifies output, and optionally auto-injects script tags into a `.cshtml` file.

Install:

```
npm install @alkami/albus-preset-vue --save-dev
npm install @alkami/iris-vue --save-dev   // dev dependency, used for unit tests only; Iris Vue is loaded from the CDN at runtime
```

Additional files (solution root): `tsconfig.json` and `tslint.json` (now `.eslintrc.js`, section 6.2) if using TypeScript; `postcss.config.js` and `.browserslistrc` if using SCSS.

**Including Vue and Iris Vue.** The webpack configuration strips out the `vue`, `vuex`, and `vue-router` libraries, so they must be referenced in the `.cshtml` view. To load Iris Vue, install the `Alkami.WebAssets.Helpers` NuGet package (https://packagerepo.orb.alkamitech.com/feeds/nuget.dev/Alkami.WebAssets.Helpers) and use its helper methods (docs: https://confluence.alkami.com/display/UDT/Including+Iris+Vue+in+your+widget).

```cshtml
// Required in WebClientAdmin
@using Alkami.Client.Framework.Utility
@using Alkami.Client.WebClient.Shared.Helpers

// Desktop
@section StyleSheetContentPlaceholder {
    @* Load Iris Vue stylesheets *@
    @Html.IrisVueLinksSnippet()
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vuex/vuex.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.min.js")

    @* Load Iris Vue component library *@
    @Html.IrisVueScriptsSnippet()
}

// Mobile
@section StyleSheets {
    @* For mobile, pass the 'device: "mobile"' named argument to load the mobile specific shim file *@
    @Html.IrisVueLinksSnippet(device: "mobile")
}

@section BodyScripts {
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vuex/vuex.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.min.js")

    @* Load Iris Vue component library *@
    @Html.IrisVueScriptsSnippet()
}
```

Vue devtools only work with the unminified library; see section 5.7.

**`webpack` plugin defaults** (the preset's only plugin; it compiles TypeScript and SCSS, precompiles `.vue` templates, splits chunks, and minifies):

```js
'webpack': {
  autoInject: null,   // See autoinject section
  config: {},         // Custom webpack config that overrides the default
  deploy: null,       // Where to deploy the resulting files to
  entry: {            // Register main application files (e.g., app.ts)
    app: path.resolve('<projectNamespace>/Scripts/app.ts')
  },
  outputPath: {       // Output directory of resulting files
    js: path.resolve('<projectNamespace>/Scripts/'),
    css: path.resolve('<projectNamespace>/Styles/'),
  },
  // Only used for JavaScript testing
  testConfigPath: path.resolve(path.join(__dirname, '../../configs/jest.config.js')),
}
```

**Basic configuration.** With a single root file named `app.ts` in `<projectNamespace>/Scripts` and no autoinject, only tokens are required:

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.WidgetName',
    orbWidgetName: 'WidgetName'
  }
}
```

**Multiple entry points.** `entry` maps to webpack's entry configuration; keys are entry names (arbitrary, `desktop` and `mobile` are not reserved) and values are paths:

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['vue'],
  presetConfiguration: {
    'vue': {
      plugins: ['webpack'],
      pluginConfiguration: {
        'webpack': {
          entry: {
            desktop: './Scripts/desktopScriptName.ts',
            mobile: './Scripts/Mobile/mobileScriptName.ts',
          },
        },
      },
    },
  },
};
```

**Autoinject.** Webpack identifies vendor files (imported from `node_modules`) and common chunks (shared across entry points) and emits files named `<type>~<entry_name(s)>.min.js`, e.g. `common~desktop.min.js`, `common~desktop~mobile.min.js`, `vendors~desktop.min.js`, `vendors~desktop~mobile.min.js`. Because the chunk set is only known at compile time, `autoInject` lets webpack write the references into a cshtml template. One object per configured entry:

```js
autoInject: [
  {
    chunks: ['desktop'],
    htmlFilePath: '<projectNamespace>/Views/index.desktop.cshtml',
    htmlTemplateFilePath: '<projectNamespace>/Views/index.desktop.template.cshtml',
  },
],
```

- `chunks`: entry name(s) whose chunks to inject.
- `htmlFilePath`: output path of the generated view with references injected.
- `htmlTemplateFilePath`: path of the template webpack reads.

Add injection-point HTML comments inside the script/style sections of the template cshtml:

```cshtml
<!-- Desktop -->
@section StyleSheetContentPlaceholder{
    <!-- css:inject:start -->
    <!-- css:inject:end -->
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.min.js")

    <!-- js:inject:start -->
    <!-- js:inject:end -->
}

<!-- Mobile -->
@section StyleSheets {
    <!-- css:inject:start -->
    <!-- css:inject:end -->
}

@section BodyScripts {
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.min.js")

    <!-- js:inject:start -->
    <!-- js:inject:end -->
}
```

After `npx albus transmute` the generated file looks like:

```cshtml
@section StyleSheetContentPlaceholder{
    <!-- css:inject:start -->
    @Html.CssWithCacheExpiration("/Areas/Widget/Styles/app.min.css")
    <!-- css:inject:end -->
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.IncludeSiteTextScript()

    <!-- js:inject:start -->
    @Html.ScriptWithCacheExpiration("/Areas/Widget/Scripts/vendors~desktop~mobile.min.js")
    @Html.ScriptWithCacheExpiration("/Areas/Widget/Scripts/vendors~desktop.min.js")
    @Html.ScriptWithCacheExpiration("/Areas/Widget/Scripts/desktop.min.js")
    <!-- js:inject:end -->
}
```

Full autoinject configuration:

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['vue'],
  presetConfiguration: {
    'vue': {
      plugins: ['webpack'],
      pluginConfiguration: {
        'webpack': {
          entry: {
            desktop: './Scripts/main.ts',
            mobile: './Scripts/Mobile/main.ts',
          },
          autoInject: [
            {
              chunks: ['desktop'],
              htmlFilePath: './Views/index.desktop.cshtml',
              htmlTemplateFilePath: './Views/index.desktop.template.cshtml',
            },
            {
              chunks: ['mobile'],
              htmlFilePath: './Views/index.mobile.cshtml',
              htmlTemplateFilePath: './Views/index.mobile.template.cshtml',
            },
          ],
        },
      },
    },
  },
};
```

### 5.5 Vue 3 preset

Same capabilities as the Vue preset plus aid for migrating from Vue 2 to Vue 3 (via the Vue compat build), a mock/dev-server environment with routes, and externals injection.

Install: `npm install @alkami/albus-preset-vue3 --save-dev`

Additional files (solution root): `tsconfig.json` and `.eslintrc.js` if using TypeScript; `postcss.config.js` and `.browserslistrc` if using SCSS.

**Including Vue 3 and Iris Vue.** The webpack config strips `vue`, `vuex`, and `vue-router`; reference them in the view, preferably through autoinject. Iris Vue is loaded via `Alkami.WebAssets.Helpers` as in the Vue preset. Desktop example from the page (the mobile section on the page still shows the Vue 2 paths):

```cshtml
// Required in WebClientAdmin
@using Alkami.Client.Framework.Utility
@using Alkami.Client.WebClient.Shared.Helpers

@section StyleSheetContentPlaceholder {
    @Html.IrisVueLinksSnippet()
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.ScriptWithCacheExpiration("~/lib/vue-compat/vue.runtime.global.js", null, "vue-3")
    @Html.ScriptWithCacheExpiration("~/lib/vue-demi/index.iife.js", null, "official")
    @Html.ScriptWithCacheExpiration("~/lib/pinia/pinia.iife.js", null, "official")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.global.js", null, "vue-3")

    @Html.IrisVueScriptsSnippet()
}
```

**`webpack` plugin defaults (Vue 3):**

```js
'webpack': {
  autoInject: null,
  config: {},
  deploy: null,
  entry: {
    app: path.resolve('<projectNamespace>/Scripts/app.ts')
  },
  outputPath: {
    js: path.resolve('<projectNamespace>/Scripts/'),
    css: path.resolve('<projectNamespace>/Styles/'),
  },
  cloudfrontUri: 'https://assets.orb.alkamitech.com/', // Where the CDN resides
  stage: 'prod',                // The environment to utilize
  externals: {},                // The Third Party Scripts available from the CDN
  injectedExternals: [          // injected scripts into Mock and AutoInject
    'vue',
    'vue-demi',
    'pinia',
    'vue-router',
    'miragejs',
  ],
  routes: [                     // Routes created inside the mock environment
    {
      path: '/',                // Route path in the browser
      entry: 'app',             // What entry to add to the mock
      layout: 'desktop',        // Which mock layout to use 'desktop', or 'mobile'
    },
    {
      path: '/Mobile/',
      entry: 'app',
      layout: 'mobile',
    },
  ],
  vueCompatBuild: true,         // utilize Vue Migration Build instead of the standard
  vueCompatMode: 2,             // switch between 2 or 3. only works when vueCompatBuild is true
  siteTextPath: './_SiteText/*.sitetext.en.xml', // glob to find xml to inject SiteText into mocks
  testConfigPath: path.resolve(path.join(__dirname, '../../configs/jest.config.js')),
}
```

Basic configuration and multiple entry points work exactly as in the Vue preset (the page's examples use `presets: ['vue']`; the preset package is `albus-preset-vue3`).

**Autoinject with externals.** The Vue 3 template adds an `externals:js:inject` block for the `injectedExternals` list:

```cshtml
<!-- Desktop -->
@section StyleSheetContentPlaceholder{
    <!-- css:inject:start -->
    <!-- css:inject:end -->
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.IncludeSiteTextScript()

    <!-- externals:js:inject:start -->
    <!-- externals:js:inject:end -->

    @Html.IrisVueScriptsSnippet()

    <!-- js:inject:start -->
    <!-- js:inject:end -->
}

<!-- Mobile -->
@section StyleSheets {
    <!-- css:inject:start -->
    <!-- css:inject:end -->
}

@section BodyScripts {
    @Html.IncludeSiteTextScript()

    <!-- externals:js:inject:start -->
    <!-- externals:js:inject:end -->

    @Html.IrisVueScriptsSnippet()

    <!-- js:inject:start -->
    <!-- js:inject:end -->
}
```

Generated output example:

```cshtml
@section StyleSheetContentPlaceholder{
    <!-- css:inject:start -->
    @Html.CssWithCacheExpiration("/Areas/Widget/Styles/app.min.css")
    <!-- css:inject:end -->
}

@section JavaScriptIncludeContentPlaceHolder {
    @Html.IncludeSiteTextScript()

    <!-- externals:js:inject:start -->
    @Html.ScriptWithCacheExpiration("~/lib/vue-compat/vue.runtime.global.js", null, "vue-3")
    @Html.ScriptWithCacheExpiration("~/lib/vuex/vuex.global.js", null, "vue-3")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.global.js", null, "vue-3")
    @Html.ScriptWithCacheExpiration("~/lib/miragejs/mirage-umd.js", null, "official")
    <!-- externals:js:inject:end -->

    @Html.IrisVueScriptsSnippet()

    <!-- js:inject:start -->
    @Html.ScriptWithCacheExpiration("~/Areas/Example/Scripts/vendors.min.js")
    @Html.ScriptWithCacheExpiration("~/Areas/Example/Scripts/app.min.js")
    <!-- js:inject:end -->
}
```

Full Vue 3 autoinject configuration:

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['vue'],
  presetConfiguration: {
    'vue': {
      plugins: ['webpack'],
      pluginConfiguration: {
        'webpack': {
          entry: {
            desktop: './Scripts/main.ts',
            mobile: './Scripts/Mobile/main.ts',
          },
          injectedExternals: [
            'vue',
            'vuex',
            'vue-router',
            'miragejs',
          ],
          routes: [
            {
              path: '/',
              entry: 'desktop',
              layout: 'desktop',
            },
            {
              path: '/Mobile/',
              entry: 'mobile',
              layout: 'mobile',
            },
          ],
          autoInject: [
            {
              chunks: ['desktop'],
              htmlFilePath: './Views/index.desktop.cshtml',
              htmlTemplateFilePath: './Views/index.desktop.template.cshtml',
            },
            {
              chunks: ['mobile'],
              htmlFilePath: './Views/index.mobile.cshtml',
              htmlTemplateFilePath: './Views/index.mobile.template.cshtml',
            },
          ],
        },
      },
    },
  },
};
```

### 5.6 Webpack preset

Modern code bundling for non-Vue widgets: bundles everything a file needs, removes unused code, allows ES6 `import` syntax (so typings resolve without hand-written definition files), and minifies output.

Install: `npm install @alkami/albus-preset-webpack --save-dev`

Additional files (solution root): `tsconfig.json` and `tslint.json` (now `.eslintrc.js`) if using TypeScript; `postcss.config.js` and `.browserslistrc` if using SCSS.

Single plugin, `webpack`, defaults:

```js
'webpack': {
  config: {},
  deploy: null,
  entry: {
    app: path.resolve('<projectNamespace>/Scripts/app.ts')
  },
  outputPath: path.resolve('<projectNamespace>/Scripts/'),
  testConfigPath: path.resolve(path.join(__dirname, '../../configs/jest.config.js'))
}
```

Basic configuration (single `app.ts` in `<projectNamespace>/Scripts`) needs only the tokens, as with Vue. Multiple entry points (note the entry paths on this page include `<projectNamespace>`):

```js
module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['webpack'],
  presetConfiguration: {
    'webpack': {
      plugins: ['webpack'],
      pluginConfiguration: {
        'webpack': {
          entry: {
            desktop: './<projectNamespace>/Scripts/desktopScriptName.ts',
            mobile: './<projectNamespace>/Scripts/Mobile/mobileScriptName.ts',
          },
        },
      },
    },
  },
};
```

**Custom webpack configuration.** The `config` key is merged with the preset's base settings using webpack-merge (https://www.npmjs.com/package/webpack-merge); any option from https://webpack.js.org/configuration is supported:

```js
pluginConfiguration: {
  'webpack': {
    config: {
      output: {
        filename: '[name].[hash:8].min.js',
      },
    },
  },
},
```

Environment-specific configuration: add a `production` or `development` key inside `config` and it merges only into the corresponding environment (`-P/--production` selects production mode):

```js
pluginConfiguration: {
  'webpack': {
    config: {
      development: {
        output: {
          filename: '[name].development.min.js',
        },
      },
      production: {
        output: {
          filename: '[name].production.min.js',
        },
      },
    },
  },
},
```

### 5.7 Vue devtools

Vue devtools only work with the unminified library, and views must reference the minified file for production. Two options:

1. **`ForceUseOfProductionAssetsInDebug`** in the `web.config` of WebClient. When a script or stylesheet is referenced via an HTML helper such as `@Html.ScriptWithCacheExpiration`, the platform forces or removes the `.min` extension if the other version exists on disk. Set it to `false` while developing to serve unminified Vue libraries (and all other scripts).

2. **Autoinject templating.** Before injecting, the inject plugin runs the template through lodash templating with the full webpack configuration as the model, so the extension can depend on `webpackConfig.mode`:

```cshtml
@section JavaScriptIncludeContentPlaceHolder {
    <!-- Block Syntax -->
    <% if (webpackConfig.mode === 'production') { %>
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vuex/vuex.min.js")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.min.js")
    <% } else { %>
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime.js")
    @Html.ScriptWithCacheExpiration("~/lib/vuex/vuex.js")
    @Html.ScriptWithCacheExpiration("~/lib/vue-router/vue-router.js")
    <% } %>

    <!-- Single line syntax -->
    @Html.ScriptWithCacheExpiration("~/lib/vue/vue.runtime<%= webpackConfig.mode === 'development' ? '' : '.min' %>.js")
}
```

### 5.8 Aliasing source paths (`@/`)

To use `import MyComponent from '@/components/MyComponent.vue';` instead of relative paths, configure both TypeScript and webpack.

`tsconfig.json`: add `baseUrl` and `paths`:

```json
{
  "extends": "./node_modules/@alkami/albus-preset-vue/configs/tsconfig.json",
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": [
        "Scripts/*"
      ]
    },
  },
  "include": [
    "Scripts/**/*.ts",
    "Scripts/**/*.vue"
  ],
  "exclude": [
    "node_modules",
    "**/*.spec.ts",
    "**/__tests__",
    "**/__mocks__",
    "packages/**"
  ]
}
```

`albus.config.js`: add `resolve.alias` in the webpack `config` (any number of aliases allowed):

```js
const path = require('path');

module.exports = {
  tokens: {
    projectNamespace: 'Alkami.Apps.Widget',
    orbWidgetName: 'Widget'
  },
  presets: ['vue'],
  presetConfiguration: {
    'vue': {
      plugins: ['webpack'],
      pluginConfiguration: {
        'webpack': {
          entry: {
            desktop: './Scripts/app.ts'
          },
          config: {
            resolve: {
              alias: {
                '@': path.resolve(process.cwd(), './Scripts'),
              },
            },
          },
        },
      },
    },
  },
}
```

Sources: Presets (https://confluence.alkami.com/spaces/SDKC/pages/303531412); Copy Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531413); Legacy Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531414); SCSS Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531415); Vue 3 Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531416); Vue Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531417); Aliasing Source Paths (https://confluence.alkami.com/spaces/SDKC/pages/303531418); Using Vue Devtools (https://confluence.alkami.com/spaces/SDKC/pages/303531419); Webpack Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531420)

## 6. Release notes and upgrade guides

### 6.1 CSSNano replaced by CSSO (Albus 1.9.0)

Since Albus 1.9.0 the Vue preset uses CSSO instead of CSSNano for CSS minification. The project's PostCSS configuration must be updated.

Automatic: after updating Albus dependencies, re-run catalyst in the repo root and commit the changed `postcss.config.js`:

```
npx albus catalyst -C Alkami.Client.Widget.MyWidget
```

Manual: replace `repo-root/Alkami.Client.Widget.MyWidget/postcss.config.js` with:

```js
module.exports = {
  plugins: {
    autoprefixer: {},
    'postcss-csso': {
      restructure: false,
    },
  },
};
```

### 6.2 TSLint to ESLint

TSLint was deprecated in Albus 1.4.0 and support has since been removed entirely. A leftover `tslint.json` keeps producing warnings until deleted.

Automatic: re-run catalyst after upgrading Albus dependencies; it removes all `tslint.json` files and adds `.eslintrc.js` files. Commit both changes.

```
npx albus catalyst -C Alkami.Client.Widget.MyWidget
```

Manual: delete `tslint.json`, then create `.eslintrc.js` in the solution directory (`repo-root/Alkami.Client.Widget.MyWidget/.eslintrc.js`) extending one of the base configs. The config packages are installed as dependencies of the presets; nothing extra to install.

Vue preset, standard:

```js
module.exports = {
  'extends': '@alkami/eslint-config-vue-standard',
  'parserOptions': {
    'tsconfigRootDir': __dirname
  }
};
```

Webpack preset, standard:

```js
module.exports = {
  'extends': '@alkami/eslint-config-typescript-standard',
  'parserOptions': {
    'tsconfigRootDir': __dirname
  }
};
```

"Unsafe" (more lax) variants exist for cases that must deviate from Alkami development standards: `@alkami/eslint-config-vue-unsafe` (Vue preset) and `@alkami/eslint-config-typescript-unsafe` (webpack preset), used with the identical `.eslintrc.js` shape.

### 6.3 Vue deep selector (`>>>` to `::v-deep`)

Newer Dart Sass versions emit:

```
Deprecation Warning: The selector " > > > " is invalid CSS. It will be omitted from the generated CSS.
```

Replace all `>>>` usages in Sass styles with `::v-deep`. Albus 1.21.0 lists this as a breaking change: the `>>>` hack Vue relied on is no longer supported now that CSS nesting is standardized.

### 6.4 Changelog highlights

Full changelog follows standard-version conventions. Notable entries (newest first):

- **1.21.0 (2023-08-31)**: dependency updates across albus, albus-cli, albus-util, cypress module, vue and legacy presets to safe versions; TypeScript peer dependency reined in to actual support; vue2-preset respects Orb version for CDN calls; legacy cypress uses the CDN routes helper instead of local Orb. Breaking: `>>>` to `::v-deep`.
- **1.20.0 (2023-03-28)**: `preset-cdn` introduces a CDN deploy plugin to push CDN package files to a local integrated environment.
- **1.19.x (2022-12 to 2023-01)**: admin layout mock support; environment variables always strings; miragejs pulls from the correct global; iris-vue and CSS fixes during watch.
- **1.18.0 (2022-12-12)**: introduces a vue2 preset based off the vue3 preset code; Cypress tests run in parallel.
- **1.17.0 (2022-10-05)**: preset-vue3 makes creating `.d.ts` files a configurable option (default false).
- **1.16.x (2022-09/10)**: Vue 3 support for Albus; devServer built out; inject plugin gains vendor support; ESLint rules upgraded; `ag-grid-vue` added to externals; CSS not exported as modules.
- **1.15.0 (2022-05-09)**: legacy cypress module can test mobile views; legacy preset builds sass when testing for cypress.
- **1.14.0 (2022-04-19)**: Cypress gains Istanbul instrumentation and NYC coverage output.
- **1.13.0 (2022-03-02)**: cypress `orbVersion` optional; browserslist updated for Android and IE.
- **1.11.0 (2021-10-27)**: all TypeScript implementations updated to ES2019; plugins can read the entire Albus configuration; CLI args exposed in global configuration.
- **1.10.0 (2021-08-23)**: non-Vue Cypress runner; module-cypress updated to Cypress 7.x.x.
- **1.9.0 (2021-04-28)**: copy plugin becomes a supported preset for use outside Orb; preset-vue switches CSS minification from cssnano to csso.
- **1.8.0 (2021-04-01)**: warning when no files are processed by legacy plugins; Windows path encoding fix for Razor injection helpers; ESLint config fixes.
- **1.7.0 (2021-03-18)**: preset-legacy can specify the tsconfig file used for compilation (`tsconfigPath`).
- **1.6.1 (2021-02-09)**: preset-vue turns off minification of cshtml templates when using auto-inject.
- **1.6.0 (2020-12-01)**: css-loader no longer rewrites urls; postcss@8.x.x dependency added to preset-vue; module-cypress can target a specific Orb version.
- **1.5.0 (2020-11-03)**: shared ESLint configurations for TypeScript and Vue projects; preset-vue and preset-webpack gain ESLint support; all SCSS implementations switched from node-sass to dart-sass; vs-run squelches package output to avoid false msbuild failures.
- **1.4.0 (2020-09-24)**: autoinject plugin upgraded; `--deployPath` passed to panopticon. (TSLint deprecation begins here.)
- **1.2.0 (2020-08-10)**: preset-webpack supports multiple entry files; altered sass partials trigger rebuilds; logging moved to `@alkami/flamel`; preset-vue gains Cypress e2e testing.
- **1.1.0 (2020-03-10)**: preset-vue init adds a `vue-shims.d.ts` file.
- **1.0.0 (2019-12-05)**: official release.

Sources: Release Notes (https://confluence.alkami.com/spaces/SDKC/pages/303531421); CSSNano Plugin Removal (https://confluence.alkami.com/spaces/SDKC/pages/303531422); TSLint Deprecation Warnings Solution: Upgrading to ESLint from TSLint (https://confluence.alkami.com/spaces/SDKC/pages/303531423); Vue Deep Selector (https://confluence.alkami.com/spaces/SDKC/pages/303531424)

## 7. Tips and tricks

### 7.1 Combining the legacy and Vue presets

For gradually introducing Vue while leaving existing code alone. Assumes `@alkami/albus-preset-legacy` and `@alkami/albus-preset-vue` are installed and `npx albus catalyst` has run.

`tsconfig.json` in the project folder must extend the Vue preset base, not the legacy one:

```json
{
  "extends": "../node_modules/@alkami/albus-preset-vue/configs/tsconfig.json",
  "include": [
    "**/*.ts",
    "**/*.vue"
  ],
  "exclude": [
    "node_modules",
    "**/*.spec.ts",
    "packages/**"
  ]
}
```

**No legacy TypeScript.** Drop `compile-ts` from the legacy plugins (and `bundle-js` too if there are no `[something].bundle.json` bundles). This compiles all sass, minifies JavaScript, and compiles the default Vue entry point at `Scripts/app.ts`:

```js
presets: ['legacy', 'vue'],
presetConfiguration: {
  legacy: {
    plugins: [
      'bundle-js',
      'compile-sass',
      'minify-js',
    ]
  },
  vue: {},
}
```

**A few legacy TypeScript files.** Add them as extra webpack entry points in the Vue configuration instead of using the legacy TypeScript plugin:

```js
presets: ['legacy', 'vue'],
presetConfiguration: {
  legacy: {
    plugins: [
      'bundle-js',
      'compile-sass',
      'minify-js',
    ]
  },
  vue: {
    webpack: {
      entry: {
        'app': './Scripts/app.ts',
        'scriptname-ts': './Scripts/scriptname.ts',
        'path/to/scriptname-ts': './Scripts/path/to/scriptname.ts',
      },
    }
  },
}
```

(The page nests `webpack` directly under `vue`; elsewhere in the docs it is nested under `vue.pluginConfiguration.webpack`.)

**Lots of legacy TypeScript.** Keep the legacy `compile-ts` plugin but craft a glob that excludes the Vue-related TypeScript, following the Vue project anatomy guidelines (Confluence pageId=78847239):

```js
presets: ['legacy', 'vue'],
presetConfiguration: {
  legacy: {
    plugins: [
      'bundle-js',
      'compile-sass',
      'minify-js',
      'compile-ts'
    ],
    'compile-ts': {
      files: [
        './Scripts/**/*.ts',
        '!./Scripts/+(api|components|directives|filters|interfaces|store|styles|utils|views)/**/*.ts',
        '!./Scripts/+(app|router|shims-vue.d).ts',
      ],
      suffix: '-ts',
    }
  },
  vue: { },
}
```

### 7.2 Create your own preset, and npm scripts

Both "Create Your Own Preset" and "Using npm Scripts to Execute Albus" pages contain only a TODO placeholder; no content exists. (The CLI usage line `albus <presetName> <scriptName>` indicates presets can expose named scripts, but this is not documented further.)

Sources: Tips and Tricks (https://confluence.alkami.com/spaces/SDKC/pages/303531425); Combining Legacy and Vue Presets (https://confluence.alkami.com/spaces/SDKC/pages/303531426); Create Your Own Preset (https://confluence.alkami.com/spaces/SDKC/pages/303531427); Using npm Scripts to Execute Albus (https://confluence.alkami.com/spaces/SDKC/pages/303531428)

## 8. Unit testing

With the webpack or Vue preset, unit testing works out of the box. `npx albus test` finds test files by glob and runs them with Jest.

| Preset | Glob |
|---|---|
| Webpack | `**/__tests__/**/*.[jt]s?(x)` and `**/?(*.)+(spec|test).[jt]s?(x)` |
| Vue | `**/__tests__/*.[jt]s` and `**/tests/unit/**/*.spec.[jt]s` |

Typical structure:

```
├── jest.config.js          # Optional Jest Configuration
└── tests                   # Jest setup files and utility scripts
    ├── globalSetup.js      # Optional
    ├── data                # Data mocks for various tests
    │   ├── payees.ts
    │   ├── accounts.ts
    │   └── ...
    ├── unit                # Test folders for various categories of tests
    │   ├── utils
    │   │   ├── translation.spec.ts   # Named after the source file with ".spec" suffix
    │   │   ├── localStorage.spec.ts
    │   │   └── ...
    │   └── ...
    └── ...
```

Writing a test is three steps: export the function from its file, import it in the test file, write test cases.

```ts
// currency.ts
export function parseCurrency(input: string): number {
  const negated = /-[\$0-9\.,]+|\([\$0-9\.,]+\)/.test(input);
  const sanitized = (negated ? '-' : '') + input.replace(/[^0-9\.]+/g, '');

  return parseFloat(sanitized);
}
```

```ts
// currency.spec.ts
import { parseCurrency } from '../../util/currency';

test('parses simple currency string', () => {
  expect(parseCurrency('$345.23')).toBe(345.23);
});

test('parses currency string with commas', () => {
  expect(parseCurrency('$23,345.23')).toBe(23345.23);
});

test('parses parenthesis currency string', () => {
  expect(parseCurrency('($0.00)')).toBe(-0.00);
});

test('parses negative currency string', () => {
  expect(parseCurrency('-$23,345.23')).toBe(-23345.23);
});
```

Run with `npx albus test`; by default only pass/fail is shown. For coverage, create `jest.config.js` at the root of the widget:

```js
module.exports = {
  collectCoverage: true,
  collectCoverageFrom: [
    'scripts/**/*.{ts,vue}',
    '!scripts/api/**',
    '!scripts/shims-vue.d.ts', // TypeScript shim file
    '!**/node_modules/**',
  ],
  coverageReporters: ['html', 'text-summary'],
}
```

For SonarQube or Bamboo import, add the `lcov` reporter: `coverageReporters: ['lcov', 'html', 'text']`. SonarQube does not yet support `.vue` files using TypeScript (SonarSource roadmap MMF-1441).

Vue component testing is covered on the separate "Testing a Vue Project" page (Confluence pageId=78847252), not part of this set. End-to-end testing uses `npx albus test --e2e` with `albus-module-cypress` (see CLI).

Sources: Unit Testing (https://confluence.alkami.com/spaces/SDKC/pages/303531429); Albus CLI Commands (https://confluence.alkami.com/spaces/SDKC/pages/303531404)
