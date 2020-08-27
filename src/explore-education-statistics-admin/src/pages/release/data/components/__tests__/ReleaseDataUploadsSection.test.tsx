import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import _releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  UploadDataFilesRequest,
} from '@admin/services/releaseDataFileService';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ReleaseDataUploadsSection', () => {
  const testDataFiles: DataFile[] = [
    {
      title: 'Test data 1',
      userName: 'user1@test.com',
      filename: 'data-1.csv',
      metadataFilename: 'data-1.meta.csv',
      rows: 100,
      fileSize: {
        size: 50,
        unit: 'Kb',
      },
      created: new Date('2020-06-12T12:00:00'),
    },
    {
      title: 'Test data 2',
      userName: 'user2@test.com',
      filename: 'data-2.csv',
      metadataFilename: 'data-2.meta.csv',
      rows: 200,
      fileSize: {
        size: 100,
        unit: 'Kb',
      },
      created: new Date('2020-07-01T12:00:00'),
    },
  ];

  const testQueuedImportStatus: DataFileImportStatus = {
    status: 'QUEUED',
    numberOfRows: 0,
    percentageComplete: '0%',
  };

  const testCompleteImportStatus: DataFileImportStatus = {
    status: 'COMPLETE',
    numberOfRows: 100,
    percentageComplete: '100%',
  };

  test('renders list of uploaded data files', async () => {
    releaseDataFileService.getReleaseDataFiles.mockResolvedValue(testDataFiles);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
      testQueuedImportStatus,
    );

    render(
      <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
    );

    await waitFor(() => {
      expect(releaseDataFileService.getReleaseDataFiles).toHaveBeenCalledWith(
        'release-1',
      );

      const sections = screen.getAllByTestId('accordionSection');

      expect(sections).toHaveLength(2);

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
      expect(section1.getByTestId('Status')).toHaveTextContent('Queued');
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
      expect(section2.getByTestId('Data file size')).toHaveTextContent(
        '100 Kb',
      );
      expect(section2.getByTestId('Number of rows')).toHaveTextContent('200');
      expect(section2.getByTestId('Status')).toHaveTextContent('Queued');
      expect(section2.getByTestId('Uploaded by')).toHaveTextContent(
        'user2@test.com',
      );
      expect(section2.getByTestId('Date uploaded')).toHaveTextContent(
        '1 July 2020 12:00',
      );
    });
  });

  test('renders empty message when there are no data files', async () => {
    releaseDataFileService.getReleaseDataFiles.mockResolvedValue([]);

    render(
      <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
    );

    await waitFor(() => {
      const sections = screen.queryAllByTestId('accordionSection');

      expect(sections).toHaveLength(0);
      expect(
        screen.getByText('No data files have been uploaded.'),
      ).toBeInTheDocument();
    });
  });

  describe('deleting data file', () => {
    test('does not render delete files button if file is not ready for deletion', async () => {
      releaseDataFileService.getReleaseDataFiles.mockResolvedValue(
        testDataFiles,
      );
      releaseDataFileService.getDataFileImportStatus.mockImplementation(
        async (releaseId, dataFileName) => {
          if (dataFileName === testDataFiles[1].filename) {
            return testCompleteImportStatus;
          }

          return testQueuedImportStatus;
        },
      );

      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      let sections: HTMLElement[] = [];

      await waitFor(() => {
        sections = screen.getAllByTestId('accordionSection');
        expect(sections).toHaveLength(2);

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
    });

    test('clicking delete files button shows modal to confirm deletion plan', async () => {
      releaseDataFileService.getReleaseDataFiles.mockResolvedValue(
        testDataFiles,
      );
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
            },
            {
              contentSectionHeading: 'Test section 2',
              name: 'Test data block 2',
              infographicFilesInfo: [],
            },
          ],
        },
        footnoteIds: ['footnote-1'],
      });

      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      let sections: HTMLElement[] = [];

      await waitFor(() => {
        sections = screen.getAllByTestId('accordionSection');
        expect(sections).toHaveLength(2);

        expect(
          within(sections[1]).getByRole('button', {
            name: 'Delete files',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      );

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();

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
    });

    test('confirming deletion removes the data file section', async () => {
      releaseDataFileService.getReleaseDataFiles.mockResolvedValue(
        testDataFiles,
      );
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
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      let sections: HTMLElement[] = [];

      await waitFor(() => {
        sections = screen.getAllByTestId('accordionSection');
        expect(sections).toHaveLength(2);

        expect(
          within(sections[1]).getByRole('button', {
            name: 'Delete files',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete files',
        }),
      );

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      userEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(1);
        expect(
          within(screen.getByTestId('accordionSection')).getByRole('button', {
            name: 'Test data 1',
          }),
        ).toBeInTheDocument();
      });
    });
  });

  describe('uploading data file', () => {
    beforeEach(() => {
      releaseDataFileService.getReleaseDataFiles.mockResolvedValue(
        testDataFiles,
      );
      releaseDataFileService.getDataFileImportStatus.mockResolvedValue(
        testQueuedImportStatus,
      );
    });

    test('show validation message when no subject title', async () => {
      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
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
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
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

    test('shows validation message when no data file selected', async () => {
      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      userEvent.click(screen.getByLabelText('Upload data file'));
      fireEvent.change(screen.getByLabelText('Upload data file'), {
        target: {
          value: null,
        },
      });
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Choose a data file', {
            selector: '#dataFileUploadForm-dataFile-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when no meta data file selected', async () => {
      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      userEvent.click(screen.getByLabelText('Upload metadata file'));
      fireEvent.change(screen.getByLabelText('Upload metadata file'), {
        target: { value: null },
      });
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Choose a metadata file', {
            selector: '#dataFileUploadForm-metadataFile-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('cannot submit with invalid values', async () => {
      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
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
      });
    });

    test('can submit with valid values', async () => {
      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      const dataFile = new File([''], 'test-data.csv');
      const metadataFile = new File([''], 'test-data.meta.csv');

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
            subjectTitle: 'Test title',
            dataFile,
            metadataFile,
          } as UploadDataFilesRequest,
        );
      });
    });

    test('renders with uploaded data file appended to list of data files', async () => {
      releaseDataFileService.uploadDataFiles.mockResolvedValue({
        title: 'Test title',
        userName: 'user1@test.com',
        filename: 'test-data.csv',
        metadataFilename: 'test-data.meta.csv',
        rows: 300,
        fileSize: {
          size: 150,
          unit: 'Kb',
        },
        created: new Date('2020-08-18T12:00:00'),
      });

      render(
        <ReleaseDataUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      const dataFile = new File([''], 'test-data.csv');
      const metadataFile = new File([''], 'test-data.meta.csv');

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
            subjectTitle: 'Test title',
            dataFile,
            metadataFile,
          } as UploadDataFilesRequest,
        );

        const sections = screen.getAllByTestId('accordionSection');

        expect(sections).toHaveLength(3);

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
        expect(section3.getByTestId('Number of rows')).toHaveTextContent('300');
        expect(section3.getByTestId('Status')).toHaveTextContent('Queued');
        expect(section3.getByTestId('Uploaded by')).toHaveTextContent(
          'user1@test.com',
        );
        expect(section3.getByTestId('Date uploaded')).toHaveTextContent(
          '18 August 2020 12:00',
        );
      });
    });
  });
});
