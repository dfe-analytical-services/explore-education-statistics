declare class GovUkModule {
  constructor(selector: HTMLElement | null);
  public init(): void;
}

declare module 'govuk-frontend' {
  export const Tabs = GovUkModule;

  export function initAll(): void;
}

declare module 'govuk-frontend/components/checkboxes/checkboxes' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/radios/radios' {
  export default GovUkModule;
}

declare module 'govuk-frontend/components/tabs/tabs' {
  export default GovUkModule;
}
