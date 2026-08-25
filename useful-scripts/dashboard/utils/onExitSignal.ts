import { onExit } from 'signal-exit';
import exitProcess, { ExitInstruction } from './exitProcess';

/**
 * Remove the exit signal listener.
 */
type OnExitSignalReturn = () => void;

/**
 * Handle an exit signal. By default, nothing will happen at the
 * end of the listener.
 *
 * If you want to kill the process after the listener has completed, you can
 * return an {@link ExitInstruction} that can specify a signal or exit code.
 */
type ExitSignalHandler = (
  code: number | null | undefined,
  signal: NodeJS.Signals | null,
) => ExitInstruction | void | Promise<ExitInstruction | void>;

/**
 * Listen for a signal that would normally cause a process to exit.
 *
 * @param handler Run a handler upon receiving an exit signal.
 */
export default function onExitSignal(
  handler: ExitSignalHandler,
): OnExitSignalReturn {
  return onExit((exitCode, signal) => {
    const result = handler(exitCode, signal);

    if (result instanceof Promise) {
      result.then(instruction => {
        if (instruction) {
          exitProcess(instruction);
        }
      });

      return true;
    }

    if (result) {
      exitProcess(result);
    }

    return true;
  });
}
