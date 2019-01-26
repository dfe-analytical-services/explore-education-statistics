import React from 'react';
import { render } from 'react-testing-library';
import AccordionSection from '../AccordionSection';

describe('AccordionSection', () => {
  test('renders correctly with required props', () => {
    const { container, getByText } = render(
      <AccordionSection heading="Test heading">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(getByText('Test heading')).toBeDefined();
    expect(getByText('Test content')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with different heading size', () => {
    const { container } = render(
      <AccordionSection heading="Test heading" headingTag="h3">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(container.querySelector('h2')).toBeNull();
    expect(container.querySelector('h3')).not.toBeNull();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with caption', () => {
    const { container, getByText } = render(
      <AccordionSection heading="Test heading" caption="Some caption text">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(getByText('Some caption text')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });
});
