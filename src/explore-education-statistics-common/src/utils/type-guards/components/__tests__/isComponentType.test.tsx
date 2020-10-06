import React, { Component } from 'react';
import isComponentType from '../isComponentType';

describe('isComponentType', () => {
  // eslint-disable-next-line react/prefer-stateless-function
  class TestClassComponent extends Component {
    public render(): React.ReactNode {
      return null;
    }
  }

  const TestFunctionComponent = () => null;

  test('returns true when value matches class component type', () => {
    const node = <TestClassComponent />;
    expect(isComponentType(node, TestClassComponent)).toBe(true);
  });

  test('returns true when value matches function component type', () => {
    const node = <TestFunctionComponent />;
    expect(isComponentType(node, TestFunctionComponent)).toBe(true);
  });

  test('returns false when value is not a component', () => {
    expect(isComponentType('string', TestClassComponent)).toBe(false);
    expect(isComponentType(123, TestClassComponent)).toBe(false);
    expect(isComponentType(true, TestClassComponent)).toBe(false);
    expect(isComponentType(null, TestClassComponent)).toBe(false);
    expect(isComponentType(undefined, TestClassComponent)).toBe(false);
    expect(isComponentType([], TestClassComponent)).toBe(false);
    expect(isComponentType({}, TestClassComponent)).toBe(false);
  });
});
