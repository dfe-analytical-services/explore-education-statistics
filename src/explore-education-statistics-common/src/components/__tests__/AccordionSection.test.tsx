import AccordionSection from '@common/components/AccordionSection';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('AccordionSection', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <AccordionSection heading="Test heading">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(
      screen.getByRole('heading', { level: 2, name: 'Test heading' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Test content')).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with different heading size', () => {
    const { container } = render(
      <AccordionSection heading="Test heading" headingTag="h3">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(screen.queryByRole('heading', { level: 2 })).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { level: 3 })).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with caption', () => {
    const { container } = render(
      <AccordionSection heading="Test heading" caption="Some caption text">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(screen.getByText('Some caption text')).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('adds an id to the heading', () => {
    render(
      <AccordionSection heading="Test heading">
        <p>Test content</p>
      </AccordionSection>,
    );
    expect(
      screen.getByRole('heading', { name: 'Test heading' }),
    ).toHaveAttribute('id', 'section-test-heading');
  });

  test('adds a copy link button when anchorLinkUrl is set', () => {
    const testUrl = 'http://test.com/1#test-heading';
    render(
      <AccordionSection anchorLinkUrl={() => testUrl} heading="Test heading">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    ).toBeInTheDocument();
  });

  test('does not add  a copy link button when anchorLinkUrl is undefined', () => {
    render(
      <AccordionSection heading="Test heading">
        <p>Test content</p>
      </AccordionSection>,
    );

    expect(
      screen.queryByRole('button', { name: 'Copy link to the clipboard' }),
    ).not.toBeInTheDocument();
  });
});
