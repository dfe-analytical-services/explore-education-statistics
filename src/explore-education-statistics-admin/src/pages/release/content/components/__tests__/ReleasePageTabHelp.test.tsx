import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleasePageTabHelp from '@admin/pages/release/content/components/ReleasePageTabHelp';
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

describe('ReleasePageTabHelp', () => {
  test('renders help tab content when all section content exists', () => {
    renderWithContext(
      <ReleasePageTabHelp hidden={false} />,
      generateReleaseContent({
        release: generateEditableRelease({
          hasPreReleaseAccessList: true,
        }),
      }),
    );

    expect(
      screen.getByRole('heading', {
        name: 'Get help by contacting us',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Official statistics',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Related information',
        level: 2,
      }),
    ).toBeInTheDocument();

    const relatedInfoList = screen.getByTestId('related-information-list');
    const relatedInfoListItems =
      within(relatedInfoList).getAllByRole('listitem');
    expect(relatedInfoListItems).toHaveLength(1);
    expect(
      within(relatedInfoListItems[0]).getByRole('link', {
        name: 'Related information description',
      }),
    ).toHaveAttribute('href', 'https://test.com');

    expect(
      screen.getByRole('heading', {
        name: 'Pre-release access list',
        level: 2,
      }),
    ).toBeInTheDocument();
  });

  test('renders help tab content without optional sections', () => {
    renderWithContext(
      <ReleasePageTabHelp hidden={false} />,
      generateReleaseContent({
        release: generateEditableRelease({
          relatedInformation: [],
        }),
      }),
    );

    expect(
      screen.getByRole('heading', {
        name: 'Get help by contacting us',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Official statistics',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', {
        name: 'Related information',
        level: 2,
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', {
        name: 'Pre-release access list',
        level: 2,
      }),
    ).not.toBeInTheDocument();
  });
});
