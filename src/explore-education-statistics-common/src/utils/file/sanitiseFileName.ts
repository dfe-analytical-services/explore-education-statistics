// these options aren't exhaustive, add as required
export type FileExtension = 'png' | 'csv';

/**
 * Remove characters not permitted in file names.
 */
export default function sanitiseFileName(
  fileName: string,
  extension?: FileExtension,
) {
  const name = fileName.replace(/[|*?"/<>:]/g, '').trim();

  if (extension) {
    const hasExtension =
      name.split('.').at(-1)?.toLocaleLowerCase() ===
      extension.toLocaleLowerCase();

    if (hasExtension) {
      // prefers the typed lowercase extension
      return [...name.split('.').slice(0, -1), extension].join('.');
    }

    return `${name}.${extension}`;
  }

  return name;
}
