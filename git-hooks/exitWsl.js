const fs = require('fs');
const os = require('os');

/**
 * Script to exit with status code 0 if the current
 * process is running on the Windows Subsystem for Linux (WSL).
 *
 * This is required to avoid issues with WSL not running
 * `lint-staged` correctly for some users.
 * @see https://github.com/okonet/lint-staged/issues/420
 */

const isWsl = () => {
  if (process.platform !== 'linux') {
    return false;
  }

  if (os.release().includes('Microsoft')) {
    return true;
  }

  try {
    return fs.readFileSync('/proc/version', 'utf8').includes('Microsoft');
  } catch (err) {
    return false;
  }
};

if (isWsl()) {
  process.exit(0);
}

process.exit(1);
