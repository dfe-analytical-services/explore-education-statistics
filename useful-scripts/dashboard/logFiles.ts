import fs from 'node:fs';
import path from 'node:path';
import { projectRoot, ServiceName } from '../services';

/**
 * Where each service's full output is kept, so there's something to read when
 * a startup failure has already scrolled out of the in-memory buffer the
 * dashboard shows.
 */
export const LOG_DIR = path.join(projectRoot, 'data/dashboard-logs');

/**
 * How much of a service's output to keep before rotating.
 *
 * Roughly 80k lines - two orders of magnitude more than the 500-line buffer
 * the UI holds - which is enough to cover any startup, while still bounding
 * what a service left running for hours can put on disk. One rotation is
 * kept, so the guarantee is "at least this much recent history, at most twice
 * this much on disk", per service.
 */
export const MAX_LOG_FILE_BYTES = 8 * 1024 * 1024;

function currentPath(service: ServiceName): string {
  return path.join(LOG_DIR, `${service}.log`);
}

function previousPath(service: ServiceName): string {
  return path.join(LOG_DIR, `${service}.1.log`);
}

/**
 * The log files for a service, oldest first, skipping any that don't exist.
 * Read in this order they read chronologically.
 */
export function logFilePaths(service: ServiceName): string[] {
  return [previousPath(service), currentPath(service)].filter(file =>
    fs.existsSync(file),
  );
}

/**
 * A service's on-disk log, rotated once it gets too big.
 *
 * Writes are synchronous. Rotation means closing a file, renaming it and
 * opening a new one, and doing that around asynchronous writes means either
 * buffering the lines that arrive mid-rotation or silently sending them to
 * the file that was just rotated away. The writes are small and this is a
 * local dev tool, so the simpler thing that cannot drop or misfile a line is
 * the better trade.
 */
export class ServiceLogFile {
  private fd: number;

  private bytes = 0;

  private closed = false;

  constructor(private readonly service: ServiceName) {
    fs.mkdirSync(LOG_DIR, { recursive: true });
    // Truncating: a log file describes one run of one service, so that the
    // service failing to start doesn't leave you reading the previous run's
    // successful startup.
    this.fd = fs.openSync(currentPath(service), 'w');
  }

  write(line: string): void {
    if (this.closed) {
      return;
    }

    const chunk = Buffer.from(`${line}\n`);

    if (this.bytes > 0 && this.bytes + chunk.length > MAX_LOG_FILE_BYTES) {
      this.rotate();
    }

    fs.writeSync(this.fd, chunk);
    this.bytes += chunk.length;
  }

  close(): void {
    if (this.closed) {
      return;
    }

    this.closed = true;
    fs.closeSync(this.fd);
  }

  private rotate(): void {
    fs.closeSync(this.fd);
    fs.renameSync(currentPath(this.service), previousPath(this.service));
    this.fd = fs.openSync(currentPath(this.service), 'w');
    this.bytes = 0;
  }
}
