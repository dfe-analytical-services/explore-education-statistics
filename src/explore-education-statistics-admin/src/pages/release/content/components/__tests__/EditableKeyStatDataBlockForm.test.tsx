import EditableKeyStatDataBlockForm, {
  KeyStatDataBlockFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatDataBlockForm';
import render from '@common-test/render';
import {
  KeyStatistic,
  KeyStatisticDataBlock,
} from '@common/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableKeyStatDataBlockForm', () => {
  const testKeyStat: KeyStatisticDataBlock = {
    type: 'KeyStatisticDataBlock',
    id: 'keyStat-1',
    trend: 'Key stat 1 trend',
    guidanceTitle: 'Key stat 1 guidance title',
    guidanceText: 'Key stat 1 guidance text',
    dataBlockParentId: 'block-1',
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
      guidanceTitle: 'Key stat 3 guidance title',
      statistic: '3000',
    },
  ];

  test('renders correctly', async () => {
    render(
      <EditableKeyStatDataBlockForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
        statistic="Key stat 1 statistic"
        testId="test-id"
        title="Key stat 1 title"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTestId('test-id-title')).toHaveTextContent(
      'Key stat 1 title',
    );
    expect(screen.getByTestId('test-id-statistic')).toHaveTextContent(
      'Key stat 1 statistic',
    );
    expect(screen.getByLabelText('Trend')).toHaveValue('Key stat 1 trend');
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'Key stat 1 guidance title',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveValue(
      'Key stat 1 guidance text',
    );

    expect(screen.getByRole('button', { name: /Save/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cancel/ })).toBeInTheDocument();
  });

  test('submitting the form calls `onSubmit` handler with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
        statistic="Key stat 1 statistic"
        testId="test-id"
        title="Key stat 1 title"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

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
      expect(handleSubmit).toHaveBeenCalledWith<KeyStatDataBlockFormValues[]>({
        trend: 'New trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'New guidance text',
      });
    });
  });

  test('submitting the form calls `onSubmit` handler with trimmed updated guidance title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={testKeyStat}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
        statistic="Key stat 1 statistic"
        testId="test-id"
        title="Key stat 1 title"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('Guidance title'));
    await user.type(
      screen.getByLabelText('Guidance title'),
      '  New guidance title  ',
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<KeyStatDataBlockFormValues[]>({
        trend: 'Key stat 1 trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'Key stat 1 guidance text',
      });
    });
  });

  test('shows a validation error when there is guidance text without a guidance title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={{ ...testKeyStat, guidanceText: '', guidanceTitle: '' }}
        keyStats={[testKeyStat, ...testOtherKeyStats]}
        statistic="Key stat 1 statistic"
        testId="test-id"
        title="Key stat 1 title"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

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
    ).toHaveAttribute(
      'href',
      '#editableKeyStatDataBlockForm-keyStat-1-guidanceTitle',
    );
    expect(
      screen.getByTestId(
        'editableKeyStatDataBlockForm-keyStat-1-guidanceTitle-error',
      ),
    ).toHaveTextContent('Enter a guidance title');
  });

  test.each([
    [testOtherKeyStats[0].guidanceTitle!],
    [testOtherKeyStats[1].guidanceTitle!],
  ])(
    "shows a validation error when the guidance title is not unique using '%s'",
    async guidanceTitle => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <EditableKeyStatDataBlockForm
          keyStat={{ ...testKeyStat, guidanceText: '', guidanceTitle: '' }}
          keyStats={[testKeyStat, ...testOtherKeyStats]}
          statistic="Key stat 1 statistic"
          testId="test-id"
          title="Key stat 1 title"
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(screen.getByLabelText('Guidance title'), guidanceTitle);
      await user.click(screen.getByRole('button', { name: 'Save' }));

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();
      expect(
        within(screen.getByTestId('errorSummary')).getByRole('link', {
          name: 'Guidance title must be unique',
        }),
      ).toHaveAttribute(
        'href',
        '#editableKeyStatDataBlockForm-keyStat-1-guidanceTitle',
      );
      expect(
        screen.getByTestId(
          'editableKeyStatDataBlockForm-keyStat-1-guidanceTitle-error',
        ),
      ).toHaveTextContent('Guidance title must be unique');
    },
  );
});
