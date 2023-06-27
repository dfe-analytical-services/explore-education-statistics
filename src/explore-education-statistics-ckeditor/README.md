# CKEditor 5 editor

A custom CKEditor build including custom EES plugins.

## Building the editor

Create the build: `pnpm build`

This will build the CKEditor 5 to the `build` directory which is imported into EES Admin.

## Adding or removing CKEditor plugins

Now you can install additional plugin in the build. Just follow the [Adding a plugin to an editor tutorial](https://ckeditor.com/docs/ckeditor5/latest/builds/guides/integration/installing-plugins.html#adding-a-plugin-to-an-editor)

Note that having plugins at different versions can cause build errors so it's generally best to update them all when adding a new one.

## Custom plugins

EES custom plugins should be added to the `plugins` folder then imported and added to `Editor.builtinPlugins` in `src/ckeditor.js`.

## Debugging

The [CKEditor Inspector](https://ckeditor.com/docs/ckeditor5/latest/framework/guides/development-tools.html) can be used in the host app to help with debugging.
