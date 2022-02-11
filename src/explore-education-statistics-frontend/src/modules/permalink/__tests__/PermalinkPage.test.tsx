import { Permalink } from '@common/services/permalinkService';
import PermalinkPage from '@frontend/modules/permalink/PermalinkPage';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('PermalinkPage', () => {
  const testPermalink: Permalink = {
    id: 'permalink-1',
    invalidated: false,
    created: '2020-10-07T12:00:00.00Z',
    configuration: {
      tableHeaders: {
        columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
        columns: [{ type: 'Filter', value: 'gender-female' }],
        rowGroups: [
          [{ type: 'Location', value: 'barnet', level: 'localAuthority' }],
        ],
        rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
      },
    },
    fullTable: {
      subjectMeta: {
        publicationName: 'Test publication',
        boundaryLevels: [],
        footnotes: [],
        subjectName: 'Subject 1',
        geoJsonAvailable: false,
        locations: {
          localAuthority: [
            {
              label: 'Barnet',
              value: 'barnet',
            },
          ],
        },
        timePeriodRange: [{ code: 'AY', year: 2020, label: '2020/21' }],
        indicators: [
          {
            value: 'authorised-absence-sessions',
            label: 'Number of authorised absence sessions',
            unit: '',
            name: 'sess_authorised',
            decimalPlaces: 2,
          },
        ],
        filters: {
          Characteristic: {
            totalValue: '',
            hint: 'Filter by pupil characteristic',
            legend: 'Characteristic',
            name: 'characteristic',
            options: {
              Gender: {
                label: 'Gender',
                options: [
                  {
                    label: 'Gender female',
                    value: 'gender-female',
                  },
                ],
              },
            },
          },
        },
      },
      results: [
        {
          timePeriod: '2020_AY',
          measures: {
            'authorised-absence-sessions': '123',
          },
          locationId: 'barnet',
          geographicLevel: 'localAuthority',
          filters: ['gender-female'],
        },
      ],
    },
  };

  test('renders correctly with permalink', () => {
    render(<PermalinkPage data={testPermalink} />);

    expect(
      screen.getByText("'Subject 1' from 'Test publication'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      "Number of authorised absence sessions for 'Subject 1' for Gender female in Barnet for 2020/21",
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');

    expect(rows).toHaveLength(2);
    expect(within(rows[0]).getByRole('columnheader')).toHaveTextContent(
      '2020/21',
    );
    expect(within(rows[1]).getByRole('rowheader')).toHaveTextContent('Barnet');
    expect(within(rows[1]).getByRole('cell')).toHaveTextContent('123');

    expect(
      screen.getByText('Source: Test publication, Subject 1'),
    ).toBeInTheDocument();
  });

  test('renders warning message with invalidated permalink', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          invalidated: true,
        }}
      />,
    );

    expect(
      screen.getByText(
        'WARNING - The data used in this permalink may be out-of-date.',
      ),
    ).toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });
});
