import process from 'node:process';

export interface ExitInstruction {
  /**
   * Kill the process immediately with this code (typically 1).
   */
  code?: number | null;
  /**
   * Terminate the process using a signal.
   */
  signal?: NodeJS.Signals | null;
}

export default function exitProcess({ signal, code }: ExitInstruction): void {
  if (signal) {
    process.kill(process.pid, signal);
  } else {
    process.exit(code ?? 1);
  }
}
