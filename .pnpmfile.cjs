function readPackage(pkg, context) {
  if (pkg.name === 'applicationinsights') {
    pkg.dependencies['diagnostic-channel'] = '1.1.0';
  }
  return pkg;
}

module.exports = {
  hooks: {
    readPackage,
  },
};
