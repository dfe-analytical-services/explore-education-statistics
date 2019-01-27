import React from 'react';
import { fireEvent, render } from 'react-testing-library';
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
    const { container, getByText } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">
          <p>Test content</p>
        </AccordionSection>
      </Accordion>,
    );

    const heading = container.querySelector(
      '#test-sections-heading-1',
    ) as HTMLElement;

    expect(heading).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(getByText('Test heading'));

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
});
