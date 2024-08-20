import ApiDataSetNotesForm from '@admin/pages/release/data/components/ApiDataSetNotesForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';

describe('ApiDataSetNotesForm', () => {
  test('renders correctly', () => {
    render(<ApiDataSetNotesForm onSubmit={noop} />);

    expect(screen.getByLabelText('Public guidance notes')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add public guidance notes' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with initial notes', () => {
    render(<ApiDataSetNotesForm notes="Test notes" onSubmit={noop} />);

    expect(screen.getByLabelText('Public guidance notes')).toHaveValue(
      'Test notes',
    );
    expect(
      screen.getByRole('button', { name: 'Update public guidance notes' }),
    ).toBeInTheDocument();
  });

  test('submitting form calls the `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<ApiDataSetNotesForm onSubmit={handleSubmit} />);

    await user.type(
      screen.getByLabelText('Public guidance notes'),
      'Test notes',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Add public guidance notes' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith({ notes: 'Test notes' });
    });
  });
});
