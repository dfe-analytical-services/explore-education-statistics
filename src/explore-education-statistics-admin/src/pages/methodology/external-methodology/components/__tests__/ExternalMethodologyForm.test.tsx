import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('ExternalMethodologyForm', () => {
  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<ExternalMethodologyForm onSubmit={handleSubmit} onCancel={noop} />);

    await userEvent.type(screen.getByLabelText('Link title'), 'Test title');

    await userEvent.type(screen.getByLabelText('URL'), 'hive.co.uk');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        url: 'https://hive.co.uk',
      });
    });
  });

  test('show validation errors when no external methodology link title', async () => {
    render(<ExternalMethodologyForm onSubmit={noop} onCancel={noop} />);

    userEvent.click(screen.getByLabelText('Link title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology link title', {
          selector: '#methodology-external-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when no external methodology URL', async () => {
    render(<ExternalMethodologyForm onSubmit={noop} onCancel={noop} />);

    await userEvent.clear(screen.getByLabelText('URL'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology URL', {
          selector: '#methodology-external-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when invalid external methodology URL', async () => {
    render(<ExternalMethodologyForm onSubmit={noop} onCancel={noop} />);

    await userEvent.type(screen.getByLabelText('URL'), 'not a valid url');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid external methodology URL', {
          selector: '#methodology-external-url-error',
        }),
      ).toBeInTheDocument();
    });
  });
});
