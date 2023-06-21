import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import { KeyStatisticText } from '@common/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';
import { KeyStatTextFormValues } from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import _keyStatisticService from '@admin/services/keyStatisticService';

jest.mock('@admin/services/keyStatisticService');
const keyStatisticService = _keyStatisticService as jest.Mocked<
  typeof _keyStatisticService
>;

describe('EditableKeyStatText', () => {
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

  test('renders preview correctly', async () => {
    render(<EditableKeyStatText keyStat={keyStatText} onSubmit={noop} />);

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

  test('renders preview correctly without trend', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...keyStatText, trend: undefined }}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent('Text title');

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      'Over 9000',
    );

    expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Text guidance title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Text guidance text',
    );

    expect(
      screen.queryByRole('button', { name: 'Remove' }),
    ).not.toBeInTheDocument();
  });

  test('renders preview correctly without guidanceTitle', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...keyStatText, guidanceTitle: undefined }}
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
        name: 'Help for Text title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Text guidance text',
    );

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders preview correctly without guidanceText', async () => {
    render(
      <EditableKeyStatText
        keyStat={{ ...keyStatText, guidanceText: undefined }}
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
      screen.queryByRole('button', {
        name: 'Text guidance title',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Edit/ }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  describe('when editing', () => {
    test('renders edit form correctly', async () => {
      render(
        <EditableKeyStatText keyStat={keyStatText} isEditing onSubmit={noop} />,
      );

      expect(screen.getByText(/Edit/)).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toHaveValue('Text title');
      });

      expect(screen.getByLabelText('Statistic')).toHaveValue('Over 9000');
      expect(screen.getByLabelText('Trend')).toHaveValue('Text trend');

      expect(screen.getByLabelText('Guidance title')).toHaveValue(
        'Text guidance title',
      );

      expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
        'Text guidance text',
      );

      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('submitting edit form calls `onSubmit` handler with updated values', async () => {
      const onSubmit = jest.fn();

      render(
        <EditableKeyStatText
          keyStat={keyStatText}
          isEditing
          onSubmit={onSubmit}
        />,
      );

      expect(screen.getByText(/Edit/)).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Title'));
      await userEvent.type(screen.getByLabelText('Title'), 'New title');

      userEvent.clear(screen.getByLabelText('Statistic'));
      await userEvent.type(screen.getByLabelText('Statistic'), 'New stat');

      userEvent.clear(screen.getByLabelText('Trend'));
      await userEvent.type(screen.getByLabelText('Trend'), 'New trend');

      userEvent.clear(screen.getByLabelText('Guidance title'));
      await userEvent.type(
        screen.getByLabelText('Guidance title'),
        'New guidance title',
      );

      userEvent.clear(screen.getByLabelText('Guidance text'));
      await userEvent.type(
        screen.getByLabelText('Guidance text'),
        'New guidance text',
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith<KeyStatTextFormValues[]>({
          title: 'New title',
          statistic: 'New stat',
          trend: 'New trend',
          guidanceTitle: 'New guidance title',
          guidanceText: 'New guidance text',
        });
      });
    });

    test('submitting edit form calls `onSubmit` handler with trimmed updated guidance title', async () => {
      const onSubmit = jest.fn();

      render(
        <EditableKeyStatText
          keyStat={keyStatText}
          isEditing
          onSubmit={onSubmit}
        />,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Guidance title'));
      await userEvent.type(
        screen.getByLabelText('Guidance title'),
        '   New guidance title  ',
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith<KeyStatTextFormValues[]>({
          title: 'Text title',
          statistic: 'Over 9000',
          trend: 'Text trend',
          guidanceTitle: 'New guidance title',
          guidanceText: 'Text guidance text',
        });
      });
    });

    test('clicking `Cancel` button toggles back to preview', async () => {
      render(
        <EditableKeyStatText keyStat={keyStatText} isEditing onSubmit={noop} />,
      );

      expect(screen.getByText(/Edit/)).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('Cancel')).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(screen.queryByText('Cancel')).not.toBeInTheDocument();
      });

      expect(screen.queryByLabelText('Trend')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Guidance title')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Guidance text')).not.toBeInTheDocument();

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Text title',
      );
      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        'Over 9000',
      );
      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Text trend',
      );

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
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();

      expect(keyStatisticService.updateKeyStatisticText).not.toHaveBeenCalled();
    });
  });
});
