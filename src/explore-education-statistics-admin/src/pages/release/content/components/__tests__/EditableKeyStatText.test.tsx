import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import { KeyStatisticText } from '@common/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import { noop } from 'lodash';
import React from 'react';

describe('EditableKeyStatText', () => {
  // TODO: EES-2469 Write tests for EditableKeyStatText
  // Text isEditing
  // Text isEditing form
  // Text isEditing form Cancel
  // Text onSubmit
  // Text onRemove

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

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Text title',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
        'Over 9000',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Text trend',
      );
    });

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

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Text title',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
        'Over 9000',
      );

      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
    });

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

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Text title',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
        'Over 9000',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Text trend',
      );
    });

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
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Text title',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
        'Over 9000',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Text trend',
      );
    });

    expect(
      screen.queryByRole('button', {
        name: 'Text guidance title',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });
});
