import React from 'react';
import { render } from 'react-testing-library';
import findAllByText from '../findAllByText';

describe('findAllByText', () => {
  test('matches multiple elements', () => {
    render(
      <div>
        <p>Test 1</p>
        <p>Test 2</p>
      </div>,
    );

    const elements = findAllByText('Test', 'p');

    expect(elements).toHaveLength(2);
    expect(elements[0]).toHaveTextContent('Test 1');
    expect(elements[1]).toHaveTextContent('Test 2');
  });

  test('matches single element', () => {
    render(
      <div>
        <p>Test 1</p>
        <p>Test 2</p>
      </div>,
    );

    const elements = findAllByText('Test 1', 'p');

    expect(elements).toHaveLength(1);
    expect(elements[0]).toHaveTextContent('Test 1');
  });

  test('matches element by class', () => {
    render(
      <div>
        <p className="test-1">Test</p>
        <p className="test-2">Test</p>
      </div>,
    );

    const elements = findAllByText('Test', '.test-1');

    expect(elements).toHaveLength(1);
    expect(elements[0]).toHaveClass('test-1');
  });

  test('matches no elements', () => {
    render(
      <div>
        <p className="test-1">Test</p>
        <p className="test-2">Test</p>
      </div>,
    );

    const elements = findAllByText('Test', '.test-3');

    expect(elements).toHaveLength(0);
  });
});
