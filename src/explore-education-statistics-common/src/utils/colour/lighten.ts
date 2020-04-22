/* eslint-disable no-bitwise */

/**
 * Lighten a {@param colour} by a {@param percent}.
 */
export default function lighten(colour: string, percent: number) {
  let R = parseInt(colour.substring(1, 3), 16);
  let G = parseInt(colour.substring(3, 5), 16);
  let B = parseInt(colour.substring(5, 7), 16);

  R = parseInt(((R * (100 + percent)) / 100).toString(), 10);
  G = parseInt(((G * (100 + percent)) / 100).toString(), 10);
  B = parseInt(((B * (100 + percent)) / 100).toString(), 10);

  R = R < 255 ? R : 255;
  G = G < 255 ? G : 255;
  B = B < 255 ? B : 255;

  const RR =
    R.toString(16).length === 1 ? `0${R.toString(16)}` : R.toString(16);
  const GG =
    G.toString(16).length === 1 ? `0${G.toString(16)}` : G.toString(16);
  const BB =
    B.toString(16).length === 1 ? `0${B.toString(16)}` : B.toString(16);

  return `#${RR}${GG}${BB}`;

  // if (amount === 0) {
  //   return colour;
  // }
  //
  // const useHash = colour.startsWith('#');
  // const number = parseInt(useHash ? colour.slice(1) : colour, 16);
  //
  // let r = (number >> 16) + amount;
  //
  // if (r > 255) r = 255;
  // else if (r < 0) r = 0;
  //
  // let b = ((number >> 8) & 0x00ff) + amount;
  //
  // if (b > 255) b = 255;
  // else if (b < 0) b = 0;
  //
  // let g = (number & 0x0000ff) + amount;
  //
  // if (g > 255) g = 255;
  // else if (g < 0) g = 0;
  //
  // return (useHash ? '#' : '') + (g | (b << 8) | (r << 16)).toString(16);
}
