import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseSeriesLegacyLinkForm', () => {
  test('shows validation error for empty `description`', async () => {
    const { user } = render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a description')).not.toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a description', {
          selector: '#releaseSeriesLegacyLinkForm-description-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for empty `url`', async () => {
    const { user } = render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a URL')).not.toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a URL', {
          selector: '#releaseSeriesLegacyLinkForm-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for invalid `url`', async () => {
    const { user } = render(<ReleaseSeriesLegacyLinkForm onSubmit={noop} />);

    expect(screen.queryByText('Enter a valid URL')).not.toBeInTheDocument();

    await user.type(screen.getByLabelText('URL'), 'not a url');

    await user.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

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

      expect(screen.getByLabelText('Description')).toHaveValue('');
      expect(screen.getByLabelText('URL')).toHaveValue('');
    });

    test('cannot submit with only invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />,
      );

      await user.click(
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

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />,
      );

      await user.type(screen.getByLabelText('Description'), 'Test description');

      await user.click(
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

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />,
      );

      await user.type(screen.getByLabelText('Description'), 'Test description');
      await user.type(screen.getByLabelText('URL'), 'http://test.com');

      await user.click(
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

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm onSubmit={handleSubmit} />,
      );

      await user.clear(screen.getByLabelText('Description'));

      await user.click(
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

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
          }}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(
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

      const { user } = render(
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: 'Test description',
            url: 'http://test.com',
          }}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(screen.getByLabelText('Description'), ' 2');
      await user.type(screen.getByLabelText('URL'), '/updated');

      await user.click(
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
