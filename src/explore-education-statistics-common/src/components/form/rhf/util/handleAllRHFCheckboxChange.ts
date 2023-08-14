import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import difference from 'lodash/difference';
import {
  FieldValues,
  Path,
  PathValue,
  UseFormSetValue,
  UseFormTrigger,
} from 'react-hook-form';

interface Props<TFormValues extends FieldValues> {
  name: Path<TFormValues>;
  checked: boolean;
  options: CheckboxOption[];
  selectedValues: string[];
  setValue: UseFormSetValue<TFormValues>;
  submitCount: number;
  trigger: UseFormTrigger<TFormValues>;
}

export default function handleAllRHFCheckboxChange<
  TFormValues extends FieldValues,
>({
  checked,
  name,
  options,
  selectedValues,
  setValue,
  submitCount,
  trigger,
}: Props<TFormValues>): void {
  const allOptionValues = options.map(option => option.value);
  const restValues = difference(selectedValues, allOptionValues);
  const nextValues = checked ? restValues : [...restValues, ...allOptionValues];

  setValue(name, nextValues as PathValue<TFormValues, Path<TFormValues>>);

  // trigger revalidation if the form has been submitted.
  if (submitCount > 0) {
    trigger(name);
  }
}
