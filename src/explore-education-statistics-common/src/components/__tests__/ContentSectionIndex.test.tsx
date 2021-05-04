import ContentSectionIndex from '@common/components/ContentSectionIndex';
import { render, screen } from '@testing-library/react';
import React, { useRef } from 'react';

describe('ContentSectionIndex', () => {
  test('renders links to headings with ids', () => {
    const TestComponent = () => {
      const contentRef = useRef<HTMLDivElement>(null);

      return (
        <div>
          <ContentSectionIndex id="content-section" contentRef={contentRef} />

          <div ref={contentRef}>
            <h3 id="test-1">Test 1</h3>
            <h3 id="test-2">Test 2</h3>
            <h4 id="test-3">Test 3</h4>
          </div>
        </div>
      );
    };

    render(<TestComponent />);

    expect(screen.getAllByRole('link')).toHaveLength(3);

    expect(
      screen.getByRole('link', {
        name: 'Test 1',
      }),
    ).toHaveAttribute('href', '#test-1');

    expect(
      screen.getByRole('link', {
        name: 'Test 2',
      }),
    ).toHaveAttribute('href', '#test-2');

    expect(
      screen.getByRole('link', {
        name: 'Test 3',
      }),
    ).toHaveAttribute('href', '#test-3');
  });

  test('renders links to headings using custom selector', () => {
    const TestComponent = () => {
      const contentRef = useRef<HTMLDivElement>(null);

      return (
        <div>
          <ContentSectionIndex
            id="content-section"
            contentRef={contentRef}
            selector="[data-test-me='true']"
          />

          <div ref={contentRef}>
            <h3 id="test-1">Test 1</h3>
            <h3 id="test-2" data-test-me="true">
              Test 2
            </h3>
            <h4 id="test-3">Test 3</h4>
          </div>
        </div>
      );
    };

    render(<TestComponent />);

    expect(screen.getAllByRole('link')).toHaveLength(1);

    expect(
      screen.getByRole('link', {
        name: 'Test 2',
      }),
    ).toHaveAttribute('href', '#test-2');
  });

  test('renders links to headings with auto-generated ids', () => {
    const TestComponent = () => {
      const contentRef = useRef<HTMLDivElement>(null);

      return (
        <div>
          <ContentSectionIndex id="content-section" contentRef={contentRef} />

          <div ref={contentRef}>
            <h3>1. Test 1</h3>
            <h3>2.1 Test 2</h3>
            <h4>Test 3 </h4>
            <h4>Test 4</h4>
          </div>
        </div>
      );
    };

    render(<TestComponent />);

    expect(screen.getAllByRole('link')).toHaveLength(4);

    expect(
      screen.getByRole('link', {
        name: '1. Test 1',
      }),
    ).toHaveAttribute('href', '#content-section-1');

    expect(
      screen.getByRole('link', {
        name: '2.1 Test 2',
      }),
    ).toHaveAttribute('href', '#content-section-2');

    expect(
      screen.getByRole('link', {
        name: 'Test 3',
      }),
    ).toHaveAttribute('href', '#content-section-3');

    expect(
      screen.getByRole('link', {
        name: 'Test 4',
      }),
    ).toHaveAttribute('href', '#content-section-4');
  });
});
