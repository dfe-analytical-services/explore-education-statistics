import React from 'react';
import { render, screen } from '@testing-library/react';
import AccreditedOfficialStatisticsSection from '@common/modules/release/components/AccreditedOfficialStatisticsSection';

describe('AccreditedOfficialStatisticsSection', () => {
  test('renders', () => {
    render(<AccreditedOfficialStatisticsSection />);

    expect(
      screen.getByRole('link', {
        name: 'Office for Statistics Regulation (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(<AccreditedOfficialStatisticsSection showHeading />);

    expect(
      screen.getByRole('heading', { name: 'Accredited official statistics' }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(<AccreditedOfficialStatisticsSection showHeading={false} />);

    expect(
      screen.queryByRole('heading', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('includes the OSR guidance text', () => {
    // Introduced because of changes to the National Statistics Designation Review,
    // Documented in https://dfedigital.atlassian.net/browse/EES-4620

    render(<AccreditedOfficialStatisticsSection />);

    expect(
      screen.getByText(
        'Our statistical practice is regulated by the Office for Statistics Regulation (OSR).',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryAllByRole('link', {
        name: 'Code of Practice for Statistics (opens in new tab)',
      })[0],
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
