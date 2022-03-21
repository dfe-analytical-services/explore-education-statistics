/**
 * Babel plugin needed for compatibility between Webpack 5's
 * web workers and Jest. It simply removes `import.meta.url`
 * when instantiating instances of {@see URL}:
 *
 * ```ts
 * new URL('the-web-worker-import', import.meta.url);
 *
 * // Transpiles to:
 * new URL('the-web-worker-import');
 * ```
 *
 * Currently, Jest does not fully support ES modules and this
 * means that we can't use `import.meta.url` in any of our code
 * (it's necessary for using Webpack 5's web workers).
 *
 * In the future, we hope that Jest will fully support
 * `import.meta.url` so that we won't need this plugin anymore.
 * @see https://github.com/facebook/jest/issues/4842
 * @see https://github.com/facebook/jest/issues/9430
 */
module.exports = babel => {
  const { types: t } = babel;

  // Only execute during tests
  if (process.env.NODE_ENV !== 'test') {
    return {};
  }

  return {
    visitor: {
      NewExpression(path) {
        if (!t.isIdentifier(path.node.callee, { name: 'URL' })) {
          return;
        }

        if (path.node.arguments.length !== 2) {
          return;
        }

        const [urlArg, importArg] = path.node.arguments;

        // Check that the shape of the second argument
        // corresponds to `import.meta.url`, then
        // remove it from the transpiled code.
        if (
          t.isStringLiteral(urlArg) &&
          t.isMemberExpression(importArg) &&
          t.isMetaProperty(importArg.object) &&
          t.isIdentifier(importArg.object.meta, { name: 'import' }) &&
          t.isIdentifier(importArg.object.property, { name: 'meta' }) &&
          t.isIdentifier(importArg.property, { name: 'url' })
        ) {
          path.get('arguments.1').remove();
        }
      },
    },
  };
};
