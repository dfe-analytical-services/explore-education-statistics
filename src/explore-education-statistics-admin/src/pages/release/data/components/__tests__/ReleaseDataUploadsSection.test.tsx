import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import _releaseDataFileService, {
  DataFileImportStatus,
  DataFile,
  UploadDataFilesRequest,
  UploadZipDataFileRequest,
} from '@admin/services/releaseDataFileService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';
import _permissionService, {
  DataFilePermissions,
} from '@admin/services/permissionService';

jest.mock('@admin/services/releaseDataFileService');
jest.mock('@admin/services/permissionService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

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

  const testUploadedDataFile: DataFile = {
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

  const testUploadedZipFile: DataFile = {
    id: 'zip-file-1',
    title: 'Test zip title',
    userName: 'user1@test.com',
    fileName: 'test-data.zip',
    metaFileId: 'file-1-meta',
    metaFileName: 'test-data.meta.zip',
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

  test('renders list of uploaded data files', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testCompleteImportStatus,
    );

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
        'release-1',
      );

      expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
    });

    const sections = screen.getAllByTestId('accordionSection');
    const section1 = within(sections[0]);

    expect(
      section1.getByRole('button', { name: 'Test data 1' }),
    ).toBeInTheDocument();

    expect(section1.getByTestId('Subject title')).toHaveTextContent(
      'Test data 1',
    );
    expect(section1.getByTestId('Data file')).toHaveTextContent('data-1.csv');
    expect(section1.getByTestId('Metadata file')).toHaveTextContent(
      'data-1.meta.csv',
    );
    expect(section1.getByTestId('Data file size')).toHaveTextContent('50 Kb');
    expect(section1.getByTestId('Number of rows')).toHaveTextContent('100');
    expect(section1.getByTestId('Status')).toHaveTextContent('Complete');
    expect(section1.getByTestId('Uploaded by')).toHaveTextContent(
      'user1@test.com',
    );
    expect(section1.getByTestId('Date uploaded')).toHaveTextContent(
      '12 June 2020 12:00',
    );

    const section2 = within(sections[1]);

    expect(
      section2.getByRole('button', { name: 'Test data 2' }),
    ).toBeInTheDocument();

    expect(section2.getByTestId('Subject title')).toHaveTextContent(
      'Test data 2',
    );

    expect(section2.getByTestId('Data file')).toHaveTextContent('data-2.csv');
    expect(section2.getByTestId('Metadata file')).toHaveTextContent(
      'data-2.meta.csv',
    );
    expect(section2.getByTestId('Data file size')).toHaveTextContent('100 Kb');
    expect(section2.getByTestId('Number of rows')).toHaveTextContent('200');
    expect(section2.getByTestId('Status')).toHaveTextContent('Complete');
    expect(section2.getByTestId('Uploaded by')).toHaveTextContent(
      'user2@test.com',
    );
    expect(section2.getByTestId('Date uploaded')).toHaveTextContent(
      '1 July 2020 12:00',
    );
  });

  test("renders data file details with status of 'Replacement in progress' if being replaced", async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue([
      {
        ...testDataFiles[0],
        replacedBy: 'data-replacement-1',
      },
    ]);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testCompleteImportStatus,
    );

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(releaseDataFileService.getDataFiles).toHaveBeenCalledWith(
        'release-1',
      );

      expect(screen.getAllByTestId('accordionSection')).toHaveLength(1);
    });

    const section1 = getAccordionSection(0);

    expect(
      section1.getByRole('button', { name: 'Test data 1' }),
    ).toBeInTheDocument();

    expect(section1.getByTestId('Status')).toHaveTextContent(
      'Data replacement in progress',
    );
  });

  test('renders empty message when there are no data files', async () => {
    releaseDataFileService.getDataFiles.mockResolvedValue([]);

    render(
      <MemoryRouter>
        <ReleaseDataUploadsSection
          publicationId="publication-1"
          releaseId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.queryAllByTestId('accordionSection')).toHaveLength(0);
    });

    expect(
      screen.getByText('No data files have been uploaded.'),
    ).toBeInTheDocument();
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
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');

      const section1 = within(sections[0]);
      expect(section1.getByTestId('Status')).toHaveTextContent('Queued');
      expect(
        section1.queryByRole('button', { name: 'Delete files' }),
      ).not.toBeInTheDocument();

      const section2 = within(sections[1]);
      expect(section2.getByTestId('Status')).toHaveTextContent('Complete');
      expect(
        section2.getByRole('button', { name: 'Delete files' }),
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
            },
          ],
        },
        footnoteIds: ['footnote-1'],
      });

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');

      expect(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm deletion of selected data files'),
        ).toBeInTheDocument();
      });

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

      expect(dataBlocks[1]).toHaveTextContent('Test data block 2');
      expect(dataBlocks[1]).toHaveTextContent(
        'It will also be removed from the "Test section 2" content section.',
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

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');

      expect(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm deletion of selected data files'),
        ).toBeInTheDocument();
      });

      userEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1);
      });

      expect(
        within(screen.getByTestId('accordionSection')).getByRole('button', {
          name: 'Test data 1',
        }),
      ).toBeInTheDocument();
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
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');
      expect(sections).toHaveLength(2);

      const section1 = within(sections[0]);
      expect(section1.getByTestId('Status')).toHaveTextContent('Queued');
      expect(
        section1.queryByRole('link', { name: 'Replace data' }),
      ).not.toBeInTheDocument();

      const section2 = within(sections[1]);
      expect(section2.getByTestId('Status')).toHaveTextContent('Complete');
      expect(
        section2.getByRole('link', { name: 'Replace data' }),
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
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');

      const section2 = within(sections[1]);
      expect(
        section2.getByRole('link', { name: 'Replace data' }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-1/release/release-1/data/data-2/replace',
      );
    });
  });

  describe('uploading data file', () => {
    beforeEach(() => {
      releaseDataFileService.getDataFiles.mockResolvedValue(testDataFiles);
    });

    test('show validation message when no subject title', async () => {
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      userEvent.click(screen.getByLabelText('Subject title'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a subject title', {
            selector: '#dataFileUploadForm-subjectTitle-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when non-unique subject title', async () => {
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await userEvent.type(
        screen.getByLabelText('Subject title'),
        'Test data 1',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a unique subject title', {
            selector: '#dataFileUploadForm-subjectTitle-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('cannot submit with invalid values when trying to upload CSV files', async () => {
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.uploadDataFiles).not.toHaveBeenCalled();

        expect(
          screen.getByText('Enter a subject title', {
            selector: '#dataFileUploadForm-subjectTitle-error',
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
          screen.queryByText('Choose a zip file', {
            selector: '#dataFileUploadForm-metadataFile-error',
          }),
        ).not.toBeInTheDocument();
      });
    });

    test('cannot submit with invalid values when trying to upload ZIP file', async () => {
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      userEvent.click(screen.getByLabelText('ZIP file'));
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.uploadDataFiles).not.toHaveBeenCalled();

        expect(
          screen.getByText('Enter a subject title', {
            selector: '#dataFileUploadForm-subjectTitle-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByText('Choose a data file', {
            selector: '#dataFileUploadForm-dataFile-error',
          }),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText('Choose a metadata file', {
            selector: '#dataFileUploadForm-metadataFile-error',
          }),
        ).not.toBeInTheDocument();

        expect(
          screen.getByText('Choose a zip file', {
            selector: '#dataFileUploadForm-zipFile-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('successful submit with CSV files renders with uploaded data file appended to list', async () => {
      releaseDataFileService.uploadDataFiles.mockResolvedValue(
        testUploadedDataFile,
      );
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
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

      await userEvent.type(
        screen.getByLabelText('Subject title'),
        'Test title',
      );

      userEvent.upload(screen.getByLabelText('Upload data file'), dataFile);
      userEvent.upload(
        screen.getByLabelText('Upload metadata file'),
        metadataFile,
      );
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.uploadDataFiles).toHaveBeenCalledWith(
          'release-1',
          {
            title: 'Test title',
            dataFile,
            metadataFile,
          } as UploadDataFilesRequest,
        );

        expect(screen.getAllByTestId('accordionSection')).toHaveLength(3);
      });

      const sections = screen.getAllByTestId('accordionSection');

      const section1 = within(sections[0]);

      expect(
        section1.getByRole('button', { name: 'Test data 1' }),
      ).toBeInTheDocument();

      const section2 = within(sections[1]);

      expect(
        section2.getByRole('button', { name: 'Test data 2' }),
      ).toBeInTheDocument();

      expect(section2.getByTestId('Subject title')).toHaveTextContent(
        'Test data 2',
      );

      const section3 = within(sections[2]);

      expect(
        section3.getByRole('button', { name: 'Test title' }),
      ).toBeInTheDocument();

      expect(section3.getByTestId('Subject title')).toHaveTextContent(
        'Test title',
      );

      expect(section3.getByTestId('Data file')).toHaveTextContent(
        'test-data.csv',
      );
      expect(section3.getByTestId('Metadata file')).toHaveTextContent(
        'test-data.meta.csv',
      );
      expect(section3.getByTestId('Data file size')).toHaveTextContent(
        '150 Kb',
      );
      expect(section3.getByTestId('Number of rows')).toHaveTextContent(
        'Unknown',
      );
      expect(section3.getByTestId('Status')).toHaveTextContent('Queued');
      expect(section3.getByTestId('Uploaded by')).toHaveTextContent(
        'user1@test.com',
      );
      expect(section3.getByTestId('Date uploaded')).toHaveTextContent(
        '18 August 2020 12:00',
      );
    });

    test('successful submit with ZIP file renders with uploaded data file appended to list', async () => {
      releaseDataFileService.uploadZipDataFile.mockResolvedValue({
        ...testUploadedZipFile,
      });
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const zipFile = new File(['test'], 'test-data.zip', {
        type: 'application/zip',
      });

      await userEvent.type(
        screen.getByLabelText('Subject title'),
        'Test zip title',
      );

      userEvent.click(screen.getByLabelText('ZIP file'));

      userEvent.upload(screen.getByLabelText('Upload ZIP file'), zipFile);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.uploadZipDataFile).toHaveBeenCalledWith(
          'release-1',
          {
            title: 'Test zip title',
            zipFile,
          } as UploadZipDataFileRequest,
        );
      });

      const sections = screen.getAllByTestId('accordionSection');
      const section1 = within(sections[0]);
      const section2 = within(sections[1]);
      const section3 = within(sections[2]);

      expect(sections).toHaveLength(3);

      expect(
        section1.getByRole('button', { name: 'Test data 1' }),
      ).toBeInTheDocument();

      expect(
        section2.getByRole('button', { name: 'Test data 2' }),
      ).toBeInTheDocument();

      expect(section2.getByTestId('Subject title')).toHaveTextContent(
        'Test data 2',
      );

      expect(
        section3.getByRole('button', { name: 'Test zip title' }),
      ).toBeInTheDocument();

      expect(section3.getByTestId('Subject title')).toHaveTextContent(
        'Test zip title',
      );

      expect(section3.getByTestId('Data file')).toHaveTextContent(
        'test-data.zip',
      );
      expect(section3.getByTestId('Metadata file')).toHaveTextContent(
        'test-data.meta.zip',
      );
      expect(section3.getByTestId('Data file size')).toHaveTextContent(
        '150 Kb',
      );
      expect(section3.getByTestId('Number of rows')).toHaveTextContent(
        'Unknown',
      );
      expect(section3.getByTestId('Status')).toHaveTextContent('Queued');
      expect(section3.getByTestId('Uploaded by')).toHaveTextContent(
        'user1@test.com',
      );
      expect(section3.getByTestId('Date uploaded')).toHaveTextContent(
        '18 August 2020 12:00',
      );
    });

    test('updates the number of rows after uploading CSV file when status changes', async () => {
      releaseDataFileService.uploadDataFiles.mockResolvedValue(
        testUploadedDataFile,
      );
      releaseDataFileService.getDataFileImportStatus
        .mockResolvedValue(testQueuedImportStatus)
        .mockResolvedValueOnce(testImportingImportStatus);

      permissionService.getDataFilePermissions.mockResolvedValue(
        {} as DataFilePermissions,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
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

      await userEvent.type(
        screen.getByLabelText('Subject title'),
        'Test title',
      );

      userEvent.upload(screen.getByLabelText('Upload data file'), dataFile);
      userEvent.upload(
        screen.getByLabelText('Upload metadata file'),
        metadataFile,
      );
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() => {
        expect(releaseDataFileService.uploadDataFiles).toHaveBeenCalledWith(
          'release-1',
          {
            title: 'Test title',
            dataFile,
            metadataFile,
          } as UploadDataFilesRequest,
        );
      });

      const sections = screen.getAllByTestId('accordionSection');
      const section3 = within(sections[2]);

      await waitFor(() =>
        expect(
          releaseDataFileService.getDataFileImportStatus,
        ).toHaveBeenCalledWith('release-1', testUploadedDataFile),
      );
      await waitFor(() => {
        expect(section3.getByTestId('Number of rows')).toHaveTextContent('100');
      });
    });

    test('updates the number of rows after uploading ZIP file when status changes', async () => {
      releaseDataFileService.uploadZipDataFile.mockResolvedValue({
        ...testUploadedZipFile,
      });

      releaseDataFileService.getDataFileImportStatus
        .mockResolvedValue(testQueuedImportStatus)
        .mockResolvedValueOnce(testImportingImportStatus);

      permissionService.getDataFilePermissions.mockResolvedValue(
        {} as DataFilePermissions,
      );

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      const zipFile = new File(['test'], 'test-data.zip', {
        type: 'application/zip',
      });

      await userEvent.type(
        screen.getByLabelText('Subject title'),
        'Test title',
      );

      userEvent.click(screen.getByLabelText('ZIP file'));

      userEvent.upload(screen.getByLabelText('Upload ZIP file'), zipFile);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload data files',
        }),
      );

      await waitFor(() =>
        expect(releaseDataFileService.uploadZipDataFile).toHaveBeenCalledWith(
          'release-1',
          {
            title: 'Test title',
            zipFile,
          } as UploadZipDataFileRequest,
        ),
      );

      const sections = screen.getAllByTestId('accordionSection');
      const section3 = within(sections[2]);

      await waitFor(() =>
        expect(
          releaseDataFileService.getDataFileImportStatus,
        ).toHaveBeenCalledWith('release-1', testUploadedZipFile),
      );
      await waitFor(() => {
        expect(section3.getByTestId('Number of rows')).toHaveTextContent('100');
      });
    });

    describe('permissions during upload', () => {
      test('cancel button is available when permissions allow it ', async () => {
        releaseDataFileService.getDataFiles.mockResolvedValue([
          {
            ...testUploadedDataFile,
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
              releaseId="release-1"
              canUpdateRelease
            />
          </MemoryRouter>,
        );

        await waitFor(() =>
          expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
        );

        const section = getAccordionSection(0);

        expect(
          section.getByRole('button', { name: 'Cancel' }),
        ).toBeInTheDocument();
      });

      test('cancel button is not available when permissions do not allow it', async () => {
        releaseDataFileService.getDataFiles.mockResolvedValue([
          {
            ...testUploadedDataFile,
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
              releaseId="release-1"
              canUpdateRelease
            />
          </MemoryRouter>,
        );

        await waitFor(() =>
          expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
        );

        const section = getAccordionSection(0);

        expect(
          section.queryByRole('button', { name: 'Cancel' }),
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
        testUploadedDataFile,
      ]);

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() =>
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
      );

      const section = getAccordionSection(0);

      userEvent.click(section.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm cancellation of selected data file'),
        ).toBeInTheDocument();
      });

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
        testUploadedDataFile,
      ]);

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() =>
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
      );

      const section = getAccordionSection(0);

      userEvent.click(section.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm cancellation of selected data file'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).toHaveBeenCalledWith(
          'release-1',
          testUploadedDataFile.id,
        );
      });

      await waitFor(() => {
        expect(
          screen.queryByText('Confirm cancellation of selected data file'),
        ).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(
        section.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();
    });

    test('cancelling the cancellation modal calls off the import cancellation', async () => {
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testUploadedDataFile,
      ]);

      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() =>
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
      );

      const section = getAccordionSection(0);

      userEvent.click(section.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm cancellation of selected data file'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).not.toHaveBeenCalled();

        expect(
          screen.queryByText('Confirm cancellation of selected data file'),
        ).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(
        section.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('show error message and close modal if cancellation fails', async () => {
      releaseDataFileService.cancelImport.mockRejectedValue(
        new Error('oh no!'),
      );
      releaseDataFileService.getDataFiles.mockResolvedValue([
        testUploadedDataFile,
      ]);
      render(
        <MemoryRouter>
          <ReleaseDataUploadsSection
            publicationId="publication-1"
            releaseId="release-1"
            canUpdateRelease
          />
        </MemoryRouter>,
      );

      await waitFor(() =>
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1),
      );

      const section = getAccordionSection(0);

      userEvent.click(section.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm cancellation of selected data file'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(releaseDataFileService.cancelImport).toHaveBeenCalledWith(
          'release-1',
          testUploadedDataFile.id,
        );
      });

      await waitFor(() => {
        expect(
          screen.queryByText('Confirm cancellation of selected data file'),
        ).not.toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      expect(
        section.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();

      expect(screen.getByText('Cancellation failed')).toBeInTheDocument();
    });
  });

  function getAccordionSection(index: number) {
    return within(screen.getAllByTestId('accordionSection')[index]);
  }
});
