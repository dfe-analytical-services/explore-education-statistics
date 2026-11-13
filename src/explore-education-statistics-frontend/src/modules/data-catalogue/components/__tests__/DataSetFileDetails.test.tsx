import render from '@common-test/render';
import { Organisation } from '@common/services/types/organisation';
import { testDataSetFile } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileDetails', () => {
  test('renders the data set details', async () => {
    render(<DataSetFileDetails dataSetFile={testDataSetFile} />);

    expect(
      within(screen.getByTestId('Theme')).getByText('Theme 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Publication')).getByText('Publication 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Release')).getByRole('link', {
        name: 'Release 1',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/publication-1-slug/release-1-slug',
    );
    expect(
      within(screen.getByTestId('Release type')).getByRole('button', {
        name: /Accredited official statistics/,
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Number of rows')).getByText('65'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National',
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByTestId(
        'Geographic levels (only available via download / public API)',
      ),
    ).not.toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2023 to 2024'),
    ).toBeInTheDocument();
  });

  test('renders csv-only geographic levels in a separate row', async () => {
    render(
      <DataSetFileDetails
        dataSetFile={{
          ...testDataSetFile,
          file: {
            ...testDataSetFile.file,
            meta: {
              ...testDataSetFile.file.meta,
              geographicLevelsCsvOnly: ['School', 'Institution'],
            },
          },
        }}
      />,
    );

    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National',
      ),
    ).toBeInTheDocument();
    expect(
      within(
        screen.getByTestId(
          'Geographic levels (only available via download / public API)',
        ),
      ).getByText('Institution, School'),
    ).toBeInTheDocument();
  });

  test('clicking the release type shows the modal', async () => {
    const { user } = render(
      <DataSetFileDetails dataSetFile={testDataSetFile} />,
    );

    await user.click(
      screen.getByRole('button', { name: /Accredited official statistics/ }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Accredited official statistics' }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('button', { name: 'Close modal' }),
    ).toBeInTheDocument();
  });

  describe('official statistics in development release type', () => {
    const skillsEngland: Organisation = {
      id: 'organisation-skills-england',
      title: 'Skills England',
      url: 'https://www.gov.uk/government/organisations/skills-england',
      useGISLogo: true,
      logoFileName: 'skills-england-logo.png',
    };

    const [departmentForEducation] =
      testDataSetFile.release.publishingOrganisations;

    async function renderAndOpenReleaseTypeModal(
      publishingOrganisations: Organisation[],
    ) {
      const { user } = render(
        <DataSetFileDetails
          dataSetFile={{
            ...testDataSetFile,
            release: {
              ...testDataSetFile.release,
              type: 'OfficialStatisticsInDevelopment',
              publishingOrganisations,
            },
          }}
        />,
      );

      await user.click(
        within(screen.getByTestId('Release type')).getByRole('button', {
          name: /Official statistics in development/,
        }),
      );

      return within(screen.getByRole('dialog'));
    }

    test('shows the Skills England text when Skills England is the only publishing organisation', async () => {
      const modal = await renderAndOpenReleaseTypeModal([skillsEngland]);

      expect(
        modal.getByText(
          /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
        ),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('link', {
          name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
        }),
      ).toHaveAttribute(
        'href',
        'https://www.gov.uk/government/publications/skills-england-standards-for-official-statistics',
      );

      expect(
        modal.queryByText(/Head of Profession for Statistics/),
      ).not.toBeInTheDocument();
      expect(
        modal.queryByRole('link', {
          name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
        }),
      ).not.toBeInTheDocument();
    });

    test('shows the default text when DfE is the publishing organisation', async () => {
      const modal = await renderAndOpenReleaseTypeModal([
        departmentForEducation,
      ]);

      expect(
        modal.getByText(
          /They have been developed under the guidance of the Head of Profession for Statistics/,
        ),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('link', {
          name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
        }),
      ).toBeInTheDocument();

      expect(
        modal.queryByText(/Skills England Lead Official for Statistics/),
      ).not.toBeInTheDocument();
    });

    test('shows the default text when Skills England is not the only publishing organisation', async () => {
      const modal = await renderAndOpenReleaseTypeModal([
        departmentForEducation,
        skillsEngland,
      ]);

      expect(
        modal.getByText(
          /They have been developed under the guidance of the Head of Profession for Statistics/,
        ),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('link', {
          name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
        }),
      ).toBeInTheDocument();

      expect(
        modal.queryByText(/Skills England Lead Official for Statistics/),
      ).not.toBeInTheDocument();
      expect(
        modal.queryByRole('link', {
          name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
        }),
      ).not.toBeInTheDocument();
    });
  });
});
