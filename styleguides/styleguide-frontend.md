# Explore Education Statistics - Frontend Style Guide

<details markdown="1">
  <summary>Table of Contents</summary>

- [1 Background](#s1)
- [2 Code style](#s2)
  - [2.1 General](#s2.1)
  - [2.2 Naming, ordering and formatting](#s2.2)
  - [2.3 Hooks](#s2.3)
  - [2.4 Contexts](#s2.4)
  - [2.5 Components](#s2.5)
  - [2.6 Typescript](#s2.6)
  - [2.7 CSS](#s2.7)
- [3 Dependencies](#s3)
  - [3.1 React Hook Forms over Formik](#s3.1)
  - [3.2 TanStack Query over useAsync\*](#s3.2)
  - [3.3 When to use third-party dependencies](#s3.3)
- [4 Accessibility](#s4)
  - [4.1 GOV.UK Design System](#s4.1)
- [5 Tests](#s5)
  - [5.1 What, when and how to test](#s5.1)
  - [5.2 Rendering components in tests](#s5.2)
  - [5.3 Naming and syntax](#s5.3)
  - [5.4 Queries](#s5.4)
  - [5.4 Events](#s5.5)
  - [5.6 Test data](#s5.6)
  - [5.7 Assertions](#s5.7)
  - [5.8 Mocking](#s5.8)
  </details>

<a id="s1"></a>

## 1 Background

This style guide is a list of agreed coding standards to conform to when writing frontend code for this repository.

<a id="s2"></a>

## 2. Code style

<a id="s2.1"></a>

### 2.1. General

<a id="s2.1.1"></a>

#### 2.1.1 Boilerplate / readability versus abstraction / less code to maintain

- Try stick to rule of 3 - see something 3 times before abstracting.
- If there are areas that are suffering from copy-pasta maintenance overhead, then consider abstracting as well.
- If there is a deadlock on abstracting or not, get a third opinion.

<a id="s2.2"></a>

### 2.2 Naming, ordering and formatting

<a id="s2.2.1"></a>

#### 2.2.1 Filename conventions

- Components use PascalCase.
- Functions or other files typically use camelCase.
- Directories are kebab-cased.
- Files are named in a hierarchical way using nouns. Nouns are ordered in specificity i.e. going from less specific nouns to more specific i.e. `CommentAddForm` not `AddCommentForm`.

<a id="s2.2.2"></a>

#### 2.2.2 Variable naming convention

- Use camelCase.
- `test` prefix for test data.
- Avoid SCREAMING_SNAKE_CASE besides for environment variables

<a id="s2.2.3"></a>

#### 2.2.3 Event handlers naming convention

- An event handler is something that should happen in response to a user event e.g. click.
- Prefix event handlers with 'on' as this follows DOM naming conventions, e.g. `onClick`

<a id="s2.2.4"></a>

#### 2.2.4 ID naming convention

- Use hyphens for IDs, e.g. `my-id` not `myId`.
- Don't interpolate variables into ids without using the [lodash kebabCase](https://lodash.com/docs/4.17.15#kebabCase) function as these can include spaces (illegal id syntax).

<a id="s2.2.5"></a>

#### 2.2.5 Align interfaces with backend view models/requests one-to-one

- Interfaces mostly align between backend and frontend, but we don't add suffixes like ViewModel in the frontend.
- If you feel that naming / properties could be aligned better - do some refactoring.

<a id="s2.2.6"></a>

#### 2.2.6 Ordering of props - alphabetised

- It is preferable, but not required, to order props alphabetically, with on\* event handlers ordered beneath other props, e.g.

```
aProp,
bProp,
onA,
onB
```

<a id="s2.2.7"></a>

#### 2.2.7 Number format

- Use underscores in increments of 1000 to make large numbers more readable, e.g. `6_000_000` rather than `6000000`.

<a id="s2.3"></a>

### 2.3 Hooks

<a id="s2.3.1"></a>

#### 2.3.1 useMemo and useCallback

- Use `useMemo` and `useCallback` when necessary. Does not need to be used for consistency of hooks.
- Will be necessary in places such as:
  - Closures
  - Performance sensitive areas.
- Use React DevTools to figure out if excessive rendering is happening.

<a id="s2.3.2"></a>

#### 2.3.2 Custom hooks

- Create hooks (where possible) that provide React wrappers around DOM APIs e.g. tracking mouse scroll position.
- In general, avoid unnecessarily abstracting into new custom hooks unless there's a benefit.
- It may be worth doing in cases where the state is complicated (e.g. chart builder, release and methodology content) or is a re-usable state pattern.
- Create custom hooks for contexts.

<a id="s2.4"></a>

### 2.4 Contexts

<a id="s2.4.1"></a>

#### 2.4.1 Using Context vs prop drilling

- Context should be used sparingly, when it is useful, rather than haphazardly for any situation.
- Prop drilling is fine for the most part - proper typing and prop spreading are encouraged.
- Context would be more useful where there's a lot more global state on a page between lots of components e.g. release context.

<a id="s2.4.2"></a>

#### 2.4.2 Context API conventions e.g. `use\*Context` hook

- Create an exported custom hook called `use*Context` which uses `Context.Consumer`.
- Create an exported Provider component which uses `Context.Provider`.
- Context itself should be private to the module (usually).
- Throw an error in `use*Context` hook if context hasn't been set up correctly (there are some exceptions).

<a id="s2.5"></a>

### 2.5 Components

<a id="s2.5.1"></a>

#### 2.5.1 Component function style

- It is preferable, but not required, to use `export default function() {...}` instead of arrow functions for components.

<a id="s2.5.3"></a>

#### 2.5.3 Ternary vs && in JSX

- Can use &&, but need examples of when to use it or not use it e.g. length checks
- Leave it to personal preference of the implementor in most cases

<a id="s2.5.4"></a>

#### 2.5.4 When to break up a component into smaller components?

- Typically split out form components that just handle form state.
- Split out components where there visible landmarks in the UI e.g. lists, navbars, panels.
- Also split out components if state would be better grouped into a separate component(s) e.g. groups of inputs, dropdowns, etc.
- Create components for re-usable text that is going to be in multiple places i.e. admin and frontend. Example - guidance modals.

<a id="s2.6"></a>

### 2.6 Typescript

<a id="s2.6.1"></a>

#### 2.6.1 Explicit type declarations - return and variables types

- Use explicit types on variables and functions, particularly if declared globally and exported.
- Explicit types should be used as much as possible if declaring object literals / arrays as this avoids potential bugs from changing the structure in the future.
- Implicit types on primitives and class instances are fine.

<a id="s2.7"></a>

### 2.7 CSS

<a id="s2.7.1"></a>

#### 2.7.1 Using [classNames](https://github.com/JedWatson/classnames)

- You don't need to a use classNames with a single class string - this is extra overhead.
- Don't use `&&` for conditional classes - use classNames.

<a id="s2.7.2"></a>

#### 2.7.2 Custom styles

- In general, use the [GOV.UK design system](https://design-system.service.gov.uk/).
- When custom styles are needed add them in a `componentName.module.scss` file in the same folder as the component.
- Import the custom styles as `styles` and use camelCase for the class names, e.g. `styles.myCustomClass`.

<a id="s3"></a>

## 3. Dependencies

<a id="s3.1"></a>

### 3.1 React Hook Forms over Formik

[React Hook Form](https://react-hook-form.com/) (RHF) is preferred over [Formik](https://formik.org/). RHF has is more performant than Formik and Formik is unmaintained.

Work is ongoing to convert existing forms to RHF. New RHF form components that mirror their Formik equivalents are being added to the common repo to aid the conversion.

- Use RHF for new forms
- When editing an existing form consider converting it to RHF (not required).
- Continue to use Yup validation (this may be reviewed in future).

<a id="s3.2"></a>

### 3.2 TanStack Query over useAsync\*

[TanStack Query](https://tanstack.com/query) is preferred over our custom useAsync* hooks. Custom useAsync* hooks were previously used for async, but are no longer required due to useQuery.

- Use Query when adding new async calls.
- When editing a file using a useAsync\* hook consider converting it to use Query.
- We may introduce a pattern to merge multiple queries in the future into one async state

<a id="s3.3"></a>

### 3.3 When to use third-party dependencies

- Things to consider:
  - is there a smaller library?
  - is the bundle size worth it?
  - is the dependency maintained?
  - can it be copied straight into the codebase instead?
  - all the potential alternatives and their pros and cons.
- If in doubt, discuss with the team.

<a id="s4"></a>

## 4. Accessibility

<a id="s4.1"></a>

### 4.1 GOV.UK Design System

- Where possible use the [GOV.UK design system](https://design-system.service.gov.uk/) components, styles and patterns as these have been accessibility tested.
- When custom components are needed these should be designed and tested to ensure they meet the [WCAG 2.2 guidelines](https://www.w3.org/TR/WCAG22/).

<a id="s5"></a>

## 5. Tests

<a id="s5.1"></a>

### 5.1 What, when and how to test

<a id="s5.1.1"></a>

#### 5.1.1 Page vs individual Component tests

- 'Base' or common components should be well tested in a unit test style.
- 'Wrapper' components should only test behaviour they add to base components and their integration.
- Page tests should (mostly) test happy path (and errors if needed).
- Form tests should test everything validates and success, error and loading is correct.

<a id="s5.1.2"></a>

#### 5.1.2 Determining when a Component doesn't need any tests at all

- Textual components that just add copy don't need to be tested.
- If tests for this textual components gets added it's acceptable.
- Some components may use dependencies that can't be tested. Use E2E tests for that instead.

<a id="s5.1.3"></a>

#### 5.1.3 Determining whether to leave particular test cases out as they're covered by Robot tests

- Currently, UI tests only really test happy path.
- Frontend unit tests tend to test happy and unhappy paths as well.
- Prefer to put more detail into frontend unit tests to test different permutations.
- Duplication is acceptable between UI tests and fronted unit tests.

<a id="s5.1.4"></a>

#### 5.1.4 Use of snapshot tests

- Prefer to not use snapshot tests - they tend to let mistakes through as they rely on people checking
  them properly (which is hard).
- If snapshot is small in scope and there's a valid use case (e.g. some HTML), then a snapshot might
  be a suitable tool - up to developer.

<a id="s5.1.5"></a>

#### 5.1.5 Avoid logic and loops in tests

- Prefer to avoid loops and logic in tests - keep things as simple as possible!
- It may be tempting to abstract some repeated assertions on some items, however, it's usually
  simpler, clearer and more precise to just copy and paste the assertions across your items.
- For example, instead of doing something like the following:

  ```tsx
  // ❌ Don't do this

  const expectedOptions: SelectOption[] = [
    { value: 'value-1', label: 'Value 1' },
    { value: 'value-2', label: 'Value 2' },
  ];

  const options = screen.getAllByRole('option');

  options.forEach((option, index) => {
    if (index === 0) {
      expect(option).toHaveValue('');
      expect(option).toHaveTextContent('Choose option');
    } else {
      expect(option).toHaveValue(expectedOptions[index].value);
      expect(option).toHaveTextContent(expectedOptions[index].label);
    }
  });
  ```

  Simplify to the following instead:

  ```tsx
  // ✅ Do this

  const options = screen.getAllByRole('option');

  expect(options[0]).toHaveValue('');
  expect(options[0]).toHaveTextContent('Choose option');

  expect(options[1]).toHaveValue('value-1');
  expect(options[1]).toHaveTextContent('Value 1');

  expect(options[2]).toHaveValue('value-2');
  expect(options[2]).toHaveTextContent('Value 2');
  ```

- If a test needs to use loops or logic, it should be for a specific case where a copy and paste
  approach would result in reduced clarity by the sheer number of assertions. Typically, this would
  be where there are many options, or where we need to test **many permutations** within a broad
  range of constraints.

<a id="s5.2"></a>

### 5.2 Rendering components in tests

<a id="s5.2.1"></a>

#### 5.2.1 Use the custom render function

- Use the `render` function from `common-test` rather than from `@testing-library/react` directly. This does two things for us:
  - Wraps the render in a `QueryClientProvider` to make testing components which use `react-query` easier
  - Sets up the `userEvent` library so it doesn't need to be done in each test. To access the user events:
    ```tsx
    const { user } = render(<MyComponent />);
    await user.click(screen.getByRole('button', { name: 'click me' }));
    ```

<a id="s5.3"></a>

### 5.3 Naming and syntax

<a id="s5.3.1"></a>

#### 5.3.1 Test case naming

- The general naming convention is `{action} {subject?} {expectation or condition?}` e.g.
  - `renders correctly`
  - `renders button if user is logged in`
  - `clicking the button calls x service`
  - `submitting form successfully`
  - `submitting form with invalid values shows errors`
- The name should start **lowercased** unless there is a specific reason for it not to.

<a id="s5.3.2"></a>

#### 5.3.2 Test ID (`data-testid`) naming

- Use hyphens
- We're not too precious about this as sometimes it makes sense to put a string (e.g. a name) inside
  of the test ID for ease

<a id="s5.3.3"></a>

#### 5.3.3 Test data variable naming conventions

- Prefix variables with `test*`

<a id="s5.3.4"></a>

#### 5.3.4 Test description syntax

- Use `test` instead of `it`.

<a id="s5.4"></a>

### 5.4 Queries

<a id="s5.4.1"></a>

#### 5.4.1 `getByRole` vs `getByText` vs `getByLabel`

- `getByRole` is good for most things and should be used as much as possible.
- `getByLabel` is good for form inputs.
- `getByText` is good for checking text on page.
- Combine with `within` if there is potential for duplication.
- Use `getByTestId` as a fallback where selection isn't possible with other selectors.

<a id="s5.4.2"></a>

#### 5.4.2 Waiting for UI changes

- UI changes on load or following user interaction usually have to be waited for.
- Use `findBy*` where applicable e.g.
  ```tsx
  expect(await screen.findByText('find me')).toBeInTheDocument();
  ```
- When using `waitFor` don't use `getByRole` as it causes performance degradation. Use a faster
  selector like `getByText` or `getByLabel` in `waitFor` and keep assertions minimal inside.

  Follow up after `waitFor` with any `getByRole` selectors.

<a id="s5.5"></a>

### 5.4 Events

<a id="s5.5.1"></a>

#### 5.4.1 Using `userEvent` library

- Don't use `fireEvent` unless it's necessary. Use `userEvent` library via the render function ([see above](#s5.2)) instead.
- Use `await` with all `user` methods.

<a id="s5.5.3"></a>

#### 5.4.2 Using `jest.useFakeTimers` with `userEvent`

- Using `jest.useFakeTimers()` with `userEvent` can cause tests to timeout.
- To fix this invoke `userEvent` directly with one of following:
  ```tsx
  const user = userEvent.setup({ advanceTimers: jest.advanceTimersByTime });
  ```
  ```tsx
  const user = userEvent.setup({ delay: null });
  ```
  <a id="s5.6"></a>

### 5.6 Test data

<a id="s5.6.1"></a>

#### 5.6.1 Use of shared test data

- Test data files should go in `__data__` folder
- `__data__` folder should be a sibling of `__tests__`
- Test data files should be named appropriately prefixed `test*` as well.

<a id="s5.6.2"></a>

#### 5.6.2 Test data generators

- Start creating test data generators in `test` directories under `generators` dir.
- Keep generators as simple as possible and refactor where necessary.

<a id="s5.6.3"></a>

#### 5.6.3 What to use for test data values

- Values don't have to realistic. Prefer to use generic defaults like 'Publication title 1' following
  structure: `{type} {property} {number}`.
- This doesn't need to hard and fast rule and people can use their discretion.

<a id="s5.7"></a>

### 5.7 Assertions

<a id="s5.7.1"></a>

#### 5.7.1 `toEqual` vs individual assertions

- Prefer to use toEqual when comparing objects (instead assertions on each property).

<a id="s5.7.2"></a>

#### 5.7.2 `toBeInTheDocument` vs `toBeVisible`

- Use `toBeInTheDocument` by default
- Use `toBeVisible` when you actually need to test visibility e.g. transparent or `display: none` stuff
- `toBeVisible` incurs extra style checks during assertion (which is overkill)

<a id="s5.7.3"></a>

#### 5.7.3 Asserting error messages are displayed

- Have a check that the error summary message links back to the input that has the error
- Check that the error message is rendered next to the field
- We plan to create a custom Jest assertion that handles this for us at some point e.g.
  `expect().toHaveFormError()`. This would roll up any required assertions into one.

<a id="s5.7.4"></a>

#### 5.7.4 Asserting radio or checkbox options are present

- Length check - make sure there are correct number of checkboxes or radios.
- Check each radio or checkbox state as part of a list of assertions
- `getByLabelText` to get the input and assert that it's one of the radios

  ```tsx
  const radios = within(
    screen.getByRole('group', { name: 'Change page view' }),
  ).getAllByRole('radio');

  expect(radios).toHaveLength(3);

  expect(radios[0]).toEqual(screen.getByLabelText('Edit content'));
  ```

<a id="s5.7.5"></a>

#### 5.7.5 Number of assertions in waitFor blocks

- Typically only one assertion should be used within block
- If waiting for multiple states, then more can be added to block

<a id="s5.7.6"></a>

#### 5.7.6 Asserting mocks have been called

- Check that the mock hasn't been called before an event happens.
- Check that the mock has been called with right values after event happens.
- Check number of calls explicitly if required, but don't need to in most cases - up to developer.

<a id="s5.8"></a>

### 5.8 Mocking

<a id="s5.8.1"></a>

#### 5.8.1 Integration vs mocking in Jest tests

- Mock service calls, integration test everything else as standard
- Avoid mocking as much as possible
- In certain cases, it may be useful to mock, but should be discussed and agreed as a good testing approach
- Tests should test user interaction and avoid creating simulated environments that don't actually test this

<a id="s5.8.2"></a>

#### 5.8.2 Setting up Jest mocks

- We currently do a lot of the following:
  ```tsx
  import _theService from 'theService';
  const theService = _theService as jest.Mocked<typeof _theService>;
  ```
- However, we should switch to using jest.mocked.
  ```tsx
  import _theService from 'theService';
  const theService = jest.mocked(_theService);
  ```
