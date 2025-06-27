import { FeatureFlags } from '@admin/config/featureFlags';
import { FeatureFlagProvider } from '@admin/contexts/FeatureFlagContext';
import DataFilesTableRow from '@admin/pages/release/data/components/DataFilesTableRow';
import { DataFile } from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { screen, waitFor } from '@testing-library/react';

describe('DataFilesTableRow', () => {
  const baseProps = {
    canUpdateRelease: true,
    publicationId: 'pub-1',
    releaseVersionId: 'release-1',
    onConfirmDelete: jest.fn(),
    onStatusChange: jest.fn(),
  };

  const mockDataFile: DataFile = {
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

  describe('when feature flag enableReplacementOfPublicApiDataSets is toggled', () => {
    test('shows no API modal that stops the user when feature flag enableReplacementOfPublicApiDataSets is enabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testFeatureFlag: FeatureFlags = {
        enableReplacementOfPublicApiDataSets: true,
      };
      const { user } = render(
        <FeatureFlagProvider initialFlags={testFeatureFlag}>
          <MemoryRouter>
            <DataFilesTableRow {...baseProps} dataFile={dataFile} />
          </MemoryRouter>
        </FeatureFlagProvider>,
      );
      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      await user.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(/This data file has an API data set linked to it/),
        ).not.toBeInTheDocument();
      });
    });

    test('shows - modal that stops the user, when feature flag enableReplacementOfPublicApiDataSets is disabled', async () => {
      const dataFile = { ...mockDataFile, publicApiDataSetId: 'dataset-1' };
      const testFeatureFlag: FeatureFlags = {
        enableReplacementOfPublicApiDataSets: false,
      };
      const { user } = render(
        <FeatureFlagProvider initialFlags={testFeatureFlag}>
          <MemoryRouter>
            <DataFilesTableRow {...baseProps} dataFile={dataFile} />
          </MemoryRouter>
        </FeatureFlagProvider>,
      );
      const replaceLink = screen.getByText('Replace data');
      expect(replaceLink).toBeInTheDocument();

      await user.click(replaceLink);

      await waitFor(() => {
        expect(
          screen.queryByText(/This data file has an API data set linked to it/),
        ).toBeInTheDocument();
      });
    });
  });
});
