import generateReleaseContent, {
  defaultPublication,
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleasePageTabMethodology from '@admin/pages/release/content/components/ReleasePageTabMethodology';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

const testReleaseContent = generateReleaseContent({});
const renderWithContext = (
  component: React.ReactNode,
  releaseContent: ReleaseContentType = testReleaseContent,
) =>
  render(
    <ReleaseContentProvider
      value={{
        ...releaseContent,
        canUpdateRelease: true,
      }}
    >
      <EditingContextProvider editingMode="preview">
        <MemoryRouter>{component}</MemoryRouter>
      </EditingContextProvider>
      ,
    </ReleaseContentProvider>,
  );

describe('ReleasePageTabMethodology', () => {
  test('renders methodology tab content', () => {
    renderWithContext(<ReleasePageTabMethodology />);

    expect(
      screen.getByRole('heading', {
        name: 'Contact us',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Methodology',
        level: 2,
      }),
    ).toBeInTheDocument();

    const list = screen.getByTestId('methodologies-list');
    const listItems = within(list).getAllByRole('listitem');
    expect(listItems).toHaveLength(1);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Methodology title',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-id/summary');
  });

  test('renders external methodology', () => {
    renderWithContext(
      <ReleasePageTabMethodology />,
      generateReleaseContent({
        release: generateEditableRelease({
          publication: {
            ...defaultPublication,
            externalMethodology: {
              title: 'External methodology title',
              url: 'https://example.com',
            },
          },
        }),
      }),
    );

    expect(
      screen.getByRole('heading', {
        name: 'Methodology',
        level: 2,
      }),
    ).toBeInTheDocument();

    const list = screen.getByTestId('methodologies-list');
    const listItems = within(list).getAllByRole('listitem');
    expect(listItems).toHaveLength(2);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Methodology title',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-id/summary');
    expect(
      within(listItems[1]).getByRole('link', {
        name: 'External methodology title (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://example.com');
  });
});
