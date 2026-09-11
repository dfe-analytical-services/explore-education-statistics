/**
 * The first dotted version number in a command's output (or a pin file's
 * contents), or undefined if there isn't one.
 *
 * Matched out of the middle rather than anchored, because the tools this is
 * used on don't agree on a format: `func --version` prints the bare version,
 * `python3 --version` prefixes it with "Python ", and `.nvmrc` may carry a
 * leading "v" and trailing newline.
 */
export function parseVersion(text: string): string | undefined {
  return text.match(/\d+(?:\.\d+)+/)?.[0];
}

/** Whether version `a` is older than version `b`, compared numerically. */
export function isOlderVersion(a: string, b: string): boolean {
  const aParts = a.split('.').map(Number);
  const bParts = b.split('.').map(Number);
  const length = Math.max(aParts.length, bParts.length);

  for (let index = 0; index < length; index += 1) {
    const difference = (aParts[index] ?? 0) - (bParts[index] ?? 0);

    if (difference !== 0) {
      return difference < 0;
    }
  }

  return false;
}

/**
 * Whether an installed version matches a pinned one, to the precision the pin
 * bothers with - a pin of `22` accepts any Node 22, while `22.23.1` means
 * exactly that. That's how the pin files behave in the tools that own them
 * (`nvm use`, pyenv), so it's how the dashboard should read them too.
 */
export function versionSatisfiesPin(installed: string, pin: string): boolean {
  const installedParts = installed.split('.');

  return pin
    .split('.')
    .every((part, index) => Number(part) === Number(installedParts[index]));
}
