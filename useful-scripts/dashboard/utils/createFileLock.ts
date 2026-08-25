import { lock } from 'cross-process-lock';
import fs from 'node:fs';
import onExitSignal from './onExitSignal';

interface LockOptions {
  /**
   * The path to the file to lock.
   */
  lockFile: string;
  /**
   * The maximum timeout before any lock on the file must be
   * released, allowing to be acquired by another process.
   */
  lockTimeout?: number;
  /**
   * Event triggered when a lock already exists on the file.
   */
  onExistingLock?: () => void;
  /**
   * The maximum timeout that the process will wait trying
   * to acquire a lock before throwing a lock error.
   */
  waitTimeout?: number;
}

export type UnlockCallback = () => Promise<void>;

/**
 * Create a lock on a file.
 */
export default async function createFileLock(
  options: LockOptions,
): Promise<UnlockCallback> {
  const { lockFile, lockTimeout, onExistingLock, waitTimeout } = options;

  if (fs.existsSync(`${lockFile}.lock`)) {
    onExistingLock?.();
  }

  const unlock = await lock(lockFile, {
    lockTimeout,
    waitTimeout,
  });

  const removeExitSignal = onExitSignal(async () => {
    await unlock();
  });

  return async () => {
    await unlock();
    removeExitSignal();
  };
}
