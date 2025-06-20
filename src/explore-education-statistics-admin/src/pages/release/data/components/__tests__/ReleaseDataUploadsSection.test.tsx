import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import _releaseDataFileService, {
  DataFileImportStatus,
  DataFile,
  UploadDataFilesRequest,
  UploadZipDataFileRequest,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import _permissionService, {
  DataFilePermissions,
} from '@admin/services/permissionService';
import render from '@common-test/render';

jest.mock('@admin/services/releaseDataFileService');
jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/dataReplacementService');

const releaseDataFileService = jest.mocked(_releaseDataFileService);
const permissionService = jest.mocked(_permissionService);
const dataReplacementService = jest.mocked(_dataReplacementService);

describe('ReleaseDataUploadsSection', () => {
  const testDataFiles: DataFile[] = [
    {
      id: 'data-1',
      title: 'Test data 1',
      userName: 'user1@test.com',
      fileName: 'data-1.csv',
      metaFileId: 'data-1-meta',
      metaFileName: 'data-1.meta.csv',
      rows: 100,
      fileSize: {
        size: 50,
        unit: 'Kb',
      },
      created: '2020-06-12T12:00:00',
      status: 'COMPLETE',
      permissions: {
        canCancelImport: false,
      },
    },
    {
      id: 'data-2',
      title: 'Test data 2',
      userName: 'user2@test.com',
      fileName: 'data-2.csv',
      metaFileId: 'data-2-meta',
      metaFileName: 'data-2.meta.csv',
      rows: 200,
      fileSize: {
        size: 100,
        unit: 'Kb',
      },
      created: '2020-07-01T12:00:00',
      status: 'COMPLETE',
      permissions: {
        canCancelImport: false,
      },
    },
  ];

  const testImportedDataFile: DataFile = {
    id: 'file-1',
    title: 'Test title',
    userName: 'user1@test.com',
    fileName: 'test-data.csv',
    metaFileId: 'file-1-meta',
    metaFileName: 'test-data.meta.csv',
    rows: undefined,
    fileSize: {
      size: 150,
      unit: 'Kb',
    },
    created: '2020-08-18T12:00:00',
    status: 'QUEUED',
    permissions: {
      canCancelImport: true,
    },
  };

  const testUploadResult: DataSetUpload = {
    dataFileName: 'test.csv',
    dataSetTitle: 'Data set 1',
    metaFileName: 'test.meta.csv',
    status: 'SCREENING',
    created: new Date('2000-01-01'),
    dataFileSize: '50 Kb',
    id: 'test-data',
    metaFileSize: '50 B',
    replacingFileId: undefined,
    uploadedBy: 'user1@test.com',
    screenerResult: {
      message: 'message',
      overallResult: 'Passed',
      testResults: [
        {
          notes: 'notes',
          result: 'PASS',
          stage: 'Passed',
          testFunctionName: 'testFunctionName',
        },
      ],
    },
  };

  const testQueuedImportStatus: DataFileImportStatus = {
    status: 'QUEUED',
    percentageComplete: 0,
    stagePercentageComplete: 0,
    totalRows: 0,
  };

  const testImportingImportStatus: DataFileImportStatus = {
    status: 'STAGE_2',
    percentageComplete: 0,
    stagePercentageComplete: 0,
    totalRows: 100,
  };

  const testCompleteImportStatus: DataFileImportStatus = {
    status: 'COMPLETE',
    percentageComplete: 100,
    stagePercentageComplete: 100,
    totalRows: 100,
  };

  const testValidReplacementPlan: DataReplacementPlan = {
    valid: true,
    dataBlocks: [],
    footnotes: [],
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-1',
    apiDataSetVersionPlan: {
      id: '',
      dataSetId: '',
      name: '',
      version: '',
      status: '',
      valid: false,
      readyToPublish: false,
    },
  };

  test('renders uploaded data files table', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testCompleteImportStatus,
    );

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseVersionId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
      'release-1',
    );

    expect(await screen.findByText('Uploaded data files')).toBeInTheDocument();

    const fileTableRows = getAllFileTableRows('Data files');

    expect(fileTableRows).toHaveLength(3);

    const fileTableRow1 = within(fileTableRows[1]);

    expect(fileTableRow1.getByTestId('Title')).toHaveTextContent('Test data 1');
    expect(fileTableRow1.getByTestId('Size')).toHaveTextContent('50 Kb');
    expect(fileTableRow1.getByTestId('Status')).toHaveTextContent('Complete');

    const fileTableRow2 = within(fileTableRows[2]);

    expect(fileTableRow2.getByTestId('Title')).toHaveTextContent('Test data 2');
    expect(fileTableRow2.getByTestId('Size')).toHaveTextContent('100 Kb');
    expect(fileTableRow2.getByTestId('Status')).toHaveTextContent('Complete');
  });

  test('renders data files replacements table', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue([
      { ...testDataFiles[0], replacedBy: 'data-replacement-1' },
      { ...testDataFiles[1], replacedBy: 'data-replacement-2' },
    ]);

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testCompleteImportStatus,
    );

    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testDataFiles[0],
      id: 'data-replacement-1',
    });

    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testDataFiles[1],
      id: 'data-replacement-2',
    });

    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseVersionId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Uploaded data files')).toBeInTheDocument();

    expect(await screen.findByText('Test data 2')).toBeInTheDocument();

    const replacementRows = getAllFileTableRows('Data file replacements');

    expect(replacementRows).toHaveLength(3);

    const replacementRow1 = within(replacementRows[1]);

    expect(replacementRow1.getByTestId('Title')).toHaveTextContent(
      'Test data 1',
    );
    expect(replacementRow1.getByTestId('Size')).toHaveTextContent('50 Kb');
    expect(replacementRow1.getByTestId('Status')).toHaveTextContent('Complete');

    const replacementRow2 = within(replacementRows[2]);

    expect(replacementRow2.getByTestId('Title')).toHaveTextContent(
      'Test data 2',
    );
    expect(replacementRow2.getByTestId('Size')).toHaveTextContent('100 Kb');
    expect(replacementRow2.getByTestId('Status')).toHaveTextContent('Complete');
  });

  test('renders data files and data file replacements tables', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue([
      { ...testDataFiles[0], replacedBy: 'data-replacement-1' },
      testDataFiles[1],
    ]);

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testCompleteImportStatus,
    );

    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testDataFiles[0],
      id: 'data-replacement-1',
    });

    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseVersionId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Uploaded data files')).toBeInTheDocument();
    expect(await screen.findByText('Test data 1')).toBeInTheDocument();

    const replacementRows = getAllFileTableRows('Data file replacements');

    expect(replacementRows).toHaveLength(2);

    const replacementRow1 = within(replacementRows[1]);

    expect(replacementRow1.getByTestId('Title')).toHaveTextContent(
      'Test data 1',
    );
    expect(replacementRow1.getByTestId('Size')).toHaveTextContent('50 Kb');
    expect(replacementRow1.getByTestId('Status')).toHaveTextContent('Complete');

    const fileTableRows = getAllFileTableRows('Data files');

    expect(fileTableRows).toHaveLength(2);

    const fileTableRow2 = within(fileTableRows[1]);

    expect(fileTableRow2.getByTestId('Title')).toHaveTextContent('Test data 2');
    expect(fileTableRow2.getByTestId('Size')).toHaveTextContent('100 Kb');
    expect(fileTableRow2.getByTestId('Status')).toHaveTextContent('Complete');
  });

  test('renders empty message when there are no data files', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue([]);

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseVersionId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    expect(
      await screen.findByText('No data files have been uploaded.'),
    ).toBeInTheDocument();

    expect(screen.queryByText('Uploaded data files')).not.toBeInTheDocument();
  });

  describe('view details modal', () => {
    test('renders details of data file when opened', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([testDataFiles[1]]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testCompleteImportStatus,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
        'release-1',
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const dataFileRows = getAllFileTableRows('Data files');

      expect(dataFileRows).toHaveLength(2);

      const dataFileRow = within(dataFileRows[1]);

      await user.click(
        dataFileRow.getByRole('button', { name: 'View details' }),
      );

      expect(await screen.findByText('Data file details')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByTestId('Title')).toHaveTextContent('Test data 2');

      expect(
        within(modal.getByTestId('Data file')).getByRole('button'),
      ).toHaveTextContent('data-2.csv');
      expect(
        within(modal.getByTestId('Meta file')).getByRole('button'),
      ).toHaveTextContent('data-2.meta.csv');

      expect(modal.getByTestId('Size')).toHaveTextContent('100 Kb');
      expect(modal.getByTestId('Number of rows')).toHaveTextContent('200');
      expect(modal.getByTestId('Status')).toHaveTextContent('Complete');

      expect(
        within(modal.getByTestId('Uploaded by')).getByRole('link', {
          name: 'user2@test.com',
        }),
      ).toHaveAttribute('href', 'mailto:user2@test.com');

      expect(modal.getByTestId('Date uploaded')).toHaveTextContent(
        '1 July 2020, 12:00',
      );
    });
  });

  describe('deleting data file', () => {
    test('does not render delete files button if file is not ready for deletion', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        { ...testDataFiles[0], status: 'QUEUED' },
        testDataFiles[1],
      ]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(3);

      const fileTableRow1 = within(fileTableRows[1]);

      expect(fileTableRow1.getByTestId('Status')).toHaveTextContent('Queued');
      expect(
        fileTableRow1.queryByRole('button', { name: 'Delete files' }),
      ).not.toBeInTheDocument();

      const fileTableRow2 = within(fileTableRows[2]);

      expect(fileTableRow2.getByTestId('Status')).toHaveTextContent('Complete');
      expect(
        fileTableRow2.getByRole('button', { name: 'Delete files' }),
      ).toBeInTheDocument();
    });

    test('clicking delete files button shows modal to confirm deletion plan', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testCompleteImportStatus,
      );
      releaseDataFileService.getDeleteDataFilePlan.mockResolvedValue({
        deleteDataBlockPlan: {
          dependentDataBlocks: [
            {
              contentSectionHeading: 'Test section 1',
              name: 'Test data block 1',
              infographicFilesInfo: [],
              isKeyStatistic: true,
            },
            {
              contentSectionHeading: 'Test section 2',
              name: 'Test data block 2',
              infographicFilesInfo: [],
              isKeyStatistic: false,
              featuredTable: {
                name: 'Test data block 2 featured table',
                description: 'Test data block 2 featured table description',
              },
            },
          ],
        },
        footnoteIds: ['footnote-1'],
      });

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(3);

      expect(
        within(fileTableRows[2]).getByRole('button', { name: 'Delete files' }),
      ).toBeInTheDocument();

      await user.click(
        within(fileTableRows[2]).getByRole('button', { name: 'Delete files' }),
      );

      expect(
        await screen.findByText('Confirm deletion of selected data files'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText('Confirm deletion of selected data files'),
      ).toBeInTheDocument();

      expect(
        modal.getByText('The following data blocks will also be deleted:'),
      ).toBeInTheDocument();

      const dataBlocks = modal.getAllByRole('listitem');

      expect(dataBlocks[0]).toHaveTextContent('Test data block 1');
      expect(dataBlocks[0]).toHaveTextContent(
        'It will also be removed from the "Test section 1" content section.',
      );
      expect(dataBlocks[0]).toHaveTextContent(
        'A key statistic associated with this data block will be removed.',
      );
      expect(dataBlocks[0]).not.toHaveTextContent('The featured table ');

      expect(dataBlocks[1]).toHaveTextContent('Test data block 2');
      expect(dataBlocks[1]).toHaveTextContent(
        'It will also be removed from the "Test section 2" content section.',
      );
      expect(dataBlocks[1]).not.toHaveTextContent(
        'A key statistic associated with this data block will be removed.',
      );
      expect(dataBlocks[1]).toHaveTextContent(
        'The featured table "Test data block 2 featured table" using this data block will be removed.',
      );

      expect(
        modal.getByText('1 footnote will be removed or updated.'),
      ).toBeInTheDocument();
    });

    test('confirming deletion removes the data file section', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testCompleteImportStatus,
      );
      releaseDataFileService.getDeleteDataFilePlan.mockResolvedValue({
        deleteDataBlockPlan: {
          dependentDataBlocks: [],
        },
        footnoteIds: [],
      });
      releaseDataFileService.deleteDataFiles.mockResolvedValue();

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      let fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(3);

      expect(
        within(fileTableRows[2]).getByRole('button', { name: 'Delete files' }),
      ).toBeInTheDocument();

      await user.click(
        within(fileTableRows[2]).getByRole('button', { name: 'Delete files' }),
      );

      expect(
        await screen.findByText('Confirm deletion of selected data files'),
      ).toBeInTheDocument();

      await user.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      expect(within(fileTableRows[1]).getByTestId('Title')).toHaveTextContent(
        'Test data 1',
      );
    });

    test('does not allow deleting files when linked to an API data set', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        { ...testDataFiles[0], publicApiDataSetId: 'test-data-set-id' },
      ]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testCompleteImportStatus,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      await user.click(
        within(fileTableRows[1]).getByRole('button', { name: 'Delete files' }),
      );

      expect(
        await screen.findByText('Cannot delete files'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText(
          'This data file has an API data set linked to it. Please remove the API data set before deleting.',
        ),
      ).toBeInTheDocument();

      expect(
        modal.getByRole('link', { name: 'Go to API data set' }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-1/release/release-1/api-data-sets/test-data-set-id',
      );
    });
  });

  describe('replace data file', () => {
    test('does not render replace data button if file import is not completed', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        { ...testDataFiles[0], status: 'QUEUED' },
        testDataFiles[1],
      ]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(3);

      const fileTableRow1 = within(fileTableRows[1]);

      expect(fileTableRow1.getByTestId('Status')).toHaveTextContent('Queued');
      expect(
        fileTableRow1.queryByRole('link', { name: 'Replace data' }),
      ).not.toBeInTheDocument();

      const fileTableRow2 = within(fileTableRows[2]);

      expect(fileTableRow2.getByTestId('Status')).toHaveTextContent('Complete');
      expect(
        fileTableRow2.getByRole('link', { name: 'Replace data' }),
      ).toBeInTheDocument();
    });

    test('renders replace data button with correct link', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        { ...testDataFiles[0], status: 'QUEUED' },
        testDataFiles[1],
      ]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(3);

      const fileTableRow2 = within(fileTableRows[2]);
      expect(
        fileTableRow2.getByRole('link', { name: 'Replace data' }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-1/release/release-1/data/data-2/replace',
      );
    });

    test('does not allow replacing data when linked to an API data set', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        { ...testDataFiles[0], publicApiDataSetId: 'test-data-set-id' },
      ]);
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testCompleteImportStatus,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      await user.click(
        within(fileTableRows[1]).getByRole('button', { name: 'Replace data' }),
      );

      expect(
        await screen.findByText('Cannot replace data'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText(
          'This data file has an API data set linked to it. Please remove the API data set before replacing the data.',
        ),
      ).toBeInTheDocument();

      expect(
        modal.getByRole('link', { name: 'Go to API data set' }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-1/release/release-1/api-data-sets/test-data-set-id',
      );
    });
  });

  describe('uploading data file', () => {
    beforeEach(() => {
      releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
    });

    test('show validation message when no subject title', async () => {
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      expect(
        await screen.findByText('Enter a title', {
          selector: '#dataFileUploadForm-title-error',
        }),
      ).toBeInTheDocument();
    });

    test('shows validation message when non-unique subject title', async () => {
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await user.type(screen.getByLabelText('Title'), 'Test data 1');

      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await user.click(screen.getByLabelText('Title'));

      expect(
        await screen.findByText('Enter a unique title', {
          selector: '#dataFileUploadForm-title-error',
        }),
      ).toBeInTheDocument();
    });

    test('cannot submit with invalid values when trying to upload CSV files', async () => {
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        releaseDataFileService.uploadDataSetFilePair,
      ).not.toHaveBeenCalled();

      expect(
        screen.getByText('Enter a title', {
          selector: '#dataFileUploadForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByText('Choose a metadata file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).not.toBeInTheDocument();
    });

    test('cannot submit with invalid values when trying to upload ZIP file', async () => {
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await user.click(screen.getByLabelText('ZIP file'));
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        releaseDataFileService.uploadZippedDataSetFilePair,
      ).not.toHaveBeenCalled();

      expect(
        screen.getByText('Enter a title', {
          selector: '#dataFileUploadForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('Choose a data file that is not empty', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByText('Choose a metadata file that is not empty', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });

    test('cannot submit with invalid values when trying to upload bulk ZIP file', async () => {
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await user.click(screen.getByLabelText('Bulk ZIP upload'));
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        releaseDataFileService.uploadBulkZipDataSetFile,
      ).not.toHaveBeenCalled();

      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-bulkZipFile-error',
        }),
      ).toBeInTheDocument();
    });

    test('successful import with CSV files refetches data files', async () => {
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const dataFile = new File(['test'], 'test-data.csv', {
        type: 'text/csv',
      });
      const metadataFile = new File(['test'], 'test-data.meta.csv', {
        type: 'text/csv',
      });

      await user.type(screen.getByLabelText('Title'), 'Test title');

      await user.upload(screen.getByLabelText('Upload data file'), dataFile);
      await user.upload(
        screen.getByLabelText('Upload metadata file'),
        metadataFile,
      );
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await waitFor(() => {
        expect(
          releaseDataFileService.uploadDataSetFilePair,
        ).toHaveBeenCalledWith('release-1', {
          title: 'Test title',
          dataFile,
          metadataFile,
        } as UploadDataFilesRequest);

        expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
          'release-1',
        );
      });
    });

    test('successful import with zip file refetches data files', async () => {
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const zipFile = new File(['test'], 'test-data.zip', {
        type: 'application/zip',
      });

      await user.type(screen.getByLabelText('Title'), 'Test zip title');

      await user.click(screen.getByLabelText('ZIP file'));

      await user.upload(screen.getByLabelText('Upload ZIP file'), zipFile);
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await waitFor(() => {
        expect(
          releaseDataFileService.uploadZippedDataSetFilePair,
        ).toHaveBeenCalledWith('release-1', {
          title: 'Test zip title',
          zipFile,
        } as UploadZipDataFileRequest);

        expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
          'release-1',
        );
      });
    });

    test('successful import with bulk zip file refetches data files', async () => {
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const zipFile = new File(['test'], 'test-data.zip', {
        type: 'application/zip',
      });

      await user.click(screen.getByLabelText('Bulk ZIP upload'));

      await user.upload(screen.getByLabelText('Upload bulk ZIP file'), zipFile);
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
          'release-1',
        );
      });
    });

    test('updates the file size after importing CSV file when status changes', async () => {
      // we don't display rows :/
      releaseDataFileService.getDataFileImportStatus
        .mockResolvedValue(testQueuedImportStatus)
        .mockResolvedValueOnce(testImportingImportStatus);

      const testUploadedDataFile2 = {
        ...testImportedDataFile,
        title: 'Test title 2',
      };

      releaseDataFileService.getDataFiles.mockResolvedValue([
        ...testDataFiles,
        testUploadedDataFile2,
      ]);

      permissionService.getDataFilePermissions.mockResolvedValue(
        {} as DataFilePermissions,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const dataFile = new File(['test'], 'test-data.csv', {
        type: 'text/csv',
      });
      const metadataFile = new File(['test'], 'test-data.meta.csv', {
        type: 'text/csv',
      });

      await user.type(screen.getByLabelText('Title'), 'Test title');

      await user.upload(screen.getByLabelText('Upload data file'), dataFile);
      await user.upload(
        screen.getByLabelText('Upload metadata file'),
        metadataFile,
      );
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await waitFor(() => {
        expect(
          releaseDataFileService.uploadDataSetFilePair,
        ).toHaveBeenCalledWith('release-1', {
          title: 'Test title',
          dataFile,
          metadataFile,
        } as UploadDataFilesRequest);

        expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
          'release-1',
        );
      });

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(4);

      const fileTableRow3 = within(fileTableRows[3]);

      await waitFor(() =>
        expect(
          releaseDataFileService.getDataFileImportStatus,
        ).toHaveBeenCalledWith('release-1', 'file-1'),
      );

      expect(fileTableRow3.getByTestId('Size')).toHaveTextContent('150 Kb');
    });

    test('updates the file size after importing ZIP file when status changes', async () => {
      releaseDataFileService.getDataFileImportStatus
        .mockResolvedValue(testQueuedImportStatus)
        .mockResolvedValueOnce(testImportingImportStatus);

      const testUploadedDataFile2 = {
        ...testImportedDataFile,
        title: 'Test title 2',
      };

      releaseDataFileService.getDataFiles.mockResolvedValue([
        ...testDataFiles,
        testUploadedDataFile2,
      ]);

      permissionService.getDataFilePermissions.mockResolvedValue(
        {} as DataFilePermissions,
      );

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const zipFile = new File(['test'], 'test-data.zip', {
        type: 'application/zip',
      });

      await user.type(screen.getByLabelText('Title'), 'Test title');

      await user.click(screen.getByLabelText('ZIP file'));

      await user.upload(screen.getByLabelText('Upload ZIP file'), zipFile);
      await user.click(
        screen.getByRole('button', { name: 'Upload data files' }),
      );

      await waitFor(() =>
        expect(
          releaseDataFileService.uploadZippedDataSetFilePair,
        ).toHaveBeenCalledWith('release-1', {
          title: 'Test title',
          zipFile,
        } as UploadZipDataFileRequest),
      );

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(4);

      const fileTableRow3 = within(fileTableRows[3]);

      await waitFor(() =>
        expect(
          releaseDataFileService.getDataFileImportStatus,
        ).toHaveBeenCalledWith('release-1', 'file-1'),
      );

      expect(fileTableRow3.getByTestId('Size')).toHaveTextContent('150 Kb');
    });

    describe('permissions during upload', () => {
      test('cancel button is available when permissions allow it ', async () => {
        releaseDataFileService.getDataFiles.mockResolvedValue([
          {
            ...testImportedDataFile,
            permissions: {
              canCancelImport: true,
            },
          },
        ]);
        releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
          testQueuedImportStatus,
        );

        render(
          <MemoryRouter>
            <ReleaseDataUploadsSection
              publicationId="publication-1"
              releaseVersionId="release-1"
              canUpdateRelease
            />
          </MemoryRouter>,
        );

        expect(
          await screen.findByText('Uploaded data files'),
        ).toBeInTheDocument();

        const fileTableRows = getAllFileTableRows('Data files');

        expect(fileTableRows).toHaveLength(2);

        const fileTableRow = within(fileTableRows[1]);

        expect(
          fileTableRow.getByRole('button', { name: 'Cancel' }),
        ).toBeInTheDocument();
      });

      test('cancel button is not available when permissions do not allow it', async () => {
        releaseDataFileService.getDataFiles.mockResolvedValue([
          {
            ...testImportedDataFile,
            permissions: {
              canCancelImport: false,
            },
          },
        ]);
        releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
          testQueuedImportStatus,
        );

        render(
          <MemoryRouter>
            <ReleaseDataUploadsSection
              publicationId="publication-1"
              releaseVersionId="release-1"
              canUpdateRelease
            />
          </MemoryRouter>,
        );

        expect(
          await screen.findByText('Uploaded data files'),
        ).toBeInTheDocument();

        const fileTableRows = getAllFileTableRows('Data files');

        expect(fileTableRows).toHaveLength(2);

        expect(
          within(fileTableRows[1]).queryByRole('button', { name: 'Cancel' }),
        ).not.toBeInTheDocument();
      });
    });
  });

  describe('cancelling file import', () => {
    beforeEach(() => {
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );
    });

    test('clicking cancel presents a cancellation modal', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testImportedDataFile,
      ]);

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      expect(screen.getAllByRole('row')).toHaveLength(2);

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      const fileTableRow = within(fileTableRows[1]);

      await user.click(fileTableRow.getByRole('button', { name: 'Cancel' }));

      expect(
        await screen.findByText('Confirm cancellation of selected data file'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText('Confirm cancellation of selected data file'),
      ).toBeInTheDocument();

      expect(
        modal.getByText(
          'This file upload will be cancelled and may then be removed.',
        ),
      ).toBeInTheDocument();

      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();

      expect(
        modal.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    test('confirming the cancellation modal initiates cancellation and removes the Cancel button', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testImportedDataFile,
      ]);

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      const fileTableRow = within(fileTableRows[1]);

      await user.click(fileTableRow.getByRole('button', { name: 'Cancel' }));

      expect(
        await screen.findByText('Confirm cancellation of selected data file'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      await user.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).toHaveBeenCalledWith(
          'release-1',
          testImportedDataFile.id,
        );
      });

      expect(
        screen.queryByText('Confirm cancellation of selected data file'),
      ).not.toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(
        fileTableRow.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();
    });

    test('cancelling the cancellation modal calls off the import cancellation', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testImportedDataFile,
      ]);

      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      const fileTableRow = within(fileTableRows[1]);

      await user.click(fileTableRow.getByRole('button', { name: 'Cancel' }));

      expect(
        await screen.findByText('Confirm cancellation of selected data file'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      await user.click(modal.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).not.toHaveBeenCalled();
      });

      expect(
        screen.queryByText('Confirm cancellation of selected data file'),
      ).not.toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(
        fileTableRow.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('show error message and close modal if cancellation fails', async () => {
      releaseDataFileService.cancelImport.mockRejectedValue(
        new Error('oh no!'),
      );
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testImportedDataFile,
      ]);
      const { user } = render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseVersionId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText('Uploaded data files'),
      ).toBeInTheDocument();

      const fileTableRows = getAllFileTableRows('Data files');

      expect(fileTableRows).toHaveLength(2);

      const fileTableRow = within(fileTableRows[1]);

      await user.click(fileTableRow.getByRole('button', { name: 'Cancel' }));

      expect(
        await screen.findByText('Confirm cancellation of selected data file'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).toHaveBeenCalledWith(
          'release-1',
          testImportedDataFile.id,
        );
      });

      expect(
        screen.queryByText('Confirm cancellation of selected data file'),
      ).not.toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      expect(
        fileTableRow.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();

      expect(screen.getByText('Cancellation failed')).toBeInTheDocument();
    });
  });

  function getAllFileTableRows(caption: string) {
    const table = screen.getByRole('table', { name: caption });

    return within(table).getAllByRole('row');
  }
});
