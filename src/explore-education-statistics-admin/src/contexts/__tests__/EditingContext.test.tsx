import {
  EditingContextProvider,
  EditingContextProviderProps,
  useEditingContext,
} from '@admin/contexts/EditingContext';
import { OmitStrict } from '@common/types';
import React, { FC } from 'react';
import { act, renderHook } from '@testing-library/react-hooks';

describe('EditingContext', () => {
  type Props = OmitStrict<EditingContextProviderProps, 'children'>;

  const wrapper: FC<Props> = ({ ...props }) => (
    <EditingContextProvider {...props}>{props.children}</EditingContextProvider>
  );

  test('setEditingMode', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
      },
    });
    expect(result.current.editingMode).toBe('edit');

    act(() => {
      result.current.setEditingMode('table-preview');
    });

    expect(result.current.editingMode).toBe('table-preview');
  });

  test('addUnsavedBlock', () => {
    const { result } = renderHook(() => useEditingContext(), { wrapper });
    expect(result.current.unsavedBlocks).toEqual([]);

    act(() => {
      result.current.addUnsavedBlock('block-1');
    });

    expect(result.current.unsavedBlocks).toEqual(['block-1']);
  });

  test('removeUnsavedBlock', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unsavedBlocks: ['block-1', 'block-2', 'block-3'],
      },
    });

    expect(result.current.unsavedBlocks).toEqual([
      'block-1',
      'block-2',
      'block-3',
    ]);

    act(() => {
      result.current.removeUnsavedBlock('block-2');
    });

    expect(result.current.unsavedBlocks).toEqual(['block-1', 'block-3']);
  });

  test('updateUnresolvedComments - stores unresolved comment by block', () => {
    const { result } = renderHook(() => useEditingContext(), { wrapper });

    expect(result.current.unresolvedComments).toEqual({});

    act(() => {
      result.current.updateUnresolvedComments.current('block-1', 'comment-1');
    });

    expect(result.current.unresolvedComments).toEqual({
      'block-1': ['comment-1'],
    });
  });

  test('updateUnresolvedComments - stores another unresolved comment on the same block', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unresolvedComments: { 'block-1': ['comment-1'] },
      },
    });

    act(() => {
      result.current.updateUnresolvedComments.current('block-1', 'comment-2');
    });

    expect(result.current.unresolvedComments).toEqual({
      'block-1': ['comment-1', 'comment-2'],
    });
  });

  test('updateUnresolvedComments - stores another unresolved comment on a different block', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unresolvedComments: { 'block-1': ['comment-1', 'comment-2'] },
      },
    });

    act(() => {
      result.current.updateUnresolvedComments.current('block-2', 'comment-3');
    });

    expect(result.current.unresolvedComments).toEqual({
      'block-1': ['comment-1', 'comment-2'],
      'block-2': ['comment-3'],
    });
  });

  test('updateUnresolvedComments -  removes unresolved comment', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unresolvedComments: { 'block-1': ['comment-1', 'comment-2'] },
      },
    });

    act(() => {
      result.current.updateUnresolvedComments.current('block-1', 'comment-1');
    });

    expect(result.current.unresolvedComments).toEqual({
      'block-1': ['comment-2'],
    });
  });

  test('updateUnsavedCommentDeletions - stores unresolved comment by block', () => {
    const { result } = renderHook(() => useEditingContext(), { wrapper });

    expect(result.current.unsavedCommentDeletions).toEqual({});

    act(() => {
      result.current.updateUnsavedCommentDeletions.current(
        'block-1',
        'comment-1',
      );
    });

    expect(result.current.unsavedCommentDeletions).toEqual({
      'block-1': ['comment-1'],
    });
  });

  test('updateUnsavedCommentDeletions - stores another unresolved comment on the same block', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unsavedCommentDeletions: { 'block-1': ['comment-1'] },
      },
    });

    act(() => {
      result.current.updateUnsavedCommentDeletions.current(
        'block-1',
        'comment-2',
      );
    });

    expect(result.current.unsavedCommentDeletions).toEqual({
      'block-1': ['comment-1', 'comment-2'],
    });
  });

  test('updateUnsavedCommentDeletions - stores another unresolved comment on a different block', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unsavedCommentDeletions: { 'block-1': ['comment-1', 'comment-2'] },
      },
    });

    act(() => {
      result.current.updateUnsavedCommentDeletions.current(
        'block-2',
        'comment-3',
      );
    });

    expect(result.current.unsavedCommentDeletions).toEqual({
      'block-1': ['comment-1', 'comment-2'],
      'block-2': ['comment-3'],
    });
  });

  test('updateUnsavedCommentDeletions -  removes unresolved comment', () => {
    const { result } = renderHook(() => useEditingContext(), {
      wrapper,
      initialProps: {
        editingMode: 'edit',
        unsavedCommentDeletions: { 'block-1': ['comment-1', 'comment-2'] },
      },
    });

    act(() => {
      result.current.updateUnsavedCommentDeletions.current(
        'block-1',
        'comment-1',
      );
    });

    expect(result.current.unsavedCommentDeletions).toEqual({
      'block-1': ['comment-2'],
    });
  });
});
