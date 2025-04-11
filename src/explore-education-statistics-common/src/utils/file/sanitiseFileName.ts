// Remove characters not permitted in file names.
export default function sanitiseFileName(fileName: string) {
  return fileName.replace(/[|*?"/<>:.]/g, '').trim();
}
