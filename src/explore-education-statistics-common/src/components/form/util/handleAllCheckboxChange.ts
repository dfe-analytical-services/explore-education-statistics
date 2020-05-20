import { FieldHelperProps } from 'formik';
import difference from 'lodash/difference';
import {
  CheckboxGroupAllChangeEvent,
  CheckboxOption,
} from '../FormCheckboxGroup';

export default function handleAllChange({
  checked,
  event,
  helpers,
  options,
  value,
}: {
  checked: boolean;
  event: CheckboxGroupAllChangeEvent;
  helpers: FieldHelperProps<unknown>;
  options: CheckboxOption[];
  value: string[];
}): void {
  if (event.isDefaultPrevented()) {
    return;
  }

  const allOptionValues = options.map(option => option.value);
  const restValues = difference(value, allOptionValues);

  if (!checked) {
    helpers.setValue([...restValues, ...allOptionValues]);
  } else {
    helpers.setValue(restValues);
  }
}
