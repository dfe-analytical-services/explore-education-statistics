import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import { KeyStatisticText } from '@common/services/publicationService';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('EditableKeyStatText', () => {
  const testKeyStat: KeyStatisticText = {
    type: 'KeyStatisticText',
    id: 'keyStatDataBlock-1',
    trend: 'Text trend',
    guidanceTitle: 'Text guidance title',
    guidanceText: 'Text guidance text',
    title: 'Text title',
    statistic: 'Over 9000',
  };

  test('renders correctly when `isEditing` is false', async () => {
    render(
      <EditableKeyStatText
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent('Text title');

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      'Over 9000',
    );

    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent('Text trend');

    expect(
      screen.getByRole('button', {
        name: 'Text guidance title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Text guidance text',
    );

    expect(
      screen.queryByRole('button', { name: /Edit/ }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when `isEditing` is true', async () => {
    render(
      <EditableKeyStatText
        isEditing
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent('Text title');

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      'Over 9000',
    );

    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent('Text trend');

    expect(
      screen.getByRole('button', {
        name: 'Text guidance title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Text guidance text',
    );

    expect(screen.getByRole('button', { name: /Edit/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Remove/ })).toBeInTheDocument();
  });

  test('does not render the trend when `trend` is undefined', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...testKeyStat, trend: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
  });

  test('renders the default guidance title when `guidanceTitle` is undefined', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...testKeyStat, guidanceTitle: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Help for Text title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Text guidance text',
    );
  });

  test('does not render the guidance when `guidanceText` is undefined', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...testKeyStat, guidanceText: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.queryByRole('button', {
        name: 'Text guidance title',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();
  });

  test('clicking the edit button shows the form', async () => {
    const user = userEvent.setup();

    render(
      <EditableKeyStatText
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Edit key statistic: Text title',
      }),
    );

    expect(await screen.findByLabelText('Title')).toBeInTheDocument();

    expect(screen.getByLabelText('Title')).toHaveValue('Text title');
    expect(screen.getByLabelText('Statistic')).toHaveValue('Over 9000');
    expect(screen.getByLabelText('Trend')).toHaveValue('Text trend');
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'Text guidance title',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveValue(
      'Text guidance text',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Edit key statistic: Text title',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('clicking the cancel button toggles back to preview', async () => {
    const user = userEvent.setup();

    render(
      <EditableKeyStatText
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Edit key statistic: Text title',
      }),
    );

    expect(await screen.findByLabelText('Title')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    );

    expect(
      await screen.findByRole('button', {
        name: 'Edit key statistic: Text title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByRole('button', { name: /Remove/ })).toBeInTheDocument();

    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument();
  });

  test('clicking the remove button calls the onRemove handler', async () => {
    const handleRemove = jest.fn();
    const user = userEvent.setup();

    render(
      <EditableKeyStatText
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={handleRemove}
        onSubmit={noop}
      />,
    );

    expect(handleRemove).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Remove key statistic: Text title',
      }),
    );

    expect(handleRemove).toHaveBeenCalled();
  });
});
