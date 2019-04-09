import * as React from 'react';
import { baseUrl } from "../../../services/api";

interface FormProps {
  /* The http path that the form will be posted to */
  action: string;
}

export interface Values {
  /* Key value pairs for all the field values with key being the field name */
  [key: string]: any;
}

export interface Errors {
  /* The validation error messages for each field (key is the field name */
  [key: string]: string;
}

export interface FormState {
  /* The field values */
  values: Values;

  /* The field validation error messages */
  errors: Errors;

  /* Whether the form has been successfully submitted */
  submitSuccess?: boolean;
}

export class SubscriptionForm extends React.Component<FormProps, FormState> {
  constructor(props: FormProps) {
    super(props);

    const errors: Errors = {};
    const values: Values = {};
    this.state = {
      errors,
      values,
    };
  }

  private haveErrors(errors: Errors) {
    let haveError: boolean = false;
    Object.keys(errors).map((key: string) => {
      if (errors[key].length > 0) {
        haveError = true;
      }
    });
    return haveError;
  }

  private handleSubmit = async (
    e: React.FormEvent<HTMLFormElement>,
  ): Promise<void> => {
    e.preventDefault();

    if (this.validateForm()) {
      const submitSuccess: boolean = await this.submitForm();
      this.setState({ submitSuccess });
    }
  };

  private validateForm(): boolean {
    return true;
  }

  private async submitForm(): Promise<boolean> {
    event.preventDefault();

    const data = {
      'email': this.state.values[0],
      'publication-id': '123',
    };

    if (data.email !== '') {

      const response = await fetch(`${baseUrl.data}/publication/subscribe/`, {
        body: JSON.stringify(data),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
        },
        method: 'POST'
      });

      const json = await response.json();
    }
  }

  public render() {
    const { submitSuccess, errors } = this.state;
    return (
      <form onSubmit={this.handleSubmit} noValidate={true}>
        <div className="container">
          <h3>Your details</h3>

          <div className="govuk-form-group">
            <label className="govuk-label" htmlFor="email">
              Email address
            </label>
            <input
              className="govuk-input"
              id="email"
              name="email"
              type="email"
              aria-describedby="email-hint"
            />
          </div>

          <button
            type="submit"
            className="govuk-button"
            disabled={this.haveErrors(errors)}
          >
            Subscribe
          </button>

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
    );
  }
}

export default SubscriptionForm;
