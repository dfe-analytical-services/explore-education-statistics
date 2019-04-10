import * as React from 'react';
import { FieldProps } from './Field';

export interface FormContext extends FormState {
  /* Function that allows values in the values state to be set */
  setValues: (values: XValues) => void;
  /* Function that validates a field */
  validate: (fieldName: string) => string;
}
/*
 * The context which allows state and functions to be shared with Field.
 * Note that we need to pass createContext a default value which is why undefined is unioned in the type
 */
export const FormContext = React.createContext<FormContext | undefined>(
  undefined,
);

/**
 * Validates whether a field has a value
 * @param {XValues} values - All the field values in the form
 * @param {string} fieldName - The field to validate
 * @returns {string} - The error message
 */
export const required = (values: XValues, fieldName: string): string =>
  values[fieldName] === undefined ||
  values[fieldName] === null ||
  values[fieldName] === ''
    ? 'This must be populated'
    : '';

/**
 * Validates whether a field is a valid email
 * @param {XValues} values - All the field values in the form
 * @param {string} fieldName - The field to validate
 * @returns {string} - The error message
 */
export const isEmail = (values: XValues, fieldName: string): string =>
  values[fieldName] &&
  values[fieldName].search(
    /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/,
  )
    ? 'This must be in a valid email format'
    : '';

/**
 * Validates whether a field is within a certain amount of characters
 * @param {IValues} values - All the field values in the form
 * @param {string} fieldName - The field to validate
 * @param {number} length - The maximum number of characters
 * @returns {string} - The error message
 */
export const maxLength = (
  values: XValues,
  fieldName: string,
  length: number,
): string =>
  values[fieldName] && values[fieldName].length > length
    ? `This can not exceed ${length} characters`
    : '';

export interface Fields {
  [key: string]: FieldProps;
}
interface FormProps {
  /* The http path that the form will be posted to */
  action: string;

  /* The props for all the fields on the form */
  fields: Fields;

  /* A prop which allows content to be injected */
  render: () => React.ReactNode;
}

export interface XValues {
  /* Key value pairs for all the field values with key being the field name */
  [key: string]: any;
}

export interface Errors {
  /* The validation error messages for each field (key is the field name */
  [key: string]: string;
}

export interface FormState {
  /* The field values */
  values: XValues;

  /* The field validation error messages */
  errors: Errors;

  /* Whether the form has been successfully submitted */
  submitSuccess?: boolean;
}

export class Form extends React.Component<FormProps, FormState> {
  constructor(props: FormProps) {
    super(props);

    const errors: Errors = {};
    const values: XValues = {};
    this.state = {
      errors,
      values,
    };
  }

  public validate(fieldName: string): string {
    let newError: string = '';

    if (
      this.props.fields[fieldName] &&
      this.props.fields[fieldName].validation
    ) {
      newError = this.props.fields[fieldName].validation!.rule(
        this.state.values,
        fieldName,
        this.props.fields[fieldName].validation!.args,
      );
    }
    this.state.errors[fieldName] = newError;
    this.setState({
      errors: { ...this.state.errors, [fieldName]: newError },
    });
    return newError;
  }

  public setValues(values: XValues) {
    this.setState({ values: { ...this.state.values, ...values } });
  }

  /**
   * Returns whether there are any errors in the errors object that is passed in
   * @param {Errors} errors - The field errors
   * @returns {boolean} - Whether there are any errors
   */
  private haveErrors(errors: Errors) {
    let haveError: boolean = false;
    Object.keys(errors).map((key: string) => {
      if (errors[key].length > 0) {
        haveError = true;
      }
    });
    return haveError;
  }

  /**
   * Handles form submission
   * @param {React.FormEvent<HTMLFormElement>} e - The form event
   */
  private handleSubmit = async (
    e: React.FormEvent<HTMLFormElement>,
  ): Promise<void> => {
    e.preventDefault();

    if (this.validateForm()) {
      const submitSuccess: boolean = await this.submitForm();
      this.setState({ submitSuccess });
    }
  };

  /**
   * Executes the validation rules for all the fields on the form and sets the error state
   * @returns {boolean} - Returns true if the form is valid
   */
  private validateForm(): boolean {
    const errors: Errors = {};
    Object.keys(this.props.fields).map((fieldName: string) => {
      errors[fieldName] = this.validate(fieldName);
    });
    this.setState({ errors });
    return !this.haveErrors(errors);
  }

  /**
   * Submits the form to the http api
   * @returns {boolean} - Whether the form submission was successful or not
   */
  private async submitForm(): Promise<boolean> {
    try {
      const response = await fetch(this.props.action, {
        body: JSON.stringify(this.state.values),
        headers: new Headers({
          Accept: 'application/json',
          'Content-Type': 'application/json',
        }),
        method: 'post',
      });
      if (response.status === 400) {
        /* Map any validation errors to IErrors */
        let responseBody: any;
        responseBody = await response.json();
        const errors: Errors = {};
        Object.keys(responseBody).map((key: string) => {
          // For ASP.NET core, the field names are in title case - so convert to camel case
          const fieldName = key.charAt(0).toLowerCase() + key.substring(1);
          errors[fieldName] = responseBody[key];
        });
        this.setState({ errors });
      }
      return response.ok;
    } catch (ex) {
      return false;
    }
  }

  public render() {
    const { submitSuccess, errors } = this.state;
    const context: FormContext = {
      ...this.state,
      setValues: this.setValues,
      validate: this.validate,
    };

    return (
      <FormContext.Provider value={context}>
        <form onSubmit={this.handleSubmit} noValidate={true}>
          <div className="govuk-form-group">
            {this.props.render()}
            <div className="form-group">
              <button
                type="submit"
                className="govuk-button"
                disabled={this.haveErrors(errors)}
              >
                Submit
              </button>
            </div>
            {submitSuccess && (
              <div className="alert alert-info" role="alert">
                The form was successfully submitted!
              </div>
            )}
            {submitSuccess === false && !this.haveErrors(errors) && (
              <div className="alert alert-danger" role="alert">
                Sorry, an unexpected error has occurred
              </div>
            )}
            {submitSuccess === false && this.haveErrors(errors) && (
              <div className="alert alert-danger" role="alert">
                Sorry, the form is invalid. Please review, adjust and try again
              </div>
            )}
          </div>
        </form>
      </FormContext.Provider>
    );
  }
}
