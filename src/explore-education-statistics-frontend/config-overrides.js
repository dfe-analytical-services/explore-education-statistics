const StylelintPlugin = require('stylelint-webpack-plugin');

module.exports = {
  webpack: (config) => {
    config.plugins.push(StylelintPlugin());

    return config;
  },
};
