import { ReleaseType } from '@common/services/publicationService';
import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { testRelease } from './__data__/testReleaseData';

describe('PublicationReleasePage', () => {
  test('renders national statistics image', () => {
    const { container } = render(
      <PublicationReleasePage release={testRelease} />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeDefined();
  });

  test('renders national statistics section', () => {
    render(<PublicationReleasePage release={testRelease} />);

    expect(
      screen.getByRole('button', { name: 'National Statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Official Statistics' }),
    ).not.toBeInTheDocument();
  });

  test('renders official statistics image', () => {
    const { container } = render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          type: {
            id: 'official-stats-1',
            title: ReleaseType.OfficialStatistics,
          },
        }}
      />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeNull();
  });

  test('renders official statistics section', () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          type: {
            id: 'official-stats-1',
            title: ReleaseType.OfficialStatistics,
          },
        }}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'National Statistics' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Official Statistics' }),
    ).toBeInTheDocument();
  });

  test('renders "Next update" section if this is the latest Release for a Publication and there is a valid partial date', () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          latestRelease: true,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
        }}
      />,
    );

    const nextUpdateValue = screen.getByTestId('next-update-list-item-value');
    expect(nextUpdateValue.textContent).toEqual('February 2022');
  });

  test(`doesn't render "Next update" section if there is no valid partial date`, () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          latestRelease: true,
          nextReleaseDate: undefined,
        }}
      />,
    );

    expect(
      screen.queryByTestId('next-update-list-item-value'),
    ).not.toBeInTheDocument();
  });

  test(`doesn't render "Next update" section if the Release is not the latest Release for the Publication`, () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          latestRelease: false,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
          publication: {
            ...testRelease.publication,
            otherReleases: [
              {
                id: 'latest-release',
                title: 'Latest Release Title',
                slug: 'latest-release-slug',
              },
            ],
          },
        }}
      />,
    );

    expect(
      screen.queryByTestId('next-update-list-item-value'),
    ).not.toBeInTheDocument();
  });

  test('renders "Last Updated" section correctly with updates in correct order', async () => {
    render(<PublicationReleasePage release={testRelease} />);

    expect(screen.getByTestId('Last updated-value')).toHaveTextContent(
      '19 April 2018',
    );

    userEvent.click(
      screen.getByRole('button', { name: 'See all updates (2)' }),
    );

    const updates = within(screen.getByTestId('all-updates')).getAllByRole(
      'listitem',
    );

    const update1 = within(updates[0]);
    expect(update1.getByTestId('update-on')).toHaveTextContent('19 April 2018');
    expect(update1.getByTestId('update-reason')).toHaveTextContent(
      'Second update',
    );

    const update2 = within(updates[1]);
    expect(update2.getByTestId('update-on')).toHaveTextContent('22 March 2018');
    expect(update2.getByTestId('update-reason')).toHaveTextContent(
      'First update',
    );
  });

  test('renders link to a methodology', () => {
    render(<PublicationReleasePage release={testRelease} />);
    const usefulInfo = screen.getByRole('complementary');

    expect(
      within(usefulInfo).getByRole('heading', { name: 'Methodologies' }),
    ).toBeInTheDocument();

    expect(
      within(usefulInfo).getByRole('link', {
        name: 'Pupil absence statistics: methodology',
      }),
    ).toHaveAttribute(
      'href',
      '/methodology/pupil-absence-in-schools-in-england',
    );
  });

  test('renders link to an external methodology', () => {
    const testReleaseWithExternalMethodology = {
      ...testRelease,
      publication: {
        ...testRelease.publication,
        methodologies: [],
        externalMethodology: {
          title: 'External methodology title',
          url: 'http://gov.uk',
        },
      },
    };
    render(
      <PublicationReleasePage release={testReleaseWithExternalMethodology} />,
    );
    const usefulInfo = screen.getByRole('complementary');

    expect(
      within(usefulInfo).getByRole('heading', { name: 'Methodologies' }),
    ).toBeInTheDocument();

    expect(
      within(usefulInfo).getByRole('link', {
        name: 'External methodology title',
      }),
    ).toHaveAttribute('href', 'http://gov.uk');
  });

  test('renders links to internal and external methodologies', () => {
    const testReleaseWithInternalAndExternalMethodologies = {
      ...testRelease,
      publication: {
        ...testRelease.publication,
        externalMethodology: {
          title: 'External methodology title',
          url: 'http://gov.uk',
        },
      },
    };
    render(
      <PublicationReleasePage
        release={testReleaseWithInternalAndExternalMethodologies}
      />,
    );
    const usefulInfo = screen.getByRole('complementary');

    expect(
      within(usefulInfo).getByRole('heading', { name: 'Methodologies' }),
    ).toBeInTheDocument();

    expect(
      within(usefulInfo).getByRole('link', {
        name: 'Pupil absence statistics: methodology',
      }),
    ).toHaveAttribute(
      'href',
      '/methodology/pupil-absence-in-schools-in-england',
    );

    expect(
      within(usefulInfo).getByRole('link', {
        name: 'External methodology title',
      }),
    ).toHaveAttribute('href', 'http://gov.uk');
  });
});
