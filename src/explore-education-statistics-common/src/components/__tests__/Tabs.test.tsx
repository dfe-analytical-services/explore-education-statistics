import React from 'react';
import { fireEvent, render, screen, within } from '@testing-library/react';
import Tabs from '../Tabs';
import TabsSection from '../TabsSection';

describe('Tabs', () => {
  test('renders single tab correctly', () => {
    const { container } = render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" headingTitle="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
      </Tabs>,
    );

    const tabs = screen.getAllByRole('tab');
    const sections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabs).toHaveLength(1);
    expect(sections).toHaveLength(1);

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[0]).toHaveTextContent('Tab 1');

    expect(
      within(sections[0]).getByRole('heading', {
        name: 'Tab 1',
      }),
    ).toBeInTheDocument();

    expect(sections[0]).toHaveTextContent('Test section 1 content');

    expect(container).toMatchSnapshot();
  });

  test('renders multiple tabs correctly with titles', () => {
    const { container } = render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" headingTitle="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2" headingTitle="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    const tabs = screen.getAllByRole('tab');
    const sections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabs).toHaveLength(2);
    expect(sections).toHaveLength(2);

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[0]).toHaveTextContent('Tab 1');

    expect(
      within(sections[0]).getByRole('heading', {
        name: 'Tab 1',
      }),
    ).toBeInTheDocument();

    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[1]).toHaveTextContent('Tab 2');

    expect(
      within(sections[1]).getByRole('heading', {
        name: 'Tab 2',
        hidden: true,
      }),
    ).toBeInTheDocument();

    expect(container).toMatchSnapshot();
  });

  test('renders multiple tabs correctly without titles', () => {
    const { container } = render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    const tabs = screen.getAllByRole('tab');
    const sections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabs).toHaveLength(2);
    expect(sections).toHaveLength(2);

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[0]).toHaveTextContent('Tab 1');

    expect(
      within(sections[0]).queryByRole('heading', {
        name: 'Tab 1',
      }),
    ).toBeNull();

    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[1]).toHaveTextContent('Tab 2');

    expect(
      within(sections[1]).queryByRole('heading', {
        name: 'Tab 2',
        hidden: true,
      }),
    ).toBeNull();

    expect(container).toMatchSnapshot();
  });

  test('can use custom section IDs', () => {
    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" id="custom-section">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(
      screen.getByRole('tab', {
        name: 'Tab 1',
      }),
    ).toHaveAttribute('id', 'custom-section-tab');

    expect(
      screen.getByRole('tab', {
        name: 'Tab 2',
      }),
    ).toHaveAttribute('id', 'test-tabs-2-tab');

    const sections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(sections[0]).toHaveAttribute('id', 'custom-section');
    expect(sections[1]).toHaveAttribute('id', 'test-tabs-2');
  });

  test('setting `headingTag` changes section heading size', () => {
    const { container } = render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" headingTitle="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2" headingTitle="Tab 2" headingTag="h2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    const heading1 = container.querySelector('h3') as HTMLHeadingElement;
    const heading2 = container.querySelector('h2') as HTMLHeadingElement;

    expect(heading1).toHaveTextContent('Tab 1');
    expect(heading2).toHaveTextContent('Tab 2');
  });

  test('does not immediately render lazy tab section', () => {
    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" headingTitle="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2" headingTitle="Tab 2" lazy>
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(screen.queryByText('Test section 1 content')).not.toBeNull();
    expect(screen.queryByText('Test section 2 content')).toBeNull();
  });

  test('tab links match section ids', () => {
    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(screen.getAllByText('Tab 1')[0]).toHaveAttribute(
      'href',
      '#test-tabs-1',
    );
    expect(screen.getAllByText('Tab 2')[0]).toHaveAttribute(
      'href',
      '#test-tabs-2',
    );
  });

  test('renders with tab open that matches location hash', () => {
    window.location.hash = '#test-tabs-2';

    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(screen.getByRole('tab', { name: 'Tab 1' })).toHaveAttribute(
      'aria-selected',
      'false',
    );
    expect(screen.getByRole('tab', { name: 'Tab 2' })).toHaveAttribute(
      'aria-selected',
      'true',
    );

    const tabSections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabSections[0]).not.toBeVisible();
    expect(tabSections[1]).toBeVisible();
  });

  test('renders with tab with custom id open that matches location hash', () => {
    window.location.hash = '#tab2';

    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1" id="tab1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2" id="tab2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(screen.getByRole('tab', { name: 'Tab 1' })).toHaveAttribute(
      'aria-selected',
      'false',
    );
    expect(screen.getByRole('tab', { name: 'Tab 2' })).toHaveAttribute(
      'aria-selected',
      'true',
    );

    const tabSections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabSections[0]).not.toBeVisible();
    expect(tabSections[1]).toBeVisible();
  });

  test('clicking tab reveals correct section', () => {
    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    const tabSections = screen.getAllByRole('tabpanel', {
      hidden: true,
    });

    expect(tabSections[0]).toBeVisible();
    expect(tabSections[1]).not.toBeVisible();

    fireEvent.click(screen.getByRole('tab', { name: 'Tab 2' }));

    expect(tabSections[0]).not.toBeVisible();
    expect(tabSections[1]).toBeVisible();
  });

  test('clicking tab changes location hash', () => {
    render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(window.location.hash).toBe('');

    fireEvent.click(screen.getByRole('tab', { name: 'Tab 2' }));

    expect(window.location.hash).toBe('#test-tabs-2');
  });

  test('clicking tab does not change location hash if `modifyHash` is false', () => {
    render(
      <Tabs id="test-tabs" modifyHash={false}>
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2">
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(window.location.hash).toBe('');

    fireEvent.click(screen.getByRole('tab', { name: 'Tab 2' }));

    expect(window.location.hash).toBe('');
  });

  test('clicking tab renders lazy section', () => {
    const { getAllByText, queryByText } = render(
      <Tabs id="test-tabs">
        <TabsSection title="Tab 1">
          <p>Test section 1 content</p>
        </TabsSection>
        <TabsSection title="Tab 2" lazy>
          <p>Test section 2 content</p>
        </TabsSection>
      </Tabs>,
    );

    expect(queryByText('Test section 2 content')).toBeNull();

    fireEvent.click(getAllByText('Tab 2')[0]);

    expect(queryByText('Test section 2 content')).not.toBeNull();
  });

  describe('keyboard interactions', () => {
    let tab1: HTMLAnchorElement;
    let tab2: HTMLAnchorElement;
    let tab3: HTMLAnchorElement;
    let tabSection1: HTMLElement;
    let tabSection2: HTMLElement;
    let tabSection3: HTMLElement;

    beforeEach(() => {
      render(
        <Tabs id="test-tabs">
          <TabsSection title="Tab 1">
            <p>Test section 1 content</p>
          </TabsSection>
          <TabsSection title="Tab 2">
            <p>Test section 2 content</p>
          </TabsSection>
          <TabsSection title="Tab 3">
            <p>Test section 2 content</p>
          </TabsSection>
        </Tabs>,
      );

      tab1 = screen.getByRole('tab', { name: 'Tab 1' }) as HTMLAnchorElement;
      tab2 = screen.getByRole('tab', { name: 'Tab 2' }) as HTMLAnchorElement;
      tab3 = screen.getByRole('tab', { name: 'Tab 3' }) as HTMLAnchorElement;

      const tabSections = screen.getAllByRole('tabpanel', {
        hidden: true,
      });

      tabSection1 = tabSections[0] as HTMLElement;
      tabSection2 = tabSections[1] as HTMLElement;
      tabSection3 = tabSections[2] as HTMLElement;
    });

    test('pressing left arrow key moves to previous tab', () => {
      fireEvent.click(tab2);

      expect(tab1).toHaveAttribute('aria-selected', 'false');
      expect(tab2).toHaveAttribute('aria-selected', 'true');

      expect(tabSection1).not.toBeVisible();
      expect(tabSection2).toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-2');

      fireEvent.keyDown(tab2, { key: 'ArrowLeft' });

      expect(tab1).toHaveAttribute('aria-selected', 'true');
      expect(tab1).toHaveFocus();
      expect(tab2).toHaveAttribute('aria-selected', 'false');

      expect(tabSection1).toBeVisible();
      expect(tabSection2).not.toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-1');
    });

    test('pressing left arrow key cycles to end of tabs', () => {
      fireEvent.click(tab1);

      expect(tab1).toHaveAttribute('aria-selected', 'true');
      expect(tab3).toHaveAttribute('aria-selected', 'false');

      expect(tabSection1).toBeVisible();
      expect(tabSection3).not.toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-1');

      fireEvent.keyDown(tab1, { key: 'ArrowLeft' });

      expect(tab1).toHaveAttribute('aria-selected', 'false');
      expect(tab3).toHaveAttribute('aria-selected', 'true');
      expect(tab3).toHaveFocus();

      expect(tabSection1).not.toBeVisible();
      expect(tabSection3).toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-3');
    });

    test('pressing right arrow key moves to next tab', () => {
      fireEvent.click(tab2);

      expect(tab2).toHaveAttribute('aria-selected', 'true');
      expect(tab3).toHaveAttribute('aria-selected', 'false');

      expect(tabSection2).toBeVisible();
      expect(tabSection3).not.toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-2');

      fireEvent.keyDown(tab2, { key: 'ArrowRight' });

      expect(tab2).toHaveAttribute('aria-selected', 'false');
      expect(tab3).toHaveAttribute('aria-selected', 'true');
      expect(tab3).toHaveFocus();

      expect(tabSection2).not.toBeVisible();
      expect(tabSection3).toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-3');
    });

    test('pressing right arrow key cycles to beginning of tabs', () => {
      fireEvent.click(tab3);

      expect(tab1).toHaveAttribute('aria-selected', 'false');
      expect(tab3).toHaveAttribute('aria-selected', 'true');

      expect(tabSection1).not.toBeVisible();
      expect(tabSection3).toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-3');

      fireEvent.keyDown(tab3, { key: 'ArrowRight' });

      expect(tab1).toHaveAttribute('aria-selected', 'true');
      expect(tab1).toHaveFocus();
      expect(tab3).toHaveAttribute('aria-selected', 'false');

      expect(tabSection1).toBeVisible();
      expect(tabSection3).not.toBeVisible();

      expect(window.location.hash).toBe('#test-tabs-1');
    });

    test('pressing down arrow key focuses the tab section', async () => {
      expect(tab1).toHaveAttribute('aria-selected', 'true');

      fireEvent.keyDown(tab1, { key: 'ArrowDown' });

      expect(tabSection1).toHaveFocus();
    });
  });
});
