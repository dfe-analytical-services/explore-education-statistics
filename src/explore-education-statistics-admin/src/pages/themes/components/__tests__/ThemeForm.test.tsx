import ThemeForm, {
  ThemeFormValues,
} from '@admin/pages/themes/components/ThemeForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ThemeForm', () => {
  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<ThemeForm onSubmit={handleSubmit} />);

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save theme' }));

    expect(
      await screen.findByText('Enter a title', {
        selector: '#themeForm-title-error',
      }),
    ).toBeInTheDocument();

    expect(
      await screen.findByText('Enter a summary', {
        selector: '#themeForm-summary-error',
      }),
    ).toBeInTheDocument();

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<ThemeForm onSubmit={handleSubmit} />);

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Summary'), 'Test summary');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save theme' }));

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

      const { user } = render(<ThemeForm onSubmit={handleSubmit} />);

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.clear(screen.getByLabelText('Title'));
      await user.clear(screen.getByLabelText('Summary'));

      await user.click(screen.getByRole('button', { name: 'Save theme' }));

      expect(
        await screen.findByText('Enter a title', {
          selector: '#themeForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        await screen.findByText('Enter a summary', {
          selector: '#themeForm-summary-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(<ThemeForm onSubmit={handleSubmit} />);

      await user.type(screen.getByLabelText('Title'), 'Updated title');
      await user.type(screen.getByLabelText('Summary'), 'Updated summary');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Save theme' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
          summary: 'Updated summary',
        } as ThemeFormValues);
      });
    });
  });
});
