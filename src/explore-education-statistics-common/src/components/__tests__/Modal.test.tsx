import render from '@common-test/render';
import Modal from '@common/components/Modal';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

describe('Modal', () => {
  test('clicking the trigger button opens the modal', async () => {
    const { user } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Open' }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();
  });

  test('clicking the close button closes the modal', async () => {
    const { user } = render(
      <Modal
        showClose
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Close modal' }));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();
  });

  test('renders when `open` prop changes from false to true', () => {
    const { rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();
  });

  test('does not render when `open` prop changes from true to false', () => {
    const { rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();
  });

  test('when opened the `onOpen` handler is called', async () => {
    const onOpen = jest.fn();

    const { user } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onOpen={onOpen}
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onOpen).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(onOpen).toHaveBeenCalled();
    });
  });

  test('closing the modal calls the `onExit` handler', async () => {
    const onExit = jest.fn();

    const { user } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onExit={onExit}
        open
        showClose
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Close modal' }));

    await waitFor(() => {
      expect(onExit).toHaveBeenCalled();
    });
  });

  test('pressing Esc key calls `onExit` handler', async () => {
    const onExit = jest.fn();

    render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onExit={onExit}
        open
        showClose
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    fireEvent.keyDown(screen.getByRole('dialog'), {
      key: 'Escape',
    });

    await waitFor(() => {
      expect(onExit).toHaveBeenCalled();
    });
  });

  test('when open, root element has aria-hidden=true', () => {
    const { container } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');
  });

  test('when closed, root element does not have aria-hidden', () => {
    const { container, rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).not.toHaveAttribute('aria-hidden');
  });

  // Note: jsdom does not load the `js-enabled` stylesheet, so these assert on
  // the `dfe-js-hidden` / `dfe-js-only` classes rather than on visibility.
  // `renderToStaticMarkup` gives the server HTML (no effects run), while
  // `render` mounts the component. Static markup goes into a detached
  // container, so it is asserted on directly rather than with
  // `toBeInTheDocument`.
  describe('noJsFallbackSummary', () => {
    test('server-renders the content in a details fallback instead of the modal', () => {
      const container = document.createElement('div');
      container.innerHTML = renderToStaticMarkup(
        <Modal
          description="Test description"
          noJsFallbackSummary="What is this?"
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
        >
          <p>Content</p>
        </Modal>,
      );

      const fallback = within(container).getByTestId('modal-no-js-fallback');
      expect(fallback.tagName).toBe('DETAILS');
      expect(fallback).toHaveTextContent('What is this?');
      expect(fallback).toHaveTextContent('Test description');
      expect(fallback).toHaveTextContent('Content');

      // No trigger, as it can't work without JS.
      expect(within(container).queryByRole('button')).toBeNull();
      expect(within(container).queryByRole('dialog')).toBeNull();
    });

    test('swaps the fallback for the modal once mounted', async () => {
      const { user } = render(
        <Modal
          noJsFallbackSummary="What is this?"
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
        >
          <p>Content</p>
        </Modal>,
      );

      expect(
        screen.queryByTestId('modal-no-js-fallback'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Content')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Open' }));

      expect(screen.getByRole('dialog')).toBeInTheDocument();
      expect(screen.getByText('Content')).toBeInTheDocument();
    });

    test('server-renders the trigger, not a fallback, by default', () => {
      const container = document.createElement('div');
      container.innerHTML = renderToStaticMarkup(
        <Modal
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
        >
          <p>Content</p>
        </Modal>,
      );

      expect(
        within(container).queryByTestId('modal-no-js-fallback'),
      ).toBeNull();
      expect(
        within(container).getByRole('button', { name: 'Open' }),
      ).toHaveTextContent('Open');
    });
  });
});
