import render from '@common-test/render';
import { DataSetItem } from '@common/services/publicationService';
import { screen, within } from '@testing-library/react';
import React from 'react';
import ReleaseDataSetFileSummary from '../ReleaseDataSetFileSummary';

const testDataSetItem: DataSetItem = {
  dataSetFileId: 'test-dataset-1-datasetfileid',
  fileId: 'test-dataset-1-fileid',
  subjectId: 'test-dataset-1-subjectid',
  meta: {
    filters: ['Characteristic', 'School type'],
    geographicLevels: [
      'Local authority',
      'Local authority district',
      'National',
    ],
    indicators: ['Authorised absence rate', 'Authorised absence rate exact'],
    numDataFileRows: 1000,
    timePeriodRange: {
      start: '2012/13',
      end: '2016/17',
    },
  },
  title: 'Test dataset 1',
  summary: '<p>Test dataset 1 summary</p>',
};

describe('ReleaseDataSetFileSummary', () => {
  test('renders the collapsed view correctly', () => {
    render(<ReleaseDataSetFileSummary dataSetFile={testDataSetItem} />);

    expect(
      screen.getByRole('button', {
        name: 'Show more details about Test dataset 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders the expanded view when show more details is clicked', async () => {
    const { user } = render(
      <ReleaseDataSetFileSummary dataSetFile={testDataSetItem} />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show more details about Test dataset 1',
      }),
    );

    expect(
      within(screen.getByTestId('Number of rows')).getByText('1,000'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText(
        'Authorised absence rate',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText(
        'Authorised absence rate exact',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('School type'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Characteristic'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2012/13 to 2016/17'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Hide details about Test dataset 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders the expanded view when `expanded` is true', () => {
    render(
      <ReleaseDataSetFileSummary dataSetFile={testDataSetItem} expanded />,
    );

    expect(
      within(screen.getByTestId('Number of rows')).getByText('1,000'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText(
        'Authorised absence rate',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText(
        'Authorised absence rate exact',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('School type'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Characteristic'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2012/13 to 2016/17'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Hide details about Test dataset 1',
      }),
    ).toBeInTheDocument();
  });
});
