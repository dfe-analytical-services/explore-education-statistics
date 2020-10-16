export interface FileInfo {
  id: string;
  extension: string;
  fileName: string;
  name: string;
  path: string;
  size: string;
  type: 'Data' | 'DataZip' | 'Metadata' | 'Ancillary' | 'Chart';
}
