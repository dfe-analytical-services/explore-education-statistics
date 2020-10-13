/* eslint-disable max-classes-per-file */
declare class GovUkModule {
  public constructor(selector: HTMLElement | null);

  public init(): void;
}

declare module 'govuk-frontend' {
  export const Accordion = GovUkModule;
  export const Checkboxes = GovUkModule;
  export const Details = GovUkModule;
  export const ErrorSummary = GovUkModule;
  export const Radios = GovUkModule;
  export const Tabs = GovUkModule;

  export function initAll(): void;
}

declare module 'govuk-frontend/govuk/components/accordion/accordion' {
  export default GovUkModule;
}

declare module 'govuk-frontend/govuk/components/checkboxes/checkboxes' {
  export default GovUkModule;
}

declare module 'govuk-frontend/govuk/components/details/details' {
  export class Details extends GovUkModule {
    public setAttributes(): void;
  }

  export default Details;
}

declare module 'govuk-frontend/govuk/components/error-summary/error-summary' {
  export class ErrorSummary extends GovUkModule {
    public handleClick(event: MouseEvent): void;
  }

  export default ErrorSummary;
}

declare module 'govuk-frontend/govuk/components/radios/radios' {
  export default GovUkModule;
}

declare module 'govuk-frontend/govuk/components/tabs/tabs' {
  export default GovUkModule;
}

declare module '*.json' {
  const value: Record<string, unknown>;
  export default value;
}
