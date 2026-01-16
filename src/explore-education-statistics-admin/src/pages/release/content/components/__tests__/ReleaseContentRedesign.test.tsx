import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleaseContentRedesign from '@admin/pages/release/content/components/ReleaseContentRedesign';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
  useDesktopMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const testReleaseContent = generateReleaseContent({});
const renderWithContext = (
  component: React.ReactNode,
  releaseContent: ReleaseContentType = testReleaseContent,
) =>
  render(
    <TestConfigContextProvider>
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
      </ReleaseContentProvider>
      ,
    </TestConfigContextProvider>,
  );

describe('ReleaseContentRedesign', () => {
  test('renders the publication summary on desktop with correct info', () => {
    renderWithContext(<ReleaseContentRedesign />);
    const releaseSummaryBlock = screen.getByTestId('release-summary-block');

    expect(releaseSummaryBlock).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Last updated-value'),
    ).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Published-value'),
    ).toHaveTextContent('TBA');
  });

  test('renders the publication summary correctly when published with no updates', () => {
    renderWithContext(
      <ReleaseContentRedesign />,
      generateReleaseContent({
        release: generateEditableRelease({
          published: '2025-08-10T09:30:00+01:00',
          updates: [],
        }),
      }),
    );
    const releaseSummaryBlock = screen.getByTestId('release-summary-block');

    expect(releaseSummaryBlock).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).queryByTestId('Last updated-value'),
    ).not.toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Published-value'),
    ).toHaveTextContent('10 August 2025');
  });

  test('renders the publication summary correctly when release scheduled', () => {
    renderWithContext(
      <ReleaseContentRedesign />,
      generateReleaseContent({
        release: generateEditableRelease({
          publishScheduled: '2025-08-10T09:30:00+01:00',
        }),
      }),
    );
    const releaseSummaryBlock = screen.getByTestId('release-summary-block');

    expect(releaseSummaryBlock).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Last updated-value'),
    ).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Published-value'),
    ).toHaveTextContent('10 August 2025');
  });

  test('does not render the publication summary on mobile', () => {
    mockIsMedia = true;
    renderWithContext(<ReleaseContentRedesign />);

    expect(
      screen.queryByTestId('release-summary-block'),
    ).not.toBeInTheDocument();
    mockIsMedia = false;
  });

  test('tabs render when expected', async () => {
    const { user } = renderWithContext(<ReleaseContentRedesign />);

    const tabsContainer = screen.getByTestId('release-page-tabs');
    const tabs = within(tabsContainer).getAllByRole('tab');
    expect(tabs).toHaveLength(4);

    let tabPanels = screen.getAllByTestId('release-page-tab-panel');

    expect(tabPanels).toHaveLength(1);
    expect(tabPanels[0]).toHaveAttribute('id', 'tab-home');
    expect(tabPanels[0]).not.toHaveAttribute('hidden', '');
    expect(tabPanels[0]).toHaveRole('tabpanel');

    await user.click(tabs[1]);
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels).toHaveLength(2);
    expect(tabPanels[1]).toHaveAttribute('id', 'tab-explore');
    expect(tabPanels[1]).not.toHaveAttribute('hidden', '');
    expect(tabPanels[1]).toHaveRole('tabpanel');
    expect(tabPanels[0]).toHaveAttribute('hidden', '');

    await user.keyboard('[ArrowRight]');
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels).toHaveLength(3);

    await user.keyboard('[ArrowRight]');
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels).toHaveLength(4);
  });
});
