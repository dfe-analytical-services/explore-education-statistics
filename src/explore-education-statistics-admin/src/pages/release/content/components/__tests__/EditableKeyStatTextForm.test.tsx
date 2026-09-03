import EditableKeyStatTextForm, {
  KeyStatTextFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import render from '@common-test/render';
import {
  KeyStatistic,
  KeyStatisticText,
} from '@common/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableKeyStatTextForm', () => {
  const testKeyStat: KeyStatisticText = {
    id: 'keyStat-1',
    type: 'KeyStatisticText',
    title: 'Key stat 1 title',
    statistic: '1000',
    trend: 'Key stat 1 trend',
    guidanceTitle: 'Key stat 1 guidance title',
    guidanceText: 'Key stat 1 guidance text',
  };

  const testOtherKeyStats: KeyStatistic[] = [
    {
      id: 'keyStat-2',
      type: 'KeyStatisticDataBlock',
      guidanceTitle: 'Key stat 2 guidance title',
      dataBlockParentId: 'block-2',
    },
    {
      id: 'keyStat-3',
      type: 'KeyStatisticText',
      title: 'Key stat 3 title',
      statistic: 'Key stat 3 stat',
      guidanceTitle: 'Key stat 3 guidance title',
    },
  ];

  test('renders correctly without initial values', async () => {
    render(
      <EditableKeyStatTextForm
        testId="test-id"
        keyStats={[]}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).not.toHaveValue();
    expect(screen.getByLabelText('Statistic')).not.toHaveValue();
    expect(screen.getByLabelText('Trend')).not.toHaveValue();
    expect(screen.getByLabelText('Guidance title')).not.toHaveValue();
    expect(screen.getByLabelText('Guidance text')).not.toHaveValue();

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with initial values', async () => {
    render(
      <EditableKeyStatTextForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).toHaveValue('Key stat 1 title');
    expect(screen.getByLabelText('Statistic')).toHaveValue('1000');
    expect(screen.getByLabelText('Trend')).toHaveValue('Key stat 1 trend');
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'Key stat 1 guidance title',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveValue(
      'Key stat 1 guidance text',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('submitting form calls `onSubmit` handler with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatTextForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
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
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatTextForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
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
        title: 'Key stat 1 title',
        statistic: '1000',
        trend: 'Key stat 1 trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'Key stat 1 guidance text',
      });
    });
  });

  test('shows a validation error if submit without a title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatTextForm
        keyStats={testOtherKeyStats}
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
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatTextForm
        keyStats={testOtherKeyStats}
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

  test('shows a validation error when there is guidance text without a guidance title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatTextForm
        keyStats={testOtherKeyStats}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Statistic'), 'Test stat');
    await user.type(
      screen.getByLabelText('Guidance text'),
      'Test guidance text',
    );
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a guidance title',
      }),
    ).toHaveAttribute('href', '#editableKeyStatTextForm-create-guidanceTitle');
    expect(
      screen.getByTestId('editableKeyStatTextForm-create-guidanceTitle-error'),
    ).toHaveTextContent('Enter a guidance title');
  });

  test.each([
    [testOtherKeyStats[0].guidanceTitle!],
    [testOtherKeyStats[1].guidanceTitle!],
  ])(
    "shows a validation error when the guidance title is not unique using '%s'",
    async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EditableKeyStatTextForm
          keyStats={[testKeyStat, ...testOtherKeyStats]}
          testId="test-id"
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(screen.getByLabelText('Title'), 'Test title');
      await user.type(screen.getByLabelText('Statistic'), 'Test stat');
      await user.type(
        screen.getByLabelText('Guidance title'),
        'Key stat 2 guidance title',
      );
      await user.click(screen.getByRole('button', { name: 'Save' }));

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();
      expect(
        within(screen.getByTestId('errorSummary')).getByRole('link', {
          name: 'Guidance title must be unique',
        }),
      ).toHaveAttribute(
        'href',
        '#editableKeyStatTextForm-create-guidanceTitle',
      );
      expect(
        screen.getByTestId(
          'editableKeyStatTextForm-create-guidanceTitle-error',
        ),
      ).toHaveTextContent('Guidance title must be unique');
    },
  );
});
