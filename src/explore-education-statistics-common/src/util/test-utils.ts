/**
 * Strip out newlines and multiple spaces from strings (caused by strings being broken over several lines in tsx files)
 */
export const formatTestId = (id?: string) =>
  id && id.replace('\n', ' ').replace(/[\s]{2,}/g, ' ');

export default {};
