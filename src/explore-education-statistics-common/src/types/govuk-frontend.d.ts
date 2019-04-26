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

declare module 'govuk-frontend/components/accordion/accordion' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/checkboxes/checkboxes' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/details/details' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/error-summary/error-summary' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/radios/radios' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/tabs/tabs' {
  export default GovUkModule;
}

declare module '*.json' {
  const value: object;
  export default value;
}
