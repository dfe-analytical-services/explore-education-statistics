import React from 'react';
import { render } from 'react-testing-library';
import Form from '../Form';

describe('Form', () => {
  test('renders error summary from form errors', () => {
    const { container, getByText } = render(
      <Form
        id="test-form"
        errors={{
          firstName: 'First name is required',
          lastName: 'Last name is required',
        }}
        touched={{
          firstName: true,
          lastName: true,
        }}
      >
        The form
      </Form>,
    );

    expect(getByText('First name is required')).toHaveAttribute(
      'href',
      '#test-form-firstName',
    );
    expect(getByText('Last name is required')).toHaveAttribute(
      'href',
      '#test-form-lastName',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render errors for fields that have not been touched', () => {
    const { container, queryByText } = render(
      <Form
        id="test-form"
        errors={{
          firstName: 'First name is required',
          lastName: 'Last name is required',
        }}
        touched={{
          firstName: true,
          lastName: false,
        }}
      >
        The form
      </Form>,
    );

    expect(queryByText('First name is required')).not.toBeNull();
    expect(queryByText('Last name is required')).toBeNull();

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders nested error messages', () => {
    const { getByText } = render(
      <Form
        id="test-form"
        errors={{
          address: {
            line1: 'Line 1 of address is required',
          },
        }}
        touched={{
          address: {
            line1: true,
          },
        }}
      >
        The form
      </Form>,
    );

    expect(getByText('Line 1 of address is required')).toHaveAttribute(
      'href',
      '#test-form-addressLine1',
    );
  });

  test('does not render nested error messages for fields that have not been touched', () => {
    const { queryByText } = render(
      <Form
        id="test-form"
        errors={{
          address: {
            line1: 'Line 1 of address is required',
          },
        }}
        touched={{
          address: {
            line1: false,
          },
        }}
      >
        The form
      </Form>,
    );

    expect(queryByText('Line 1 of address is required')).toBeNull();
  });

  test('renders other errors alongside form errors', () => {
    const { container, getByText } = render(
      <Form
        id="test-form"
        errors={{
          firstName: 'First name is required',
          lastName: 'Last name is required',
        }}
        touched={{
          firstName: true,
          lastName: true,
        }}
        otherErrors={[{ id: 'submit-error', message: 'Something went wrong' }]}
      >
        The form
      </Form>,
    );

    expect(getByText('First name is required')).toHaveAttribute(
      'href',
      '#test-form-firstName',
    );
    expect(getByText('Last name is required')).toHaveAttribute(
      'href',
      '#test-form-lastName',
    );
    expect(getByText('Something went wrong')).toHaveAttribute(
      'href',
      '#submit-error',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
