import EducationInNumbersPageForm, {
  EducationInNumbersPageFormValues,
} from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('EducationInNumbersPageForm', () => {
  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EducationInNumbersPageForm onSubmit={handleSubmit} />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    expect(
      await screen.findByText('Enter a title', {
        selector: '#educationInNumbersPageForm-title-error',
      }),
    ).toBeInTheDocument();

    expect(
      await screen.findByText('Enter a description', {
        selector: '#educationInNumbersPageForm-description-error',
      }),
    ).toBeInTheDocument();

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EducationInNumbersPageForm onSubmit={handleSubmit} />,
    );

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Description'), 'Test description');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        description: 'Test description',
      } as EducationInNumbersPageFormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly', async () => {
      render(
        <EducationInNumbersPageForm
          initialValues={{
            title: 'Test title',
            description: 'Test description',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toHaveValue('Test title');
        expect(screen.getByLabelText('Description')).toHaveValue(
          'Test description',
        );
      });
    });

    test('cannot submit with updated invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersPageForm onSubmit={handleSubmit} />,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.clear(screen.getByLabelText('Title'));
      await user.clear(screen.getByLabelText('Description'));

      await user.click(screen.getByRole('button', { name: 'Create page' }));

      expect(
        await screen.findByText('Enter a title', {
          selector: '#educationInNumbersPageForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        await screen.findByText('Enter a description', {
          selector: '#educationInNumbersPageForm-description-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersPageForm onSubmit={handleSubmit} />,
      );

      await user.type(screen.getByLabelText('Title'), 'Updated title');
      await user.type(
        screen.getByLabelText('Description'),
        'Updated description',
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Create page' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
          description: 'Updated description',
        } as EducationInNumbersPageFormValues);
      });
    });
  });
});
