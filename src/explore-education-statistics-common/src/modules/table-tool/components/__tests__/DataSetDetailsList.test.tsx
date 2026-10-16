import DataSetDetailsList from '@common/modules/table-tool/components/DataSetDetailsList';
import { Subject } from '@common/services/tableBuilderService';
import { render, screen } from '@testing-library/react';
import omit from 'lodash/omit';
import React from 'react';

describe('DataSetDetailsList', () => {
  const testSubject: Subject = {
    id: 'subject-1',
    name: 'Subject 1',
    content: 'Test content 1',
    timePeriods: {
      from: '2018/19',
      to: '2020/21',
    },
    geographicLevels: ['Local Authority District', 'Ward'],
    geographicLevelsCsvOnly: ['School'],
    file: {
      id: 'file-1',
      name: 'Subject 1',
      fileName: 'file-1.csv',
      extension: 'csv',
      size: '10 Mb',
      type: 'Data',
    },
    filters: ['School type'],
    indicators: ['Headcount', 'Percent'],
    lastUpdated: '2023-12-01',
  };

  test('renders the CSV-only geographic levels', () => {
    render(<DataSetDetailsList subject={testSubject} />);

    expect(screen.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority District; Ward',
    );
    expect(
      screen.getByTestId(
        'Geographic levels (only available via download / public API)',
      ),
    ).toHaveTextContent('School');
  });

  test('does not render the CSV-only geographic levels when there are none', () => {
    render(
      <DataSetDetailsList
        subject={{ ...testSubject, geographicLevelsCsvOnly: [] }}
      />,
    );

    expect(
      screen.queryByTestId(
        'Geographic levels (only available via download / public API)',
      ),
    ).not.toBeInTheDocument();
  });

  test('does not render the CSV-only geographic levels when the field is missing', () => {
    // Subject responses cached before `geographicLevelsCsvOnly` was added
    // won't include it, so it can be undefined at runtime despite the type.
    render(
      <DataSetDetailsList
        subject={omit(testSubject, 'geographicLevelsCsvOnly') as Subject}
      />,
    );

    expect(screen.getByTestId('Selected dataset')).toHaveTextContent(
      'Subject 1',
    );
    expect(screen.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority District; Ward',
    );
    expect(
      screen.queryByTestId(
        'Geographic levels (only available via download / public API)',
      ),
    ).not.toBeInTheDocument();
  });
});
