import Modal from '@common/components/Modal';
import {
  fireEvent,
  render as baseRender,
  screen,
  waitFor,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { ReactElement } from 'react';

describe('Modal', () => {
  const originalAppRootId = process.env.APP_ROOT_ID;

  beforeAll(() => {
    process.env.APP_ROOT_ID = 'root';
  });

  afterAll(() => {
    process.env.APP_ROOT_ID = originalAppRootId;
  });

  test('renders when `open` prop changes from false to true', () => {
    const { rerender } = render(
      <Modal title="Test modal" open={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();

    rerender(
      <Modal title="Test modal" open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();
  });

  test('does not render when `open` prop changes from true to false', () => {
    const { rerender } = render(
      <Modal title="Test modal" open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();

    rerender(
      <Modal title="Test modal" open={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();
  });

  test('when open,`onOpen` handler is called', async () => {
    const onOpen = jest.fn();

    const { rerender } = render(
      <Modal title="Test modal" onOpen={onOpen} open={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onOpen).not.toHaveBeenCalled();

    rerender(
      <Modal title="Test modal" onOpen={onOpen} open>
        <button type="button">Close</button>
      </Modal>,
    );

    await waitFor(() => {
      expect(onOpen).toHaveBeenCalled();
    });
  });

  test('pressing Esc key calls `onExit` handler', () => {
    const onExit = jest.fn();

    render(
      <Modal title="Test modal" onExit={onExit} open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    fireEvent.keyDown(screen.getByRole('dialog'), {
      key: 'Esc',
      keyCode: 27,
    });

    expect(onExit).toHaveBeenCalled();
  });

  test('mouseDown on underlay of modal calls `onExit` handler', () => {
    const onExit = jest.fn();

    const { baseElement } = render(
      <Modal title="Test modal" underlayClass="underlay" onExit={onExit} open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    userEvent.click(baseElement.querySelector('.underlay') as HTMLElement);

    expect(onExit).toHaveBeenCalled();
  });

  test('when open, root element has aria-hidden=true', () => {
    const { container } = render(
      <Modal title="Test modal" open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');
  });

  test('when closed, root element does not have aria-hidden', () => {
    const { container, rerender } = render(
      <Modal title="Test modal" open>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');

    rerender(
      <Modal title="Test modal" open={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(container).not.toHaveAttribute('aria-hidden');
  });

  const render = (element: ReactElement) => {
    const container = document.createElement('div');
    container.id = process.env.APP_ROOT_ID;

    document.body.appendChild(container);

    return baseRender(element, {
      container,
      baseElement: document.body,
    });
  };
});
