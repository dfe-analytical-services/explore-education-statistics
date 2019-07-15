interface File {
  id: string;
  fileName: string;
}

export interface DataFile {
  title: string;
  file: File;
  fileSize: {
    size: number;
    unit: string;
  };
  numberOfRows: number;
  metadataFile: File;
}

export interface DataFileView {
  publicationTitle: string;
  dataFiles: DataFile[];
}
