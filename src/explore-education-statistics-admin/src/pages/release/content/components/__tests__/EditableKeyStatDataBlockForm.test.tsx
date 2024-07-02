import EditableKeyStatDataBlockForm, {
  KeyStatDataBlockFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatDataBlockForm';
import render from '@common-test/render';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableKeyStatDataBlockForm', () => {
  const keyStatDataBlock: KeyStatisticDataBlock = {
    type: 'KeyStatisticDataBlock',
    id: 'keyStatDataBlock-1',
    trend: 'DataBlock trend',
    guidanceTitle: 'DataBlock guidance title',
    guidanceText: 'DataBlock guidance text',
    order: 0,
    created: '2023-01-01',
    dataBlockParentId: 'block-1',
  };

  test('renders correctly', async () => {
    render(
      <EditableKeyStatDataBlockForm
        keyStat={keyStatDataBlock}
        statistic="Key stat statistic"
        testId="test-id"
        title="Key stat title"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTestId('test-id-title')).toHaveTextContent(
      'Key stat title',
    );
    expect(screen.getByTestId('test-id-statistic')).toHaveTextContent(
      'Key stat statistic',
    );
    expect(screen.getByLabelText('Trend')).toHaveValue('DataBlock trend');
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'DataBlock guidance title',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveValue(
      'DataBlock guidance text',
    );

    expect(screen.getByRole('button', { name: /Save/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cancel/ })).toBeInTheDocument();
  });

  test('submitting the form calls `onSubmit` handler with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={keyStatDataBlock}
        statistic="Key stat statistic"
        testId="test-id"
        title="Key stat title"
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
        keyStat={keyStatDataBlock}
        statistic="Key stat statistic"
        testId="test-id"
        title="Key stat title"
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
        trend: 'DataBlock trend',
        guidanceTitle: 'New guidance title',
        guidanceText: 'DataBlock guidance text',
      });
    });
  });

  test('shows a validation error when have guidance text without a guidance title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={{ ...keyStatDataBlock, guidanceText: '', guidanceTitle: '' }}
        statistic="Key stat statistic"
        testId="test-id"
        title="Key stat title"
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
      '#editableKeyStatDataBlockForm-keyStatDataBlock-1-guidanceTitle',
    );
    expect(
      screen.getByTestId(
        'editableKeyStatDataBlockForm-keyStatDataBlock-1-guidanceTitle-error',
      ),
    ).toHaveTextContent('Enter a guidance title');
  });

  test('shows a validation error when the guidance title is not unique', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableKeyStatDataBlockForm
        keyStat={{ ...keyStatDataBlock, guidanceText: '', guidanceTitle: '' }}
        keyStatisticGuidanceTitles={['test guidance title', 'something else']}
        statistic="Key stat statistic"
        testId="test-id"
        title="Key stat title"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.type(
      screen.getByLabelText('Guidance title'),
      'test guidance title',
    );
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Guidance titles must be unique',
      }),
    ).toHaveAttribute(
      'href',
      '#editableKeyStatDataBlockForm-keyStatDataBlock-1-guidanceTitle',
    );
    expect(
      screen.getByTestId(
        'editableKeyStatDataBlockForm-keyStatDataBlock-1-guidanceTitle-error',
      ),
    ).toHaveTextContent('Guidance titles must be unique');
  });
});
