import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { testDataSetItems } from '../../__tests__/__data__/testReleaseData';
import ReleaseDataSetFileSummary from '../ReleaseDataSetFileSummary';

describe('ReleaseDataSetFileSummary', () => {
  test('renders the collapsed view correctly', () => {
    render(<ReleaseDataSetFileSummary dataSetFile={testDataSetItems[0]} />);

    expect(
      screen.getByRole('button', {
        name: 'Show more details about Test dataset 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders the expanded view when show more details is clicked', async () => {
    const { user } = render(
      <ReleaseDataSetFileSummary dataSetFile={testDataSetItems[0]} />,
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
      <ReleaseDataSetFileSummary dataSetFile={testDataSetItems[0]} expanded />,
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
