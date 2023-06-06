import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import _releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import render from '@common-test/render';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/releaseAncillaryFileService');

const releaseAncillaryFileService = _releaseAncillaryFileService as jest.Mocked<
  typeof _releaseAncillaryFileService
>;

describe('ReleaseFileUploadsSection', () => {
  const testFiles: AncillaryFile[] = [
    {
      id: 'file-1',
      title: 'Test file 1',
      summary: 'Test summary 1',
      filename: 'file-1.docx',
      fileSize: {
        size: 50,
        unit: 'Kb',
      },
      userName: 'test@test.com',
      created: '2021-05-26T00:00:00',
    },
    {
      id: 'file-2',
      title: 'Test file 2',
      summary: 'Test summary 2',
      filename: 'file-2.docx',
      fileSize: {
        size: 100,
        unit: 'Kb',
      },
      userName: 'test@test.com',
      created: '2021-05-27T00:00:00',
    },
  ];

  function renderPage() {
    return render(
      <MemoryRouter>
        <ReleaseFileUploadsSection
          publicationId="publication-1"
          releaseId="release-1"
          canUpdateRelease
        />
      </MemoryRouter>,
    );
  }

  test('renders list of uploaded files', async () => {
    releaseAncillaryFileService.listFiles.mockResolvedValue(testFiles);

    renderPage();

    await waitFor(() => {
      expect(releaseAncillaryFileService.listFiles).toHaveBeenCalledWith(
        'release-1',
      );

      expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
    });

    const sections = screen.getAllByTestId('accordionSection');

    const section1 = within(sections[0]);

    expect(
      section1.getByRole('button', { name: 'Test file 1' }),
    ).toBeInTheDocument();

    expect(section1.getByTestId('Title')).toHaveTextContent('Test file 1');
    expect(section1.getByTestId('File')).toHaveTextContent('file-1.docx');
    expect(section1.getByTestId('File size')).toHaveTextContent('50 Kb');
    expect(section1.getByTestId('Summary')).toHaveTextContent('Test summary 1');
    expect(section1.getByTestId('Uploaded by')).toHaveTextContent(
      'test@test.com',
    );
    expect(section1.getByTestId('Date uploaded')).toHaveTextContent(
      '26 May 2021 00:00',
    );

    const section2 = within(sections[1]);

    expect(
      section2.getByRole('button', { name: 'Test file 2' }),
    ).toBeInTheDocument();

    expect(section2.getByTestId('Title')).toHaveTextContent('Test file 2');
    expect(section2.getByTestId('File')).toHaveTextContent('file-2.docx');
    expect(section2.getByTestId('File size')).toHaveTextContent('100 Kb');
    expect(section2.getByTestId('Summary')).toHaveTextContent('Test summary 2');
    expect(section2.getByTestId('Uploaded by')).toHaveTextContent(
      'test@test.com',
    );
    expect(section2.getByTestId('Date uploaded')).toHaveTextContent(
      '27 May 2021 00:00',
    );
  });

  test('renders empty message when there are no files', async () => {
    releaseAncillaryFileService.listFiles.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(screen.queryAllByTestId('accordionSection')).toHaveLength(0);
    });

    await waitFor(() => {
      expect(
        screen.getByText('No files have been uploaded.'),
      ).toBeInTheDocument();
    });
  });

  describe('deleting file', () => {
    test('clicking delete file button shows modal to confirm deletion', async () => {
      releaseAncillaryFileService.listFiles.mockResolvedValue(testFiles);

      renderPage();

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');
      expect(sections).toHaveLength(2);

      expect(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm deletion of file'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByText('Confirm deletion of file')).toBeInTheDocument();
    });

    test('confirming deletion removes the file section', async () => {
      releaseAncillaryFileService.listFiles.mockResolvedValue(testFiles);

      renderPage();

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const sections = screen.getAllByTestId('accordionSection');

      expect(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm deletion of file'),
        ).toBeInTheDocument();
      });

      expect(screen.getByRole('dialog')).toBeInTheDocument();

      expect(releaseAncillaryFileService.deleteFile).not.toHaveBeenCalled();

      userEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      releaseAncillaryFileService.listFiles.mockResolvedValue([testFiles[0]]);

      await waitFor(() => {
        expect(releaseAncillaryFileService.deleteFile).toHaveBeenCalledWith<
          Parameters<typeof releaseAncillaryFileService.deleteFile>
        >('release-1', 'file-2');
      });

      const updatedSections = screen.getAllByTestId('accordionSection');

      expect(updatedSections).toHaveLength(1);

      expect(
        within(updatedSections[0]).getByRole('button', {
          name: 'Test file 1',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('uploading file', () => {
    beforeEach(() => {
      releaseAncillaryFileService.listFiles.mockResolvedValue(testFiles);
    });

    test('shows validation message when no `file` selected', async () => {
      renderPage();

      userEvent.click(screen.getByLabelText('Upload file'));
      fireEvent.change(screen.getByLabelText('Upload file'), {
        target: {
          value: null,
        },
      });
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Choose a file', {
            selector: '#ancillaryFileForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when `file` uploaded is an empty file', async () => {
      renderPage();

      userEvent.upload(
        screen.getByLabelText('Upload file'),
        new File([''], 'test.txt'),
      );
      userEvent.click(screen.getByRole('button', { name: 'Add file' }));

      await waitFor(() => {
        expect(
          screen.getByText('Choose a file that is not empty', {
            selector: '#ancillaryFileForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('cannot submit with invalid values', async () => {
      renderPage();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Add file',
        }),
      );

      await waitFor(() => {
        expect(releaseAncillaryFileService.createFile).not.toHaveBeenCalled();

        expect(
          screen.getByText('Enter a title', {
            selector: '#ancillaryFileForm-title-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Enter a summary', {
            selector: '#ancillaryFileForm-summary-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Choose a file', {
            selector: '#ancillaryFileForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('can submit with valid values', async () => {
      releaseAncillaryFileService.createFile.mockResolvedValue({
        id: 'test-file',
        title: 'Test title',
        summary: 'Test summary',
        filename: 'test-file.docx',
        fileSize: {
          size: 150,
          unit: 'Kb',
        },
        userName: 'test@test.com',
        created: '2021-05-25T00:00:00',
      });

      renderPage();

      const file = new File(['test'], 'test-file.txt');

      await userEvent.type(screen.getByLabelText('Title'), 'Test title');
      await userEvent.type(screen.getByLabelText('Summary'), 'Test summary');

      userEvent.upload(screen.getByLabelText('Upload file'), file);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Add file',
        }),
      );

      await waitFor(() => {
        expect(releaseAncillaryFileService.createFile).toHaveBeenCalledWith<
          Parameters<typeof releaseAncillaryFileService.createFile>
        >('release-1', {
          title: 'Test title',
          summary: 'Test summary',
          file,
        });
      });
    });

    test('renders with uploaded file appended to list of files', async () => {
      const newTestFile: AncillaryFile = {
        id: 'test-file',
        title: 'Test file 3',
        summary: 'Test summary 3',
        filename: 'test-file.docx',
        fileSize: {
          size: 150,
          unit: 'Kb',
        },
        userName: 'test@test.com',
        created: '2021-05-25T00:00:00',
      };
      releaseAncillaryFileService.createFile.mockResolvedValue(newTestFile);

      renderPage();

      const file = new File(['test'], 'test-file.docx');

      await userEvent.type(screen.getByLabelText('Title'), 'Test file 3');
      await userEvent.type(screen.getByLabelText('Summary'), 'Test summary 3');
      userEvent.upload(screen.getByLabelText('Upload file'), file);

      userEvent.click(
        screen.getByRole('button', {
          name: 'Add file',
        }),
      );

      releaseAncillaryFileService.listFiles.mockResolvedValue([
        ...testFiles,
        newTestFile,
      ]);

      await waitFor(() => {
        expect(releaseAncillaryFileService.createFile).toHaveBeenCalledWith<
          Parameters<typeof releaseAncillaryFileService.createFile>
        >('release-1', {
          title: 'Test file 3',
          summary: 'Test summary 3',
          file,
        });
      });

      const sections = screen.getAllByTestId('accordionSection');

      expect(sections).toHaveLength(3);

      const section1 = within(sections[0]);

      expect(
        section1.getByRole('button', { name: 'Test file 1' }),
      ).toBeInTheDocument();

      const section2 = within(sections[1]);

      expect(
        section2.getByRole('button', { name: 'Test file 2' }),
      ).toBeInTheDocument();

      const section3 = within(sections[2]);

      expect(
        section3.getByRole('button', { name: 'Test file 3' }),
      ).toBeInTheDocument();

      expect(section3.getByTestId('Title')).toHaveTextContent('Test file 3');

      expect(section3.getByTestId('File')).toHaveTextContent('test-file.docx');
      expect(section3.getByTestId('File size')).toHaveTextContent('150 Kb');
      expect(section3.getByTestId('Summary')).toHaveTextContent(
        'Test summary 3',
      );
      expect(section3.getByTestId('Uploaded by')).toHaveTextContent(
        'test@test.com',
      );
      expect(section3.getByTestId('Date uploaded')).toHaveTextContent(
        '25 May 2021 00:00',
      );
    });
  });
});
