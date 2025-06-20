import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import DataFilesTableRow from '../DataFilesTableRow';

describe('DataFilesTableRow', () => {
  const baseProps = {
    canUpdateRelease: true,
    publicationId: 'pub-1',
    releaseVersionId: 'release-1',
    onConfirmDelete: jest.fn(),
    onStatusChange: jest.fn(),
  };

  const mockDataFile = {
    fileName: '',
    metaFileName: '',
    metaFileId: '',
    userName: '',
    id: 'file-1',
    title: 'Test File',
    status: 'COMPLETE' as const,
    fileSize: { size: 1000, unit: 'B' },
    permissions: { canCancelImport: false },
  };

  const defaultTestConfig = {
    appInsightsKey: '',
    publicAppUrl: 'http://localhost',
    publicApiUrl: 'http://public-api',
    publicApiDocsUrl: 'http://public-api-docs',
    permittedEmbedUrlDomains: ['https://department-for-education.shinyapps.io'],
    oidc: {
      clientId: '',
      authority: '',
      knownAuthorities: [''],
      adminApiScope: '',
      authorityMetadata: {
        authorizationEndpoint: '',
        tokenEndpoint: '',
        issuer: '',
        userInfoEndpoint: '',
        endSessionEndpoint: '',
      },
    },
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('when feature flag enableReplacementOfPublicApiDataSets is toggled', () => {
    test('shows no API modal that stops the user when feature flag enableReplacementOfPublicApiDataSets is enabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };

      render(
        <TestConfigContextProvider
          config={{
            ...defaultTestConfig,
            enableReplacementOfPublicApiDataSets: true,
          }}
        >
          <MemoryRouter>
            <table>
              <thead>
                <tr>
                  <th scope="col">Title</th>
                  <th scope="col">Size</th>
                  <th scope="col">Status</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              <tbody>
                <DataFilesTableRow {...baseProps} dataFile={dataFile} />
              </tbody>
            </table>
          </MemoryRouter>
        </TestConfigContextProvider>,
      );
      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      fireEvent.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(/This data file has an API data set linked to it/),
        ).not.toBeInTheDocument();
      });
    });

    test('shows - modal that stops the user, when feature flag enableReplacementOfPublicApiDataSets is disabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };

      render(
        <TestConfigContextProvider
          config={{
            ...defaultTestConfig,
            enableReplacementOfPublicApiDataSets: false,
          }}
        >
          <MemoryRouter>
            <table>
              <thead>
                <tr>
                  <th scope="col">Title</th>
                  <th scope="col">Size</th>
                  <th scope="col">Status</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              <tbody>
                <DataFilesTableRow {...baseProps} dataFile={dataFile} />
              </tbody>
            </table>
          </MemoryRouter>
        </TestConfigContextProvider>,
      );
      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      fireEvent.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(/This data file has an API data set linked to it/),
        ).toBeInTheDocument();
      });
    });
  });
});
