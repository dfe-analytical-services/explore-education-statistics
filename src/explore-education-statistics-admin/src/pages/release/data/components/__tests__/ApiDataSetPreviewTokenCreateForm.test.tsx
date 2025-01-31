import ApiDataSetPreviewTokenCreateForm from '@admin/pages/release/data/components/ApiDataSetPreviewTokenCreateForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';

describe('ApiDataSetPreviewTokenCreateForm', () => {
  test('renders the form correctly', () => {
    render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    expect(screen.getByLabelText('Token name')).toBeInTheDocument();
    expect(screen.getByLabelText(/I agree/)).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Continue' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows validation error when no label given', async () => {
    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('Enter a token name', {
        selector: '#apiDataSetTokenCreateForm-label-error',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation error when agreeing to the terms of usage not checked', async () => {
    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('The terms of usage must be agreed', {
        selector: '#apiDataSetTokenCreateForm-terms-error',
      }),
    ).toBeInTheDocument();
  });

  test('submitting form successfully calls `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.type(screen.getByLabelText('Token name'), 'Test label');
    await user.click(screen.getByLabelText(/I agree/));

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => expect(handleSubmit).toHaveBeenCalledTimes(1));

    expect(handleSubmit).toHaveBeenCalledWith('Test label');
  });

  test('submitting form with validation error does not call `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('Enter a token name', {
        selector: '#apiDataSetTokenCreateForm-label-error',
      }),
    ).toBeInTheDocument();

    await waitFor(() => expect(handleSubmit).not.toHaveBeenCalled());
  });

  test('clicking the cancel button calls the `onCancel` handler', async () => {
    const handleCancel = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={handleCancel}
        onSubmit={noop}
      />,
    );

    expect(handleCancel).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => expect(handleCancel).toHaveBeenCalled());
  });
});
