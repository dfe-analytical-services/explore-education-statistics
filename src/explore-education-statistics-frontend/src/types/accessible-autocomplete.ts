declare module 'accessible-autocomplete/react' {
  import React from 'react';

  /**
   * Type for a single suggestion item in accessible-autocomplete.
   * It can be a string or an object, depending on the source data and `templates.inputValue`.
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  type AutocompleteSuggestion = string | object | any;

  /**
   * Defines custom template functions for accessible-autocomplete.
   */
  interface AutocompleteTemplates {
    /**
     * A function that returns the string to be displayed in the input field when a suggestion is selected.
     * If your `source` provides objects, this function tells the autocomplete how to convert
     * that object into the text shown in the input.
     * @param result The selected suggestion item (string or object).
     * @returns The string to display in the input.
     */
    inputValue?: (result: AutocompleteSuggestion) => string;
    /**
     * A function that returns the HTML string for a suggestion as it appears in the dropdown menu.
     * This allows for rich HTML formatting of suggestions.
     * @param result The suggestion item (string or object).
     * @returns The HTML string for the suggestion.
     */
    suggestion?: (result: AutocompleteSuggestion) => string;
  }

  /**
   * Type for the dropdown arrow SVG generator.
   * This function receives a configuration object and should return an SVG string.
   */
  type DropdownArrowRenderer = (config: { className: string }) => string;

  /**
   * Defines the source of suggestions for the autocomplete.
   * Can be:
   * - An array of strings or objects.
   * - A synchronous function that takes a query and a `populateResults` callback.
   * - An asynchronous function that takes a query and returns a Promise resolving to suggestions.
   */
  type AutocompleteSource =
    | AutocompleteSuggestion[]
    | ((
        query: string,
        populateResults: (suggestions: AutocompleteSuggestion[]) => void,
      ) => void)
    | ((query: string) => Promise<AutocompleteSuggestion[]>);

  /**
   * Options for configuring the `AccessibleAutocomplete` React component.
   * These largely mirror the options for the core `accessible-autocomplete` library.
   */
  interface AccessibleAutocompleteProps {
    /**
     * The id to assign to the autocomplete input field,
     * to use with a <label for=id>.
     * Not required if using enhanceSelectElement
     */
    id?: string;
    /**
     * The source of suggestions for the autocomplete.
     */
    source: AutocompleteSource;
    /**
     * The minimum number of characters the user must type before suggestions are shown.
     * Defaults to `0`.
     */
    minLength?: number;
    /**
     * The debounce delay in milliseconds for the `source` function.
     * Suggestions will only be fetched after the user stops typing for this duration.
     * Defaults to `0`.
     */
    debounce?: number;
    /**
     * If `true`, the first suggestion will be automatically highlighted when the dropdown appears.
     * Defaults to `false`.
     */
    autoselect?: boolean;
    /**
     * The initial value of the input field.
     */
    defaultValue?: string;
    /**
     * Controls how the dropdown menu is displayed.
     * - `'inline'`: The dropdown appears directly below the input.
     * - `'overlay'`: The dropdown appears as an overlay over other content.
     * Defaults to `'inline'`.
     */
    displayMenu?: 'inline' | 'overlay';
    /**
     * Sets html attributes and their values on the generated ul menu element.
     * Useful for adding aria-labelledby and setting to the value of the id attribute on your existing label,to provide context to an assistive technology user.
     */
    menuAttributes?: Record<string, string | boolean>;
    /**
     * Adds custom html classes to the generated ul menu element.
     */
    menuClasses?: string;
    /**
     * Adds custom html classes to the generated input element.
     */
    inputClasses?: string;
    /**
     * Adds custom html classes to the additional input element that appears
     * when what the user typed matches the start of a suggestion.
     */
    hintClasses?: string;
    /**
     * A callback function that is triggered when a suggestion is confirmed (selected by user or on blur).
     * @param query The confirmed suggestion (string or object from the source), or `undefined` if nothing was selected.
     */
    onConfirm?: (query: AutocompleteSuggestion | undefined) => void;
    /**
     * If `true`, `onConfirm` will be triggered when the input field loses focus,
     * even if no suggestion was explicitly selected.
     * Defaults to `false`.
     */
    confirmOnBlur?: boolean;
    /**
     * Custom template functions to control how suggestions are displayed and how the input value is set.
     */
    templates?: AutocompleteTemplates;
    /**
     * If `true` and the `source` is an array, all values will be shown in the dropdown immediately
     * when the input is focused, without needing to type.
     * Defaults to `false`.
     */
    showAllValues?: boolean;
    /**
     * The autocomplete will display a "No results found" template when there are no results.
     * Defaults to `false`.
     */
    showNoOptionsFound?: boolean;
    /**
     * A function that returns the SVG string for the dropdown arrow icon.
     * Useful for custom arrow icons.
     * @param config Configuration object, currently only includes `className`.
     * @returns An SVG string.
     */
    dropdownArrow?: DropdownArrowRenderer;
    /**
     * A function that returns the text to display when no results are found.
     * Defaults to a predefined message.
     */
    tNoResults?: () => string;
    /**
     * A function that returns the assistive hint text for screen readers.
     * Defaults to a predefined string.
     */
    tAssistiveHint?: () => string;
    /**
     * The `name` attribute for the generated hidden input field that typically holds the confirmed value.
     */
    name?: string;
    /**
     * The `placeholder` attribute for the generated text input field.
     */
    placeholder?: string;
    /**
     * Optional `className` to apply to the top-level autocomplete wrapper div.
     */
    className?: string;
  }

  /**
   * The Accessible Autocomplete React component.
   * This component wraps the core accessible-autocomplete library to provide a React-friendly interface.
   */
  const AccessibleAutocomplete: React.FC<AccessibleAutocompleteProps>;

  export default AccessibleAutocomplete;
}
