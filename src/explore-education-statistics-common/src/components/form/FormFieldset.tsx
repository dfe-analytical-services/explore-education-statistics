import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import classNames from 'classnames';
import React, { FocusEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import FormGroup from './FormGroup';

export interface FormFieldsetProps {
  children?: ReactNode;
  className?: string;
  error?: string;
  hint?: string | ReactNode;
  id: string;
  legend: ReactNode | string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  legendWeight?: 'strong' | 'regular';
  legendHidden?: boolean;
  /**
   * Set to false to disable default prefixing
   * of `id` with the current form context's form id.
   */
  useFormId?: boolean;
  onBlur?: FocusEventHandler<HTMLFieldSetElement>;
  onFocus?: FocusEventHandler<HTMLFieldSetElement>;
}

const FormFieldset = ({
  children,
  className,
  error,
  hint,
  id,
  legend,
  legendSize = 'l',
  legendWeight = 'strong',
  legendHidden = false,
  useFormId = true,
  onBlur,
  onFocus,
}: FormFieldsetProps) => {
  const { prefixFormId } = useFormIdContext();
  const fieldId = useFormId ? prefixFormId(id) : id;

  return (
    <FormGroup hasError={!!error}>
      <fieldset
        aria-describedby={
          classNames({
            [`${fieldId}-error`]: !!error,
            [`${fieldId}-hint`]: !!hint,
          }) || undefined
        }
        className={classNames('govuk-fieldset', className)}
        id={fieldId}
        onBlur={onBlur}
        onFocus={onFocus}
      >
        <legend
          className={classNames(
            'govuk-fieldset__legend',
            `govuk-fieldset__legend--${legendSize}`,
            {
              'govuk-visually-hidden': legendHidden,
              'govuk-!-font-weight-regular': legendWeight === 'regular',
            },
          )}
        >
          {legend}
        </legend>

        {hint && (
          <span className="govuk-hint" id={`${fieldId}-hint`}>
            {hint}
          </span>
        )}

        {error && <ErrorMessage id={`${fieldId}-error`}>{error}</ErrorMessage>}

        {children}
      </fieldset>
    </FormGroup>
  );
};

export default FormFieldset;
