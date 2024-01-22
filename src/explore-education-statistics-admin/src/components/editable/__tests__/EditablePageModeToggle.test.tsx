import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('EditablePageModeToggle', () => {
  test('renders the default options', () => {
    render(<EditablePageModeToggle />);

    const group = screen.getByRole('group', { name: 'Change page view' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(2);
    expect(options[0]).toEqual(screen.getByLabelText('Edit content'));
    expect(options[1]).toEqual(screen.getByLabelText('Preview content'));
  });

  test('deso not render the edit content option when canUpdateRelease is false', () => {
    render(<EditablePageModeToggle canUpdateRelease={false} />);

    const group = screen.getByRole('group', { name: 'Change page view' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(1);
    expect(options[0]).toEqual(screen.getByLabelText('Preview content'));
  });

  test('renders the preview table tool option when showTablePreviewOption is true', () => {
    render(<EditablePageModeToggle showTablePreviewOption />);

    const group = screen.getByRole('group', { name: 'Change page view' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(3);
    expect(options[0]).toEqual(screen.getByLabelText('Edit content'));
    expect(options[1]).toEqual(screen.getByLabelText('Preview content'));
    expect(options[2]).toEqual(screen.getByLabelText('Preview table tool'));
  });

  test('renders the mobile version', async () => {
    mockIsMedia = true;

    render(<EditablePageModeToggle />);

    expect(
      screen.getByRole('button', { name: 'Change page view' }),
    ).toBeInTheDocument();
    const group = screen.getByRole('group', { name: 'Change page view' });
    expect(group).toBeVisible();

    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(2);
    expect(options[0]).toEqual(screen.getByLabelText('Edit content'));
    expect(options[1]).toEqual(screen.getByLabelText('Preview content'));

    mockIsMedia = false;
  });

  test('renders the desktop version', () => {
    expect(
      screen.queryByRole('button', { name: 'Change page view' }),
    ).not.toBeInTheDocument();
  });
});
