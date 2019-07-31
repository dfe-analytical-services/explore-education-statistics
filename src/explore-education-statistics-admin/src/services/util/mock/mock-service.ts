export const getCaptureGroups: (
  regex: RegExp,
  string?: string,
) => RegExpMatchArray = (regex, string) => {
  if (!string) {
    return [];
  }

  const matches = string.match(regex);

  return matches ? matches.slice(1) : [];
};

export const generateRandomInteger = (max: number) =>
  Math.floor(Math.random() * Math.floor(max));

export const generateRandomIntegerString = (max: number) =>
  generateRandomInteger(max).toString();

export default {};
