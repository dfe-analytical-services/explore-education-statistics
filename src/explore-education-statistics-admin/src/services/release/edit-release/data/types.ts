export interface DataFile {
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  numberOfRows: number;
  metadataFilename: string;
  canDelete?: boolean;
}

export interface UploadDataFilesRequest {
  subjectTitle: string;
  dataFile: File;
  metadataFile: File;
}

export interface AncillaryFile {
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
}

export interface UploadAncillaryFileRequest {
  name: string;
  file: File;
}

export interface ChartFile {
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
}

export interface UploadChartFileRequest {
  name: string;
  file: File;
}
