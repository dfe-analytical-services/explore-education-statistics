import ThemeForm, {
  ThemeFormValues,
} from '@admin/pages/themes/components/ThemeForm';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ThemeForm', () => {
  test('shows validation error when there is no title', async () => {
    render(<ThemeForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a title', {
          selector: '#themeForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when there is no summary', async () => {
    render(<ThemeForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Summary'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a summary', {
          selector: '#themeForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(<ThemeForm onSubmit={handleSubmit} />);

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save theme' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<ThemeForm onSubmit={handleSubmit} />);

    await userEvent.type(screen.getByLabelText('Title'), 'Test title');
    await userEvent.type(screen.getByLabelText('Summary'), 'Test summary');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save theme' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        summary: 'Test summary',
      } as ThemeFormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly', async () => {
      render(
        <ThemeForm
          initialValues={{
            title: 'Test title',
            summary: 'Test summary',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toHaveValue('Test title');
        expect(screen.getByLabelText('Summary')).toHaveValue('Test summary');
      });
    });

    test('cannot submit with updated invalid values', async () => {
      const handleSubmit = jest.fn();

      render(<ThemeForm onSubmit={handleSubmit} />);

      expect(handleSubmit).not.toHaveBeenCalled();

      await userEvent.clear(screen.getByLabelText('Title'));
      await userEvent.clear(screen.getByLabelText('Summary'));

      userEvent.click(screen.getByRole('button', { name: 'Save theme' }));

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      render(<ThemeForm onSubmit={handleSubmit} />);

      await userEvent.type(screen.getByLabelText('Title'), 'Updated title');
      await userEvent.type(screen.getByLabelText('Summary'), 'Updated summary');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save theme' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
          summary: 'Updated summary',
        } as ThemeFormValues);
      });
    });
  });
});
