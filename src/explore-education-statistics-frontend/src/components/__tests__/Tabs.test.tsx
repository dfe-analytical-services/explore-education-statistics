import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import Tabs from '../Tabs';
import TabsSection from '../TabsSection';

describe('Tabs', () => {
  describe('multiple tabs', () => {
    test('renders correctly', () => {
      const { container } = render(
        <Tabs>
          <TabsSection id="tab-1" title="Tab 1">
            <p>Test section 1 content</p>
          </TabsSection>
          <TabsSection id="tab-2" title="Tab 2">
            <p>Test section 2 content</p>
          </TabsSection>
        </Tabs>,
      );

      expect(container.innerHTML).toMatchSnapshot();
    });

    test('tab links match section ids', () => {
      const { getByText } = render(
        <Tabs>
          <TabsSection id="tab-1" title="Tab 1">
            <p>Test section 1 content</p>
          </TabsSection>
          <TabsSection id="tab-2" title="Tab 2">
            <p>Test section 2 content</p>
          </TabsSection>
        </Tabs>,
      );

      expect(getByText('Tab 1')).toHaveAttribute('href', '#tab-1');
      expect(getByText('Tab 2')).toHaveAttribute('href', '#tab-2');
    });

    test('clicking tab reveals correct section', () => {
      const { getByText, container } = render(
        <Tabs>
          <TabsSection id="tab-1" title="Tab 1">
            <p>Test section 1 content</p>
          </TabsSection>
          <TabsSection id="tab-2" title="Tab 2">
            <p>Test section 2 content</p>
          </TabsSection>
        </Tabs>,
      );

      const tabSection1 = container.querySelector('#tab-1');
      const tabSection2 = container.querySelector('#tab-2');

      expect(tabSection1).not.toHaveClass('govuk-tabs__panel--hidden');
      expect(tabSection2).toHaveClass('govuk-tabs__panel--hidden');

      fireEvent.click(getByText('Tab 2'));

      expect(tabSection1).toHaveClass('govuk-tabs__panel--hidden');
      expect(tabSection2).not.toHaveClass('govuk-tabs__panel--hidden');
    });
  });

  describe('single tab', () => {
    test('renders correctly', () => {
      const { container } = render(
        <Tabs>
          <TabsSection id="tab-1" title="Tab 1">
            <p>Test section 1 content</p>
          </TabsSection>
        </Tabs>,
      );

      expect(container.innerHTML).toMatchSnapshot();
    });
  });
});
