import React from 'react';
import { render, screen } from '@testing-library/react';
import OfficialStatisticsInDevelopmentSection from '@common/modules/release/components/OfficialStatisticsInDevelopmentSection';

describe('OfficialStatisticsInDevelopmentSection', () => {
  test('renders correctly with default settings', () => {
    render(<OfficialStatisticsInDevelopmentSection />);

    expect(
      screen.getByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when the publishing organisation is Skills England', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[
          { id: 'test-id', title: 'Skills England', url: 'test-url' },
        ]}
      />,
    );

    expect(
      screen.getByText(
        /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).not.toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(<OfficialStatisticsInDevelopmentSection showHeading />);

    expect(
      screen.getByRole('heading', {
        name: 'Official statistics in development',
      }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(<OfficialStatisticsInDevelopmentSection showHeading={false} />);

    expect(
      screen.queryByRole('heading', {
        name: 'Official statistics in development',
      }),
    ).not.toBeInTheDocument();
  });
});
