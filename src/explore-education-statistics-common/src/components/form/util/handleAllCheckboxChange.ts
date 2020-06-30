import { FieldHelperProps, FieldMetaProps } from 'formik';
import difference from 'lodash/difference';
import { CheckboxOption } from '../FormCheckboxGroup';

export default function handleAllChange({
  checked,
  meta,
  helpers,
  options,
}: {
  checked: boolean;
  meta: FieldMetaProps<string[]>;
  helpers: FieldHelperProps<string[]>;
  options: CheckboxOption[];
}): void {
  const allOptionValues = options.map(option => option.value);
  const restValues = difference(meta.value, allOptionValues);

  if (!checked) {
    helpers.setValue([...restValues, ...allOptionValues]);
  } else {
    helpers.setValue(restValues);
  }
}
