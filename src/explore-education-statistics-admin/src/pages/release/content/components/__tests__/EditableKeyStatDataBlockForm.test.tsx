import EditableKeyStatDataBlockForm, {
  KeyStatDataBlockFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatDataBlockForm';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
    expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
      'DataBlock guidance text',
    );

    expect(screen.getByRole('button', { name: /Save/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cancel/ })).toBeInTheDocument();
  });

  test('submitting the form calls `onSubmit` handler with updated values', async () => {
    const user = userEvent.setup();

    const handleSubmit = jest.fn();

    render(
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
    const user = userEvent.setup();

    const handleSubmit = jest.fn();

    render(
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
});
