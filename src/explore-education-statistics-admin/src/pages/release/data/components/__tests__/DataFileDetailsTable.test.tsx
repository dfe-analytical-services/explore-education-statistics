import React from 'react';
import { render, screen } from '@testing-library/react';
import { DataFile } from '@admin/services/releaseDataFileService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { useQuery } from '@tanstack/react-query';
import DataFileDetailsTable from '../DataFileDetailsTable';

jest.mock('@tanstack/react-query', () => ({
  useQuery: jest.fn(),
}));
const mockUseQuery = useQuery as jest.Mock;

const baseDataFile: DataFile = {
  id: '1',
  title: 'Test Data File',
  fileName: 'test.csv',
  metaFileId: 'meta1',
  metaFileName: 'test-meta.csv',
  fileSize: { size: 1234, unit: 'KB' },
  rows: 100,
  userName: 'user@example.com',
  created: '2024-06-01T12:00:00Z',
  status: 'COMPLETE',
  permissions: {
    canCancelImport: true,
  },
};

describe('DataFileDetailsTable - replacementFileHasApi logic', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('does not render API data set attachment status row when publicApiDataSetId is undefined', () => {
    setUpMocksUseQuery();

    renderWithFeatureFlag(
      true,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: undefined,
          replacedBy: undefined,
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(screen.queryByText('Data file import status')).toBeInTheDocument();
    expect(
      screen.queryByText('API data set attachment status'),
    ).not.toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();
  });

  it('does not render API data set attachment status row when replacedBy is undefined', () => {
    setUpMocksUseQuery();

    renderWithFeatureFlag(
      true,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: 'api-1-draft',
          replacedBy: undefined,
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(screen.queryByText('Data file import status')).toBeInTheDocument();
    expect(
      screen.queryByText('API data set attachment status'),
    ).not.toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();
  });

  it('renders Ready tag API data set attachment status row when publicApiDataSetId and replacedBy are defined', () => {
    setUpMocksUseQuery();

    renderWithFeatureFlag(
      true,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: 'api-1-draft',
          replacedBy: 'file-2',
        }}
        replacementDataFile={{
          ...baseDataFile,
          id: '2',
          title: 'Replacement',
          userName: 'other@example.com',
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(
      screen.getByText('API data set attachment status'),
    ).toBeInTheDocument();
    expect(screen.queryByText('Action required')).not.toBeInTheDocument();
    expect(screen.getByText('Ready')).toBeInTheDocument();
  });

  it('renders Action required tag for replacement API status when its mapping)', async () => {
    setUpMocksUseQuery(false, 'Mapping');

    renderWithFeatureFlag(
      true,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: 'api-1-draft',
          replacedBy: 'file-2',
        }}
        replacementDataFile={{
          ...baseDataFile,
          id: '2',
          title: 'Replacement',
          userName: 'other@example.com',
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(
      screen.getByText('API data set attachment status'),
    ).toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();
    expect(screen.getByText('Action required')).toBeInTheDocument();
  });

  it('renders "Processing" tag when isLoading is true', () => {
    setUpMocksUseQuery(true, 'Draft');

    renderWithFeatureFlag(
      true,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: 'api-1-draft',
          replacedBy: 'file-2',
        }}
        replacementDataFile={{
          ...baseDataFile,
          id: '2',
          title: 'Replacement',
          userName: 'other@example.com',
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(
      screen.getByText('API data set attachment status'),
    ).toBeInTheDocument();
    expect(screen.getByText('Processing')).toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();
  });

  it('does not render API data set attachment status row when publicApiDataSetId and replacedBy are defined but the API replacement feature flag is off', () => {
    setUpMocksUseQuery();

    renderWithFeatureFlag(
      false,
      <DataFileDetailsTable
        dataFile={{
          ...baseDataFile,
          publicApiDataSetId: 'api-1-draft',
          replacedBy: 'file-2',
        }}
        replacementDataFile={{
          ...baseDataFile,
          id: '2',
          title: 'Replacement',
          userName: 'other@example.com',
        }}
        releaseVersionId="rel-1"
      />,
    );

    expect(
      screen.queryByText('API data set attachment status'),
    ).not.toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();
  });

  function renderWithFeatureFlag(
    replaceApifeatureFlag: boolean,
    children: React.ReactNode,
  ) {
    const defaultTestConfig = {
      appInsightsKey: '',
      publicAppUrl: 'http://localhost',
      publicApiUrl: 'http://public-api',
      publicApiDocsUrl: 'http://public-api-docs',
      permittedEmbedUrlDomains: [
        'https://department-for-education.shinyapps.io',
      ],
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
    return render(
      <TestConfigContextProvider
        config={{
          ...defaultTestConfig,
          enableReplacementOfPublicApiDataSets: replaceApifeatureFlag,
        }}
      >
        {children}
      </TestConfigContextProvider>,
    );
  }
  function setUpMocksUseQuery(isLoading = false, status = 'Draft') {
    mockUseQuery.mockImplementation(({ queryKey }: { queryKey?: string[] }) => {
      if (queryKey?.[0] === 'apiDataSetQueries' && queryKey?.[1] === 'get') {
        return {
          data: { draftVersion: { status } },
          isLoading,
        };
      }
      return { data: undefined, isLoading: true };
    });
  }
});
