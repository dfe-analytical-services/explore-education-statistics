import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
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
        <ReleaseVersionContextProvider releaseVersion={testRelease}>
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
          </TestConfigContextProvider>
        </ReleaseVersionContextProvider>,
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
          <ReleaseVersionContextProvider releaseVersion={testRelease}>
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
          </ReleaseVersionContextProvider>
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

    test('does not show draft API modal when the release is an amendment', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testReleaseAmendment = { ...testRelease, amendment: true };

      render(
        <ReleaseVersionContextProvider releaseVersion={testReleaseAmendment}>
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
          </TestConfigContextProvider>
        </ReleaseVersionContextProvider>,
      );

      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      fireEvent.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(
            /This data replacement can not be completed as it is targeting an existing draft API data set/i,
          ),
        ).not.toBeInTheDocument();
      });
    });

    test('shows draft API modal when not an amendment and dataFile has publicApiDataSetId', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testReleaseDraft = { ...testRelease, amendment: false };

      render(
        <ReleaseVersionContextProvider releaseVersion={testReleaseDraft}>
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
          </TestConfigContextProvider>
        </ReleaseVersionContextProvider>,
      );

      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      fireEvent.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.getByText(
            /This data replacement can not be completed as it is targeting an existing draft API data set/i,
          ),
        ).toBeInTheDocument();
        expect(
          screen.queryByText(/Go to API data set/),
        ).not.toBeInTheDocument();
      });
    });

    test('does not show draft API modal when dataFile has no publicApiDataSetId', async () => {
      const dataFile = { ...mockDataFile };
      const testReleaseDraft = { ...testRelease, amendment: true };
      const testQueryClient = createTestQueryClient();

      render(
        <ReleaseVersionContextProvider releaseVersion={testReleaseDraft}>
          <QueryClientProvider client={testQueryClient}>
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
            </TestConfigContextProvider>
          </QueryClientProvider>
        </ReleaseVersionContextProvider>,
      );

      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      fireEvent.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(
            /This data replacement can not be completed as it is targeting an existing draft API data set/i,
          ),
        ).not.toBeInTheDocument();
      });
    });

    const createTestQueryClient = () => {
      return new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
          },
        },
      });
    };
  });
});
