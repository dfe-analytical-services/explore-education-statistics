module.exports = {
    hooks: {
      readPackage(pkg) {
        pkg.dependenciesMeta = pkg.dependenciesMeta || {};
        for (const [depName, depVersion] of Object.entries(pkg.dependencies)) {
          if (depVersion.startsWith('workspace:')) {
            pkg.dependenciesMeta[depName] = {
              injected: true,
            };
          }
        }
        return pkg;
      },
    },
  };