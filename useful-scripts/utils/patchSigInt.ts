import * as os from 'node:os';
import * as readline from 'node:readline';

/**
 * Patch to fix SIGINT not always be received by the Node process
 * in various shells such as Git Bash for Windows.
 */
export default function patchSigInt() {
  if (os.platform() === 'win32') {
    const rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
    });

    rl.on('SIGINT', () => {
      process.emit('SIGINT');
    });
  }
}
