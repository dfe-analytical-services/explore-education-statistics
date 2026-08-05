import {defineConfig, globalIgnores} from "eslint/config";
import {fixupConfigRules} from "@eslint/compat";
import globals from "globals";
import tsParser from "@typescript-eslint/parser";
import path from "node:path";
import {fileURLToPath} from "node:url";
import js from "@eslint/js";
import {FlatCompat} from "@eslint/eslintrc";
import nextPlugin from '@next/eslint-plugin-next'
import * as airbnbExtendedLegacy from "eslint-config-airbnb-extended/legacy";

const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);
const compat = new FlatCompat({
  baseDirectory: dirname,
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
  ]),
  ...airbnbExtendedLegacy.configs.base.legacy,
  ...airbnbExtendedLegacy.configs.react.base,
  ...airbnbExtendedLegacy.configs.react.recommended,
  ...airbnbExtendedLegacy.configs.react.hooks,
  {
    linterOptions: {},

    extends: [...fixupConfigRules(compat.extends(
      "plugin:@typescript-eslint/recommended",
      "prettier",
      "prettier/prettier",
    )),

    ],
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
        version: "19",
      },
    },


    rules: {
      "class-methods-use-this": "off",
      "default-param-last": "off",
      "lines-between-class-members": ["error", "always", {
        exceptAfterSingleLine: true,
      }],
      "no-console": "warn",
      "no-empty-function": "off",
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

      "@typescript-eslint/ban-ts-ignore": "off",
      "@typescript-eslint/default-param-last": "error",
      "@typescript-eslint/explicit-function-return-type": "off",
      "@typescript-eslint/explicit-module-boundary-types": "off",
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
      
      "import/extensions": "off",
      "import/no-cycle": "off",
      "import/no-duplicates": "error",
      "import/no-extraneous-dependencies": ["error", {
        devDependencies: true,
      }],
      "import/no-unresolved": "off",
      
      "jsx-a11y/anchor-is-valid": "off",
      'jsx-a11y/autocomplete-valid': 'error',
      "jsx-a11y/control-has-associated-label": "off",
      "jsx-a11y/label-has-associated-control": ["error", {
        assert: "htmlFor",
      }],
      "react-hooks/exhaustive-deps": "error",
      'react/display-name': 'error',
      "react/forbid-prop-types": "off",
      "react/function-component-definition": "off",
      "react/jsx-curly-newline": "off",
      "react/jsx-filename-extension": ["error", {
        extensions: [".jsx", ".tsx"],
      }],
      'react/jsx-key': 'error',
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
      'react/no-direct-mutation-state': 'error',
      "react/no-unescaped-entities": ["error", {
        forbid: [{
          char: ">",
          alternatives: ["&gt;"],
        }, {
          char: "}",
          alternatives: ["&#125;"],
        }],
      }],
      "react/no-unstable-nested-components": ["error", {
        allowAsProps: true,
      }],
      "react/prop-types": "off",
      "react/react-in-jsx-scope": "off",
      "react/require-default-props": "off",
      "react/state-in-constructor": "off",
      "react/static-property-placement": ["error", "static public field"],
    },
  },

  {
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
      // ...nextPlugin.configs.recommended.rules,

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