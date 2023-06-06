import AncillaryFileForm, {
  AncillaryFileFormProps,
} from '@admin/pages/release/data/components/AncillaryFileForm';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('AncillaryFileForm', () => {
  test('shows validation message if `title` field is empty', async () => {
    render(<AncillaryFileForm onSubmit={noop} />);

    userEvent.clear(screen.getByLabelText('Title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a title', {
          selector: '#ancillaryFileForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message if `title` field is not unique to other files', async () => {
    render(
      <AncillaryFileForm
        files={[
          {
            id: 'file-1',
            title: 'Test title',
            summary: 'Test summary',
            filename: 'test-file.txt',
            fileSize: {
              size: 20,
              unit: 'kB',
            },
            userName: '',
            created: '',
          },
        ]}
        onSubmit={noop}
      />,
    );

    userEvent.clear(screen.getByLabelText('Title'));
    await userEvent.type(screen.getByLabelText('Title'), 'Test title');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a unique title', {
          selector: '#ancillaryFileForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message if `summary` field is empty', async () => {
    render(<AncillaryFileForm onSubmit={noop} />);

    userEvent.clear(screen.getByLabelText('Summary'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a summary', {
          selector: '#ancillaryFileForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message if `file` field contains an empty file', async () => {
    const testFile = new File([''], 'test.txt');

    render(<AncillaryFileForm onSubmit={noop} />);

    userEvent.upload(screen.getByLabelText('Upload file'), testFile);
    userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(
        screen.getByText('Choose a file that is not empty', {
          selector: '#ancillaryFileForm-file-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation messages if submitted form is invalid', async () => {
    render(<AncillaryFileForm onSubmit={noop} />);

    userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a title', {
          selector: '#ancillaryFileForm-title-error',
        }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText('Enter a summary', {
        selector: '#ancillaryFileForm-summary-error',
      }),
    ).toBeInTheDocument();
  });

  test('successfully submitting form calls `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    render(<AncillaryFileForm onSubmit={handleSubmit} />);

    await userEvent.type(screen.getByLabelText('Title'), 'Test title');
    await userEvent.type(screen.getByLabelText('Summary'), 'Test summary');

    const testFile = new File(['test'], 'test.txt');

    userEvent.upload(screen.getByLabelText('Upload file'), testFile);

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<
        Parameters<AncillaryFileFormProps['onSubmit']>
      >({
        title: 'Test title',
        summary: 'Test summary',
        file: testFile,
      });
    });
  });
});
