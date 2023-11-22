import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('CommentsWrapper', () => {
  test('renders the child element', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.getByText('child block')).toBeInTheDocument();
  });

  test('renders the add comment form when `showCommentAddForm` is true', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          showCommentAddForm
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.getByTestId('comment-add-form')).toBeInTheDocument();
  });

  test('does not render the add comment form when `showCommentAddForm` is false', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          showCommentAddForm={false}
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.queryByTestId('comment-add-form')).not.toBeInTheDocument();
  });

  test('renders the comments list when there are comments and `showCommentsList` is true', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          showCommentsList
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.getByTestId('comments-unresolved')).toBeInTheDocument();
  });

  test('does not render the comments list when there are no comments and `showCommentsList` is true', () => {
    render(
      <CommentsContextProvider
        comments={[]}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          showCommentsList
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.queryByTestId('comments-unresolved')).not.toBeInTheDocument();
  });

  test('does not render the comments list when there are comments and `showCommentsList` is false', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          commentType="inline"
          id="id"
          showCommentsList={false}
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.queryByTestId('comments-unresolved')).not.toBeInTheDocument();
  });

  test('does not render the sidebar if allow comments is false', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        onDelete={() => Promise.resolve()}
        onCreate={jest.fn()}
        onUpdate={() => Promise.resolve()}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsWrapper
          allowComments={false}
          commentType="inline"
          id="id"
          showCommentsList
          onAdd={noop}
          onAddCancel={noop}
          onAddSave={noop}
        >
          <p>child block</p>
        </CommentsWrapper>
      </CommentsContextProvider>,
    );

    expect(screen.queryByTestId('comments-sidebar')).not.toBeInTheDocument();
  });

  describe('inline comments', () => {
    test('renders the view button when there are comments and `showCommentsList` is false', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={() => Promise.resolve()}
          onCreate={jest.fn()}
          onUpdate={() => Promise.resolve()}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <CommentsWrapper
            commentType="inline"
            id="id"
            showCommentsList={false}
            onAdd={noop}
            onAddCancel={noop}
            onAddSave={noop}
          >
            <p>child block</p>
          </CommentsWrapper>
        </CommentsContextProvider>,
      );

      expect(
        screen.getByRole('button', { name: 'View comments (3 unresolved)' }),
      ).toBeInTheDocument();
    });

    test('does not render the view button when there are comments and `showCommentsList` is true', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={() => Promise.resolve()}
          onCreate={jest.fn()}
          onUpdate={() => Promise.resolve()}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <CommentsWrapper
            commentType="inline"
            id="id"
            showCommentsList
            onAdd={noop}
            onAddCancel={noop}
            onAddSave={noop}
          >
            <p>child block</p>
          </CommentsWrapper>
        </CommentsContextProvider>,
      );

      expect(
        screen.queryByRole('button', { name: 'View comments (3 unresolved)' }),
      ).not.toBeInTheDocument();
    });

    test('does not render the view button when there are no comments and `showCommentsList` is false', () => {
      render(
        <CommentsContextProvider
          comments={[]}
          onDelete={() => Promise.resolve()}
          onCreate={jest.fn()}
          onUpdate={() => Promise.resolve()}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <CommentsWrapper
            commentType="inline"
            id="id"
            showCommentsList={false}
            onAdd={noop}
            onAddCancel={noop}
            onAddSave={noop}
          >
            <p>child block</p>
          </CommentsWrapper>
        </CommentsContextProvider>,
      );

      expect(
        screen.queryByRole('button', { name: 'View comments (3 unresolved)' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('block comments', () => {
    test('renders the add comment button when `showCommentAddForm` is false', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={() => Promise.resolve()}
          onCreate={jest.fn()}
          onUpdate={() => Promise.resolve()}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <CommentsWrapper
            commentType="block"
            id="id"
            showCommentAddForm={false}
            onAdd={noop}
            onAddCancel={noop}
            onAddSave={noop}
          >
            <p>child block</p>
          </CommentsWrapper>
        </CommentsContextProvider>,
      );

      expect(screen.getByTestId('comment-add-button')).toBeInTheDocument();
    });

    test('does not render the add comment button when `showCommentAddForm` is true', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={() => Promise.resolve()}
          onCreate={jest.fn()}
          onUpdate={() => Promise.resolve()}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <CommentsWrapper
            commentType="block"
            id="id"
            showCommentAddForm
            onAdd={noop}
            onAddCancel={noop}
            onAddSave={noop}
          >
            <p>child block</p>
          </CommentsWrapper>
        </CommentsContextProvider>,
      );

      expect(
        screen.queryByTestId('comment-add-button'),
      ).not.toBeInTheDocument();
    });
  });
});
