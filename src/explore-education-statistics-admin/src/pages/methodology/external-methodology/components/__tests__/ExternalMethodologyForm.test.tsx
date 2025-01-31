import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import render from '@common-test/render';

describe('ExternalMethodologyForm', () => {
  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ExternalMethodologyForm onSubmit={handleSubmit} onCancel={noop} />,
    );

    await user.type(screen.getByLabelText('Link title'), 'Test title');

    await user.type(screen.getByLabelText('URL'), 'hive.co.uk');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        url: 'https://hive.co.uk',
      });
    });
  });

  test('show validation errors when no external methodology link title', async () => {
    const { user } = render(
      <ExternalMethodologyForm onSubmit={noop} onCancel={noop} />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology link title', {
          selector: '#methodology-external-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when no external methodology URL', async () => {
    const { user } = render(
      <ExternalMethodologyForm onSubmit={noop} onCancel={noop} />,
    );

    await user.clear(screen.getByLabelText('URL'));
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology URL', {
          selector: '#methodology-external-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when invalid external methodology URL', async () => {
    const { user } = render(
      <ExternalMethodologyForm onSubmit={noop} onCancel={noop} />,
    );

    await user.type(screen.getByLabelText('URL'), 'not a valid url');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid external methodology URL', {
          selector: '#methodology-external-url-error',
        }),
      ).toBeInTheDocument();
    });
  });
});
