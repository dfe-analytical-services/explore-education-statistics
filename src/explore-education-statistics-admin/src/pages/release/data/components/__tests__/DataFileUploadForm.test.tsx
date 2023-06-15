import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('DataFileUploadForm', () => {
  test('shows validation message when no data file selected', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('CSV files'));

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when data file is not empty', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    userEvent.upload(screen.getByLabelText('Upload data file'), file);
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file that is not empty', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no meta data file selected', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

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

  test('shows validation message when metadata file is not empty', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    userEvent.upload(screen.getByLabelText('Upload metadata file'), file);
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a metadata file that is not empty', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no ZIP file selected', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('ZIP file'));

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when ZIP file is empty', async () => {
    render(<DataFileUploadForm onSubmit={noop} />);

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    userEvent.click(screen.getByLabelText('ZIP file'));
    userEvent.upload(screen.getByLabelText('Upload ZIP file'), file);
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values when trying to upload CSV files', async () => {
    const handleSubmit = jest.fn();

    render(<DataFileUploadForm onSubmit={handleSubmit} />);

    userEvent.click(
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
    });
  });

  test('cannot submit with invalid values when trying to upload ZIP file', async () => {
    const handleSubmit = jest.fn();

    render(<DataFileUploadForm onSubmit={handleSubmit} />);

    userEvent.click(screen.getByLabelText('ZIP file'));
    userEvent.click(
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
