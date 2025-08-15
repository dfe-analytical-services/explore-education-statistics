import EducationInNumbersSummaryForm, {
  EducationInNumbersSummaryFormValues,
} from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import Button from '@common/components/Button';

describe('EducationInNumbersSummaryForm', () => {
  describe('create page', () => {
    test('cannot submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersSummaryForm onSubmit={handleSubmit} />,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Create page' }));

      expect(
        await screen.findByText('Enter a title', {
          selector: '#educationInNumbersSummaryForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        await screen.findByText('Enter a description', {
          selector: '#educationInNumbersSummaryForm-description-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('can submit with valid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersSummaryForm onSubmit={handleSubmit} />,
      );

      await user.type(screen.getByLabelText('Title'), 'Test title');
      await user.type(screen.getByLabelText('Description'), 'Test description');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Create page' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Test title',
          description: 'Test description',
        } as EducationInNumbersSummaryFormValues);
      });
    });
  });

  describe('edit page', () => {
    const initialValues: EducationInNumbersSummaryFormValues = {
      title: 'Initial title',
      description: 'Initial description',
    };

    test('renders correctly with initial values', () => {
      render(
        <EducationInNumbersSummaryForm
          initialValues={initialValues}
          isEditForm
          onSubmit={noop}
        />,
      );

      expect(screen.getByLabelText('Title')).toHaveValue('Initial title');
      expect(screen.getByLabelText('Description')).toHaveValue(
        'Initial description',
      );
      expect(
        screen.getByRole('button', { name: 'Update page' }),
      ).toBeInTheDocument();
    });

    test('cannot submit with updated invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersSummaryForm
          initialValues={initialValues}
          isEditForm
          onSubmit={handleSubmit}
        />,
      );

      await user.clear(screen.getByLabelText('Title'));
      await user.clear(screen.getByLabelText('Description'));

      await user.click(screen.getByRole('button', { name: 'Update page' }));

      expect(
        await screen.findByText('Enter a title', {
          selector: '#educationInNumbersSummaryForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        await screen.findByText('Enter a description', {
          selector: '#educationInNumbersSummaryForm-description-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EducationInNumbersSummaryForm
          initialValues={initialValues}
          isEditForm
          onSubmit={handleSubmit}
        />,
      );

      await user.clear(screen.getByLabelText('Title'));
      await user.type(screen.getByLabelText('Title'), 'Updated title');

      await user.clear(screen.getByLabelText('Description'));
      await user.type(
        screen.getByLabelText('Description'),
        'Updated description',
      );

      await user.click(screen.getByRole('button', { name: 'Update page' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
          description: 'Updated description',
        } as EducationInNumbersSummaryFormValues);
      });
    });

    test('renders and calls the cancel button handler when clicked', async () => {
      const handleCancel = jest.fn();
      const { user } = render(
        <EducationInNumbersSummaryForm
          onSubmit={noop}
          cancelButton={<Button onClick={handleCancel}>Cancel</Button>}
        />,
      );

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      expect(cancelButton).toBeInTheDocument();

      await user.click(cancelButton);

      expect(handleCancel).toHaveBeenCalledTimes(1);
    });
  });
});
