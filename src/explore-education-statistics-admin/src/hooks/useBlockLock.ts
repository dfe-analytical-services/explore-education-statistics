import { useAuthContext } from '@admin/contexts/AuthContext';
import { UserDetails } from '@admin/services/types/user';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useThrottledCallback from '@common/hooks/useThrottledCallback';
import logger from '@common/services/logger';
import { differenceInMilliseconds, isFuture } from 'date-fns';
import { useCallback, useEffect, useState } from 'react';

export interface BlockLock {
  locked?: string;
  lockedUntil?: string;
  lockedBy?: UserDetails;
}

export interface BlockLockState extends BlockLock {
  endLock: () => void;
  error?: unknown;
  isLocking: boolean;
  isLocked: boolean;
  isLockedByUser: boolean;
  isLockedByOtherUser: boolean;
  lockThrottle: number;
  refreshLock: () => void;
  startLock: () => void;
  setLock: (nextLock: BlockLock | undefined) => void;
}

export interface BlockLockOptions {
  getLock: () => Promise<BlockLock>;
  unlock: () => Promise<void>;
  /**
   * The initial lock state. This will typically
   * be supplied by the lock fields on the block.
   */
  initialLock?: BlockLock;
  /**
   * The maximum rate that a new lock can be
   * requested (in milliseconds).
   *
   * This should be something relatively high
   * to prevent excessive lock requests.
   */
  lockThrottle?: number;
}

/**
 * Hook managing the async state around a lock for a content block. This
 * includes invalidation of the lock after it expires.
 */
export default function useBlockLock({
  getLock,
  unlock,
  initialLock,
  lockThrottle = 60_000,
}: BlockLockOptions): BlockLockState {
  const { user } = useAuthContext();

  const [{ value, setState, isLoading: isLocking, error }, startLock] =
    useAsyncCallback<BlockLock | undefined>(getLock, [], {
      keepStaleValue: true,
      initialState: initialLock
        ? {
            isLoading: false,
            value: {
              locked: initialLock.locked,
              lockedUntil: initialLock.lockedUntil,
              lockedBy: initialLock.lockedBy,
            },
          }
        : undefined,
    });

  const { locked, lockedBy, lockedUntil } = value ?? {};

  const isLocked =
    !!locked && !!lockedBy && !!lockedUntil && isFuture(new Date(lockedUntil));

  const isLockedByUser = isLocked && lockedBy.id === user?.id;
  const isLockedByOtherUser = isLocked && lockedBy.id !== user?.id;

  const [lockTimeout, setLockTimeout] = useState(() =>
    isLocked && lockedUntil
      ? differenceInMilliseconds(new Date(lockedUntil), new Date())
      : 0,
  );

  const setLock = useCallback(
    (nextLock: BlockLock | undefined) => {
      setState({
        value: nextLock,
      });
    },
    [setState],
  );

  const [refreshLock] = useThrottledCallback(() => {
    if (!isLockedByUser) {
      return;
    }

    startLock();
  }, lockThrottle);

  const endLock = useCallback(async () => {
    if (!isLockedByUser) {
      return;
    }

    try {
      await unlock();
      setLock(undefined);
    } catch (err) {
      logger.error(err);
    }
  }, [isLockedByUser, setLock, unlock]);

  // Handles invalidation of the lock by causing the component to
  // re-render when the lock expires (via setting lock timeout state).
  const [startLockCheck, stopLockCheck] = useDebouncedCallback(async () => {
    if (!isLocked) {
      return;
    }

    const remainingLockTime = differenceInMilliseconds(
      new Date(lockedUntil),
      new Date(),
    );

    if (remainingLockTime > 0) {
      // Add some extra milliseconds in case there are
      // any rounding errors in the calculated timeout.
      setLockTimeout(remainingLockTime + 2);
      await startLockCheck();
    } else {
      setLockTimeout(0);
      await endLock();
    }
  }, lockTimeout);

  useEffect(() => {
    if (lockedUntil && isFuture(new Date(lockedUntil))) {
      startLockCheck();
    } else {
      stopLockCheck();
    }
  }, [lockedUntil, startLockCheck, stopLockCheck]);

  return {
    endLock,
    error,
    isLocking,
    isLocked,
    isLockedByUser,
    isLockedByOtherUser,
    lockThrottle,
    locked,
    lockedBy,
    lockedUntil,
    refreshLock,
    setLock,
    startLock,
  };
}
