import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { FeatureFlags } from '@admin/config/featureFlags';
import { FeatureFlagProvider } from '@admin/contexts/FeatureFlagContext';
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

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('when feature flag is toggled', () => {
    test('shows no modal when feature flag is enabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testFeatureFlag: FeatureFlags = {
        enableReplacementOfPublicApiDataSets: true,
      };
      render(
        <FeatureFlagProvider initialFlags={testFeatureFlag}>
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
        </FeatureFlagProvider>,
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

    test('shows modal when feature flag is disabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testFeatureFlag: FeatureFlags = {
        enableReplacementOfPublicApiDataSets: false,
      };
      render(
        <FeatureFlagProvider initialFlags={testFeatureFlag}>
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
        </FeatureFlagProvider>,
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
