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
    const hasExtension = name.split('.').at(-1) === extension;
    if (hasExtension) return name;
    return `${name}.${extension}`;
  }

  return name;
}
