import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { testPublication, testRelease } from './__data__/testReleaseData';

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

  test('renders latest data tag', () => {
    render(<PublicationReleasePage release={testRelease} />);

    expect(screen.queryByText('This is the latest data')).toBeInTheDocument();
  });

  test('does not render latest data tag when publication is superseded', () => {
    const testReleaseSuperseded = {
      ...testRelease,
      publication: { ...testPublication, isSuperseded: true },
    };
    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders superseded warning text when publication is superseded', () => {
    const testReleaseSuperseded = {
      ...testRelease,
      publication: {
        ...testPublication,
        isSuperseded: true,
        supersededByTitle: 'publication A',
        supersededBySlug: 'publication-a',
      },
    };

    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    expect(
      screen.getByTestId('superseded-by-warning').querySelector('a'),
    ).toHaveAttribute('href', '/find-statistics/publication-a');

    expect(
      screen.getByTestId('superseded-by-warning').querySelector('a'),
    ).toHaveTextContent('publication A');
  });

  test("doesn't render superseded warning text when publication is superseded", () => {
    const testReleaseSuperseded = {
      ...testRelease,
      publication: {
        ...testPublication,
        isSuperseded: false,
      },
    };

    render(<PublicationReleasePage release={testReleaseSuperseded} />);

    expect(
      screen.queryByTestId('superseded-by-warning'),
    ).not.toBeInTheDocument();
  });

  test('renders data downloads links', async () => {
    render(
      <PublicationReleasePage
        release={{
          ...testRelease,
          relatedDashboardsSection: undefined,
        }}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Data downloads' }),
    ).toBeInTheDocument();

    const dataDownloadsNav = screen.getByRole('navigation', {
      name: 'Data downloads',
    });

    const dataDownloadsLinks = within(dataDownloadsNav).getAllByRole('link');

    expect(dataDownloadsLinks).toHaveLength(2);

    expect(dataDownloadsLinks[0]).toHaveTextContent('Explore data and files');
    expect(dataDownloadsLinks[0]).toHaveAttribute(
      'href',
      '#explore-data-and-files',
    );

    expect(dataDownloadsLinks[1]).toHaveTextContent('View data guidance');
    expect(dataDownloadsLinks[1]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/data-guidance',
    );
  });

  test(`renders data download link to view related dashboard(s) when section exists`, async () => {
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
      screen.getByRole('navigation', { name: 'Data downloads' }),
    ).toBeInTheDocument();

    const dataDownloadsNav = screen.getByRole('navigation', {
      name: 'Data downloads',
    });

    const dataDownloadsLinks = within(dataDownloadsNav).getAllByRole('link');

    expect(dataDownloadsLinks).toHaveLength(3);

    expect(dataDownloadsLinks[0]).toHaveTextContent('Explore data and files');
    expect(dataDownloadsLinks[0]).toHaveAttribute(
      'href',
      '#explore-data-and-files',
    );

    expect(dataDownloadsLinks[1]).toHaveTextContent('View data guidance');
    expect(dataDownloadsLinks[1]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/data-guidance',
    );

    expect(dataDownloadsLinks[2]).toHaveTextContent(
      'View related dashboard(s)',
    );
    expect(dataDownloadsLinks[2]).toHaveAttribute(
      'href',
      '#related-dashboards',
    );
  });

  test(`renders other releases including legacy releases`, async () => {
    render(<PublicationReleasePage release={testRelease} />);

    const usefulInfo = within(screen.getByRole('complementary'));

    expect(
      usefulInfo.getByRole('heading', { name: 'Past releases' }),
    ).toBeInTheDocument();

    expect(usefulInfo.getByTestId('current-release-title')).toHaveTextContent(
      'Academic Year 2016/17',
    );

    const details = within(usefulInfo.getByRole('group'));

    userEvent.click(
      details.getByRole('button', {
        name: 'See other releases (5) for Pupil absence in schools in England',
      }),
    );

    const otherReleaseLinks = details.getAllByRole('link');

    expect(otherReleaseLinks).toHaveLength(5);

    expect(otherReleaseLinks[0]).toHaveTextContent('Academic Year 2018/19');
    expect(otherReleaseLinks[0]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/2018-19',
    );

    expect(otherReleaseLinks[1]).toHaveTextContent('Academic Year 2017/18');
    expect(otherReleaseLinks[1]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/2017-18',
    );

    expect(otherReleaseLinks[2]).toHaveTextContent('Academic Year 2014/15');
    expect(otherReleaseLinks[2]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
    );

    expect(otherReleaseLinks[3]).toHaveTextContent('Academic Year 2013/14');
    expect(otherReleaseLinks[3]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014',
    );

    expect(otherReleaseLinks[4]).toHaveTextContent('Academic Year 2012/13');
    expect(otherReleaseLinks[4]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013',
    );
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

  test('renders official statistics image', () => {
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
    ).toBeNull();
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

    expect(screen.getByTestId('Last updated-value')).toHaveTextContent(
      '19 April 2018',
    );

    userEvent.click(
      screen.getByRole('button', {
        name: `See all updates (2) for Academic Year 2016/17`,
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
