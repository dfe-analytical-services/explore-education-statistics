import { AuthContext } from '@admin/contexts/AuthContext';
import _releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import DataFilesReplacementTableRow from '@admin/pages/release/data/components/DataFilesReplacementTableRow';
import render from '@common-test/render';
import React from 'react';
import { act, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseDataFileService');
jest.mock('@admin/services/dataReplacementService');

const releaseDataFileService = jest.mocked(_releaseDataFileService);
const dataReplacementService = jest.mocked(_dataReplacementService);

describe('DataFilesReplacementTableRow', () => {
  const testDataFile: DataFile = {
    fileName: '',
    metaFileName: '',
    metaFileId: '',
    userName: '',
    id: 'file-1',
    replacedBy: 'file-1-replacement',
    title: 'Test File',
    status: 'COMPLETE',
    fileSize: { size: 1000, unit: 'B' },
    permissions: { canCancelImport: false },
  };

  const testReplacementDataFile: DataFile = {
    fileName: '',
    metaFileName: '',
    metaFileId: '',
    userName: '',
    id: 'file-1-replacement',
    title: 'Test File',
    status: 'COMPLETE',
    fileSize: { size: 1000, unit: 'B' },
    permissions: { canCancelImport: false },
  };

  const testDataReplacementPlan: DataReplacementPlan = {
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-1',
    dataBlocks: [],
    footnotes: [],
    apiDataSetVersionPlan: {
      id: '',
      dataSetId: '',
      name: '',
      version: '',
      status: '',
      valid: true,
      readyToPublish: false,
    },
    valid: true,
  };

  const defaultPermissions = {
    isBauUser: true,
    canAccessSystem: true,
    canAccessPrereleasePages: true,
    canAccessAnalystPages: true,
    canAccessAllImports: true,
    canManageAllTaxonomy: true,
    isApprover: true,
  };

  const user = {
    id: 'user-1',
    name: 'Test User',
    permissions: defaultPermissions,
  };

  test('renders with a valid replacement', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(
      testReplacementDataFile,
    );

    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testDataReplacementPlan,
    );

    await act(async () => {
      render(
        <MemoryRouter>
          <table>
            <tbody>
              <DataFilesReplacementTableRow
                dataFile={testDataFile}
                publicationId="test-publication"
                releaseVersionId="test-release-version"
                onReplacementStatusChange={() => {}}
              />
            </tbody>
          </table>
        </MemoryRouter>,
      );
    });

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Ready')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Ready');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).getByRole('button', { name: 'Confirm replacement' }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).getByRole('button', { name: 'Cancel replacement' }),
    ).toBeInTheDocument();
  });

  test('renders with a replacement error', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(
      testReplacementDataFile,
    );

    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testDataReplacementPlan,
      valid: false,
    });

    render(
      <MemoryRouter>
        <table>
          <tbody>
            <DataFilesReplacementTableRow
              dataFile={testDataFile}
              publicationId="test-publication"
              releaseVersionId="test-release-version"
              onReplacementStatusChange={() => {}}
            />
          </tbody>
        </table>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Error')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Error');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Confirm replacement' }),
    ).not.toBeInTheDocument();
  });

  test('does not show the confirm and cancel buttons while the replacement is being processed', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testReplacementDataFile,
      status: 'STAGE_3',
    });

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      errors: [],
      percentageComplete: 67,
      stagePercentageComplete: 59,
      status: 'STAGE_3',
      totalRows: 5904158,
    });

    render(
      <MemoryRouter>
        <table>
          <tbody>
            <DataFilesReplacementTableRow
              dataFile={testDataFile}
              publicationId="test-publication"
              releaseVersionId="test-release-version"
              onReplacementStatusChange={() => {}}
            />
          </tbody>
        </table>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Importing')).toBeInTheDocument();
    expect(screen.queryByText('Ready')).not.toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Importing');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Confirm replacement' }),
    ).not.toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Cancel replacement' }),
    ).not.toBeInTheDocument();
  });

  test('does not show confirm and cancel buttons when user has no permission to edit (not BAU user and data file is linked to API)', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testReplacementDataFile,
      status: 'COMPLETE',
      publicApiDataSetId: 'api-dataset-id',
    });

    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testDataReplacementPlan,
      valid: true,
    });
    render(
      <MemoryRouter>
        <table>
          <tbody>
            <AuthContext.Provider
              value={{
                user: {
                  ...user,
                  permissions: { ...defaultPermissions, isBauUser: false },
                },
              }}
            >
              <DataFilesReplacementTableRow
                dataFile={{
                  ...testDataFile,
                  publicApiDataSetId: 'api-dataset-id',
                }}
                publicationId="test-publication"
                releaseVersionId="test-release-version"
                onReplacementStatusChange={() => {}}
              />
            </AuthContext.Provider>
          </tbody>
        </table>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Ready')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Ready');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Confirm replacement' }),
    ).not.toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Cancel replacement' }),
    ).not.toBeInTheDocument();
  });

  test('show confirm and cancel buttons when user analyst but the data file has no API linked', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testReplacementDataFile,
      status: 'COMPLETE',
      publicApiDataSetId: undefined,
    });

    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testDataReplacementPlan,
      valid: true,
    });

    render(
      <MemoryRouter>
        <table>
          <tbody>
            <AuthContext.Provider
              value={{
                user: {
                  ...user,
                  permissions: { ...defaultPermissions, isBauUser: false },
                },
              }}
            >
              <DataFilesReplacementTableRow
                dataFile={{
                  ...testDataFile,
                  publicApiDataSetId: undefined,
                }}
                publicationId="test-publication"
                releaseVersionId="test-release-version"
                onReplacementStatusChange={() => {}}
              />
            </AuthContext.Provider>
          </tbody>
        </table>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Ready')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Ready');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Confirm replacement' }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Cancel replacement' }),
    ).toBeInTheDocument();
  });

  test('shows confirm and cancel buttons when user has permission to edit (is BAU user)', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testReplacementDataFile,
      status: 'COMPLETE',
      publicApiDataSetId: 'api-dataset-id',
    });

    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testDataReplacementPlan,
      valid: true,
    });

    render(
      <MemoryRouter>
        <table>
          <tbody>
            <AuthContext.Provider
              value={{
                user: {
                  ...user,
                  permissions: { ...defaultPermissions, isBauUser: true },
                },
              }}
            >
              <DataFilesReplacementTableRow
                dataFile={{
                  ...testDataFile,
                  publicApiDataSetId: 'api-dataset-id',
                }}
                publicationId="test-publication"
                releaseVersionId="test-release-version"
                onReplacementStatusChange={() => {}}
              />
            </AuthContext.Provider>
          </tbody>
        </table>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Ready')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Ready');
    expect(
      within(cells[3]).getByRole('link', {
        name: 'View details for Test File',
      }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).getByRole('button', { name: 'Confirm replacement' }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).getByRole('button', { name: 'Cancel replacement' }),
    ).toBeInTheDocument();
  });
});
