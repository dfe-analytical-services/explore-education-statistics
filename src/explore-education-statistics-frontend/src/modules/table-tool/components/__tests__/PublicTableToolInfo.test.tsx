import { render, screen, waitFor, within } from '@testing-library/react';
import {
  testSelectedPublicationWithLatestRelease,
  testSelectedPublicationWithNonLatestRelease,
  testPublicationRelease,
} from '@frontend/modules/table-tool/components/__tests__/__data__/tableData';
import React from 'react';
import PublicTableToolInfo from '../PublicTableToolInfo';

describe('PublicTableToolInfo', () => {
  test('renders successfully', async () => {
    render(
      <PublicTableToolInfo
        fullPublication={testPublicationRelease}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    // test that the related information is rendered correctly
    expect(
      screen.getByRole('heading', {
        name: 'Related information',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Test publication, Latest Release Title',
      }),
    ).toBeInTheDocument();

    // test that contact us section is rendered correctly
    await waitFor(() => {
      expect(screen.getByText('Contact us')).toBeInTheDocument();
    });

    expect(screen.queryByText('The team name')).toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'team@name.com',
      }),
    ).toBeInTheDocument();
  });

  test(`renders the 'View the release for this data' URL with the Release's slug, if the selected Release is not the latest for the Publication`, async () => {
    render(
      <PublicTableToolInfo
        fullPublication={testPublicationRelease}
        selectedPublication={testSelectedPublicationWithNonLatestRelease}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Test publication, Latest Release Title',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/latest-release-slug',
    );
  });

  test(`renders the 'View the release for this data' URL with only the Publication slug, if the selected Release is the latest Release for that Publication`, async () => {
    render(
      <PublicTableToolInfo
        fullPublication={testPublicationRelease}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Test publication, Latest Release Title',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/latest-release-slug',
    );
  });

  test(`renders correctly if this is not the latest data`, async () => {
    render(
      <PublicTableToolInfo
        fullPublication={testPublicationRelease}
        selectedPublication={testSelectedPublicationWithNonLatestRelease}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Test publication, Latest Release Title',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/latest-release-slug',
    );
  });

  test('renders the methodology link correctly', async () => {
    render(
      <PublicTableToolInfo
        selectedPublication={testSelectedPublicationWithLatestRelease}
        fullPublication={testPublicationRelease}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('methodology title')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'methodology title',
      }),
    ).toHaveAttribute('href', '/methodology/m1');
  });

  test('renders the external methodology link correctly', async () => {
    render(
      <PublicTableToolInfo
        fullPublication={{
          ...testPublicationRelease,
          publication: {
            ...testPublicationRelease.publication,
            methodologies: [],
            externalMethodology: {
              url: 'http://somewhere.com',
              title: 'An external methodology',
            },
          },
        }}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('An external methodology')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'An external methodology',
      }),
    ).toHaveAttribute('href', 'http://somewhere.com');
  });
});
