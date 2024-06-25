import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import render from '@common-test/render';

describe('DataFileUploadForm', () => {
  test('shows validation message when no data file selected', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when data file is not empty', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    await user.upload(screen.getByLabelText('Upload data file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file that is not empty', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no meta data file selected', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a metadata file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when metadata file is not empty', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    await user.upload(screen.getByLabelText('Upload metadata file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a metadata file that is not empty', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no ZIP file selected', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    await user.click(screen.getByLabelText('ZIP file'));
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when ZIP file is empty', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('ZIP file'));
    await user.upload(screen.getByLabelText('Upload ZIP file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no bulk ZIP file selected', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('Bulk ZIP upload'));
    await user.upload(screen.getByLabelText('Upload bulk ZIP file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a zip file that is not empty', {
          selector: '#dataFileUploadForm-bulkZipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when bulk ZIP file is empty', async () => {
    const { user } = render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('Bulk ZIP upload'));
    await user.upload(screen.getByLabelText('Upload bulk ZIP file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-bulkZipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values when trying to upload CSV files', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<DataFileUploadForm onSubmit={handleSubmit} />);

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();

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
    const handleSubmit = jest.fn();

    const { user } = render(<DataFileUploadForm onSubmit={handleSubmit} />);

    await user.click(screen.getByLabelText('ZIP file'));
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();

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
});
