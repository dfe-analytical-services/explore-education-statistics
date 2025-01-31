import React from 'react';
import { render } from '@testing-library/react';
import FormattedDate from '../FormattedDate';

describe('FormattedDate', () => {
  describe('using default format', () => {
    test('renders correctly with string date', () => {
      const { container } = render(<FormattedDate>2019-03-12</FormattedDate>);

      expect(container.textContent).toContain('12 March 2019');
    });

    test('renders correctly with number date', () => {
      const { container } = render(
        <FormattedDate>{1552395197000}</FormattedDate>,
      );

      expect(container.textContent).toContain('12 March 2019');
    });

    test('renders correctly with Date', () => {
      const { container } = render(
        <FormattedDate>{new Date('2019-03-12')}</FormattedDate>,
      );

      expect(container.textContent).toContain('12 March 2019');
    });
  });

  describe('using custom format', () => {
    test('renders correctly with string date', () => {
      const { container } = render(
        <FormattedDate format="yy MM eee">2019-03-12</FormattedDate>,
      );

      expect(container.textContent).toContain('19 03 Tue');
    });

    test('renders correctly with number date', () => {
      const { container } = render(
        <FormattedDate format="yy MM eee">{1552395197000}</FormattedDate>,
      );

      expect(container.textContent).toContain('19 03 Tue');
    });

    test('renders correctly with Date', () => {
      const { container } = render(
        <FormattedDate format="yy MM eee">
          {new Date('2019-03-12')}
        </FormattedDate>,
      );

      expect(container.textContent).toContain('19 03 Tue');
    });
  });

  describe('invalid date', () => {
    test('does not render with string date', () => {
      const { container } = render(<FormattedDate>not a date</FormattedDate>);

      expect(container.textContent).toBe('');
    });

    test('does not render with Date', () => {
      const { container } = render(
        <FormattedDate>{new Date('not a date')}</FormattedDate>,
      );

      expect(container.textContent).toBe('');
    });
  });
});
