import ApiDataSetGuidanceNotesForm from '@admin/pages/release/data/components/ApiDataSetGuidanceNotesForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';

describe('ApiDataSetGuidanceNotesForm', () => {
  test('renders correctly', () => {
    render(<ApiDataSetGuidanceNotesForm onSubmit={noop} />);

    expect(screen.getByLabelText('Public guidance notes')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with initial notes', () => {
    render(<ApiDataSetGuidanceNotesForm notes="Test notes" onSubmit={noop} />);

    expect(screen.getByLabelText('Public guidance notes')).toHaveValue(
      'Test notes',
    );
    expect(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    ).toBeInTheDocument();
  });

  test('submitting form calls the `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetGuidanceNotesForm onSubmit={handleSubmit} />,
    );

    await user.type(
      screen.getByLabelText('Public guidance notes'),
      'Test notes',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith({ notes: 'Test notes' });
    });
  });
});
