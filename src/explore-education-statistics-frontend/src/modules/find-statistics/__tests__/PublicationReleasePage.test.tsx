import { ReleaseVersion } from '@common/services/publicationService';
import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { testPublication, testRelease } from './__data__/testReleaseData';

describe('PublicationReleasePage', () => {
  test('renders latest data tag', () => {
    render(<PublicationReleasePage releaseVersion={testRelease} />);

    expect(
      screen.queryByText('This is the latest release'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('This is not the latest release'),
    ).not.toBeInTheDocument();
  });

  test('does not render latest data tag when publication is superseded', () => {
    const testReleaseSuperseded: ReleaseVersion = {
      ...testRelease,
      publication: { ...testPublication, isSuperseded: true },
    };
    render(<PublicationReleasePage releaseVersion={testReleaseSuperseded} />);

    expect(
      screen.queryByText('This is the latest release'),
    ).not.toBeInTheDocument();
  });

  test('renders not latest data link and tag when publication is not the latest', () => {
    const testReleaseNotLatest: ReleaseVersion = {
      ...testRelease,
      latestRelease: false,
    };
    render(<PublicationReleasePage releaseVersion={testReleaseNotLatest} />);

    expect(
      screen.getByText('This is not the latest release'),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('This is the latest release'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View latest data: Academic year 2018/19',
      }),
    ).toBeInTheDocument();
  });

  test('renders superseded warning text when publication is superseded', () => {
    const testReleaseSuperseded: ReleaseVersion = {
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

    render(<PublicationReleasePage releaseVersion={testReleaseSuperseded} />);

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
    const testReleaseSuperseded: ReleaseVersion = {
      ...testRelease,
      publication: {
        ...testPublication,
        isSuperseded: false,
      },
    };

    render(<PublicationReleasePage releaseVersion={testReleaseSuperseded} />);

    expect(screen.queryByTestId('superseded-warning')).not.toBeInTheDocument();
  });

  test('renders quick links', async () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
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

  test(`renders 'Download all data (zip)' as a link when files present`, async () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
          ...testRelease,
          relatedDashboardsSection: undefined,
          downloadFiles: [
            {
              id: 'file-id',
              extension: 'csv',
              fileName: 'test-file',
              name: 'test file',
              size: '1 MB',
              type: 'Data',
            },
          ],
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

    expect(quickLinks[0]).toHaveTextContent('Download all data (zip)');
    expect(quickLinks[0]).toHaveAttribute('href');
  });

  test('does not render release contents link when there is no content', async () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
          ...testRelease,
          content: [],
        }}
      />,
    );

    const quickLinksNav = screen.getByRole('navigation', {
      name: 'Quick links',
    });

    expect(
      within(quickLinksNav).queryByRole('link', { name: 'Release contents' }),
    ).not.toBeInTheDocument();
  });

  test(`renders quick link to view related dashboard(s) when section exists`, async () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
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

  test(`renders other releases including legacy links`, async () => {
    render(<PublicationReleasePage releaseVersion={testRelease} />);

    const usefulInfo = within(screen.getByTestId('useful-information'));

    expect(
      usefulInfo.getByRole('heading', { name: 'Releases in this series' }),
    ).toBeInTheDocument();

    const details = within(usefulInfo.getByRole('group'));

    await userEvent.click(
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

    expect(otherReleaseLinks[1]).toHaveTextContent('Academic year 2014/15');
    expect(otherReleaseLinks[1]).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
    );

    expect(otherReleaseLinks[2]).toHaveTextContent('Academic year 2017/18');
    expect(otherReleaseLinks[2]).toHaveAttribute(
      'href',
      '/find-statistics/pupil-absence-in-schools-in-england/2017-18',
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

  test('renders accredited official statistics image', () => {
    const { container } = render(
      <PublicationReleasePage releaseVersion={testRelease} />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeInTheDocument();
  });

  test('renders accredited official statistics section', () => {
    render(<PublicationReleasePage releaseVersion={testRelease} />);

    expect(
      screen.getByRole('heading', { name: 'Accredited official statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('does not render image for official statistics', () => {
    const { container } = render(
      <PublicationReleasePage
        releaseVersion={{
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
        releaseVersion={{
          ...testRelease,
          type: 'OfficialStatistics',
        }}
      />,
    );

    expect(
      screen.queryByRole('heading', { name: 'Accredited official statistics' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Official statistics' }),
    ).toBeInTheDocument();
  });

  test('renders "Next update" section if this is the latest Release for a Publication and there is a valid partial date', () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
          ...testRelease,
          latestRelease: true,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
        }}
      />,
    );

    const nextUpdateValue = screen.getByTestId('Next update-value');
    expect(nextUpdateValue.textContent).toEqual('February 2022');
  });

  test(`doesn't render "Next update" section if there is no valid partial date`, () => {
    render(
      <PublicationReleasePage
        releaseVersion={{
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
        releaseVersion={{
          ...testRelease,
          latestRelease: false,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
        }}
      />,
    );

    expect(
      screen.queryByTestId('next-update-list-item-value'),
    ).not.toBeInTheDocument();
  });

  test('renders "Last Updated" section correctly with updates in correct order', async () => {
    render(<PublicationReleasePage releaseVersion={testRelease} />);

    expect(screen.getByTestId('Last updated')).toHaveTextContent(
      '19 April 2018',
    );

    await userEvent.click(
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
    render(<PublicationReleasePage releaseVersion={testRelease} />);
    const usefulInfo = screen.getByTestId('useful-information');

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
    const testReleaseWithExternalMethodology: ReleaseVersion = {
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
      <PublicationReleasePage
        releaseVersion={testReleaseWithExternalMethodology}
      />,
    );
    const usefulInfo = screen.getByTestId('useful-information');

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
    const testReleaseWithInternalAndExternalMethodologies: ReleaseVersion = {
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
        releaseVersion={testReleaseWithInternalAndExternalMethodologies}
      />,
    );
    const usefulInfo = screen.getByTestId('useful-information');

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

  test('renders default publishing organisation text', () => {
    render(<PublicationReleasePage releaseVersion={testRelease} />);
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
      <PublicationReleasePage
        releaseVersion={{
          ...testRelease,
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
