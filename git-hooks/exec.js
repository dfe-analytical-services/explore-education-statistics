const childProcess = require('child_process');

/**
 * Simple script to execute a command string
 * in a child process.
 *
 * This is mostly to act as a workaround for a
 * current issue in `lint-staged` which is stopping us
 * from executing `tsc` in project mode.
 * @see https://github.com/okonet/lint-staged/issues/539
 */

const command = process.argv[2];

if (command === undefined) {
  console.error('Script requires a string command as parameter 1 to execute');
  process.exit(1);
}

const commandProcess = childProcess.exec(command, { encoding: 'buffer' });

commandProcess.stdout.pipe(process.stdout);
commandProcess.stderr.pipe(process.stderr);
commandProcess.on('exit', process.exit);

commandProcess.on('error', error => {
  console.error(error);
  process.exit(1);
});
