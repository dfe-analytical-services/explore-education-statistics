import { Path } from '@common/types';

export interface SubmitErrorOptions<TFormValues> {
  /**
   * Specify a form field that this error should
   * be assigned to. The error summary will link
   * to this field.
   */
  field?: Path<TFormValues>;
}

/**
 * Throw this error during form submission to display a custom
 * form submit error, otherwise a fallback message is displayed.
 */
export default class SubmitError<TFormValues> extends Error {
  public readonly field?: Path<TFormValues>;

  constructor(
    error: string | Error,
    options?: SubmitErrorOptions<TFormValues>,
  ) {
    if (error instanceof Error) {
      super(error.message);
    } else {
      super(error);
    }

    this.field = options?.field;
  }
}
