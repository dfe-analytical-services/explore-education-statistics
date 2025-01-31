export interface ProblemDetails {
  /**
   * A short, human-readable summary of the problem type.
   */
  title: string;
  /**
   * A URI reference [RFC3986] that identifies the problem type.
   */
  type: string;
  /**
   * The HTTP status code.
   */
  status: number;
  /**
   * A human-readable explanation specific to this occurrence of the problem.
   */
  detail?: string;
}

export interface ValidationProblemDetails<TCode extends string = string>
  extends ProblemDetails {
  /**
   * Errors associated with the validation problem.
   */
  errors: ValidationProblemError<TCode>[];
}

export interface ValidationProblemError<TCode extends string = string> {
  /**
   * The error message.
   */
  message: string;
  /**
   * The path to the property on the request that the error relates to.
   * May be omitted or be empty if no specific property of the
   * request relates to the error (it is a 'global' error).
   */
  path?: string;
  /**
   * The error's machine-readable code. Use this for localisation
   * or general processing when presenting the error to users.
   * May be omitted if there is none.
   */
  code?: TCode;
  /**
   * Additional detail about the error that can be used to provide
   * more context to users. May be omitted if there is none.
   */
  detail?: unknown;
}
