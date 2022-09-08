import TableToolFinalStep from '@frontend/modules/table-tool/components/TableToolFinalStep';
import { render, screen, waitFor } from '@testing-library/react';
import _publicationService from '@common/services/publicationService';
import {
  testTable,
  testTableHeaders,
  testQuery,
  testSelectedPublicationWithLatestRelease,
  testSelectedPublicationWithNonLatestRelease,
  testPublicationRelease,
} from '@frontend/modules/table-tool/components/__tests__/__data__/tableData';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@common/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('TableToolFinalStep', () => {
  test('renders the final step successfully', async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );

    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    // test the reordering controls for the table headers are present
    expect(
      screen.getByRole('button', {
        name: 'Move and reorder table headers',
      }),
    ).toBeInTheDocument();

    // test that the preview table is rendered correctly
    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "'dates' for 02/04/2021 and 02/06/2020 in England between 2020 Week 23 and 2020 Week 26",
    );

    const table = screen.queryByTestId('dataTableCaption-table');
    expect(table).toBeInTheDocument();
    expect(table).toMatchSnapshot();

    // test the permalink controls are present
    expect(
      screen.queryByRole('button', { name: 'Generate shareable link' }),
    ).toBeInTheDocument();

    // test that the related information is rendered correctly
    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    expect(relatedInfoRevealButton).toBeInTheDocument();

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      expect(
        screen.queryByRole('link', {
          name: 'Test publication, Latest Release Title',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('radio', {
          name: 'Table in ODS format (spreadsheet, with title and footnotes)',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('radio', {
          name: 'Table in CSV format (flat file, with location codes)',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Download table',
        }),
      ).toBeInTheDocument();
    });

    // test that contact us section is rendered correctly
    const contactUsRevealButton = screen.getByRole('button', {
      name: 'Contact us',
    });
    expect(contactUsRevealButton).toBeInTheDocument();

    userEvent.click(contactUsRevealButton);

    await waitFor(() => {
      expect(screen.queryByText('The team name')).toBeInTheDocument();
      expect(
        screen.queryByRole('link', {
          name: 'team@name.com',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows and hides table header reordering controls successfully', async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );

    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    const reorderRevealButton = screen.getByRole('button', {
      name: 'Move and reorder table headers',
    });

    expect(
      screen.queryByRole('button', { name: 'Update and view reordered table' }),
    ).not.toBeInTheDocument();

    userEvent.click(reorderRevealButton);

    await waitFor(() =>
      expect(
        screen.queryByRole('button', {
          name: 'Update and view reordered table',
        }),
      ).toBeInTheDocument(),
    );
  });

  test(`renders the 'View the release for this data' URL with the Release's slug, if the selected Release is not the latest for the Publication`, async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithNonLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      const viewReleaseLink = screen.getByRole('link', {
        name: 'Test publication, Selected Release Title',
      }) as HTMLAnchorElement;
      expect(viewReleaseLink.href).toEqual(
        'http://localhost/find-statistics/test-publication/selected-release-slug',
      );
    });
  });

  test(`renders the 'View the release for this data' URL with only the Publication slug, if the selected Release is the latest Release for that Publication`, async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      const viewReleaseLink = screen.getByRole('link', {
        name: 'Test publication, Latest Release Title',
      }) as HTMLAnchorElement;

      expect(viewReleaseLink.href).toEqual(
        'http://localhost/find-statistics/test-publication',
      );
    });
  });

  test('renders the Table Tool final step correctly when this is the latest data', async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    expect(screen.queryByText('This is the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This data is not from the latest release'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('View latest data link'),
    ).not.toBeInTheDocument();
  });

  test(`renders the Table Tool final step correctly if this is not the latest data`, async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithNonLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('This data is not from the latest release'),
    ).toBeInTheDocument();

    const latestDataLink = screen.queryByTestId(
      'View latest data link',
    ) as HTMLAnchorElement;
    expect(latestDataLink).toBeInTheDocument();
    expect(latestDataLink.href).toEqual(
      'http://localhost/find-statistics/test-publication',
    );
    expect(latestDataLink.text).toContain('View latest data');
    expect(latestDataLink.text).toContain('Latest Release Title');

    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      const viewReleaseLink = screen.getByRole('link', {
        name: 'Test publication, Selected Release Title',
      }) as HTMLAnchorElement;

      expect(viewReleaseLink.href).toEqual(
        'http://localhost/find-statistics/test-publication/selected-release-slug',
      );
    });
  });

  test('renders the methodology link correctly', async () => {
    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationRelease,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      const methodologyLink = screen.getByRole('link', {
        name: 'methodology title',
      }) as HTMLAnchorElement;

      expect(methodologyLink.href).toEqual('http://localhost/methodology/m1');
    });
  });

  test('renders the external methodology link correctly', async () => {
    const testPublicationWithExternalMethodology = {
      ...testPublicationRelease,
      publication: {
        ...testPublicationRelease.publication,
        methodologies: [],
        externalMethodology: {
          url: 'http://somewhere.com',
          title: 'An external methodology',
        },
      },
    };

    publicationService.getLatestPublicationRelease.mockResolvedValue(
      testPublicationWithExternalMethodology,
    );
    render(
      <TableToolFinalStep
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
        onReorderTableHeaders={noop}
      />,
    );

    const relatedInfoRevealButton = screen.getByRole('button', {
      name: 'Related information',
    });

    userEvent.click(relatedInfoRevealButton);

    await waitFor(() => {
      const methodologyLink = screen.getByRole('link', {
        name: 'An external methodology',
      }) as HTMLAnchorElement;

      expect(methodologyLink.href).toEqual('http://somewhere.com/');
    });
  });
});
