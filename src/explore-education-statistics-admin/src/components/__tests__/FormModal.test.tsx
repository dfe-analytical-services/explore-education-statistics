import delay from '@common/utils/delay';
import { render, RenderResult, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { ReactNode } from 'react';
import { FieldValues } from 'react-hook-form';
import FormModal from '../FormModal';

describe('FormModal', () => {
  test('trigger button opens modal', async () => {
    const handleSubmit = jest.fn(async () => {
      await delay(100);
    });

    await renderModal(handleSubmit, false, undefined, false);

    await userEvent.click(screen.getByRole('button', { name: 'Open' }));

    expect(screen.getByTestId('modal-underlay')).toBeInTheDocument();

    expect(screen.getByText('Test Modal')).toBeInTheDocument();
  });

  describe('submitting WITHOUT confirmation warning', () => {
    test('clicking submit button disables all buttons and shows loading spinner', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, false);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();

      expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
    });

    test('clicking submit button prevents closing modal using Esc', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, false);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      await userEvent.keyboard('[Escape]');

      expect(screen.getByText('Test Modal')).toBeInTheDocument();
    });

    test('clicking submit button prevents closing modal by clicking the underlay', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      const { baseElement } = await renderModal(handleSubmit, false);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      expect(screen.getByText('Test Modal')).toBeInTheDocument();
    });

    test('closes the modal once `onSubmit` has completed', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, false);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      await waitFor(() => {
        expect(screen.queryByText('Save')).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('cancelling WITHOUT confirmation warning', () => {
    test('clicking Cancel button closes modal', async () => {
      const handleSubmit = jest.fn();

      await renderModal(handleSubmit, false);

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.queryByText('Save')).not.toBeInTheDocument();
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('submitting WITH confirmation warning', () => {
    test('clicking FIRST submit button displays confirmation warning', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, true, <p>Warning text.</p>);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(screen.getByText('Warning text.')).toBeInTheDocument();
    });

    test('clicking SECOND confirmation button prevents closing modal using Esc', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, true, <p>Warning text.</p>);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      await userEvent.keyboard('[Escape]');

      expect(screen.getByText('Test Modal')).toBeInTheDocument();
    });

    test('clicking SECOND confirmation button prevents closing modal by clicking the underlay', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      const { baseElement } = await renderModal(
        handleSubmit,
        true,
        <p>Warning text.</p>,
      );

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      await userEvent.click(
        baseElement.querySelector('.underlay') as HTMLElement,
      );

      expect(screen.getByText('Test Modal')).toBeInTheDocument();
    });

    test('closes the modal once `onSubmit` has completed after clicking SECOND confirmation button', async () => {
      const handleSubmit = jest.fn(async () => {
        await delay(100);
      });

      await renderModal(handleSubmit, true, <p>Warning text.</p>);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalled();
      });

      await waitFor(() => {
        expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('cancelling WITH confirmation warning', () => {
    test('clicking SECOND Cancel button displays initial modal content and removes the confirmation warning', async () => {
      const handleSubmit = jest.fn();

      await renderModal(handleSubmit, true, <p>Warning text.</p>);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(screen.getByText('Warning text.')).toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.queryByText('Warning text.')).not.toBeInTheDocument();

      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(screen.queryByRole('dialog')).toBeInTheDocument();

      expect(screen.getByText('Child node.')).toBeInTheDocument();
    });
  });

  async function renderModal<TFormValues extends FieldValues>(
    onSubmit: (formValues: TFormValues) => Promise<void>,
    withConfirmationWarning: boolean,
    confirmationWarningText?: ReactNode,
    open: boolean = true,
  ): Promise<RenderResult> {
    const renderResult = render(
      <FormModal
        underlayClass=".underlay"
        title="Test Modal"
        formId="testForm"
        triggerButton={<button type="button">Open</button>}
        onSubmit={onSubmit}
        confirmationWarningText={confirmationWarningText}
      >
        <div>Child node.</div>
      </FormModal>,
    );

    expect(
      await screen.findByRole('button', { name: 'Open' }),
    ).toBeInTheDocument();

    if (open) {
      await userEvent.click(screen.getByRole('button', { name: 'Open' }));

      expect(
        await screen.findByRole('button', { name: 'Save' }),
      ).toBeInTheDocument();

      expect(
        await screen.findByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(await screen.findByText('Child node.')).toBeInTheDocument();
    }

    return renderResult;
  }
});
