const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const GlobEntries = require('webpack-glob-entries');

module.exports = {
  mode: 'production',
  entry: GlobEntries('./src/**/*.ts'), // Generates multiple entry for each test
  output: {
    path: path.join(__dirname, 'dist'),
    libraryTarget: 'commonjs',
    filename: '[name].js',
  },
  resolve: {
    extensions: ['.ts', '.js'],
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        exclude: /node_modules/,
        use: 'babel-loader',
      },
    ],
  },
  target: 'node',
  externals: /^(k6|https?:\/\/)(\/.*)?/,
  stats: {
    colors: true,
  },
  plugins: [
    new CleanWebpackPlugin(),
    new CopyPlugin({
      patterns: [
        {
          context: path.join(__dirname, 'src/tests'),
          from: '**/*.(csv|zip)',
          to: path.join(__dirname, 'dist'),
        },
      ],
    }),
  ],
  optimization: {
    // Don't minimize, as it's not used in the browser
    minimize: false,
  },
};
