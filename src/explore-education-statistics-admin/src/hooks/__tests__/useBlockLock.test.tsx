import { AuthContextTestProvider } from '@admin/contexts/AuthContext';
import useBlockLock, { BlockLock } from '@admin/hooks/useBlockLock';
import { GlobalPermissions } from '@admin/services/permissionService';
import { UserDetails } from '@admin/services/types/user';
import MockDate from '@common-test/mockDate';
import { renderHook } from '@testing-library/react-hooks';
import React, { FC } from 'react';

describe('useBlockLock', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  const testUser1: UserDetails = {
    id: 'user-1',
    email: 'jane@test.com',
    displayName: 'Jane Doe',
  };

  const testUser2: UserDetails = {
    id: 'user-2',
    email: 'rob@test.com',
    displayName: 'Rob Rowe',
  };

  const wrapper: FC = ({ children }) => (
    <AuthContextTestProvider
      user={{
        id: 'user-1',
        name: 'Jane Doe',

        permissions: {} as GlobalPermissions,
      }}
    >
      {children}
    </AuthContextTestProvider>
  );

  const getLock = jest.fn();
  const unlock = jest.fn();

  test('returns correct default state', () => {
    MockDate.set('2022-03-10T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();
  });

  test('returns correct state when `initialLock` is not expired', () => {
    MockDate.set('2022-03-10T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-10T12:00:00Z',
            lockedUntil: '2022-03-10T12:10:00Z',
            lockedBy: testUser1,
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-10T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-10T12:10:00Z');
  });

  test('returns correct state when `initialLock` is expired', () => {
    MockDate.set('2022-03-21T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-10T12:00:00Z',
            lockedUntil: '2022-03-10T12:10:00Z',
            lockedBy: testUser1,
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-10T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-10T12:10:00Z');
  });

  test('returns correct state when `initialLock` is owned by another user', () => {
    MockDate.set('2022-03-10T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-10T12:00:00Z',
            lockedUntil: '2022-03-10T12:10:00Z',
            lockedBy: testUser2,
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(true);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-10T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser2);
    expect(result.current.lockedUntil).toBe('2022-03-10T12:10:00Z');
  });

  test('returns correct state when `initialLock` is expired and owned by another user', () => {
    MockDate.set('2022-03-20T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-10T12:00:00Z',
            lockedUntil: '2022-03-10T12:10:00Z',
            lockedBy: testUser2,
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-10T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser2);
    expect(result.current.lockedUntil).toBe('2022-03-10T12:10:00Z');
  });

  test('re-renders with correct state when `initialLock` expires', async () => {
    // Lock is about to expire
    MockDate.set('2022-03-20T12:09:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedUntil: '2022-03-20T12:10:00Z',
            lockedBy: testUser1,
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');

    // Lock has now expired
    MockDate.set('2022-03-20T12:10:01Z');
    jest.advanceTimersByTime(61_000);

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');
  });

  test('calls `unlock` when `initialLock` expires', async () => {
    // Lock is about to expire
    MockDate.set('2022-03-20T12:09:00Z');

    renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedUntil: '2022-03-20T12:10:00Z',
            lockedBy: testUser1,
          },
        }),
      {
        wrapper,
      },
    );

    expect(unlock).toHaveBeenCalledTimes(0);

    // Lock has now expired
    MockDate.set('2022-03-20T12:10:01Z');
    jest.advanceTimersByTime(61_000);

    expect(unlock).toHaveBeenCalledTimes(1);
  });

  test('calling `startLock` calls `getLock` option', async () => {
    MockDate.set('2022-03-21T12:00:00Z');

    const { result, waitFor } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    expect(getLock).not.toHaveBeenCalled();

    result.current.startLock();

    await waitFor(() => {
      expect(getLock).toHaveBeenCalledTimes(1);
    });
  });

  test('calling `startLock` correctly updates state whilst resolving', async () => {
    MockDate.set('2022-03-21T12:00:00Z');

    const { result, waitFor } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();

    result.current.startLock();

    await waitFor(() => {
      expect(result.current.error).toBeUndefined();
      expect(result.current.isLocked).toBe(false);
      expect(result.current.isLockedByUser).toBe(false);
      expect(result.current.isLockedByOtherUser).toBe(false);
      expect(result.current.isLocking).toBe(true);
      expect(result.current.locked).toBeUndefined();
      expect(result.current.lockedBy).toBeUndefined();
      expect(result.current.lockedUntil).toBeUndefined();
    });
  });

  test('calling `startLock` correctly updates state once resolved', async () => {
    MockDate.set('2022-03-20T12:00:00Z');

    const nextLock: BlockLock = {
      locked: '2022-03-20T12:00:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:10:00Z',
    };

    getLock.mockResolvedValue(nextLock);

    const { result, waitForNextUpdate } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();

    result.current.startLock();

    await waitForNextUpdate();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');
  });

  test('calling `startLock` retains previous lock state until new lock resolves', async () => {
    MockDate.set('2022-03-20T12:00:00Z');

    const nextLock: BlockLock = {
      locked: '2022-03-20T12:08:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:18:00Z',
    };

    getLock.mockResolvedValue(nextLock);

    const { result, waitForNextUpdate } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');

    result.current.startLock();

    await waitForNextUpdate();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:08:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:18:00Z');
  });

  test('calling `setLock` correctly updates state', async () => {
    MockDate.set('2022-03-20T12:00:00Z');

    const nextLock: BlockLock = {
      locked: '2022-03-20T12:00:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:10:00Z',
    };

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    result.current.setLock(nextLock);

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');
  });

  test('calling `setLock` with undefined correctly updates state', async () => {
    MockDate.set('2022-03-20T12:00:00Z');

    // Provide an initial lock to prove that everything gets unset.
    const initialLock: BlockLock = {
      locked: '2022-03-20T12:00:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:10:00Z',
    };

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          initialLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    result.current.setLock(undefined);

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();
  });

  test('calling `refreshLock` does not update state if another user owns the lock', () => {
    MockDate.set('2022-03-10T12:05:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-10T12:00:00Z',
            lockedUntil: '2022-03-10T12:10:00Z',
            lockedBy: testUser2,
          },
        }),
      {
        wrapper,
      },
    );

    result.current.refreshLock();

    jest.runOnlyPendingTimers();

    expect(getLock).not.toHaveBeenCalled();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(true);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-10T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser2);
    expect(result.current.lockedUntil).toBe('2022-03-10T12:10:00Z');
  });

  test('calling `refreshLock` does not update state if no user owns the lock', async () => {
    MockDate.set('2022-03-20T12:00:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );

    result.current.refreshLock();

    jest.runOnlyPendingTimers();

    expect(getLock).not.toHaveBeenCalled();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();
  });

  test('calling `refreshLock` updates state with new lock if user currently owns the lock', async () => {
    // Lock is nearly expired
    MockDate.set('2022-03-20T12:08:00Z');

    const nextLock: BlockLock = {
      locked: '2022-03-20T12:08:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:18:00Z',
    };

    getLock.mockResolvedValue(nextLock);

    const { result, waitForNextUpdate } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );

    result.current.refreshLock();

    jest.runOnlyPendingTimers();

    await waitForNextUpdate();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:08:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:18:00Z');
  });

  test('calling `refreshLock` does not call `getLock` until `lockThrottle` has elapsed', async () => {
    // Lock is nearly expired
    MockDate.set('2022-03-20T12:08:00Z');

    const nextLock: BlockLock = {
      locked: '2022-03-20T12:08:00Z',
      lockedBy: testUser1,
      lockedUntil: '2022-03-20T12:18:00Z',
    };

    getLock.mockResolvedValue(nextLock);

    const { result, waitForNextUpdate } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
          // Set a custom lock throttle
          lockThrottle: 2000,
        }),
      {
        wrapper,
      },
    );

    result.current.refreshLock();

    expect(getLock).toHaveBeenCalledTimes(0);

    // Advance by our custom lock throttle time
    jest.advanceTimersByTime(2000);

    await waitForNextUpdate();

    expect(getLock).toHaveBeenCalledTimes(1);
  });

  test('calling `endLock` calls `unlock` option if lock is owned by user', () => {
    MockDate.set('2022-03-20T12:08:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );
    expect(unlock).toHaveBeenCalledTimes(0);

    result.current.endLock();

    expect(unlock).toHaveBeenCalledTimes(1);
  });

  test('calling `endLock` correctly updates state if lock is owned by user', async () => {
    MockDate.set('2022-03-20T12:08:00Z');

    const { result, waitForNextUpdate } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );

    result.current.endLock();

    await waitForNextUpdate();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();
  });

  test('calling `endLock` does nothing if lock is owned by another user', () => {
    MockDate.set('2022-03-20T12:08:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser2,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );
    expect(unlock).not.toHaveBeenCalled();

    result.current.endLock();

    expect(unlock).not.toHaveBeenCalled();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(true);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser2);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');
  });

  test('calling `endLock` does nothing if no user owns the lock', () => {
    MockDate.set('2022-03-20T12:08:00Z');

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
        }),
      {
        wrapper,
      },
    );
    expect(unlock).not.toHaveBeenCalled();

    result.current.endLock();

    expect(unlock).not.toHaveBeenCalled();

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(false);
    expect(result.current.isLockedByUser).toBe(false);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBeUndefined();
    expect(result.current.lockedBy).toBeUndefined();
    expect(result.current.lockedUntil).toBeUndefined();
  });

  test('calling `endLock` does not update state if `unlock` throws an error', () => {
    MockDate.set('2022-03-20T12:08:00Z');

    unlock.mockRejectedValue(new Error('Something went wrong'));

    const { result } = renderHook(
      () =>
        useBlockLock({
          getLock,
          unlock,
          initialLock: {
            locked: '2022-03-20T12:00:00Z',
            lockedBy: testUser1,
            lockedUntil: '2022-03-20T12:10:00Z',
          },
        }),
      {
        wrapper,
      },
    );
    expect(unlock).toHaveBeenCalledTimes(0);

    result.current.endLock();

    expect(unlock).toHaveBeenCalledTimes(1);

    expect(result.current.error).toBeUndefined();
    expect(result.current.isLocked).toBe(true);
    expect(result.current.isLockedByUser).toBe(true);
    expect(result.current.isLockedByOtherUser).toBe(false);
    expect(result.current.isLocking).toBe(false);
    expect(result.current.locked).toBe('2022-03-20T12:00:00Z');
    expect(result.current.lockedBy).toEqual(testUser1);
    expect(result.current.lockedUntil).toBe('2022-03-20T12:10:00Z');
  });
});
