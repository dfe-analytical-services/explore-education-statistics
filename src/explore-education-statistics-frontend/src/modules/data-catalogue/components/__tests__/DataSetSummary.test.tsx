import render from '@common-test/render';
import DataSetFileSummary from '@frontend/modules/data-catalogue/components/DataSetFileSummary';
import { testDataSetFileSummaries } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import React from 'react';

const longSummary = `Number of places approved and available for use, 
  number of children accommodated and occupancy rate in individual secure 
  children's homes, 2010 to 2023 Individual home notes are: - Beechfield 
  was undergoing renovation between 2017 and 2019 whilst unoccupied. 
  It closed in 2019. - Swanwick Lodge reported in 2018 ongoing staffing issues 
  and building works. - Leverton Hall closed on 31 July 2014. - Red Bank 
  closed on 31 May 2014. It included the homes Newton House, Willows House 
  and Vardy House. - Adel Beck was previously known as East Moor until January 2015.`;

describe('DataSetFileSummary', () => {
  test('renders the collapsed view correctly', () => {
    render(<DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} />);

    expect(
      screen.getByRole('heading', { name: 'Data set 1' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Data set summary 1')).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Theme')).getByText('Theme 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Published')).getByText('1 Jan 2020'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Publication')).getByText('Publication 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Release')).getByText('Release 1'),
    ).toBeInTheDocument();
  });

  test('renders the expanded view when show more details is clicked', async () => {
    const { user } = render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show more details about Data set 1',
      }),
    );

    expect(
      within(screen.getByTestId('Number of rows')).getByText('11'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National, Regional',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2010 to 2020'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Hide details about Data set 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders the expanded view when `expanded` is true', () => {
    render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} expanded />,
    );

    expect(
      within(screen.getByTestId('Number of rows')).getByText('11'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National, Regional',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2010 to 2020'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Hide details about Data set 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders the `read more` button when the content is long', () => {
    render(
      <DataSetFileSummary
        dataSetFile={{ ...testDataSetFileSummaries[0], content: longSummary }}
        expanded
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Read more about Data set 1' }),
    ).toBeInTheDocument();
  });

  test('does not render the `read more` button when the content is short', () => {
    render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} expanded />,
    );

    expect(
      screen.queryByRole('button', { name: 'Read more about Data set 1' }),
    ).not.toBeInTheDocument();
  });

  test('renders the `latest data` tag when it is the latest release', () => {
    render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} expanded />,
    );

    expect(screen.getByText('This is the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This is not the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when it is not the latest release', () => {
    render(
      <DataSetFileSummary
        dataSetFile={{ ...testDataSetFileSummaries[0], latestData: false }}
        expanded
      />,
    );

    expect(screen.getByText('This is not the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when the publication is superseded', () => {
    render(
      <DataSetFileSummary
        dataSetFile={{ ...testDataSetFileSummaries[0], isSuperseded: true }}
        expanded
      />,
    );

    expect(screen.getByText('This is not the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders the `Available by API` tag when it is available by API', () => {
    render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[0]} expanded />,
    );

    expect(screen.getByText('Available by API')).toBeInTheDocument();
  });

  test('does not render the `Available by API` tag when it is not available by API', () => {
    render(
      <DataSetFileSummary dataSetFile={testDataSetFileSummaries[1]} expanded />,
    );

    expect(screen.queryByText('Available by API')).not.toBeInTheDocument();
  });
});
