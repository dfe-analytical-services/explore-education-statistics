import ReleaseNoteForm from '@admin/pages/release/content/components/ReleaseNoteForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ReleaseNoteForm', () => {
  describe('creating a release note', () => {
    test('renders the form correctly', () => {
      render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ reason: '' }}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(screen.getByLabelText('New release note')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Save note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('group', { name: 'Edit date' }),
      ).not.toBeInTheDocument();
    });

    test('shows a validation error if no reason is given', async () => {
      const { user } = render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ reason: '' }}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Save note' }));

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Release note must be provided',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('test-id-reason-error')).toHaveTextContent(
        'Release note must be provided',
      );
    });

    test('submits successfully', async () => {
      const handleSubmit = jest.fn();
      const { user } = render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ reason: '' }}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.type(screen.getByLabelText('New release note'), 'Test note');
      await user.click(screen.getByRole('button', { name: 'Save note' }));

      await waitFor(() => expect(handleSubmit).toHaveBeenCalledTimes(1));
      expect(handleSubmit).toHaveBeenCalledWith({ reason: 'Test note' });
    });
  });

  describe('editing a release note', () => {
    test('renders the form correctly with initial values', () => {
      render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ on: new Date('2024-01-01'), reason: 'Test note' }}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByRole('group', { name: 'Edit date' }),
      ).toBeInTheDocument();
      expect(screen.getByLabelText('Day')).toHaveNumericValue(1);
      expect(screen.getByLabelText('Month')).toHaveNumericValue(1);
      expect(screen.getByLabelText('Year')).toHaveNumericValue(2024);
      expect(screen.getByLabelText('Edit release note')).toHaveValue(
        'Test note',
      );
      expect(
        screen.getByRole('button', { name: 'Update note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('shows a validation error if no reason is given', async () => {
      const { user } = render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ on: new Date('2024-01-01'), reason: 'Test note' }}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.clear(screen.getByLabelText('Edit release note'));
      await user.click(screen.getByRole('button', { name: 'Update note' }));

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Release note must be provided',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('test-id-reason-error')).toHaveTextContent(
        'Release note must be provided',
      );
    });

    test('shows a validation error if no date is given', async () => {
      const { user } = render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ on: new Date('2024-01-01'), reason: 'Test note' }}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.clear(screen.getByLabelText('Day'));
      await user.clear(screen.getByLabelText('Month'));
      await user.clear(screen.getByLabelText('Year'));
      await user.click(screen.getByRole('button', { name: 'Update note' }));

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Enter a valid edit date',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('test-id-on-error')).toHaveTextContent(
        'Enter a valid edit date',
      );
    });

    test('submits successfully with updated values', async () => {
      const handleSubmit = jest.fn();
      const { user } = render(
        <ReleaseNoteForm
          id="test-id"
          initialValues={{ on: new Date('2024-01-01'), reason: 'Test note' }}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.clear(screen.getByLabelText('Day'));
      await user.type(screen.getByLabelText('Day'), '22');

      await user.clear(screen.getByLabelText('Month'));
      await user.type(screen.getByLabelText('Month'), '02');

      await user.clear(screen.getByLabelText('Year'));
      await user.type(screen.getByLabelText('Year'), '2025');

      await user.clear(screen.getByLabelText('Edit release note'));
      await user.type(
        screen.getByLabelText('Edit release note'),
        'Updated test note',
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update note' }));

      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith({
        on: new Date('2025-02-22'),
        reason: 'Updated test note',
      });
    });
  });
});
