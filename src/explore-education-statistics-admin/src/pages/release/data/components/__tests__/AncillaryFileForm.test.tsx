import AncillaryFileForm, {
  AncillaryFileFormProps,
} from '@admin/pages/release/data/components/AncillaryFileForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('AncillaryFileForm', () => {
  test('shows validation message if `title` field is empty', async () => {
    const { user } = render(<AncillaryFileForm onSubmit={noop} />);

    await user.clear(screen.getByLabelText('Title'));
    await user.click(screen.getByRole('button', { name: 'Add file' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a title', {
          selector: '#ancillaryFileForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message if `title` field is not unique to other files', async () => {
    const { user } = render(
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

    await user.clear(screen.getByLabelText('Title'));
    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.click(screen.getByRole('button', { name: 'Add file' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a unique title', {
          selector: '#ancillaryFileForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message if `summary` field is empty', async () => {
    const { user } = render(<AncillaryFileForm onSubmit={noop} />);

    await user.clear(screen.getByLabelText('Summary'));
    await user.click(screen.getByRole('button', { name: 'Add file' }));

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

    const { user } = render(<AncillaryFileForm onSubmit={noop} />);

    await user.upload(screen.getByLabelText('Upload file'), testFile);
    await user.click(screen.getByRole('button', { name: 'Add file' }));

    await waitFor(() => {
      expect(
        screen.getByText('Choose a file that is not empty', {
          selector: '#ancillaryFileForm-file-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation messages if submitted form is invalid', async () => {
    const { user } = render(<AncillaryFileForm onSubmit={noop} />);

    await user.click(screen.getByRole('button', { name: 'Add file' }));

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

    const { user } = render(<AncillaryFileForm onSubmit={handleSubmit} />);

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Summary'), 'Test summary');

    const testFile = new File(['test'], 'test.txt');

    await user.upload(screen.getByLabelText('Upload file'), testFile);

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Add file' }));

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
