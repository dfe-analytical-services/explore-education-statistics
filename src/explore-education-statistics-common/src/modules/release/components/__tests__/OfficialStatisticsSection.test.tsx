import React from 'react';
import { render, screen } from '@testing-library/react';
import OfficialStatisticsSection from '@common/modules/release/components/OfficialStatisticsSection';

describe('OfficialStatisticsSection', () => {
  test('renders', () => {
    render(<OfficialStatisticsSection />);

    expect(
      screen.getByRole('link', {
        name: 'Code of Practice for Official Statistics (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(<OfficialStatisticsSection showHeading />);

    expect(
      screen.getByRole('heading', { name: 'Official statistics' }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(<OfficialStatisticsSection showHeading={false} />);

    expect(
      screen.queryByRole('heading', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('includes the OSR guidance text', () => {
    // Introduced because of changes to the National Statistics Designation Review,
    // Documented in https://dfedigital.atlassian.net/browse/EES-4621

    render(<OfficialStatisticsSection />);

    expect(
      screen.getByText(
        'Our statistical practice is regulated by the Office for Statistics Regulation (OSR).',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Code of Practice for Statistics (opens in new tab)',
      }),
    ).toHaveAttribute(
      'href',
      'https://code.statisticsauthority.gov.uk/the-code/',
    );

    expect(
      screen.getByRole('link', { name: 'regulation@statistics.gov.uk' }),
    ).toHaveAttribute('href', 'mailto:regulation@statistics.gov.uk');

    expect(
      screen.getByRole('link', { name: 'OSR website (opens in new tab)' }),
    ).toHaveAttribute('href', 'https://osr.statisticsauthority.gov.uk/');
  });
});
