import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import Accordion from '../Accordion';
import AccordionSection from '../AccordionSection';

describe('Accordion', () => {
  test('renders with hidden content by default', async () => {
    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    await wait();

    expect(
      container.querySelector('govuk-accordion__section--expanded'),
    ).toBeNull();
    expect(container.querySelector('#test-sections-heading-1')).toHaveAttribute(
      'aria-expanded',
      'false',
    );

    expect(container).toMatchSnapshot();
  });

  test('renders with visible content when `open` is true', async () => {
    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading" open>
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    await wait();

    expect(
      container.querySelector('govuk-accordion__section--expanded'),
    ).toBeDefined();
    expect(container.querySelector('#test-sections-heading-1')).toHaveAttribute(
      'aria-expanded',
      'true',
    );

    expect(container).toMatchSnapshot();
  });

  test('clicking heading makes the hidden content visible', async () => {
    const { getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    await wait();

    const heading = getByText('Test heading');

    expect(heading).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('generates section IDs in correct order', async () => {
    const { getAllByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    await wait();

    const headings = getAllByText('Test heading');
    const contents = getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'test-sections-heading-1');
    expect(contents[0]).toHaveAttribute('id', 'test-sections-content-1');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-heading-2');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-content-2');
  });

  test('scrolls to and opens section if location hash matches section heading ID', async () => {
    location.hash = '#test-sections-heading-1';

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

    await wait();

    const heading = getByText('Test heading 1');

    expect(heading).toHaveAttribute('aria-expanded', 'true');
    expect(heading.scrollIntoView).toHaveBeenCalled();
  });

  test('scrolls to and opens section if location hash matches section content ID', async () => {
    location.hash = '#test-sections-heading-1';

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

    await wait();

    expect(getByText('Test heading 1')).toHaveAttribute(
      'aria-expanded',
      'true',
    );

    const content = container.querySelector(
      '#test-sections-content-1',
    ) as HTMLElement;

    expect(content.scrollIntoView).toHaveBeenCalled();
  });

  test('scrolls to and opens section if location hash matches an element in the section content', async () => {
    location.hash = '#test-heading';

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

    await wait();

    expect(getByText('Test heading 2')).toHaveAttribute(
      'aria-expanded',
      'true',
    );

    const element = container.querySelector('#test-heading') as HTMLElement;

    expect(element.scrollIntoView).toHaveBeenCalled();
  });
});
