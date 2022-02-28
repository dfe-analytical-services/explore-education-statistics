import { useAuthContext } from '@admin/contexts/AuthContext';
import { UserDetails } from '@admin/services/types/user';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { differenceInMilliseconds, isFuture } from 'date-fns';
import { useCallback, useEffect, useState } from 'react';

export interface BlockLock {
  locked?: string;
  lockedUntil?: string;
  lockedBy?: UserDetails;
}

export interface BlockLockState extends BlockLock {
  error?: unknown;
  isLocking: boolean;
  isLocked: boolean;
  isLockedByUser: boolean;
  isLockedByOtherUser: boolean;
  startLock: () => void;
  setLock: (nextLock: BlockLock | undefined) => void;
}

export interface BlockLockOptions {
  getLock: () => Promise<BlockLock>;
  initialLock?: BlockLock;
}

/**
 * Hook managing the async state around a lock for a content block. This
 * includes invalidation of the lock after it expires.
 */
export default function useBlockLock({
  getLock,
  initialLock,
}: BlockLockOptions): BlockLockState {
  const { user } = useAuthContext();

  const [
    { value, setState, isLoading: isLocking, error },
    startLock,
  ] = useAsyncCallback<BlockLock | undefined>(
    getLock,
    [],
    initialLock
      ? {
          isLoading: false,
          value: {
            locked: initialLock.locked,
            lockedUntil: initialLock.lockedUntil,
            lockedBy: initialLock.lockedBy,
          },
        }
      : undefined,
  );

  const { locked, lockedBy, lockedUntil } = value ?? {};

  const isLocked =
    !!locked && !!lockedBy && !!lockedUntil && isFuture(new Date(lockedUntil));

  const [lockTimeout, setLockTimeout] = useState(() =>
    isLocked && lockedUntil
      ? differenceInMilliseconds(new Date(lockedUntil), new Date())
      : 0,
  );

  // Handles invalidation of the lock by causing the component to
  // re-render when the lock expires (via setting lock timeout state).
  const [startLockCheck, stopLockCheck] = useDebouncedCallback(() => {
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
      startLockCheck();
    } else {
      setLockTimeout(0);
    }
  }, lockTimeout);

  useEffect(() => {
    if (lockedUntil && isFuture(new Date(lockedUntil))) {
      startLockCheck();
    } else {
      stopLockCheck();
    }
  }, [lockedUntil, startLockCheck, stopLockCheck]);

  const setLock = useCallback(
    (nextLock: BlockLock | undefined) => {
      setState({ value: nextLock });
    },
    [setState],
  );

  const isLockedByUser = isLocked && lockedBy.id === user?.id;
  const isLockedByOtherUser = isLocked && lockedBy.id !== user?.id;

  return {
    error,
    setLock,
    startLock,
    isLocking,
    locked,
    lockedBy,
    lockedUntil,
    isLocked,
    isLockedByUser,
    isLockedByOtherUser,
  };
}
