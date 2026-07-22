import {defineConfig, globalIgnores} from "eslint/config";
import {fixupConfigRules} from "@eslint/compat";
import globals from "globals";
import tsParser from "@typescript-eslint/parser";
import path from "node:path";
import {fileURLToPath} from "node:url";
import js from "@eslint/js";
import {FlatCompat} from "@eslint/eslintrc";
import nextPlugin from '@next/eslint-plugin-next'

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const compat = new FlatCompat({
    baseDirectory: __dirname,
    recommendedConfig: js.configs.recommended,
    allConfig: js.configs.all,
});

export default defineConfig([
    globalIgnores([
        "**/.idea/",
        "**/.next/",
        "**/build/",
        "**/dist/",
        "**/coverage/",
        "**/node_modules/",
        "**/*.min.js",
        "**/wwwroot/",
        "src/GovUk.Education.ExploreEducationStatistics.*/",
        "src/explore-education-statistics-ckeditor/sample",
        "eslint.config.mjs",
    ]),
    {
        linterOptions: {
        },

        extends: [...fixupConfigRules(compat.extends(
            "eslint:recommended",
            "plugin:@typescript-eslint/recommended",

            "plugin:react/recommended",
            "plugin:react-hooks/recommended",
            "plugin:jsx-a11y/recommended",
            "plugin:import/recommended",
            "plugin:import/typescript",
            "prettier",
            "prettier/prettier",
        ))],

        plugins: {
            //"import": fixupPluginRules(importPlugin),
        },

        languageOptions: {
            globals: {
                ...globals.browser,
                ...globals.jest,
                ...globals.node,
            },

            parser: tsParser,
        },

        settings: {
            "import/resolver": {
                typescript: {
                    
                    project: "./src",
                },
            },

            react: {
                version: "19.2",
            },
        },


        rules: {

            "@typescript-eslint/ban-ts-ignore": "off",
            "@typescript-eslint/default-param-last": "error",
            "@typescript-eslint/explicit-function-return-type": "off",

            "@typescript-eslint/no-empty-function": ["error", {
                allow: ["arrowFunctions", "functions", "methods"],
            }],

            "@typescript-eslint/no-shadow": "error",

            "@typescript-eslint/no-unused-vars": ["warn", {
                argsIgnorePattern: "^_$",
                caughtErrors: "none",
            }],

            "@typescript-eslint/no-useless-constructor": "error",
            "@typescript-eslint/triple-slash-reference": "off",
            "@typescript-eslint/explicit-module-boundary-types": "off",
            "class-methods-use-this": "off",
            "default-param-last": "off",
            "import/extensions": "off",
            "import/no-cycle": "off",
            "import/no-duplicates": "error",

            "import/no-extraneous-dependencies": ["error", {
                devDependencies: true,
            }],

            "import/no-unresolved": "off",

            "lines-between-class-members": ["error", "always", {
                exceptAfterSingleLine: true,
            }],

            "react/forbid-prop-types": "off",
            "react/function-component-definition": "off",

            "react/no-unstable-nested-components": ["error", {
                allowAsProps: true,
            }],

            "react-hooks/exhaustive-deps": "error",

            "react/no-unescaped-entities": ["error", {
                forbid: [{
                    char: ">",
                    alternatives: ["&gt;"],
                }, {
                    char: "}",
                    alternatives: ["&#125;"],
                }],
            }],

            "react/jsx-filename-extension": ["error", {
                extensions: [".jsx", ".tsx"],
            }],

            "react/jsx-curly-newline": "off",

            "react/jsx-no-useless-fragment": ["error", {
                allowExpressions: true,
            }],

            "react/jsx-one-expression-per-line": "off",

            "react/jsx-props-no-spreading": ["error", {
                html: "enforce",
                custom: "ignore",
            }],

            "react/jsx-wrap-multilines": ["error", {
                prop: "ignore",
            }],

            "react/react-in-jsx-scope": "off",
            "react/require-default-props": "off",
            "react/state-in-constructor": "off",
            "react/static-property-placement": ["error", "static public field"],
            "react/prop-types": "off",
            "jsx-a11y/anchor-is-valid": "off",

            "jsx-a11y/label-has-associated-control": ["error", {
                assert: "htmlFor",
            }],

            "no-empty-function": "off",
            "no-console": "warn",

            "no-param-reassign": ["error", {
                props: true,
                ignorePropertyModificationsFor: ["draft", "acc"],
            }],

            "no-promise-executor-return": "off",

            "no-restricted-exports": ["error", {
                restrictDefaultExports: {
                    defaultFrom: false,
                },
            }],

            "no-shadow": "off",

            "no-underscore-dangle": ["error", {
                allow: ["_def"],
            }],

            "no-unreachable": "error",
            "no-use-before-define": "off",
            "no-useless-constructor": "off",

            // unknown reason why this is occurring? Maybe due to IDEs defaulting to BOM files
            "unicode-bom": "off",

            // Additional linting added in react updates, it would be best to introduce these and fix the issues
            // highlighted
            "react-hooks/set-state-in-effect": "off",
            "react-hooks/refs": "off",
            "react-hooks/immutability": "off",
            "react-hooks/preserve-manual-memoization": "off",
            "react-hooks/set-state-in-render": "off",
            "react-hooks/error-boundaries": "off",

            // list of rules generated due to comments, a lot of this is copied over from the errors in airbnb for now
            // these have been separated from the above rules as they are the original rules we had previously defined
            'max-classes-per-file': ['error', 1],
            'no-await-in-loop': 'error',
            'no-template-curly-in-string': 'error',
            'preserve-caught-error': 'off',
            'no-nested-ternary': 'error',
            'no-bitwise': 'error',
            'no-constructor-return': 'error',
            'no-global-assign': ['error', { exceptions: [] }],


            'no-restricted-globals': [
                'error',
                {
                    name: 'isFinite',
                    message:
                        'Use Number.isFinite instead https://github.com/airbnb/javascript#standard-library--isfinite',
                },
                {
                    name: 'isNaN',
                    message:
                        'Use Number.isNaN instead https://github.com/airbnb/javascript#standard-library--isnan',
                },
                'addEventListener',
                'blur',
                'close',
                'closed',
                'confirm',
                'defaultStatus',
                'defaultstatus',
                'event',
                'external',
                'find',
                'focus',
                'frameElement',
                'frames',
                'history',
                'innerHeight',
                'innerWidth',
                'length',
                'location',
                'locationbar',
                'menubar',
                'moveBy',
                'moveTo',
                'name',
                'onblur',
                'onerror',
                'onfocus',
                'onload',
                'onresize',
                'onunload',
                'open',
                'opener',
                'opera',
                'outerHeight',
                'outerWidth',
                'pageXOffset',
                'pageYOffset',
                'parent',
                'print',
                'removeEventListener',
                'resizeBy',
                'resizeTo',
                'screen',
                'screenLeft',
                'screenTop',
                'screenX',
                'screenY',
                'scroll',
                'scrollbars',
                'scrollBy',
                'scrollTo',
                'scrollX',
                'scrollY',
                'self',
                'status',
                'statusbar',
                'stop',
                'toolbar',
                'top'
            ],
            'prefer-rest-params': 'error',
            'consistent-return': 'error',
            'prefer-object-spread': 'error',
            'no-new-require': 'error',
            'no-new': 'error',
            'prefer-destructuring': ['error', {
                VariableDeclarator: {
                    array: false,
                    object: true,
                },
                AssignmentExpression: {
                    array: true,
                    object: false,
                },
            }, {
                enforceForRenamedProperties: false,
            }],
            camelcase: ['error', {properties: 'never', ignoreDestructuring: false}],
            
            // this can probably be relaxed as we move into newer libraries?
            "no-restricted-syntax": [
                'error',
                {
                    selector: 'ForInStatement',
                    message: 'for..in loops iterate over the entire prototype chain, which is virtually never what you want. Use Object.{keys,values,entries}, and iterate over the resulting array.',
                },
                {
                    selector: 'ForOfStatement',
                    message: 'iterators/generators require regenerator-runtime, which is too heavyweight for this guide to allow them. Separately, loops should be avoided in favor of array iterations.',
                },
                {
                    selector: 'LabeledStatement',
                    message: 'Labels are a form of GOTO; using them makes code confusing and hard to maintain and understand.',
                },
                {
                    selector: 'WithStatement',
                    message: '`with` is disallowed in strict mode because it makes code impossible to predict and optimize.',
                },
            ],

            'jsx-a11y/no-autofocus': ['error', {ignoreNonDOM: true}],
            
            "import/no-import-module-exports": "error",
            "import/prefer-default-export": "error",

            //"@typescript-eslint/no-non-null-assertion": "error",

            'react/prefer-stateless-function': ['error', {ignorePureComponents: true}],
            'react/destructuring-assignment': ['error', 'always'],
            'react/jsx-boolean-value': ['error', 'never', {always: []}],
            'react/no-array-index-key': 'error',
            'react/no-this-in-sfc': 'error',
            'react/button-has-type': ['error', {
                button: true,
                submit: true,
                reset: false,
            }],
            'react/jsx-no-target-blank': ['error', {enforceDynamicLinks: 'always'}],
            'react/jsx-no-constructed-context-values': 'error',
            'react/no-unused-prop-types': ['error', {
                customValidators: [],
                skipShapeProps: true,
            }],
            'react/display-name': ['off', {ignoreTranspilerName: false}],
            "react/no-danger": "error",
            
        },
    }, {
        files: ["**/*.js"],

        rules: {
            "@typescript-eslint/explicit-module-boundary-types": "off",
            "@typescript-eslint/no-var-requires": "off",
            "global-require": "off",
            "import/no-dynamic-require": "off",
            "no-console": "off",
            // due to old config files
            "@typescript-eslint/no-require-imports": "off",
        },
    }, {
        files: ["useful-scripts/*.ts", "useful-scripts/*/**.ts"],

        rules: {
            "no-console": "off",

            "no-underscore-dangle": ["error", {
                allow: ["__dirname", "__filename"],
            }],
        },
    },
    {
        files: ["src/explore-education-statistics-frontend/**/*.{tsx,ts}"],

        settings: {
            next: {
                rootDir: "src/explore-education-statistics-frontend",
            },
        },

        plugins: {
            //  "@next/next": nextPlugin,
        },

        extends: [
            nextPlugin.configs.recommended
        ],

        rules: {
            //...nextPlugin.configs.recommended.rules,

            "@next/next/no-img-element": "off",

            "@next/next/no-html-link-for-pages": [
                "error",
                ["src/pages", "src/explore-education-statistics-frontend/src/pages"],
            ],
        },


    },
    {
        files: ["**/*.d.ts"],
        rules: {
            // due to type definitions
            "no-undef": "off",
        }
    }, {
        files: ["src/explore-education-statistics-ckeditor/webpack.config.js"],
        languageOptions: {
            globals: {
                ...globals.node,
            }
        }
    }
]);