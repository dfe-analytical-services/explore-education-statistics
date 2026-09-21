import ModalConfirm from '@common/components/ModalConfirm';
import createDeferredHandler from '@common-test/createDeferredHandler';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ModalConfirm', () => {
  describe('confirming', () => {
    test('clicking Confirm button disables all buttons and shows loading spinner', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const { handler: handleConfirm } = createDeferredHandler();

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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      expect(handleConfirm).toHaveBeenCalled();

      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    });

    test('clicking Confirm button prevents closing modal using Esc', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const { handler: handleConfirm } = createDeferredHandler();

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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      await userEvent.keyboard('[Escape]');

      expect(handleExit).not.toHaveBeenCalled();

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    test('clicking Confirm button prevents closing modal by clicking the underlay', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const { handler: handleConfirm } = createDeferredHandler();

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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      expect(handleExit).not.toHaveBeenCalled();

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    test('closes the modal once `onConfirm` has completed', async () => {
      const handleExit = jest.fn();
      const handleCancel = jest.fn();
      const { handler: handleConfirm, resolveHandler: resolveConfirm } =
        createDeferredHandler();

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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      await resolveConfirm();

      expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
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
      const { handler: handleCancel } = createDeferredHandler();
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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      expect(handleCancel).toHaveBeenCalled();

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
    });

    test('clicking Cancel button prevents closing modal using Esc', async () => {
      const handleExit = jest.fn();
      const { handler: handleCancel } = createDeferredHandler();
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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      await userEvent.keyboard('[Escape]');

      expect(handleExit).not.toHaveBeenCalled();

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    test('clicking Cancel button prevents closing modal by clicking the underlay', async () => {
      const handleExit = jest.fn();
      const { handler: handleCancel } = createDeferredHandler();
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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      expect(handleExit).not.toHaveBeenCalled();

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    test('closes the modal once `onCancel` has completed', async () => {
      const handleExit = jest.fn();
      const { handler: handleCancel, resolveHandler: resolveCancel } =
        createDeferredHandler();
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

      expect(await screen.findByTestId('loadingSpinner')).toBeInTheDocument();

      expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();

      await resolveCancel();

      expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });
});
