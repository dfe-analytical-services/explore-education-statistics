import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import _releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
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
    releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(testFiles);

    renderPage();

    await waitFor(() => {
      expect(
        releaseAncillaryFileService.getAncillaryFiles,
      ).toHaveBeenCalledWith('release-1');

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
    releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(screen.queryAllByTestId('accordionSection')).toHaveLength(0);
    });

    expect(
      screen.getByText('No files have been uploaded.'),
    ).toBeInTheDocument();
  });

  describe('deleting file', () => {
    test('clicking delete file button shows modal to confirm deletion', async () => {
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );

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
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );

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
          name: 'Test file 1',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('uploading file', () => {
    beforeEach(() => {
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );
    });

    test('show validation message when no `title`', async () => {
      renderPage();

      userEvent.click(screen.getByLabelText('Title'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a title', {
            selector: '#fileUploadForm-title-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when `title` is non-unique', async () => {
      renderPage();

      await userEvent.type(screen.getByLabelText('Title'), 'Test file 1');
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a unique title', {
            selector: '#fileUploadForm-title-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when no `summary`', async () => {
      renderPage();

      userEvent.click(screen.getByLabelText('Summary'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a summary', {
            selector: '#fileUploadForm-summary-error',
          }),
        ).toBeInTheDocument();
      });
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
            selector: '#fileUploadForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when `file` is empty', async () => {
      renderPage();

      const file = new File([''], 'test.txt', {
        type: 'text/plain',
      });

      userEvent.upload(screen.getByLabelText('Upload file'), file);
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Choose a file that is not empty', {
            selector: '#fileUploadForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('cannot submit with invalid values', async () => {
      renderPage();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload file',
        }),
      );

      await waitFor(() => {
        expect(
          releaseAncillaryFileService.uploadAncillaryFile,
        ).not.toHaveBeenCalled();

        expect(
          screen.getByText('Enter a title', {
            selector: '#fileUploadForm-title-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Enter a summary', {
            selector: '#fileUploadForm-summary-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Choose a file', {
            selector: '#fileUploadForm-file-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('can submit with valid values', async () => {
      releaseAncillaryFileService.uploadAncillaryFile.mockResolvedValue({
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
          name: 'Upload file',
        }),
      );

      await waitFor(() => {
        expect(
          releaseAncillaryFileService.uploadAncillaryFile,
        ).toHaveBeenCalledWith<
          Parameters<typeof releaseAncillaryFileService.uploadAncillaryFile>
        >('release-1', {
          title: 'Test title',
          summary: 'Test summary',
          file,
        });
      });
    });

    test('renders with uploaded file appended to list of files', async () => {
      releaseAncillaryFileService.uploadAncillaryFile.mockResolvedValue({
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
      });

      renderPage();

      const file = new File(['test'], 'test-file.docx');

      await userEvent.type(screen.getByLabelText('Title'), 'Test file 3');
      await userEvent.type(screen.getByLabelText('Summary'), 'Test summary 3');
      userEvent.upload(screen.getByLabelText('Upload file'), file);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload file',
        }),
      );

      await waitFor(() => {
        expect(
          releaseAncillaryFileService.uploadAncillaryFile,
        ).toHaveBeenCalledWith<
          Parameters<typeof releaseAncillaryFileService.uploadAncillaryFile>
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
