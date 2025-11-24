import ReleasePageTabBar from '@admin/pages/release/content/components/ReleasePageTabBar';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ReleasePageTabBar', () => {
  test('renders the tabs', () => {
    render(<ReleasePageTabBar activeTab="home" onChangeTab={noop} />);

    const tabs = screen.getAllByRole('tab');

    expect(tabs).toHaveLength(4);

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[0]).toHaveAttribute('id', 'tab-home-tab');
    expect(tabs[0]).toHaveTextContent('Release home');

    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[1]).toHaveAttribute('id', 'tab-explore-tab');
    expect(tabs[1]).toHaveTextContent('Explore and download data');

    expect(tabs[2]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[2]).toHaveAttribute('id', 'tab-methodology-tab');
    expect(tabs[2]).toHaveTextContent('Methodology');

    expect(tabs[3]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[3]).toHaveAttribute('id', 'tab-help-tab');
    expect(tabs[3]).toHaveTextContent('Help and related information');
  });

  test('click and keyboard interaction works', async () => {
    const onChangeTab = jest.fn();
    const { user } = render(
      <ReleasePageTabBar activeTab="explore" onChangeTab={onChangeTab} />,
    );

    const tabs = screen.getAllByRole('tab');

    expect(tabs).toHaveLength(4);

    await user.click(tabs[2]);

    expect(onChangeTab).toHaveBeenCalledWith('methodology');

    await user.keyboard('[ArrowRight]');

    expect(onChangeTab).toHaveBeenCalledWith('help');
    expect(onChangeTab).toHaveBeenCalledTimes(2);

    await user.keyboard('[ArrowRight]');
    // No tabs to the right of help, should not be called again
    expect(onChangeTab).toHaveBeenCalledTimes(2);

    await user.keyboard('[ArrowLeft]');
    expect(onChangeTab).toHaveBeenCalledWith('methodology');
    expect(onChangeTab).toHaveBeenCalledTimes(3);
    await user.keyboard('[ArrowLeft]');
    expect(onChangeTab).toHaveBeenCalledWith('explore');
    expect(onChangeTab).toHaveBeenCalledTimes(4);
    await user.keyboard('[ArrowLeft]');
    expect(onChangeTab).toHaveBeenCalledWith('home');
    expect(onChangeTab).toHaveBeenCalledTimes(5);
    await user.keyboard('[ArrowLeft]');
    expect(onChangeTab).toHaveBeenCalledTimes(5);
  });
});
