import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleaseContentPreview from '@admin/pages/release/content/components/ReleaseContentPreview';
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
      </ReleaseContentProvider>
    </TestConfigContextProvider>,
  );

describe('ReleaseContentPreview', () => {
  test('renders the publication summary on desktop with correct info', () => {
    renderWithContext(<ReleaseContentPreview />);
    const releaseSummaryBlock = screen.getByTestId('release-summary-block');

    expect(releaseSummaryBlock).toBeInTheDocument();

    expect(
      within(releaseSummaryBlock).getByTestId('Published-value'),
    ).toHaveTextContent('TBA');
  });

  test('renders the publication summary correctly when published with no updates', () => {
    renderWithContext(
      <ReleaseContentPreview />,
      generateReleaseContent({
        release: generateEditableRelease({
          publishScheduled: '2025-08-12T09:30:00+01:00',
          published: '2025-08-10T09:30:00+01:00',
          publishedDisplayDate: '2025-08-10T09:30:00+01:00',
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
      <ReleaseContentPreview />,
      generateReleaseContent({
        release: generateEditableRelease({
          lastUpdated: '2025-08-10T09:30:00+01:00',
          publishScheduled: '2025-08-10T09:30:00+01:00',
          publishedDisplayDate: '2025-08-10T09:30:00+01:00',
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

  test('renders the published date correctly when both published and release scheduled', () => {
    renderWithContext(
      <ReleaseContentPreview />,
      generateReleaseContent({
        release: generateEditableRelease({
          lastUpdated: '2025-08-12T09:30:00+01:00',
          publishScheduled: '2025-08-12T09:30:00+01:00',
          published: '2025-08-10T09:30:00+01:00',
          publishedDisplayDate: '2025-08-10T09:30:00+01:00',
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
    renderWithContext(<ReleaseContentPreview />);

    expect(
      screen.queryByTestId('release-summary-block'),
    ).not.toBeInTheDocument();
    mockIsMedia = false;
  });

  test('tabs render when expected', async () => {
    const { user } = renderWithContext(<ReleaseContentPreview />);

    const tabsContainer = screen.getByTestId('release-page-tabs');
    const tabs = within(tabsContainer).getAllByRole('tab');
    expect(tabs).toHaveLength(4);

    let tabPanels = screen.getAllByTestId('release-page-tab-panel');

    expect(tabPanels).toHaveLength(1);
    expect(tabPanels[0]).toHaveAttribute('id', 'tab-home');
    expect(tabPanels[0]).toHaveRole('tabpanel');

    await user.click(tabs[1]);
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels).toHaveLength(1);
    expect(tabPanels[0]).toHaveAttribute('id', 'tab-explore');

    await user.keyboard('[ArrowRight]');
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels[0]).toHaveAttribute('id', 'tab-methodology');
    expect(tabPanels).toHaveLength(1);

    await user.keyboard('[ArrowRight]');
    tabPanels = screen.getAllByTestId('release-page-tab-panel');
    expect(tabPanels[0]).toHaveAttribute('id', 'tab-help');
    expect(tabPanels).toHaveLength(1);
  });
});
