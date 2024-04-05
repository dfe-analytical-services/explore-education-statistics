import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseSeriesLegacyLinkForm', () => {
  test('shows validation error for empty `description`', async () => {
    render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a description')).not.toBeInTheDocument();

    const description = screen.getByLabelText('Description');

    await userEvent.click(description);
    await userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a description', {
          selector: '#releaseSeriesLegacyLinkForm-description-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for empty `url`', async () => {
    render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a URL')).not.toBeInTheDocument();

    await userEvent.click(screen.getByLabelText('URL'));
    await userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a URL', {
          selector: '#releaseSeriesLegacyLinkForm-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for invalid `url`', async () => {
    render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a valid URL')).not.toBeInTheDocument();

    await userEvent.type(screen.getByLabelText('URL'), 'not a url');
    await userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid URL', {
          selector: '#releaseSeriesLegacyLinkForm-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('with no `initialValues`', () => {
    test('renders correctly`', () => {
      render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

      expect(screen.getByLabelText('Description')).toHaveAttribute('value', '');
      expect(screen.getByLabelText('URL')).toHaveAttribute('value', '');
    });

    test('cannot submit with only invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />);

      await userEvent.click(
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

      render(<ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />);

      await userEvent.type(
        screen.getByLabelText('Description'),
        'Test description',
      );

      await userEvent.click(
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

      render(<ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />);

      await userEvent.type(
        screen.getByLabelText('Description'),
        'Test description',
      );
      await userEvent.type(screen.getByLabelText('URL'), 'http://test.com');

      await userEvent.click(
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
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: 'Test',
            url: 'http://test.com',
          }}
          onSubmit={noop}
        />,
      );

      expect(screen.getByLabelText('Description')).toHaveValue('Test');
      expect(screen.getByLabelText('URL')).toHaveValue('http://test.com');
    });

    test('cannot submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />);

      await userEvent.clear(screen.getByLabelText('Description'));

      await userEvent.click(
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
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
          }}
          onSubmit={handleSubmit}
        />,
      );

      await userEvent.click(
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

    test('can submit with valid values', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
          }}
          onSubmit={handleSubmit}
        />,
      );

      await userEvent.type(screen.getByLabelText('Description'), ' 2');
      await userEvent.type(screen.getByLabelText('URL'), '/updated');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Save legacy release',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit.mock.calls[0][0]).toEqual({
          description: 'Test description 2',
          url: 'http://test.com/updated',
        });
      });
    });
  });
});
