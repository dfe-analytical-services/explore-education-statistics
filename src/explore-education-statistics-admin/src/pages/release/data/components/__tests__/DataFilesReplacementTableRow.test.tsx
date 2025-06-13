import _releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import DataFilesReplacementTableRow from '@admin/pages/release/data/components/DataFilesReplacementTableRow';
import render from '@common-test/render';
import React from 'react';
import { screen, within } from '@testing-library/react';
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

  test('renders with a valid replacement', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(
      testReplacementDataFile,
    );

    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testDataReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFilesReplacementTableRow
          dataFile={testDataFile}
          publicationId="test-publication"
          releaseVersionId="test-release-version"
        />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Ready')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Ready');
    expect(
      within(cells[3]).getByRole('link', { name: 'View details' }),
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
        <DataFilesReplacementTableRow
          dataFile={testDataFile}
          publicationId="test-publication"
          releaseVersionId="test-release-version"
        />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Test File')).toBeInTheDocument();
    expect(await screen.findByText('Error')).toBeInTheDocument();

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('Test File');
    expect(cells[1]).toHaveTextContent('1,000 B');
    expect(cells[2]).toHaveTextContent('Error');
    expect(
      within(cells[3]).getByRole('link', { name: 'View details' }),
    ).toBeInTheDocument();
    expect(
      within(cells[3]).queryByRole('button', { name: 'Confirm replacement' }),
    ).not.toBeInTheDocument();
  });
});
