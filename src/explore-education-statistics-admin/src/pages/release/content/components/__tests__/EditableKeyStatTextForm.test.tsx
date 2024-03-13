import EditableKeyStatTextForm, {
  KeyStatTextFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import { KeyStatisticText } from '@common/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('EditableKeyStatTextForm', () => {
  const keyStatText: KeyStatisticText = {
    type: 'KeyStatisticText',
    id: 'keyStatDataBlock-1',
    trend: 'Text trend',
    guidanceTitle: 'Text guidance title',
    guidanceText: 'Text guidance text',
    order: 0,
    created: '2023-01-01',
    title: 'Text title',
    statistic: 'Over 9000',
  };

  test('renders correctly without initial values', async () => {
    render(
      <EditableKeyStatTextForm
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).not.toHaveValue();
    expect(screen.getByLabelText('Statistic')).not.toHaveValue();
    expect(screen.getByLabelText('Trend')).not.toHaveValue();
    expect(screen.getByLabelText('Guidance title')).toHaveValue('Help');
    expect(screen.getByLabelText('Guidance text')).not.toHaveValue();

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with initial values', async () => {
    render(
      <EditableKeyStatTextForm
        keyStat={keyStatText}
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).toHaveValue('Text title');
    expect(screen.getByLabelText('Statistic')).toHaveValue('Over 9000');
    expect(screen.getByLabelText('Trend')).toHaveValue('Text trend');
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'Text guidance title',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
      'Text guidance text',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('submitting form calls `onSubmit` handler with updated values', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();

    render(
      <EditableKeyStatTextForm
        keyStat={keyStatText}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('Title'));
    await user.type(screen.getByLabelText('Title'), 'New title');

    await user.clear(screen.getByLabelText('Statistic'));
    await user.type(screen.getByLabelText('Statistic'), 'New stat');

    await user.clear(screen.getByLabelText('Trend'));
    await user.type(screen.getByLabelText('Trend'), 'New trend');

    await user.clear(screen.getByLabelText('Guidance title'));
    await user.type(
      screen.getByLabelText('Guidance title'),
      'New guidance title',
    );

    await user.clear(screen.getByLabelText('Guidance text'));
    await user.type(
      screen.getByLabelText('Guidance text'),
      'New guidance text',
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<KeyStatTextFormValues[]>({
        title: 'New title',
        statistic: 'New stat',
        trend: 'New trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'New guidance text',
      });
    });
  });

  test('submitting form calls `onSubmit` handler with trimmed updated guidance title', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();

    render(
      <EditableKeyStatTextForm
        keyStat={keyStatText}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('Guidance title'));
    await user.type(
      screen.getByLabelText('Guidance title'),
      '   New guidance title  ',
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<KeyStatTextFormValues[]>({
        title: 'Text title',
        statistic: 'Over 9000',
        trend: 'Text trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'Text guidance text',
      });
    });
  });

  test('shows a validation error if submit without a title', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();

    render(
      <EditableKeyStatTextForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a title',
      }),
    ).toHaveAttribute('href', '#editableKeyStatTextForm-create-title');
    expect(
      screen.getByTestId('editableKeyStatTextForm-create-title-error'),
    ).toHaveTextContent('Enter a title');
  });

  test('shows a validation error if submit without a statistic', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();

    render(
      <EditableKeyStatTextForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a statistic',
      }),
    ).toHaveAttribute('href', '#editableKeyStatTextForm-create-statistic');
    expect(
      screen.getByTestId('editableKeyStatTextForm-create-statistic-error'),
    ).toHaveTextContent('Enter a statistic');
  });
});
