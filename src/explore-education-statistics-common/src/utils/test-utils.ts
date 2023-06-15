/**
 * Strip out newlines and multiple spaces from strings (caused by strings being broken over several lines in tsx files)
 */
const formatTestId = (id?: string) => {
  if (id) {
    return id.replace('\n', ' ').replace(/[\s]{2,}/g, ' ');
  }
  return null;
};

export default formatTestId;
