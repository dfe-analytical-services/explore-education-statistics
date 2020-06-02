const getFileNameFromPath = (path: string) =>
  path.substring(path.lastIndexOf('/') + 1);

export default getFileNameFromPath;
