import render from '@common-test/render';
import ReleasePageIntro from '@frontend/modules/find-statistics/components/ReleasePageIntro';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from '@frontend/modules/find-statistics/__tests__/__data__/testReleaseData';
import { screen, within } from '@testing-library/react';
import React from 'react';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('ReleasePageIntro', () => {
  test('renders latest release tag', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(screen.queryByText('Latest release')).toBeInTheDocument();

    expect(
      screen.queryByText('Not the latest release'),
    ).not.toBeInTheDocument();
  });

  test('does not render latest/not latest release tag when publication is superseded', () => {
    const testPublicationSummarySuperseded: PublicationSummaryRedesign = {
      ...testPublicationSummary,
      supersededByPublication: {
        id: '223e4567-e89b-12d3-a456-426614174000',
        title: 'Superseding publication',
        slug: 'superseding-publication',
      },
    };
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummarySuperseded}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(screen.queryByText('Latest release')).not.toBeInTheDocument();
    expect(
      screen.queryByText('Not the latest release'),
    ).not.toBeInTheDocument();
  });

  test('renders not latest release link and tag when publication is not the latest', () => {
    const testReleaseVersionSummaryNotLatest: ReleaseVersionSummary = {
      ...testReleaseVersionSummary,
      isLatestRelease: false,
    };
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummaryNotLatest}
      />,
    );

    expect(screen.getByText('Not the latest release')).toBeInTheDocument();
    expect(screen.queryByText('Latest release')).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View latest release: Calendar year 2024 - Final',
      }),
    ).toBeInTheDocument();
  });

  test('renders the link to all releases in series', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'All releases in this series',
      }),
    ).toHaveAttribute('href', '/find-statistics/publication-slug/releases');
  });

  test('renders superseded warning text when publication is superseded', () => {
    const testPublicationSummarySuperseded: PublicationSummaryRedesign = {
      ...testPublicationSummary,
      supersededByPublication: {
        id: '223e4567-e89b-12d3-a456-426614174000',
        title: 'Superseding publication',
        slug: 'superseding-publication',
      },
    };

    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummarySuperseded}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    const supersededWarningLink = within(
      screen.getByTestId('superseded-warning'),
    ).getByRole('link', {
      name: 'Superseding publication',
    });

    expect(supersededWarningLink).toHaveAttribute(
      'href',
      '/find-statistics/superseding-publication',
    );
  });

  test('does not render superseded warning text when publication is not superseded', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(screen.queryByTestId('superseded-warning')).not.toBeInTheDocument();
  });

  test('renders "Next release" section if this is the latest Release for a Publication and there is a valid partial date', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );
    const nextUpdateValue = screen.getByTestId('Next update');
    expect(nextUpdateValue.textContent).toEqual('March 2026');
  });

  test(`doesn't render "Next release" section if there is no valid partial date`, () => {
    render(
      <ReleasePageIntro
        publicationSummary={{
          ...testPublicationSummary,
          nextReleaseDate: undefined,
        }}
        releaseVersionSummary={{
          ...testReleaseVersionSummary,
          isLatestRelease: true,
        }}
      />,
    );

    expect(screen.queryByTestId('Next update')).not.toBeInTheDocument();
  });

  test(`doesn't render "Next release" section if the Release is not the latest Release for the Publication`, () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={{
          ...testReleaseVersionSummary,
          isLatestRelease: false,
        }}
      />,
    );

    expect(screen.queryByTestId('Next update')).not.toBeInTheDocument();
  });

  test('does not render ReleaseSummaryBlock on small screens', () => {
    mockIsMedia = true;
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.queryByTestId('release-summary-block'),
    ).not.toBeInTheDocument();

    mockIsMedia = false;
  });

  test('renders accredited official statistics image', () => {
    const { container } = render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeInTheDocument();
  });

  test('renders accredited official statistics section', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Accredited official statistics Information on Accredited official statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Official statistics Information on Official statistics',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not render image for official statistics', () => {
    const { container } = render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={{
          ...testReleaseVersionSummary,
          type: 'OfficialStatistics',
        }}
      />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).not.toBeInTheDocument();
  });

  test('renders default publishing organisation text', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );
    const producedBy = screen.getByTestId('Produced by-value');

    expect(screen.getByTestId('Produced by-value')).toHaveTextContent(
      'Department for Education',
    );

    expect(
      within(producedBy).getByRole('link', {
        name: 'Department for Education',
      }),
    ).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/organisations/department-for-education',
    );
  });

  test('renders custom publishing organisation text correctly if set', () => {
    render(
      <ReleasePageIntro
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={{
          ...testReleaseVersionSummary,
          publishingOrganisations: [
            {
              id: 'org-id-1',
              title: 'Department for Education',
              url: 'https://www.gov.uk/government/organisations/department-for-education',
            },
            {
              id: 'org-id-2',
              title: 'Other Organisation',
              url: 'https://example.com',
            },
          ],
        }}
      />,
    );
    const producedBy = screen.getByTestId('Produced by-value');

    expect(screen.getByTestId('Produced by-value')).toHaveTextContent(
      'Department for Education and Other Organisation',
    );

    expect(
      within(producedBy).getByRole('link', {
        name: 'Other Organisation',
      }),
    ).toHaveAttribute('href', 'https://example.com');
  });
});
