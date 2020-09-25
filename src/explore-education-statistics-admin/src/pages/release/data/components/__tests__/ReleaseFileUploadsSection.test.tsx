import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import _releaseAncillaryFileService, {
  AncillaryFile,
  UploadAncillaryFileRequest,
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

jest.mock('@admin/services/releaseAncillaryFileService');

const releaseAncillaryFileService = _releaseAncillaryFileService as jest.Mocked<
  typeof _releaseAncillaryFileService
>;

describe('ReleaseFileUploadsSection', () => {
  const testFiles: AncillaryFile[] = [
    {
      id: 'file-1',
      title: 'Test file 1',
      filename: 'file-1.docx',
      fileSize: {
        size: 50,
        unit: 'Kb',
      },
    },
    {
      id: 'file-2',
      title: 'Test file 2',
      filename: 'file-2.docx',
      fileSize: {
        size: 100,
        unit: 'Kb',
      },
    },
  ];

  test('renders list of uploaded files', async () => {
    releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(testFiles);

    render(
      <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
    );

    await waitFor(() => {
      expect(
        releaseAncillaryFileService.getAncillaryFiles,
      ).toHaveBeenCalledWith('release-1');

      const sections = screen.getAllByTestId('accordionSection');

      expect(sections).toHaveLength(2);

      const section1 = within(sections[0]);

      expect(
        section1.getByRole('button', { name: 'Test file 1' }),
      ).toBeInTheDocument();

      expect(section1.getByTestId('Name')).toHaveTextContent('Test file 1');
      expect(section1.getByTestId('File')).toHaveTextContent('file-1.docx');
      expect(section1.getByTestId('File size')).toHaveTextContent('50 Kb');

      const section2 = within(sections[1]);

      expect(
        section2.getByRole('button', { name: 'Test file 2' }),
      ).toBeInTheDocument();

      expect(section2.getByTestId('Name')).toHaveTextContent('Test file 2');
      expect(section2.getByTestId('File')).toHaveTextContent('file-2.docx');
      expect(section2.getByTestId('File size')).toHaveTextContent('100 Kb');
    });
  });

  test('renders empty message when there are no files', async () => {
    releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue([]);

    render(
      <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
    );

    await waitFor(() => {
      const sections = screen.queryAllByTestId('accordionSection');

      expect(sections).toHaveLength(0);
      expect(
        screen.getByText('No files have been uploaded.'),
      ).toBeInTheDocument();
    });
  });

  describe('deleting file', () => {
    test('clicking delete file button shows modal to confirm deletion', async () => {
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );

      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      let sections: HTMLElement[] = [];

      await waitFor(() => {
        sections = screen.getAllByTestId('accordionSection');
        expect(sections).toHaveLength(2);

        expect(
          within(sections[1]).getByRole('button', {
            name: 'Delete file',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
        }),
      );

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();

        const modal = within(screen.getByRole('dialog'));

        expect(modal.getByText('Confirm deletion of file')).toBeInTheDocument();
      });
    });

    test('confirming deletion removes the file section', async () => {
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );

      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      let sections: HTMLElement[] = [];

      await waitFor(() => {
        sections = screen.getAllByTestId('accordionSection');
        expect(sections).toHaveLength(2);

        expect(
          within(sections[1]).getByRole('button', {
            name: 'Delete file',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(
        within(sections[1]).getByRole('button', {
          name: 'Delete file',
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
            name: 'Test file 1',
          }),
        ).toBeInTheDocument();
      });
    });
  });

  describe('uploading file', () => {
    beforeEach(() => {
      releaseAncillaryFileService.getAncillaryFiles.mockResolvedValue(
        testFiles,
      );
    });

    test('show validation message when no subject title', async () => {
      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      userEvent.click(screen.getByLabelText('Name'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a name', {
            selector: '#fileUploadForm-name-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when non-unique name', async () => {
      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      await userEvent.type(screen.getByLabelText('Name'), 'Test file 1');
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a unique name', {
            selector: '#fileUploadForm-name-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when no file selected', async () => {
      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

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

    test('shows validation message when file is empty', async () => {
      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

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
      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

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
          screen.getByText('Enter a name', {
            selector: '#fileUploadForm-name-error',
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
        title: 'Test name',
        filename: 'test-file.docx',
        fileSize: {
          size: 150,
          unit: 'Kb',
        },
      });

      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      const file = new File(['test'], 'test-file.txt');

      await userEvent.type(screen.getByLabelText('Name'), 'Test title');

      userEvent.upload(screen.getByLabelText('Upload file'), file);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload file',
        }),
      );

      await waitFor(() => {
        expect(
          releaseAncillaryFileService.uploadAncillaryFile,
        ).toHaveBeenCalledWith('release-1', {
          name: 'Test title',
          file,
        } as UploadAncillaryFileRequest);
      });
    });

    test('renders with uploaded file appended to list of files', async () => {
      releaseAncillaryFileService.uploadAncillaryFile.mockResolvedValue({
        id: 'test-file',
        title: 'Test name',
        filename: 'test-file.docx',
        fileSize: {
          size: 150,
          unit: 'Kb',
        },
      });

      render(
        <ReleaseFileUploadsSection releaseId="release-1" canUpdateRelease />,
      );

      const file = new File(['test'], 'test-file.docx');

      await userEvent.type(screen.getByLabelText('Name'), 'Test name');
      userEvent.upload(screen.getByLabelText('Upload file'), file);
      userEvent.click(
        screen.getByRole('button', {
          name: 'Upload file',
        }),
      );

      await waitFor(() => {
        expect(
          releaseAncillaryFileService.uploadAncillaryFile,
        ).toHaveBeenCalledWith('release-1', {
          name: 'Test name',
          file,
        } as UploadAncillaryFileRequest);

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
          section3.getByRole('button', { name: 'Test name' }),
        ).toBeInTheDocument();

        expect(section3.getByTestId('Name')).toHaveTextContent('Test name');

        expect(section3.getByTestId('File')).toHaveTextContent(
          'test-file.docx',
        );
        expect(section3.getByTestId('File size')).toHaveTextContent('150 Kb');
      });
    });
  });
});
