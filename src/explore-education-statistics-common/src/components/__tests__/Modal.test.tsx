import Modal from '@common/components/Modal';
import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';

describe('Modal', () => {
  const originalAppRootId = process.env.APP_ROOT_ID;

  afterEach(() => {
    process.env.APP_ROOT_ID = originalAppRootId;
  });

  test('renders when `mounted` prop changes from false to true', () => {
    const { rerender, queryByText } = render(
      <Modal title="Test modal" onExit={() => {}} mounted={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(queryByText('Test modal')).toBeNull();

    rerender(
      <Modal title="Test modal" onExit={() => {}} mounted>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(queryByText('Test modal')).not.toBeNull();
  });

  test('does not render when `mounted` prop changes from true to false', () => {
    const { rerender, queryByText } = render(
      <Modal title="Test modal" onExit={() => {}} mounted>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(queryByText('Test modal')).not.toBeNull();

    rerender(
      <Modal title="Test modal" onExit={() => {}} mounted={false}>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(queryByText('Test modal')).toBeNull();
  });

  test('when mounted, `onEnter` handler is called', () => {
    const onEnter = jest.fn();

    const { rerender } = render(
      <Modal
        title="Test modal"
        onEnter={onEnter}
        onExit={() => {}}
        mounted={false}
      >
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onEnter).not.toHaveBeenCalled();

    rerender(
      <Modal title="Test modal" onEnter={onEnter} onExit={() => {}} mounted>
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onEnter).toHaveBeenCalled();
  });

  test('pressing Esc key calls `onExit` handler', async () => {
    const onExit = jest.fn();

    const { container } = render(
      <Modal title="Test modal" onExit={onExit} mounted>
        <button type="button">Close</button>
      </Modal>,
    );

    await wait();

    expect(onExit).not.toHaveBeenCalled();

    fireEvent.keyDown(container, {
      key: 'Esc',
    });

    expect(onExit).toHaveBeenCalled();
  });

  test('mouseDown on underlay of modal calls `onExit` handler', async () => {
    const onExit = jest.fn();

    const { baseElement } = render(
      <Modal
        title="Test modal"
        underlayClass="underlay"
        onExit={onExit}
        mounted
      >
        <button type="button">Close</button>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    fireEvent.mouseDown(baseElement.querySelector('.underlay') as HTMLElement);

    expect(onExit).toHaveBeenCalled();
  });

  test('when mounted, root element has aria-hidden=true', async () => {
    process.env.APP_ROOT_ID = 'root';

    const { container } = render(
      <div id="root">
        <Modal title="Test modal" onExit={() => {}} mounted>
          <button type="button">Close</button>
        </Modal>
      </div>,
    );

    await wait();

    expect(container.querySelector('#root')).toHaveAttribute(
      'aria-hidden',
      'true',
    );
  });

  test('when unmounted, root element has aria-hidden=false', async () => {
    process.env.APP_ROOT_ID = 'root';

    const { container, rerender } = render(
      <div id="root">
        <Modal title="Test modal" onExit={() => {}} mounted>
          <button type="button">Close</button>
        </Modal>
      </div>,
    );

    await wait();

    expect(container.querySelector('#root')).toHaveAttribute(
      'aria-hidden',
      'true',
    );

    rerender(
      <div id="root">
        <Modal title="Test modal" onExit={() => {}} mounted={false}>
          <button type="button">Close</button>
        </Modal>
      </div>,
    );

    expect(container.querySelector('#root')).toHaveAttribute(
      'aria-hidden',
      'false',
    );
  });
});
