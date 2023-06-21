import FormNumberInput from '@common/components/form/FormNumberInput';
import { render } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';

describe('FormNumberInput', () => {
  test('renders correctly with minimal props', () => {
    const { container } = render(
      <FormNumberInput
        id="test-input"
        label="Test input"
        name="test"
        onChange={noop}
      />,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('sets an empty `value` attribute if `value` prop is undefined', () => {
    const { getByLabelText } = render(
      <FormNumberInput
        id="test-input"
        label="Test input"
        name="test"
        onChange={noop}
      />,
    );

    expect(getByLabelText('Test input')).toHaveAttribute('value', '');
  });

  test('sets `value` attribute to 0 if `value` prop is 0', () => {
    const { getByLabelText } = render(
      <FormNumberInput
        id="test-input"
        label="Test input"
        name="test"
        value={0}
        onChange={noop}
      />,
    );

    expect(getByLabelText('Test input')).toHaveAttribute('value', '0');
  });
});
