import React from 'react';
import { render, screen } from '@testing-library/react';
import NationalStatisticsSection from '@common/modules/release/components/NationalStatisticsSection';

describe('NationalStatisticsSection', () => {
  test('renders', () => {
    render(<NationalStatisticsSection />);

    expect(
      screen.getByRole('link', {
        name: 'Office for Statistics Regulation',
      }),
    ).toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(<NationalStatisticsSection showHeading />);

    expect(
      screen.getByRole('heading', { name: 'National statistics' }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(<NationalStatisticsSection showHeading={false} />);

    expect(
      screen.queryByRole('heading', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('includes the OSR guidance text', () => {
    // Introduced because of changes to the National Statistics Designation Review,
    // Documented in https://dfedigital.atlassian.net/browse/EES-4620

    render(<NationalStatisticsSection />);

    expect(
      screen.getByText(
        'Our statistical practice is regulated by the Office for Statistics Regulation (OSR).',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryAllByRole('link', {
        name: 'Code of Practice for Statistics',
      })[0],
    ).toHaveAttribute(
      'href',
      'https://code.statisticsauthority.gov.uk/the-code/',
    );

    expect(
      screen.getByRole('link', { name: 'regulation@statistics.gov.uk' }),
    ).toHaveAttribute('href', 'mailto:regulation@statistics.gov.uk');

    expect(screen.getByRole('link', { name: 'OSR website' })).toHaveAttribute(
      'href',
      'https://osr.statisticsauthority.gov.uk/',
    );
  });
});