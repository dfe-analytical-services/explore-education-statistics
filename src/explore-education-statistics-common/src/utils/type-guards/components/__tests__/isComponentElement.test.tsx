import React from 'react';
import isComponentElement from '../isComponentElement';

describe('isComponentElement', () => {
  const TestClassComponent = () => null;

  const TestFunctionComponent = () => null;

  test('returns true when value is a component class element', () => {
    const node = <TestClassComponent />;
    expect(isComponentElement(node)).toBe(true);
  });

  test('returns true when value is a function component element', () => {
    const node = <TestFunctionComponent />;
    expect(isComponentElement(node)).toBe(true);
  });

  test('returns false when value is not a component', () => {
    expect(isComponentElement('string')).toBe(false);
    expect(isComponentElement(123)).toBe(false);
    expect(isComponentElement(true)).toBe(false);
    expect(isComponentElement(null)).toBe(false);
    expect(isComponentElement(undefined)).toBe(false);
    expect(isComponentElement([])).toBe(false);
    expect(isComponentElement({})).toBe(false);
  });
});
