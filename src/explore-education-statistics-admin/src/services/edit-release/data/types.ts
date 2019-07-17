interface FileDetails {
  id: string;
  fileName: string;
}

export interface DataFile {
  title: string;
  file: FileDetails;
  fileSize: {
    size: number;
    unit: string;
  };
  numberOfRows: number;
  metadataFile: FileDetails;
}

export interface UploadDataFilesRequest {
  subjectTitle: string;
  dataFile: File;
  metadataFile: File;
}

export interface AdhocFile {
  title: string;
  file: FileDetails;
  fileSize: {
    size: number;
    unit: string;
  };
}

export interface UploadAdhocFileRequest {
  name: string;
  file: File;
}