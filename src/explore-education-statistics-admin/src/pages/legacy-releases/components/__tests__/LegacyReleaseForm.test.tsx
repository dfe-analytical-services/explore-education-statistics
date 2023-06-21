import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('LegacyReleaseForm', () => {
  test('shows validation error for empty `description`', async () => {
    render(<LegacyReleaseForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a description')).not.toBeInTheDocument();

    const description = screen.getByLabelText('Description');

    userEvent.click(description);
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a description', {
          selector: '#legacyReleaseForm-description-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for empty `url`', async () => {
    render(<LegacyReleaseForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a URL')).not.toBeInTheDocument();

    userEvent.click(screen.getByLabelText('URL'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a URL', {
          selector: '#legacyReleaseForm-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for invalid `url`', async () => {
    render(<LegacyReleaseForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a valid URL')).not.toBeInTheDocument();

    userEvent.type(screen.getByLabelText('URL'), 'not a url');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid URL', {
          selector: '#legacyReleaseForm-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('with no `initialValues`', () => {
    test('renders correctly`', () => {
      render(<LegacyReleaseForm onSubmit={noop} />);

      expect(screen.getByLabelText('Description')).toHaveAttribute('value', '');
      expect(screen.getByLabelText('URL')).toHaveAttribute('value', '');
      expect(screen.queryByLabelText('Order')).not.toBeInTheDocument();
    });

    test('cannot submit with only invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<LegacyReleaseForm onSubmit={handleSubmit} />);

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('cannot submit with some invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<LegacyReleaseForm onSubmit={handleSubmit} />);

      userEvent.type(screen.getByLabelText('Description'), 'Test description');

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('can submit with valid values', async () => {
      const handleSubmit = jest.fn();

      render(<LegacyReleaseForm onSubmit={handleSubmit} />);

      userEvent.type(screen.getByLabelText('Description'), 'Test description');
      userEvent.type(screen.getByLabelText('URL'), 'http://test.com');

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit.mock.calls[0][0]).toEqual({
          description: 'Test description',
          url: 'http://test.com',
        });
      });
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly', () => {
      render(
        <LegacyReleaseForm
          initialValues={{
            description: 'Test',
            url: 'http://test.com',
            order: 1,
          }}
          onSubmit={noop}
        />,
      );

      expect(screen.getByLabelText('Description')).toHaveAttribute(
        'value',
        'Test',
      );
      expect(screen.getByLabelText('URL')).toHaveAttribute(
        'value',
        'http://test.com',
      );
      expect(screen.getByLabelText('Order')).toHaveAttribute('value', '1');
    });

    test('cannot submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<LegacyReleaseForm onSubmit={handleSubmit} />);

      userEvent.clear(screen.getByLabelText('Description'));

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('can submit with valid initial values', async () => {
      const handleSubmit = jest.fn();

      render(
        <LegacyReleaseForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
            order: 1,
          }}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit.mock.calls[0][0]).toEqual({
          description: 'Test description',
          url: 'http://test.com',
          order: 1,
        });
      });
    });

    test('can submit with valid values', async () => {
      const handleSubmit = jest.fn();

      render(
        <LegacyReleaseForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
            order: 1,
          }}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.type(screen.getByLabelText('Description'), ' 2');
      userEvent.type(screen.getByLabelText('URL'), '/updated');
      userEvent.type(screen.getByLabelText('Order'), '0');

      userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit.mock.calls[0][0]).toEqual({
          description: 'Test description 2',
          url: 'http://test.com/updated',
          order: 10,
        });
      });
    });
  });
});
