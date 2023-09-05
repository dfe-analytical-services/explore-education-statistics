import { get } from 'lodash';
import { FieldValues, FieldErrors, Path } from 'react-hook-form';

export default function getErrorMessage<TValues extends FieldValues>(
  errors: FieldErrors<TValues>,
  name: Path<TValues>,
  showError = true,
): string {
  return showError ? (get(errors, name)?.message as string) ?? '' : '';
}
