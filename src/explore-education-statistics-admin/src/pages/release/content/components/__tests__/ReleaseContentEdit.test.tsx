import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleaseContentEdit from '@admin/pages/release/content/components/ReleaseContentEdit';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';
import {
  render as baseRender,
  RenderResult,
  screen,
  within,
} from '@testing-library/react';
import React, { ReactNode } from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/hubs/utils/createConnection');

describe('ReleaseContentEdit', () => {
  const testReleaseContent = generateReleaseContent({});

  test('renders the content', async () => {
    render(<ReleaseContentEdit />);

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
    expect(quickLinks).toHaveLength(1);
    expect(quickLinks[0]).toHaveTextContent('Release contents');
    expect(quickLinks[0]).toHaveAttribute('href', '#releaseMainContent');

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
      screen.getByRole('heading', {
        name: 'Headline facts and figures - 2020/21',
      }),
    );

    expect(
      screen.getByRole('heading', {
        name: 'Data dashboards',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Contents',
      }),
    ).toBeInTheDocument();
  });

  function render(
    child: ReactNode,
    releaseContent: ReleaseContentType = testReleaseContent,
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
            <EditingContextProvider editingMode="edit">
              <MemoryRouter>{child}</MemoryRouter>
            </EditingContextProvider>
          </ReleaseContentProvider>
        </ReleaseContentHubContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
