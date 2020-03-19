/**
 * Truncate some {@param text} around a {@param position}
 * index so that only only characters between the start
 * and end truncation lengths are returns
 * e.g. `...something something...`.
 *
 * Additional {@param options} can be passed through to
 * specify how the truncation should be performed.
 *
 * - Only full words will be returned in the truncated text
 *   if `fullWords` is true.
 * - The omission text used on the ends of the truncated text
 *   can also be changed with `start/endOmissionText`.
 */
export default function truncateAround(
  text: string,
  position: number,
  options: {
    startTruncateLength?: number;
    endTruncateLength?: number;
    startOmissionText?: string;
    endOmissionText?: string;
    fullWords?: boolean;
  } = {},
) {
  const {
    startTruncateLength = 30,
    endTruncateLength = 30,
    startOmissionText = '...',
    endOmissionText = '...',
    fullWords = true,
  } = options;

  const lastTextPos = text.length;

  let startTruncatePos =
    position - startTruncateLength > 0 ? position - startTruncateLength : 0;
  let endTruncatePos =
    position + endTruncateLength < lastTextPos
      ? position + endTruncateLength
      : lastTextPos;

  if (fullWords) {
    if (startTruncatePos > 0 && !/\s/.test(text[startTruncatePos - 1])) {
      startTruncatePos = text.indexOf(' ', startTruncatePos);
    }

    if (endTruncatePos < lastTextPos && !/\s/.test(text[endTruncatePos + 1])) {
      endTruncatePos = text.lastIndexOf(' ', endTruncatePos);
    }
  }

  const truncatedText = text.substring(startTruncatePos, endTruncatePos);

  return `${
    startTruncatePos === 0 ? '' : startOmissionText
  }${truncatedText.trim()}${
    endTruncatePos === lastTextPos ? '' : endOmissionText
  }`;
}
