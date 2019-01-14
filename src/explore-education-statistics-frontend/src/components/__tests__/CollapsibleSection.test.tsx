import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import CollapsibleSection from '../CollapsibleSection';

describe('CollapsibleSection', () => {
  test('renders with hidden content by default', () => {
    const { container } = render(
      <CollapsibleSection heading="Test heading" contentId="the-content">
        <p>Test content</p>
      </CollapsibleSection>,
    );

    expect(container.querySelector('#the-content')).toHaveAttribute('hidden');
    expect(container).toMatchSnapshot();
  });

  test('renders with visible content when `open` is true', () => {
    const { container } = render(
      <CollapsibleSection heading="Test heading" contentId="the-content" open>
        <p>Test content</p>
      </CollapsibleSection>,
    );

    expect(container.querySelector('#the-content')).not.toHaveAttribute(
      'hidden',
    );
    expect(container).toMatchSnapshot();
  });

  test('clicking heading makes the hidden content visible', () => {
    const { container, getByText } = render(
      <CollapsibleSection heading="Test heading" contentId="the-content">
        <p>Test content</p>
      </CollapsibleSection>,
    );

    const content = container.querySelector('#the-content') as HTMLElement;

    expect(content).toHaveAttribute('hidden');

    fireEvent.click(getByText('Test heading'));

    expect(content).not.toHaveAttribute('hidden');
  });

  test('auto-generates content `id` if no `contentId` is set', () => {
    const { container } = render(
      <CollapsibleSection heading="Test heading">
        <p>Test content</p>
      </CollapsibleSection>,
    );

    const content = container.querySelector('[id]') as HTMLElement;

    expect(content.getAttribute('id')).toContain('collapsible-section-');
  });
});
