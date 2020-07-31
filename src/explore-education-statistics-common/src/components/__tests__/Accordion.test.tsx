import { fireEvent, render, waitFor } from '@testing-library/react';
import React from 'react';
import Accordion from '../Accordion';
import AccordionSection from '../AccordionSection';

describe('Accordion', () => {
  test('renders with hidden content by default', () => {
    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    expect(
      container.querySelector('.govuk-accordion__section--expanded'),
    ).toBeNull();
    expect(container.querySelector('#test-sections-1-heading')).toHaveAttribute(
      'aria-expanded',
      'false',
    );

    expect(container).toMatchSnapshot();
  });

  test('renders with visible content when `open` is true', () => {
    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading" open>
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    expect(
      container.querySelector('.govuk-accordion__section--expanded'),
    ).toBeDefined();
    expect(container.querySelector('#test-sections-1-heading')).toHaveAttribute(
      'aria-expanded',
      'true',
    );

    expect(container).toMatchSnapshot();
  });

  test('clicking heading makes the hidden content visible', () => {
    const { getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    const heading = getByText('Test heading');

    expect(heading).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('generates section IDs in correct order', () => {
    const { getAllByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const headings = getAllByText('Test heading');
    const contents = getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'test-sections-1-heading');
    expect(contents[0]).toHaveAttribute('id', 'test-sections-1-content');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-2-heading');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-2-content');
  });

  test('can use custom heading or content IDs for sections', () => {
    const { getAllByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading" id="custom-1">
          Test content
        </AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const headings = getAllByText('Test heading');
    const contents = getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'custom-1-heading');
    expect(contents[0]).toHaveAttribute('id', 'custom-1-content');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-2-heading');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-2-content');
  });

  test('scrolls to and opens section if location hash matches section heading ID', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-heading';

    const { getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const heading = getByText('Test heading 1');

    jest.runOnlyPendingTimers();

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('scrolls to and opens section if location hash matches section content ID', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-content';

    const { container, getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const content = container.querySelector(
      '#test-sections-1-content',
    ) as HTMLElement;

    jest.advanceTimersByTime(300);

    await waitFor(() => {
      expect(getByText('Test heading 1')).toHaveAttribute(
        'aria-expanded',
        'true',
      );
      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('scrolls to and opens section if location hash matches an element in the section content', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-heading';

    const { container, getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          <h2 id="test-heading">Test content heading</h2>
        </AccordionSection>
      </Accordion>,
    );

    jest.advanceTimersByTime(300);

    await waitFor(() => {
      expect(getByText('Test heading 2')).toHaveAttribute(
        'aria-expanded',
        'true',
      );

      const content = container.querySelector('#test-heading') as HTMLElement;

      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('accordion sections that were opened when the location hash matches an element in the section content can be closed', () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-content';

    const { getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    jest.runOnlyPendingTimers();

    const heading = getByText('Test heading 1') as HTMLElement;

    expect(heading).toHaveAttribute('aria-expanded', 'true');

    fireEvent.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'false');
  });

  test('clicking on `Open all` reveals all sections', () => {
    const { getByText, getAllByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          <p>Test content 1</p>
        </AccordionSection>
        <AccordionSection heading="Test heading 2" open>
          <p>Test content 2</p>
        </AccordionSection>
      </Accordion>,
    );

    const button = getByText('Open all');
    const sections = getAllByText(/Test heading/);

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    fireEvent.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking on `Open all` causes `onToggleAll` handler to be called with new state', () => {
    const toggleAll = jest.fn();

    const { getByText } = render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    fireEvent.click(getByText('Open all'));

    expect(toggleAll).toHaveBeenCalledWith(true);
  });

  test('clicking on `Close all` closes all sections', () => {
    const { getByText, getAllByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1" open>
          <p>Test content 1</p>
        </AccordionSection>
        <AccordionSection heading="Test heading 2" open>
          <p>Test content 2</p>
        </AccordionSection>
      </Accordion>,
    );

    const button = getByText('Close all');
    const sections = getAllByText(/Test heading/);

    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    fireEvent.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'false');
  });

  test('clicking on `Close all` causes `onToggleAll` handler to be called with new state', () => {
    const toggleAll = jest.fn();

    const { getByText } = render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading" open>
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    fireEvent.click(getByText('Close all'));

    expect(toggleAll).toHaveBeenCalledWith(false);
  });
});
