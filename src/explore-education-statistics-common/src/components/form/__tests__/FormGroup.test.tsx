import React from 'react';
import { render } from '@testing-library/react';
import FormGroup from '../FormGroup';

describe('FormGroup', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormGroup>
        <p>Some content</p>
      </FormGroup>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly when there is an error', () => {
    const { container } = render(
      <FormGroup hasError>
        <p>Some content</p>
      </FormGroup>,
    );

    expect(container.querySelector('.govuk-form-group')).not.toBeNull();
    expect(container.innerHTML).toMatchSnapshot();
  });
});
