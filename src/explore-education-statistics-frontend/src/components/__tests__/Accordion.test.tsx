import React, { ComponentClass } from 'react';
import { MemoryRouter } from 'react-router';
import { fireEvent, render } from 'react-testing-library';
import AccordionWithRouter, { AccordionProps } from '../Accordion';
import AccordionSection from '../AccordionSection';

const Accordion: ComponentClass<AccordionProps> = (AccordionWithRouter as any)
  .WrappedComponent;

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
      container.querySelector('govuk-accordion__section--expanded'),
    ).toBeNull();
    expect(container.querySelector('#test-sections-heading-1')).toHaveAttribute(
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
      container.querySelector('govuk-accordion__section--expanded'),
    ).toBeDefined();
    expect(container.querySelector('#test-sections-heading-1')).toHaveAttribute(
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

    expect(headings[0]).toHaveAttribute('id', 'test-sections-heading-1');
    expect(contents[0]).toHaveAttribute('id', 'test-sections-content-1');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-heading-2');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-content-2');
  });

  test('scrolls to and opens section if location hash matches section heading ID', () => {
    const { getByText } = render(
      <MemoryRouter
        initialEntries={[{ pathname: '/', hash: '#test-sections-heading-1' }]}
      >
        <AccordionWithRouter id="test-sections">
          <AccordionSection heading="Test heading 1">
            Test content 1
          </AccordionSection>
          <AccordionSection heading="Test heading 2">
            Test content 2
          </AccordionSection>
        </AccordionWithRouter>
      </MemoryRouter>,
    );

    const heading = getByText('Test heading 1');

    expect(heading).toHaveAttribute('aria-expanded', 'true');
    expect(heading.scrollIntoView).toHaveBeenCalled();
  });

  test('scrolls to and opens section if location hash matches section content ID', () => {
    const { container, getByText } = render(
      <MemoryRouter
        initialEntries={[{ pathname: '/', hash: '#test-sections-content-1' }]}
      >
        <AccordionWithRouter id="test-sections">
          <AccordionSection heading="Test heading 1">
            Test content 1
          </AccordionSection>
          <AccordionSection heading="Test heading 2">
            Test content 2
          </AccordionSection>
        </AccordionWithRouter>
      </MemoryRouter>,
    );

    expect(getByText('Test heading 1')).toHaveAttribute(
      'aria-expanded',
      'true',
    );

    const content = container.querySelector(
      '#test-sections-content-1',
    ) as HTMLElement;

    expect(content.scrollIntoView).toHaveBeenCalled();
  });
});
