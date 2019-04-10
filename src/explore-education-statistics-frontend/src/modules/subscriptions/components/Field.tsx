import * as React from 'react';
import { Errors, FormContext, XValues } from './Form';

/* The available editors for the field */
type Editor = 'textbox' | 'dropdown';

export interface Validation {
  rule: (values: XValues, fieldName: string, args: any) => string;
  args?: any;
}

export interface FieldProps {
  /* The unique field name */
  id: string;

  /* The label text for the field */
  label?: string;

  /* The type of field */
  type?: string;

  /* The editor for the field */
  editor?: Editor;

  /* The drop down items for the field */
  options?: string[];

  /* The field value */
  value?: any;

  /* The field validator function and argument */
  validation?: Validation;
}

export const Field: React<FieldProps> = ({
  id,
  label,
  type,
  editor,
  options,
  value,
}) => {
  const getError = (errors: Errors): string => (errors ? errors[id] : '');

  /**
   * Gets the inline styles for editor
   * @param {Errors} errors - All the errors from the form
   * @returns {any} - The style object
   */
  const getEditorStyle = (errors: Errors): any =>
    getError(errors) ? { borderColor: 'red' } : {};

  return (
    <FormContext.Consumer>
      {(context: FormContext) => (
        <div className="govuk-form-group">
          {label && (
            <label className="govuk-label" htmlFor={id}>
              {label}
            </label>
          )}

          {editor!.toLowerCase() === 'textbox' && (
            <input
              id={id}
              type={type}
              value={value}
              onChange={(e: React.FormEvent<HTMLInputElement>) =>
                context.setValues({ [id]: e.currentTarget.value })
              }
              onBlur={() => context.validate(id)}
              className="govuk-input"
              style={getEditorStyle(context.errors)}
            />
          )}

          {editor!.toLowerCase() === 'dropdown' && (
            <select
              id={id}
              name={id}
              value={value}
              onChange={(e: React.FormEvent<HTMLSelectElement>) =>
                context.setValues({ [id]: e.currentTarget.value })
              }
              onBlur={() => context.validate(id)}
              className="form-control"
              style={getEditorStyle(context.errors)}
            >
              {options &&
                options.map(option => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
            </select>
          )}

          {getError(context.errors) && (
            <div style={{ color: 'red', fontSize: '80%' }}>
              <p>{getError(context.errors)}</p>
            </div>
          )}
        </div>
      )}
    </FormContext.Consumer>
  );
};
Field.defaultProps = {
  editor: 'textbox',
};
