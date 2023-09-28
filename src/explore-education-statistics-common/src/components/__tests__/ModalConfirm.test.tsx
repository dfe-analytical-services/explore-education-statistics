import flushPromises from '@common-test/flushPromises';
import ModalConfirm from '@common/components/ModalConfirm';
import delay from '@common/utils/delay';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ModalConfirm', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });
  describe('confirming', () => {
    test('clicking Confirm button disables all buttons', () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
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
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
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
          triggerButton={<button type="button">Open</button>}
          title="Test modal"
          underlayClass="underlay"
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      userEvent.click(baseElement.querySelector('.underlay') as HTMLElement);

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('closes the modal once `onConfirm` has completed', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn(async () => {
        await delay(500);
      });

      render(
        <ModalConfirm
          open
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      jest.advanceTimersByTime(500);

      await flushPromises();

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
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
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
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
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      fireEvent.keyDown(screen.getByRole('dialog'), {
        key: 'Escape',
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
          triggerButton={<button type="button">Open</button>}
          title="Test modal"
          underlayClass="underlay"
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
      userEvent.click(baseElement.querySelector('.underlay') as HTMLElement);

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('closes the modal once `onCancel` has completed', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn(async () => {
        await delay(500);
      });
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          onExit={handleExit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      jest.advanceTimersByTime(500);

      await flushPromises();

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      });
    });
  });
});
