import { Release } from '@common/services/publicationService';
import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { testPublication, testRelease } from './__data__/testReleaseData';

describe('PublicationReleasePage', () => {
  test('renders latest data tag', () => {
    render(<PublicationReleasePage release={testRelease} />);

    expect(screen.queryByText('This is the latest data')).toBeInTheDocument();

    expect(
      screen.queryByText('This is not the latest data'),
    ).not.toBeInTheDocument();
  });

  test('does not render latest data tag when publication is superseded', () => {
    const testReleaseSuperseded: Release = {
      ...testRelease,
      publication: { ...testPublication, isSuperseded: true },
    };
    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders not latest data link and tag when publication is not the latest', () => {
    const testReleaseNotLatest: Release = {
      ...testRelease,
      latestRelease: false,
    };
    render(<PublicationReleasePage release={testReleaseNotLatest} />);

    expect(screen.getByText('This is not the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View latest data: Academic year 2018/19',
      }),
    ).toBeInTheDocument();
  });

  test('renders superseded warning text when publication is superseded', () => {
    const testReleaseSuperseded: Release = {
      ...testRelease,
      publication: {
        ...testPublication,
        isSuperseded: true,
        supersededBy: {
          id: 'publication-a',
          title: 'publication A',
          slug: 'publication-a',
        },
      },
    };

    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    const supersededWarningLink = within(
      screen.getByTestId('superseded-warning'),
    ).getByRole('link', {
      name: 'publication A',
    });

    expect(supersededWarningLink).toHaveAttribute(
      'href',
      '/find-statistics/publication-a',
    );
  });

  test('does not render superseded warning text when publication is not superseded', () => {
    const testReleaseSuperseded: Release = {
      ...testRelease,
      publication: {
        ...testPublication,
        isSuperseded: false,
      },
    };

    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    expect(screen.queryByTestId('superseded-warning')).not.toBeInTheDocument();
  });

  test('renders quick links', async () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          relatedDashboardsSection: undefined,
        }}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Quick links' }),
    ).toBeInTheDocument();

    const quickLinksNav = screen.getByRole('navigation', {
      name: 'Quick links',
    });

    const quickLinks = within(quickLinksNav).getAllByRole('link');

    expect(quickLinks).toHaveLength(3);

    expect(quickLinks[0]).toHaveTextContent('Release contents');
    expect(quickLinks[0]).toHaveAttribute('href', '#content');

    expect(quickLinks[1]).toHaveTextContent('Explore data');
    expect(quickLinks[1]).toHaveAttribute('href', '#explore-data-and-files');

    expect(quickLinks[2]).toHaveTextContent('Help and support');
    expect(quickLinks[2]).toHaveAttribute('href', '#help-and-support');
  });

  test(`renders quick link to view related dashboard(s) when section exists`, async () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          relatedDashboardsSection: {
            id: 'related-dashboards-id',
            order: 0,
            heading: '',
            content: [
              {
                id: 'related-dashboards-content-block-id',
                order: 0,
                body: '',
                type: 'HtmlBlock',
              },
            ],
          },
        }}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Quick links' }),
    ).toBeInTheDocument();

    const quickLinksNav = screen.getByRole('navigation', {
      name: 'Quick links',
    });

    const quickLinks = within(quickLinksNav).getAllByRole('link');

    expect(quickLinks).toHaveLength(4);

    expect(quickLinks[0]).toHaveTextContent('View related dashboard(s)');
    expect(quickLinks[0]).toHaveAttribute('href', '#related-dashboards');

    expect(quickLinks[1]).toHaveTextContent('Release contents');
    expect(quickLinks[1]).toHaveAttribute('href', '#content');

    expect(quickLinks[2]).toHaveTextContent('Explore data');
    expect(quickLinks[2]).toHaveAttribute('href', '#explore-data-and-files');

    expect(quickLinks[3]).toHaveTextContent('Help and support');
    expect(quickLinks[3]).toHaveAttribute('href', '#help-and-support');
  });

  test(`renders other releases including legacy releases`, async () => {
    render(<PublicationReleasePage release={testRelease} />);

    const usefulInfo = within(screen.getByRole('complementary'));

    expect(
      usefulInfo.getByRole('heading', { name: 'Releases in this series' }),
    ).toBeInTheDocument();

    const details = within(usefulInfo.getByRole('group'));

    userEvent.click(
      details.getByRole('button', {
        name: 'View releases (5) for Pupil absence in schools in England',
      }),
    );

    const otherReleaseLinks = details.getAllByRole('link');

    expect(otherReleaseLinks).toHaveLength(5);

    expect(otherReleaseLinks[0]).toHaveTextContent('Academic year 2018/19');
    expect(otherReleaseLinks[0]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/2018-19',
    );

    expect(otherReleaseLinks[1]).toHaveTextContent('Academic year 2017/18');
    expect(otherReleaseLinks[1]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/2017-18',
    );

    expect(otherReleaseLinks[2]).toHaveTextContent('Academic year 2014/15');
    expect(otherReleaseLinks[2]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
    );

    expect(otherReleaseLinks[3]).toHaveTextContent('Academic year 2013/14');
    expect(otherReleaseLinks[3]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014',
    );

    expect(otherReleaseLinks[4]).toHaveTextContent('Academic year 2012/13');
    expect(otherReleaseLinks[4]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013',
    );
  });

  test('renders national statistics image', () => {
    const { container } = render(
      <PublicationReleasePage release={testRelease} />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeInTheDocument();
  });

  test('renders national statistics section', () => {
    render(<PublicationReleasePage release={testRelease} />);

    expect(
      screen.getByRole('button', { name: 'National statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('does not render image for official statistics', () => {
    const { container } = render(
      <PublicationReleasePage
        release={{
          ...testRelease,
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

  test('renders official statistics section', () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          type: 'OfficialStatistics',
        }}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'National statistics' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Official statistics' }),
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
            releases: [
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

    expect(screen.getByTestId('Last updated')).toHaveTextContent(
      '19 April 2018',
    );

    userEvent.click(
      screen.getByRole('button', {
        name: `See all updates (2) for Academic year 2016/17`,
      }),
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
    const testReleaseWithExternalMethodology: Release = {
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
    const testReleaseWithInternalAndExternalMethodologies: Release = {
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
