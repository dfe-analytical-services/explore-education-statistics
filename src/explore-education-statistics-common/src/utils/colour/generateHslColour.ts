import hashCode from '@common/utils/string/hashCode';

/**
 * Generate a HSL colour based on an input {@param string}.
 * We just use the string's
 * The {@param saturation} and {@param lightness}
 * can also be modified if desired.
 */
export default function generateHslColour(
  string: string,
  saturation = 80,
  lightness = 30,
): string {
  const hash = hashCode(string);

  return `hsl(${hash % 360}, ${saturation}%, ${lightness}%)`;
}
