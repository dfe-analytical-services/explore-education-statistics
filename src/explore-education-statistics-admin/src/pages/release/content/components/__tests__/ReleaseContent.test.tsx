import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import {
  EditingContextProvider,
  EditingMode,
} from '@admin/contexts/EditingContext';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import {
  render as baseRender,
  RenderResult,
  screen,
  within,
} from '@testing-library/react';
import React, { ReactNode } from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/hubs/utils/createConnection');

describe('ReleaseContent', () => {
  const testReleaseContent = generateReleaseContent({});

  test('renders the content', async () => {
    render(<ReleaseContent />);

    expect(await screen.findByTestId('Published-value')).toHaveTextContent(
      'TBA',
    );

    expect(
      within(screen.getByTestId('release-summary')).getByText(
        'Summary block body',
      ),
    );

    const quickLinks = within(
      screen.getByRole('navigation', {
        name: 'Quick links',
      }),
    ).getAllByRole('link');
    expect(quickLinks).toHaveLength(3);
    expect(quickLinks[0]).toHaveTextContent('Release contents');
    expect(quickLinks[0]).toHaveAttribute('href', '#releaseMainContent');
    expect(quickLinks[1]).toHaveTextContent('Explore data');
    expect(quickLinks[1]).toHaveAttribute('href', '#explore-data-and-files');
    expect(quickLinks[2]).toHaveTextContent('Help and support');
    expect(quickLinks[2]).toHaveAttribute('href', '#help-and-support');

    const relatedInfo = within(
      screen.getByTestId('related-information'),
    ).getAllByRole('link');
    expect(relatedInfo).toHaveLength(2);
    expect(relatedInfo[0]).toHaveTextContent('Data guidance');
    expect(relatedInfo[0]).toHaveAttribute(
      'href',
      '/publication/publication-id/release/Release-title-id/data-guidance',
    );
    expect(relatedInfo[1]).toHaveTextContent('Contact us');
    expect(relatedInfo[1]).toHaveAttribute('href', '#contact-us');

    expect(
      screen.getByRole('heading', { name: 'Related pages' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'View releases (1)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Releases in this series' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Methodologies' }),
    ).toBeInTheDocument();

    const methodologiesList = within(
      screen.getByTestId('methodologies-list'),
    ).getAllByRole('listitem');
    expect(methodologiesList).toHaveLength(1);
    expect(methodologiesList[0]).toHaveTextContent('Methodology title');

    expect(
      screen.getByRole('heading', {
        name: 'Headline facts and figures - 2020/21',
      }),
    );

    expect(
      screen.getByRole('heading', {
        name: 'Explore data and files used in this release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Contents',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Help and support',
      }),
    ).toBeInTheDocument();
  });

  test('renders the "release contents" quick link in edit mode when there is no content', async () => {
    const testReleaseContentWithoutContent = generateReleaseContent({
      release: generateEditableRelease({ content: [] }),
    });
    render(<ReleaseContent />, testReleaseContentWithoutContent);

    const navigation = await screen.findByRole('navigation', {
      name: 'Quick links',
    });

    const quickLinks = within(navigation).getAllByRole('link');
    expect(quickLinks).toHaveLength(3);
    expect(quickLinks[0]).toHaveTextContent('Release contents');
    expect(quickLinks[0]).toHaveAttribute('href', '#releaseMainContent');
  });

  test('does not render the "release contents" quick link in preview mode when there is no content', async () => {
    const testReleaseContentWithoutContent = generateReleaseContent({
      release: generateEditableRelease({ content: [] }),
    });
    render(<ReleaseContent />, testReleaseContentWithoutContent, 'preview');

    const navigation = await screen.findByRole('navigation', {
      name: 'Quick links',
    });

    const quickLinks = within(navigation).getAllByRole('link');
    expect(quickLinks).toHaveLength(2);
    expect(quickLinks[0]).toHaveTextContent('Explore data');
    expect(quickLinks[0]).toHaveAttribute('href', '#explore-data-and-files');
    expect(quickLinks[1]).toHaveTextContent('Help and support');
    expect(quickLinks[1]).toHaveAttribute('href', '#help-and-support');

    expect(screen.queryByText('Release contents')).not.toBeInTheDocument();
  });

  function render(
    child: ReactNode,
    releaseContent: ReleaseContentType = testReleaseContent,
    editingMode: EditingMode = 'edit',
  ): RenderResult {
    return baseRender(
      <TestConfigContextProvider>
        unattachedDataBlocks: [], unattachedDataBlocks: [],
        <ReleaseContentHubContextProvider
          releaseVersionId={releaseContent.release.id}
        >
          <ReleaseContentProvider
            value={{
              ...releaseContent,
              canUpdateRelease: true,
            }}
          >
            <EditingContextProvider editingMode={editingMode}>
              <MemoryRouter>{child}</MemoryRouter>
            </EditingContextProvider>
          </ReleaseContentProvider>
        </ReleaseContentHubContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
