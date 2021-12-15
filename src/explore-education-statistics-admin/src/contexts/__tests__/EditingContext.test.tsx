import {
  BlockCommentIds,
  EditingProvider,
  useEditingContext,
} from '@admin/contexts/EditingContext';
import React from 'react';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

describe('EditingContext', () => {
  const setUp = ({
    initialUnsavedBlocks = [],
    initialUnsavedCommentDeletions = {},
    initialUnresolvedComments = {},
  }: {
    initialUnsavedBlocks?: string[];
    initialUnsavedCommentDeletions?: BlockCommentIds;
    initialUnresolvedComments?: BlockCommentIds;
  }) => {
    const TestComponent = () => {
      const {
        editingMode,
        setEditingMode,
        unresolvedComments,
        unsavedBlocks,
        unsavedCommentDeletions,
        addUnsavedBlock,
        removeUnsavedBlock,
        clearUnsavedCommentDeletions,
        updateUnresolvedComments,
        updateUnsavedCommentDeletions,
      } = useEditingContext();

      return (
        <>
          <button type="button" onClick={() => setEditingMode('preview')}>
            Set editing mode
          </button>

          <button type="button" onClick={() => addUnsavedBlock('block-1')}>
            Add unsaved block
          </button>

          <button type="button" onClick={() => removeUnsavedBlock('block-2')}>
            Remove unsaved block
          </button>

          <button
            type="button"
            onClick={() => clearUnsavedCommentDeletions('block-1')}
          >
            Remove unsaved deletions for block
          </button>

          <button
            type="button"
            onClick={() =>
              updateUnresolvedComments.current('block-1', 'comment-1')
            }
          >
            Update unresolved comments
          </button>

          <button
            type="button"
            onClick={() =>
              updateUnsavedCommentDeletions.current('block-1', 'comment-1')
            }
          >
            Update unsaved comment deletions
          </button>

          <div data-testid="editingMode">{editingMode}</div>

          <div data-testid="unresolvedComments">
            {Object.keys(unresolvedComments).map(unresolvedCommentBlock => (
              <ul
                data-testid={`unresolvedCommentBlock-${unresolvedCommentBlock}`}
                key={unresolvedCommentBlock}
              >
                {unresolvedComments[unresolvedCommentBlock].map(comment => (
                  <li key={comment} id={comment} />
                ))}
              </ul>
            ))}
          </div>

          <div data-testid="unsavedCommentDeletions">
            {Object.keys(unsavedCommentDeletions).map(
              unsavedCommentDeletionsBlock => (
                <ul
                  data-testid={`unsavedCommentDeletionsBlock-${unsavedCommentDeletionsBlock}`}
                  key={unsavedCommentDeletionsBlock}
                >
                  {unsavedCommentDeletions[unsavedCommentDeletionsBlock].map(
                    comment => (
                      <li key={comment} id={comment} />
                    ),
                  )}
                </ul>
              ),
            )}
          </div>

          <ul data-testid="unsavedBlocks">
            {unsavedBlocks.map(block => (
              <li key={block} id={block} />
            ))}
          </ul>
        </>
      );
    };

    return render(
      <EditingProvider
        editingMode="edit"
        unresolvedComments={initialUnresolvedComments}
        unsavedBlocks={initialUnsavedBlocks}
        unsavedCommentDeletions={initialUnsavedCommentDeletions}
      >
        <TestComponent />
      </EditingProvider>,
    );
  };

  test('setEditingMode', () => {
    setUp({});
    userEvent.click(screen.getByRole('button', { name: 'Set editing mode' }));
    expect(screen.getByTestId('editingMode')).toHaveTextContent('preview');
  });

  test('addUnsavedBlock', () => {
    setUp({});
    userEvent.click(screen.getByRole('button', { name: 'Add unsaved block' }));

    const unsavedBlocks = within(
      screen.getByTestId('unsavedBlocks'),
    ).getAllByRole('listitem');
    expect(unsavedBlocks).toHaveLength(1);
    expect(unsavedBlocks[0]).toHaveAttribute('id', 'block-1');
  });

  test('removeUnsavedBlock', () => {
    setUp({ initialUnsavedBlocks: ['block-1', 'block-2', 'block-3'] });
    userEvent.click(
      screen.getByRole('button', { name: 'Remove unsaved block' }),
    );

    const unsavedBlocks = within(
      screen.getByTestId('unsavedBlocks'),
    ).getAllByRole('listitem');
    expect(unsavedBlocks).toHaveLength(2);
    expect(unsavedBlocks[0]).toHaveAttribute('id', 'block-1');
    expect(unsavedBlocks[1]).toHaveAttribute('id', 'block-3');
  });

  test('updateUnresolvedComments - stores unresolved comment by block', () => {
    setUp({});
    userEvent.click(
      screen.getByRole('button', { name: 'Update unresolved comments' }),
    );
    const block1UnresolvedComments = within(
      screen.getByTestId('unresolvedCommentBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnresolvedComments).toHaveLength(1);
    expect(block1UnresolvedComments[0]).toHaveAttribute('id', 'comment-1');
  });

  test('updateUnresolvedComments - stores another unresolved comment on the same block', () => {
    setUp({ initialUnresolvedComments: { 'block-1': ['comment-2'] } });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unresolved comments' }),
    );

    const block1UnresolvedComments = within(
      screen.getByTestId('unresolvedCommentBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnresolvedComments).toHaveLength(2);
    expect(block1UnresolvedComments[0]).toHaveAttribute('id', 'comment-2');
    expect(block1UnresolvedComments[1]).toHaveAttribute('id', 'comment-1');
  });

  test('updateUnresolvedComments - stores another unresolved comment on a different block', () => {
    setUp({
      initialUnresolvedComments: { 'block-2': ['comment-2', 'comment-3'] },
    });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unresolved comments' }),
    );

    const block1UnresolvedComments = within(
      screen.getByTestId('unresolvedCommentBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnresolvedComments).toHaveLength(1);
    expect(block1UnresolvedComments[0]).toHaveAttribute('id', 'comment-1');

    const block2UnresolvedComments = within(
      screen.getByTestId('unresolvedCommentBlock-block-2'),
    ).getAllByRole('listitem');
    expect(block2UnresolvedComments).toHaveLength(2);
    expect(block2UnresolvedComments[0]).toHaveAttribute('id', 'comment-2');
    expect(block2UnresolvedComments[1]).toHaveAttribute('id', 'comment-3');
  });

  test('updateUnresolvedComments -  removes unresolved comment', () => {
    setUp({
      initialUnresolvedComments: { 'block-1': ['comment-1', 'comment-2'] },
    });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unresolved comments' }),
    );

    const block1UnresolvedComments = within(
      screen.getByTestId('unresolvedCommentBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnresolvedComments).toHaveLength(1);
    expect(block1UnresolvedComments[0]).toHaveAttribute('id', 'comment-2');
  });

  test('updateUnsavedCommentDeletions - stores unresolved comment by block', () => {
    setUp({});
    userEvent.click(
      screen.getByRole('button', { name: 'Update unsaved comment deletions' }),
    );
    const block1UnsavedCommentDeletions = within(
      screen.getByTestId('unsavedCommentDeletionsBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnsavedCommentDeletions).toHaveLength(1);
    expect(block1UnsavedCommentDeletions[0]).toHaveAttribute('id', 'comment-1');
  });

  test('updateUnsavedCommentDeletions - stores another unresolved comment on the same block', () => {
    setUp({ initialUnsavedCommentDeletions: { 'block-1': ['comment-2'] } });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unsaved comment deletions' }),
    );

    const block1UnsavedCommentDeletions = within(
      screen.getByTestId('unsavedCommentDeletionsBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnsavedCommentDeletions).toHaveLength(2);
    expect(block1UnsavedCommentDeletions[0]).toHaveAttribute('id', 'comment-2');
    expect(block1UnsavedCommentDeletions[1]).toHaveAttribute('id', 'comment-1');
  });

  test('updateUnsavedCommentDeletions - stores another unresolved comment on a different block', () => {
    setUp({
      initialUnsavedCommentDeletions: { 'block-2': ['comment-2', 'comment-3'] },
    });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unsaved comment deletions' }),
    );

    const block1UnsavedCommentDeletions = within(
      screen.getByTestId('unsavedCommentDeletionsBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnsavedCommentDeletions).toHaveLength(1);
    expect(block1UnsavedCommentDeletions[0]).toHaveAttribute('id', 'comment-1');

    const block2UnresolvedComments = within(
      screen.getByTestId('unsavedCommentDeletionsBlock-block-2'),
    ).getAllByRole('listitem');
    expect(block2UnresolvedComments).toHaveLength(2);
    expect(block2UnresolvedComments[0]).toHaveAttribute('id', 'comment-2');
    expect(block2UnresolvedComments[1]).toHaveAttribute('id', 'comment-3');
  });

  test('updateUnsavedCommentDeletions -  removes unresolved comment', () => {
    setUp({
      initialUnsavedCommentDeletions: { 'block-1': ['comment-1', 'comment-2'] },
    });
    userEvent.click(
      screen.getByRole('button', { name: 'Update unsaved comment deletions' }),
    );

    const block1UnsavedCommentDeletions = within(
      screen.getByTestId('unsavedCommentDeletionsBlock-block-1'),
    ).getAllByRole('listitem');
    expect(block1UnsavedCommentDeletions).toHaveLength(1);
    expect(block1UnsavedCommentDeletions[0]).toHaveAttribute('id', 'comment-2');
  });
});
