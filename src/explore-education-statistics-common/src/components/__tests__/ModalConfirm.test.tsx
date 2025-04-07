import ModalConfirm from '@common/components/ModalConfirm';
import delay from '@common/utils/delay';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ModalConfirm', () => {
  describe('confirming', () => {
    test('clicking Confirm button disables all buttons and shows loading spinner', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn(async () => {
        await delay(100);
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

      expect(
        await screen.findByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
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

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await userEvent.keyboard('[Escape]');

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

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('closes the modal once `onConfirm` has completed', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn(async () => {
        await delay(100);
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

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      await waitFor(() => {
        expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('cancelling', () => {
    test('clicking Cancel button calls `onExit` when no `onCancel` set', async () => {
      const handleExit = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onConfirm={handleConfirm}
          onExit={handleExit}
        />,
      );

      expect(handleExit).not.toHaveBeenCalled();

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(handleExit).toHaveBeenCalledTimes(1);
      });
    });

    test('clicking Cancel button calls `onCancel` when set', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const handleConfirm = jest.fn();

      render(
        <ModalConfirm
          open
          title="Test modal"
          triggerButton={<button type="button">Open</button>}
          onCancel={handleCancel}
          onConfirm={handleConfirm}
          onExit={handleExit}
        />,
      );

      expect(handleCancel).not.toHaveBeenCalled();
      expect(handleExit).not.toHaveBeenCalled();

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(handleCancel).toHaveBeenCalledTimes(1);
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('clicking Cancel button disables all buttons and shows loading spinner', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn(async () => {
        await delay(100);
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

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();

      expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
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

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await userEvent.keyboard('[Escape]');
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

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      await waitFor(() => {
        expect(handleExit).not.toHaveBeenCalled();
      });
    });

    test('closes the modal once `onCancel` has completed', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn(async () => {
        await delay(100);
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

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();

      await waitFor(() => {
        expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });
});
