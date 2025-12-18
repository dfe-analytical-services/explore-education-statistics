import { EditingContextProvider } from '@admin/contexts/EditingContext';
import EditableTileGroupBlock from '@admin/pages/education-in-numbers/content/components/EditableTileGroupBlock';
import { EducationInNumbersPageContentProvider } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import testEinPageVersion, {
  testEinPageContent,
} from '../../__tests__/__data__/testEducationInNumbersPageAndContent';
import testBlock from './__data__/testBlock';

const renderWithContext = (component: React.ReactNode) =>
  render(
    <EducationInNumbersPageContentProvider
      value={{
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      }}
    >
      <EditingContextProvider editingMode="edit">
        {component}
      </EditingContextProvider>
    </EducationInNumbersPageContentProvider>,
  );
describe('EditableTileGroupBlock', () => {
  test('renders correct initial heading', async () => {
    renderWithContext(
      <EditableTileGroupBlock
        block={testBlock}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test Tile Group Block Title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: /Edit group heading/ }),
    ).toBeInTheDocument();
  });

  test('renders correctly without heading', async () => {
    const { user } = renderWithContext(
      <EditableTileGroupBlock
        block={{ ...testBlock, title: undefined }}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );

    expect(screen.queryByTestId('tile-group-heading')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: /Add group heading/ }),
    ).toBeInTheDocument();

    user.click(screen.getByRole('button', { name: /Add group heading/ }));

    await waitFor(() => {
      expect(screen.getByLabelText('Edit heading')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Save group heading' }),
    ).toBeInTheDocument();
  });

  test('renders no tiles correctly', async () => {
    renderWithContext(
      <EditableTileGroupBlock
        block={{ ...testBlock, tiles: [] }}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('No statistic tiles have been added.'),
    ).toBeInTheDocument();
  });

  test('renders tile correctly', async () => {
    renderWithContext(
      <EditableTileGroupBlock
        block={testBlock}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );

    const tile = screen.getByTestId('free-text-stat-tile-tile');
    expect(tile).toBeInTheDocument();
    expect(
      within(tile).getByTestId('free-text-stat-tile-title'),
    ).toHaveTextContent('Tile 1 title');
    expect(
      within(tile).getByTestId('free-text-stat-tile-statistic'),
    ).toHaveTextContent('1000');
    expect(
      within(tile).getByTestId('free-text-stat-tile-trend'),
    ).toHaveTextContent('Tile 1 trend');
    expect(
      within(tile).getByRole('link', { name: 'Tile 1 link text' }),
    ).toHaveAttribute('href', 'https://example.com/tile-1');

    expect(
      screen.getByRole('button', {
        name: 'Edit tile: Test Tile Group Block Title',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Delete tile - Test Tile Group Block Title',
      }),
    ).toBeInTheDocument();
  });

  test('shows edit tile form correctly', async () => {
    const { user } = renderWithContext(
      <EditableTileGroupBlock
        block={testBlock}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );
    expect(
      screen.queryByTestId('freeTextStatTile-editForm'),
    ).not.toBeInTheDocument();

    user.click(
      screen.getByRole('button', {
        name: 'Edit tile: Test Tile Group Block Title',
      }),
    );
    const editForm = await screen.findByTestId('freeTextStatTile-editForm');
    expect(editForm).toBeInTheDocument();
    expect(
      within(editForm).getByRole('textbox', {
        name: 'Title',
      }),
    ).toHaveAttribute('id', 'editableFreeTextStatTileForm-tile-1-title');

    user.click(
      within(editForm).getByRole('button', {
        name: 'Cancel',
      }),
    );
    await waitFor(() =>
      expect(
        screen.queryByTestId('freeTextStatTile-editForm'),
      ).not.toBeInTheDocument(),
    );
  });

  test('when the remove tile button is clicked a confirm modal is shown', async () => {
    const { user } = renderWithContext(
      <EditableTileGroupBlock
        block={testBlock}
        editable
        sectionId="test-section"
        onDelete={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Delete tile - Test Tile Group Block Title',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    await waitFor(() => {
      expect(modal.getByText('Remove tile')).toBeInTheDocument();
    });
  });
});
