import camelCase from 'lodash/camelCase';
import toPath from 'lodash/toPath';

/**
 * Format a {@param formId} and {@param name} into a
 * string for use in `id` attributes.
 *
 * We should use this to ensure that things like error summary
 * messages will correctly reference the form fields.
 */
export default function fieldId(formId: string, name: string): string {
  if (!name) {
    throw new Error('Field name is required to create id');
  }

  const parts = toPath(name).map(camelCase);
  const id = parts.join('-');

  return formId ? `${formId}-${id}` : id;
}
