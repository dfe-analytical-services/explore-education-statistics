import { hasBadContrast } from 'color2k';

type TextSize = 'normal' | 'large';
type WcagLevel = 'AA' | 'AAA';

interface Options {
  backgroundColour: string;
  textColour: string;
  textSize?: TextSize;
  wcagLevel?: WcagLevel;
}

/**
 * Checks if the text colour has sufficient contrast against the background colour.
 * If not, returns white or black.
 * This isn't infallible, at AAA level especially some background colours will
 * have contrast problems with white and black text at normal size.
 */
export default function getAccessibleTextColour({
  backgroundColour,
  textColour,
  textSize = 'normal',
  wcagLevel = 'AA',
}: Options) {
  const standard = getColor2kStandard(textSize, wcagLevel);

  // The text colour is accessible against the background colour.
  if (!hasBadContrast(textColour, standard, backgroundColour)) {
    return textColour;
  }

  // If white text is accessible return that, otherwise black text.
  return hasBadContrast(textColour, standard, '#FFFFFF')
    ? '#000000'
    : '#FFFFFF';
}

// Convert to color2k standards
// https://github.com/ricokahler/color2k/blob/main/src/hasBadContrast.ts
function getColor2kStandard(textSize: TextSize, wcagLevel: WcagLevel) {
  switch (wcagLevel) {
    case 'AA':
      return textSize === 'normal' ? 'aa' : 'readable';
    case 'AAA':
      return textSize === 'normal' ? 'aaa' : 'aa';
    default:
      return 'aa';
  }
}
