/**
 * Convert a file into a base64 encoded data URL.
 */
export default function toDataUrl(
  file: File | Blob,
): Promise<string | ArrayBuffer> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result || '');
    reader.onerror = error => reject(error);
  });
}
