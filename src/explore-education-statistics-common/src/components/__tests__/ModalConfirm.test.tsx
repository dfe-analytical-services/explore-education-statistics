import ModalConfirm from '@common/components/ModalConfirm';
import delay from '@common/utils/delay';
import {
  fireEvent,
  render as baseRender,
  screen,
  waitFor,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { ReactElement } from 'react';

describe('ModalConfirm', () => {
  const originalAppRootId = process.env.APP_ROOT_ID;

  beforeAll(() => {
    process.env.APP_ROOT_ID = 'root';
  });

  afterAll(() => {
    process.env.APP_ROOT_ID = originalAppRootId;
  });

  describe('confirming', () => {
    test('clicking Confirm button disables all buttons', () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    });

    test('clicking Confirm button prevents closing modal using Esc', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      fireEvent.keyDown(screen.getByRole('dialog'), { key: 'Esc' });

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('clicking Confirm button prevents closing modal by clicking the underlay', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      const { baseElement } = render(
        <ModalConfirm
          open
          underlayClass="underlay"
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      userEvent.click(baseElement.querySelector('.underlay') as HTMLElement);

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('re-enables buttons once `onConfirm` has completed', async () => {
      jest.useFakeTimers();

      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn(async () => {
        await delay(500);
      });

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      jest.advanceTimersByTime(500);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Confirm' })).toBeEnabled();
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
      });
    });

    test('re-enables exiting once `onConfirm` has completed', async () => {
      jest.useFakeTimers();

      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn(async () => {
        await delay(500);
      });

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      jest.advanceTimersByTime(500);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Confirm' })).toBeEnabled();
      });

      fireEvent.keyDown(screen.getByRole('dialog'), {
        key: 'Esc',
        keyCode: 27,
      });

      await waitFor(() => {
        expect(handleExit).toHaveBeenCalled();
      });
    });
  });

  describe('cancelling', () => {
    test('clicking Cancel button disables all buttons', () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    });

    test('clicking Cancel button prevents closing modal using Esc', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      fireEvent.keyDown(screen.getByRole('dialog'), {
        key: 'Esc',
        keyCode: 27,
      });

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('clicking Cancel button prevents closing modal by clicking the underlay', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      const { baseElement } = render(
        <ModalConfirm
          open
          underlayClass="underlay"
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
      userEvent.click(baseElement.querySelector('.underlay') as HTMLElement);

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('re-enables buttons once `onCancel` has completed', async () => {
      jest.useFakeTimers();

      const handleExit = jest.fn();
      const handleCancel = jest.fn(async () => {
        await delay(500);
      });
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      jest.advanceTimersByTime(500);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Confirm' })).toBeEnabled();
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
      });
    });

    test('re-enables exiting once `onCancel` has completed', async () => {
      jest.useFakeTimers();

      const handleExit = jest.fn();
      const handleCancel = jest.fn(async () => {
        await delay(500);
      });
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
          title="Test modal"
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      jest.advanceTimersByTime(500);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
      });

      fireEvent.keyDown(screen.getByRole('dialog'), {
        key: 'Esc',
        keyCode: 27,
      });

      await waitFor(() => {
        expect(handleExit).toHaveBeenCalled();
      });
    });
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
