/**
 * Start downloading a {@param file} onto the client's disk.
 *
 * The {@param file} can be either a Blob or string. If a string
 * is provided, then we assume this is just a URL that the
 * file is located at.
 *
 * A custom {@param fileName} can be specified if the file is
 * a Blob, otherwise, the filename provided by the file
 * response's `Content-Disposition` header is used instead.
 */
export default function downloadFile(file: Blob | string, fileName?: string) {
  const url = typeof file === 'string' ? file : URL.createObjectURL(file);
  const link = document.createElement('a');

  link.href = url;

  if (fileName) {
    link.setAttribute('download', fileName);
  }

  document.body.appendChild(link);

  link.click();
  link.remove();
}
