/**
 * Create an `aria-describedby` attribute string
 * from a hint and error.
 *
 * This is purely for convenience and is not
 * meant to be used anywhere else.
 */
export default ({
  id,
  error,
  hint,
}: {
  id: string;
  error: boolean;
  hint: boolean;
}): string | undefined => {
  const parts = [];

  if (error) {
    parts.push(`${id}-error`);
  }

  if (hint) {
    parts.push(`${id}-hint`);
  }

  const describedBy = parts.join(' ');

  return describedBy !== '' ? describedBy : undefined;
};
