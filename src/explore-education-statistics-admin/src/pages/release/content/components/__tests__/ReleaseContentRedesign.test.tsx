import ReleaseContentRedesign from '@admin/pages/release/content/components/ReleaseContentRedesign';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import React from 'react';
import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';

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
        {component}
      </EditingContextProvider>
      ,
    </ReleaseContentProvider>,
  );

describe('ReleaseContentRedesign', () => {
  test('renders the publication summary', () => {
    renderWithContext(<ReleaseContentRedesign />);

    expect(screen.getByTestId('release-summary-block')).toBeInTheDocument();
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
