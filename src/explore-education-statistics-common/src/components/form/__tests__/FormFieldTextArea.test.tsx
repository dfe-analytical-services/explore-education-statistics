import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormProvider from '@common/components/form/FormProvider';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('FormFieldTextArea', () => {
  describe('maxLength', () => {
    test('shows a character count message when `maxLength` is above 0', () => {
      render(
        <FormProvider>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={10}
          />
        </FormProvider>,
      );

      expect(
        screen.getByText('You have 10 characters remaining'),
      ).toBeInTheDocument();
    });

    test('aria-describedby contains the character count message id when `maxLength` is above 0', () => {
      render(
        <FormProvider>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={10}
          />
        </FormProvider>,
      );

      const ariaDescribedBy = screen
        .getByLabelText('Test input')
        .getAttribute('aria-describedby');

      expect(
        screen.getByText('You have 10 characters remaining'),
      ).toHaveAttribute('id', 'test-input-info');
      expect(ariaDescribedBy).toContain('test-input-info');
    });

    test('does not show a character count message when `maxLength` is below 0', () => {
      render(
        <FormProvider>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={-1}
          />
        </FormProvider>,
      );

      expect(
        screen.queryByText(/You have .+ characters remaining/),
      ).not.toBeInTheDocument();
    });

    test('does not show a character count message when `maxLength` is 0', () => {
      render(
        <FormProvider>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={0}
          />
        </FormProvider>,
      );

      expect(
        screen.queryByText(/You have .+ characters remaining/),
      ).not.toBeInTheDocument();
    });

    test('shows correct character count message when difference to `maxLength` is 1', () => {
      render(
        <FormProvider initialValues={{ testInput: 'aaa' }}>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={4}
            onChange={noop}
          />
        </FormProvider>,
      );

      expect(
        screen.getByText('You have 1 character remaining'),
      ).toBeInTheDocument();
    });

    test('shows correct character count message when difference to `maxLength` is 0', () => {
      render(
        <FormProvider initialValues={{ testInput: 'aaaa' }}>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={4}
            onChange={noop}
          />
        </FormProvider>,
      );

      expect(
        screen.getByText('You have 0 characters remaining'),
      ).toBeInTheDocument();
    });

    test('shows correct character count message when difference to `maxLength` is -1', () => {
      render(
        <FormProvider initialValues={{ testInput: 'aaaaa' }}>
          <FormFieldTextArea
            id="test-input"
            label="Test input"
            name="testInput"
            maxLength={4}
            onChange={noop}
          />
        </FormProvider>,
      );

      expect(
        screen.getByText('You have 1 character too many'),
      ).toBeInTheDocument();
    });
  });
});
