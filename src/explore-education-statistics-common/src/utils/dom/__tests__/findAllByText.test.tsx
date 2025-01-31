import { render, screen } from '@testing-library/react';
import React from 'react';
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

  test('can match on lowercase text', () => {
    render(
      <div>
        <p className="test-1">Test</p>
        <p className="test-2">Test</p>
      </div>,
    );

    expect(
      findAllByText('test', 'p', {
        useLowerCase: false,
      }),
    ).toHaveLength(0);
    expect(
      findAllByText('test', 'p', {
        useLowerCase: true,
      }),
    ).toHaveLength(2);
  });

  test('can match element within specific root element', () => {
    render(
      <div>
        <div data-testid="container-1">
          <p>Test 1</p>
        </div>
        <div data-testid="container-2">
          <p>Test 2</p>
        </div>
      </div>,
    );

    const elements = findAllByText('Test', 'p', {
      rootElement: screen.getByTestId('container-2'),
    });

    expect(elements).toHaveLength(1);
    expect(elements[0]).toHaveTextContent('Test 2');
  });
});
