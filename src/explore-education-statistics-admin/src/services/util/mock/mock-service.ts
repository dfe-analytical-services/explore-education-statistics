const getCaptureGroups: (regex: RegExp, string?: string) => RegExpMatchArray = (regex, string) => {

  if (!string) {
    return [];
  }

  const matches = string.match(regex);

  return matches ? matches.slice(1) : [];
};

export default getCaptureGroups;