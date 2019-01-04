import React from 'react';
import { render } from 'react-testing-library';
import Heading, { H1, H2, H3, H4 } from '../Heading';

describe('Heading', () => {
  test('renders `xl` size correctly as h1', () => {
    const { container } = render(<Heading size="xl">Test Heading 1</Heading>);
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders `l` size correctly as h2', () => {
    const { container } = render(<Heading size="l">Test Heading 2</Heading>);
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders `m` size correctly as h3', () => {
    const { container } = render(<Heading size="m">Test Heading 3</Heading>);
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders `s` size correctly as h4', () => {
    const { container } = render(<Heading size="s">Test Heading 4</Heading>);
    expect(container.innerHTML).toMatchSnapshot();
  });
});

describe('H1', () => {
  test('renders correctly as `xl` size', () => {
    const { container } = render(<H1>Test Heading 1</H1>);
    expect(container.innerHTML).toMatchSnapshot();
  });
});

describe('H2', () => {
  test('renders correctly as `l` size', () => {
    const { container } = render(<H2>Test Heading 2</H2>);
    expect(container.innerHTML).toMatchSnapshot();
  });
});

describe('H3', () => {
  test('renders correctly as `m` size', () => {
    const { container } = render(<H3>Test Heading 3</H3>);
    expect(container.innerHTML).toMatchSnapshot();
  });
});

describe('H4', () => {
  test('renders correctly as `xl` size', () => {
    const { container } = render(<H4>Test Heading 4</H4>);
    expect(container.innerHTML).toMatchSnapshot();
  });
});
